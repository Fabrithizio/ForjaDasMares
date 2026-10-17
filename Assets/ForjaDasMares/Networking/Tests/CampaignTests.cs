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
        [Test] public void ProgressBeforeMapGenerationIsZeroAndSafeForSessionAdvertising()
        {run.Authority=true;Assert.AreEqual(0,run.Completion);}
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
        [UnityTest] public IEnumerator CampaignStartsSoloWithEmptyBagAndTenPercentBonus()
        {
            Assert.IsTrue(session.HostDirect("CMPTEST",7792,false));yield return null;
            Assert.AreEqual(0,session.GetComponent<CoopProgression>().Profile(0).Profile.inventory.Count);
            Assert.NotNull(session.Actor(0).Combat.BasicAttack);session.StartGame();yield return null;
            Assert.AreEqual(CoopPhase.Playing,session.Phase);var gear=session.Actor(0).GetComponent<HeroEquipment>();
            var normal=ProgressionStats.Calculate(session.Actor(0).Roster.Current,session.GetComponent<CoopProgression>().Profile(0).Profile,session.GetComponent<CoopProgression>().Catalog);
            Assert.AreEqual(Mathf.RoundToInt(normal.maximumHealth*1.1f),gear.Stats.maximumHealth);Assert.AreEqual(normal.damageMultiplier*1.1f,gear.Stats.damageMultiplier,.0001f);
            session.Actor(0).Health.ApplyDamage(30);float before=(float)session.Actor(0).Health.Current/session.Actor(0).Health.Maximum;
            for(int i=0;i<5;i++){gear.SetCampaignSolo(false);gear.SetCampaignSolo(true);}
            Assert.That((float)session.Actor(0).Health.Current/session.Actor(0).Health.Maximum,Is.LessThanOrEqualTo(before));
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
            Assert.AreEqual(4,CoopLimits.Protocol);
        }
        [UnityTest] public IEnumerator SoloCanSolvePlatesSequentiallyAndAdmissionClosesAtTwentyPercent()
        {
            Assert.IsTrue(session.HostDirect("CMPSOLO",7796,false));yield return null;session.StartGame();yield return null;
            var pair=run.Puzzles[0];session.Dungeon.Enter(pair.Room);pair.Solo=true;var actor=session.Actor(0);
            actor.Motor.Teleport(pair.Point(0)+Vector3.up*.06f,Quaternion.identity);pair.Tick(.2f);Assert.IsFalse(pair.Solved);
            actor.Motor.Teleport(pair.Point(1)+Vector3.up*.06f,Quaternion.identity);pair.Tick(2);Assert.IsTrue(pair.Solved);
            Assert.IsTrue(session.CanAdmitNewPlayer);
            int target=Mathf.CeilToInt((run.Level.route.Length-1)*.2f);int marked=0;
            foreach(var room in session.Dungeon.Generator.Rooms)if(room.node.category!=RoomCategory.Entrance){room.completed=true;if(++marked>=target)break;}
            Assert.That(run.Completion,Is.GreaterThanOrEqualTo(.2f));Assert.IsFalse(session.CanAdmitNewPlayer);
        }
        [Test] public void LateParticipantReceivesOnlyFutureRewardsAndCannotReplayPastEvents()
        {
            var catalog=session.GetComponent<CoopProgression>().Catalog;
            ProfileService Profile()=>new ProfileService(catalog,new ProfileStore(Path.Combine(Application.temporaryCachePath,"LateJoin",Guid.NewGuid().ToString("N"),"profile.json")));
            var host=Profile();var late=Profile();var ledger=new CoopRewardLedger(catalog);ledger.Assign(0,host);
            Assert.IsTrue(ledger.Begin(new[]{"warrior",null,null,null},"Campaign"));Assert.IsTrue(ledger.Grant("past",catalog.room,1));
            Assert.IsTrue(ledger.Join(1,late,"warrior","Campaign"));Assert.IsTrue(ledger.Grant("past",catalog.room,1));Assert.AreEqual(0,late.Profile.expedition.experience);
            Assert.IsTrue(ledger.Grant("future",catalog.room,2));Assert.AreEqual(catalog.room.experience,late.Profile.expedition.experience);
            Assert.IsFalse(ledger.Join(1,Profile(),"warrior","Campaign"));Assert.IsTrue(ledger.Finish(true,"Victory"));
        }
        [UnityTest] public IEnumerator HomeCampaignFriendsAndSettingsUseRealNavigation()
        {
            yield return null;var hud=UnityEngine.Object.FindFirstObjectByType<CampaignHud>();Assert.IsTrue(hud.AtHome);
            var home=hud.GetComponentInChildren<CampaignHome>();Assert.NotNull(home);
            UnityEngine.UI.Button Find(string name){foreach(var button in home.GetComponentsInChildren<UnityEngine.UI.Button>(true))if(button.name==name)return button;return null;}
            Find("Friends").onClick.Invoke();Assert.IsTrue(Find("SaveInvite").gameObject.activeInHierarchy);
            foreach(var button in home.GetComponentsInChildren<UnityEngine.UI.Button>())if(button.name=="Back"){button.onClick.Invoke();break;}
            Find("Settings").onClick.Invoke();float prior=AudioListener.volume;Find("VolumeDown").onClick.Invoke();Assert.That(AudioListener.volume,Is.LessThanOrEqualTo(prior));Find("VolumeUp").onClick.Invoke();
            Find("Campaign").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);Assert.IsFalse(hud.AtHome);
        }
    }
}
