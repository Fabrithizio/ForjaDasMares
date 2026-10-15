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
        [UnityTest] public IEnumerator ObstructionChangesElevationAtABoundedRate()
        {
            Obstruct(); float previous = rig.transform.eulerAngles.x, elapsed = 0f;
            while (elapsed < 1.7f)
            {
                float dt = Time.deltaTime; yield return null; elapsed += dt;
                float next = rig.transform.eulerAngles.x;
                Assert.That(Mathf.Abs(Mathf.DeltaAngle(previous, next)), Is.LessThanOrEqualTo(22f * dt + .06f), "Obstruction caused an abrupt tilt.");
                AssertSafe(); previous = next;
            }
            Assert.That(previous, Is.GreaterThan(initialPitch + 3f), "Fixture must actually trigger obstruction recovery.");
        }
        [UnityTest] public IEnumerator ClearanceReturnsGraduallyAndRestoresTheNormalView()
        {
            Obstruct(); yield return new WaitForSeconds(1.8f);
            float elevated = rig.transform.eulerAngles.x; Assert.That(elevated, Is.GreaterThan(initialPitch + 3f));
            obstacle.SetActive(false); Physics.SyncTransforms();
            float previous = elevated, elapsed = 0f;
            while (elapsed < 4.5f)
            {
                float dt = Time.deltaTime; yield return null; elapsed += dt;
                float next = rig.transform.eulerAngles.x;
                Assert.That(Mathf.Abs(Mathf.DeltaAngle(previous, next)), Is.LessThanOrEqualTo(12f * dt + .06f));
                if (elapsed < .2f) Assert.That(next, Is.EqualTo(elevated).Within(.1f), "Brief clearance must not bounce the camera back.");
                AssertSafe(); previous = next;
            }
            Assert.That(previous, Is.EqualTo(initialPitch).Within(.15f));
            Assert.That(Vector3.Distance(rig.transform.position, target.transform.position + Vector3.up * .9f), Is.EqualTo(new Vector3(9, 13, -9).magnitude).Within(.1f));
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
