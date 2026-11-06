using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ForjaDasMares.Networking.Tests
{
    public sealed class TownLayoutTests
    {
        RegionalWorld world;
        [UnitySetUp] public IEnumerator Load()
        {yield return SceneManager.LoadSceneAsync("RefugioUltimoSino");yield return null;yield return null;world=Object.FindFirstObjectByType<RegionalWorld>();}
        [Test] public void SceneryExtendsBeyondWalkableTownWithoutNavigationColliders()
        {
            var landscape=GameObject.Find("TownLandscape");Assert.NotNull(landscape);
            Assert.Greater(landscape.GetComponent<Renderer>().bounds.size.x,400);
            Assert.IsNull(landscape.GetComponent<Collider>());
            Assert.IsTrue(RenderSettings.fog);Assert.Greater(RenderSettings.fogEndDistance,RenderSettings.fogStartDistance);
            Assert.NotNull(GameObject.Find("DistantRidges"));
        }
        [Test] public void RoadHasAContinuousPathThroughTheTownWall()
        {
            var passage=Object.FindFirstObjectByType<TownRoadPassage>();Assert.NotNull(passage);
            Assert.IsTrue(passage.GetComponent<BoxCollider>().isTrigger);
            Assert.IsNull(passage.GetComponent<Renderer>());
            Assert.IsTrue(NavMesh.SamplePosition(world.Definition.refuge,out var start,1,NavMesh.AllAreas));
            Assert.IsTrue(NavMesh.SamplePosition(new Vector3(0,.05f,-42),out var end,1,NavMesh.AllAreas));
            var path=new NavMeshPath();Assert.IsTrue(NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path));Assert.AreEqual(NavMeshPathStatus.PathComplete,path.status);
            Assert.IsFalse(Physics.CheckCapsule(new Vector3(0,.5f,-36),new Vector3(0,1.3f,-36),.35f,1<<9,QueryTriggerInteraction.Ignore));
        }
        [UnityTest] public IEnumerator MaximumZoomAndScreenRatiosKeepLandscapeUnderEveryViewportCorner()
        {
            var follow=Object.FindFirstObjectByType<ArenaCamera>();var camera=follow.GetComponent<Camera>();
            var bounds=GameObject.Find("TownLandscape").GetComponent<Renderer>().bounds;
            var motor=world.Hero.GetComponent<PlayerMotor>();world.Hero.GetComponent<ArenaInput>().Desktop.Cancel();
            foreach(float aspect in new[]{4f/3,16f/9,20f/9,9f/16})
            {
                camera.aspect=aspect;follow.SetZoom(1.35f);yield return new WaitForSeconds(.7f);
                foreach(var point in new[]{new Vector3(-34,.08f,-33),new Vector3(34,.08f,-33),new Vector3(-34,.08f,33),new Vector3(34,.08f,33)})
                {
                    motor.Teleport(point,Quaternion.identity);follow.Snap();
                    foreach(var corner in new[]{Vector2.zero,Vector2.one,Vector2.right,Vector2.up})
                    {var ray=camera.ViewportPointToRay(corner);Assert.IsTrue(new Plane(Vector3.up,Vector3.zero).Raycast(ray,out float d));var hit=ray.GetPoint(d);Assert.Less(Mathf.Abs(hit.x),bounds.extents.x-5);Assert.Less(Mathf.Abs(hit.z),bounds.extents.z-5);}
                }
            }
        }
        [UnityTest] public IEnumerator ExitDoesNotAdvertiseAnUnrelatedChallengeInteraction()
        {
            world.Hero.GetComponent<ArenaInput>().Desktop.Cancel();
            world.Hero.GetComponent<PlayerMotor>().Teleport(new Vector3(0,.08f,-34),Quaternion.identity);yield return null;
            Assert.IsFalse(world.GetComponent<TownHub>().Hint.Contains("desafios"));
            Assert.IsFalse(world.GetComponent<TownHub>().IsTravelling);
        }
        [UnityTest] public IEnumerator ExpeditionCourtInsideTownOpensTheExistingFunctionalLobby()
        {
            var board=System.Array.Find(Object.FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None),t=>t.Label.StartsWith("Expedições"));
            Assert.NotNull(board);Assert.Less(Mathf.Abs(board.transform.position.x),35);Assert.Less(Mathf.Abs(board.transform.position.z),35);
            world.Hero.GetComponent<ArenaInput>().Desktop.Cancel();world.Hero.GetComponent<PlayerMotor>().Teleport(board.Approach,Quaternion.identity);yield return null;
            Assert.IsTrue(board.TryUse(world.Hero.transform));
            float deadline=Time.realtimeSinceStartup+8;
            while(SceneManager.GetActiveScene().name!="CampaignCoop"&&Time.realtimeSinceStartup<deadline)yield return null;
            yield return null;Assert.AreEqual("CampaignCoop",SceneManager.GetActiveScene().name);Assert.NotNull(Object.FindFirstObjectByType<CampaignRun>());
        }
        [UnityTest] public IEnumerator WalkingThroughGateTransitionsOnceWithoutUsingADevice()
        {
            var hub=world.GetComponent<TownHub>();var input=world.Hero.GetComponent<ArenaInput>();input.Desktop.Cancel();
            world.Hero.GetComponent<PlayerMotor>().Teleport(new Vector3(0,.08f,-38.7f),Quaternion.identity);
            input.SetTouchMove(Vector2.down,true);
            float deadline=Time.realtimeSinceStartup+8;
            while(SceneManager.GetActiveScene().name=="RefugioUltimoSino"&&Time.realtimeSinceStartup<deadline)yield return null;
            yield return null;Assert.AreEqual("CostaAto1",SceneManager.GetActiveScene().name);
            Assert.AreEqual(1,Object.FindObjectsByType<RegionalWorld>(FindObjectsSortMode.None).Length);
        }
    }
}

