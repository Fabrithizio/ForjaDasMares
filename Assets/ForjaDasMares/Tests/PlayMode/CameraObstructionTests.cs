using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ForjaDasMares.Tests
{
    public sealed class CameraObstructionTests
    {
        GameObject rig, target, obstacle;
        ArenaCamera follow;
        float initialPitch, initialYaw;

        [UnitySetUp] public IEnumerator SetUp()
        {
            SceneManager.LoadScene("ArenaTest"); yield return null; yield return null;
            Object.FindFirstObjectByType<ArenaCamera>().enabled = false;
            target = new GameObject("CameraRegressionTarget"); target.transform.position = new Vector3(1000, 0, 0);
            rig = new GameObject("CameraRegressionRig"); rig.SetActive(false);
            rig.AddComponent<Camera>().enabled = false; follow = rig.AddComponent<ArenaCamera>();
            follow.Configure(target.transform, 1 << 9); rig.SetActive(true); yield return null;
            initialPitch = rig.transform.eulerAngles.x; initialYaw = rig.transform.eulerAngles.y;
        }
        [UnityTearDown] public IEnumerator TearDown()
        { Object.Destroy(obstacle); Object.Destroy(rig); Object.Destroy(target); yield return null; }

        void Obstruct()
        {
            obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube); obstacle.layer = 9;
            obstacle.transform.position = target.transform.position + new Vector3(1.27f, 1.6f, -1.27f);
            obstacle.transform.localScale = new Vector3(1.2f, 3.2f, 1.2f); Physics.SyncTransforms();
        }
        void AssertSafe()
        {
            Assert.That(Physics.CheckSphere(rig.transform.position, .19f, 1 << 9), Is.False, "Camera must not enter the blocker.");
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(initialYaw, rig.transform.eulerAngles.y)), Is.LessThan(.05f), "Yaw must stay fixed.");
            Assert.That(rig.transform.eulerAngles.x, Is.LessThanOrEqualTo(75.05f), "Must not switch to a nearly vertical view.");
        }
        [UnityTest] public IEnumerator BoxesNeverChangePitchOrZoom()
        {
            Vector3 focus = target.transform.position + Vector3.up * .9f;
            float distance = Vector3.Distance(rig.transform.position, focus);
            Obstruct();
            for (int i = 0; i < 90; i++)
            {
                obstacle.transform.position = target.transform.position + new Vector3(1.27f + Mathf.Sin(i * .2f), 1.6f, -1.27f);
                Physics.SyncTransforms(); yield return null;
                Assert.That(rig.transform.eulerAngles.x, Is.EqualTo(initialPitch).Within(.01f));
                Assert.That(Vector3.Distance(rig.transform.position, focus), Is.EqualTo(distance).Within(.01f));
                AssertSafe();
            }
            Assert.IsTrue(obstacle.GetComponent<Renderer>().forceRenderingOff, "Occluding box must be hidden locally.");
        }
        [UnityTest] public IEnumerator ClearanceRestoresTheObstacleWithoutMovingTheCamera()
        {
            Obstruct(); yield return null; yield return null;
            var renderer = obstacle.GetComponent<Renderer>(); Assert.IsTrue(renderer.forceRenderingOff);
            Vector3 position = rig.transform.position;
            obstacle.transform.position += Vector3.right * 20f; Physics.SyncTransforms();
            yield return new WaitForSecondsRealtime(.1f); Assert.IsTrue(renderer.forceRenderingOff, "Hold prevents flicker at the edge.");
            yield return new WaitForSecondsRealtime(.3f); Assert.IsFalse(renderer.forceRenderingOff);
            Assert.That(Vector3.Distance(position, rig.transform.position), Is.LessThan(.01f));
        }
        [UnityTest] public IEnumerator HiddenGeometryStillBlocksPhysicsAndRestoresOnDisable()
        {
            Obstruct(); yield return null; yield return null;
            var renderer = obstacle.GetComponent<Renderer>(); Assert.IsTrue(renderer.forceRenderingOff);
            Assert.IsTrue(renderer.enabled); Assert.IsTrue(obstacle.GetComponent<Collider>().enabled);
            Assert.IsTrue(Physics.Linecast(rig.transform.position, target.transform.position + Vector3.up * .9f, 1 << 9));
            follow.enabled = false; Assert.IsFalse(renderer.forceRenderingOff);
        }
        [UnityTest] public IEnumerator DoesNotHideTheFloorOrRevealPreviouslyHiddenGeometry()
        {
            obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube); obstacle.layer = 9;
            obstacle.transform.position = target.transform.position + Vector3.down * .5f;
            obstacle.transform.localScale = new Vector3(30, 1, 30); Physics.SyncTransforms();
            yield return null; yield return null;
            Assert.IsFalse(obstacle.GetComponent<Renderer>().forceRenderingOff, "Floor must remain visible.");
            Object.Destroy(obstacle); yield return null;
            Obstruct(); obstacle.GetComponent<Renderer>().forceRenderingOff = true;
            yield return null; yield return null; follow.enabled = false;
            Assert.IsTrue(obstacle.GetComponent<Renderer>().forceRenderingOff, "Restore original renderer state.");
        }
        [UnityTest] public IEnumerator SnapAtAnObstructedCornerStillFramesTheHero()
        {
            Obstruct(); follow.Snap(); var view = rig.GetComponent<Camera>();
            foreach (float aspect in new[] { 16f / 9f, 20f / 9f, 9f / 16f })
            {
                view.aspect = aspect; follow.Snap(); AssertSafe();
                foreach (float height in new[] { .05f, 1.8f })
                {
                    Vector3 point = view.WorldToViewportPoint(target.transform.position + Vector3.up * height);
                    Assert.That(point.z, Is.GreaterThan(view.nearClipPlane));
                    Assert.That(point.x, Is.InRange(.05f, .95f)); Assert.That(point.y, Is.InRange(.05f, .95f));
                }
            }
            yield return null;
        }
        [UnityTest] public IEnumerator PauseDoesNotInvalidateTheCameraSpring()
        {
            float previousScale = Time.timeScale;
            try
            {
                Time.timeScale = 0f; yield return null; yield return null;
                Assert.IsFalse(float.IsNaN(rig.transform.position.x));
                AssertSafe();
            }
            finally { Time.timeScale = previousScale; }
            yield return null;
            Assert.IsFalse(float.IsNaN(rig.transform.position.x)); AssertSafe();
        }
    }
}
