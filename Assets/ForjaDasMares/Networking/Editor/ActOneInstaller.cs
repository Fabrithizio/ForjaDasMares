using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.AI.Navigation;
namespace ForjaDasMares.Networking.Editor
{
    public static class ActOneInstaller
    {
        public const string ScenePath="Assets/ForjaDasMares/Scenes/CostaAto1.unity";
        const string Data="Assets/ForjaDasMares/Data/World/Act1/",Art="Assets/ForjaDasMares/Art/World/Act1/",Coast="Assets/ForjaDasMares/Art/World/Coast/";
        static readonly Vector3 Refuge=Vector3.zero,Cais=new Vector3(-88,0,-48),Pedreira=new Vector3(90,0,4),Jardim=new Vector3(-28,0,95),Entrance=new Vector3(0,0,132),Breu=new Vector3(91,0,118);
        static readonly Vector3 West=new Vector3(-65,0,36),East=new Vector3(48,0,58),Beach=new Vector3(5,0,-62);
        static readonly RegionConnection[] Links={new RegionConnection(Refuge,Cais),new RegionConnection(Cais,West),new RegionConnection(West,Jardim),new RegionConnection(Jardim,East),new RegionConnection(East,Refuge),new RegionConnection(Refuge,Pedreira),new RegionConnection(Pedreira,East),new RegionConnection(Cais,Beach),new RegionConnection(Beach,Refuge),new RegionConnection(Jardim,Entrance),new RegionConnection(East,Breu),new RegionConnection(Breu,Pedreira)};
        static readonly Vector3[] Sites={Refuge,Cais,Pedreira,Jardim,Entrance,Breu,West,East,Beach};
        static readonly Dictionary<string,GameObject> Props=new Dictionary<string,GameObject>();
        public static void Apply()
        {
            Directory.CreateDirectory(Data);AssetDatabase.Refresh();Props.Clear();
            foreach(string name in new[]{"RefugeTent","CoastalRock","LowWall","GuardianStatue","RootStump","CoalBrazier","Gravewood"})Props[name]=PrepareProp(Art+name+"/",name);
            foreach(string name in new[]{"Mossbound_Relic","Mossbound_Ruin_Gate","Sunfall_Ruin"})Props[name]=AssetDatabase.LoadAssetAtPath<GameObject>(Coast+name+"/"+name+".prefab");
            var scene=EditorSceneManager.OpenScene(RegionalWorldInstaller.ScenePath);
            foreach(var go in scene.GetRootGameObjects())if(go.name=="CoastEnvironment"||go.name=="RegionalNavigation")UnityEngine.Object.DestroyImmediate(go);
            var world=UnityEngine.Object.FindFirstObjectByType<RegionalWorld>();var hero=world.Hero;var camera=UnityEngine.Object.FindFirstObjectByType<ArenaCamera>();
            var region=Asset<RegionDefinition>(Data+"Region.asset");region.regionId="costa-ato1";region.displayName="Costa das Ruínas · Ato 1";region.refuge=Refuge+Vector3.up*.08f;region.portal=Entrance+Vector3.up*.08f;region.bounds=new Bounds(new Vector3(0,0,32),new Vector3(320,45,320));region.route=Sites;region.connections=Links;region.activeEnemyLimit=12;
            region.landmarks=new[]{Mark("Refúgio do Último Sino",Refuge,Color.green),Mark("Cais Partido",Cais,Color.yellow),Mark("Pedreira do Eco",Pedreira,Color.yellow),Mark("Jardim dos Nomes",Jardim,Color.yellow),Mark("Passagem do Breu",Breu,Color.cyan)};
            var crawler=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/ForjaDasMares/Data/World/Catacombs/Crawler.asset");
            var ranged=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/ForjaDasMares/Data/Campaign/Wilderness/Enemy_vigia-sombrio.asset");
            var guard=AssetDatabase.FindAssets("t:EnemyDefinition",new[]{"Assets/ForjaDasMares/Data/Campaign/Wilderness"}).Select(g=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(d=>d.kind==EnemyKind.Guardian);
            var centers=new[]{Cais+new Vector3(9,0,9),Pedreira+new Vector3(-6,0,12),Jardim+new Vector3(10,0,-10),West+new Vector3(8,0,0),Breu+new Vector3(-7,0,2)};
            region.camps=centers.Select((c,i)=>new RegionCampDefinition{label=new[]{"Vigias do Cais","Extratores do Eco","Guardiões dos Nomes","Patrulha da Encosta","Raízes Despertas"}[i],center=c,spawns=new[]{c+new Vector3(-3,.08f,3),c+new Vector3(3,.08f,3),c+new Vector3(0,.08f,-3)},enemies=new[]{crawler,i==2?guard:crawler,ranged},respawnSeconds=65}).ToArray();
            world.Enemies.Configure(new[]{crawler,ranged,guard},hero,hero.GetComponent<HeroCombat>().Pool,UnityEngine.Object.FindFirstObjectByType<TelegraphPool>(),1<<9,(1<<9)|(1<<11)|(1<<13),1<<11);
            world.Configure(region,hero,world.Enemies,camera,AssetDatabase.LoadAssetAtPath<SoundscapeLibrary>("Assets/ForjaDasMares/Resources/CampaignSoundscape.asset"));hero.transform.position=region.refuge;
            var environment=new GameObject("ActOneEnvironment");var terrainRoot=new GameObject("TerrainSectors");terrainRoot.transform.SetParent(environment.transform);
            var layers=new[]{TerrainLayer("Grass","Grass",new Color(1.5f,1.55f,1.3f)),TerrainLayer("Road","Soil",new Color(1.25f,1.28f,1.2f)),TerrainLayer("Rock","Rock",new Color(1.1f,1.14f,1.12f))};
            for(int z=0;z<2;z++)for(int x=0;x<2;x++)BuildTerrain(terrainRoot.transform,x,z,layers,centers);
            Material stone=Mat("Stone",new Color(.43f,.49f,.47f)),wood=Mat("Wood",new Color(.38f,.23f,.12f)),brass=Mat("Brass",new Color(.72f,.45f,.13f)),water=Mat("Water",new Color(.055f,.32f,.39f));water.SetFloat("_Smoothness",.65f);
            Box(environment.transform,"Sea",new Vector3(0,-1,-166),new Vector3(600,.3f,108),water,false,8);
            // Discreet collision boundaries, dressed with repeated rocks. No interior room gates.
            Box(environment.transform,"WestEdge",new Vector3(-153,4,32),new Vector3(2,12,320),stone,true);
            Box(environment.transform,"EastEdge",new Vector3(153,4,32),new Vector3(2,12,320),stone,true);
            Box(environment.transform,"NorthEdge",new Vector3(0,4,184),new Vector3(308,12,2),stone,true);
            Box(environment.transform,"CoastEdge",new Vector3(0,1,-112),new Vector3(308,5,2),stone,true);
            var random=new System.Random(9173);
            for(int i=0;i<180;i++)
            {
                var p=new Vector3(random.Next(-143,144),0,random.Next(-100,177));
                if(RoadDistance(p)<13||Sites.Any(c=>(c-p).sqrMagnitude<24*24)||centers.Any(c=>(c-p).sqrMagnitude<15*15))continue;
                string kind=i%5==0?"Gravewood":i%5==1?"RootStump":"CoastalRock";
                Place(kind,environment.transform,p,kind=="Gravewood"?1.05f:kind=="RootStump"?.45f:.65f+(float)random.NextDouble()*.4f,random.Next(360),true);
            }
            PlantPatches(environment.transform);
            // Refuge: two tents, workbench, stools, brazier and original low-poly bell.
            Place("RefugeTent",environment.transform,new Vector3(-10,0,3),1.15f,155,true);
            Place("RefugeTent",environment.transform,new Vector3(12,0,8),.85f,190,true);
            Place("CoalBrazier",environment.transform,new Vector3(-1,0,8),.24f,0,true);
            Bench(environment.transform,new Vector3(8,0,-7),wood);Bell(environment.transform,new Vector3(0,0,13),wood,brass);
            foreach(var p in new[]{new Vector3(5,0,3),new Vector3(3,0,5)})Box(environment.transform,"RefugeStool",p+Vector3.up*.4f,new Vector3(.65f,.8f,.65f),wood,true);
            Box(environment.transform,"SupplyCrate",new Vector3(-9,.55f,-6),new Vector3(1.3f,1.1f,1.2f),wood,true);
            Npc(environment.transform,"Nara · Cartógrafa",new Vector3(-4,0,-3),Mat("Nara",new Color(.15f,.6f,.67f)));
            Npc(environment.transform,"Daro · Artífice",new Vector3(6,0,-4),Mat("Daro",new Color(.75f,.39f,.15f)));
            Label(environment.transform,"Refúgio do Último Sino",new Vector3(0,3.8f,14),.08f);
            // Cais: traversable planks and piles on a broad shore, debris stays off the route.
            for(int i=0;i<16;i++)Box(environment.transform,"DockPlank",new Vector3(-91,.02f,-51-i*.9f),new Vector3(7,.18f,.83f),wood,true,8);
            foreach(int side in new[]{-1,1})for(int i=0;i<5;i++)Box(environment.transform,"DockPost",new Vector3(-91+side*3.7f,.5f,-51-i*3),new Vector3(.4f,2,.4f),wood,true);
            Label(environment.transform,"Cais Partido",Cais+new Vector3(-4,3,-1),.075f);
            // Pedreira: reusable rock piles, extraction frame and short rail strips.
            for(int i=0;i<6;i++)Place("CoastalRock",environment.transform,Pedreira+new Vector3(15+(i%2)*5,0,-12+i*5),.7f+i*.08f,i*43,true);
            for(int i=0;i<9;i++)Box(environment.transform,"QuarryRailTie",Pedreira+new Vector3(-7,.03f,-8+i*2),new Vector3(3,.12f,.3f),wood,false);
            Box(environment.transform,"ExtractionFrame",Pedreira+new Vector3(0,2,-9),new Vector3(4,.35f,.5f),wood,false);
            Label(environment.transform,"Pedreira do Eco",Pedreira+new Vector3(0,3,-14),.075f);
            // Garden: low walls, memorials and the new supplied tree.
            for(int i=0;i<6;i++)Place("LowWall",environment.transform,Jardim+new Vector3(i<3?-15:16,0,-16+(i%3)*12),.7f,90,true);
            Place("Gravewood",environment.transform,Jardim+new Vector3(-10,0,10),1.35f,25,true);
            Place("GuardianStatue",environment.transform,Jardim+new Vector3(0,0,12),.5f,180,true);
            for(int i=0;i<8;i++)Box(environment.transform,"MemorialStone",Jardim+new Vector3(-9+(i%4)*5,.6f,-12+(i/4)*7),new Vector3(.9f,1.3f,.4f),stone,true);
            Label(environment.transform,"Jardim dos Nomes",Jardim+new Vector3(0,3,14),.075f);
            Place("Mossbound_Ruin_Gate",environment.transform,Entrance,1.4f,0,false);
            for(int i=0;i<5;i++)Place("RootStump",environment.transform,Breu+new Vector3(i%2==0?-12:12,0,-15+i*7),.55f,i*65,true);
            Place("Gravewood",environment.transform,Breu+new Vector3(15,0,15),1.4f,40,true);
            Label(environment.transform,"Passagem do Breu",Breu+Vector3.up*3,.075f);
            var story=Asset<RegionStoryDefinition>(Data+"Story.asset");story.points=new[]{
                Point("Conversar com Nara",RegionalInteraction.Cartographer,new Vector3(-4,0,-3)),Point("Conversar com Daro",RegionalInteraction.Artificer,new Vector3(6,0,-4)),
                Point("Examinar diário da expedição",RegionalInteraction.Clue,Cais+new Vector3(-4,0,-3),0,"O diário menciona um sino silenciado e registros levados ao jardim."),
                Point("Examinar mecanismo de extração",RegionalInteraction.Clue,Pedreira+new Vector3(0,0,-7),1,"O extrator desviava a Maré. O sino ficou sem energia."),
                Point("Ler registro dos nomes",RegionalInteraction.Clue,Jardim+new Vector3(0,0,6),2,"Os nomes pertencem à cidade perdida. Os guardiões repetem suas lembranças."),
                Point("Entrar nas catacumbas",RegionalInteraction.Catacombs,Entrance+new Vector3(0,0,-4))};
            var clueMat=Mat("ClueGlow",new Color(.12f,.8f,.9f));clueMat.EnableKeyword("_EMISSION");clueMat.SetColor("_EmissionColor",new Color(.05f,.5f,.7f));
            foreach(var point in story.points){point.position=Ground(point.position)+Vector3.up*.08f;if(point.interaction==RegionalInteraction.Clue){Box(environment.transform,"Clue_"+point.clue,point.position+Vector3.up*.6f,new Vector3(.8f,1.2f,.7f),stone,true);Box(environment.transform,"ClueMarker",point.position+Vector3.up*1.4f,new Vector3(.25f,.25f,.25f),clueMat,false);}}
            var journey=world.gameObject.AddComponent<RegionalJourney>();journey.Configure(world,story);
            foreach(var c in region.camps){c.center=Ground(c.center);for(int i=0;i<c.spawns.Length;i++)c.spawns[i]=Ground(c.spawns[i])+Vector3.up*.08f;}
            region.refuge=Ground(Refuge)+Vector3.up*.08f;
            var nav=new GameObject("ActOneNavigation").AddComponent<NavMeshSurface>();nav.layerMask=(1<<8)|(1<<9);nav.useGeometry=NavMeshCollectGeometry.PhysicsColliders;nav.collectObjects=CollectObjects.All;nav.BuildNavMesh();
            string navPath=Data+"Navigation.asset";var prior=AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);if(prior==null)AssetDatabase.CreateAsset(nav.navMeshData,navPath);else{EditorUtility.CopySerialized(nav.navMeshData,prior);nav.navMeshData=prior;}
            if(!NavMesh.SamplePosition(region.refuge,out var start,3,NavMesh.AllAreas))throw new InvalidOperationException("Act1 refuge disconnected");
            foreach(var p in Sites.Concat(region.camps.SelectMany(c=>c.spawns)).Concat(story.points.Select(p=>p.position)))
            {var path=new NavMeshPath();if(!NavMesh.SamplePosition(p,out var end,3,NavMesh.AllAreas)||!NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)||path.status!=NavMeshPathStatus.PathComplete)throw new InvalidOperationException("Act1 destination disconnected: "+p);}
            camera.Configure(hero.transform,1<<9);camera.GetComponent<Camera>().farClipPlane=380;
            var quality=camera.GetComponent<PresentationQuality>()??camera.gameObject.AddComponent<PresentationQuality>();
            var sun=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).First(l=>l.type==LightType.Directional);sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.color=new Color(1,.96f,.87f);sun.intensity=1.6f;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.48f,.59f,.66f);RenderSettings.ambientEquatorColor=new Color(.36f,.42f,.39f);RenderSettings.ambientGroundColor=new Color(.21f,.19f,.16f);RenderSettings.ambientIntensity=1;RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=95;RenderSettings.fogEndDistance=280;RenderSettings.fogColor=new Color(.26f,.42f,.46f);camera.GetComponent<Camera>().backgroundColor=RenderSettings.fogColor;
            var volume=new GameObject("ActOneColorGrade").AddComponent<Volume>();volume.isGlobal=true;volume.priority=2;var profile=Asset<VolumeProfile>(Data+"ColorGrade.asset");var adjustments=profile.components.OfType<ColorAdjustments>().FirstOrDefault();if(adjustments==null){adjustments=profile.Add<ColorAdjustments>(true);AssetDatabase.AddObjectToAsset(adjustments,profile);}adjustments.saturation.Override(8);adjustments.contrast.Override(6);adjustments.postExposure.Override(.12f);volume.sharedProfile=profile;
            EditorUtility.SetDirty(profile);EditorUtility.SetDirty(region);EditorUtility.SetDirty(story);EditorSceneManager.SaveScene(scene,ScenePath);
            var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path==ScenePath))scenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
            Debug.Log("ACT1_READY terrain=320x320 sectors=4 loops=4 landmarks=5 camps=5 budget=12 NPCs=2 provisional=true story=2-of-3 local=true");
        }
        static RegionLandmark Mark(string label,Vector3 p,Color c)=>new RegionLandmark{label=label,position=p,color=c};
        static RegionalStoryPoint Point(string label,RegionalInteraction kind,Vector3 p,int clue=0,string discovery="")=>new RegionalStoryPoint{label=label,interaction=kind,position=p,clue=clue,discovery=discovery};
        static T Asset<T>(string path)where T:ScriptableObject{var a=AssetDatabase.LoadAssetAtPath<T>(path);if(a==null){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);}return a;}
        static T NativeAsset<T>(string path,Func<T> create)where T:UnityEngine.Object{var a=AssetDatabase.LoadAssetAtPath<T>(path);if(a==null){a=create();AssetDatabase.CreateAsset(a,path);}return a;}
        static Material Mat(string name,Color color){string path=Data+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.SetFloat("_Smoothness",.18f);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
        static GameObject PrepareProp(string folder,string name)
        {
            var imp=(ModelImporter)AssetImporter.GetAtPath(folder+"Model.fbx");imp.materialImportMode=ModelImporterMaterialImportMode.None;imp.importAnimation=false;imp.isReadable=false;imp.SaveAndReimport();
            foreach(string kind in new[]{"BaseColor","Normal","MetalSmooth"}){var t=(TextureImporter)AssetImporter.GetAtPath(folder+kind+".png");t.textureType=kind=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;t.sRGBTexture=kind=="BaseColor";t.maxTextureSize=1024;t.mipmapEnabled=true;t.textureCompression=TextureImporterCompression.Compressed;t.SaveAndReimport();}
            string path=folder+"Surface.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}mat.color=Color.white;mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"BaseColor.png"));mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Normal.png"));mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"MetalSmooth.png"));mat.SetFloat("_Smoothness",.55f);mat.SetFloat("_BumpScale",.85f);mat.EnableKeyword("_NORMALMAP");mat.EnableKeyword("_METALLICSPECGLOSSMAP");mat.enableInstancing=true;EditorUtility.SetDirty(mat);
            var root=new GameObject(name);var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(folder+"Model.fbx"));model.transform.SetParent(root.transform,false);var renderers=root.GetComponentsInChildren<Renderer>();foreach(var r in renderers)r.sharedMaterial=mat;
            root.AddComponent<LODGroup>().SetLODs(new[]{new LOD(.16f,renderers.Where(r=>r.name=="LOD0").ToArray()),new LOD(.065f,renderers.Where(r=>r.name=="LOD1").ToArray()),new LOD(.012f,renderers.Where(r=>r.name=="LOD2").ToArray())});root.GetComponent<LODGroup>().RecalculateBounds();var prefab=PrefabUtility.SaveAsPrefabAsset(root,folder+name+".prefab");UnityEngine.Object.DestroyImmediate(root);return prefab;
        }
        static TerrainLayer TerrainLayer(string name,string texture,Color tint)
        {var layer=NativeAsset(Data+name+".terrainlayer",()=>new TerrainLayer());layer.diffuseTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Coast+texture+".png");layer.tileSize=new Vector2(name=="Road"?7:11,name=="Road"?7:11);layer.diffuseRemapMin=Vector4.zero;layer.diffuseRemapMax=new Vector4(tint.r,tint.g,tint.b,1);EditorUtility.SetDirty(layer);return layer;}
        static float RoadDistance(Vector3 p)
        {float best=float.MaxValue;foreach(var link in Links){Vector3 d=link.to-link.from;float t=Mathf.Clamp01(Vector3.Dot(p-link.from,d)/d.sqrMagnitude);best=Mathf.Min(best,Vector3.Distance(p,link.from+d*t));}return best;}
        static void BuildTerrain(Transform parent,int x,int z,TerrainLayer[] layers,Vector3[] camps)
        {
            var data=NativeAsset(Data+"Terrain_"+x+"_"+z+".asset",()=>new TerrainData());data.heightmapResolution=129;data.alphamapResolution=128;data.size=new Vector3(160,12,160);data.terrainLayers=layers;
            var heights=new float[129,129];for(int j=0;j<129;j++)for(int i=0;i<129;i++){var p=new Vector3(-160+x*160+i*1.25f,0,-128+z*160+j*1.25f);float d=RoadDistance(p);foreach(var site in Sites.Concat(camps))d=Mathf.Min(d,Mathf.Max(0,Vector3.Distance(p,site)-20));heights[j,i]=(2+Mathf.SmoothStep(0,1,Mathf.InverseLerp(8,26,d))*(1.5f+Mathf.PerlinNoise((p.x+312)*.025f,(p.z+78)*.025f)*4))/12;}
            data.SetHeights(0,0,heights);var alpha=new float[128,128,3];for(int j=0;j<128;j++)for(int i=0;i<128;i++){var p=new Vector3(-160+x*160+i*160f/127,0,-128+z*160+j*160f/127);float d=RoadDistance(p);float road=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(3,9,d));if(Vector3.Distance(p,Refuge)<14)road=1;float rock=Mathf.Clamp01((d-28)/30)*.55f;alpha[j,i,0]=(1-road)*(1-rock);alpha[j,i,1]=road;alpha[j,i,2]=(1-road)*rock;}data.SetAlphamaps(0,0,alpha);
            var go=Terrain.CreateTerrainGameObject(data);go.name="Act1Terrain_"+x+"_"+z;go.transform.SetParent(parent);go.transform.position=new Vector3(-160+x*160,-2,-128+z*160);go.layer=8;var terrain=go.GetComponent<Terrain>();terrain.drawInstanced=true;terrain.heightmapPixelError=8;terrain.basemapDistance=120;
            string path=Data+"Terrain.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));AssetDatabase.CreateAsset(material,path);}terrain.materialTemplate=material;EditorUtility.SetDirty(data);
        }
        static Vector3 Ground(Vector3 p){foreach(var t in Terrain.activeTerrains){var v=t.transform.position;if(p.x>=v.x&&p.x<=v.x+t.terrainData.size.x&&p.z>=v.z&&p.z<=v.z+t.terrainData.size.z){p.y=t.SampleHeight(p)+v.y;break;}}return p;}
        static GameObject Place(string name,Transform parent,Vector3 p,float scale,float yaw,bool collision)
        {var go=(GameObject)PrefabUtility.InstantiatePrefab(Props[name]);go.transform.SetParent(parent,false);go.transform.SetPositionAndRotation(Ground(p),Quaternion.Euler(0,yaw,0));go.transform.localScale=Vector3.one*scale;go.layer=9;if(collision){var box=go.AddComponent<BoxCollider>();var bounds=go.GetComponentsInChildren<Renderer>().First(r=>r.name=="LOD0").localBounds;if(name=="Gravewood"){box.center=new Vector3(0,1.3f,0);box.size=new Vector3(.9f,2.6f,.9f);}else{box.center=bounds.center;box.size=bounds.size*.88f;}}return go;}
        static GameObject Box(Transform parent,string name,Vector3 p,Vector3 size,Material material,bool collision,int layer=9)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.position=p;go.transform.localScale=size;go.layer=layer;go.GetComponent<Renderer>().sharedMaterial=material;if(!collision)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
        static void Npc(Transform parent,string name,Vector3 p,Material mat)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Capsule);go.name=name+" (provisório)";go.transform.SetParent(parent,false);go.transform.position=Ground(p)+Vector3.up;go.transform.localScale=new Vector3(.8f,1,.8f);go.GetComponent<Renderer>().sharedMaterial=mat;go.layer=9;Label(parent,name,Ground(p)+Vector3.up*2.6f,.06f);}
        static void Label(Transform parent,string text,Vector3 p,float size)
        {var t=new GameObject("Label_"+text).AddComponent<TextMesh>();t.transform.SetParent(parent,false);t.transform.position=p;t.transform.rotation=Quaternion.Euler(50,0,0);t.text=text;t.fontSize=48;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(1,.88f,.55f);}
        static void PlantPatches(Transform parent)
        {
            // Three original geometric ground-cover variants, merged by 32m chunk and material.
            // No alpha cards, colliders, per-plant scripts or per-frame animation.
            var random=new System.Random(8712);var chunks=new Dictionary<string,List<Vector3>>();var indices=new Dictionary<string,List<int>>();
            var materials=new[]{Mat("GrassTufts",new Color(.25f,.39f,.12f)),Mat("CoastalReeds",new Color(.5f,.49f,.22f)),Mat("BroadLeaves",new Color(.16f,.36f,.19f))};
            foreach(var m in materials){m.SetFloat("_Cull",0);EditorUtility.SetDirty(m);}
            for(int i=0;i<1400;i++)
            {
                var p=new Vector3(random.Next(-140,141),0,random.Next(-98,171));float d=RoadDistance(p);
                if(d<7||d>28||Sites.Any(site=>(site-p).sqrMagnitude<13*13))continue;p=Ground(p);
                int type=i%3;string key=type+"_"+Mathf.FloorToInt(p.x/32)+"_"+Mathf.FloorToInt(p.z/32);
                if(!chunks.ContainsKey(key)){chunks[key]=new List<Vector3>();indices[key]=new List<int>();}
                var vertices=chunks[key];var triangles=indices[key];float height=type==1?1.1f:type==2?.5f:.65f;
                for(int blade=0;blade<5;blade++)
                {
                    float a=(float)random.NextDouble()*Mathf.PI*2;var side=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(type==2?.45f:.12f);var offset=new Vector3((float)random.NextDouble()-.5f,0,(float)random.NextDouble()-.5f);
                    int n=vertices.Count;vertices.Add(p+offset-side);vertices.Add(p+offset+side);vertices.Add(p+offset+Vector3.up*height+side*.4f);triangles.Add(n);triangles.Add(n+2);triangles.Add(n+1);
                }
            }
            foreach(var item in chunks)
            {
                var mesh=new Mesh{name="GroundCover_"+item.Key};mesh.SetVertices(item.Value);mesh.SetTriangles(indices[item.Key],0);mesh.RecalculateNormals();string path=Data+mesh.name+".asset";var prior=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(prior==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,prior);mesh=prior;}
                var go=new GameObject(mesh.name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=materials[item.Key[0]-'0'];go.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            }
        }
        static void Bench(Transform parent,Vector3 p,Material wood)
        {Box(parent,"Workbench",p+new Vector3(1,1.1f,1),new Vector3(2.6f,.22f,1.2f),wood,true);foreach(int sign in new[]{-1,1})Box(parent,"BenchLeg",p+new Vector3(1+sign,.5f,1),new Vector3(.2f,1,.8f),wood,true);}
        static void Bell(Transform parent,Vector3 p,Material wood,Material brass)
        {
            foreach(int side in new[]{-1,1})Box(parent,"BellSupport",p+new Vector3(side*1.3f,1.7f,0),new Vector3(.3f,3.4f,.4f),wood,true);
            Box(parent,"BellBeam",p+new Vector3(0,3.4f,0),new Vector3(3.2f,.35f,.5f),wood,true);
            var vertices=new List<Vector3>();var triangles=new List<int>();const int sides=16;float[] radii={.8f,.55f,.4f,.25f};float[] ys={1.65f,2.15f,2.75f,2.95f};
            for(int ring=0;ring<4;ring++)for(int i=0;i<sides;i++){float angle=i*Mathf.PI*2/sides;vertices.Add(new Vector3(Mathf.Cos(angle)*radii[ring],ys[ring],Mathf.Sin(angle)*radii[ring]));if(ring<3){int a=ring*sides+i,b=ring*sides+(i+1)%sides;triangles.AddRange(new[]{a,b,b+sides,a,b+sides,a+sides});}}
            var mesh=new Mesh{name="RefugeBell"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();string path=Data+"Bell.asset";var prior=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(prior==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,prior);mesh=prior;}
            var go=new GameObject("Sino do refugio",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.transform.position=p;go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=brass;
        }
    }
}
