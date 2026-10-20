using UnityEngine;
namespace ForjaDasMares
{
    public sealed class CombatAilments:MonoBehaviour
    {
        Health health,bleedSource,burnSource;float bleed,burn,fracture,tick;int bleedDamage,burnDamage,replicaBits;bool replica;LineRenderer halo;
        public bool Burning=>replica?(replicaBits&2)!=0:burn>0;public float DefenseScale=>fracture>0?.75f:1;
        public int Bits=>replica?replicaBits:(bleed>0?1:0)|(burn>0?2:0)|(fracture>0?4:0);
        void Awake()=>health=GetComponent<Health>();
        public void ApplyBleed(Health source,int damage){if(health==null||!health.IsAlive||source==null)return;replica=false;bleedSource=source;bleed=2.5f;bleedDamage=Mathf.Clamp(damage,1,15);}
        public void ApplyBurn(Health source,int damage){if(health==null||!health.IsAlive||source==null)return;replica=false;burnSource=source;burn=2.5f;burnDamage=Mathf.Clamp(damage,1,15);}
        public void Fracture(){if(health!=null&&health.IsAlive){replica=false;fracture=3;}}
        public void Replica(int bits){replica=true;replicaBits=bits&7;}
        public void Clear(){bleed=burn=fracture=tick=0;bleedSource=burnSource=null;replicaBits=0;replica=false;}
        void OnDisable(){Clear();if(halo!=null)halo.enabled=false;}
        void LateUpdate()
        {
            int bits=Bits;if(bits!=0&&halo==null)
            {
                var feel=GetComponent<EnemyActor>()?.Profile?.feedback;var material=feel!=null?feel.trailMaterial:null;if(material==null)return;
                var go=new GameObject("StatusHalo");go.transform.SetParent(transform,false);halo=go.AddComponent<LineRenderer>();halo.useWorldSpace=false;halo.sharedMaterial=material;halo.positionCount=25;halo.startWidth=halo.endWidth=.055f;halo.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;halo.receiveShadows=false;
                for(int i=0;i<25;i++){float angle=i*Mathf.PI/12;halo.SetPosition(i,new Vector3(Mathf.Sin(angle)*.55f,.09f,Mathf.Cos(angle)*.55f));}
            }
            if(halo!=null){halo.enabled=bits!=0&&health.IsAlive;var color=(bits&2)!=0?new Color(1,.32f,.05f,.8f):(bits&1)!=0?new Color(.85f,.08f,.15f,.8f):new Color(.3f,.7f,1,.8f);halo.startColor=halo.endColor=color;}
        }
        bool CanTick(Health source)=>source!=null&&source.IsAlive&&source.gameObject.activeInHierarchy&&(source.GetComponent<HeroCombat>()==null||source.GetComponent<HeroCombat>().enabled);
        void Update()=>Simulate(Time.deltaTime);
        public void Simulate(float delta)
        {
            if(replica)return;if(health==null||!health.IsAlive){Clear();return;}delta=Mathf.Max(0,delta);tick+=delta;
            int ticks=Mathf.Min(6,Mathf.FloorToInt(tick/.5f));tick-=ticks*.5f;for(int i=0;i<ticks;i++){if(bleed>i*.5f&&CanTick(bleedSource))health.ApplyDamage(bleedDamage);if(burn>i*.5f&&CanTick(burnSource)&&health.IsAlive)health.ApplyDamage(burnDamage);}
            bleed=Mathf.Max(0,bleed-delta);burn=Mathf.Max(0,burn-delta);fracture=Mathf.Max(0,fracture-delta);
        }
    }
}
