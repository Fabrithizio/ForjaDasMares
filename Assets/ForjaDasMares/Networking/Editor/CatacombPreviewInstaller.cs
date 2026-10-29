using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
namespace ForjaDasMares.Networking.Editor
{
    public static class CatacombPreviewInstaller
    {
        public const string ScenePath="Assets/ForjaDasMares/Scenes/CatacumbasDasMares.unity";
        const string Art="Assets/ForjaDasMares/Art/World/Catacombs/",Creature="Assets/ForjaDasMares/Art/Enemies/TideCrawler/",Data="Assets/ForjaDasMares/Data/World/Catacombs/";
        static readonly Vector3[] Route={new Vector3(0,0,-50),new Vector3(-16,0,-24),new Vector3(-20,0,8),new Vector3(20,0,8),new Vector3(0,0,48),new Vector3(0,0,66)};
        static readonly Vector3[] Centers={new Vector3(-16,0,-24),new Vector3(20,0,8),new Vector3(0,0,48)};
        public static void Apply()
        {
            Directory.CreateDirectory(Data);AssetDatabase.Refresh();var gate=Prop("Ancient_Tidegate");var urn=Prop("Ancient_Wave_Urn");var crawler=Crawler();
            var scene=EditorSceneManager.OpenScene(RegionalWorldInstaller.ScenePath);
            foreach(var go in scene.GetRootGameObjects())if(go.name=="CoastEnvironment"||go.name=="RegionalNavigation")UnityEngine.Object.DestroyImmediate(go);
            var world=UnityEngine.Object.FindFirstObjectByType<RegionalWorld>();var hero=world.Hero;var pool=world.Enemies;var follow=UnityEngine.Object.FindFirstObjectByType<ArenaCamera>();
            var region=AssetDatabase.LoadAssetAtPath<RegionDefinition>(Data+"Catacombs.asset");if(region==null){region=ScriptableObject.CreateInstance<RegionDefinition>();AssetDatabase.CreateAsset(region,Data+"Catacombs.asset");}
            region.regionId="catacumbas-previa";region.displayName="Catacumbas das Marés";region.route=Route;region.refuge=Route[0]+Vector3.up*.06f;region.portal=Route[5]+Vector3.up*.06f;region.bounds=new Bounds(new Vector3(0,0,8),new Vector3(80,32,144));region.activeEnemyLimit=10;
            region.camps=Centers.Select((c,i)=>new RegionCampDefinition{label=new[]{"Galeria dos Sepultados","Cripta Inundada","Santuário das Marés"}[i],center=c,spawns=new[]{c+new Vector3(-3,.06f,2),c+new Vector3(3,.06f,2),c+new Vector3(0,.06f,-3)},enemies=new[]{crawler,crawler,crawler},respawnSeconds=45}).ToArray();EditorUtility.SetDirty(region);
            var combat=hero.GetComponent<HeroCombat>();pool.Configure(new[]{crawler},hero,combat.Pool,UnityEngine.Object.FindFirstObjectByType<TelegraphPool>(),1<<9,(1<<9)|(1<<11)|(1<<13),1<<11);
            world.Configure(region,hero,pool,follow,AssetDatabase.LoadAssetAtPath<SoundscapeLibrary>("Assets/ForjaDasMares/Resources/CampaignSoundscape.asset"));hero.transform.position=region.refuge;follow.Configure(hero.transform,1<<9);
            var environment=new GameObject("CatacombEnvironment");
            var floor=Mat("Floor",new Color(.8f,.85f,.9f));var ft=(TextureImporter)AssetImporter.GetAtPath(Art+"FloorColor.png");ft.maxTextureSize=1024;ft.mipmapEnabled=true;ft.wrapMode=TextureWrapMode.Repeat;ft.SaveAndReimport();floor.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"FloorColor.png");floor.SetFloat("_Smoothness",.13f);EditorUtility.SetDirty(floor);
            var stone=Mat("Masonry",new Color(.24f,.30f,.32f));stone.mainTexture=floor.mainTexture;EditorUtility.SetDirty(stone);
            var bronze=Mat("AgedBronze",new Color(.36f,.25f,.12f));var water=Mat("StillWater",new Color(.035f,.20f,.24f));water.SetFloat("_Smoothness",.8f);
            var cells=new HashSet<Vector2Int>();
            Add(cells,-2,1,-14,-10);Add(cells,-7,1,-10,-3);Add(cells,-7,7,-3,5);Add(cells,-4,3,6,13);Add(cells,-3,2,14,17);
            // Sunken crypt: two flanking walkways remain connected at both ends.
            for(int x=-2;x<=1;x++)for(int z=-1;z<=3;z++)if(z!=1)cells.Remove(new Vector2Int(x,z));
            foreach(var c in cells)
            {
                Vector3 p=new Vector3(c.x*4+2,-.2f,c.y*4+2);Box(environment.transform,"Floor",p,new Vector3(4,.4f,4),floor,8,true);
                foreach(var side in new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down})if(!cells.Contains(c+side))
                {
                    bool poolEdge=c.y>=-2&&c.y<=4&&c.x>=-3&&c.x<=2;
                    Vector3 edge=new Vector3(p.x+side.x*2,poolEdge?.55f:1.5f,p.z+side.y*2);
                    Box(environment.transform,poolEdge?"WaterCurb":"CatacombWall",edge,side.x!=0?new Vector3(.5f,poolEdge?1.1f:3,4.4f):new Vector3(4.4f,poolEdge?1.1f:3,.5f),stone,9,true);
                    if(!poolEdge&&Mathf.Abs(c.x+c.y)%3==0)
                    {
                        Box(environment.transform,"BurialPier",edge+Vector3.up*.3f,new Vector3(.9f,3.6f,.9f),stone,9,true);
                        Box(environment.transform,"PierCapital",edge+Vector3.up*2.2f,new Vector3(1.2f,.3f,1.2f),bronze,9,false);
                    }
                }
            }
            Box(environment.transform,"CryptWater",new Vector3(0,-.65f,6),new Vector3(16,.2f,20),water,8,false);
            // One mesh per material per spatial chunk, with original simple static colliders retained.
            // Group primitive masonry after dressing; imported LOD meshes remain separate.
            foreach(float z in new[]{-40f,-12f,24f,56f})Place(gate,environment.transform,new Vector3(0,0,z),1);
            Place(gate,environment.transform,new Vector3(-16,0,-30),1);Place(gate,environment.transform,new Vector3(20,0,18),1);
            foreach(var center in Centers)
            {
                for(int side=-1;side<=1;side+=2)
                {
                    Place(urn,environment.transform,center+new Vector3(side*6,0,5),.12f);
                    // Reusable burial alcove blocks, clearly provisional until new carved modules arrive.
                    var tomb=Box(environment.transform,"StoneSarcophagus",center+new Vector3(side*7,.5f,-5),new Vector3(1.1f,1,2.3f),stone,9,true);
                    Box(environment.transform,"SarcophagusLid",tomb.transform.position+Vector3.up*.6f,new Vector3(1.3f,.25f,2.5f),bronze,9,false);
                }
            }
            var cyan=Mat("LanternGlow",new Color(.1f,.8f,1));cyan.EnableKeyword("_EMISSION");cyan.SetColor("_EmissionColor",new Color(.1f,.8f,1)*1.6f);EditorUtility.SetDirty(cyan);
            var amber=Mat("BrazierGlow",new Color(1,.45f,.10f));amber.EnableKeyword("_EMISSION");amber.SetColor("_EmissionColor",new Color(1,.3f,.03f)*1.5f);EditorUtility.SetDirty(amber);
            for(int i=0;i<Route.Length;i++)for(int sign=-1;sign<=1;sign+=2)
            {
                Vector3 p=Route[i]+new Vector3(sign*5,0,0);Box(environment.transform,"LanternPedestal",p+Vector3.up*.6f,new Vector3(.65f,1.2f,.65f),stone,9,true);Box(environment.transform,"LanternCore",p+Vector3.up*1.4f,new Vector3(.32f,.5f,.32f),i%2==0?amber:cyan,9,false);
                if(sign==1){var lamp=new GameObject("AccentLight").AddComponent<Light>();lamp.transform.SetParent(environment.transform);lamp.transform.position=p+Vector3.up*2;lamp.type=LightType.Point;lamp.range=9;lamp.intensity=1.4f;lamp.color=i%2==0?new Color(1,.55f,.25f):new Color(.15f,.75f,1);lamp.shadows=LightShadows.None;}
            }
            Box(environment.transform,"RestPlinth",region.refuge+Vector3.up*.05f,new Vector3(2,.1f,2),bronze,8,false);
            Combine(environment.transform);
            var nav=new GameObject("CatacombNavigation").AddComponent<NavMeshSurface>();nav.layerMask=(1<<8)|(1<<9);nav.useGeometry=NavMeshCollectGeometry.PhysicsColliders;nav.collectObjects=CollectObjects.All;nav.BuildNavMesh();
            string navPath=Data+"Navigation.asset";var prior=AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);if(prior==null)AssetDatabase.CreateAsset(nav.navMeshData,navPath);else{EditorUtility.CopySerialized(nav.navMeshData,prior);nav.navMeshData=prior;}
            if(!NavMesh.SamplePosition(region.refuge,out var start,2,NavMesh.AllAreas))throw new InvalidOperationException("No entrance navigation");
            foreach(var destination in Route.Concat(region.camps.SelectMany(c=>c.spawns)))
            {var path=new NavMeshPath();if(!NavMesh.SamplePosition(destination,out var end,2,NavMesh.AllAreas)||!NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)||path.status!=NavMeshPathStatus.PathComplete)throw new InvalidOperationException("Catacomb disconnected "+destination);}
            var sun=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).First(x=>x.type==LightType.Directional);sun.intensity=.8f;sun.color=new Color(.64f,.79f,.88f);sun.transform.rotation=Quaternion.Euler(58,-25,0);sun.shadows=LightShadows.Soft;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.22f,.29f,.34f);RenderSettings.fogColor=new Color(.035f,.065f,.08f);RenderSettings.fogStartDistance=25;RenderSettings.fogEndDistance=90;follow.GetComponent<Camera>().backgroundColor=RenderSettings.fogColor;
            EditorSceneManager.SaveScene(scene,ScenePath);var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path==ScenePath))scenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
            Debug.Log("CATACOMB_READY cells="+cells.Count+" routes=6 camps=3 enemy=TideCrawler clips=6 limit=10");
        }
        static void Add(HashSet<Vector2Int> cells,int minX,int maxX,int minZ,int maxZ){for(int x=minX;x<=maxX;x++)for(int z=minZ;z<=maxZ;z++)cells.Add(new Vector2Int(x,z));}
        static Material Mat(string name,Color color){string path=Data+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
        static Material Surface(string folder)
        {
            foreach(string kind in new[]{"BaseColor","Normal","MetalSmooth"}){var t=(TextureImporter)AssetImporter.GetAtPath(folder+kind+".png");t.textureType=kind=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;t.sRGBTexture=kind=="BaseColor";t.maxTextureSize=1024;t.mipmapEnabled=true;t.textureCompression=TextureImporterCompression.Compressed;t.SaveAndReimport();}
            string path=folder+"Surface.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"BaseColor.png"));mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Normal.png"));mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"MetalSmooth.png"));mat.EnableKeyword("_NORMALMAP");mat.EnableKeyword("_METALLICSPECGLOSSMAP");mat.enableInstancing=true;EditorUtility.SetDirty(mat);return mat;
        }
        static void Lods(GameObject root,Renderer[] renderers)
        {var group=root.GetComponent<LODGroup>();if(group==null)group=root.AddComponent<LODGroup>();group.SetLODs(new[]{new LOD(.18f,renderers.Where(r=>r.name=="LOD0").ToArray()),new LOD(.08f,renderers.Where(r=>r.name=="LOD1").ToArray()),new LOD(.015f,renderers.Where(r=>r.name=="LOD2").ToArray())});group.RecalculateBounds();}
        static GameObject Prop(string name)
        {string folder=Art+name+"/";var imp=(ModelImporter)AssetImporter.GetAtPath(folder+"Model.fbx");imp.materialImportMode=ModelImporterMaterialImportMode.None;imp.importAnimation=false;imp.isReadable=false;imp.SaveAndReimport();var mat=Surface(folder);var root=new GameObject(name);var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(folder+"Model.fbx"));model.transform.SetParent(root.transform,false);var renderers=root.GetComponentsInChildren<Renderer>();foreach(var r in renderers)r.sharedMaterial=mat;Lods(root,renderers);var prefab=PrefabUtility.SaveAsPrefabAsset(root,folder+name+".prefab");UnityEngine.Object.DestroyImmediate(root);return prefab;}
        static EnemyDefinition Crawler()
        {
            string modelPath=Creature+"CrawlerAnimated.fbx";var imp=(ModelImporter)AssetImporter.GetAtPath(modelPath);imp.materialImportMode=ModelImporterMaterialImportMode.None;imp.animationType=ModelImporterAnimationType.Generic;imp.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;imp.importAnimation=true;imp.isReadable=false;imp.SaveAndReimport();
            var clips=imp.defaultClipAnimations;foreach(var c in clips){c.name=c.takeName.Split('|').Last();c.loopTime=c.name=="Idle"||c.name=="Walk"||c.name=="Run";c.lockRootHeightY=c.lockRootPositionXZ=c.lockRootRotation=true;}imp.clipAnimations=clips;imp.SaveAndReimport();
            var motions=AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();string controllerPath=Creature+"Crawler.controller";var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine=controller.layers[0].stateMachine;foreach(var prior in machine.states)machine.RemoveState(prior.state);
            foreach(string name in new[]{"Idle","Walk","Run","Attack","Hit","Dead"}){var motion=motions.FirstOrDefault(c=>c.name==name);if(motion==null)throw new InvalidOperationException("Missing crawler clip "+name+" found "+string.Join(",",motions.Select(c=>c.name)));var state=machine.AddState(name);state.motion=motion;if(name=="Idle")machine.defaultState=state;}
            var original=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/ForjaDasMares/Data/Campaign/Wilderness/Enemy_raspador.asset");var data=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(Data+"Crawler.asset");if(data==null){data=UnityEngine.Object.Instantiate(original);AssetDatabase.CreateAsset(data,Data+"Crawler.asset");}
            data.enemyId="servo-mare";data.displayName="Servo das Marés";data.maximumHealth=32;data.height=1.05f;data.radius=.55f;data.speed=3.3f;data.poolCapacity=12;data.color=Color.white;
            var root=PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(original.prefab));var view=root.GetComponent<EnemyPresentation>();var so=new SerializedObject(view);var old=so.FindProperty("model").objectReferenceValue as Transform;if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var visual=new GameObject("CrawlerVisual");visual.transform.SetParent(root.transform,false);var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));model.transform.SetParent(visual.transform,false);
            var renderers=model.GetComponentsInChildren<SkinnedMeshRenderer>();var bounds=renderers[0].bounds;float factor=1.05f/Mathf.Max(.1f,bounds.size.y);model.transform.localScale=Vector3.one*factor;model.transform.localPosition=Vector3.up*(-bounds.min.y*factor);
            var mat=Surface(Creature);foreach(var r in renderers){r.sharedMaterial=mat;r.updateWhenOffscreen=false;r.quality=SkinQuality.Bone2;}Lods(visual,renderers.Cast<Renderer>().ToArray());
            var animator=model.GetComponent<Animator>()??model.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.CullUpdateTransforms;visual.AddComponent<RiggedEnemyAnimation>().Configure(animator);
            view.Configure(root.GetComponent<EnemyActor>(),visual.transform,Array.Empty<Renderer>(),null);data.prefab=PrefabUtility.SaveAsPrefabAsset(root,Creature+"TideCrawler.prefab");PrefabUtility.UnloadPrefabContents(root);EditorUtility.SetDirty(data);EditorUtility.SetDirty(controller);return data;
        }
        static GameObject Box(Transform parent,string name,Vector3 p,Vector3 size,Material material,int layer,bool collision)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.position=p;go.transform.localScale=size;go.layer=layer;go.GetComponent<Renderer>().sharedMaterial=material;if(!collision)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
        static void Place(GameObject prefab,Transform parent,Vector3 point,float scale){var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(parent,false);go.transform.position=point;go.transform.localScale=Vector3.one*scale;if(prefab.name=="Ancient_Wave_Urn"){go.layer=9;var collider=go.AddComponent<BoxCollider>();collider.center=new Vector3(0,4,0);collider.size=new Vector3(6,8,6);}else{foreach(int side in new[]{-1,1}){go.layer=9;var collider=go.AddComponent<BoxCollider>();collider.center=new Vector3(side*3.3f,2.5f,0);collider.size=new Vector3(1.3f,5,1);}var lintel=go.AddComponent<BoxCollider>();lintel.center=new Vector3(0,6,0);lintel.size=new Vector3(5.5f,1,1);}}
        static void Combine(Transform root)
        {
            var parts=root.GetComponentsInChildren<MeshFilter>().Where(p=>p.sharedMesh!=null&&p.sharedMesh.isReadable).ToArray();var groups=parts.GroupBy(p=>p.GetComponent<Renderer>().sharedMaterial.name+"_"+Mathf.FloorToInt(p.transform.position.x/32)+"_"+Mathf.FloorToInt(p.transform.position.z/32));
            foreach(var group in groups)
            {var elements=group.ToArray();var mesh=new Mesh{name=group.Key};mesh.CombineMeshes(elements.Select(p=>new CombineInstance{mesh=p.sharedMesh,transform=p.transform.localToWorldMatrix}).ToArray());string path=Data+group.Key+".asset";var prior=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(prior==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,prior);mesh=prior;}var go=new GameObject("MasonryChunk_"+group.Key,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=elements[0].GetComponent<Renderer>().sharedMaterial;foreach(var part in elements){part.transform.SetParent(go.transform,true);UnityEngine.Object.DestroyImmediate(part.GetComponent<MeshRenderer>());UnityEngine.Object.DestroyImmediate(part);}}
        }
    }
}
