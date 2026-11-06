using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class CityLifeTests
    {
        [UnitySetUp]public IEnumerator Load(){yield return SceneManager.LoadSceneAsync("RefugioUltimoSino");yield return null;yield return null;}
        [Test]public void MarketHasSixDistinctModelsWithSharedMaterialAndCheaperLods()
        {
            var root=GameObject.Find("CityLife029");Assert.NotNull(root);
            var stalls=root.GetComponentsInChildren<LODGroup>().Where(g=>g.name.StartsWith("Banca_")).ToArray();Assert.AreEqual(6,stalls.Length);
            Material shared=null;
            foreach(var stall in stalls)
            {
                var lods=stall.GetLODs();Assert.AreEqual(2,lods.Length);
                var high=lods[0].renderers[0].GetComponent<MeshFilter>().sharedMesh;var low=lods[1].renderers[0].GetComponent<MeshFilter>().sharedMesh;
                Assert.LessOrEqual(high.GetIndexCount(0)/3,2500);Assert.Less(low.GetIndexCount(0),high.GetIndexCount(0));Assert.AreEqual(1,high.subMeshCount);
                if(shared==null)shared=lods[0].renderers[0].sharedMaterial;Assert.AreEqual(shared,lods[0].renderers[0].sharedMaterial);
                Assert.IsNull(stall.GetComponent<WorldClickTarget>(),"Decorative stall must not advertise a fake shop");
            }
        }
        [Test]public void GateAndDungeonFacadeDoNotBlockTheirOwnOpenings()
        {
            var root=GameObject.Find("CityLife029").transform;
            foreach(string name in new[]{"Portao_da_cidade","Entrada_estacao_invadida"})
            {
                var gate=root.Find(name);Assert.NotNull(gate);
                foreach(float z in new[]{-1.5f,0,1.5f})
                {
                    var point=gate.TransformPoint(new Vector3(0,0,z));Assert.IsFalse(Physics.CheckCapsule(point+Vector3.up*.5f,point+Vector3.up*1.3f,.3f,1<<9,QueryTriggerInteraction.Ignore),name);
                }
            }
        }
        [Test]public void ReservationsHaveUniqueIdsAndDoNotBecomeInvisibleWalls()
        {
            var spaces=Object.FindObjectsByType<TownActivitySpace>(FindObjectsSortMode.None);Assert.AreEqual(3,spaces.Length);Assert.AreEqual(3,spaces.Select(s=>s.SpaceId).Distinct().Count());
            foreach(var space in spaces){Assert.IsEmpty(space.GetComponentsInChildren<Collider>());Assert.Greater(space.Size.x,1);}
            Assert.AreEqual(1,spaces.Count(s=>s.Kind==TownSpaceKind.Event));Assert.AreEqual(1,spaces.Count(s=>s.Kind==TownSpaceKind.Resident));Assert.AreEqual(1,spaces.Count(s=>s.Kind==TownSpaceKind.FutureExpansion));
        }
        [UnityTest]public IEnumerator ExitArrowAppearsOnlyNearThePassage()
        {
            var cue=Object.FindFirstObjectByType<TownExitCue>();Assert.NotNull(cue);
            var world=Object.FindFirstObjectByType<RegionalWorld>();world.Hero.GetComponent<ArenaInput>().Desktop.Cancel();
            Assert.IsFalse(cue.Visible);
            world.Hero.GetComponent<PlayerMotor>().Teleport(new Vector3(0,.08f,-33),Quaternion.identity);yield return null;Assert.IsTrue(cue.Visible);
            world.Hero.GetComponent<PlayerMotor>().Teleport(Vector3.up*.08f,Quaternion.identity);yield return null;Assert.IsFalse(cue.Visible);
        }
        [Test]public void OlderTreesUseLowerGeometryBeforeTakingOverTheScreen()
        {
            var trees=GameObject.Find("TownEnvironment").GetComponentsInChildren<LODGroup>().Where(g=>g.name.StartsWith("Arvore_")).ToArray();Assert.Greater(trees.Length,10);
            foreach(var tree in trees)Assert.GreaterOrEqual(tree.GetLODs()[0].screenRelativeTransitionHeight,.44f);
        }
    }
}
