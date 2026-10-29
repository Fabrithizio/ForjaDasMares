using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
namespace ForjaDasMares.Networking.Editor
{
    public static class RegionalWorldInstaller
    {
        public const string ScenePath="Assets/ForjaDasMares/Scenes/CostaDasRuinas.unity";
        const string Art="Assets/ForjaDasMares/Art/World/Coast/",Data="Assets/ForjaDasMares/Data/World/";
        static readonly Vector3[] Route={new Vector3(0,0,-50),new Vector3(-37,0,-12),new Vector3(-15,0,12),new Vector3(36,0,30),new Vector3(25,0,53),new Vector3(-24,0,54),new Vector3(0,0,70)};
        static string[] names={"Ancient_Flame_Bastion","Emberstone_Citadel_Bridge","Emberwatch_Gate","Molten_Spire_Citadel","Mossbound_Relic","Mossbound_Ruin_Gate","Sunfall_Ruin"};
        public static void Apply()
        {
            Directory.CreateDirectory(Data);AssetDatabase.Refresh();var props=PrepareProps();
            var scene=EditorSceneManager.OpenScene(CampaignInstaller.ScenePath);
            var hero=UnityEngine.Object.FindFirstObjectByType<HeroRoster>();var camera=UnityEngine.Object.FindFirstObjectByType<ArenaCamera>();var pool=UnityEngine.Object.FindFirstObjectByType<EnemyPool>();var effects=hero.GetComponent<HeroCombat>().Pool;var warnings=UnityEngine.Object.FindFirstObjectByType<TelegraphPool>();
            var keep=new HashSet<GameObject>{hero.transform.root.gameObject,camera.transform.root.gameObject,pool.transform.root.gameObject,effects.transform.root.gameObject,warnings.transform.root.gameObject};
            var sun=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).First(x=>x.type==LightType.Directional);keep.Add(sun.transform.root.gameObject);
            foreach(var root in scene.GetRootGameObjects())if(root.GetComponent<UnityEngine.EventSystems.EventSystem>()!=null)keep.Add(root);
            foreach(var root in scene.GetRootGameObjects())if(!keep.Contains(root))UnityEngine.Object.DestroyImmediate(root);
            foreach(var c in hero.GetComponents<MonoBehaviour>())if(c is CoopActor||c is HeroEquipment||c is MeleeAttack)c.enabled=false;
            var motor=hero.GetComponent<PlayerMotor>();var input=hero.GetComponent<ArenaInput>();motor.Configure(input,camera.transform);motor.enabled=true;input.enabled=true;
            hero.enabled=true;hero.GetComponent<HeroCombat>().enabled=true;hero.GetComponent<PlayerAim>().Configure(motor,camera.GetComponent<Camera>());hero.GetComponent<PlayerAim>().enabled=true;hero.GetComponent<HeroPresentation>().ReplicaMode=false;
            var cameraSettings=new SerializedObject(camera);cameraSettings.FindProperty("offset").vector3Value=new Vector3(7,10,-7);cameraSettings.ApplyModifiedPropertiesWithoutUndo();
            hero.transform.position=new Vector3(0,.1f,-50);camera.Configure(hero.transform,1<<9);camera.GetComponent<Camera>().farClipPlane=280;
            var enemyData=AssetDatabase.FindAssets("t:EnemyDefinition",new[]{"Assets/ForjaDasMares/Data/Campaign/Wilderness"}).Select(id=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(id))).Where(x=>x.kind==EnemyKind.Raider||x.kind==EnemyKind.Archer||x.kind==EnemyKind.Guardian).ToArray();
            pool.Configure(enemyData,hero,effects,warnings,1<<9,(1<<9)|(1<<11)|(1<<13),1<<11);
            RegionDefinition region=AssetDatabase.LoadAssetAtPath<RegionDefinition>(Data+"Costa.asset");if(region==null){region=ScriptableObject.CreateInstance<RegionDefinition>();AssetDatabase.CreateAsset(region,Data+"Costa.asset");}
            region.route=Route;region.camps=new RegionCampDefinition[3];Vector3[] centers={Route[1],Route[3],Route[5]};
            var raider=enemyData.First(e=>e.kind==EnemyKind.Raider);var archer=enemyData.First(e=>e.kind==EnemyKind.Archer);var guard=enemyData.First(e=>e.kind==EnemyKind.Guardian);
            for(int i=0;i<3;i++)region.camps[i]=new RegionCampDefinition{label=new[]{"Vigia do Cais","Pátio dos Esquecidos","Santuário Partido"}[i],center=centers[i],enemies=i==0?new[]{raider,raider,archer}:i==1?new[]{raider,archer,guard}:new[]{guard,raider,archer,raider},spawns=new[]{centers[i]+new Vector3(-3,.06f,3),centers[i]+new Vector3(3,.06f,3),centers[i]+new Vector3(0,.06f,-3),centers[i]+new Vector3(4,.06f,-2)},respawnSeconds=45};EditorUtility.SetDirty(region);
            var environment=new GameObject("CoastEnvironment");var ground=new GameObject("TerrainSectors");ground.transform.SetParent(environment.transform);
            var layers=new[]{Layer("Grass"),Layer("Soil"),Layer("Rock")};
            for(int z=0;z<2;z++)for(int x=0;x<2;x++)BuildTerrain(ground.transform,x,z,layers,centers);
            var rock=Material("Cliff",new Color(.2f,.25f,.23f));var wood=Material("Wood",new Color(.24f,.17f,.1f));var water=Material("Sea",new Color(.06f,.22f,.27f));water.SetFloat("_Smoothness",.85f);
            Box(environment.transform,"Sea",new Vector3(0,-1.2f,-100),new Vector3(380,.3f,100),water,false,8);
            Box(environment.transform,"NorthBoundary",new Vector3(0,2,87),new Vector3(160,6,2),rock,true);Box(environment.transform,"SouthBoundary",new Vector3(0,2,-71),new Vector3(160,6,2),rock,true);
            Box(environment.transform,"WestBoundary",new Vector3(-79,2,8),new Vector3(2,6,160),rock,true);Box(environment.transform,"EastBoundary",new Vector3(79,2,8),new Vector3(2,6,160),rock,true);
            var rng=new System.Random(5527);
            for(int i=0;i<35;i++)
            {
                var p=new Vector3(rng.Next(-70,71),0,rng.Next(-62,79));if(PathDistance(p)<13||centers.Any(c=>Vector3.Distance(c,p)<16)||Vector3.Distance(region.refuge,p)<17)continue;
                Place(props[4+i%3],environment.transform,p,Quaternion.Euler(0,rng.Next(360),0),.65f+(float)rng.NextDouble()*.55f,true);
            }
            for(int segment=1;segment<Route.Length;segment++)
            {
                Vector3 direction=(Route[segment]-Route[segment-1]).normalized,side=Vector3.Cross(Vector3.up,direction);int steps=Mathf.Max(1,Mathf.FloorToInt(Vector3.Distance(Route[segment],Route[segment-1])/9));
                for(int i=0;i<steps;i++)for(int sign=-1;sign<=1;sign+=2)
                {
                    Vector3 p=Vector3.Lerp(Route[segment-1],Route[segment],(i+.5f)/steps)+side*(sign*(7+(i%3)));
                    if(centers.Any(c=>Vector3.Distance(c,p)<8))continue;Place(props[i%5==0?6:4],environment.transform,p,Quaternion.Euler(0,segment*37+i*61,0),i%5==0?.35f:.28f+.06f*(i%3),true);
                }
            }
            Place(props[4],environment.transform,new Vector3(-10,0,-49),Quaternion.Euler(0,45,0),.8f,true);
            Place(props[5],environment.transform,new Vector3(0,0,73),Quaternion.identity,1.4f,false);
            for(int i=0;i<3;i++)
            {
                Place(props[i==1?4:6],environment.transform,centers[i]+new Vector3(-9,0,8),Quaternion.Euler(0,90*i,0),1,true);
                Sign(environment.transform,region.camps[i].label,centers[i]+new Vector3(8,0,2),wood);
            }
            Sign(environment.transform,"REFÚGIO · F PARA DESCANSAR",region.refuge+Vector3.right*4,wood);Sign(environment.transform,"PORTAL DOS DESAFIOS",new Vector3(4,0,68),wood);
            // A stone shortcut links the western camp to the sanctuary through a second route.
            for(int i=0;i<8;i++)Box(environment.transform,"ShortcutStone_"+i,new Vector3(-39+i*2,.015f,17+i*4),new Vector3(2,.1f,2),rock,false,8);
            var nav=new GameObject("RegionalNavigation").AddComponent<NavMeshSurface>();nav.layerMask=(1<<8)|(1<<9);nav.useGeometry=NavMeshCollectGeometry.PhysicsColliders;nav.collectObjects=CollectObjects.All;nav.BuildNavMesh();
            string navPath=Data+"CoastNavigation.asset";var old=AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);if(old==null)AssetDatabase.CreateAsset(nav.navMeshData,navPath);else{EditorUtility.CopySerialized(nav.navMeshData,old);nav.navMeshData=old;}
            if(nav.navMeshData==null)throw new InvalidOperationException("Region navigation missing");
            foreach(var point in Route)if(!NavMesh.SamplePosition(point,out _,3,NavMesh.AllAreas))throw new InvalidOperationException("Route off navigation: "+point);
            if(UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()==null){var events=new GameObject("RegionalEventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>();events.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();}
            var systems=new GameObject("RegionalWorld").AddComponent<RegionalWorld>();systems.Configure(region,hero,pool,camera,AssetDatabase.LoadAssetAtPath<SoundscapeLibrary>("Assets/ForjaDasMares/Resources/CampaignSoundscape.asset"));
            new GameObject("RegionalHUD",typeof(RectTransform)).AddComponent<RegionalWorldHud>().Configure(systems);
            sun.transform.rotation=Quaternion.Euler(48,-28,0);sun.intensity=1.3f;sun.color=new Color(1,.89f,.72f);sun.shadows=LightShadows.Soft;
            RenderSettings.ambientLight=new Color(.33f,.43f,.48f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=55;RenderSettings.fogEndDistance=175;RenderSettings.fogColor=new Color(.16f,.27f,.3f);
            EditorSceneManager.SaveScene(scene,ScenePath);RegisterScene();AssetDatabase.SaveAssets();Debug.Log("REGION_INSTALLED props=7 terrainSectors=4 camps=3 scene="+ScenePath);
        }
        public static void RegisterScene(){var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path==ScenePath))scenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();}
        static GameObject[] PrepareProps()
        {
            var output=new List<GameObject>();
            foreach(string name in names)
            {
                string folder=Art+name+"/",modelPath=folder+"Model.fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importAnimation=false;importer.isReadable=false;importer.SaveAndReimport();
                foreach(string tex in new[]{"BaseColor","Normal","MetalSmooth"}){var t=(TextureImporter)AssetImporter.GetAtPath(folder+tex+".png");t.textureType=tex=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;t.sRGBTexture=tex=="BaseColor";t.maxTextureSize=1024;t.mipmapEnabled=true;t.textureCompression=TextureImporterCompression.Compressed;t.SaveAndReimport();}
                string matPath=folder+"Surface.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,matPath);}
                mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"BaseColor.png"));mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Normal.png"));mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"MetalSmooth.png"));mat.EnableKeyword("_NORMALMAP");mat.EnableKeyword("_METALLICSPECGLOSSMAP");mat.SetFloat("_Smoothness",.65f);EditorUtility.SetDirty(mat);
                var root=new GameObject(name);var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));model.transform.SetParent(root.transform,false);var renders=model.GetComponentsInChildren<MeshRenderer>();
                foreach(var r in renders){r.sharedMaterial=mat;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;}
                var group=root.AddComponent<LODGroup>();group.SetLODs(new[]{new LOD(.22f,renders.Where(r=>r.name=="LOD0").Cast<Renderer>().ToArray()),new LOD(.09f,renders.Where(r=>r.name=="LOD1").Cast<Renderer>().ToArray()),new LOD(.018f,renders.Where(r=>r.name=="LOD2").Cast<Renderer>().ToArray())});group.RecalculateBounds();
                output.Add(PrefabUtility.SaveAsPrefabAsset(root,folder+name+".prefab"));UnityEngine.Object.DestroyImmediate(root);
            }
            return output.ToArray();
        }
        static TerrainLayer Layer(string name)
        {
            string path=Data+name+".terrainlayer";var layer=AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);if(layer==null){layer=new TerrainLayer();AssetDatabase.CreateAsset(layer,path);}layer.diffuseTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+name+".png");layer.tileSize=new Vector2(6,6);EditorUtility.SetDirty(layer);return layer;
        }
        static float PathDistance(Vector3 p)
        {
            float distance=1000;for(int i=1;i<Route.Length;i++){Vector3 a=Route[i-1],b=Route[i];float t=Mathf.Clamp01(Vector3.Dot(p-a,b-a)/(b-a).sqrMagnitude);distance=Mathf.Min(distance,Vector3.Distance(p,a+(b-a)*t));}return distance;
        }
        static void BuildTerrain(Transform parent,int sx,int sz,TerrainLayer[] layers,Vector3[] camps)
        {
            string path=Data+"Coast_"+sx+"_"+sz+".asset";var data=AssetDatabase.LoadAssetAtPath<TerrainData>(path);if(data==null){data=new TerrainData();AssetDatabase.CreateAsset(data,path);}data.heightmapResolution=129;data.alphamapResolution=128;data.size=new Vector3(80,8,80);data.terrainLayers=layers;
            var heights=new float[129,129];for(int z=0;z<129;z++)for(int x=0;x<129;x++){Vector3 p=new Vector3(-80+sx*80+x*80f/128,0,-72+sz*80+z*80f/128);float d=PathDistance(p);foreach(var c in camps)d=Mathf.Min(d,Mathf.Max(0,Vector3.Distance(p,c)-7));d=Mathf.Min(d,Mathf.Max(0,Vector3.Distance(p,Route[0])-10));heights[z,x]=(2+Mathf.SmoothStep(0,1,Mathf.InverseLerp(4,15,d))*(Mathf.PerlinNoise((p.x+230)*.055f,(p.z+170)*.055f)*2.2f))/8;}
            data.SetHeights(0,0,heights);var alpha=new float[128,128,3];for(int z=0;z<128;z++)for(int x=0;x<128;x++){Vector3 p=new Vector3(-80+sx*80+x*80f/127,0,-72+sz*80+z*80f/127);float road=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.5f,6,PathDistance(p)));alpha[z,x,0]=1-road;alpha[z,x,1]=road;}
            data.SetAlphamaps(0,0,alpha);var go=Terrain.CreateTerrainGameObject(data);go.name="TerrainSector_"+sx+"_"+sz;go.transform.SetParent(parent);go.transform.position=new Vector3(-80+sx*80,-2,-72+sz*80);go.layer=8;var terrain=go.GetComponent<Terrain>();terrain.heightmapPixelError=8;terrain.drawInstanced=true;terrain.materialTemplate=Material("Terrain",Color.white,"Universal Render Pipeline/Terrain/Lit");EditorUtility.SetDirty(data);
        }
        static Material Material(string name,Color color,string shader="Universal Render Pipeline/Lit")
        {string path=Data+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(mat,path);}mat.color=color;EditorUtility.SetDirty(mat);return mat;}
        static GameObject Box(Transform parent,string name,Vector3 pos,Vector3 size,Material mat,bool collision,int layer=9)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.position=pos;go.transform.localScale=size;go.layer=layer;go.GetComponent<Renderer>().sharedMaterial=mat;if(!collision)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
        static void Place(GameObject prefab,Transform parent,Vector3 point,Quaternion rotation,float scale,bool block)
        {foreach(var terrain in Terrain.activeTerrains){var start=terrain.transform.position;if(point.x>=start.x&&point.x<=start.x+terrain.terrainData.size.x&&point.z>=start.z&&point.z<=start.z+terrain.terrainData.size.z){point.y=terrain.SampleHeight(point)+start.y;break;}}
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(parent,false);go.transform.SetPositionAndRotation(point,rotation);go.transform.localScale=Vector3.one*scale;if(block){var collider=go.AddComponent<BoxCollider>();collider.center=new Vector3(0,1.5f,0);collider.size=new Vector3(4,3,3);go.layer=9;}}
        static void Sign(Transform parent,string text,Vector3 point,Material wood)
        {Box(parent,"SignPost",point+Vector3.up*.9f,new Vector3(.15f,1.8f,.15f),wood,true);Box(parent,"SignBoard",point+Vector3.up*1.65f,new Vector3(2,.7f,.12f),wood,false);var label=new GameObject("SignText").AddComponent<TextMesh>();label.transform.SetParent(parent,false);label.transform.position=point+new Vector3(0,2.2f,0);label.transform.rotation=Quaternion.Euler(40,0,0);label.text=text;label.fontSize=44;label.characterSize=.08f;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(.9f,.83f,.63f);}
    }
}
