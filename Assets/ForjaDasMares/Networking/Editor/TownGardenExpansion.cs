using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.AI;
using Unity.AI.Navigation;
namespace ForjaDasMares.Networking.Editor
{
    public static class TownGardenExpansion
    {
        const string Kit="Assets/ForjaDasMares/Art/World/SettlementKit/";
        const string Data="Assets/ForjaDasMares/Data/World/Town/Surroundings/";
        [MenuItem("Forja/World/Expandir jardim do refugio")]
        public static void Apply()
        {
            var scene=EditorSceneManager.OpenScene(TownInstaller.ScenePath);
            var env=GameObject.Find("TownEnvironment").transform;
            var existing=env.Find("JardimNorte");if(existing!=null){Refine(existing);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return;}
            var root=new GameObject("JardimNorte").transform;root.SetParent(env,false);
            var world=UnityEngine.Object.FindFirstObjectByType<RegionalWorld>();
            var terrain=GameObject.Find("TownLandscape").GetComponent<MeshFilter>();
            var mesh=UnityEngine.Object.Instantiate(terrain.sharedMesh);var vertices=mesh.vertices;
            for(int i=0;i<vertices.Length;i++)
            {
                var p=vertices[i];
                float blend=Mathf.Clamp01((55-Mathf.Abs(p.x))/12)*Mathf.Clamp01((84-p.z)/12)*Mathf.Clamp01((p.z-25)/10);
                vertices[i].y=Mathf.Lerp(p.y,-.35f,blend);
            }
            mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();
            var saved=AssetDatabase.LoadAssetAtPath<Mesh>(Data+"TownLandscapeExpanded.asset");
            if(saved==null){AssetDatabase.CreateAsset(mesh,Data+"TownLandscapeExpanded.asset");saved=mesh;}
            else {EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);}
            terrain.sharedMesh=saved;
            foreach(var t in env.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Entrada_estacao_invadida"||t.name=="Raizes_na_maquina"||t.name=="Protetor_da_praca").ToArray())
                t.gameObject.SetActive(false);
            var floor=GameObject.Find("TownFloor");floor.transform.position=new Vector3(0,-.2f,14);floor.transform.localScale=new Vector3(76,.4f,104);
            var fountain=env.Find("FonteProtetoraMagica");if(fountain!=null)fountain.position=new Vector3(0,0,49);
            var layout=env.Find("TownLayout028");
            var walls=layout.Cast<Transform>().Where(t=>t.name=="TownStoneBoundary").ToArray();
            walls.Single(t=>Mathf.Abs(t.position.x)<1&&t.position.z>30).position=new Vector3(0,1.5f,64);
            foreach(var wall in walls.Where(t=>Mathf.Abs(t.position.x)>35&&Mathf.Abs(t.position.z)<1))
            {
                var extension=UnityEngine.Object.Instantiate(wall.gameObject,root);extension.name="JardimMuralhaLateral";
                extension.transform.position=new Vector3(wall.position.x,wall.position.y,50);
                extension.transform.localScale=new Vector3(1,1,28f/73);
            }
            var meadow=floor.GetComponent<Renderer>().sharedMaterial;meadow.SetColor("_BaseColor",new Color(.61f,.73f,.59f));meadow.mainTextureScale=new Vector2(16,22);EditorUtility.SetDirty(meadow);
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/ForjaDasMares/Art/World/Coast/GrassFantasy032.png");
            importer.maxTextureSize=256;importer.anisoLevel=1;importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
            var paving=GameObject.Find("CityPaths").GetComponent<Renderer>().sharedMaterial;
            // Side walks skirt the central building and meet the garden plaza.
            Strip(root,"AlamedaOeste",-10,33,5,25,paving);
            Strip(root,"AlamedaLeste",10,33,5,25,paving);
            Strip(root,"PracaJardim",0,49,33,8,paving);
            Strip(root,"PasseioJardim",0,58,28,5,paving);
            foreach(int side in new[]{-1,1})
            {
                Place(root,"Arvore_copa_aberta",new Vector3(side*20,0,49),1.8f);
                Place(root,"Arvore_luminosa",new Vector3(side*24,0,59),1.35f);
                Place(root,"Banco_de_madeira",new Vector3(side*12,0,54),1.1f);
                Place(root,"Floreira_folhosa",new Vector3(side*15,0,54),1.2f);
                Place(root,"Arbusto_florido",new Vector3(side*18,0,53),1.35f);
                Place(root,"Arbusto_florido",new Vector3(side*20,0,56),1.25f);
            }
            Refine(root);
            // Small foliage no longer participates in the shadow pass; structural landmarks still cast shadows.
            int shadows=0;
            foreach(var group in env.GetComponentsInChildren<LODGroup>())
            {
                if(!group.name.StartsWith("Arbusto")&&!group.name.StartsWith("Tufo")&&!group.name.StartsWith("Flores")&&!group.name.StartsWith("Samambaia"))continue;
                foreach(var r in group.GetComponentsInChildren<Renderer>()){if(r.shadowCastingMode!=ShadowCastingMode.Off){r.shadowCastingMode=ShadowCastingMode.Off;shadows++;}}
            }
            world.Definition.bounds=new Bounds(new Vector3(0,0,14),new Vector3(76,30,104));EditorUtility.SetDirty(world.Definition);
            var surface=UnityEngine.Object.FindFirstObjectByType<NavMeshSurface>();surface.BuildNavMesh();
            if(!NavMesh.SamplePosition(world.Definition.refuge,out var a,1,NavMesh.AllAreas)||!NavMesh.SamplePosition(new Vector3(10,0,58),out var b,1,NavMesh.AllAreas))throw new InvalidOperationException("Jardim sem superficie navegavel.");
            var route=new NavMeshPath();if(!NavMesh.CalculatePath(a.position,b.position,NavMesh.AllAreas,route)||route.status!=NavMeshPathStatus.PathComplete)throw new InvalidOperationException("Jardim desconectado.");
            var nav=AssetDatabase.LoadAssetAtPath<NavMeshData>("Assets/ForjaDasMares/Data/World/Town/Navigation.asset");EditorUtility.CopySerialized(surface.navMeshData,nav);surface.RemoveData();surface.navMeshData=nav;surface.AddData();EditorUtility.SetDirty(nav);
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log($"TOWN_GARDEN expanded=76x104 gardenReachable=true foliageShadowRenderersDisabled={shadows}");
        }
        static void Refine(Transform root)
        {
            foreach(var group in root.GetComponentsInChildren<LODGroup>())
            {
                var lods=group.GetLODs();if(lods.Length<2)continue;
                lods[0].screenRelativeTransitionHeight=.46f;group.SetLODs(lods);
            }
            foreach(var name in new[]{"AlamedaOeste","AlamedaLeste"})
            {
                var path=root.Find(name);var scale=path.localScale;scale.z=24;path.localScale=scale;
                var p=path.localPosition;p.z=33;path.localPosition=p;
                var m=path.GetComponent<Renderer>().sharedMaterial;m.mainTextureScale=new Vector2(scale.x/7.6f,scale.z/7.6f);EditorUtility.SetDirty(m);
            }
        }
        static void Place(Transform root,string asset,Vector3 position,float scale)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Kit+asset+".prefab");
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);go.transform.position=position;go.transform.localScale=Vector3.one*scale;
            if(asset.StartsWith("Arvore")){var c=go.AddComponent<CapsuleCollider>();c.radius=.45f;c.height=2;c.center=Vector3.up;}
            else if(asset.StartsWith("Banco")){var c=go.AddComponent<BoxCollider>();c.center=Vector3.up*.4f;c.size=new Vector3(2.2f,.8f,.65f);}
        }
        static void Strip(Transform root,string name,float x,float z,float width,float length,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.layer=2;go.transform.SetParent(root,false);go.transform.position=new Vector3(x,-.016f,z);go.transform.localScale=new Vector3(width,.065f,length);
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            string path=Data+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(material);AssetDatabase.CreateAsset(m,path);}
            m.mainTextureScale=new Vector2(width/7.6f,length/7.6f);EditorUtility.SetDirty(m);
            var r=go.GetComponent<Renderer>();r.sharedMaterial=m;r.shadowCastingMode=ShadowCastingMode.Off;
        }
    }
}
