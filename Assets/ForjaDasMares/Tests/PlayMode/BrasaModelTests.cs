using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Tests
{
    public sealed class BrasaModelTests
    {
        HeroRoster heroes;HeroModelView model;ProgressionSceneBridge bridge;
        [UnitySetUp] public IEnumerator Load()
        {
            SceneManager.LoadScene("GaleriasDungeon");yield return null;yield return null;yield return null;
            heroes=Object.FindFirstObjectByType<HeroRoster>();bridge=Object.FindFirstObjectByType<ProgressionSceneBridge>();
            model=heroes.GetComponent<HeroPresentation>().Visible.GetComponentInChildren<HeroModelView>(true);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            bridge.Abort();var profile=Object.FindFirstObjectByType<ProgressionProfileRuntime>();
            SceneManager.LoadScene("ArenaTest");if(profile!=null)Object.Destroy(profile.gameObject);yield return null;
        }
        [Test] public void AuthoredWarriorHasSkinnedGeometryEightClipsAndHandSockets()
        {
            Assert.NotNull(model);Assert.NotNull(model.PrimarySocket);Assert.NotNull(model.SecondarySocket);Assert.NotNull(model.DefaultWeapon);
            int vertices=0;foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Assert.NotNull(skin.sharedMesh);vertices+=skin.sharedMesh.vertexCount;Assert.That(skin.bones.Length,Is.GreaterThan(0));
                foreach(var material in skin.sharedMaterials){Assert.NotNull(material);Assert.That(material.shader.name,Is.EqualTo("Universal Render Pipeline/Lit"));}
            }
            Assert.That(vertices,Is.GreaterThan(2000));var clips=model.Animator.runtimeAnimatorController.animationClips;
            foreach(string name in new[]{"Idle","Walk","Attack","Skill","Dodge","Hit","Dead","Victory"})
                Assert.IsTrue(System.Array.Exists(clips,c=>c.name==name&&c.length>.1f),"Missing authored clip "+name);
            Assert.IsFalse(model.Animator.applyRootMotion);Assert.That(heroes.GetComponent<CharacterController>().height,Is.GreaterThan(1));
        }
        [Test] public void WalkAndAttackMoveBonesWithoutMovingTheActorOrApplyingDamage()
        {
            var presentation=heroes.GetComponent<HeroPresentation>();presentation.enabled=false;
            Vector3 actor=heroes.transform.position;int hp=heroes.Health.Current;
            model.Animate(HeroState.Walk,0,5.8f,true);model.Animator.SetFloat("GaitTime",.25f);model.Animator.Play("Base Layer.Walk",0,.25f);model.Animator.Update(0);
            Vector3 hand=model.PrimarySocket.position;
            model.Animator.SetFloat("GaitTime",.75f);model.Animator.Play("Base Layer.Walk",0,.75f);model.Animator.Update(0);
            Assert.That(Vector3.Distance(hand,model.PrimarySocket.position),Is.GreaterThan(.02f),"A real walk clip must move the skeleton.");
            model.Animate(HeroState.Attack,.25f,0,true);model.Animator.Update(.08f);hand=model.PrimarySocket.position;
            model.Animate(HeroState.Attack,.75f,0,true);model.Animator.Update(0);
            Assert.That(Vector3.Distance(hand,model.PrimarySocket.position),Is.GreaterThan(.02f));
            Assert.That(Vector3.Distance(actor,heroes.transform.position),Is.LessThan(.001f));Assert.That(heroes.Health.Current,Is.EqualTo(hp));
        }
        [Test] public void AllEightPresentationStatesResolveToRealAnimationStates()
        {
            heroes.GetComponent<HeroPresentation>().enabled=false;
            foreach(HeroState state in System.Enum.GetValues(typeof(HeroState)))
            {
                model.Animate(state,.5f,5.8f,state!=HeroState.Dead);model.Animator.Play("Base Layer."+state,0,.5f);model.Animator.Update(0);
                Assert.IsTrue(model.Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer."+state));
                Assert.IsFalse(float.IsNaN(model.PrimarySocket.position.x));
            }
        }
        [UnityTest] public IEnumerator EquippingSwitchingHeroesAndUnequippingPreserveTheNewModel()
        {
            var item=bridge.Service.Profile.inventory.Find(i=>i.definitionId=="cais-blade");Assert.NotNull(item);
            Assert.IsTrue(bridge.Equip(item.instanceId,EquipmentSlot.MainHand));Assert.IsFalse(model.DefaultWeapon.activeSelf);
            Assert.That(model.PrimarySocket.childCount,Is.GreaterThan(0));Assert.IsTrue(bridge.Unequip(EquipmentSlot.MainHand));Assert.IsTrue(model.DefaultWeapon.activeSelf);
            heroes.Select(1);yield return null;Assert.IsNull(heroes.GetComponent<HeroPresentation>().Visible.GetComponentInChildren<HeroModelView>(true));
            heroes.Select(0);yield return null;Assert.IsTrue(model.gameObject.activeInHierarchy);Assert.IsTrue(model.DefaultWeapon.activeSelf);
        }
        [UnityTest] public IEnumerator OnlyOneBodyRemainsVisibleAfterEquipmentAndHeroChanges()
        {
            for(int i=0;i<4;i++)
            {
                heroes.Select(1);yield return null;heroes.Select(0);yield return null;
                var visible=heroes.GetComponent<HeroPresentation>().Visible;
                Assert.That(visible.GetComponentsInChildren<HeroModelView>(true).Length,Is.EqualTo(1));
                foreach(string name in new[]{"Body_Placeholder","Head_Placeholder","FacingMarker","GreatSword"})Assert.IsFalse(visible.Find(name).gameObject.activeInHierarchy,"Legacy overlap: "+name);
                int bodies=0;foreach(var skin in visible.GetComponentsInChildren<SkinnedMeshRenderer>())if(skin.sharedMesh.vertexCount>1000)bodies++;
                Assert.That(bodies,Is.EqualTo(1),"Exactly one body skin must render.");
                foreach(var renderer in visible.GetComponentsInChildren<MeshRenderer>())Assert.IsFalse(renderer.name=="Body_Placeholder"||renderer.name=="Head_Placeholder");
            }
        }
        [Test] public void ComboHeavyAndSuperUseSeparateClipsWithFiniteSkinDeformation()
        {
            heroes.GetComponent<HeroPresentation>().enabled=false;
            var baked=new Mesh();
            for(int variant=0;variant<5;variant++)
            {
                model.Animate(variant==4?HeroState.Skill:HeroState.Attack,.5f,0,true,variant,variant+1,.3f);model.Animator.Update(.1f);
                string name=new[]{"SwordSlash","SwordReturn","SwordThrust","HeavySword","Super"}[variant];
                Assert.IsTrue(model.Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer."+name),name);
                foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {skin.BakeMesh(baked);Assert.That(baked.bounds.size.magnitude,Is.LessThan(6),"Exploding or duplicated displaced skin: "+name);Assert.IsFalse(float.IsNaN(baked.bounds.center.x));}
            }
            Object.DestroyImmediate(baked);
        }
        [UnityTest] public IEnumerator HoldingACombatPoseDoesNotRestartAnAnimatorTransitionEveryFrame()
        {
            heroes.GetComponent<HeroPresentation>().enabled=false;
            for(int i=0;i<30;i++){model.Animate(HeroState.Attack,.5f,0,true,1,42,.25f);yield return null;}
            yield return new WaitForSeconds(.12f);
            Assert.IsFalse(model.Animator.IsInTransition(0));Assert.IsTrue(model.Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.SwordReturn"));
        }
    }
}
