using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace ForjaDasMares
{
    // Only the locally pointed/auto-aimed target is outlined. No collider or gameplay actor clones.
    [DefaultExecutionOrder(300)]public sealed class TargetOutline:MonoBehaviour
    {
        sealed class Surface{public Renderer source,outline;}
        readonly Dictionary<Health,List<Surface>> cache=new Dictionary<Health,List<Surface>>();
        DesktopRpgControls controls;Health selected;Material material;bool attacking;
        public Health Selected=>selected;
        public bool IsShown=>selected!=null&&selected.IsAlive&&selected.gameObject.activeInHierarchy;
        void Awake(){controls=GetComponent<DesktopRpgControls>();var shader=Resources.Load<Shader>("SelectionOutline");if(shader!=null)material=new Material(shader);}
        void LateUpdate()
        {
            var next=controls!=null?controls.SelectionTarget:null;
            if(next!=null&&(!next.IsAlive||!next.gameObject.activeInHierarchy))next=null;
            if(next!=selected){Hide();selected=next;if(selected!=null&&material!=null&&!cache.ContainsKey(selected))Create(selected);}
            if(material==null||selected==null||!cache.TryGetValue(selected,out var surfaces))return;
            bool firing=controls.SelectionAttacking;
            if(firing!=attacking||material.GetColor("_OutlineColor").a==0){attacking=firing;material.SetColor("_OutlineColor",firing?new Color(1,.38f,.08f,1):new Color(1,.85f,.3f,1));}
            foreach(var item in surfaces)if(item.outline!=null)item.outline.enabled=item.source!=null&&item.source.enabled&&item.source.gameObject.activeInHierarchy;
        }
        void Create(Health hp)
        {
            var list=new List<Surface>();cache.Add(hp,list);
            var presentation=hp.GetComponent<EnemyPresentation>();var root=presentation!=null&&presentation.Model!=null?presentation.Model:hp.transform;
            foreach(var source in root.GetComponentsInChildren<Renderer>(true))
            {
                if(source is ParticleSystemRenderer||source is LineRenderer||source is TrailRenderer||source.name.StartsWith("SelectionOutline"))continue;
                // Models use LOD0/1/2 children. One silhouette is sufficient for one selected target.
                if(source.name.StartsWith("LOD")&&source.name!="LOD0")continue;
                GameObject go=null;Renderer renderer=null;
                if(source is SkinnedMeshRenderer skin&&skin.sharedMesh!=null)
                {go=new GameObject("SelectionOutlineSkin");var copy=go.AddComponent<SkinnedMeshRenderer>();copy.sharedMesh=skin.sharedMesh;copy.bones=skin.bones;copy.rootBone=skin.rootBone;copy.localBounds=skin.localBounds;renderer=copy;}
                else if(source is MeshRenderer&&source.TryGetComponent<MeshFilter>(out var filter)&&filter.sharedMesh!=null)
                {go=new GameObject("SelectionOutlineMesh");go.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;renderer=go.AddComponent<MeshRenderer>();}
                if(go==null)continue;go.layer=source.gameObject.layer;go.transform.SetParent(source.transform,false);
                int slots=source.sharedMaterials.Length;var mats=new Material[Mathf.Max(1,slots)];for(int i=0;i<mats.Length;i++)mats[i]=material;renderer.sharedMaterials=mats;
                renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.lightProbeUsage=LightProbeUsage.Off;renderer.enabled=false;
                list.Add(new Surface{source=source,outline=renderer});
            }
            material.SetColor("_OutlineColor",new Color(1,.85f,.3f,1));attacking=false;
        }
        void Hide(){if(selected!=null&&cache.TryGetValue(selected,out var list))foreach(var item in list)if(item.outline!=null)item.outline.enabled=false;}
        void OnDisable(){Hide();selected=null;}
        void OnDestroy(){foreach(var pair in cache)foreach(var item in pair.Value)if(item.outline!=null)Destroy(item.outline.gameObject);cache.Clear();if(material!=null)Destroy(material);}
    }
}
