using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class PrismaTests
    {
        CoopDirector session;ProgressionCatalog catalog;
        CoopActor Mage=>session.Actor(0);
        static StatSnapshot Plain=>new StatSnapshot{damageMultiplier=1,attackSpeed=1,cooldownMultiplier=1,criticalMultiplier=1.5f};
        [UnitySetUp]public IEnumerator Load()
        {
            SceneManager.LoadScene("CampaignCoop");yield return null;yield return null;
            session=Object.FindFirstObjectByType<CoopDirector>();catalog=session.GetComponent<CoopProgression>().Catalog;
            session.SelectHero(2);yield return null;
        }
        [UnityTearDown]public IEnumerator Close(){session.Close();yield return null;yield return null;}
        [Test]public void SingleAuthoredMageAndNoLegacyBodyOnAllFourActors()
        {
            for(int slot=0;slot<4;slot++)
            {
                var actor=session.Actor(slot);actor.Roster.Select(2);var root=actor.Presentation.Visible;
                Assert.AreEqual(1,root.GetComponentsInChildren<HeroModelView>(true).Length);
                foreach(string name in new[]{"Body_Placeholder","Head_Placeholder","FacingMarker","Staff","Focus"})
                {var old=root.Find(name);if(old!=null)Assert.IsFalse(old.gameObject.activeSelf,name);}
                var skin=root.GetComponentsInChildren<SkinnedMeshRenderer>(true);Assert.GreaterOrEqual(skin.Length,1);
                Assert.IsTrue(skin.All(x=>x.sharedMesh!=null&&x.bones.Length>=1));
                Assert.IsFalse(root.GetComponentInChildren<HeroModelView>(true).Animator.applyRootMotion);
            }
        }
        [Test]public void TwentyFiveRealClipsAndMaskedLocomotionArePresent()
        {
            var view=Mage.Presentation.Visible.GetComponentInChildren<HeroModelView>();Assert.NotNull(view);if(!view.AuthoredAnimationsReady)Assert.Ignore("Animações da nova personagem adiadas pelo usuário.");
            string[] names={"Idle","Walk","Run","Attack","Attack2","Attack3","Charged","Skill","Super","Dodge","Hit","Dead","Victory","StaffBolt","StaffReturn","StaffRelease","HeavyStaff","BookWord","BookPage","BookSeal","HeavyBook","OrbCast","OrbReturn","OrbBurst","HeavyOrb"};
            foreach(string name in names)Assert.IsTrue(view.Animator.runtimeAnimatorController.animationClips.Any(c=>c.name==name&&c.length>.1f),name);
            Assert.AreEqual(2,view.Animator.layerCount);Assert.NotNull(view.GetComponent<BrasaFootPlant>());
        }
        [Test]public void AuthoredStaffAndBookHaveDistinctHandPosesAndBoundedEffects()
        {
            Mage.Presentation.enabled=false;var view=Mage.Presentation.Visible.GetComponentInChildren<HeroModelView>();Assert.IsTrue(view.AuthoredAnimationsReady);
            view.SetWeaponCategory(WeaponCategory.Staff);view.Animate(HeroState.Attack,.30f,0,true,0,91,.3f);view.Animator.Update(.3f);
            Vector3 staff=view.SecondarySocket.position;
            view.SetWeaponCategory(WeaponCategory.Grimoire);view.Animate(HeroState.Attack,.30f,0,true,0,92,.3f);view.Animator.Update(.3f);
            Assert.Greater(Vector3.Distance(staff,view.SecondarySocket.position),.025f,"Book must use a different free-hand gesture.");
            var fx=Mage.GetComponent<HeroWeaponEffects>();Assert.NotNull(fx);Assert.AreEqual(0,fx.RibbonCapacity);Assert.LessOrEqual(fx.LiveParticles,64);
            foreach(var line in Mage.GetComponentsInChildren<LineRenderer>(true).Where(x=>x.name.StartsWith("HeroEnergy_")))Assert.IsNull(line.GetComponent<Collider>());
        }
        [UnityTest]public IEnumerator EnergyReleaseUsesTimelineAndCleansWithoutChangingHealth()
        {
            Mage.Presentation.enabled=false;Mage.Presentation.ReplicaMode=true;int hp=Mage.Health.Current;
            Mage.Presentation.ApplyReplica(HeroState.Attack,.8f,0,999,.3f);yield return null;yield return null;
            var fx=Mage.GetComponent<HeroWeaponEffects>();Assert.Greater(fx.LiveParticles,0);
            var lines=Mage.GetComponentsInChildren<LineRenderer>(true).Where(x=>x.name.StartsWith("HeroEnergy_")).ToArray();
            Assert.IsEmpty(lines);Assert.AreEqual(hp,Mage.Health.Current);
            fx.enabled=false;Assert.AreEqual(0,fx.LiveParticles);Assert.IsTrue(lines.All(x=>!x.enabled));
        }
        [Test]public void ThreeWeaponStylesSelectCastingAndHeavyClips()
        {
            Mage.Presentation.enabled=false;var view=Mage.Presentation.Visible.GetComponentInChildren<HeroModelView>();if(!view.AuthoredAnimationsReady)Assert.Ignore("Animações da nova personagem adiadas pelo usuário.");int version=0;
            var kinds=new[]{WeaponCategory.Staff,WeaponCategory.Grimoire,WeaponCategory.MagicWeapon};
            string[] prefixes={"StaffBolt","BookWord","OrbCast"},heavies={"HeavyStaff","HeavyBook","HeavyOrb"};
            for(int i=0;i<kinds.Length;i++)
            {
                view.SetWeaponCategory(kinds[i]);view.Animate(HeroState.Attack,.4f,0,true,0,++version,.3f);view.Animator.Update(.2f);
                Assert.IsTrue(view.Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer."+prefixes[i]),prefixes[i]);
                view.Animate(HeroState.Attack,.5f,0,true,3,++version,.5f);view.Animator.Update(.2f);
                Assert.IsTrue(view.Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer."+heavies[i]),heavies[i]);
                view.Animate(HeroState.Skill,.5f,0,true,0,++version,.5f);view.Animator.Update(.2f);
                Assert.IsTrue(view.Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Skill"));
                view.Animate(HeroState.Attack,.4f,5.5f,true,0,++version,.3f);view.Animator.Update(.2f);
                Assert.IsTrue(view.Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Run"));
                Assert.IsTrue(view.Animator.GetCurrentAnimatorStateInfo(1).IsName("Upper Body."+prefixes[i]));
            }
        }
        [Test]public void WeakStarterAndWeaponSpecificBuildsStayDistinct()
        {
            var hero=Mage.Roster.Current;var starter=catalog.equipment.Single(x=>x.starter&&x.compatibleHeroes==HeroAffinity.Arcanist);
            Assert.AreEqual(72,hero.maximumHealth);Assert.AreEqual(9,starter.attacks[0].damage);Assert.AreEqual(1,starter.attacks[0].maximumTargets);Assert.AreEqual(0,starter.attacks[0].splashRadius);
            var staff=Weapon(WeaponCategory.Staff);var book=Weapon(WeaponCategory.Grimoire);var orb=Weapon(WeaponCategory.MagicWeapon);
            Assert.AreEqual(ControlEffect.Slow,staff.attacks[0].control);Assert.AreEqual(AbilityEffect.Trap,book.ability.effect);
            Assert.Greater(book.attacks[2].splashRadius,staff.attacks[0].splashRadius);Assert.AreEqual(3,orb.attacks[2].projectileCount);
            Assert.AreEqual(ControlEffect.Root,orb.attacks[2].control);Assert.AreNotEqual(staff.attacks[0].color,book.attacks[0].color);
            foreach(var item in new[]{staff,book,orb}){Assert.NotNull(item.icon);Assert.NotNull(item.visualMesh);Assert.AreEqual(HeroAffinity.Arcanist,item.compatibleHeroes);}
        }
        [UnityTest]public IEnumerator GlyphsAreBoundedVisualOnlyAndDisappearWithHero()
        {
            yield return null;var view=Mage.Presentation.Visible.GetComponentInChildren<PrismaCastingView>();Assert.NotNull(view);Assert.AreEqual(0,view.EffectCapacity);
            Assert.AreEqual(0,view.GetComponentsInChildren<LineRenderer>(true).Count(x=>x.name.StartsWith("PrismaGlyph_")));
            Assert.IsNull(view.GetComponentInChildren<Collider>(true));Mage.Roster.Select(0);yield return null;
            Assert.IsTrue(view.GetComponentsInChildren<LineRenderer>(true).All(x=>!x.enabled));
        }
        [UnityTest]public IEnumerator ElementalProjectilesHitOnceEvenWithTwoCollidersAndSplash()
        {
            var pool=Object.FindFirstObjectByType<CombatEffectPool>();var target=GameObject.CreatePrimitive(PrimitiveType.Capsule);target.layer=10;target.transform.position=new Vector3(100,1,103);target.AddComponent<SphereCollider>();var hp=target.AddComponent<Health>();
            try
            {
                foreach(var kind in new[]{WeaponCategory.Staff,WeaponCategory.Grimoire,WeaponCategory.MagicWeapon})
                {
                    hp.Configure(1000,CombatFaction.Enemy);var hit=Weapon(kind).attacks[0];Physics.SyncTransforms();
                    Assert.IsTrue(pool.Shoot(new Vector3(100,1,100),Vector3.forward,hit,Mage.Health,1<<10,1<<9));
                    float deadline=Time.realtimeSinceStartup+1;while(hp.Current==1000&&Time.realtimeSinceStartup<deadline)yield return null;
                    Assert.AreEqual(1000-hit.damage,hp.Current,kind.ToString());yield return new WaitForSeconds(.15f);Assert.AreEqual(1000-hit.damage,hp.Current,"Duplicate impact");
                }
            }
            finally{pool.Clear();Object.Destroy(target);}
        }
        [UnityTest]public IEnumerator ProjectileStopsAtWallWithoutDamagingBeyondIt()
        {
            var pool=Object.FindFirstObjectByType<CombatEffectPool>();var target=GameObject.CreatePrimitive(PrimitiveType.Capsule);var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                target.layer=10;target.transform.position=new Vector3(100,1,103);var hp=target.AddComponent<Health>();hp.Configure(100,CombatFaction.Enemy);
                wall.layer=9;wall.transform.position=new Vector3(100,1,101.5f);wall.transform.localScale=new Vector3(3,3,.3f);Physics.SyncTransforms();
                pool.Shoot(new Vector3(100,1,100),Vector3.forward,Weapon(WeaponCategory.Grimoire).attacks[2],Mage.Health,1<<10,1<<9);
                yield return new WaitForSeconds(.5f);Assert.AreEqual(100,hp.Current);Assert.AreEqual(0,pool.ActiveProjectiles);
            }
            finally{pool.Clear();Object.Destroy(target);Object.Destroy(wall);}
        }
        [UnityTest]public IEnumerator SealsRootOnceAndSuperConsumesMeterAndAppliesRealDamage()
        {
            Assert.IsTrue(session.HostDirect("PRISMA",7897,false));yield return null;session.StartGame();float deadline=Time.realtimeSinceStartup+8;while(session.Phase!=CoopPhase.Playing&&Time.realtimeSinceStartup<deadline)yield return null;Assert.AreEqual(CoopPhase.Playing,session.Phase);
            var pool=Object.FindFirstObjectByType<CombatEffectPool>();var target=GameObject.CreatePrimitive(PrimitiveType.Capsule);target.layer=10;target.AddComponent<SphereCollider>();var hp=target.AddComponent<Health>();var charge=ScriptableObject.CreateInstance<AttackDefinition>();charge.damage=1;charge.knockback=0;
            try
            {
                hp.Configure(10000,CombatFaction.Enemy);target.transform.position=Mage.transform.position+Vector3.forward*2+Vector3.up;Physics.SyncTransforms();
                var book=Weapon(WeaponCategory.Grimoire);pool.PlaceZone(target.transform.position,2.5f,6,book.ability.hit,Mage.Health,1<<10,1<<9,true);
                yield return new WaitForSeconds(.15f);Assert.AreEqual(9980,hp.Current);Assert.AreEqual(0,hp.ControlMultiplier);yield return new WaitForSeconds(.2f);Assert.AreEqual(9980,hp.Current);
                Mage.Combat.ResetCombat();Mage.Combat.ApplyEquipment(null,Plain,1,false);hp.Restore();for(int i=0;i<Mage.Roster.Current.onlineFlow.hitsForSuper;i++)hp.ApplyHit(charge,Vector3.forward,Mage.Health);Assert.IsTrue(Mage.Combat.SuperReady);
                int before=hp.Current;Assert.IsTrue(Mage.Combat.TrySuper());Mage.Combat.Simulate(.31f);Assert.AreEqual(before-50,hp.Current);Assert.AreEqual(0,hp.ControlMultiplier);Assert.AreEqual(0,Mage.Combat.SuperProgress);Assert.IsFalse(Mage.Combat.TrySuper());
            }
            finally{pool.Clear();Mage.Combat.ResetCombat();Object.Destroy(target);Object.Destroy(charge);}
        }
        [Test]public void FeedbackSwitchesWithHeroWithoutKeepingWarriorSounds()
        {
            var feedback=Mage.GetComponent<ActorImpactFeedback>();Assert.AreSame(Mage.Roster.Current.feedback,feedback.Settings);
            var mage=feedback.Settings;Assert.NotNull(mage.swing);Assert.NotNull(mage.impact);Assert.NotNull(mage.heavy);
            Mage.Roster.Select(0);Assert.AreSame(Mage.Roster.Current.feedback,feedback.Settings);Assert.AreNotSame(mage,feedback.Settings);
            Mage.Roster.Select(2);Assert.AreSame(mage,feedback.Settings);
        }
        [Test]public void RemoteChargeAndSuperUseExistingPresentationFrame()
        {
            var actor=session.Actor(1);actor.Roster.Select(2);actor.gameObject.SetActive(true);actor.Presentation.enabled=false;
            actor.Presentation.ReplicaMode=true;actor.Presentation.ApplyReplica(HeroState.Attack,.5f,4,98,.3f,.75f);
            Assert.AreEqual(.75f,actor.Presentation.ChargeProgress);Assert.AreEqual(4,actor.Presentation.ActionVariant);
            var model=actor.Presentation.Visible.GetComponentInChildren<HeroModelView>();model.Animate(HeroState.Attack,.5f,0,true,4,98,.3f);model.Animator.Update(.2f);
            Assert.IsTrue(model.Animator.GetCurrentAnimatorStateInfo(0).IsName(model.AuthoredAnimationsReady?"Base Layer.Super":"Base Layer.Idle"));
        }
        [UnityTest]public IEnumerator DodgePresentationUsesMovementClockInsteadOfAnIdleAttackClock()
        {
            Assert.IsTrue(session.HostDirect("PRISMD",7896,false));yield return null;session.StartGame();float deadline=Time.realtimeSinceStartup+8;
            while(session.Phase!=CoopPhase.Playing&&Time.realtimeSinceStartup<deadline)yield return null;Assert.AreEqual(CoopPhase.Playing,session.Phase);
            Mage.Combat.ResetCombat();Assert.IsTrue(Mage.Motor.TryDodge(Vector2.right));Mage.Motor.Simulate(.06f,Vector2.zero);yield return null;
            Assert.AreEqual(HeroState.Dodge,Mage.Presentation.State);Assert.That(Mage.Presentation.ActionProgress,Is.InRange(.2f,.95f));Assert.AreEqual(0,Mage.Combat.NormalizedTime);
        }
        [Test]public void CasterWeaponsUseConfiguredUprightGripsIncludingTheStarter()
        {
            var model=Mage.Presentation.Visible.GetComponentInChildren<HeroModelView>();Mage.Presentation.enabled=false;
            model.Animate(HeroState.Idle,0,0,true);model.Animator.Update(.2f);
            foreach(var item in catalog.equipment.Where(x=>x.category==WeaponCategory.Staff||x.category==WeaponCategory.Grimoire||x.category==WeaponCategory.MagicWeapon))
            {
                Assert.IsTrue(item.customGrip,item.itemId);var go=new GameObject("CasterGripProbe");
                try{EquipmentVisualPlacement.Mount(go.transform,model.PrimarySocket,true,Vector3.zero,Quaternion.identity,item);EquipmentVisualPlacement.SetWorldScale(go.transform,item.authoredScale);
                    Assert.Greater(go.transform.up.y,.4f,item.itemId+" points down");Assert.Less(Vector3.Distance(go.transform.position,model.PrimarySocket.position),.65f);}
                finally{Object.DestroyImmediate(go);}
            }
        }
        [Test] public void AcceptedAimIsUsedWhenAnAttackStartsBetweenNetworkPackets()
        {
            Mage.SetAuthority(true);Mage.Motor.enabled=Mage.Combat.enabled=false;
            Mage.Motor.Teleport(new Vector3(100,.06f,100),Quaternion.identity);
            Assert.IsTrue(Mage.Commands.Accept(1,Vector2.up,Vector2.right,HeroButtons.Attack));
            Assert.IsTrue(Mage.Combat.TryAttack());
            Assert.Greater(Vector3.Dot(Mage.transform.forward,Vector3.right),.999f);
        }
        [Test] public void ProjectileVisualAndReplicatedHeadingMatchItsFlightDirection()
        {
            var pool=Mage.Combat.Pool;pool.Clear();
            Vector3 direction=new Vector3(1,0,-1).normalized;
            Assert.IsTrue(pool.Shoot(new Vector3(100,1,100),direction,Weapon(WeaponCategory.Staff).attacks[0],Mage.Health,1<<10,1<<9));
            var shot=pool.Projectiles.First(x=>x.gameObject.activeSelf);
            Assert.Greater(Vector3.Dot(shot.transform.forward,direction),.999f);
            pool.Clear();
        }
        [Test] public void MovingInEightDirectionsTracksAimUntilReleaseThenCommitsTheShot()
        {
            Mage.SetAuthority(true);Mage.Motor.enabled=Mage.Combat.enabled=false;
            for(int i=0;i<8;i++)
            {
                Mage.Combat.ResetCombat();Mage.Motor.Teleport(new Vector3(100,.06f,100),Quaternion.identity);
                Vector3 direction=Quaternion.Euler(0,i*45,0)*Vector3.forward;
                Assert.IsTrue(Mage.Commands.Accept((uint)(i*3+10),Vector2.up,new Vector2(direction.x,direction.z),HeroButtons.Attack));
                Assert.IsTrue(Mage.Combat.TryAttack());
                Mage.Motor.Simulate(.04f,Vector2.right);
                Vector3 opposite=-direction;
                Assert.IsTrue(Mage.Commands.Accept((uint)(i*3+11),Vector2.right,new Vector2(opposite.x,opposite.z),HeroButtons.Attack));
                Assert.IsTrue(Mage.Motor.FacingLocked);
                Mage.Combat.Simulate(.3f);
                var shot=Mage.Combat.Pool.Projectiles.First(x=>x.gameObject.activeSelf);
                Assert.Greater(Vector3.Dot(shot.transform.forward,opposite),.999f,"Release direction "+i);
                Assert.Greater(Vector3.Dot(Mage.transform.forward,opposite),.999f,"Pose direction "+i);
                Assert.IsTrue(Mage.Commands.Accept((uint)(i*3+12),Vector2.left,new Vector2(direction.x,direction.z),HeroButtons.None));
                Mage.Combat.Simulate(.01f);
                Assert.Greater(Vector3.Dot(shot.transform.forward,opposite),.999f,"Flight must not home after release");
                Mage.Combat.Simulate(2);Mage.Combat.Pool.Clear();
                Assert.IsTrue(Mage.Combat.TryAttack());
                Assert.Greater(Vector3.Dot(Mage.transform.forward,direction),.999f,"Next cast "+i);
            }
        }
        [Test] public void ValidatedAimRejectsInvalidAndOldPacketsAndClearsOnReset()
        {
            var input=Mage.Commands;input.Clear();
            Assert.IsTrue(input.Accept(5,Vector2.zero,Vector2.right,HeroButtons.None));
            Assert.AreEqual(Vector3.right,input.AimDirection);
            Assert.IsFalse(input.Accept(4,Vector2.zero,Vector2.up,HeroButtons.None));
            Assert.IsFalse(input.Accept(6,Vector2.zero,new Vector2(float.NaN,0),HeroButtons.None));
            Assert.AreEqual(Vector3.right,input.AimDirection);
            input.Clear();Assert.AreEqual(Vector3.zero,input.AimDirection);
        }
        EquipmentDefinition Weapon(WeaponCategory kind)=>catalog.equipment.First(x=>x.category==kind&&!x.starter);
    }
}
