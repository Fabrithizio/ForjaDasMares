using UnityEngine;
using UnityEngine.Rendering;
namespace ForjaDasMares
{
    // Fixed visual pool: no colliders, extra damage or camera rotation.
    public sealed class HeroPowerFeedback : MonoBehaviour
    {
        [SerializeField] HeroCombat combat;
        LineRenderer[] lines;
        [SerializeField] Material material;
        MaterialPropertyBlock properties;
        int version=-1;
        bool emitted;
        float remaining, duration;
        Vector3 center, forward;
        int effect;
        float effectRange;
        public void Configure(HeroCombat source,Material ribbon) {combat=source;material=ribbon;}
        void Awake()
        {
            if(material==null){Debug.LogError("Hero power feedback requires its ribbon material.",this);enabled=false;return;}
            properties=new MaterialPropertyBlock();
            lines=new LineRenderer[10];
            for(int i=0;i<lines.Length;i++)
            {
                var item=new GameObject("CombatRibbon_"+i);item.transform.SetParent(transform,false);
                var line=item.AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;
                line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.positionCount=i<2?49:2;line.enabled=false;
                line.startWidth=line.endWidth=.06f;line.numCapVertices=2;lines[i]=line;
            }
        }
        void LateUpdate()
        {
            if(combat==null)return;
            if(version!=combat.ActionVersion){version=combat.ActionVersion;emitted=false;}
            if(!emitted&&combat.IsBusy&&combat.NormalizedTime>=combat.ImpactTime)
            {
                emitted=true;effect=combat.IsSuper?2:combat.IsHeavy?1:0;
                effectRange=combat.EffectRange;
                if(!combat.IsSuper&&(combat.IsUsingSpecial||combat.BasicAttack!=null&&combat.BasicAttack.shape==AttackShape.Projectile))return;
                center=transform.position+Vector3.up*.12f;forward=transform.forward;duration=effect==2?.7f:effect==1?.35f:.16f;remaining=duration;
            }
            remaining=Mathf.Max(0,remaining-Time.deltaTime);float t=duration>0?1-remaining/duration:1;
            Color color=effect==2?new Color(1,.75f,.18f):effect==1?new Color(1,.42f,.08f):new Color(.9f,.86f,.65f);
            color*=Mathf.Lerp(1,.12f,t);color.a=1;
            for(int i=0;i<lines.Length;i++)
            {
                var line=lines[i];line.enabled=remaining>0&&(effect==2||i==0);if(!line.enabled)continue;
                properties.SetColor("_BaseColor",color);line.SetPropertyBlock(properties);line.startWidth=line.endWidth=(effect==2?.09f:.055f)*(1-t*.7f);
                if(i<2)
                {
                    float radius=effect==2?Mathf.Lerp(.6f,effectRange,t)*(i==0?1:.75f):Mathf.Lerp(.6f,effectRange,t);
                    float arc=effect==2?360:effect==1?180:120;
                    for(int p=0;p<49;p++){float angle=Mathf.Lerp(-arc*.5f,arc*.5f,p/48f);line.SetPosition(p,center+Quaternion.Euler(0,angle,0)*forward*radius);}
                }
                else
                {
                    Vector3 ray=Quaternion.Euler(0,(i-2)*45,0)*forward;
                    Vector3 tip=center+ray*Mathf.Lerp(.4f,4,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*.65f;
                    line.SetPosition(0,tip);line.SetPosition(1,tip+ray*.35f+Vector3.up*.15f);
                }
            }
        }
        void OnDisable(){remaining=0;if(lines!=null)foreach(var line in lines)line.enabled=false;}
    }
}
