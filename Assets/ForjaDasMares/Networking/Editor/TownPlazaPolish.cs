using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace ForjaDasMares.Networking.Editor
{
    public static class TownPlazaPolish
    {
        const string Data="Assets/ForjaDasMares/Data/World/Town/Surroundings/";
        const string Kit="Assets/ForjaDasMares/Art/World/SettlementKit/";
        [MenuItem("Forja/World/Polir praca e caminhos")]
        public static void Apply()
        {
            var scene=EditorSceneManager.OpenScene(TownInstaller.ScenePath);
            var env=GameObject.Find("TownEnvironment").transform;
            var hub=UnityEngine.Object.FindFirstObjectByType<TownHub>();
            var world=UnityEngine.Object.FindFirstObjectByType<RegionalWorld>();
            var site=env.Find("AncientMemorySite");
            var terminal=site.Find("MemorySiteConsole").GetComponent<WorldClickTarget>();
            foreach(Transform child in site.Cast<Transform>().ToArray())if(child!=terminal.transform)child.gameObject.SetActive(false);
            var shrine=site.Find("SantuarioDaMemoria");
            if(shrine==null)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Kit+"Altar_antigo.prefab");
                shrine=((GameObject)PrefabUtility.InstantiatePrefab(prefab,site)).transform;shrine.name="SantuarioDaMemoria";
            }
            // Existing navigation and quest object are kept; the old metal assembly is not rendered.
            shrine.gameObject.SetActive(true);shrine.localPosition=Vector3.zero;
            terminal.transform.localPosition=new Vector3(0,.65f,-.4f);
            terminal.Configure("Examinar o santuário da memória",2.8f);
            for(int i=terminal.Used.GetPersistentEventCount()-1;i>=0;i--)UnityEventTools.RemovePersistentListener(terminal.Used,i);
            UnityEventTools.AddStringPersistentListener(terminal.Used,hub.Speak,"As runas brilham quando você se aproxima. Entre as pedras há um pequeno cristal que pulsa ao ritmo do sino.");
            var brazier=env.GetComponentsInChildren<LODGroup>().FirstOrDefault(g=>g.name.StartsWith("CoalBrazier"));
            if(brazier!=null)brazier.transform.position=new Vector3(-16,0,8);
            var rest=GameObject.Find("RestFountain").transform;
            for(int i=0;i<2;i++)
            {
                var name="CanteiroFonte_"+i;
                var planter=env.Find(name);
                if(planter==null)
                {
                    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Kit+"Floreira_folhosa.prefab");
                    planter=((GameObject)PrefabUtility.InstantiatePrefab(prefab,env)).transform;
                    planter.name=name;
                }
                planter.position=rest.position+new Vector3(i==0?-2.3f:2.3f,0,1.65f);
                planter.rotation=Quaternion.Euler(0,i==0?25:-25,0);
                foreach(var r in planter.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.Off;
            }
            var floor=GameObject.Find("TownFloor").GetComponent<Collider>();
            var land=GameObject.Find("TownLandscape").GetComponent<MeshFilter>();
            var probe=land.gameObject.AddComponent<MeshCollider>();probe.sharedMesh=land.sharedMesh;
            Physics.SyncTransforms();
            int grounded=0;
            foreach(var tree in env.GetComponentsInChildren<LODGroup>().Where(g=>g.name.StartsWith("Arvore")))
            {
                var t=tree.transform;var p=t.position;var ray=new Ray(new Vector3(p.x,100,p.z),Vector3.down);
                float ground=float.NegativeInfinity;
                if(floor.Raycast(ray,out var hit,200))ground=hit.point.y;
                if(probe.Raycast(ray,out hit,200))ground=Mathf.Max(ground,hit.point.y);
                if(float.IsNegativeInfinity(ground))continue;
                var lod=tree.GetLODs()[0];float baseY=lod.renderers.Min(r=>r.bounds.min.y);
                t.position+=Vector3.up*(ground-baseY-.035f);grounded++;
            }
            UnityEngine.Object.DestroyImmediate(probe);
            SmoothPaths(GameObject.Find("CityPaths").GetComponent<MeshFilter>());
            RenderSettings.ambientSkyColor=new Color(.70f,.79f,.88f);
            RenderSettings.ambientEquatorColor=new Color(.49f,.54f,.50f);
            RenderSettings.ambientGroundColor=new Color(.30f,.29f,.25f);
            var sun=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(l=>l.type==LightType.Directional);
            if(sun!=null){sun.color=new Color(1,.94f,.84f);sun.intensity=1.22f;sun.transform.rotation=Quaternion.Euler(52,-28,0);}
            var volume=GameObject.Find("TownColorGrade")?.GetComponent<Volume>();
            if(volume==null)volume=new GameObject("TownColorGrade").AddComponent<Volume>();
            volume.isGlobal=true;volume.priority=2;
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Data+"TownColorGrade.asset");
            if(profile==null){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Data+"TownColorGrade.asset");}
            if(!profile.TryGet<ColorAdjustments>(out var grade)){grade=profile.Add<ColorAdjustments>(true);AssetDatabase.AddObjectToAsset(grade,profile);}
            grade.postExposure.Override(.08f);grade.saturation.Override(-6);grade.contrast.Override(5);EditorUtility.SetDirty(grade);volume.sharedProfile=profile;
            Physics.SyncTransforms();
            var surface=UnityEngine.Object.FindFirstObjectByType<NavMeshSurface>();surface.BuildNavMesh();
            if(!NavMesh.SamplePosition(world.Definition.refuge,out var start,1,NavMesh.AllAreas))throw new InvalidOperationException("Refugio sem navegação");
            foreach(var target in UnityEngine.Object.FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None))
            {
                var path=new NavMeshPath();
                if(!NavMesh.SamplePosition(target.Approach,out var end,2.5f,NavMesh.AllAreas)||!NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)||path.status!=NavMeshPathStatus.PathComplete)throw new InvalidOperationException("Serviço desconectado: "+target.name);
            }
            var stable=AssetDatabase.LoadAssetAtPath<NavMeshData>("Assets/ForjaDasMares/Data/World/Town/Navigation.asset");
            EditorUtility.CopySerialized(surface.navMeshData,stable);surface.RemoveData();surface.navMeshData=stable;surface.AddData();EditorUtility.SetDirty(stable);
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log($"TOWN_PLAZA_READY grounded={grounded} noExtraLights=true");
        }
        static void SmoothPaths(MeshFilter filter)
        {
            var source=AssetDatabase.LoadAssetAtPath<Mesh>(Data+"CityPathsClean.asset");var vertices=source.vertices;var triangles=source.triangles;
            var welded=new Dictionary<Vector3,int>();var points=new List<Vector3>();var edges=new Dictionary<(int,int),int>();
            var remap=new int[vertices.Length];
            for(int i=0;i<vertices.Length;i++){var p=vertices[i];if(!welded.TryGetValue(p,out int index)){index=points.Count;welded[p]=index;points.Add(p);}remap[i]=index;}
            void Edge(int a,int b){if(a>b)(a,b)=(b,a);edges.TryGetValue((a,b),out var count);edges[(a,b)]=count+1;}
            for(int i=0;i<triangles.Length;i++)triangles[i]=remap[triangles[i]];
            for(int i=0;i<triangles.Length;i+=3){Edge(triangles[i],triangles[i+1]);Edge(triangles[i+1],triangles[i+2]);Edge(triangles[i+2],triangles[i]);}
            var borders=new List<int>[points.Count];
            void Neighbor(int a,int b){(borders[a]??=new List<int>(2)).Add(b);}
            foreach(var pair in edges.Where(e=>e.Value==1)){var (a,b)=pair.Key;Neighbor(a,b);Neighbor(b,a);}
            var smooth=points.ToArray();
            for(int i=0;i<points.Count;i++)if(borders[i]?.Count==2)smooth[i]=Vector3.Lerp(points[i],(points[borders[i][0]]+points[borders[i][1]])*.5f,.48f);
            var mesh=new Mesh{name="CityPathsRounded",vertices=smooth,uv=smooth.Select(p=>new Vector2(p.x,p.z)/7.6f+Vector2.one*.5f).ToArray(),triangles=triangles};
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            var saved=AssetDatabase.LoadAssetAtPath<Mesh>(Data+"CityPathsRounded.asset");
            if(saved==null){AssetDatabase.CreateAsset(mesh,Data+"CityPathsRounded.asset");saved=mesh;}else{EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);}
            filter.sharedMesh=saved;
        }
    }
}
