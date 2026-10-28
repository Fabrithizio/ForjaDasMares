using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class RegionalWorldTests
    {
        RegionalWorld world;
        [UnitySetUp]public IEnumerator Load(){yield return SceneManager.LoadSceneAsync("CostaDasRuinas");yield return null;yield return null;world=Object.FindFirstObjectByType<RegionalWorld>();Assert.NotNull(world);}
        [Test]public void TerrainSectorsAndAllDestinationsAreConnected()
        {
            Assert.AreEqual(4,Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None).Length);
            Assert.IsTrue(NavMesh.SamplePosition(world.Definition.refuge,out var start,3,NavMesh.AllAreas));
            foreach(var destination in world.Definition.route){Assert.IsTrue(NavMesh.SamplePosition(destination,out var end,3,NavMesh.AllAreas));var path=new NavMeshPath();Assert.IsTrue(NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path));Assert.AreEqual(NavMeshPathStatus.PathComplete,path.status,destination.ToString());}
            Assert.IsFalse(world.PortalReady);
        }
        [UnityTest]public IEnumerator PatrolsSpawnWithinBudgetAndFirstClearDoesNotDuplicate()
        {
            yield return new WaitForSeconds(1.2f);Assert.Greater(world.Enemies.ActiveCount,0);Assert.LessOrEqual(world.Enemies.ActiveCount,world.Definition.activeEnemyLimit);
            var camp=world.Definition.camps[0];float prior=camp.respawnSeconds;camp.respawnSeconds=.3f;
            try
            {
                var first=world.Enemies.Actors.Where(e=>e.IsAlive&&(e.transform.position-camp.center).sqrMagnitude<18*18).ToArray();Assert.AreEqual(camp.enemies.Length,first.Length);
                foreach(var enemy in first)enemy.Health.ApplyDamage(10000);
                Assert.AreEqual(1,world.ClearedCount);Assert.IsTrue(world.IsCampCleared(0));
                yield return new WaitForSeconds(2);Assert.Greater(world.Enemies.ActiveCount,0);Assert.AreEqual(1,world.ClearedCount);
            }finally{camp.respawnSeconds=prior;}
        }
        [Test]public void DeathAndRespawnRestoreSafeRefuge()
        {world.Hero.Health.ApplyDamage(10000);Assert.IsFalse(world.Hero.Health.IsAlive);world.Respawn();Assert.IsTrue(world.Hero.Health.IsAlive);Assert.Less(Vector3.Distance(world.Hero.transform.position,world.Definition.refuge),.2f);}
        [Test]public void SceneryContainsThreeLodsAndSingleMaterialPerRenderer()
        {var groups=Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None);Assert.Greater(groups.Length,5);foreach(var group in groups){var levels=group.GetLODs();Assert.AreEqual(3,levels.Length);foreach(var level in levels){Assert.Greater(level.renderers.Length,0);foreach(var renderer in level.renderers)Assert.AreEqual(1,renderer.sharedMaterials.Length);}}}
        [UnityTest]public IEnumerator InputMovementChangesPlayerPositionWithoutChangingElevation()
        {var input=world.Hero.GetComponent<ArenaInput>();var before=world.Hero.transform.position;input.SetTouchMove(Vector2.right,true);yield return new WaitForSeconds(.7f);input.SetTouchMove(Vector2.zero,false);Assert.Greater(Vector3.Distance(before,world.Hero.transform.position),1);Assert.Greater(world.Hero.transform.position.y,-.5f);}
    }
}
