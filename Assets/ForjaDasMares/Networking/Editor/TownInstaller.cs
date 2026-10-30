using System;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
namespace ForjaDasMares.Networking.Editor
{
    public static class TownInstaller
    {
        public const string ScenePath="Assets/ForjaDasMares/Scenes/RefugioUltimoSino.unity";
        const string Data="Assets/ForjaDasMares/Data/World/Town/",Art="Assets/ForjaDasMares/Art/World/Act1/";
        static Transform root;static TownHub hub;
        public static void Apply()
        {
            Directory.CreateDirectory(Data);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(ActOneInstaller.ScenePath);
            foreach(var go in scene.GetRootGameObjects())if(go.name=="ActOneEnvironment"||go.name=="ActOneNavigation")UnityEngine.Object.DestroyImmediate(go);
            var world=UnityEngine.Object.FindFirstObjectByType<RegionalWorld>();
            var region=AssetDatabase.LoadAssetAtPath<RegionDefinition>(Data+"Region.asset");if(region==null){region=ScriptableObject.CreateInstance<RegionDefinition>();AssetDatabase.CreateAsset(region,Data+"Region.asset");}
            region.regionId="refugio-ultimo-sino";region.displayName="Refúgio do Último Sino · Cidade segura";region.refuge=new Vector3(0,.16f,0);region.portal=new Vector3(0,.08f,-31);region.bounds=new Bounds(Vector3.zero,new Vector3(76,30,76));region.camps=Array.Empty<RegionCampDefinition>();region.route=new[]{Vector3.zero,new Vector3(0,0,-31)};region.connections=Array.Empty<RegionConnection>();region.landmarks=new[]{new RegionLandmark{label="Nara",position=new Vector3(-10,0,-4),color=Color.yellow},new RegionLandmark{label="Daro",position=new Vector3(10,0,-4),color=Color.yellow},new RegionLandmark{label="Treino",position=new Vector3(24,0,-21),color=Color.cyan}};
            world.Configure(region,world.Hero,world.Enemies,UnityEngine.Object.FindFirstObjectByType<ArenaCamera>(),AssetDatabase.LoadAssetAtPath<SoundscapeLibrary>("Assets/ForjaDasMares/Resources/CampaignSoundscape.asset"));
            var journey=world.GetComponent<RegionalJourney>();var story=UnityEngine.Object.Instantiate(journey.Definition);var existing=AssetDatabase.LoadAssetAtPath<RegionStoryDefinition>(Data+"Story.asset");if(existing==null){AssetDatabase.CreateAsset(story,Data+"Story.asset");existing=story;}else{EditorUtility.CopySerialized(story,existing);UnityEngine.Object.DestroyImmediate(story);}existing.points[0].position=new Vector3(-10,.08f,-4);existing.points[1].position=new Vector3(10,.08f,-4);journey.Configure(world,existing);
            hub=world.gameObject.AddComponent<TownHub>();hub.Configure(world,new[]{region.refuge,new Vector3(-1.4f,.16f,0),new Vector3(1.4f,.16f,0),new Vector3(0,.16f,1.4f)});
            root=new GameObject("TownEnvironment").transform;
            Material stone=Material("Stone",new Color(.45f,.49f,.48f)),wood=Material("Wood",new Color(.34f,.18f,.085f)),plaster=Material("Plaster",new Color(.73f,.65f,.48f)),roof=Material("Roof",new Color(.15f,.28f,.31f)),gold=Material("Bronze",new Color(.7f,.46f,.14f)),blue=Material("ArrivalRunes",new Color(.05f,.72f,.85f));blue.EnableKeyword("_EMISSION");blue.SetColor("_EmissionColor",new Color(.03f,.5f,.75f));EditorUtility.SetDirty(blue);
            var floor=Material("Paving",Color.white);floor.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ForjaDasMares/Art/World/Coast/Soil.png");floor.mainTextureScale=Vector2.one*10;EditorUtility.SetDirty(floor);
            Box("TownFloor",new Vector3(0,-.2f,0),new Vector3(76,.4f,76),floor,true,8);
            foreach(int side in new[]{-1,1}){Box("BoundaryWall",new Vector3(side*36,1.1f,0),new Vector3(.8f,2.2f,73),stone,true);Box("BoundaryWall",new Vector3(0,1.1f,side*36),new Vector3(73,2.2f,.8f),stone,true);}
            House("Oficina do Daro",new Vector3(24,0,11),wood,plaster,roof);House("Arquivo da Nara",new Vector3(-24,0,11),wood,plaster,roof);House("Estalagem do Sino",new Vector3(0,0,27),wood,plaster,roof);
            Model("RefugeTent",new Vector3(-23,0,-19),.75f,135,true);Model("RefugeTent",new Vector3(23,0,-9),.65f,225,true);Model("GuardianStatue",new Vector3(-24,0,28),.35f,150,true);Model("Gravewood",new Vector3(25,0,28),.55f,50,true);
            // Four arrival positions on a traversable disk. No solid altar around the player.
            var plate=GameObject.CreatePrimitive(PrimitiveType.Cylinder);plate.name="ArrivalTransportPlate";FlatCylinderCollider(plate);plate.transform.SetParent(root,false);plate.transform.localPosition=new Vector3(0,.025f,0);plate.transform.localScale=new Vector3(6,.025f,6);plate.layer=8;plate.GetComponent<Renderer>().sharedMaterial=stone;
            for(int i=0;i<16;i++){float a=i*Mathf.PI/8;var rune=Box("ArrivalRune",new Vector3(Mathf.Cos(a)*2.5f,.065f,Mathf.Sin(a)*2.5f),new Vector3(.12f,.025f,.48f),blue,false);rune.transform.rotation=Quaternion.Euler(0,-i*22.5f,0);}
            Label("ÂNCORA DO REFÚGIO",new Vector3(0,3.1f,1),.05f);
            var nara=Resident("Nara · Cartógrafa",new Vector3(-10,0,-4),new Color(.1f,.5f,.57f));UnityEventTools.AddPersistentListener(nara.Used,hub.TalkStory);
            var daro=Resident("Daro · Artífice",new Vector3(10,0,-4),new Color(.65f,.3f,.12f));UnityEventTools.AddPersistentListener(daro.Used,hub.TalkStory);
            var healer=Resident("Iria · Guardiã da fonte",new Vector3(-11,0,14),new Color(.76f,.77f,.6f));UnityEventTools.AddPersistentListener(healer.Used,hub.Rest);
            var mentor=Resident("Luma · Instrutora",new Vector3(18,0,-23),new Color(.34f,.37f,.55f));UnityEventTools.AddPersistentListener(mentor.Used,hub.Mentor);
            var merchant=Resident("Téo · Mercador",new Vector3(-19,0,-14),new Color(.48f,.3f,.46f));UnityEventTools.AddStringPersistentListener(merchant.Used,hub.Speak,"Téo: prepare seu equipamento antes de sair. Minha banca ainda está sendo montada; não há comércio nesta prévia.");
            for(int i=0;i<3;i++){var npc=Resident(new[]{"Ari · Mensageiro","Bento · Carregador","Sia · Viajante"}[i],new Vector3(-12+i*12,0,8),new Color(.3f+i*.13f,.4f,.25f),new[]{new Vector3(-12+i*10,0,8),new Vector3(-15+i*12,0,19),new Vector3(-5+i*8,0,-12)});UnityEventTools.AddStringPersistentListener(npc.Used,hub.Speak,"Morador: o sino voltou a reunir viajantes. Há rastros da expedição além do portão sul.");}
            // Workbenches, market canopy, barrels and planters share a small material set.
            for(int i=0;i<4;i++){Vector3 p=new Vector3(-18+i*12,0,21);Box("BenchSeat",p+Vector3.up*.6f,new Vector3(2.8f,.18f,.7f),wood,true);foreach(int s in new[]{-1,1})Box("BenchLeg",p+new Vector3(s,.3f,0),new Vector3(.25f,.6f,.6f),wood,true);}
            Box("WorkshopBench",new Vector3(17,1.1f,8),new Vector3(3,.25f,1.3f),wood,true);Box("MarketCounter",new Vector3(-19,1,-18),new Vector3(4,2,1.3f),wood,true);Box("MarketCanopy",new Vector3(-19,3.3f,-18),new Vector3(5,.2f,3),roof,true);
            for(int i=0;i<8;i++)Box("SupplyCrate",new Vector3(i<4?-30:30,.5f,-25+i%4*2.5f),new Vector3(1,1,1),wood,true);
            foreach(int side in new[]{-1,1})for(int i=0;i<3;i++){Vector3 p=new Vector3(side*29,.35f,-2+i*5);Box("Planter",p,new Vector3(1.5f,.7f,2),stone,true);var bush=GameObject.CreatePrimitive(PrimitiveType.Sphere);bush.name="GeometricShrub";bush.transform.SetParent(root,false);bush.transform.position=p+Vector3.up*.7f;bush.transform.localScale=new Vector3(1.2f,1.4f,1.5f);bush.GetComponent<Renderer>().sharedMaterial=Material("Leaves",new Color(.18f,.36f,.16f));UnityEngine.Object.DestroyImmediate(bush.GetComponent<Collider>());}
            Model("CoalBrazier",new Vector3(-9,0,10),.18f,0,true);
            var fountain=GameObject.CreatePrimitive(PrimitiveType.Cylinder);fountain.name="RestFountain";FlatCylinderCollider(fountain);fountain.transform.SetParent(root,false);fountain.transform.position=new Vector3(-11,.35f,10);fountain.transform.localScale=new Vector3(2,.35f,2);fountain.GetComponent<Renderer>().sharedMaterial=blue;fountain.layer=9;var rest=fountain.AddComponent<WorldClickTarget>();rest.Configure("Descansar na fonte",3);UnityEventTools.AddPersistentListener(rest.Used,hub.Rest);
            var exit=Box("SouthExpeditionGate",new Vector3(0,1.5f,-32),new Vector3(5,3,.4f),roof,true);Label("EXPEDIÇÃO · COSTA DAS RUÍNAS",new Vector3(0,4,-31),.05f);var door=exit.AddComponent<WorldClickTarget>();door.Configure("Sair para a Costa",4);UnityEventTools.AddPersistentListener(door.Used,hub.Explore);
            var dummy=Box("TrainingDummy",new Vector3(25,1,-22),new Vector3(.9f,2,.9f),wood,true,13);dummy.AddComponent<Health>().Configure(10000,CombatFaction.Training);Label("TREINO",new Vector3(25,3,-22),.05f);
            var nav=new GameObject("TownNavigation").AddComponent<NavMeshSurface>();nav.layerMask=(1<<8)|(1<<9)|(1<<13);nav.collectObjects=CollectObjects.All;nav.useGeometry=NavMeshCollectGeometry.PhysicsColliders;nav.BuildNavMesh();string navPath=Data+"Navigation.asset";var prior=AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);if(prior==null)AssetDatabase.CreateAsset(nav.navMeshData,navPath);else{EditorUtility.CopySerialized(nav.navMeshData,prior);nav.navMeshData=prior;}
            foreach(var go in UnityEngine.Object.FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None)){if(!NavMesh.SamplePosition(go.Approach,out _,2.5f,NavMesh.AllAreas))throw new InvalidOperationException("Unreachable town interaction: "+go.name);}
            EditorUtility.SetDirty(region);EditorUtility.SetDirty(existing);EditorSceneManager.SaveScene(scene,ScenePath);var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(x=>x.path==ScenePath))scenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();Debug.Log("TOWN_READY safe=true NPCs=8 roaming=3 arrivals=4 local=true");
        }
        static Material Material(string name,Color color){string p=Data+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.color=color;m.enableInstancing=true;m.SetFloat("_Smoothness",.2f);EditorUtility.SetDirty(m);return m;}
        static void FlatCylinderCollider(GameObject go){UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.AddComponent<MeshCollider>().sharedMesh=go.GetComponent<MeshFilter>().sharedMesh;}
        static GameObject Box(string name,Vector3 p,Vector3 size,Material mat,bool collision,int layer=9){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root,false);g.transform.position=p;g.transform.localScale=size;g.layer=layer;g.GetComponent<Renderer>().sharedMaterial=mat;if(!collision)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
        static TextMesh Label(string text,Vector3 p,float size){var t=new GameObject(text).AddComponent<TextMesh>();t.transform.SetParent(root,false);t.transform.position=p;t.transform.rotation=Quaternion.Euler(50,0,0);t.text=text;t.fontSize=48;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(1,.86f,.6f);return t;}
        static void House(string name,Vector3 p,Material wood,Material plaster,Material roof){Box(name,p+Vector3.up*2,new Vector3(9,4,7),plaster,true);Box("Door",p+new Vector3(0,1.2f,-3.55f),new Vector3(1.6f,2.4f,.15f),wood,false);for(int s=-1;s<=1;s+=2){var panel=Box("Roof",p+new Vector3(s*2.4f,4.8f,0),new Vector3(5.5f,.3f,8),roof,true);panel.transform.rotation=Quaternion.Euler(0,0,-s*22);Box("Window",p+new Vector3(s*2.8f,2,-3.55f),new Vector3(1.3f,1.3f,.1f),roof,false);}Label(name,p+new Vector3(0,3.6f,-4.2f),.04f);}
        static void Model(string name,Vector3 p,float scale,float yaw,bool collision){var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Art+name+"/"+name+".prefab");var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.transform.SetParent(root,false);go.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));go.transform.localScale=Vector3.one*scale;go.layer=9;if(!collision)return;var renderer=go.GetComponentsInChildren<Renderer>().First(r=>r.name=="LOD0");Bounds b=renderer.localBounds;var points=new Bounds();bool first=true;for(int i=0;i<8;i++){var v=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));v=go.transform.InverseTransformPoint(renderer.transform.TransformPoint(v));if(first){points=new Bounds(v,Vector3.zero);first=false;}else points.Encapsulate(v);}var col=go.AddComponent<BoxCollider>();col.center=points.center;col.size=points.size;}
        static WorldClickTarget Resident(string name,Vector3 p,Color tint,Vector3[] path=null)
        {
            var actor=new GameObject(name);actor.transform.SetParent(root,false);actor.transform.position=p;var collider=actor.AddComponent<CapsuleCollider>();collider.radius=.32f;collider.height=1.8f;collider.center=Vector3.up*.9f;var target=actor.AddComponent<WorldClickTarget>();target.Configure("Conversar · "+name);var coat=Material("Cloth_"+name.Split(' ')[0],tint);var skin=Material("Skin",new Color(.65f,.43f,.29f));
            Transform Part(string label,Vector3 pos,Vector3 size,Material mat){var g=Box(label,Vector3.zero,size,mat,false,0);g.transform.SetParent(actor.transform,false);g.transform.localPosition=pos;return g.transform;}
            Part("Body",new Vector3(0,1,0),new Vector3(.65f,.65f,.35f),coat);Part("Head",new Vector3(0,1.57f,0),new Vector3(.38f,.42f,.35f),skin);
            var l=Part("LeftLeg",new Vector3(-.18f,.4f,0),new Vector3(.22f,.75f,.24f),coat);var r=Part("RightLeg",new Vector3(.18f,.4f,0),new Vector3(.22f,.75f,.24f),coat);var a=Part("LeftArm",new Vector3(-.44f,.98f,0),new Vector3(.2f,.65f,.22f),skin);var b=Part("RightArm",new Vector3(.44f,.98f,0),new Vector3(.2f,.65f,.22f),skin);var label=Label(name,p+Vector3.up*2.1f,.035f);label.transform.SetParent(actor.transform,true);actor.AddComponent<TownResident>().Configure(path,l,r,a,b,label.transform);return target;
        }
    }
}
