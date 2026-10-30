using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class TownTests
    {
        RegionalWorld world;TownHub hub;
        [UnitySetUp]public IEnumerator Load(){yield return SceneManager.LoadSceneAsync("RefugioUltimoSino");yield return null;yield return null;world=Object.FindFirstObjectByType<RegionalWorld>();hub=world.GetComponent<TownHub>();Assert.NotNull(hub);}
        [UnityTest]public IEnumerator TownIsSafeAndResidentsActuallyMove()
        {var residents=Object.FindObjectsByType<TownResident>(FindObjectsSortMode.None);Assert.AreEqual(8,residents.Length);var positions=residents.Select(r=>r.transform.position).ToArray();yield return new WaitForSeconds(2);Assert.AreEqual(0,world.Enemies.ActiveCount);Assert.AreEqual(world.Hero.Health.Maximum,world.Hero.Health.Current);Assert.IsTrue(residents.Where((r,i)=>Vector3.Distance(r.transform.position,positions[i])>.4f).Any());}
        [Test]public void InteractionApproachIgnoresVisualScale(){var target=GameObject.Find("RestFountain").GetComponent<WorldClickTarget>();Assert.Less(Vector3.Distance(target.Approach,target.transform.position),3);}
        [Test]public void FourArrivalPointsAreWalkableAndUnobstructed()
        {Assert.AreEqual(4,hub.ArrivalPoints.Length);foreach(var point in hub.ArrivalPoints){Assert.IsTrue(NavMesh.SamplePosition(point,out _,1,NavMesh.AllAreas));Assert.IsFalse(Physics.CheckCapsule(point+Vector3.up*.5f,point+Vector3.up*1.3f,.3f,1<<9));}}
        [Test]public void RestAndMentorPerformRealActions()
        {world.Hero.Health.ApplyDamage(10);hub.Rest();Assert.AreEqual(world.Hero.Health.Maximum,world.Hero.Health.Current);int old=world.Hero.Selected;hub.Mentor();Assert.AreNotEqual(old,world.Hero.Selected);}
        [Test]public void ImportedTentAndBrazierHaveRealSizedCollisions()
        {foreach(string name in new[]{"RefugeTent","CoalBrazier"}){var group=Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None).First(g=>g.name.StartsWith(name));var box=group.GetComponent<BoxCollider>();Assert.NotNull(box);var render=group.GetLODs()[0].renderers[0];Assert.Greater(box.bounds.size.x,render.bounds.size.x*.5f);Assert.Greater(box.bounds.size.z,render.bounds.size.z*.5f);}}
        [UnityTest]public IEnumerator ArrivalIsTraversableWithoutFallingOrEnteringProps()
        {var controls=world.Hero.GetComponent<ArenaInput>();var before=world.Hero.transform.position;controls.SetTouchMove(Vector2.right,true);yield return new WaitForSeconds(.6f);controls.SetTouchMove(Vector2.zero,false);Assert.Greater(Vector3.Distance(before,world.Hero.transform.position),1);Assert.Greater(world.Hero.transform.position.y,-.2f);Assert.Less(world.Hero.transform.position.y,.3f);}
        [UnityTest]public IEnumerator TownGateLoadsExistingCoast()
        {hub.Explore();float until=Time.realtimeSinceStartup+10;while(SceneManager.GetActiveScene().name!="CostaAto1"&&Time.realtimeSinceStartup<until)yield return null;yield return null;Assert.AreEqual("CostaAto1",SceneManager.GetActiveScene().name);Assert.IsNotNull(Object.FindFirstObjectByType<RegionalJourney>());}
    }
}
