using UnityEngine;
using UnityEngine.Rendering;
namespace ForjaDasMares
{
    // A fading ribbon built from the weapon's actual base/tip history, never a cone.
    [DefaultExecutionOrder(240)]
    public sealed class WeaponSweepRibbon:MonoBehaviour
    {
        const int Capacity=24;
        readonly Vector3[] baseHistory=new Vector3[Capacity],tipHistory=new Vector3[Capacity],vertices=new Vector3[Capacity*2];
        readonly float[] times=new float[Capacity];readonly Color[] colors=new Color[Capacity*2];
        Mesh mesh;MeshRenderer renderer;Transform holder,visible;HeroPresentation source;HeroCombat combat;
        Vector3 bladeBase,bladeTip;EquipmentDefinition previous;int samples,version=-1;
        Vector3 previousPosition;bool positioned;
        public int SegmentCapacity=>Capacity;
        public bool Visible=>renderer!=null&&renderer.enabled;
        public void Configure(HeroPresentation view,HeroCombat attacks)
        {
            source=view;combat=attacks;var go=new GameObject("MotionWeaponRibbon",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(transform,false);holder=go.transform;
            mesh=new Mesh{name="ReusableWeaponMotionRibbon"};mesh.MarkDynamic();go.GetComponent<MeshFilter>().sharedMesh=mesh;
            renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=Resources.Load<Material>("HeroEnergyRibbon");renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.enabled=false;
            var uv=new Vector2[Capacity*2];var triangles=new int[(Capacity-1)*6];
            for(int i=0;i<Capacity;i++){uv[i*2]=new Vector2(i/(float)(Capacity-1),0);uv[i*2+1]=new Vector2(i/(float)(Capacity-1),1);if(i==Capacity-1)continue;int t=i*6,v=i*2;triangles[t]=v;triangles[t+1]=v+1;triangles[t+2]=v+2;triangles[t+3]=v+1;triangles[t+4]=v+3;triangles[t+5]=v+2;}
            mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;
        }
        void Bind()
        {
            visible=source.Visible;previous=source.EquippedWeapon;samples=0;
            var model=visible!=null?visible.GetComponentInChildren<HeroModelView>():null;
            var gear=model!=null?model.PrimarySocket.GetComponentInChildren<EquipmentMeshView>():null;
            if(gear!=null){holder=gear.transform;var b=gear.GetComponent<MeshFilter>().sharedMesh.bounds;bladeBase=b.center+Vector3.up*b.extents.y*.10f;bladeTip=b.center+Vector3.up*b.extents.y;}
            else{holder=model!=null?model.PrimarySocket:null;bladeBase=Vector3.zero;bladeTip=Vector3.up*.9f;}
        }
        void LateUpdate()
        {
            if(source==null||mesh==null)return;
            if(positioned&&(transform.position-previousPosition).sqrMagnitude>9)samples=0;
            previousPosition=transform.position;positioned=true;
            if(visible!=source.Visible||previous!=source.EquippedWeapon||holder==null)Bind();
            var weapon=source.EquippedWeapon;bool melee=!source.HasCastingView&&(weapon==null||weapon.category==WeaponCategory.Sword||weapon.category==WeaponCategory.Axe||weapon.category==WeaponCategory.Hammer);
            bool action=source.State==HeroState.Attack||source.State==HeroState.Skill;
            if(!melee||source.State==HeroState.Dead||holder==null){samples=0;renderer.enabled=false;return;}
            if(action&&source.ActionVersion!=version){version=source.ActionVersion;samples=0;}
            if(action&&source.ActionProgress>.13f&&source.ActionProgress<.85f)
            {
                for(int i=Capacity-1;i>0;i--){baseHistory[i]=baseHistory[i-1];tipHistory[i]=tipHistory[i-1];times[i]=times[i-1];}
                baseHistory[0]=holder.TransformPoint(bladeBase);tipHistory[0]=holder.TransformPoint(bladeTip);times[0]=Time.time;samples=Mathf.Min(samples+1,Capacity);
                Vector3 origin=transform.position+Vector3.up,delta=tipHistory[0]-origin;
                if(Physics.Raycast(origin,delta.normalized,out var wall,delta.magnitude,combat.GeometryMask,QueryTriggerInteraction.Ignore))tipHistory[0]=origin+delta.normalized*Mathf.Max(0,wall.distance-.025f);
            }
            Color tint=weapon!=null&&weapon.category==WeaponCategory.Axe?new Color(1,.32f,.055f):new Color(1,.80f,.38f);
            float life=source.FuryRemaining>0?.17f:.13f;bool drawn=false;
            for(int i=0;i<Capacity;i++)
            {
                int j=Mathf.Min(i,Mathf.Max(0,samples-1));float fade=i<samples?Mathf.Clamp01(1-(Time.time-times[i])/life):0;
                vertices[i*2]=transform.InverseTransformPoint(baseHistory[j]);vertices[i*2+1]=transform.InverseTransformPoint(tipHistory[j]);
                colors[i*2]=new Color(tint.r,tint.g,tint.b,fade*.15f);colors[i*2+1]=new Color(tint.r,tint.g,tint.b,fade*.85f);drawn|=fade>0;
            }
            mesh.vertices=vertices;mesh.colors=colors;mesh.RecalculateBounds();renderer.enabled=samples>1&&drawn;
        }
        void OnDisable(){samples=0;positioned=false;if(renderer!=null)renderer.enabled=false;}
        void OnDestroy(){if(mesh!=null)Destroy(mesh);}
    }
}
