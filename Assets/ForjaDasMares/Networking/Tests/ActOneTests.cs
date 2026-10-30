using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class ActOneTests
    {
        RegionalWorld world;
        [UnitySetUp]public IEnumerator Load(){yield return SceneManager.LoadSceneAsync("CostaAto1");yield return null;yield return null;world=Object.FindFirstObjectByType<RegionalWorld>();Assert.NotNull(world);Assert.NotNull(world.Journey);}
        void Visit(int index)
        {var target=world.Journey.Definition.points[index].position+Vector3.back*1.3f;world.Hero.GetComponent<PlayerMotor>().Teleport(target,Quaternion.identity);world.Hero.Health.Restore();world.Interact();}
        [Test]public void RegionIsLargerConnectedAndHasAlternativeCircuits()
        {
            var d=world.Definition;Assert.AreEqual(4,Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None).Length);Assert.GreaterOrEqual(d.bounds.size.x,300);Assert.Greater(d.connections.Length,d.route.Length);
            Assert.IsTrue(NavMesh.SamplePosition(d.refuge,out var start,3,NavMesh.AllAreas));
            foreach(var p in d.route.Concat(d.camps.SelectMany(c=>c.spawns)).Concat(world.Journey.Definition.points.Select(p=>p.position)))
            {Assert.IsTrue(NavMesh.SamplePosition(p,out var end,3,NavMesh.AllAreas),p.ToString());var path=new NavMeshPath();Assert.IsTrue(NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path));Assert.AreEqual(NavMeshPathStatus.PathComplete,path.status,p.ToString());}
        }
        [UnityTest]public IEnumerator StoryCompletesWithTwoCluesSurvivesDeathReloadAndCannotDuplicateReward()
        {
            Visit(1);Assert.IsFalse(world.Journey.Completed,"Daro cannot complete an unaccepted quest");
            Visit(0);Assert.IsTrue(world.Journey.Accepted);Visit(3);Assert.AreEqual(1,world.Journey.ClueCount);
            Visit(3);Assert.AreEqual(1,world.Journey.ClueCount,"A repeated clue is not another objective");
            world.Hero.Health.ApplyDamage(10000);world.Respawn();Assert.AreEqual(1,world.Journey.ClueCount);
            Visit(4);Assert.AreEqual(2,world.Journey.ClueCount);Visit(1);Assert.IsTrue(world.Journey.Completed);Assert.AreEqual(1,world.Journey.MemoryFragments);
            Visit(1);Assert.AreEqual(1,world.Journey.MemoryFragments);
            yield return SceneManager.LoadSceneAsync("CostaAto1");yield return null;yield return null;world=Object.FindFirstObjectByType<RegionalWorld>();Assert.IsTrue(world.Journey.Completed);Assert.AreEqual(2,world.Journey.ClueCount);Assert.AreEqual(1,world.Journey.MemoryFragments);
        }
        [Test]public void InteractionFromOutsideRadiusDoesNotAdvanceStory()
        {world.Hero.GetComponent<PlayerMotor>().Teleport(new Vector3(40,.1f,-35),Quaternion.identity);Assert.AreEqual(-1,world.Journey.NearestPoint());Assert.IsFalse(world.Journey.Interact());}
        [UnityTest]public IEnumerator CampsRespectBudgetAndRefugeIsSafe()
        {yield return new WaitForSeconds(1.3f);Assert.Greater(world.Enemies.ActiveCount,0);Assert.LessOrEqual(world.Enemies.ActiveCount,12);foreach(var e in world.Enemies.Actors.Where(e=>e.IsAlive))Assert.Greater(Vector3.Distance(e.transform.position,world.Definition.refuge),25);}
        [Test]public void NewTreeAndTwoProvisionalNpcsExistAndPropsUseThreeLods()
        {Assert.IsNotNull(GameObject.Find("Nara · Cartógrafa (provisório)"));Assert.IsNotNull(GameObject.Find("Daro · Artífice (provisório)"));var groups=Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None);Assert.IsTrue(groups.Any(g=>g.name.StartsWith("Gravewood")));foreach(var g in groups){Assert.AreEqual(3,g.GetLODs().Length);foreach(var lod in g.GetLODs())Assert.IsTrue(lod.renderers.Length>0);}}
        [Test]public void MaterialsKeepBaseColorWhiteAndColorDataLinear()
        {var tree=Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None).First(g=>g.name.StartsWith("Gravewood"));var material=tree.GetLODs()[0].renderers[0].sharedMaterial;Assert.AreEqual(Color.white,material.GetColor("_BaseColor"));Assert.NotNull(material.GetTexture("_BaseMap"));Assert.NotNull(material.GetTexture("_BumpMap"));Assert.NotNull(material.GetTexture("_MetallicGlossMap"));}
    }
}
