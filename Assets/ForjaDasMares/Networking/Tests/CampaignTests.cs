using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class CampaignTests
    {
        CoopDirector session;CampaignRun run;
        [UnitySetUp] public IEnumerator Load()
        {SceneManager.LoadScene("CampaignCoop");yield return null;yield return null;session=UnityEngine.Object.FindFirstObjectByType<CoopDirector>();run=session.Campaign;Assert.NotNull(run);}
        [UnityTearDown] public IEnumerator Close(){if(session!=null)session.Close();yield return null;yield return null;}
        [Test] public void ThreeIncreasingRoutesHaveRealDataAndBoundedNetworkMasks()
        {
            Assert.AreEqual(3,run.Count);int previous=0;
            for(int i=0;i<3;i++)
            {
                var data=run.At(i);data.Validate();Assert.That(data.route.Length,Is.GreaterThan(previous));previous=data.route.Length;
                var layout=DungeonLayout.Campaign(data.route,data.categories);Assert.IsTrue(layout.Validate(24,out var error),error);
                Assert.AreEqual(RoomCategory.Boss,layout.rooms[layout.BossId].category);
                int events=0,traps=0,combats=0;foreach(var category in data.categories){if(category==RoomCategory.Event)events++;if(category==RoomCategory.Trap)traps++;if(category==RoomCategory.Combat||category==RoomCategory.HardCombat)combats++;}
                Assert.That(events,Is.GreaterThanOrEqualTo(2));Assert.That(traps,Is.GreaterThanOrEqualTo(1));Assert.That(combats,Is.GreaterThanOrEqualTo(4));
                Assert.That(data.normal.maximumPcEnemies,Is.LessThanOrEqualTo(10));Assert.That(data.dartInterval,Is.GreaterThan(2.5f));
            }
        }
        [UnityTest] public IEnumerator EveryCampaignMapBakesConnectedNavigationAndCreatesPuzzlesAndMist()
        {
            for(int i=0;i<3;i++)
            {
                run.Prepare(i);session.Dungeon.ConfigureTest(run.Level.seed,true,run.Level.biomeIndex);yield return null;
                Assert.IsTrue(session.Dungeon.Generator.NavigationConnected);Assert.AreEqual(12+i*3,session.Dungeon.Generator.Rooms.Length);
                Assert.That(run.Puzzles.Count,Is.GreaterThanOrEqualTo(2));Assert.NotNull(GameObject.Find("ExteriorMist_OriginalPlaceholder"));
                Assert.That(session.Dungeon.Generator.GetComponentsInChildren<CampaignWallDarts>().Length,Is.GreaterThan(1));
                foreach(var room in session.Dungeon.Generator.Rooms)if(room.cooperativeObjective)Assert.IsFalse(room.objectiveComplete);
            }
        }
        [UnityTest] public IEnumerator CampaignStartsWithNoTestItemsAndRequiresTwoPlayers()
        {
            Assert.IsTrue(session.HostDirect("CMPTEST",7792,false));yield return null;
            Assert.AreEqual(0,session.GetComponent<CoopProgression>().Profile(0).Profile.inventory.Count);
            Assert.NotNull(session.Actor(0).Combat.BasicAttack);session.StartGame();Assert.AreEqual(CoopPhase.Lobby,session.Phase);
        }
        [UnityTest] public IEnumerator TwoDistinctLivingHeroesMustHoldSeparatePlatesAndUnlockTheDoors()
        {
            Assert.IsTrue(session.HostDirect("CMPLATE",7793,false));yield return null;
            run.Prepare(0);session.Dungeon.ConfigureTest(run.Level.seed,true,0);yield return null;
            var pair=run.Puzzles[0];session.Dungeon.Enter(pair.Room);var a=session.Actor(0);var b=session.Actor(1);
            b.gameObject.SetActive(true);session.Dungeon.Party.Set(1,b.Health);
            a.Motor.Teleport(pair.Point(0)+Vector3.up*.06f,Quaternion.identity);b.Motor.Teleport(pair.Point(0)+Vector3.up*.06f,Quaternion.identity);
            pair.Tick(5);Assert.IsFalse(pair.Solved);Assert.AreEqual(1,pair.Occupants);Assert.IsTrue(pair.Room.Locked);
            b.Motor.Teleport(pair.Point(1)+Vector3.up*.06f,Quaternion.identity);pair.Tick(.7f);Assert.IsFalse(pair.Solved);
            pair.Tick(1);Assert.IsTrue(pair.Solved);Assert.IsTrue(pair.Room.completed);Assert.IsFalse(pair.Room.Locked);Assert.IsTrue(session.Dungeon.HasKey);
        }
        [UnityTest] public IEnumerator WallDartsOnlyDamageOnAuthorityAndDoNotHitRepeatedlyDuringOneFlight()
        {
            Assert.IsTrue(session.HostDirect("CMPDART",7794,false));yield return null;session.enabled=false;
            run.Prepare(0);session.Dungeon.ConfigureTest(run.Level.seed,true,0);yield return null;
            DungeonRoom trap=null;foreach(var room in session.Dungeon.Generator.Rooms)if(room.node.category==RoomCategory.Trap){trap=room;break;}
            session.Dungeon.Enter(trap);foreach(var old in session.Dungeon.Generator.Traps)old.enabled=false;
            var actor=session.Actor(0);actor.Motor.enabled=false;actor.GetComponent<CharacterController>().enabled=true;actor.Motor.Teleport(trap.Center+new Vector3(0,.06f,-.5f),Quaternion.identity);
            int hp=actor.Health.Current;run.Authority=false;run.Running=true;yield return new WaitForSeconds(2.3f);Assert.AreEqual(hp,actor.Health.Current);
            run.Apply(new CampaignSnapshot{clock=0,progress=new float[run.Puzzles.Count]});run.Authority=true;
            // A new cycle resets the hit ledger; use a later full cycle to avoid previous-cycle state.
            yield return new WaitForSeconds(4.4f);int before=actor.Health.Current;yield return new WaitForSeconds(2);Assert.That(actor.Health.Current,Is.EqualTo(before-run.Level.dartAttack.damage));
        }
        [Test] public void CampaignLootStartsEmptyPersistsOnceAndSavesMapCompletion()
        {
            string path=Path.Combine(Application.temporaryCachePath,"CampaignTests",Guid.NewGuid().ToString("N"),"profile.json");var catalog=session.GetComponent<CoopProgression>().Catalog;
            var service=new ProfileService(catalog,new ProfileStore(path));Assert.AreEqual(0,service.Profile.inventory.Count);
            Assert.IsTrue(service.BeginExpedition("warrior","Campaign"));string id=service.Profile.expedition.runId;
            Assert.IsTrue(service.Grant(id,"chest",catalog.chest,123));Assert.IsFalse(service.Grant(id,"chest",catalog.chest,123));
            Assert.IsTrue(service.FinishExpedition(id,true,"Victory"));Assert.IsTrue(service.MarkCampaignClear(1));Assert.IsFalse(service.FinishExpedition(id,true,"Victory"));
            var loaded=new ProfileService(catalog,new ProfileStore(path));Assert.That(loaded.Profile.inventory.Count,Is.GreaterThan(0));Assert.AreEqual(2u,loaded.Profile.campaignCleared);
        }
        [UnityTest] public IEnumerator InventoryOpensWithFiveRealEquipmentSlotsAndAnEmptyBag()
        {
            Assert.IsTrue(session.HostDirect("CMPBAG",7795,false));yield return null;
            var hud=UnityEngine.Object.FindFirstObjectByType<CoopInventoryHud>();
            foreach(var button in hud.GetComponentsInChildren<UnityEngine.UI.Button>(true))if(button.name=="INVENTÁRIO"){button.onClick.Invoke();break;}
            yield return null;int slots=0;
            foreach(var button in hud.GetComponentsInChildren<UnityEngine.UI.Button>())if(button.name.StartsWith("EquipmentSlot_"))slots++;
            Assert.AreEqual(5,slots);Assert.AreEqual(0,session.GetComponent<CoopProgression>().Profile(0).Profile.inventory.Count);
            Assert.NotNull(hud.GetComponentInChildren<UnityEngine.UI.RawImage>());
        }
        [Test] public void CampaignFrameRoundtripContainsSharedLevelProgressAndAnimationVariants()
        {
            var frame=new WorldFrame{campaignLevel=2,campaign=new CampaignSnapshot{clock=12.5f,occupied=3,progress=new[]{.4f,1f}}};
            frame.players.Add(new ActorFrame{variant=2,actionVersion=12,impact=.3f});
            var copy=JsonUtility.FromJson<WorldFrame>(JsonUtility.ToJson(frame));Assert.AreEqual(2,copy.campaignLevel);Assert.AreEqual(.4f,copy.campaign.progress[0]);Assert.AreEqual(2,copy.players[0].variant);
            Assert.AreEqual(3,CoopLimits.Protocol);
        }
    }
}
