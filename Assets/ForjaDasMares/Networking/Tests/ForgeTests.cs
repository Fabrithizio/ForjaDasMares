using System;
using System.IO;
using System.Linq;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class ForgeTests
    {
        CoopDirector session;CoopProfileChannel channel;CoopProgression progression;
        [UnitySetUp]public IEnumerator Load(){SceneManager.LoadScene("CampaignCoop");yield return null;yield return null;session=UnityEngine.Object.FindFirstObjectByType<CoopDirector>();channel=session.GetComponent<CoopProfileChannel>();progression=session.GetComponent<CoopProgression>();}
        [UnityTearDown]public IEnumerator Close(){session.Close();yield return null;yield return null;}
        [UnityTest]public IEnumerator HomeProfileWorksWithoutCreatingRoomAndPersistsEquipment()
        {
            Assert.AreEqual(-1,session.LocalSlot);Assert.IsFalse(session.Manager.IsListening);var profile=channel.LocalProfile;Assert.AreEqual(4,profile.inventory.Count);
            var item=profile.inventory.First(x=>x.definitionId=="initiation-warrior");channel.Request(CoopInventoryAction.Unequip,null,EquipmentSlot.MainHand);Assert.IsTrue(string.IsNullOrEmpty(channel.LocalProfile.Loadout("warrior").items[0]));
            channel.Request(CoopInventoryAction.Equip,item.instanceId,EquipmentSlot.MainHand);Assert.AreEqual(item.instanceId,channel.LocalProfile.Loadout("warrior").items[0]);
            var hud=UnityEngine.Object.FindFirstObjectByType<CoopInventoryHud>();hud.OpenInventory();yield return null;Assert.IsTrue(hud.IsOpen);Assert.IsFalse(session.Manager.IsListening);hud.CloseInventory();
            progression.ReleaseHome();Assert.AreEqual(item.instanceId,channel.LocalProfile.Loadout("warrior").items[0]);Assert.AreEqual(4,channel.LocalProfile.inventory.Count);
        }
        [Test]public void StarterMigrationIsIdempotentAndClassAppropriate()
        {
            var catalog=progression.Catalog;string path=Path.Combine(Application.temporaryCachePath,"ForgeTest",Guid.NewGuid().ToString("N"),"profile.json");var service=new ProfileService(catalog,new ProfileStore(path));
            for(int i=0;i<4;i++)service.EnsureClassStarters();Assert.AreEqual(4,service.Profile.inventory.Count);
            foreach(var rule in catalog.classStarters){var set=service.Profile.Loadout(rule.heroId);var item=service.Profile.Item(set.items[0]);Assert.AreEqual(ItemRarity.Common,item.rarity);Assert.AreEqual(rule.itemId,item.definitionId);Assert.IsTrue(ProgressionStats.CanEquip(service.Profile,catalog,rule.heroId,item,EquipmentSlot.MainHand,out _));var weapon=catalog.Find(rule.itemId);Assert.IsTrue(weapon.starter);Assert.AreEqual(1,weapon.attacks[0].maximumTargets);}
        }
        [UnityTest]public IEnumerator FuryChangesSpeedWithoutAreaDamageExpiresAndClearsOnDeath()
        {
            Assert.IsTrue(session.HostDirect("FORGE1",7841,false));yield return null;session.StartGame();yield return null;var hero=session.Actor(0);var pool=session.Dungeon.Encounter.Enemies;var definition=Enumerable.Range(0,pool.Capacity).Select(pool.DefinitionAt).First(x=>x.kind==EnemyKind.Raider);var target=pool.TrySpawn(definition,hero.transform.position+Vector3.forward*2);Assert.NotNull(target);target.Health.SetReplicaState(10000,10000);target.Brain.enabled=false;
            for(int i=0;i<hero.Roster.Current.onlineFlow.hitsForSuper;i++)Assert.IsTrue(target.Health.ApplyHit(hero.Combat.BasicAttack,Vector3.forward,hero.Health));
            int hp=target.Health.Current;float before=hero.Combat.AttackSpeed;Assert.IsTrue(hero.Combat.TrySuper());Assert.AreEqual(hp,target.Health.Current);Assert.That(hero.Combat.AttackSpeed,Is.EqualTo(before*1.55f).Within(.01));Assert.IsFalse(hero.Combat.TrySuper());hero.Combat.Simulate(6.1f);Assert.AreEqual(before,hero.Combat.AttackSpeed);
            for(int i=0;i<hero.Roster.Current.onlineFlow.hitsForSuper;i++)target.Health.ApplyHit(hero.Combat.BasicAttack,Vector3.forward,hero.Health);Assert.IsTrue(hero.Combat.TrySuper());hero.Health.ApplyDamage(100000);hero.Combat.Simulate(.01f);Assert.AreEqual(0,hero.Combat.FuryRemaining);
        }
        [UnityTest]public IEnumerator WeaponSkillActuallyReplacesClassSkillAndMinimapTracksPendingFight()
        {
            Assert.IsTrue(session.HostDirect("FORGE2",7842,false));yield return null;session.StartGame();yield return null;var hero=session.Actor(0);var service=progression.Profile(0);var weapon=LootGenerator.Create(progression.Catalog,"fenda-axe",ItemRarity.Common,new System.Random(2));service.Profile.expedition.items.Add(weapon);Assert.IsTrue(service.Equip("warrior",weapon.instanceId,EquipmentSlot.MainHand));Assert.AreSame(progression.Catalog.Find("fenda-axe").ability,hero.Combat.Ability);
            var map=session.Dungeon;map.Enter(map.Generator.Rooms[1]);int fight=session.Campaign.FightRoomId;map.Enter(map.Generator.Rooms[3]);Assert.AreEqual(fight,session.Campaign.FightRoomId);Assert.AreEqual(fight,session.Campaign.Capture().fightRoom);Assert.NotNull(UnityEngine.Object.FindFirstObjectByType<CampaignMinimap>());
        }
        [UnityTest]public IEnumerator DartEmittersAndEnemyArrivalHaveVisibleOrigins()
        {
            Assert.IsTrue(session.HostDirect("FORGE3",7843,false));yield return null;session.StartGame();yield return null;Assert.That(UnityEngine.Object.FindObjectsByType<CampaignWallDarts>(FindObjectsSortMode.None).Length,Is.GreaterThan(0));Assert.IsTrue(session.Dungeon.Generator.Rooms.Any(r=>r.root.GetComponentsInChildren<Transform>().Any(t=>t.name=="VisibleDartEmitter")));
            var pool=session.Dungeon.Encounter.Enemies;var data=Enumerable.Range(0,pool.Capacity).Select(pool.DefinitionAt).First(x=>x.kind==EnemyKind.Raider);var actor=pool.TrySpawn(data,session.Actor(0).transform.position+Vector3.forward*2);Assert.That(actor.SpawnRemaining,Is.GreaterThan(.5));Assert.NotNull(actor.GetComponent<EnemyArrivalVisual>());yield return new WaitForSeconds(1.1f);Assert.AreEqual(0,actor.SpawnRemaining);
        }
        [UnityTest]public IEnumerator LastRoomProfileCanBeOrganizedWithoutImportingItemsIntoHostProfile()
        {
            var own=channel.LocalProfile;string ownWeapon=own.Loadout("warrior").items[0];var snapshot=JsonUtility.FromJson<PlayerProfile>(JsonUtility.ToJson(own));snapshot.expedition=null;snapshot.lastResult=null;
            channel.Hub.Accept(new CoopProfileFrame{scope=Guid.NewGuid().ToString("N"),profile=snapshot,deliveryCount=0});Assert.IsTrue(channel.Hub.UsingRemote);
            channel.Request(CoopInventoryAction.Unequip,null,EquipmentSlot.MainHand);Assert.IsTrue(string.IsNullOrEmpty(channel.LocalProfile.Loadout("warrior").items[0]));Assert.AreEqual(ownWeapon,progression.HomeProfile.Profile.Loadout("warrior").items[0]);
            channel.Hub.OwnSession();Assert.AreEqual(ownWeapon,channel.LocalProfile.Loadout("warrior").items[0]);yield return null;
            var actor=session.Actor(0);actor.SetAuthority(false);Assert.IsFalse(actor.GetComponent<HeroEquipment>().enabled);var restored=channel.LocalProfile;Assert.IsTrue(actor.GetComponent<HeroEquipment>().enabled);Assert.AreEqual(4,restored.inventory.Count);
        }
        [UnityTest]public IEnumerator LobbyEquipmentChangesApplyBeforeStartingAndNewRunStartsAtFullHealth()
        {
            Assert.IsTrue(session.HostDirect("FORGE4",7844,false));yield return null;var service=progression.Profile(0);var armor=LootGenerator.Create(progression.Catalog,"class-armor-0",ItemRarity.Common,new System.Random(8));service.Profile.inventory.Add(armor);Assert.IsTrue(service.Equip("warrior",armor.instanceId,EquipmentSlot.Armor));
            var actor=session.Actor(0);Assert.AreEqual(116,actor.Health.Maximum);Assert.IsTrue(actor.GetComponent<HeroEquipment>().enabled);session.StartGame();yield return null;Assert.AreEqual(116,actor.Health.Maximum);Assert.AreEqual(actor.Health.Maximum,actor.Health.Current);
        }
        [UnityTest]public IEnumerator ReplicaWeaponIsHiddenWhenActorReturnsToAuthority()
        {
            var actor=session.Actor(1);actor.gameObject.SetActive(true);var frame=new ActorFrame{hero=0,mainItem="fenda-axe",maximum=100,hp=100};actor.Apply(frame);actor.ApplyGear(frame,progression.Catalog);var visual=actor.GetComponentsInChildren<MeshRenderer>(true).First(x=>x.name=="ReplicaMain");Assert.IsTrue(visual.gameObject.activeSelf);actor.SetAuthority(true);Assert.IsFalse(visual.gameObject.activeSelf);yield return null;
        }
        [Test]public void StarterStrikeHitsOnlyOnePhysicalTarget()
        {
            var a=GameObject.CreatePrimitive(PrimitiveType.Capsule);var z=GameObject.CreatePrimitive(PrimitiveType.Capsule);a.layer=z.layer=12;a.transform.position=new Vector3(.25f,1,500);z.transform.position=new Vector3(-.25f,1,500);var hp=a.AddComponent<Health>();var hz=z.AddComponent<Health>();hp.Configure(100,CombatFaction.Enemy,0);hz.Configure(100,CombatFaction.Enemy,0);Physics.SyncTransforms();
            var hit=progression.Catalog.Find("initiation-warrior").attacks[0];var resolver=new DamageResolver();Assert.AreEqual(1,resolver.Area(new Vector3(0,1,499),2,Vector3.forward,120,hit,null,1<<12,0));Assert.AreEqual(1,new[]{hp,hz}.Count(x=>x.Current<100));UnityEngine.Object.Destroy(a);UnityEngine.Object.Destroy(z);
        }
    }
}
