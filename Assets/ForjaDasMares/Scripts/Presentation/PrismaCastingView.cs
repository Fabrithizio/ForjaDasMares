using UnityEngine;
using UnityEngine.Rendering;
namespace ForjaDasMares
{
    // Visuals only: reads the same presentation frame on authority and replicas.
    // Eight reusable lines, no colliders, damage, materials cloned or per-cast objects.
    [DefaultExecutionOrder(210)]
    public sealed class PrismaCastingView : MonoBehaviour
    {
        [SerializeField] HeroModelView model;
        [SerializeField] Material material;
        HeroPresentation presentation;
        HeroRoster roster;
        readonly LineRenderer[] lines=new LineRenderer[8];
        readonly Vector3[][] points=new Vector3[8][];
        int version=-1;
        float pulse,life,radius;
        Vector3 center;
        Color tint;
        public void Configure(HeroModelView view,Material ribbon){model=view;material=ribbon;}
        public int EffectCapacity=>lines.Length;
        void Start()
        {
            presentation=GetComponentInParent<HeroPresentation>();roster=GetComponentInParent<HeroRoster>();
            if(presentation==null||roster==null||model==null||material==null){enabled=false;return;}
            for(int i=0;i<lines.Length;i++)
            {
                var go=new GameObject("PrismaGlyph_"+i);go.transform.SetParent(transform,false);
                var line=lines[i]=go.AddComponent<LineRenderer>();line.sharedMaterial=material;
                line.useWorldSpace=true;line.loop=true;line.positionCount=i<2?25:49;points[i]=new Vector3[line.positionCount];
                line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
                line.startWidth=line.endWidth=.025f;line.enabled=false;
            }
        }
        void OnEnable(){version=-1;pulse=0;}
        void OnDisable(){pulse=0;foreach(var line in lines)if(line!=null)line.enabled=false;}
        void LateUpdate()
        {
            if(presentation==null)return;
            bool casting=presentation.State==HeroState.Attack||presentation.State==HeroState.Skill;
            bool alive=roster.Health.IsAlive;
            var weapon=presentation.EquippedWeapon;
            var ability=weapon!=null&&weapon.ability!=null?weapon.ability:roster.Current.special;
            var flow=roster.Current.onlineFlow;
            bool super=casting&&presentation.ActionVariant==4;
            bool skill=casting&&presentation.State==HeroState.Skill&&!super;
            bool heavy=casting&&presentation.ActionVariant==3;
            AttackDefinition hit=super?flow?.superAttack:skill?ability?.hit:
                weapon!=null&&weapon.attacks.Length>0?weapon.attacks[Mathf.Clamp(presentation.ActionVariant,0,weapon.attacks.Length-1)]:roster.Current.attacks[0];
            Color color=hit!=null?hit.color:roster.Current.color;
            float charge=presentation.ChargeProgress;
            if(version!=presentation.ActionVersion)
            {
                version=presentation.ActionVersion;
                if(casting&&(skill||super))
                {
                    life=super?.8f:skill?.55f:.38f;pulse=life;
                    center=presentation.transform.position+Vector3.up*.09f;
                    radius=super&&hit!=null?hit.range:skill&&ability!=null?ability.radius:1.15f;
                    // A placed seal must show the actual forward placement, not a false self AoE.
                    if(skill&&ability.effect==AbilityEffect.Trap)center+=presentation.transform.forward*ability.placementDistance;
                    tint=color;
                }
            }
            pulse=alive?Mathf.Max(0,pulse-Time.deltaTime):0;
            float progress=life>0?1-pulse/life:1;
            for(int i=0;i<lines.Length;i++)
            {
                var line=lines[i];if(line==null)continue;
                bool hand=i<2;
                line.enabled=alive&&(hand?(casting||charge>.05f):pulse>0);
                if(!line.enabled)continue;
                line.startColor=line.endColor=hand?color:new Color(tint.r,tint.g,tint.b,(1-progress)*.75f);
                line.startWidth=line.endWidth=hand?.018f+.025f*charge:.025f*(1-progress*.6f);
                Vector3 origin=hand?(i==0?model.PrimarySocket.position:model.SecondarySocket.position):center;
                float size=hand?.065f+(heavy?.10f:0)+.15f*charge+Mathf.Sin(Mathf.Clamp01(presentation.ActionProgress)*Mathf.PI)*.055f:
                    radius*Mathf.Lerp(.25f,1,progress)*(i==2?1:i==3?.78f:.2f);
                if(i>=4)origin+=Quaternion.Euler(0,(i-4)*90+progress*40,0)*Vector3.forward*radius*.6f;
                for(int j=0;j<line.positionCount;j++)
                {
                    float a=j*Mathf.PI*2/line.positionCount+Time.time*(hand?2:0);
                    Vector3 point=hand?model.transform.right*Mathf.Cos(a)+Vector3.up*Mathf.Sin(a):new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                    points[i][j]=origin+point*size;
                }
                line.SetPositions(points[i]);
            }
        }
    }
}
