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
    public sealed class WildernessTests
    {
        CoopDirector session;CampaignRun run;
        [UnitySetUp]public IEnumerator Load(){SceneManager.LoadScene("CampaignCoop");yield return null;yield return null;session=UnityEngine.Object.FindFirstObjectByType<CoopDirector>();run=session.Campaign;}
        [UnityTearDown]public IEnumerator Close(){if(session!=null)session.Close();yield return null;yield return null;}
        [UnityTest]public IEnumerator OpenNavigationNeverClosesAndChangingSectorDoesNotEraseFight()
        {
            Assert.IsTrue(session.HostDirect("WILD01",7821,false));yield return null;session.StartGame();yield return null;
            var map=session.Dungeon;Assert.IsTrue(map.OpenLandscape);Assert.IsTrue(map.Generator.NavigationConnected);Assert.That(map.Generator.Rooms[0].walkableHalf,Is.GreaterThan(12));
            map.Enter(map.Generator.Rooms[1]);var fighting=map.Encounter.Current;var source=map.FightRoom;
            map.Enter(map.Generator.Rooms[3]);Assert.AreSame(fighting,map.Encounter.Current);Assert.AreSame(source,map.FightRoom);Assert.IsFalse(map.Generator.Rooms[3].encounterComplete);
            map.RefreshDoors();foreach(var door in map.Generator.Doors)Assert.IsFalse(door.IsClosed);
            foreach(var actor in map.Encounter.Enemies.Actors)if(actor.IsAlive)actor.Health.ApplyDamage(100000);
            Assert.IsFalse(map.TryExitDungeon());
        }
        [Test]public void HordeDataAndAssetsAreRealAndBounded()
        {
            for(int i=0;i<3;i++)
            {var level=run.At(i);int total=level.hard.waves.Sum(w=>w.entries.Sum(e=>e.count));Assert.That(total,Is.GreaterThan(100));Assert.AreEqual(20,level.hard.maximumMobileEnemies);Assert.AreEqual(24,level.hard.maximumPcEnemies);Assert.That(level.sectorSize,Is.GreaterThanOrEqualTo(28));Assert.NotNull(level.coverModels[0]);}
            var defs=session.Dungeon.Encounter.Enemies.Actors.Select((a,i)=>session.Dungeon.Encounter.Enemies.DefinitionAt(i)).Distinct().ToArray();Assert.AreEqual(3,defs.Count(x=>x.miniboss));
            foreach(var elite in defs.Where(x=>x.miniboss)){Assert.That(elite.boss.attacks.Length,Is.GreaterThanOrEqualTo(2));Assert.That(elite.boss.attacks[0].warning,Is.GreaterThanOrEqualTo(1));Assert.NotNull(elite.prefab.GetComponentInChildren<EnemyModelMotion>(true));}
            var catalog=session.GetComponent<CoopProgression>().Catalog;foreach(var item in catalog.equipment){Assert.NotNull(item.icon,item.itemId);if(item.slot==EquipmentSlot.MainHand){Assert.AreEqual(3,item.attacks.Length);if(!item.starter)Assert.AreNotEqual(item.attacks[0],item.attacks[2]);}}
            var roster=session.Actor(0).Roster;for(int i=0;i<roster.Count;i++){roster.Select(i);Assert.NotNull(roster.Current.onlineFlow);}
        }
        [Test]public void ExpeditionEquipmentPersistsWithoutDuplicatingOnDefeatOrFullInventory()
        {
            var catalog=session.GetComponent<CoopProgression>().Catalog;string path=Path.Combine(Application.temporaryCachePath,"wild-"+Guid.NewGuid().ToString("N")+".json");
            try
            {
                var store=new ProfileStore(path);var service=new ProfileService(catalog,store);Assert.IsTrue(service.BeginExpedition("warrior","test"));string id=service.Profile.expedition.runId;
                var item=LootGenerator.Create(catalog,"wavecleaver",ItemRarity.Rare,new System.Random(1));service.Profile.expedition.items.Add(item);store.Save(service.Profile,catalog);
                Assert.IsTrue(service.Equip("warrior",item.instanceId,EquipmentSlot.MainHand),service.Message);Assert.AreEqual(item.definitionId,service.Profile.Item(item.instanceId).definitionId);
                var reopened=new ProfileService(catalog,store);Assert.NotNull(reopened.Profile.Item(item.instanceId));
                while(reopened.Profile.inventory.Count<catalog.inventoryCapacity)reopened.Profile.inventory.Add(LootGenerator.Create(catalog,"wavecleaver",ItemRarity.Common,new System.Random(reopened.Profile.inventory.Count+7)));
                Assert.IsTrue(reopened.FinishExpedition(id,false,"test defeat"),reopened.Message);var final=new ProfileService(catalog,store);Assert.AreEqual(item.instanceId,final.Profile.Loadout("warrior").items[0]);
                Assert.AreEqual(1,final.Profile.inventory.Count(x=>x.instanceId==item.instanceId));Assert.IsNull(final.Profile.expedition);Assert.AreEqual(1,final.Profile.lastResult.items.Count);Assert.IsFalse(final.FinishExpedition(id,false,"duplicate"));
            }
            finally{foreach(string file in new[]{path,path+".bak",path+".tmp"})if(File.Exists(file))File.Delete(file);}
        }
        [UnityTest]public IEnumerator SupplyHealsOnlyOnceAndDoesNotResurrect()
        {
            Assert.IsTrue(session.HostDirect("WILD02",7822,false));yield return null;session.StartGame();yield return null;
            var actor=session.Actor(0);var supply=run.Adventure.Props.First(x=>x.Kind==AdventurePropKind.Supply);actor.Motor.Teleport(supply.transform.position+Vector3.back,Quaternion.identity);actor.Health.ApplyDamage(60);int hp=actor.Health.Current;
            supply.Health.ApplyDamage(999);yield return new WaitForSeconds(.3f);Assert.AreEqual(hp+run.Level.adventure.supplyHealing,actor.Health.Current);yield return new WaitForSeconds(.3f);Assert.AreEqual(hp+run.Level.adventure.supplyHealing,actor.Health.Current);
        }
        [UnityTest]public IEnumerator EntireHundredEnemyHordeCompletesWithoutPoolFailureOrWrongSectorCredit()
        {
            Assert.IsTrue(session.HostDirect("WILD03",7823,false));yield return null;session.StartGame();yield return null;
            var map=session.Dungeon;var sector=map.Generator.Rooms.First(x=>x.node.category==RoomCategory.HardCombat);map.Enter(sector);int total=map.Encounter.PlannedCount;Assert.That(total,Is.GreaterThan(100));
            float previous=Time.timeScale;float deadline=Time.realtimeSinceStartup+90;
            try
            {
                Time.timeScale=4;
                while(Time.realtimeSinceStartup<deadline&&map.Encounter.State!=EncounterState.Won&&map.Encounter.State!=EncounterState.Failed)
                {
                    Assert.That(map.Encounter.Enemies.ActiveCount,Is.LessThanOrEqualTo(24));
                    foreach(var actor in map.Encounter.Enemies.Actors)if(actor.IsAlive)actor.Health.ApplyDamage(100000);
                    yield return null;
                }
                Assert.AreEqual(EncounterState.Won,map.Encounter.State);Assert.AreEqual(total,map.Encounter.DefeatedCount);Assert.IsTrue(sector.completed);Assert.AreEqual(0,map.Encounter.Enemies.ActiveCount);
            }
            finally{Time.timeScale=previous;}
        }
        [UnityTest]public IEnumerator ImportedCoverChildrenBecomeTranslucentWithoutDisablingCollision()
        {
            Assert.IsTrue(session.HostDirect("WILD04",7824,false));yield return null;session.StartGame();yield return null;
            var hero=session.Actor(0);var camera=Camera.main;var root=new GameObject("CoverOcclusionTest");root.layer=9;root.transform.position=Vector3.Lerp(camera.transform.position,hero.transform.position+Vector3.up,.65f);var collider=root.AddComponent<BoxCollider>();collider.size=Vector3.one*3;
            var body=GameObject.CreatePrimitive(PrimitiveType.Cube);body.transform.SetParent(root.transform,false);body.transform.localScale=Vector3.one*2;UnityEngine.Object.Destroy(body.GetComponent<Collider>());body.GetComponent<Renderer>().sharedMaterial=session.Dungeon.Biome.wall;
            Physics.SyncTransforms();yield return new WaitForSeconds(.2f);
            Assert.IsTrue(collider.enabled);Assert.That(body.GetComponent<Renderer>().sharedMaterial.name,Does.Contain("CameraTranslucent"));UnityEngine.Object.Destroy(root);
        }
        [UnityTest]public IEnumerator ReplicaPoolCanBecomeAuthoritativeAgain()
        {
            Assert.IsTrue(session.HostDirect("WILD05",7825,false));yield return null;session.StartGame();yield return null;var pool=session.Dungeon.Encounter.Enemies;
            int index=Enumerable.Range(0,pool.Capacity).First(i=>pool.DefinitionAt(i).kind==EnemyKind.Raider);var actor=pool.Actors[index];var data=pool.DefinitionAt(index);
            actor.SetReplica(data,20,28,EnemyState.Chase);Assert.IsFalse(actor.Brain.enabled);actor.gameObject.SetActive(false);
            var spawned=pool.TrySpawn(data,session.Actor(0).transform.position+Vector3.forward*2);Assert.NotNull(spawned);Assert.IsTrue(spawned.enabled);Assert.IsTrue(spawned.Brain.enabled);Assert.IsTrue(spawned.Motor.enabled);Assert.IsTrue(spawned.Combat.enabled);Assert.IsTrue(spawned.GetComponent<EnemyPresentation>().enabled);
        }
        [UnityTest]public IEnumerator EquippedWeaponStaysInMetresOnHostReplicaAndHeroChange()
        {
            Assert.IsTrue(session.HostDirect("WILD07",7827,false));yield return null;session.StartGame();yield return null;
            var progression=session.GetComponent<CoopProgression>();var service=progression.Profile(0);
            var item=LootGenerator.Create(progression.Catalog,"fenda-axe",ItemRarity.Rare,new System.Random(4));service.Profile.expedition.items.Add(item);
            Assert.IsTrue(service.Equip("warrior",item.instanceId,EquipmentSlot.MainHand),service.Message);yield return null;
            var host=session.Actor(0);var hostGear=host.GetComponentsInChildren<MeshRenderer>(true).First(x=>x.name=="CoopEquippedMain");
            Assert.That(hostGear.bounds.size.magnitude,Is.LessThan(3));Assert.That(Vector3.Distance(hostGear.bounds.center,host.transform.position),Is.LessThan(3));
            var replica=session.Actor(1);replica.gameObject.SetActive(true);
            var frame=new ActorFrame{hero=0,mainItem="fenda-axe",maximum=140,hp=140};replica.Apply(frame);replica.ApplyGear(frame,progression.Catalog);yield return null;
            var gear=replica.GetComponentsInChildren<MeshRenderer>(true).First(x=>x.name=="ReplicaMain");
            Assert.That(gear.bounds.size.magnitude,Is.LessThan(3));Assert.That(Vector3.Distance(gear.bounds.center,replica.transform.position),Is.LessThan(3));
            frame.hero=1;frame.mainItem="wavecleaver";replica.Apply(frame);replica.ApplyGear(frame,progression.Catalog);yield return null;
            Assert.IsTrue(gear.gameObject.activeInHierarchy);Assert.IsTrue(gear.transform.IsChildOf(replica.Presentation.Visible));Assert.That(gear.bounds.size.magnitude,Is.LessThan(3));
        }
        [UnityTest]public IEnumerator OriginalEnemyModelsRemainUprightAfterRuntimePreparation()
        {
            Assert.IsTrue(session.HostDirect("WILD06",7826,false));yield return null;session.StartGame();yield return null;var pool=session.Dungeon.Encounter.Enemies;
            var definitions=Enumerable.Range(0,pool.Capacity).Select(pool.DefinitionAt).Distinct().ToArray();
            foreach(var data in definitions)
            {
                var actor=pool.TrySpawn(data,session.Actor(0).transform.position+Vector3.forward*2);Assert.NotNull(actor,data.enemyId);
                var renderers=actor.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;foreach(var surface in renderers)bounds.Encapsulate(surface.bounds);
                Assert.That(bounds.size.y,Is.GreaterThan(data.height*.65f),data.enemyId);actor.Recycle();
            }
        }
    }
}
