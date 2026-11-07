using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class TownVisualRepairTests
    {
        [UnitySetUp]public IEnumerator Load(){yield return SceneManager.LoadSceneAsync("RefugioUltimoSino");yield return null;yield return null;}
        [Test]public void PavingHasOneSurfacePerCellAndNoDuplicateFaces()
        {
            var mesh=GameObject.Find("CityPaths").GetComponent<MeshFilter>().sharedMesh;var v=mesh.vertices;var t=mesh.triangles;var seen=new HashSet<Vector3>();
            for(int i=0;i<t.Length;i+=3){var centre=(v[t[i]]+v[t[i+1]]+v[t[i+2]])/3;Assert.IsTrue(seen.Add(centre),"Duplicate paving triangle");Assert.Greater(Vector3.Cross(v[t[i+1]]-v[t[i]],v[t[i+2]]-v[t[i]]).y,0);}
            Assert.Greater(seen.Count,100);Assert.IsFalse(GameObject.Find("TownGardenBeds").GetComponent<Renderer>().enabled);
        }
        [Test]public void ProtectorIsFantasyFountainAndGrassHasADefinedTexture()
        {
            Assert.NotNull(GameObject.Find("FonteProtetoraMagica"));Assert.IsNull(GameObject.Find("Protetor_da_praca"));
            var material=GameObject.Find("TownFloor").GetComponent<Renderer>().sharedMaterial;Assert.AreEqual("GrassFantasy032",material.mainTexture.name);
            var benches=Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None).Where(g=>g.name=="Banco_de_madeira");foreach(var bench in benches)Assert.IsTrue(Mathf.Abs(bench.transform.position.x)>25||bench.transform.position.z>26);
        }
    }
}
