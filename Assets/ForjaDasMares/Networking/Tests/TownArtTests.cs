using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ForjaDasMares.Networking.Tests
{
    public sealed class TownArtTests
    {
        RegionalWorld world;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("RefugioUltimoSino");
            yield return null; yield return null;
            world = Object.FindFirstObjectByType<RegionalWorld>();
        }
        [Test] public void BuildingsAndVegetationAreImportedMeshesWithCheaperLods()
        {
            var art = GameObject.Find("TownArt027"); Assert.NotNull(art);
            var groups = art.GetComponentsInChildren<LODGroup>();
            Assert.Greater(groups.Length, 50); Assert.Less(groups.Length, 130);
            Assert.AreEqual(3, groups.Count(g => g.name.StartsWith("Casa_")));
            Assert.IsNull(GameObject.Find("GeometricShrub"));
            foreach (var group in groups)
            {
                var lods = group.GetLODs(); Assert.AreEqual(2, lods.Length);
                var detailed = lods[0].renderers[0].GetComponent<MeshFilter>().sharedMesh;
                var distant = lods[1].renderers[0].GetComponent<MeshFilter>().sharedMesh;
                Assert.Greater(detailed.vertexCount, 0);
                Assert.LessOrEqual(distant.GetIndexCount(0), detailed.GetIndexCount(0));
                Assert.AreEqual(1, detailed.subMeshCount);
                Assert.NotNull(lods[0].renderers[0].sharedMaterial.GetTexture("_BaseMap"));
            }
        }
        [Test] public void NewArtDoesNotDisconnectServicesOrTownExit()
        {
            Assert.IsTrue(NavMesh.SamplePosition(world.Definition.refuge, out var start, 1, NavMesh.AllAreas));
            foreach (var target in Object.FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None))
            {
                Assert.IsTrue(NavMesh.SamplePosition(target.Approach, out var end, 2.5f, NavMesh.AllAreas), target.name);
                var path = new NavMeshPath();
                Assert.IsTrue(NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path));
                Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status, target.name);
            }
            foreach (var tree in GameObject.Find("TownArt027").GetComponentsInChildren<LODGroup>().Where(g => g.name.StartsWith("Arvore")))
                Assert.Less(tree.GetComponent<BoxCollider>().size.x, 1, "Foliage must not block the entire crown");
        }
        [UnityTest] public IEnumerator FountainHealsAndHasVisibleFeedbackThatReturnsToRest()
        {
            var fountain = GameObject.Find("RestFountain");
            var effect = fountain.GetComponent<WorldInteractionFeedback>();
            var visual = fountain.transform.Find("Fonte_de_cristal");
            Assert.NotNull(effect); Assert.NotNull(visual);
            Vector3 rest = visual.localScale;
            world.Hero.GetComponent<ArenaInput>().Desktop.Cancel();
            world.Hero.GetComponent<PlayerMotor>().Teleport(fountain.transform.position + Vector3.back * 2, Quaternion.identity);
            world.Hero.Health.ApplyDamage(10);
            Assert.IsTrue(fountain.GetComponent<WorldClickTarget>().TryUse(world.Hero.transform));
            Assert.AreEqual(world.Hero.Health.Maximum, world.Hero.Health.Current);
            yield return new WaitForSeconds(.2f);
            Assert.IsTrue(effect.IsPlaying); Assert.Greater(visual.localScale.x, rest.x);
            yield return new WaitForSeconds(1);
            Assert.IsFalse(effect.IsPlaying); Assert.Less(Vector3.Distance(rest, visual.localScale), .001f);
            effect.enabled = false; effect.enabled = true;
            int previous = effect.PlayCount;
            fountain.GetComponent<WorldClickTarget>().TryUse(world.Hero.transform);
            Assert.AreEqual(previous + 1, effect.PlayCount, "No duplicated event subscriptions after enabling");
        }
    }
}
