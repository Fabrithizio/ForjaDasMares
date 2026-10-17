using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ForjaDasMares.Tests
{
    public sealed class BrasaLocomotionTests
    {
        HeroRoster heroes;HeroModelView view;HeroPresentation presentation;BrasaFootPlant plant;ProgressionSceneBridge bridge;
        float previousCapture;
        [UnitySetUp] public IEnumerator Load()
        {
            previousCapture=Time.captureDeltaTime;Time.captureDeltaTime=1f/60;
            SceneManager.LoadScene("GaleriasDungeon");yield return null;yield return null;yield return null;
            heroes=Object.FindFirstObjectByType<HeroRoster>();bridge=Object.FindFirstObjectByType<ProgressionSceneBridge>();
            presentation=heroes.GetComponent<HeroPresentation>();view=presentation.Visible.GetComponentInChildren<HeroModelView>();
            plant=view.GetComponent<BrasaFootPlant>();
            heroes.GetComponent<PlayerMotor>().enabled=false;heroes.GetComponent<PlayerAim>().enabled=false;
            heroes.GetComponent<ArenaInput>().enabled=false;heroes.GetComponent<HeroCombat>().enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.captureDeltaTime=previousCapture;bridge.Abort();var profile=Object.FindFirstObjectByType<ProgressionProfileRuntime>();
            SceneManager.LoadScene("ArenaTest");if(profile!=null)Object.Destroy(profile.gameObject);yield return null;
        }
        static Transform Bone(HeroModelView owner,string name)
        {foreach(var t in owner.GetComponentsInChildren<Transform>())if(t.name==name)return t;return null;}
        [UnityTest] public IEnumerator StationaryActorDoesNotRunFromDesiredMotorVelocity()
        {
            for(int i=0;i<20;i++)yield return null;
            Assert.That(view.MovementSpeed,Is.LessThan(.01f));Assert.IsFalse(view.LocomotionActive);
            Assert.IsTrue(view.Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle"));
            float phase=view.GaitPhase;yield return null;Assert.That(view.GaitPhase,Is.EqualTo(phase));
        }
        [UnityTest] public IEnumerator MovingAttacksKeepRealLegMotionOnBaseLayer()
        {
            presentation.enabled=false;Vector3 origin=heroes.transform.position;
            var shin=Bone(view,"Shin.L");Quaternion first=shin.localRotation;float largest=0;
            for(int i=0;i<70;i++)
            {
                heroes.transform.position+=Vector3.forward*(5.8f*Time.deltaTime);
                view.Animate(HeroState.Attack,(i%25)/25f,5.8f,true,0,i/25,.25f);
                yield return null;largest=Mathf.Max(largest,Quaternion.Angle(first,shin.localRotation));
            }
            Assert.IsTrue(view.Animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Run"));
            Assert.That(view.Animator.GetLayerWeight(1),Is.GreaterThan(.95f));
            Assert.That(largest,Is.GreaterThan(15),"The knees must articulate while attacking and moving.");
            Assert.That(Vector3.Distance(origin,heroes.transform.position),Is.GreaterThan(1));
        }
        [UnityTest] public IEnumerator StanceFootRemainsAtItsWorldContactDuringWalkingAndStrafing()
        {
            presentation.enabled=false;var foot=Bone(view,"Foot.L");int contacts=0;float maxDrift=0;
            for(int axis=0;axis<4;axis++)
            {
                bool prior=false;Vector3 anchor=Vector3.zero;
                for(int i=0;i<100;i++)
                {
                    float speed=axis<2?2:5.8f;
                    heroes.transform.position+=(axis%2==0?Vector3.forward:Vector3.right)*(speed*Time.deltaTime);
                    view.Animate(HeroState.Walk,0,speed,true);yield return null;
                    if(plant.LeftPlanted&&prior&&Vector3.Distance(anchor,plant.LeftAnchor)<.001f)
                    {Vector3 error=foot.position-plant.LeftAnchor;error.y=0;maxDrift=Mathf.Max(maxDrift,error.magnitude);contacts++;}
                    prior=plant.LeftPlanted;anchor=plant.LeftAnchor;
                }
            }
            Assert.That(contacts,Is.GreaterThan(15));
            Assert.That(maxDrift,Is.LessThan(.06f),"Planted feet must remain near the same ground contact.");
        }
        [UnityTest] public IEnumerator IdleBreathesAndTransitionsRemainFiniteAfterDodgeAndTeleport()
        {
            presentation.enabled=false;var head=Bone(view,"Head");Quaternion first=head.localRotation;float motion=0;
            for(int i=0;i<90;i++){view.Animate(HeroState.Idle,0,0,true);yield return null;motion=Mathf.Max(motion,Quaternion.Angle(first,head.localRotation));}
            Assert.That(motion,Is.GreaterThan(1));
            view.Animate(HeroState.Dodge,.5f,12,true);yield return null;
            heroes.transform.position+=Vector3.right*10;view.Animate(HeroState.Walk,0,5.8f,true);yield return null;
            for(int i=0;i<20;i++){view.Animate(HeroState.Idle,0,0,true);yield return null;}
            foreach(var skin in view.GetComponentsInChildren<SkinnedMeshRenderer>())
            {var mesh=new Mesh();skin.BakeMesh(mesh);Assert.That(mesh.bounds.size.magnitude,Is.LessThan(6));Assert.IsFalse(float.IsNaN(mesh.bounds.center.x));Object.Destroy(mesh);}
        }
    }
}
