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
        [UnityTest]public IEnumerator ExistingHeroAttacksActuallyDamageRegionalEnemies()
        {
            yield return new WaitForSeconds(1.2f);var motor=world.Hero.GetComponent<PlayerMotor>();motor.Teleport(world.Definition.camps[0].center+new Vector3(0,.06f,-7),Quaternion.identity);
            var input=world.Hero.GetComponent<ArenaInput>();input.SetTouchMove(Vector2.zero,true);input.SetUiAttackHeld(true);
            var cohort=world.Enemies.Actors.Where(e=>e.IsAlive).ToArray();int before=cohort.Sum(e=>e.Health.Current);float until=Time.time+4;
            while(Time.time<until){world.Hero.Health.Restore();var target=world.Enemies.Actors.Where(e=>e.IsAlive).OrderBy(e=>(e.transform.position-world.Hero.transform.position).sqrMagnitude).FirstOrDefault();if(target!=null){var d=target.transform.position-world.Hero.transform.position;d.y=0;if(d.sqrMagnitude>.01f)world.Hero.transform.rotation=Quaternion.LookRotation(d);}yield return null;}
            input.SetUiAttackHeld(false);input.SetTouchMove(Vector2.zero,false);Assert.Less(cohort.Sum(e=>e.IsAlive?e.Health.Current:0),before);
        }
        [UnityTest]public IEnumerator CampaignHostCanEnterRegionAndReturnWithoutDuplicateNetworkManager()
        {
            yield return SceneManager.LoadSceneAsync("CampaignCoop");yield return null;yield return null;
            var director=Object.FindFirstObjectByType<CoopDirector>();Assert.IsTrue(director.HostDirect("REGIONTRAVEL",7908,false));yield return null;
            var button=Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(b=>b.name=="RegionalWorld");button.onClick.Invoke();
            float end=Time.realtimeSinceStartup+12;while(SceneManager.GetActiveScene().name!="CostaDasRuinas"&&Time.realtimeSinceStartup<end)yield return null;yield return null;
            world=Object.FindFirstObjectByType<RegionalWorld>();Assert.NotNull(world);Assert.IsNull(Object.FindFirstObjectByType<Unity.Netcode.NetworkManager>());
            world.ReturnToCampaign();end=Time.realtimeSinceStartup+12;while(SceneManager.GetActiveScene().name!="CampaignCoop"&&Time.realtimeSinceStartup<end)yield return null;yield return null;yield return null;
            Assert.AreEqual(1,Object.FindObjectsByType<Unity.Netcode.NetworkManager>(FindObjectsSortMode.None).Length);Assert.NotNull(Object.FindFirstObjectByType<CoopDirector>());
        }
        [Test]public void DeathAndRespawnRestoreSafeRefuge()
        {world.Hero.Health.ApplyDamage(10000);Assert.IsFalse(world.Hero.Health.IsAlive);world.Respawn();Assert.IsTrue(world.Hero.Health.IsAlive);Assert.Less(Vector3.Distance(world.Hero.transform.position,world.Definition.refuge),.2f);}
        [Test]public void SceneryContainsThreeLodsAndSingleMaterialPerRenderer()
        {var groups=Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None);Assert.Greater(groups.Length,5);foreach(var group in groups){var levels=group.GetLODs();Assert.AreEqual(3,levels.Length);foreach(var level in levels){Assert.Greater(level.renderers.Length,0);foreach(var renderer in level.renderers)Assert.AreEqual(1,renderer.sharedMaterials.Length);}}}
        [UnityTest]public IEnumerator InputMovementChangesPlayerPositionWithoutChangingElevation()
        {var input=world.Hero.GetComponent<ArenaInput>();var before=world.Hero.transform.position;input.SetTouchMove(Vector2.right,true);yield return new WaitForSeconds(.7f);input.SetTouchMove(Vector2.zero,false);Assert.Greater(Vector3.Distance(before,world.Hero.transform.position),1);Assert.Greater(world.Hero.transform.position.y,-.5f);}
    }
}
