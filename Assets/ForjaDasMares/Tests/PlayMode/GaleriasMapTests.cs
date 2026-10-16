using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Tests
{
    public sealed class GaleriasMapTests
    {
        DungeonSession dungeon; ProgressionSceneBridge bridge;
        [UnitySetUp] public IEnumerator Load()
        {
            SceneManager.LoadScene("GaleriasDungeon"); yield return null; yield return null; yield return new WaitForSeconds(.15f);
            dungeon=Object.FindFirstObjectByType<DungeonSession>(); bridge=Object.FindFirstObjectByType<ProgressionSceneBridge>();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            bridge.Abort(); var profile=Object.FindFirstObjectByType<ProgressionProfileRuntime>();
            SceneManager.LoadScene("ArenaTest"); if(profile!=null)Object.Destroy(profile.gameObject); yield return null;
        }
        [Test] public void OriginalRouteIsConnectedDeterministicAndDistinctFromTheReferenceMap()
        {
            var map=dungeon.Generator.Layout; var style=dungeon.Generator.Style;
            Assert.NotNull(style); Assert.That(map.rooms.Count,Is.EqualTo(9)); Assert.IsTrue(map.Validate(9,out string error),error);
            Assert.That(map.Signature(),Is.EqualTo(DungeonLayout.Generate(73129,9,true,style.mainRoute).Signature()));
            Assert.That(map.Signature(),Is.Not.EqualTo(DungeonLayout.Generate(73129,9,true).Signature()));
            Assert.That(dungeon.Biome.biomeId,Is.EqualTo("galerias-da-mare"));
            Assert.Throws<System.ArgumentException>(()=>DungeonLayout.Generate(1,9,true,new[]{Vector2Int.zero}));
        }
        [UnityTest] public IEnumerator NavigationAndEverySpawnPointRemainClear()
        {
            Assert.IsTrue(dungeon.Generator.NavigationConnected);
            var doors=dungeon.Generator.Doors; var closed=new bool[doors.Count];
            for(int i=0;i<doors.Count;i++) { closed[i]=doors[i].IsClosed; doors[i].SetClosed(false); }
            try
            {
                // Locked encounters intentionally carve the navigation mesh. Test the physical
                // connections with gates open, then restore the progression-controlled state.
                yield return null; yield return null; yield return new WaitForSeconds(.2f);
                Assert.IsTrue(dungeon.Generator.ValidateNavigation(out string error),error);
                foreach(var room in dungeon.Generator.Rooms)foreach(var point in room.spawnPoints)
                    Assert.IsFalse(Physics.CheckSphere(point+Vector3.up*.6f,.4f,1<<9,QueryTriggerInteraction.Ignore),"Blocked spawn in room "+room.node.id);
            }
            finally { for(int i=0;i<doors.Count;i++)doors[i].SetClosed(closed[i]); }
        }
        [Test] public void RoomEntryKeepsTheSameCameraFocusAndContinuousScenery()
        {
            dungeon.enabled = false;
            var follow = Object.FindFirstObjectByType<ArenaCamera>();
            var motor = dungeon.Heroes.GetComponent<PlayerMotor>();
            var rooms = dungeon.Generator.Rooms;
            motor.Teleport(Vector3.Lerp(rooms[0].Center, rooms[1].Center, .5f) + Vector3.up * .06f, Quaternion.identity);
            follow.Snap(); Vector3 before = follow.transform.position; Quaternion rotation = follow.transform.rotation;
            dungeon.Enter(rooms[1]); follow.Snap();
            Assert.That(Vector3.Distance(before, follow.transform.position), Is.LessThan(.001f),
                "Changing the encounter room must not change the camera target.");
            Assert.That(Quaternion.Angle(rotation, follow.transform.rotation), Is.LessThan(.001f));
            foreach (var room in rooms) foreach (var renderer in room.renderers)
                Assert.IsTrue(renderer.enabled, "Scenery must not pop when entering another room.");
            foreach (var corridor in dungeon.Generator.Corridors) foreach (var renderer in corridor.renderers)
                Assert.IsTrue(renderer.enabled, "Connected corridors remain visible.");
        }
        [Test] public void SavedMeshesMaterialsAndSolidCratesArePresent()
        {
            int styled=0,crates=0; var style=dungeon.Generator.Style;
            var meshes=new System.Collections.Generic.HashSet<Mesh> { style.floor,style.bossFloor,style.corridorFloor,style.longWallX,style.longWallZ,style.shortWallX,style.shortWallZ,style.crate,style.lantern,style.gate,style.chestLid };
            foreach(var room in dungeon.Generator.Rooms)foreach(var renderer in room.renderers)
            {
                var filter=renderer.GetComponent<MeshFilter>(); if(filter==null)continue;
                Assert.NotNull(filter.sharedMesh); var materials=renderer.sharedMaterials;
                Assert.That(materials.Length,Is.GreaterThanOrEqualTo(filter.sharedMesh.subMeshCount));
                foreach(var material in materials) { Assert.NotNull(material); Assert.NotNull(material.shader); }
                if(meshes.Contains(filter.sharedMesh))styled++;
                if(renderer.name=="Obstacle") { crates++; Assert.IsTrue(renderer.GetComponent<BoxCollider>().enabled); Assert.That(materials.Length,Is.EqualTo(2)); }
            }
            Assert.That(styled,Is.GreaterThan(40)); Assert.That(crates,Is.EqualTo(12));
            Assert.That(dungeon.Generator.Style.floor.vertexCount,Is.GreaterThan(1000));
        }
        [UnityTest] public IEnumerator CompleteRouteUsesRealEncountersObjectivesChestAndExit()
        {
            var motor=dungeon.Heroes.GetComponent<PlayerMotor>();
            for(int id=1;id<=6;id++)
            {
                var room=dungeon.Generator.Rooms[id]; motor.Teleport(room.Center+new Vector3(0,.06f,-3),Quaternion.identity); dungeon.Enter(room);
                if(room.encounter!=null)
                {
                    Assert.IsTrue(room.Locked); foreach(var door in room.doors)Assert.IsTrue(door.IsClosed);
                    float elapsed=0;
                    while(!room.encounterComplete&&elapsed<20)
                    {
                        foreach(var enemy in dungeon.Encounter.Enemies.Actors)if(enemy.IsAlive)enemy.Health.ApplyDamage(100000);
                        yield return new WaitForSeconds(.1f); elapsed+=.1f;
                    }
                    Assert.IsTrue(room.encounterComplete,"Encounter did not finish in room "+id);
                }
                foreach(var item in room.interactions)
                {
                    if(item.Kind==DungeonInteractionKind.Exit)continue;
                    motor.Teleport(item.transform.position+new Vector3(0,.06f,-1.3f),Quaternion.identity);
                    Assert.IsTrue(item.Use(),"Interaction failed: "+item.Kind); Assert.IsFalse(item.Use(),"Interaction duplicated");
                }
                Assert.IsTrue(room.completed,"Room objective not completed: "+id);
            }
            Assert.IsTrue(dungeon.HasKey); Assert.That(dungeon.VisibleCoins,Is.GreaterThan(0));
            var boss=dungeon.Generator.Rooms[6]; var exit=boss.interactions[0];
            motor.Teleport(exit.transform.position+new Vector3(0,.06f,-1.3f),Quaternion.identity); Assert.IsTrue(exit.Use());
            Assert.IsTrue(dungeon.Finished); Assert.NotNull(bridge.Service.Profile.lastResult);
            string result=bridge.Service.Profile.lastResult.runId; Assert.IsFalse(exit.Use()); Assert.That(bridge.Service.Profile.lastResult.runId,Is.EqualTo(result));
        }
        [UnityTest] public IEnumerator ArenaAndDungeonLinksReturnToTheNewMap()
        {
            dungeon.BackToArena(); yield return null; yield return null;
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("GaleriasArena"));
            bridge=Object.FindFirstObjectByType<ProgressionSceneBridge>(); bridge.OpenDungeon(); yield return null; yield return null;
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("GaleriasDungeon"));
            dungeon=Object.FindFirstObjectByType<DungeonSession>(); bridge=Object.FindFirstObjectByType<ProgressionSceneBridge>(); Assert.NotNull(dungeon.Generator.Style);
        }
    }
}
