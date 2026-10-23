using NUnit.Framework;
using UnityEngine;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class AimTargetSelectorTests
    {
        GameObject a,b,wall;
        AimTargetSelector selector;
        Vector3 origin=new Vector3(100,0,100);
        [SetUp]public void SetUp()
        {
            selector=new AimTargetSelector();
            a=new GameObject("AimTarget_A");a.transform.position=origin+Vector3.forward*6;a.AddComponent<Health>().Configure(100,CombatFaction.Enemy);
            b=new GameObject("AimTarget_B");b.transform.position=origin+Vector3.right*6.2f;b.AddComponent<Health>().Configure(100,CombatFaction.Enemy);
        }
        [TearDown]public void TearDown(){Object.DestroyImmediate(a);Object.DestroyImmediate(b);if(wall!=null)Object.DestroyImmediate(wall);}
        Health Scan()
        {selector.BeginScan(origin,10,1<<9);selector.Consider(a.GetComponent<Health>());selector.Consider(b.GetComponent<Health>());return selector.EndScan();}
        [Test]public void RetainsTargetAcrossSmallDistanceChangesAndSwitchesForClearAdvantage()
        {
            Assert.AreEqual(a.GetComponent<Health>(),Scan());
            b.transform.position=origin+Vector3.right*5.8f;Assert.AreEqual(a.GetComponent<Health>(),Scan());
            b.transform.position=origin+Vector3.right*3;Assert.AreEqual(b.GetComponent<Health>(),Scan());
        }
        [Test]public void DeadInactiveAndOutOfRangeTargetsAreReleased()
        {
            Assert.AreEqual(a.GetComponent<Health>(),Scan());a.GetComponent<Health>().ApplyDamage(1000);
            Assert.AreEqual(b.GetComponent<Health>(),Scan());b.SetActive(false);Assert.IsNull(Scan());
            b.SetActive(true);b.transform.position=origin+Vector3.right*20;Assert.IsNull(Scan());
        }
        [Test]public void WallAndMissingCandidateReleaseThePreviousTarget()
        {
            Assert.AreEqual(a.GetComponent<Health>(),Scan());
            wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=9;wall.transform.position=origin+new Vector3(0,1,3);wall.transform.localScale=new Vector3(3,3,.3f);Physics.SyncTransforms();
            Assert.AreEqual(b.GetComponent<Health>(),Scan());
            selector.BeginScan(origin,10,1<<9);Assert.IsNull(selector.EndScan());
        }
    }
}
