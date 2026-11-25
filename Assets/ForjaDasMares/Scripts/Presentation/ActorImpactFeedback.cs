using UnityEngine;
namespace ForjaDasMares
{
    public sealed class ActorImpactFeedback:MonoBehaviour
    {
        Health health;CombatFeelDefinition data;HeroPresentation hero;HeroModelView model;
        public CombatFeelDefinition Settings=>data;
        int action=-1,lastHitAction=-2;float lastSound,lastVibration;
        public void Configure(Health owner,CombatFeelDefinition settings,HeroPresentation presentation=null)
        {
            Unbind();health=owner;data=settings;hero=presentation;
            if(isActiveAndEnabled)Bind();
        }
        void Bind(){if(health==null)return;health.Damaged-=Hurt;health.Damaged+=Hurt;health.DealtHit-=Hit;health.DealtHit+=Hit;}
        void Unbind(){if(health==null)return;health.Damaged-=Hurt;health.DealtHit-=Hit;}
        public void ResetVisual(){model=null;action=-1;lastHitAction=-2;}
        void OnEnable()=>Bind();
        void OnDisable()=>Unbind();
        void OnDestroy()=>Unbind();
        void Hurt(){if(data==null)return;var follow=Camera.main!=null?Camera.main.GetComponent<ArenaCamera>():null;if(follow!=null&&follow.Target!=null&&(follow.Target.position-transform.position).sqrMagnitude>400)return;if(Time.unscaledTime-lastSound>.055f){CombatAudioPool.Play(data.impact,data.volume);lastSound=Time.unscaledTime;}Pause();if(Camera.main!=null){var camera=Camera.main.GetComponent<ArenaCamera>();if(camera!=null&&camera.Target==transform)camera.Impact(data.cameraImpulse);}if(hero!=null&&follow!=null&&follow.Target==transform&&PlayerPrefs.GetInt("CampaignVibration",0)==1&&UnityEngine.Device.Application.isMobilePlatform&&Time.unscaledTime-lastVibration>1){lastVibration=Time.unscaledTime;Vibrate();}}
        static void Vibrate()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
        void Hit(Health target){if(data==null||hero==null||target.Faction!=CombatFaction.Enemy||lastHitAction==hero.ActionVersion)return;lastHitAction=hero.ActionVersion;Pause();if(hero.ActionVariant>=3)SonsDoJogo.Tocar("Impacto_Forte",data.volume*.6f,data.heavy,.92f);else if(Corpo()==0)SonsDoJogo.Tocar("Espada_Acerto",data.volume*.55f);if(Corpo()==0)Corte(target,hero.ActionVariant>=3);}
        // A blade blow that lands: a crossed slash of light on the enemy (and a flash on strong blows).
        void Corte(Health target,bool strong)
        {
            var col=target.GetComponentInChildren<Collider>();
            Vector3 at=col!=null?col.bounds.center:target.transform.position+Vector3.up;
            var cam=Camera.main;Quaternion face=cam!=null?Quaternion.LookRotation(-cam.transform.forward):Quaternion.identity;
            // Bigger and brighter with the weapon's power (rarity + forge), like the slashes.
            var slashes=hero!=null?hero.GetComponent<CortesDaEspada>():null;float power=slashes!=null?slashes.Poder:0;
            EfeitosDeHabilidade.Extra("corte_cruz",at,face,(strong?1f:.7f)*(1+.6f*power));
            if(strong||power>=.5f)EfeitosDeHabilidade.Extra("acerto_brilho",at,face,1.1f*(1+.4f*power));
        }
        // What the hero holds: 0 = blade (sword, axe, hammer), 1 = caster (staff, book, focus), 2 = thrown (boomerang...).
        int Corpo()
        {
            if(hero==null||hero.Visible==null)return 2;
            if(model==null||!model.gameObject.activeInHierarchy)model=hero.Visible.GetComponentInChildren<HeroModelView>();
            var c=model!=null?model.CurrentWeaponCategory:WeaponCategory.None;
            return c==WeaponCategory.Sword||c==WeaponCategory.Axe||c==WeaponCategory.Hammer?0:c==WeaponCategory.Staff||c==WeaponCategory.Grimoire||c==WeaponCategory.MagicWeapon?1:2;
        }
        HeroState lastState;
        void Pause(){if(hero!=null&&hero.Visible!=null){if(model==null||!model.gameObject.activeInHierarchy)model=hero.Visible.GetComponentInChildren<HeroModelView>();model?.PauseImpact(data.impactPause);}}
        void LateUpdate()
        {
            if(data==null||hero==null)return;
            if(hero.ActionVersion!=action&&(hero.State==HeroState.Attack||hero.State==HeroState.Skill))
            {
                action=hero.ActionVersion;int corpo=Corpo();
                SonsDoJogo.Tocar(corpo==0?"Espada_Golpe":corpo==1?"Cajado_Magia":"Bumerangue_Arremesso",data.volume*(corpo==0?.6f:.5f),data.swing,hero.ActionVariant>=3?.85f:1);
            }
            if(hero.State==HeroState.Dodge&&lastState!=HeroState.Dodge)SonsDoJogo.Tocar("Esquiva",data.volume*.5f);
            lastState=hero.State;
            // No footstep sounds (Fabricio didn't like them): walking is silent until better steps are chosen.
        }
    }
}
