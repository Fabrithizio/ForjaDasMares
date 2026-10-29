using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class CatacombPreviewTests
    {
        RegionalWorld world;
        [UnitySetUp]public IEnumerator Load(){yield return SceneManager.LoadSceneAsync("CatacumbasDasMares");yield return null;yield return null;world=Object.FindFirstObjectByType<RegionalWorld>();Assert.NotNull(world);}
        [Test]public void RouteAndSpawnPointsAreConnectedOutsideWalls()
        {
            Assert.IsTrue(NavMesh.SamplePosition(world.Definition.refuge,out var start,2,NavMesh.AllAreas));
            foreach(var point in world.Definition.route.Concat(world.Definition.camps.SelectMany(c=>c.spawns)))
            {Assert.IsTrue(NavMesh.SamplePosition(point,out var end,2,NavMesh.AllAreas));var path=new NavMeshPath();Assert.IsTrue(NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path));Assert.AreEqual(NavMeshPathStatus.PathComplete,path.status);Assert.IsFalse(Physics.CheckSphere(end.position+Vector3.up*.6f,.4f,1<<9));}
            foreach(var wall in Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Where(c=>c.name=="CatacombWall"))Assert.NotNull(wall.GetComponentInParent<Renderer>(),"Merged wall must remain visible to camera occlusion");
            Assert.Less(Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Count(l=>l.type==LightType.Point),8);
        }
        [UnityTest]public IEnumerator RealSkinnedClipsDeformBonesWithoutMovingActorRoot()
        {
            yield return new WaitForSeconds(1);var enemy=world.Enemies.Actors.First(e=>e.IsAlive);enemy.Brain.enabled=false;enemy.Combat.Cancel();enemy.Motor.Stop();
            var driver=enemy.GetComponentInChildren<RiggedEnemyAnimation>();Assert.NotNull(driver);driver.enabled=false;
            var animator=enemy.GetComponentInChildren<Animator>();animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.speed=1;
            foreach(var name in new[]{"Idle","Walk","Run","Attack","Hit","Dead"})Assert.IsTrue(animator.runtimeAnimatorController.animationClips.Any(c=>c.name==name&&c.length>.1f),name);
            var foot=animator.GetComponentsInChildren<Transform>().First(t=>t.name=="Bone_008");var root=enemy.transform.position;
            animator.Play("Run",0,.1f);animator.Update(0);var before=foot.localRotation;animator.Play("Run",0,.6f);animator.Update(0);
            Assert.Greater(Quaternion.Angle(before,foot.localRotation),5);Assert.Less(Vector3.Distance(root,enemy.transform.position),.001f);
            var lod=enemy.GetComponentInChildren<LODGroup>().GetLODs();Assert.AreEqual(3,lod.Length);
            Assert.Less(((SkinnedMeshRenderer)lod[0].renderers[0]).sharedMesh.GetIndexCount(0)/3,7000);
        }
        [UnityTest]public IEnumerator EnemyCanDamagePlayerDieAndReturnToPool()
        {
            yield return new WaitForSeconds(1);world.Hero.GetComponent<PlayerMotor>().Teleport(world.Definition.camps[0].center+new Vector3(0,.06f,-4),Quaternion.identity);
            int before=world.Hero.Health.Current;float end=Time.time+8;while(Time.time<end&&world.Hero.Health.Current==before)yield return null;Assert.Less(world.Hero.Health.Current,before);
            var enemy=world.Enemies.Actors.First(e=>e.IsAlive);enemy.Health.ApplyDamage(10000);yield return null;Assert.AreEqual(5,enemy.GetComponentInChildren<RiggedEnemyAnimation>().CurrentState);yield return new WaitForSeconds(.8f);Assert.IsFalse(enemy.IsSpawned);
        }
        [UnityTest]public IEnumerator CampClearAndRespawnReuseExistingRules()
        {
            for(int c=0;c<3;c++)
            {world.Hero.GetComponent<PlayerMotor>().Teleport(world.Definition.camps[c].center+new Vector3(0,.06f,-10),Quaternion.identity);yield return new WaitForSeconds(1);foreach(var enemy in world.Enemies.Actors)if(enemy.IsAlive&&(enemy.transform.position-world.Definition.camps[c].center).sqrMagnitude<25*25)enemy.Health.ApplyDamage(10000);world.Hero.Health.Restore();}
            Assert.AreEqual(3,world.ClearedCount);Assert.IsTrue(world.PortalReady);Assert.LessOrEqual(world.Enemies.ActiveCount,10);
            world.Hero.Health.ApplyDamage(10000);world.Respawn();Assert.IsTrue(world.Hero.Health.IsAlive);Assert.Less(Vector3.Distance(world.Hero.transform.position,world.Definition.refuge),.2f);
        }
    }
}
