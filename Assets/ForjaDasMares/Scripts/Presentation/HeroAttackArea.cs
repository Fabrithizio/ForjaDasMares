using UnityEngine;
using UnityEngine.Rendering;
namespace ForjaDasMares
{
    [DefaultExecutionOrder(70)]public sealed class HeroAttackArea:MonoBehaviour
    {
        HeroPresentation presentation;HeroCombat combat;HeroRoster roster;Mesh mesh;MeshRenderer view;Material material;
        readonly Vector3[] vertices=new Vector3[35];readonly int[] triangles=new int[99];int version=-1;AttackDefinition previous;Vector3 previousOrigin;float previousYaw;
        public bool Visible=>view!=null&&view.enabled;
        public void Configure(HeroPresentation source,HeroCombat attacks,HeroRoster heroes)
        {
            presentation=source;combat=attacks;roster=heroes;if(view!=null)return;
            var go=new GameObject("AttackFootprint",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(transform,false);mesh=new Mesh{name="ReusableAttackFootprint"};mesh.MarkDynamic();go.GetComponent<MeshFilter>().sharedMesh=mesh;
            view=go.GetComponent<MeshRenderer>();view.shadowCastingMode=ShadowCastingMode.Off;view.receiveShadows=false;
            material=roster.Current.attackAreaMaterial!=null?new Material(roster.Current.attackAreaMaterial):new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.SetFloat("_Surface",1);material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);material.SetFloat("_ZWrite",0);material.SetFloat("_Cull",(float)CullMode.Off);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;view.sharedMaterial=material;view.enabled=false;
        }
        void LateUpdate()
        {
            if(view==null||presentation==null)return;bool skill=presentation.State==HeroState.Skill;
            if(!roster.Health.IsAlive||!skill&&presentation.State!=HeroState.Attack){view.enabled=false;return;}
            var weapon=presentation.EquippedWeapon;var attacks=weapon!=null?weapon.attacks:roster.Current.attacks;var ability=weapon!=null&&weapon.ability!=null?weapon.ability:roster.Current.special;
            var attack=skill?ability?.hit:attacks[presentation.ActionVariant>=3?0:Mathf.Clamp(presentation.ActionVariant,0,attacks.Length-1)];
            if(attack==null||attack.shape==AttackShape.Projectile||skill&&ability.effect==AbilityEffect.Trap){view.enabled=false;return;}
            float range=skill&&ability.effect!=AbilityEffect.WeaponStrike?ability.radius:attack.range,arc=skill&&ability.effect!=AbilityEffect.WeaponStrike?360:attack.arc;
            if(!skill&&presentation.ActionVariant==3&&roster.Current.onlineFlow!=null){range*=Mathf.Max(1,roster.Current.onlineFlow.heavyRangeMultiplier);arc=Mathf.Max(160,arc);}
            var origin=combat.StrikeOrigin;var facing=transform.forward;float yaw=transform.eulerAngles.y;
            if(version!=presentation.ActionVersion||previous!=attack||(origin-previousOrigin).sqrMagnitude>.01f||Mathf.Abs(Mathf.DeltaAngle(previousYaw,yaw))>1)
            {Build(attack,range,arc,origin,facing);version=presentation.ActionVersion;previous=attack;previousOrigin=origin;previousYaw=yaw;}
            view.transform.position=new Vector3(origin.x,transform.position.y+.045f,origin.z);view.transform.rotation=Quaternion.Euler(0,yaw,0);view.enabled=true;
            material.SetColor("_BaseColor",new Color(.15f,.75f,.9f,Mathf.Lerp(.20f,.05f,presentation.ActionProgress)));
        }
        void Build(AttackDefinition attack,float range,float arc,Vector3 origin,Vector3 facing)
        {
            bool line=attack.footprint==MeleeFootprint.Line&&arc<359.9f;int used;
            if(line)
            {
                vertices[0]=new Vector3(-attack.width*.5f,0,0);vertices[1]=new Vector3(-attack.width*.5f,0,range);vertices[2]=new Vector3(attack.width*.5f,0,range);vertices[3]=new Vector3(attack.width*.5f,0,0);used=4;
                triangles[0]=0;triangles[1]=1;triangles[2]=2;triangles[3]=0;triangles[4]=2;triangles[5]=3;
            }
            else
            {
                if(attack.footprint==MeleeFootprint.Circle)arc=360;vertices[0]=Vector3.zero;used=35;
                for(int i=0;i<34;i++){float angle=Mathf.Lerp(-arc*.5f,arc*.5f,i/33f)*Mathf.Deg2Rad;vertices[i+1]=new Vector3(Mathf.Sin(angle)*range,0,Mathf.Cos(angle)*range);if(i<33){triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=i+2;}}
            }
            var rotation=Quaternion.LookRotation(facing);
            for(int i=0;i<used;i++)if(vertices[i].sqrMagnitude>.0001f){Vector3 delta=rotation*vertices[i];float length=delta.magnitude;if(Physics.Raycast(origin,delta/length,out var hit,length,combat.GeometryMask,QueryTriggerInteraction.Ignore))vertices[i]*=Mathf.Max(0,hit.distance-.03f)/length;}
            mesh.Clear();mesh.SetVertices(vertices,0,used);mesh.SetTriangles(triangles,0,line?6:99,0);mesh.RecalculateBounds();
        }
        void OnDisable(){if(view!=null)view.enabled=false;}
        void OnDestroy(){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
    }
}
