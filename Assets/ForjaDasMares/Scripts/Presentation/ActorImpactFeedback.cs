using UnityEngine;
using UnityEngine.Rendering;
namespace ForjaDasMares
{
    public sealed class ActorImpactFeedback:MonoBehaviour
    {
        Health health;CombatFeelDefinition data;HeroPresentation hero;HeroModelView model;TrailRenderer trail;
        int action=-1,lastHitAction=-2;float stepClock,lastSound,lastVibration;Vector3 previous;bool positioned;
        public void Configure(Health owner,CombatFeelDefinition settings,HeroPresentation presentation=null)
        {
            Unbind();health=owner;data=settings;hero=presentation;previous=transform.position;positioned=false;
            if(isActiveAndEnabled)Bind();
        }
        void Bind(){if(health==null)return;health.Damaged-=Hurt;health.Damaged+=Hurt;health.DealtHit-=Hit;health.DealtHit+=Hit;}
        void Unbind(){if(health==null)return;health.Damaged-=Hurt;health.DealtHit-=Hit;}
        public void ResetVisual(){if(trail!=null)Destroy(trail.gameObject);trail=null;model=null;action=-1;lastHitAction=-2;}
        void OnEnable()=>Bind();
        void OnDisable(){Unbind();if(trail!=null){trail.emitting=false;trail.Clear();}positioned=false;}
        void OnDestroy(){if(trail!=null)Destroy(trail.gameObject);}
        void Hurt(){if(data==null)return;var follow=Camera.main!=null?Camera.main.GetComponent<ArenaCamera>():null;if(follow!=null&&follow.Target!=null&&(follow.Target.position-transform.position).sqrMagnitude>400)return;if(Time.unscaledTime-lastSound>.055f){CombatAudioPool.Play(data.impact,data.volume);lastSound=Time.unscaledTime;}Pause();if(Camera.main!=null){var camera=Camera.main.GetComponent<ArenaCamera>();if(camera!=null&&camera.Target==transform)camera.Impact(data.cameraImpulse);}if(hero!=null&&follow!=null&&follow.Target==transform&&PlayerPrefs.GetInt("CampaignVibration",0)==1&&Application.isMobilePlatform&&Time.unscaledTime-lastVibration>1){lastVibration=Time.unscaledTime;Handheld.Vibrate();}}
        void Hit(Health target){if(data==null||hero==null||target.Faction!=CombatFaction.Enemy||lastHitAction==hero.ActionVersion)return;lastHitAction=hero.ActionVersion;Pause();if(hero.ActionVariant>=3)CombatAudioPool.Play(data.heavy,data.volume*.5f,.92f);}
        void Pause(){if(hero!=null&&hero.Visible!=null){if(model==null||!model.gameObject.activeInHierarchy)model=hero.Visible.GetComponentInChildren<HeroModelView>();model?.PauseImpact(data.impactPause);}}
        void LateUpdate()
        {
            if(data==null||hero==null)return;
            if(hero.ActionVersion!=action&&(hero.State==HeroState.Attack||hero.State==HeroState.Skill))
            {
                action=hero.ActionVersion;CombatAudioPool.Play(data.swing,data.volume*.65f,hero.ActionVariant>=3?.8f:1);
                if(trail==null&&hero.Visible!=null)
                {
                    var gear=hero.Visible.GetComponentInChildren<EquipmentMeshView>();Transform grip=gear!=null?gear.transform:hero.Visible.GetComponentInChildren<HeroModelView>()?.PrimarySocket;
                    if(grip!=null&&data.trailMaterial!=null){var go=new GameObject("WeaponTipTrail");go.transform.SetParent(grip,false);go.transform.localPosition=Vector3.up*1.1f;trail=go.AddComponent<TrailRenderer>();trail.sharedMaterial=data.trailMaterial;trail.time=.12f;trail.minVertexDistance=.08f;trail.startWidth=.14f;trail.endWidth=0;trail.startColor=new Color(1,.8f,.35f,.65f);trail.endColor=new Color(1,.6f,.15f,0);trail.shadowCastingMode=ShadowCastingMode.Off;trail.receiveShadows=false;trail.emitting=false;}
                }
            }
            if(trail!=null)trail.emitting=health.IsAlive&&hero.State==HeroState.Attack&&hero.ActionProgress>.2f&&hero.ActionProgress<.7f&&(hero.EquippedWeapon==null||hero.EquippedWeapon.attacks[0].shape==AttackShape.Arc);
            Vector3 delta=transform.position-previous;delta.y=0;previous=transform.position;if(!positioned){positioned=true;return;}
            if(hero.State==HeroState.Walk&&delta.sqrMagnitude<1){stepClock+=delta.magnitude;if(stepClock>.95f){stepClock=0;CombatAudioPool.Play(data.step,data.volume*.35f);}}else stepClock=0;
        }
    }
}
