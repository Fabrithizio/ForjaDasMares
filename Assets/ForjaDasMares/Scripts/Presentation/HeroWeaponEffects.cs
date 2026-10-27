using UnityEngine;
using UnityEngine.Rendering;
namespace ForjaDasMares
{
    // Presentation only; fixed budget, no objects created per attack or gameplay RNG.
    [DefaultExecutionOrder(230)]
    public sealed class HeroWeaponEffects:MonoBehaviour
    {
        HeroPresentation view;HeroRoster roster;HeroCombat combat;Health health;
        readonly LineRenderer[] ribbons=new LineRenderer[6];readonly Vector3[][] points=new Vector3[6][];
        ParticleSystem sparks;int version=-1,released=-1;float tail;Color tint;AttackDefinition attack;bool ready;
        public int RibbonCapacity=>ribbons.Length;
        public int LiveParticles=>sparks!=null?sparks.particleCount:0;
        public void Configure(HeroPresentation source,HeroRoster heroes,HeroCombat attacks)
        {
            view=source;roster=heroes;combat=attacks;health=heroes.Health;
            Material material=Resources.Load<Material>("HeroEnergyRibbon"),particle=Resources.Load<Material>("HeroEnergySpark");
            if(material==null||particle==null){Debug.LogError("Hero energy materials are missing.",this);enabled=false;return;}
            for(int i=0;i<ribbons.Length;i++)
            {
                var go=new GameObject("HeroEnergy_"+i);go.transform.SetParent(transform,false);
                var line=ribbons[i]=go.AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;line.positionCount=40;points[i]=new Vector3[40];line.enabled=false;
                line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.numCapVertices=2;
            }
            var emit=new GameObject("HeroEnergySparks_64");emit.transform.SetParent(transform,false);sparks=emit.AddComponent<ParticleSystem>();
            var main=sparks.main;main.loop=false;main.playOnAwake=false;main.maxParticles=64;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startLifetime=.35f;main.startSize=.045f;main.gravityModifier=.15f;
            var emission=sparks.emission;emission.enabled=false;var shape=sparks.shape;shape.enabled=false;
            var color=sparks.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(0,1)});color.color=gradient;
            var size=sparks.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,1,1,0));
            var renderer=emit.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=particle;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            sparks.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);ready=true;
        }
        void OnEnable(){if(health!=null)health.DealtHit+=Hit;}
        void Start(){if(health!=null){health.DealtHit-=Hit;health.DealtHit+=Hit;}}
        void OnDisable(){if(health!=null)health.DealtHit-=Hit;tail=0;foreach(var r in ribbons)if(r!=null)r.enabled=false;if(sparks!=null)sparks.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
        void Hit(Health victim){if(ready&&victim!=null&&attack!=null)Burst(victim.transform.position+Vector3.up*.8f,tint,14,1.1f);}
        void Burst(Vector3 at,Color color,int count,float power)
        {
            sparks.Play();
            for(int i=0;i<count;i++)
            {
                float angle=i*2.399963f+version*.37f;
                var emit=new ParticleSystem.EmitParams{position=at,velocity=new Vector3(Mathf.Cos(angle),.2f+(i%4)*.2f,Mathf.Sin(angle))*power,startColor=color,startLifetime=.22f+(i%5)*.045f,startSize=.025f+(i%3)*.018f};sparks.Emit(emit,1);
            }
        }
        void LateUpdate()
        {
            if(!ready||view==null)return;
            if(!health.IsAlive){tail=0;foreach(var r in ribbons)r.enabled=false;return;}
            var weapon=view.EquippedWeapon;var category=weapon!=null?weapon.category:view.HasCastingView?WeaponCategory.Staff:WeaponCategory.Sword;
            bool supported=category==WeaponCategory.Staff||category==WeaponCategory.Grimoire||category==WeaponCategory.MagicWeapon||category==WeaponCategory.Sword||category==WeaponCategory.Axe;
            if(!supported){foreach(var r in ribbons)r.enabled=false;return;}
            bool book=category==WeaponCategory.Grimoire,axe=category==WeaponCategory.Axe,casting=view.HasCastingView;
            tint=book?new Color(.72f,.30f,1,.85f):casting?new Color(.18f,.85f,1,.85f):axe?new Color(1,.38f,.08f,.8f):new Color(1,.78f,.30f,.8f);
            var moves=weapon!=null&&weapon.attacks.Length>0?weapon.attacks:roster.Current.attacks;
            attack=moves[Mathf.Clamp(view.ActionVariant,0,moves.Length-1)];
            float impact=view.ActionImpact,progress=view.ActionProgress;bool action=view.State==HeroState.Attack||view.State==HeroState.Skill;
            if(action&&view.ActionVersion!=version){version=view.ActionVersion;released=-1;tail=0;}
            bool release=action&&released!=version&&progress>=impact;
            if(release){released=version;tail=casting?.30f:.22f;Burst(combat.StrikeOrigin+transform.forward*.45f,tint,view.ActionVariant>=3?24:book?14:8,view.ActionVariant>=3?1.6f:.7f);}
            tail=Mathf.Max(0,tail-Time.deltaTime);
            var model=view.Visible!=null?view.Visible.GetComponentInChildren<HeroModelView>():null;
            float charge=view.ChargeProgress;
            for(int i=0;i<ribbons.Length;i++)
            {
                var line=ribbons[i];bool hands=i<2;
                line.enabled=hands?casting&&model!=null&&(charge>.05f||action&&progress<impact):tail>0;
                if(!line.enabled)continue;
                float fade=hands?Mathf.Max(charge,.35f):Mathf.Clamp01(tail/(casting?.30f:.22f));
                line.startColor=line.endColor=new Color(tint.r,tint.g,tint.b,tint.a*fade);
                line.startWidth=line.endWidth=hands?.025f+charge*.025f:casting?.04f:i==2?.13f:.045f;
                if(!hands&&!release)continue;
                Vector3 origin=hands?(i==0?model.PrimarySocket.position:model.SecondarySocket.position):combat.StrikeOrigin;
                for(int j=0;j<40;j++)
                {
                    float u=j/39f;Vector3 p;
                    if(hands||casting)
                    {
                        float a=u*Mathf.PI*2+Time.time*(i%2==0?2:-2);float radius=hands?.10f+charge*.13f:.22f+(i-2)*.075f;
                        if(book){float side=u*4;int edge=(int)side;float t=side-edge;Vector2 v=edge==0?new Vector2(-1+2*t,-1):edge==1?new Vector2(1,-1+2*t):edge==2?new Vector2(1-2*t,1):new Vector2(-1,1-2*t);p=transform.right*v.x*radius+Vector3.up*v.y*radius;}
                        else p=(transform.right*Mathf.Cos(a)+Vector3.up*Mathf.Sin(a))*radius;
                        if(!hands)p+=transform.forward*(.45f+(i-2)*.10f);
                    }
                    else
                    {
                        float arc=attack.footprint==MeleeFootprint.Circle?360:Mathf.Min(attack.arc,160);float range=Mathf.Min(attack.range,axe?1.65f:1.45f)*(1-(i-2)*.075f);
                        if(attack.footprint==MeleeFootprint.Line)p=transform.forward*(u*range)+transform.right*(i-3)*.025f;
                        else{float a=Mathf.Lerp(-arc*.5f,arc*.5f,u)*Mathf.Deg2Rad;p=(Quaternion.AngleAxis(a,Vector3.up)*transform.forward)*range;p.y=Mathf.Sin(u*Mathf.PI)*.14f;}
                        if(Physics.Raycast(origin,p.normalized,out var wall,p.magnitude,combat.GeometryMask,QueryTriggerInteraction.Ignore))p=p.normalized*Mathf.Max(0,wall.distance-.04f);
                    }
                    points[i][j]=origin+p;
                }
                line.SetPositions(points[i]);
            }
        }
    }
}
