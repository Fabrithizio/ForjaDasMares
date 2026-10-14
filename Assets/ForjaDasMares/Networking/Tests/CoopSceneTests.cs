using System.Collections;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class CoopSceneTests
    {
        CoopDirector session;
        [UnitySetUp] public IEnumerator Load()
        { SceneManager.LoadScene("CoopDungeon"); yield return null; yield return null; session = Object.FindFirstObjectByType<CoopDirector>(); Assert.IsNotNull(session); }
        [UnityTearDown] public IEnumerator Close()
        { if (session != null) session.Close(); yield return null; yield return null; }
        [Test] public void EveryHeroHasOneConfiguredCommandAdapterAndNoClientDamageSimulation()
        {
            Assert.AreSame(session.Manager, NetworkManager.Singleton);
            for (int i = 0; i < 4; i++)
            {
                var actor = session.Actor(i); Assert.IsNotNull(actor);
                Assert.AreEqual(1, actor.GetComponents<CoopActor>().Length);
                Assert.AreEqual(1, actor.GetComponents<ValidatedHeroCommands>().Length);
                Assert.AreSame(actor.Commands, actor.Motor.Commands);
                Assert.IsFalse(actor.Motor.enabled); Assert.IsFalse(actor.Combat.enabled); Assert.IsFalse(actor.GetComponent<CharacterController>().enabled);
            }
            Assert.IsTrue(session.Manager.NetworkConfig.ConnectionApproval); Assert.IsFalse(session.Manager.NetworkConfig.EnableSceneManagement);
        }
        [UnityTest] public IEnumerator HostLoadsRealDungeonAndDetectsDefeat()
        {
            Assert.IsTrue(session.HostDirect("LOCALTEST", 7789, false)); Assert.AreEqual(1, session.ConnectedCount);
            session.StartGame(); yield return null; yield return new WaitForSeconds(.2f);
            Assert.AreEqual(CoopPhase.Playing, session.Phase); Assert.IsTrue(session.Dungeon.Generator.NavigationConnected);
            Assert.IsTrue(session.Actor(0).Motor.enabled); session.Actor(0).Health.ApplyDamage(100000); yield return null; yield return null;
            Assert.AreEqual(CoopPhase.Defeat, session.Phase); Assert.IsFalse(session.Actor(0).Combat.enabled);
        }
        [UnityTest] public IEnumerator NetworkRootDoesNotLeakIntoOfflineScene()
        {
            Assert.AreEqual(SceneManager.GetActiveScene(), session.gameObject.scene);
            SceneManager.LoadScene("ArenaTest"); yield return null; yield return null;
            Assert.IsNull(Object.FindFirstObjectByType<CoopDirector>()); Assert.IsNull(NetworkManager.Singleton);
        }
    }
}
