using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Tests
{
    public sealed class ContinuousCombatTests
    {
        HeroRoster heroes; HeroCombat combat; PlayerMotor motor; ProgressionSceneBridge bridge; GameObject target;
        [UnitySetUp] public IEnumerator Load()
        {
            SceneManager.LoadScene("GaleriasArena");yield return null;yield return null;yield return null;
            heroes=Object.FindFirstObjectByType<HeroRoster>();combat=heroes.GetComponent<HeroCombat>();motor=heroes.GetComponent<PlayerMotor>();bridge=Object.FindFirstObjectByType<ProgressionSceneBridge>();
            motor.enabled=heroes.enabled=false;heroes.GetComponent<PlayerAim>().enabled=false;heroes.GetComponent<ArenaInput>().enabled=false;
            motor.Teleport(new Vector3(0,.06f,-5),Quaternion.identity);combat.ApplyHero(heroes.Current);
            target=GameObject.CreatePrimitive(PrimitiveType.Capsule);target.name="ChargedTarget_QA";target.layer=10;target.transform.position=motor.transform.position+Vector3.forward*1.5f+Vector3.up;
            target.AddComponent<Health>().Configure(10000,CombatFaction.Enemy);Physics.SyncTransforms();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Object.Destroy(target);bridge.Abort();var profile=Object.FindFirstObjectByType<ProgressionProfileRuntime>();
            SceneManager.LoadScene("ArenaTest");if(profile!=null)Object.Destroy(profile.gameObject);yield return null;
        }
        [Test] public void HoldingRepeatsWithoutClickingAndTriggersHeavyWithCooldown()
        {
            int versions=0,heavies=0,last=combat.ActionVersion;bool wasHeavy=false;
            for(int i=0;i<200;i++)
            {
                combat.ProcessAttackInput(true,i==0,false,.02f);combat.Simulate(.02f);
                if(combat.ActionVersion!=last){versions++;last=combat.ActionVersion;if(combat.IsHeavy)heavies++;}
                wasHeavy|=combat.IsHeavy;
            }
            Assert.That(versions,Is.GreaterThan(5));Assert.IsTrue(wasHeavy);Assert.That(heavies,Is.EqualTo(1));
            combat.ProcessAttackInput(false,false,false,0);combat.Simulate(2);last=combat.ActionVersion;
            combat.ProcessAttackInput(false,false,false,1);Assert.That(combat.ActionVersion,Is.EqualTo(last));Assert.That(combat.ChargeProgress,Is.Zero);
        }
        [Test] public void HeavyHitsOnceAndDoesMoreDamageThanBasic()
        {
            var hp=target.GetComponent<Health>();combat.TryAttack();combat.Simulate(.12f);int normal=10000-hp.Current;combat.Simulate(1);hp.Restore();
            Assert.IsTrue(combat.TryHeavy());combat.Simulate(.39f);Assert.That(hp.Current,Is.EqualTo(10000));
            combat.Simulate(.02f);int heavy=10000-hp.Current;Assert.That(heavy,Is.GreaterThan(normal*2));
            combat.Simulate(1);Assert.That(10000-hp.Current,Is.EqualTo(heavy));Assert.IsFalse(combat.TryHeavy());
        }
        [Test] public void SuperRequiresRealEnemyHitsCannotDoubleCountCollidersOrRefundItself()
        {
            target.AddComponent<SphereCollider>();Physics.SyncTransforms();Assert.IsFalse(combat.TrySuper());
            for(int i=0;i<16;i++){Assert.IsTrue(combat.TryAttack());combat.Simulate(1);Assert.That(combat.SuperProgress,Is.EqualTo((i+1)/16f).Within(.001f));}
            Assert.IsTrue(combat.SuperReady);int before=target.GetComponent<Health>().Current;
            Assert.IsTrue(combat.TrySuper());Assert.That(combat.SuperProgress,Is.Zero);Assert.IsFalse(combat.TrySuper());
            combat.Simulate(.46f);int dealt=before-target.GetComponent<Health>().Current;Assert.That(dealt,Is.EqualTo(120));Assert.That(combat.SuperProgress,Is.Zero);
            combat.Simulate(2);Assert.That(before-target.GetComponent<Health>().Current,Is.EqualTo(dealt));Assert.IsFalse(combat.TrySuper());
        }
        [Test] public void WallsBlockHeavyAndSuperAndDeathStopsHeldAttacks()
        {
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=9;wall.transform.position=motor.transform.position+Vector3.forward*.7f+Vector3.up;wall.transform.localScale=new Vector3(3,3,.2f);Physics.SyncTransforms();
            combat.TryHeavy();combat.Simulate(1);Assert.That(target.GetComponent<Health>().Current,Is.EqualTo(10000));
            Object.DestroyImmediate(wall);Physics.SyncTransforms();
            for(int i=0;i<16;i++){combat.TryAttack();combat.Simulate(1);}Assert.IsTrue(combat.SuperReady);
            wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=9;wall.transform.position=motor.transform.position+Vector3.forward*.7f+Vector3.up;wall.transform.localScale=new Vector3(3,3,.2f);Physics.SyncTransforms();
            int before=target.GetComponent<Health>().Current;combat.TrySuper();combat.Simulate(2);Assert.That(target.GetComponent<Health>().Current,Is.EqualTo(before));Object.DestroyImmediate(wall);
            heroes.Health.ApplyDamage(100000);int last=combat.ActionVersion;combat.ProcessAttackInput(true,true,true,5);combat.Simulate(5);Assert.That(combat.ActionVersion,Is.EqualTo(last));
            combat.ResetCombat();Assert.That(combat.SuperProgress,Is.Zero);
        }
        [Test] public void TouchHoldOwnsItsFingerAndBlockingClearsTheIntent()
        {
            var input=heroes.GetComponent<ArenaInput>();var button=new GameObject("Hold_QA").AddComponent<HoldAttackButton>();button.Configure(input);
            button.OnPointerDown(new PointerEventData(EventSystem.current){pointerId=5});Assert.IsTrue(input.AttackHeld);
            button.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=8});Assert.IsTrue(input.AttackHeld);
            input.BlockCommands(true);Assert.IsFalse(input.AttackHeld);input.BlockCommands(false);Assert.IsFalse(input.AttackHeld);
            button.gameObject.SetActive(false);Assert.IsFalse(input.AttackHeld);Object.DestroyImmediate(button.gameObject);
        }
        [UnityTest] public IEnumerator ArenaSuperButtonReflectsTheMeterAndStartsTheActualAttack()
        {
            var hud=Object.FindFirstObjectByType<HeroHud>();var button=hud.transform.Find("ArenaPowerCard/SuperAttack").GetComponent<UnityEngine.UI.Button>();
            Assert.IsFalse(button.interactable);
            for(int i=0;i<16;i++){combat.TryAttack();combat.Simulate(1);}
            yield return null;Assert.IsTrue(button.interactable);button.onClick.Invoke();yield return null;
            Assert.IsTrue(combat.IsSuper);Assert.That(combat.SuperProgress,Is.Zero);
        }
        [Test] public void SuperPressedDuringHeldComboWaitsForRecoveryAndFiresOnlyOnce()
        {
            for(int i=0;i<16;i++){combat.TryAttack();combat.Simulate(1);}
            combat.TryAttack();int version=combat.ActionVersion;
            combat.ProcessAttackInput(true,false,true,.02f);Assert.IsTrue(combat.SuperQueued);Assert.IsFalse(combat.IsSuper);
            combat.Simulate(1);combat.ProcessAttackInput(true,false,false,.02f);Assert.IsTrue(combat.IsSuper);Assert.IsFalse(combat.SuperQueued);
            Assert.That(combat.ActionVersion,Is.EqualTo(version+1));Assert.That(combat.SuperProgress,Is.Zero);
            combat.ProcessAttackInput(true,false,true,.02f);Assert.That(combat.ActionVersion,Is.EqualTo(version+1));
        }
        [UnityTest] public IEnumerator SuperHasBoundedRealVisualRibbonsWithoutExtraColliders()
        {
            for(int i=0;i<16;i++){combat.TryAttack();combat.Simulate(1);}
            combat.TrySuper();combat.Simulate(.46f);yield return null;
            int visible=0;foreach(var line in heroes.GetComponentsInChildren<LineRenderer>())
                if(line.name.StartsWith("CombatRibbon_")){if(line.enabled)visible++;Assert.NotNull(line.sharedMaterial);Assert.IsNull(line.GetComponent<Collider>());}
            Assert.That(visible,Is.EqualTo(10));
        }
        [UnityTest] public IEnumerator ProjectileHeroesKeepTheirProjectilesWithoutHugeMeleeRibbons()
        {
            heroes.Select(1);combat.TryAttack();combat.Simulate(.12f);yield return null;
            Assert.That(combat.Pool.ActiveProjectiles,Is.GreaterThan(0));
            foreach(var line in heroes.GetComponentsInChildren<LineRenderer>())if(line.name.StartsWith("CombatRibbon_"))Assert.IsFalse(line.enabled);
        }
    }
}
