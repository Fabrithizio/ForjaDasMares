using UnityEngine;
using UnityEngine.Rendering;
namespace ForjaDasMares
{
    // Muzzle wisps, charge glow and impact sparks; no cones, circles or damage.
    [DefaultExecutionOrder(230)]
    public sealed class HeroWeaponEffects:MonoBehaviour
    {
        HeroPresentation view;HeroRoster roster;HeroCombat combat;Health health;ParticleSystem particles;
        int version=-1,released=-1,serial;float emissionClock;bool ready;Color tint;
        public int RibbonCapacity=>0;
        public int LiveParticles=>particles!=null?particles.particleCount:0;
        public void Configure(HeroPresentation source,HeroRoster heroes,HeroCombat attacks)
        {
            view=source;roster=heroes;combat=attacks;health=heroes.Health;
            var material=Resources.Load<Material>("HeroEnergySpark");
            if(material==null){Debug.LogError("Hero energy material missing.",this);enabled=false;return;}
            var go=new GameObject("HeroWispsAndSparks_64");go.transform.SetParent(transform,false);particles=go.AddComponent<ParticleSystem>();
            var main=particles.main;main.loop=false;main.playOnAwake=false;main.maxParticles=64;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startLifetime=.35f;main.gravityModifier=0;
            var emission=particles.emission;emission.enabled=false;var shape=particles.shape;shape.enabled=false;
            var color=particles.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.08f),new GradientAlphaKey(0,1)});color.color=gradient;
            var size=particles.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,1,1,0));
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=1.2f;renderer.velocityScale=.12f;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);ready=true;
        }
        void OnEnable(){if(health!=null)health.DealtHit+=Hit;}
        void Start(){if(health!=null){health.DealtHit-=Hit;health.DealtHit+=Hit;}}
        void OnDisable(){if(health!=null)health.DealtHit-=Hit;if(particles!=null)particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
        void Emit(Vector3 at,Vector3 velocity,Color color,float size,float life)
        {particles.Play();particles.Emit(new ParticleSystem.EmitParams{position=at,velocity=velocity,startColor=color,startSize=size,startLifetime=life},1);}
        void Burst(Vector3 at,int count,bool spell,float power)
        {
            for(int i=0;i<count;i++)
            {
                float a=(serial++%10000)*2.399963f;Vector3 spread=new Vector3(Mathf.Cos(a),.3f+(i%4)*.2f,Mathf.Sin(a));
                Vector3 velocity=spell?transform.forward*power+spread*.55f:spread*power;
                Emit(at,velocity,tint,(spell?.10f:.035f)+(i%3)*.025f,.18f+(i%5)*.045f);
            }
        }
        void Hit(Health victim){if(ready&&victim!=null)Burst(victim.transform.position+Vector3.up*.8f,18,view.HasCastingView,1.2f);}
        void LateUpdate()
        {
            if(!ready||view==null||!health.IsAlive)return;
            bool caster=view.HasCastingView;var weapon=view.EquippedWeapon;
            bool book=weapon!=null&&weapon.category==WeaponCategory.Grimoire;
            tint=caster?(book?new Color(.8f,.25f,1,.8f):new Color(.12f,.75f,1,.8f)):new Color(1,.48f,.08f,.8f);
            bool action=view.State==HeroState.Attack||view.State==HeroState.Skill;
            if(action&&version!=view.ActionVersion){version=view.ActionVersion;released=-1;}
            if(action&&released!=version&&view.ActionProgress>=view.ActionImpact)
            {released=version;if(caster)Burst(combat.StrikeOrigin+transform.forward*.30f,view.ActionVariant>=3?36:20,true,view.ActionVariant>=3?3.5f:2);}
            bool fury=view.FuryRemaining>0;
            float charge=view.ChargeProgress;bool preparing=caster&&(charge>.05f||action&&view.ActionProgress<view.ActionImpact);
            emissionClock-=Time.deltaTime;if(emissionClock>0||!preparing&&!fury)return;emissionClock=.025f;
            var model=view.Visible!=null?view.Visible.GetComponentInChildren<HeroModelView>():null;if(model==null)return;
            float angle=(serial++%10000)*2.399963f;
            if(preparing)
            {
                Vector3 offset=(transform.right*Mathf.Cos(angle)+Vector3.up*Mathf.Sin(angle))*(.04f+charge*.09f);
                Emit(model.PrimarySocket.position+offset,-offset*2+Vector3.up*.12f,tint,.10f+charge*.12f,.18f);
                Emit(model.SecondarySocket.position-offset,offset*2+Vector3.up*.12f,book?new Color(1,.65f,.12f,.75f):tint,.08f+charge*.10f,.18f);
            }
            if(fury)Emit(transform.position+Vector3.up*.25f+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*.28f,Vector3.up*(.7f+Mathf.Abs(Mathf.Cos(angle))),new Color(1,.25f,.025f,.65f),.055f,.4f);
        }
    }
}
