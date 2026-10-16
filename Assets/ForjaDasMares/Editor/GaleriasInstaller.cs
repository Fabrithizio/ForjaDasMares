using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ForjaDasMares.Editor
{
    public static class GaleriasInstaller
    {
        public const string DungeonScene = "Assets/ForjaDasMares/Scenes/GaleriasDungeon.unity";
        public const string ArenaScene = "Assets/ForjaDasMares/Scenes/GaleriasArena.unity";
        const string Root = "Assets/ForjaDasMares/Data/MapStyles/Galerias/";
        public const string StylePath = Root + "Style.asset";
        [MenuItem("Forja das Mares/Galerias/Create test map")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Root); AssetDatabase.Refresh(); var style=Style(); var biome=Biome(); var rooms=Rooms();
            if(!File.Exists(DungeonScene))
            {
                var scene=EditorSceneManager.OpenScene(ProgressionInstaller.DungeonScene);
                UnityEngine.Object.FindFirstObjectByType<DungeonGenerator>().ConfigureStyle(style);
                var dungeon=UnityEngine.Object.FindFirstObjectByType<DungeonSession>(); dungeon.ConfigureArenaScene("GaleriasArena");
                var data=new SerializedObject(dungeon); var array=data.FindProperty("biomes"); array.arraySize=1; array.GetArrayElementAtIndex(0).objectReferenceValue=biome;
                data.FindProperty("seed").intValue=73129; data.FindProperty("maximumRooms").intValue=9; data.FindProperty("fixedMap").boolValue=true; data.ApplyModifiedPropertiesWithoutUndo();
                UnityEngine.Object.FindFirstObjectByType<ProgressionSceneBridge>().ConfigureDungeonScene("GaleriasDungeon");
                EditorSceneManager.SaveScene(scene,DungeonScene);
            }
            var styledScene=EditorSceneManager.OpenScene(DungeonScene);
            rooms=Rooms(); // Opening a scene can unload unused asset instances held only in local variables.
            UnityEngine.Object.FindFirstObjectByType<DungeonGenerator>().ConfigureStyle(Style());
            var sessionData=new SerializedObject(UnityEngine.Object.FindFirstObjectByType<DungeonSession>());
            var biomeData=sessionData.FindProperty("biomes"); biomeData.arraySize=1; biomeData.GetArrayElementAtIndex(0).objectReferenceValue=Biome();
            sessionData.ApplyModifiedPropertiesWithoutUndo();
            var generatorData=new SerializedObject(UnityEngine.Object.FindFirstObjectByType<DungeonGenerator>());
            var roomData=generatorData.FindProperty("roomDefinitions"); roomData.arraySize=rooms.Length;
            for(int i=0;i<rooms.Length;i++) { if(rooms[i]==null)throw new InvalidOperationException("Missing style room "+i); roomData.GetArrayElementAtIndex(i).objectReferenceValue=rooms[i]; }
            generatorData.ApplyModifiedPropertiesWithoutUndo(); EditorSceneManager.SaveScene(styledScene);
            if(!File.Exists(ArenaScene))
            {
                var scene=EditorSceneManager.OpenScene(ProgressionInstaller.ArenaScene);
                UnityEngine.Object.FindFirstObjectByType<ProgressionSceneBridge>().ConfigureDungeonScene("GaleriasDungeon");
                EditorSceneManager.SaveScene(scene,ArenaScene);
            }
            var scenes=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach(string path in new[]{DungeonScene,ArenaScene}) if(!scenes.Exists(s=>s.path==path)) scenes.Add(new EditorBuildSettingsScene(path,true));
            EditorBuildSettings.scenes=scenes.ToArray(); AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(DungeonScene);
            var verify=new SerializedObject(UnityEngine.Object.FindFirstObjectByType<DungeonGenerator>());
            for(int i=0;i<8;i++)if(verify.FindProperty("roomDefinitions").GetArrayElementAtIndex(i).objectReferenceValue==null)throw new InvalidOperationException("Room reference not saved: "+i);
            if(UnityEngine.Object.FindFirstObjectByType<DungeonSession>().Biome==null)throw new InvalidOperationException("Biome reference not saved.");
            if(UnityEngine.Object.FindFirstObjectByType<DungeonGenerator>().Style==null)throw new InvalidOperationException("Style reference not saved.");
            Debug.Log("GALERIAS_MAP_READY roomReferences=8 biome=valid style=valid");
        }
        static RoomDefinition[] Rooms()
        {
            var result=new RoomDefinition[8];
            for(int i=0;i<8;i++)
            {
                string name="Room_"+((RoomCategory)i),path=Root+name+".asset";
                var data=AssetDatabase.LoadAssetAtPath<RoomDefinition>(path);
                if(data==null)
                {
                    var source=AssetDatabase.LoadAssetAtPath<RoomDefinition>("Assets/ForjaDasMares/Data/Dungeons/"+name+".asset");
                    if(source==null)throw new InvalidOperationException("Missing existing room data: "+name);
                    data=UnityEngine.Object.Instantiate(source);
                    if(i==1)data.obstaclePositions=new[]{new Vector3(-3.6f,0,-2),new Vector3(3.6f,0,-2),new Vector3(-3.6f,0,3.9f),new Vector3(3.6f,0,3.9f)};
                    if(i==2)data.obstaclePositions=new[]{new Vector3(-3.6f,0,-2),new Vector3(3.6f,0,-2),new Vector3(-2.4f,0,.5f),new Vector3(2.4f,0,.5f)};
                    AssetDatabase.CreateAsset(data,path);
                }
                result[i]=data;
            }
            return result;
        }
        static Mesh MeshAsset(string name,Func<Mesh> make)
        {
            string path=Root+name+".asset"; var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null) { mesh=make(); AssetDatabase.CreateAsset(mesh,path); } return mesh;
        }
        static Material MaterialAsset(string name,Color color)
        {
            string path=Root+name+".mat"; var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null) { material=new Material(Shader.Find("Universal Render Pipeline/Lit")); material.SetColor("_BaseColor",color); material.SetFloat("_Smoothness",.12f); AssetDatabase.CreateAsset(material,path); } return material;
        }
        static DungeonStyleDefinition Style()
        {
            var style=AssetDatabase.LoadAssetAtPath<DungeonStyleDefinition>(StylePath); if(style!=null)return style;
            style=ScriptableObject.CreateInstance<DungeonStyleDefinition>(); style.styleId="galerias-da-mare";
            style.mainRoute=new[]{new Vector2Int(0,0),new Vector2Int(1,0),new Vector2Int(1,1),new Vector2Int(2,1),new Vector2Int(2,2),new Vector2Int(3,2),new Vector2Int(3,3)};
            style.floor=MeshAsset("FloorTiles",()=>GaleriasMeshes.Floor("GaleriasFloor",12.2f,12.2f));
            style.bossFloor=MeshAsset("BossFloor",()=>GaleriasMeshes.Floor("GaleriasBossFloor",12.2f,12.2f,true));
            style.corridorFloor=MeshAsset("CorridorTiles",()=>GaleriasMeshes.Floor("GaleriasCorridor",3.2f,4.4f));
            style.longWallX=MeshAsset("LongWallX",()=>GaleriasMeshes.Wall("GaleriasLongWallX",12.6f,false));
            style.longWallZ=MeshAsset("LongWallZ",()=>GaleriasMeshes.Wall("GaleriasLongWallZ",12.6f,true));
            style.shortWallX=MeshAsset("ShortWallX",()=>GaleriasMeshes.Wall("GaleriasShortWallX",4.5f,false));
            style.shortWallZ=MeshAsset("ShortWallZ",()=>GaleriasMeshes.Wall("GaleriasShortWallZ",4.5f,true));
            style.crate=MeshAsset("Crate",()=>GaleriasMeshes.Crate("GaleriasCrate"));
            style.chestLid=MeshAsset("ChestLid",()=>GaleriasMeshes.Crate("GaleriasChestLid",true));
            style.lantern=MeshAsset("Lantern",()=>GaleriasMeshes.Lantern("GaleriasLantern"));
            style.gate=MeshAsset("Gate",()=>GaleriasMeshes.Gate("GaleriasGate"));
            style.stoneTrim=MaterialAsset("StoneTrim",new Color(.14f,.19f,.24f)); style.wood=MaterialAsset("Wood",new Color(.34f,.16f,.065f));
            style.metal=MaterialAsset("Iron",new Color(.10f,.14f,.17f)); style.glow=MaterialAsset("LanternAmber",new Color(1,.62f,.15f));
            AssetDatabase.CreateAsset(style,StylePath); return style;
        }
        static BiomeDefinition Biome()
        {
            string path=Root+"Biome.asset"; var biome=AssetDatabase.LoadAssetAtPath<BiomeDefinition>(path); if(biome!=null)return biome;
            var source=AssetDatabase.LoadAssetAtPath<BiomeDefinition>("Assets/ForjaDasMares/Data/Dungeons/catacumbas.asset");
            if(source==null)throw new InvalidOperationException("Existing catacomb gameplay data required.");
            biome=UnityEngine.Object.Instantiate(source); biome.name="GaleriasDaMare"; biome.biomeId="galerias-da-mare"; biome.displayName="GALERIAS DA MARE — mapa de teste";
            biome.floor=MaterialAsset("SandstoneFloor",new Color(.38f,.29f,.19f)); biome.decoration=MaterialAsset("LightTiles",new Color(.51f,.41f,.28f));
            biome.wall=MaterialAsset("BlueStone",new Color(.26f,.34f,.38f)); biome.accent=MaterialAsset("AmberMarkings",new Color(.94f,.57f,.18f));
            biome.ambient=new Color(.43f,.46f,.5f); biome.light=new Color(1,.9f,.75f); biome.background=new Color(.025f,.036f,.05f);
            AssetDatabase.CreateAsset(biome,path); return biome;
        }
        [MenuItem("Forja das Mares/Galerias/Open test map")]
        public static void Open() { if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return; EditorSceneManager.OpenScene(DungeonScene); }
        [MenuItem("Forja das Mares/Galerias/Build Windows")]
        public static void Windows() => Build(BuildTarget.StandaloneWindows64);
        [MenuItem("Forja das Mares/Galerias/Build Android (separate app)")]
        public static void Android() => Build(BuildTarget.Android);
        static void Build(BuildTarget target)
        {
            if(EditorUserBuildSettings.activeBuildTarget!=target)throw new InvalidOperationException("Start Unity with -buildTarget "+(target==BuildTarget.Android?"Android":"Win64"));
            if(!File.Exists(DungeonScene)||!File.Exists(ArenaScene))throw new InvalidOperationException("Create Galerias map first.");
            string folder=target==BuildTarget.Android?"Builds/GaleriasAndroid-0.7.1":"Builds/GaleriasWindows-0.7.1"; Directory.CreateDirectory(folder);
            string previousName=PlayerSettings.productName;
            string previousId=PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            try
            {
                PlayerSettings.productName="Forja das Mares - Galerias"; PlayerSettings.bundleVersion="0.7.1";
                if(target==BuildTarget.Android)
                {
                    PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP); PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
                    PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)35; PlayerSettings.Android.bundleVersionCode=16; EditorUserBuildSettings.buildAppBundle=false;
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.forjadasmares.galerias");
                }
                else PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{DungeonScene,ArenaScene,ProgressionInstaller.DungeonScene,ProgressionInstaller.ArenaScene,DungeonInstaller.ScenePath,ArenaBuilder.ScenePath},target=target,locationPathName=Path.Combine(folder,target==BuildTarget.Android?"ForjaDasMaresGalerias.apk":"ForjaDasMaresGalerias.exe"),options=BuildOptions.Development });
                Debug.Log("GALERIAS_BUILD "+target+" "+report.summary.result+" errors="+report.summary.totalErrors);
                if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Galerias build failed. Inspect log.");
            }
            finally { PlayerSettings.productName=previousName; PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,previousId); AssetDatabase.SaveAssets(); }
        }
    }
}
