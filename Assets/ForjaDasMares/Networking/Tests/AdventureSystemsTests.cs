using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.AI;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class AdventureSystemsTests
    {
        CoopDirector session;
        [UnitySetUp]public IEnumerator Load(){SceneManager.LoadScene("CampaignCoop");yield return null;yield return null;session=Object.FindFirstObjectByType<CoopDirector>();}
        [UnityTearDown]public IEnumerator Close(){session.Close();yield return null;yield return null;}
        IEnumerator Play(int level=0)
        {session.SelectCampaignLevel(level);Assert.IsTrue(session.HostDirect("POLISH",7876,false));yield return null;session.StartGame();float until=Time.realtimeSinceStartup+12;while(session.Phase!=CoopPhase.Playing&&Time.realtimeSinceStartup<until)yield return null;Assert.AreEqual(CoopPhase.Playing,session.Phase);}
        [Test]public void BoonsAreBoundedAndCounterIsConsumedOnce()
        {
            var build=session.Actor(0).GetComponent<CombatBuildEffects>();build.ResetRun();for(int i=0;i<3;i++)Assert.IsTrue(build.ChooseBoon(0));Assert.IsFalse(build.ChooseBoon(0));Assert.IsFalse(build.ChooseBoon(3));Assert.AreEqual(1.36f,build.Power,.001f);build.GrantCounter();Assert.AreEqual(1.25f,build.ConsumeCounter());Assert.AreEqual(1,build.ConsumeCounter());build.ResetRun();Assert.AreEqual(0,build.PackedBoons);
        }
        [Test]public void BleedRefreshDoesNotStackAndRestoreCleansIt()
        {
            var player=new GameObject("BleedSource");var victim=new GameObject("BleedTarget");var attack=ScriptableObject.CreateInstance<AttackDefinition>();try{var source=player.AddComponent<Health>();source.Configure(100,CombatFaction.Player);var build=player.AddComponent<CombatBuildEffects>();build.Configure(WeaponPerk.Bleed,BuildSynergy.None);var hp=victim.AddComponent<Health>();hp.Configure(1000,CombatFaction.Enemy);attack.damage=20;attack.knockback=0;hp.ApplyHit(attack,Vector3.forward,source);var status=victim.GetComponent<CombatAilments>();Assert.NotNull(status);status.ApplyBleed(source,2);status.ApplyBleed(source,2);status.Simulate(.5f);Assert.AreEqual(978,hp.Current);hp.Restore();Assert.AreEqual(0,status.Bits);status.Simulate(.6f);Assert.AreEqual(1000,hp.Current);}finally{Object.DestroyImmediate(player);Object.DestroyImmediate(victim);Object.DestroyImmediate(attack);}
        }
        [Test]public void BurnSynergyChangesRealDamageAndFractureReducesDefense()
        {
            var player=new GameObject("BuildSource");var victim=new GameObject("BuildTarget");var attack=ScriptableObject.CreateInstance<AttackDefinition>();try{var source=player.AddComponent<Health>();source.Configure(100,CombatFaction.Player);var build=player.AddComponent<CombatBuildEffects>();build.Configure(WeaponPerk.Scorch,BuildSynergy.ScorchedEdge);var hp=victim.AddComponent<Health>();hp.Configure(1000,CombatFaction.Enemy);attack.damage=20;attack.knockback=0;hp.ApplyHit(attack,Vector3.forward,source);Assert.AreEqual(980,hp.Current);Assert.IsTrue(victim.GetComponent<CombatAilments>().Burning);hp.ApplyHit(attack,Vector3.forward,source);Assert.AreEqual(956,hp.Current);hp.Configure(100,CombatFaction.Enemy,.4f);build.Configure(WeaponPerk.None,BuildSynergy.None);victim.GetComponent<CombatAilments>().Fracture();hp.ApplyHit(attack,Vector3.forward,source);Assert.AreEqual(86,hp.Current);}finally{Object.DestroyImmediate(player);Object.DestroyImmediate(victim);Object.DestroyImmediate(attack);}
        }
        [Test]public void ThemedLootRespectsClassAndConfiguredPreference()
        {
            var catalog=session.GetComponent<CoopProgression>().Catalog;var rule=new RewardRule{guaranteedItems=1,preferredItem="basalto-hammer",preferredChance=1,minimumRarity=ItemRarity.Uncommon};
            for(int i=0;i<10;i++){var own=LootGenerator.RewardItems(catalog,rule,new System.Random(i),"warrior");Assert.AreEqual("basalto-hammer",own[0].definitionId);var ranged=LootGenerator.RewardItems(catalog,rule,new System.Random(i),"ranger");Assert.IsTrue((catalog.Find(ranged[0].definitionId).compatibleHeroes&HeroAffinity.Ranger)!=0);}foreach(var table in catalog.minibossThemes)Assert.IsTrue(catalog.IsRewardRule(table));
        }
        [Test]public void SupportHealingDeduplicatesAllyColliders()
        {
            var player=new GameObject("SupportSource");var victim=new GameObject("SupportVictim");var ally=new GameObject("SupportAlly");var attack=ScriptableObject.CreateInstance<AttackDefinition>();
            try{player.transform.position=new Vector3(100,1,100);ally.transform.position=new Vector3(100,1,101);ally.layer=11;ally.AddComponent<BoxCollider>();ally.AddComponent<SphereCollider>();var source=player.AddComponent<Health>();source.Configure(100,CombatFaction.Player);var hp=ally.AddComponent<Health>();hp.Configure(100,CombatFaction.Player);hp.ApplyDamage(20);var build=player.AddComponent<CombatBuildEffects>();build.Configure(WeaponPerk.None,BuildSynergy.KindredGuard);var enemy=victim.AddComponent<Health>();enemy.Configure(10,CombatFaction.Enemy);attack.damage=20;attack.knockback=0;Physics.SyncTransforms();enemy.ApplyHit(attack,Vector3.forward,source);Assert.AreEqual(84,hp.Current);enemy.Restore();enemy.ApplyHit(attack,Vector3.forward,source);Assert.AreEqual(84,hp.Current,"Support interval must prevent immediate repeats");}
            finally{Object.DestroyImmediate(player);Object.DestroyImmediate(victim);Object.DestroyImmediate(ally);Object.DestroyImmediate(attack);}
        }
        [UnityTest]public IEnumerator AltarChecksDistanceMembershipAndDuplicateClaims()
        {
            yield return Play();var features=session.Campaign.Features;var node=features.Nodes.First(x=>x.kind==ExpeditionFeatureKind.Altar);var actor=session.Actor(0);Assert.IsFalse(features.ChooseBoon(actor.Health,1,node.state.id,0));actor.Motor.Teleport(node.root.position+Vector3.right*.7f,Quaternion.identity);yield return null;Assert.IsTrue(features.ChooseBoon(actor.Health,0,node.state.id,0));Assert.AreEqual(1,actor.GetComponent<CombatBuildEffects>().Boon(0));Assert.IsFalse(features.ChooseBoon(actor.Health,0,node.state.id,1));Assert.IsFalse(features.ChooseBoon(actor.Health,0,node.state.id,99));
        }
        [UnityTest]public IEnumerator FeaturesUseWalkablePointsAndThreeEventModes()
        {
            yield return Play();foreach(var node in session.Campaign.Features.Nodes){if(node.kind!=ExpeditionFeatureKind.BreakableWall)Assert.IsTrue(NavMesh.SamplePosition(node.root.position,out _,.4f,NavMesh.AllAreas),node.kind.ToString());Assert.IsTrue(node.room.Contains(node.root.position));}Assert.AreEqual(2,session.Campaign.Features.Nodes.Count(x=>x.kind==ExpeditionFeatureKind.Altar));Assert.AreEqual(2,session.Campaign.Features.Nodes.Count(x=>x.kind==ExpeditionFeatureKind.Cache));for(int i=0;i<3;i++){var data=session.Campaign.At(i).features;Assert.NotNull(data);Assert.AreEqual((OptionalEventKind)i,data.optionalEvent);Assert.IsTrue(data.enemies.All(x=>x!=null));Assert.IsTrue(data.miniboss.miniboss);}
        }
        [UnityTest]public IEnumerator PerfectDodgeHasOneFiniteCounterAndKeepsMovement()
        {
            yield return Play();var actor=session.Actor(0);Assert.IsTrue(actor.Motor.TryDodge(Vector2.up));actor.Motor.Simulate(actor.Roster.Current.dodge.invulnerableStart+.01f,Vector2.zero);Assert.IsTrue(actor.Motor.IsInvulnerable);int hp=actor.Health.Current;Assert.IsFalse(actor.Health.ApplyDamage(20));Assert.AreEqual(hp,actor.Health.Current);Assert.IsTrue(actor.CounterReady);Assert.IsFalse(actor.Motor.ConfirmPerfectDodge());Assert.AreEqual(1.25f,actor.GetComponent<CombatBuildEffects>().ConsumeCounter());Assert.IsFalse(actor.CounterReady);
        }
        [UnityTest]public IEnumerator ReleaseChargedAttackSelectsHeavyFinisher()
        {
            yield return Play();var actor=session.Actor(0);actor.Combat.ResetCombat();float duration=actor.Roster.Current.onlineFlow.holdDuration;actor.Combat.ProcessAttackInput(true,false,false,duration*.7f);actor.Combat.Simulate(3);actor.Combat.ProcessAttackInput(false,false,false,.01f);Assert.IsTrue(actor.Combat.IsHeavy);Assert.AreEqual(3,actor.Combat.AnimationVariant);
        }
        [UnityTest]public IEnumerator PingsAreBoundedAndClientIntentCannotSupplyRewards()
        {
            yield return Play();var channel=session.GetComponent<CampaignAdventureChannel>();Assert.IsFalse(channel.Handle(9,AdventureAction.Ping,0,0));Assert.IsFalse(channel.Handle(0,(AdventureAction)99,0,0));yield return new WaitForSeconds(.3f);Assert.IsTrue(channel.Handle(0,AdventureAction.Ping,0,2));Assert.AreEqual(4,session.Campaign.Features.Pings.Length);Assert.Greater(session.Campaign.Features.Pings[0].remaining,0);Assert.LessOrEqual(Vector3.Distance(session.Actor(0).transform.position,session.Campaign.Features.Pings[0].position),12);Assert.IsFalse(channel.Handle(0,AdventureAction.Ping,0,99));
        }
        [UnityTest]public IEnumerator CachePaysOnlyOnce()
        {
            yield return Play();var node=session.Campaign.Features.Nodes.First(x=>x.kind==ExpeditionFeatureKind.Cache);var actor=session.Actor(0);actor.Motor.Teleport(node.root.position+Vector3.right*.6f,Quaternion.identity);Assert.IsTrue(session.Campaign.Features.Use(actor.Health,node.state.id));int coins=session.GetComponent<CoopProgression>().Profile(0).Profile.coins;Assert.IsFalse(session.Campaign.Features.Use(actor.Health,node.state.id));Assert.AreEqual(coins,session.GetComponent<CoopProgression>().Profile(0).Profile.coins);
        }
        [UnityTest]public IEnumerator DestructibleWallAcceptsRealMeleeAndReleasesCollision()
        {
            yield return Play();var wall=session.Campaign.Features.Nodes.First(x=>x.kind==ExpeditionFeatureKind.BreakableWall);var source=session.Actor(0).Health;var hit=ScriptableObject.CreateInstance<AttackDefinition>();hit.damage=100;hit.range=4;hit.arc=360;hit.footprint=MeleeFootprint.Circle;hit.knockback=0;
            try{Physics.SyncTransforms();var resolver=new DamageResolver();Assert.Greater(resolver.Area(wall.root.position+Vector3.up+Vector3.forward*1.4f,4,Vector3.back,360,hit,source,1<<CampaignAdventure.PropLayer,1<<9),0);Assert.IsFalse(wall.health.IsAlive);yield return new WaitForSeconds(.3f);Assert.AreEqual(2,wall.state.stage);Assert.IsFalse(wall.visual.gameObject.activeSelf);}finally{Object.Destroy(hit);}
        }
        [UnityTest]public IEnumerator BridgeBreaksAndLeverRestoresSafeCrossing()
        {
            yield return Play();var bridge=session.Campaign.Features.Nodes.First(x=>x.kind==ExpeditionFeatureKind.FragileBridge);var actor=session.Actor(0);var hit=ScriptableObject.CreateInstance<AttackDefinition>();hit.damage=100;hit.knockback=0;bridge.health.ApplyHit(hit,Vector3.forward,actor.Health);Object.Destroy(hit);yield return new WaitForSeconds(1.5f);Assert.AreEqual(2,bridge.state.stage);session.Dungeon.DisableTrap(bridge.room);Assert.AreEqual(3,bridge.state.stage);Assert.IsTrue(bridge.visual.gameObject.activeSelf);Assert.IsTrue(bridge.health.IsAlive);
        }
        [UnityTest]public IEnumerator OptionalWaveCompletesAndDoesNotCompleteMainRooms()=>CompleteOptional(0);
        [UnityTest]public IEnumerator DefendEventCanBeWon()=>CompleteOptional(1);
        [UnityTest]public IEnumerator EscortEventCanBeWon()=>CompleteOptional(2);
        IEnumerator CompleteOptional(int level)
        {
            yield return Play(level);var features=session.Campaign.Features;var node=features.Nodes.First(x=>x.kind==ExpeditionFeatureKind.Challenge);var actor=session.Actor(0);actor.Motor.Teleport(node.root.position+Vector3.right*.6f,Quaternion.identity);
            float until=Time.realtimeSinceStartup+35;yield return new WaitForSeconds(.2f);if(session.Dungeon.FightRoom!=null){var fixture=Object.Instantiate(session.Dungeon.Encounter.Current);fixture.waves=new[]{new EncounterWave{delayBefore=0,entries=new[]{new SpawnEntry{enemy=session.Campaign.Level.features.enemies[0],count=1,delay=0}}}};session.Dungeon.Encounter.Begin(fixture,false);Object.Destroy(fixture,10);}
            while(session.Dungeon.FightRoom!=null&&Time.realtimeSinceStartup<until){foreach(var enemy in session.Dungeon.Encounter.Enemies.Actors)if(enemy.IsAlive)enemy.Health.ApplyDamage(100000);yield return null;}
            Assert.IsNull(session.Dungeon.FightRoom);int completed=session.Dungeon.Generator.Rooms.Count(x=>x.completed);Assert.IsTrue(features.Use(actor.Health,node.state.id));until=Time.realtimeSinceStartup+35;
            while(node.state.stage==1&&Time.realtimeSinceStartup<until){if(level==2)actor.Motor.Teleport(node.root.position+Vector3.right*.6f,Quaternion.identity);foreach(var enemy in session.Dungeon.Encounter.Enemies.Actors)if(enemy.IsAlive)enemy.Health.ApplyDamage(100000);yield return null;}
            yield return new WaitForSeconds(.2f);Assert.AreEqual(2,node.state.stage);Assert.AreEqual(completed,session.Dungeon.Generator.Rooms.Count(x=>x.completed));Assert.IsFalse(session.Dungeon.SideActivityBusy);Assert.IsFalse(features.Use(actor.Health,node.state.id));
        }
    }
}
