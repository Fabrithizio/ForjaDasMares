using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Unity.AI.Navigation;
namespace ForjaDasMares.Networking.Editor
{
    public static class CityLifeInstaller
    {
        const string Art="Assets/ForjaDasMares/Art/World/CityLifeKit/";
        const string Source="ArtSource/CityLifeKit/Refinado/Modelos/";
        static readonly string[] Names={"Portao_da_cidade","Folha_portao","Entrada_estacao_invadida","Protetor_da_praca","Pavilhao_de_eventos","Quadro_de_avisos","Bandeirolas_de_rua","Raizes_na_maquina","Arco_do_jardim","Banca_de_frutas","Banca_do_padeiro","Banca_de_ervas","Banca_de_ferramentas","Banca_de_tecidos","Banca_de_reliquias","Placa_da_padaria","Placa_de_ervas","Placa_da_oficina"};
        [MenuItem("Forja/World/Integrar cidade viva refinada")]
        public static void Apply()
        {
            Prepare();
            var scene=EditorSceneManager.OpenScene(TownInstaller.ScenePath);
            var env=GameObject.Find("TownEnvironment").transform;
            var previous=env.Find("CityLife029");if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
            var root=new GameObject("CityLife029").transform;root.SetParent(env,false);
            var layout=env.Find("TownLayout028");if(layout==null)throw new InvalidOperationException("Requires existing 0.28 layout");
            foreach(Transform child in layout.Cast<Transform>().ToArray())
                if((child.name=="Altar_antigo"||child.name=="Console_antigo"||child.name=="Marco_antigo")&&child.position.z>8&&Mathf.Abs(child.position.x)<4 || child.name=="Marco_de_transporte")UnityEngine.Object.DestroyImmediate(child.gameObject);
            foreach(var tent in env.GetComponentsInChildren<LODGroup>().Where(g=>g.name.StartsWith("RefugeTent")&&g.transform.position.x>0).ToArray())UnityEngine.Object.DestroyImmediate(tent.gameObject);
            foreach(var old in env.GetComponentsInChildren<LODGroup>().Where(g=>g.name=="Banca_de_mercado").ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
            Place(root,"Protetor_da_praca",new Vector3(0,0,12),1,180);
            Place(root,"Portao_da_cidade",new Vector3(0,0,-35),1,180);
            Place(root,"Entrada_estacao_invadida",new Vector3(25,0,-7),1,180);
            Place(root,"Banca_de_frutas",new Vector3(-19,0,-18),1.1f,180);
            Place(root,"Banca_do_padeiro",new Vector3(-27,0,-10),1,90);
            Place(root,"Banca_de_ervas",new Vector3(-23,0,2),1,180);
            Place(root,"Banca_de_ferramentas",new Vector3(24,0,3),1,180);
            Place(root,"Banca_de_tecidos",new Vector3(14,0,24),1,180);
            Place(root,"Banca_de_reliquias",new Vector3(-13,0,25),1,180);
            Place(root,"Pavilhao_de_eventos",new Vector3(-23,0,-28),1,180);
            Place(root,"Quadro_de_avisos",new Vector3(-18,0,-25),1,25);
            Place(root,"Bandeirolas_de_rua",new Vector3(0,0,-23),1,0);
            Place(root,"Raizes_na_maquina",new Vector3(30,0,26),1,0);
            Place(root,"Arco_do_jardim",new Vector3(-23,0,20),1,0);
            Place(root,"Placa_da_padaria",new Vector3(-25.5f,1.25f,-10),.55f,90);
            Place(root,"Placa_de_ervas",new Vector3(-23,1.25f,.75f),.55f,180);
            Place(root,"Placa_da_oficina",new Vector3(24,1.25f,1.75f),.55f,180);
            Reserve(root,"eventos-praca-oeste",TownSpaceKind.Event,new Vector3(-23,0,-28),new Vector2(7,6));
            Reserve(root,"moradores-praca-sul",TownSpaceKind.Resident,new Vector3(10,0,-18),new Vector2(5,5));
            Reserve(root,"expansao-leste",TownSpaceKind.FutureExpansion,new Vector3(39,0,18),new Vector2(10,14));
            DressGround(root);
            ExitMarker(root,UnityEngine.Object.FindFirstObjectByType<RegionalWorld>());
            var merchant=env.GetComponentsInChildren<WorldClickTarget>().FirstOrDefault(t=>t.name.StartsWith("Téo"));if(merchant!=null)merchant.transform.position=new Vector3(-19,0,-21);
            int trees=0;
            foreach(var group in env.GetComponentsInChildren<LODGroup>())
                if(group.name.StartsWith("Arvore_")||group.name.StartsWith("Arbusto_"))
                {
                    var lods=group.GetLODs();if(lods.Length!=2)continue;
                    lods[0].screenRelativeTransitionHeight=group.name.StartsWith("Arvore_")?.45f:.32f;
                    lods[1].screenRelativeTransitionHeight=.035f;group.SetLODs(lods);trees++;
                }
            Rebuild();EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Audit();Debug.Log("CITY_LIFE_READY prefabs=18 marketVariants=6 reservedSpaces=3 vegetationLodAdjusted="+trees);
        }
        static void Prepare()
        {
            Directory.CreateDirectory(Art);
            foreach(var name in Names)foreach(var level in new[]{"LOD0","LOD1"})File.Copy(Source+name+"_"+level+".fbx",Art+name+"_"+level+".fbx",true);
            File.Copy(Source+"Paleta_cidade_viva.png",Art+"Paleta_cidade_viva.png",true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var texture=(TextureImporter)AssetImporter.GetAtPath(Art+"Paleta_cidade_viva.png");texture.mipmapEnabled=true;texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;texture.npotScale=TextureImporterNPOTScale.None;texture.maxTextureSize=1024;texture.alphaSource=TextureImporterAlphaSource.None;texture.textureCompression=TextureImporterCompression.CompressedHQ;texture.SaveAndReimport();
            string materialPath=Art+"CityLifePalette.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,materialPath);}
            mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Paleta_cidade_viva.png");mat.color=Color.white;mat.SetFloat("_Smoothness",.1f);mat.enableInstancing=true;EditorUtility.SetDirty(mat);
            foreach(var name in Names)
            {
                var root=new GameObject(name);root.layer=9;var renderers=new Renderer[2];
                for(int i=0;i<2;i++)
                {
                    string path=Art+name+"_LOD"+i+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                    importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importAnimation=false;importer.importLights=false;importer.importCameras=false;importer.isReadable=false;importer.SaveAndReimport();
                    var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));instance.name="LOD"+i;instance.transform.SetParent(root.transform,false);
                    var meshes=instance.GetComponentsInChildren<MeshRenderer>();if(meshes.Length!=1)throw new InvalidOperationException("Expected one mesh: "+path);
                    renderers[i]=meshes[0];meshes[0].sharedMaterial=mat;foreach(var t in instance.GetComponentsInChildren<Transform>())t.gameObject.layer=9;
                }
                var group=root.AddComponent<LODGroup>();group.SetLODs(new[]{new LOD(.33f,new[]{renderers[0]}),new LOD(.035f,new[]{renderers[1]})});group.RecalculateBounds();
                if(name.StartsWith("Banca_"))Box(root,new Vector3(0,.9f,0),new Vector3(2.9f,1.8f,1.5f));
                else if(name=="Portao_da_cidade")foreach(int side in new[]{-1,1})Box(root,new Vector3(side*3.7f,2.7f,0),new Vector3(1.7f,5.4f,2));
                else if(name=="Entrada_estacao_invadida")foreach(int side in new[]{-1,1})Box(root,new Vector3(side*4.2f,2.7f,0),new Vector3(2.3f,5.4f,2.5f));
                else if(name=="Protetor_da_praca")Box(root,new Vector3(0,.4f,0),new Vector3(5,.8f,5));
                else if(name=="Pavilhao_de_eventos"){Box(root,new Vector3(0,.14f,0),new Vector3(5.4f,.28f,3.9f));foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})Box(root,new Vector3(x*2.5f,1.9f,z*1.8f),new Vector3(.35f,3.8f,.35f));}
                else if(name=="Quadro_de_avisos")Box(root,new Vector3(0,1.3f,0),new Vector3(2.4f,2.6f,.5f));
                else if(name=="Bandeirolas_de_rua")foreach(int side in new[]{-1,1})Box(root,new Vector3(side*3.5f,1.9f,0),new Vector3(.25f,3.8f,.25f));
                else if(name=="Arco_do_jardim")foreach(int side in new[]{-1,1})Box(root,new Vector3(side*1.6f,1.1f,0),new Vector3(.4f,2.2f,.4f));
                else if(name=="Raizes_na_maquina")Box(root,new Vector3(0,.8f,0),new Vector3(2.1f,1.6f,1.5f));
                PrefabUtility.SaveAsPrefabAsset(root,Art+name+".prefab");UnityEngine.Object.DestroyImmediate(root);
            }
        }
        static void Box(GameObject root,Vector3 centre,Vector3 size){var box=root.AddComponent<BoxCollider>();box.center=centre;box.size=size;}
        static GameObject Place(Transform root,string name,Vector3 point,float scale,float yaw)
        {var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+name+".prefab"));go.transform.SetParent(root,false);go.transform.SetPositionAndRotation(point,Quaternion.Euler(0,yaw,0));go.transform.localScale=Vector3.one*scale;return go;}
        static void Reserve(Transform root,string id,TownSpaceKind kind,Vector3 p,Vector2 size)
        {var go=new GameObject("Reserva_"+id);go.transform.SetParent(root,false);go.transform.position=p;go.AddComponent<TownActivitySpace>().Configure(id,kind,size);}
        static void DressGround(Transform root)
        {
            // Presentation only: keep the existing floor collider and navigation unchanged.
            string folder="Assets/ForjaDasMares/Data/World/Town/Surroundings/";
            var meadow=AssetDatabase.LoadAssetAtPath<Material>(folder+"CityMeadow.mat");
            if(meadow==null){meadow=new Material(AssetDatabase.LoadAssetAtPath<Material>(folder+"Meadow.mat"));AssetDatabase.CreateAsset(meadow,folder+"CityMeadow.mat");}
            meadow.color=new Color(.95f,1,.85f);meadow.mainTextureScale=Vector2.one*12;EditorUtility.SetDirty(meadow);
            GameObject.Find("TownFloor").GetComponent<Renderer>().sharedMaterial=meadow;
            var paving=AssetDatabase.LoadAssetAtPath<Material>(folder+"CityPaths.mat");
            if(paving==null){paving=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Data/World/Town/Paving.mat"));AssetDatabase.CreateAsset(paving,folder+"CityPaths.mat");}
            paving.mainTextureScale=Vector2.one;paving.mainTextureOffset=Vector2.zero;EditorUtility.SetDirty(paving);
            var vertices=new List<Vector3>();var indices=new List<int>();var uv=new List<Vector2>();
            void Polygon(Vector2[] points)
            {float signed=0;for(int j=0;j<points.Length;j++){var a=points[j];var b=points[(j+1)%points.Length];signed+=a.x*b.y-b.x*a.y;}if(signed<0)Array.Reverse(points);int start=vertices.Count;foreach(var p in points){vertices.Add(new Vector3(p.x,.023f,p.y));uv.Add(p/7.6f+Vector2.one*.5f);}for(int i=1;i<points.Length-1;i++){indices.Add(start);indices.Add(start+i+1);indices.Add(start+i);}}
            void Path(Vector2 a,Vector2 b,float width)
            {var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;Polygon(new[]{a+n,b+n,b-n,a-n});}
            Polygon(Enumerable.Range(0,48).Select(i=>new Vector2(Mathf.Cos(i*Mathf.PI*2/48)*10.5f,7+Mathf.Sin(i*Mathf.PI*2/48)*10.5f)).ToArray());
            Path(new Vector2(0,-2),new Vector2(0,-38),7);
            Path(new Vector2(-7,7),new Vector2(-24,5),5);Path(new Vector2(7,7),new Vector2(24,5),5);
            Path(new Vector2(0,15),new Vector2(0,22),5);
            Path(new Vector2(0,-17),new Vector2(-19,-23),5);Path(new Vector2(-19,-23),new Vector2(-23,-29),4);
            Path(new Vector2(0,-17),new Vector2(19,-20),5);Path(new Vector2(10,1),new Vector2(23,-14),5);
            Path(new Vector2(-24,5),new Vector2(-20,21),4);Path(new Vector2(-20,21),new Vector2(18,21),4);
            var mesh=new Mesh{name="CityPaths"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            string path=folder+"CityPaths.asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved==null){AssetDatabase.CreateAsset(mesh,path);saved=mesh;}else{EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);}
            var go=new GameObject("CityPaths",typeof(MeshFilter),typeof(MeshRenderer));go.layer=2;go.transform.SetParent(root,false);go.GetComponent<MeshFilter>().sharedMesh=saved;go.GetComponent<MeshRenderer>().sharedMaterial=paving;go.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
        }
        static void ExitMarker(Transform root,RegionalWorld world)
        {
            var mesh=new Mesh{name="CityExitArrow"};
            mesh.vertices=new[]{new Vector3(0,0,-1.2f),new Vector3(1,0,0),new Vector3(.32f,0,0),new Vector3(.32f,0,1.2f),new Vector3(-.32f,0,1.2f),new Vector3(-.32f,0,0),new Vector3(-1,0,0)};
            mesh.triangles=new[]{0,6,1,2,5,4,2,4,3};mesh.uv=Enumerable.Repeat(new Vector2(9.5f/24,.5f),7).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();
            string path=Art+"CityExitArrow.asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved==null){AssetDatabase.CreateAsset(mesh,path);saved=mesh;}else{EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);}
            var go=new GameObject("Seta_saida_Costa",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root,false);go.transform.position=new Vector3(0,.065f,-37.2f);
            go.GetComponent<MeshFilter>().sharedMesh=saved;var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"CityLifePalette.mat");renderer.shadowCastingMode=ShadowCastingMode.Off;
            var collider=go.AddComponent<BoxCollider>();collider.size=new Vector3(2,.04f,2.4f);
            var target=go.AddComponent<WorldClickTarget>();target.Configure("Costa · seguir pela estrada",.8f);UnityEventTools.AddPersistentListener(target.Used,world.GetComponent<TownHub>().Explore);
            go.AddComponent<TownExitCue>().Configure(world.Hero.transform,renderer);
        }
        static void Rebuild()
        {
            Physics.SyncTransforms();var surface=UnityEngine.Object.FindFirstObjectByType<NavMeshSurface>();surface.BuildNavMesh();var world=UnityEngine.Object.FindFirstObjectByType<RegionalWorld>();
            if(!NavMesh.SamplePosition(world.Definition.refuge,out var start,1,NavMesh.AllAreas))throw new InvalidOperationException("Arrival blocked");
            foreach(var target in UnityEngine.Object.FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None))
            {
                if(!NavMesh.SamplePosition(target.Approach,out var end,2.5f,NavMesh.AllAreas))throw new InvalidOperationException("Unreachable "+target.name);
                var path=new NavMeshPath();if(!NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)||path.status!=NavMeshPathStatus.PathComplete)throw new InvalidOperationException("Disconnected "+target.name);
            }
            var prior=AssetDatabase.LoadAssetAtPath<NavMeshData>("Assets/ForjaDasMares/Data/World/Town/Navigation.asset");
            EditorUtility.CopySerialized(surface.navMeshData,prior);surface.RemoveData();surface.navMeshData=prior;surface.AddData();
        }
        [Serializable]sealed class AssetRow{public string asset;public long lod0Triangles,lod1Triangles;public int lod0Draws;}
        [Serializable]sealed class AssetAudit{public AssetRow[] assets;public string note="Geometry counts, not GPU profiling. Existing assets are not deleted or decimated blindly.";}
        public static void Audit()
        {
            var rows=new List<AssetRow>();
            foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/ForjaDasMares/Art/World"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);var go=AssetDatabase.LoadAssetAtPath<GameObject>(path);var groups=go.GetComponentsInChildren<LODGroup>();
                if(groups.Length==0)continue;var row=new AssetRow{asset=path};
                foreach(var group in groups)
                {var lods=group.GetLODs();if(lods.Length==0)continue;foreach(var r in lods[0].renderers){var mesh=r.GetComponent<MeshFilter>()?.sharedMesh;if(mesh==null)continue;for(int s=0;s<mesh.subMeshCount;s++)row.lod0Triangles+=(long)mesh.GetIndexCount(s)/3;row.lod0Draws+=mesh.subMeshCount;}if(lods.Length>1)foreach(var r in lods[1].renderers){var mesh=r.GetComponent<MeshFilter>()?.sharedMesh;if(mesh==null)continue;for(int s=0;s<mesh.subMeshCount;s++)row.lod1Triangles+=(long)mesh.GetIndexCount(s)/3;}}
                rows.Add(row);
            }
            Directory.CreateDirectory("Docs/Validation/0.30.0");File.WriteAllText("Docs/Validation/0.30.0/asset-audit.json",JsonUtility.ToJson(new AssetAudit{assets=rows.OrderByDescending(r=>r.lod0Triangles).ToArray()},true));
            Debug.Log("ASSET_AUDIT count="+rows.Count+" heaviest="+string.Join(";",rows.OrderByDescending(r=>r.lod0Triangles).Take(5).Select(r=>r.asset+":"+r.lod0Triangles)));
        }
    }
}
