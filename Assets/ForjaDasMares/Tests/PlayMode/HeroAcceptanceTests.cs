using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ForjaDasMares.Tests
{
    public sealed class HeroAcceptanceTests
    {
        HeroRoster roster; HeroCombat combat; PlayerMotor motor; ArenaSession session;
        [UnitySetUp] public IEnumerator Load()
        {
            SceneManager.LoadScene("ArenaTest"); yield return null; yield return null;
            roster = Object.FindFirstObjectByType<HeroRoster>(); combat = roster.GetComponent<HeroCombat>(); motor = roster.GetComponent<PlayerMotor>(); session = Object.FindFirstObjectByType<ArenaSession>();
            motor.enabled = false; roster.enabled = false; combat.enabled = false; motor.GetComponent<PlayerAim>().enabled = false; motor.GetComponent<ArenaInput>().enabled = false; session.enabled = false;
        }
        void Position(float z = 0) => motor.Teleport(new Vector3(0, 0.06f, z), Quaternion.identity);
        [Test] public void FourProfilesAreCompleteDistinctAndSelectable()
        {
            Assert.That(roster.Count, Is.EqualTo(4));
            int[] hp = { 140, 105, 80, 210 };
            for (int i = 0; i < 4; i++)
            {
                roster.Select(i); Assert.That(roster.Health.Maximum, Is.EqualTo(hp[i])); Assert.That(motor.Speed, Is.EqualTo(roster.Current.speed));
                Assert.That(roster.Current.attacks, Is.Not.Empty); Assert.That(roster.Current.special.hit, Is.Not.Null);
                Assert.That(roster.Current.dodge.invulnerableEnd, Is.LessThanOrEqualTo(roster.Current.dodge.duration));
                foreach (var attack in roster.Current.attacks) { Assert.That(attack.damage, Is.GreaterThan(0)); Assert.That(attack.range, Is.GreaterThan(0)); }
            }
        }
        [Test] public void WarriorComboRecoveryDirectionalArcAndGroupImpactWork()
        {
            Position(); int[] remaining = { 74, 46, 10 };
            for (int i = 0; i < 3; i++) { Assert.That(combat.TryAttack(), Is.True); Assert.That(combat.TryAttack(), Is.False); combat.Simulate(0.12f); Assert.That(session.TrainingTarget.Current, Is.EqualTo(remaining[i])); combat.Simulate(1); }
            session.ResetArena(); Position(3.5f); Assert.That(combat.TryAttack(), Is.True); combat.Simulate(1); Assert.That(session.TrainingTarget.Current, Is.EqualTo(100));
            session.ResetArena(); Position(1.4f); Assert.That(combat.TrySpecial(), Is.True); combat.Simulate(0.12f);
            foreach (var target in Object.FindObjectsByType<TrainingTargetMotion>(FindObjectsSortMode.None)) Assert.That(target.GetComponent<Health>().Current, Is.LessThan(target.GetComponent<Health>().Maximum));
            Assert.That(combat.TrySpecial(), Is.False);
        }
        [UnityTest] public IEnumerator RangerProjectileDamagesAndWallBlocksIt()
        {
            roster.Select(1); Position(-3); Assert.That(combat.TryAttack(), Is.True); combat.Simulate(0.12f); Assert.That(combat.Pool.ActiveProjectiles, Is.EqualTo(1));
            yield return new WaitForSeconds(0.5f); Assert.That(session.TrainingTarget.Current, Is.EqualTo(80));
            session.ResetArena(); Position(-3);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.layer = 9; wall.transform.position = new Vector3(0, 1, 0); wall.transform.localScale = new Vector3(2, 2, 0.3f); Physics.SyncTransforms();
            combat.TryAttack(); combat.Simulate(0.12f); yield return new WaitForSeconds(0.5f); Assert.That(session.TrainingTarget.Current, Is.EqualTo(100)); Assert.That(combat.Pool.ActiveProjectiles, Is.Zero); Object.Destroy(wall);
        }
        [UnityTest] public IEnumerator RangerTrapRootsOnlyOnceAndExpires()
        {
            roster.Select(1); Position(-0.7f); Assert.That(combat.TrySpecial(), Is.True); combat.Simulate(0.12f);
            yield return new WaitForSeconds(0.15f); Assert.That(session.TrainingTarget.Current, Is.EqualTo(72)); Assert.That(session.TrainingTarget.ControlMultiplier, Is.Zero);
            yield return new WaitForSeconds(1.7f); Assert.That(session.TrainingTarget.ControlMultiplier, Is.EqualTo(1)); Assert.That(session.TrainingTarget.Current, Is.EqualTo(72)); Assert.That(combat.Pool.ActiveZones, Is.Zero);
        }
        [UnityTest] public IEnumerator ArcanistElementsAndBurstHaveDifferentEffects()
        {
            roster.Select(2); Position(-3); combat.TryAttack(); combat.Simulate(0.12f); yield return new WaitForSeconds(0.5f);
            Assert.That(session.TrainingTarget.Current, Is.EqualTo(84)); Assert.That(session.TrainingTarget.ControlMultiplier, Is.LessThan(1));
            combat.Simulate(1); combat.TryAttack(); combat.Simulate(0.12f); yield return new WaitForSeconds(0.5f);
            Assert.That(session.TrainingTarget.Current, Is.EqualTo(64));
            foreach (var target in Object.FindObjectsByType<TrainingTargetMotion>(FindObjectsSortMode.None)) Assert.That(target.GetComponent<Health>().Current, Is.LessThan(target.GetComponent<Health>().Maximum));
            combat.Simulate(1); Position(); combat.TrySpecial(); combat.Simulate(0.12f); Assert.That(session.TrainingTarget.ControlMultiplier, Is.Zero);
        }
        [UnityTest] public IEnumerator RuneKnightProtectsAllyAndChargeStopsAtWalls()
        {
            roster.Select(3); Position(-5); Assert.That(combat.TrySpecial(), Is.True); combat.Simulate(0.12f);
            Assert.That(motor.IsCharging, Is.True); Assert.That(motor.IsInvulnerable, Is.False);
            roster.TestDamage(); Assert.That(roster.Ally.Current, Is.EqualTo(134)); Assert.That(roster.Health.Current, Is.EqualTo(196));
            yield return new WaitForSeconds(3.1f); Assert.That(roster.Ally.ProtectionRemaining, Is.Zero);
            session.ResetArena(); motor.Teleport(new Vector3(0, 0.06f, 11), Quaternion.identity); combat.TrySpecial(); combat.Simulate(0.12f); motor.Simulate(0.3f, Vector2.zero);
            Assert.That(motor.transform.position.z, Is.LessThan(11.85f));
        }
        [Test] public void DodgeInvulnerabilityEndsDeathStopsAndResetRestores()
        {
            Position(-5); Assert.That(motor.TryDodge(Vector2.up), Is.True); Assert.That(motor.IsInvulnerable, Is.False);
            motor.Simulate(0.05f, Vector2.zero); Assert.That(motor.IsInvulnerable, Is.True); Assert.That(roster.Health.ApplyDamage(40), Is.False);
            motor.Simulate(0.12f, Vector2.zero); Assert.That(motor.IsInvulnerable, Is.False); Assert.That(roster.Health.ApplyDamage(40), Is.True);
            motor.Simulate(0.1f, Vector2.zero); Assert.That(motor.TryDodge(Vector2.up), Is.False);
            roster.Health.ApplyDamage(10000); Vector3 start = motor.transform.position; motor.Simulate(0.2f, Vector2.up); Assert.That(Vector3.Distance(new Vector3(start.x, 0, start.z), new Vector3(motor.transform.position.x, 0, motor.transform.position.z)), Is.LessThan(0.01f));
            session.ResetArena(); Assert.That(roster.Health.Current, Is.EqualTo(roster.Health.Maximum)); Assert.That(motor.IsInvulnerable, Is.False);
        }
        [UnityTest] public IEnumerator FixedTouchStickHoldsInputAndIgnoresOtherFingerRelease()
        {
            var hud = Object.FindFirstObjectByType<ArenaHud>(); var stick = hud.GetComponentInChildren<TouchMoveStick>(true); stick.transform.parent.gameObject.SetActive(true);
            var reader = motor.GetComponent<ArenaInput>(); reader.enabled = true; yield return null;
            Vector3 basePosition = stick.transform.position; Vector2 center = RectTransformUtility.WorldToScreenPoint(null, basePosition);
            var finger = new PointerEventData(EventSystem.current) { pointerId = 10, position = center };
            stick.OnPointerDown(finger); finger.position += Vector2.right * stick.Radius * hud.GetComponent<Canvas>().scaleFactor; stick.OnDrag(finger);
            Vector3 initial = motor.transform.position; motor.enabled = true;
            yield return new WaitForSeconds(0.2f); Assert.That(reader.Move.x, Is.GreaterThan(0.8f));
            Assert.That(Vector3.Distance(initial, motor.transform.position), Is.GreaterThan(0.5f)); motor.enabled = false;
            stick.OnPointerUp(new PointerEventData(EventSystem.current) { pointerId = 11 }); Assert.That(reader.Move.x, Is.GreaterThan(0.8f)); Assert.That(stick.transform.position, Is.EqualTo(basePosition));
            stick.OnPointerUp(finger); Assert.That(reader.Move, Is.EqualTo(Vector2.zero));
        }
        [Test] public void SwitchingHeroClearsPooledAttacksAndResetsFiniteCooldowns()
        {
            roster.Select(1); Position(-5); combat.TryAttack(); combat.Simulate(0.12f); Assert.That(combat.Pool.ActiveProjectiles, Is.EqualTo(1));
            roster.Select(2); Assert.That(combat.Pool.ActiveProjectiles, Is.Zero); Assert.That(combat.IsBusy, Is.False); Assert.That(combat.SpecialCooldownRemaining, Is.Zero);
            int children = combat.Pool.transform.childCount; for (int i = 0; i < 40; i++) { combat.TryAttack(); combat.Simulate(1); combat.Pool.Clear(); }
            Assert.That(combat.Pool.transform.childCount, Is.EqualTo(children));
        }
        [UnityTest] public IEnumerator ChargeHitsOnceAndKnockbackMovesTrainingTarget()
        {
            roster.Select(3); Position(); combat.TrySpecial(); combat.Simulate(0.12f);
            Assert.That(session.TrainingTarget.Current, Is.EqualTo(76)); combat.Simulate(0.05f);
            Assert.That(session.TrainingTarget.Current, Is.EqualTo(76));
            yield return new WaitForSeconds(0.15f); Assert.That(session.TrainingTarget.transform.position.z, Is.GreaterThan(1.8f));
            session.ResetArena(); Assert.That(session.TrainingTarget.transform.position.z, Is.EqualTo(1.5f));
        }
        [UnityTest] public IEnumerator ProceduralPlaceholderShowsHitDeathAndVictoryStates()
        {
            var visual = roster.GetComponent<HeroPresentation>(); roster.TestDamage(); yield return null;
            Assert.That(visual.State, Is.EqualTo(HeroState.Hit)); roster.Health.ApplyDamage(10000); yield return null;
            Assert.That(visual.State, Is.EqualTo(HeroState.Dead)); session.ResetArena(); roster.Victory(); yield return null;
            Assert.That(visual.State, Is.EqualTo(HeroState.Victory));
        }
        [Test] public void ResetRestartsComboAndSwitchingClearsOldFacingLock()
        {
            Position(); combat.TryAttack(); combat.Simulate(0.12f); Assert.That(session.TrainingTarget.Current, Is.EqualTo(74));
            session.ResetArena(); Position(); combat.TryAttack(); combat.Simulate(0.12f); Assert.That(session.TrainingTarget.Current, Is.EqualTo(74));
            session.ResetArena(); combat.TryAttack(); Assert.That(motor.FacingLocked, Is.True);
            roster.Select(1); Assert.That(motor.FacingLocked, Is.False);
        }
    }
}
