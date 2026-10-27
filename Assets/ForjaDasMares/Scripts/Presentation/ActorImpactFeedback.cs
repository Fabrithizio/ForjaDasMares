using UnityEngine;
namespace ForjaDasMares
{
    public sealed class ActorImpactFeedback:MonoBehaviour
    {
        Health health;CombatFeelDefinition data;HeroPresentation hero;HeroModelView model;
        public CombatFeelDefinition Settings=>data;
        int action=-1,lastHitAction=-2;float stepClock,lastSound,lastVibration;Vector3 previous;bool positioned;
        public void Configure(Health owner,CombatFeelDefinition settings,HeroPresentation presentation=null)
        {
            Unbind();health=owner;data=settings;hero=presentation;previous=transform.position;positioned=false;
            if(isActiveAndEnabled)Bind();
        }
        void Bind(){if(health==null)return;health.Damaged-=Hurt;health.Damaged+=Hurt;health.DealtHit-=Hit;health.DealtHit+=Hit;}
        void Unbind(){if(health==null)return;health.Damaged-=Hurt;health.DealtHit-=Hit;}
        public void ResetVisual(){model=null;action=-1;lastHitAction=-2;}
        void OnEnable()=>Bind();
        void OnDisable(){Unbind();positioned=false;}
        void OnDestroy()=>Unbind();
        void Hurt(){if(data==null)return;var follow=Camera.main!=null?Camera.main.GetComponent<ArenaCamera>():null;if(follow!=null&&follow.Target!=null&&(follow.Target.position-transform.position).sqrMagnitude>400)return;if(Time.unscaledTime-lastSound>.055f){CombatAudioPool.Play(data.impact,data.volume);lastSound=Time.unscaledTime;}Pause();if(Camera.main!=null){var camera=Camera.main.GetComponent<ArenaCamera>();if(camera!=null&&camera.Target==transform)camera.Impact(data.cameraImpulse);}if(hero!=null&&follow!=null&&follow.Target==transform&&PlayerPrefs.GetInt("CampaignVibration",0)==1&&Application.isMobilePlatform&&Time.unscaledTime-lastVibration>1){lastVibration=Time.unscaledTime;Vibrate();}}
        static void Vibrate()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
        void Hit(Health target){if(data==null||hero==null||target.Faction!=CombatFaction.Enemy||lastHitAction==hero.ActionVersion)return;lastHitAction=hero.ActionVersion;Pause();if(hero.ActionVariant>=3)CombatAudioPool.Play(data.heavy,data.volume*.5f,.92f);}
        void Pause(){if(hero!=null&&hero.Visible!=null){if(model==null||!model.gameObject.activeInHierarchy)model=hero.Visible.GetComponentInChildren<HeroModelView>();model?.PauseImpact(data.impactPause);}}
        void LateUpdate()
        {
            if(data==null||hero==null)return;
            if(hero.ActionVersion!=action&&(hero.State==HeroState.Attack||hero.State==HeroState.Skill))
            {
                action=hero.ActionVersion;CombatAudioPool.Play(data.swing,data.volume*.65f,hero.ActionVariant>=3?.8f:1);
            }
            Vector3 delta=transform.position-previous;delta.y=0;previous=transform.position;if(!positioned){positioned=true;return;}
            if(hero.State==HeroState.Walk&&delta.sqrMagnitude<1){stepClock+=delta.magnitude;float stride=delta.magnitude/Mathf.Max(.001f,Time.deltaTime)>3.2f?1.23f:.46f;if(stepClock>stride){stepClock-=stride;CombatAudioPool.Play(data.step,data.volume*.35f);}}else stepClock=0;
        }
    }
}
