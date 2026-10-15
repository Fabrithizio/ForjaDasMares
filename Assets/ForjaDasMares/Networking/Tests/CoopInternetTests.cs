using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class CoopInternetTests
    {
        [Test] public void PublicConfigurationExistsAndHasSafeDefaults()
        {
            var settings = Resources.Load<CoopInternetSettings>(CoopInternetSettings.ResourceName);
            Assert.IsNotNull(settings); Assert.IsTrue(settings.IsValid);
            var defaults = ScriptableObject.CreateInstance<CoopInternetSettings>();
            try { Assert.AreEqual("production", defaults.Environment); Assert.IsNull(defaults.RelayRegion); Assert.AreEqual(45, defaults.ConnectionTimeoutSeconds); }
            finally { Object.DestroyImmediate(defaults); }
        }
        [Test] public void ResumeOnlyMatchesTheRequestedSessionCode()
        {
            var resume = new CoopSessionResume { sessionId = "opaque-service-session-id", code = "ABC123" };
            Assert.IsTrue(resume.Matches(" abc123 ")); Assert.IsFalse(resume.Matches("OTHER1"));
            Assert.IsFalse(resume.Matches(null)); resume.sessionId = ""; Assert.IsFalse(resume.Matches("ABC123"));
            resume.sessionId = new string('x', 129); Assert.IsFalse(resume.Matches("ABC123"));
        }
        [Test] public void MissingOrFakeProjectCannotEnableInternet()
        {
            Assert.IsFalse(CoopInternetSettings.HasLinkedProject(null));
            Assert.IsFalse(CoopInternetSettings.HasLinkedProject("project"));
            Assert.IsFalse(CoopInternetSettings.HasLinkedProject(System.Guid.Empty.ToString()));
            Assert.IsTrue(CoopInternetSettings.HasLinkedProject("af3e62ee-b725-400a-a64d-65c25f3c9dea"));
        }
        [UnityTest] public IEnumerator UnlinkedInternetAttemptPreservesOfflineSession()
        {
            if (CoopInternetSettings.HasLinkedProject(Application.cloudProjectId)) Assert.Ignore("Requires unlinked project; no live service calls in regression.");
            SceneManager.LoadScene("CoopDungeon"); yield return null; yield return null;
            var director = Object.FindFirstObjectByType<CoopDirector>();
            var cloud = director.GetComponent<CoopCloud>();
            Assert.IsFalse(cloud.Configured); cloud.Create(); yield return null;
            Assert.IsFalse(cloud.Busy); Assert.IsFalse(director.Manager.IsListening);
            Assert.AreEqual(CoopPhase.Offline, director.Phase);
            Assert.That(cloud.Message, Does.Contain("Internet indisponível"));
            director.Close(); SceneManager.LoadScene("ArenaTest"); yield return null; yield return null;
        }
    }
}
