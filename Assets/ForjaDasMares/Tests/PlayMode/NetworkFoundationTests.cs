using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace ForjaDasMares.Tests
{
    public sealed class NetworkFoundationTests
    {
        [Test] public void RejectsNonFiniteAndOverlargeInputs()
        {
            Assert.IsFalse(ValidatedHeroCommands.IsValid(new Vector2(float.NaN, 0), Vector2.zero, HeroButtons.None));
            Assert.IsFalse(ValidatedHeroCommands.IsValid(Vector2.one, Vector2.zero, HeroButtons.Attack));
            Assert.IsFalse(ValidatedHeroCommands.IsValid(Vector2.zero, new Vector2(float.PositiveInfinity, 0), HeroButtons.None));
            Assert.IsFalse(ValidatedHeroCommands.IsValid(Vector2.zero, Vector2.zero, (HeroButtons)128));
            Assert.IsTrue(ValidatedHeroCommands.IsValid(Vector2.one.normalized, Vector2.right, HeroButtons.Attack | HeroButtons.Special));
        }
        [Test] public void DuplicateAndOldSequencesCannotRepeatActions()
        {
            var go = new GameObject("ValidatedCommandsTest");
            try
            {
                var input = go.AddComponent<ValidatedHeroCommands>();
                Assert.IsTrue(input.Accept(10, Vector2.right, Vector2.up, HeroButtons.Attack));
                Assert.IsTrue(input.TakeAttack()); Assert.IsFalse(input.TakeAttack());
                Assert.IsFalse(input.Accept(10, Vector2.right, Vector2.up, HeroButtons.Attack));
                Assert.IsFalse(input.Accept(9, Vector2.right, Vector2.up, HeroButtons.Attack));
                Assert.IsFalse(input.TakeAttack());
                input.Clear(); Assert.IsTrue(input.Accept(uint.MaxValue, Vector2.zero, Vector2.up, HeroButtons.Dodge));
                Assert.IsTrue(input.Accept(0, Vector2.zero, Vector2.up, HeroButtons.Special));
                Assert.IsTrue(input.TakeDodge()); Assert.IsTrue(input.TakeSpecial()); Assert.IsFalse(input.TakeSpecial());
            }
            finally { Object.DestroyImmediate(go); }
        }
        [UnityTest] public IEnumerator LostInputTimesOutToStopMovement()
        {
            var go = new GameObject("TimeoutTest"); var input = go.AddComponent<ValidatedHeroCommands>();
            input.Accept(1, Vector2.right, Vector2.up, HeroButtons.None); Assert.AreEqual(Vector2.right, input.Move);
            yield return new WaitForSecondsRealtime(.4f); Assert.AreEqual(Vector2.zero, input.Move); Object.Destroy(go);
        }
        [Test] public void PartyTargetsLivingNearestMemberAndSurvivesHostDeath()
        {
            var root = new GameObject("PartyTest"); var a = new GameObject("A"); var b = new GameObject("B");
            try
            {
                var party = root.AddComponent<CombatParty>(); var hpA = a.AddComponent<Health>(); var hpB = b.AddComponent<Health>();
                hpA.Configure(10, CombatFaction.Player); hpB.Configure(10, CombatFaction.Player);
                b.transform.position = Vector3.right * 5; party.Set(0, hpA); party.Set(1, hpB);
                Assert.AreSame(hpA, party.Nearest(Vector3.zero)); hpA.ApplyDamage(10);
                Assert.IsTrue(party.AnyAlive); Assert.AreSame(hpB, party.Nearest(Vector3.zero));
                b.SetActive(false); Assert.IsFalse(party.AnyAlive); Assert.IsNull(party.Nearest(Vector3.zero));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(a); Object.DestroyImmediate(b); }
        }
    }
}
