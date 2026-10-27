using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using ForjaDasMares.Editor;
namespace ForjaDasMares.Networking.Editor
{
    public static class CampaignInstaller
    {
        public const string ScenePath="Assets/ForjaDasMares/Scenes/CampaignCoop.unity";
        public const string DataRoot="Assets/ForjaDasMares/Data/Campaign/";
        [MenuItem("Forja das Mares/Campaign/Install campaign")]
        public static void Apply()
        {
            if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            Directory.CreateDirectory(DataRoot);AssetDatabase.Refresh();
            var levels=Levels();var catalog=Catalog();
            var scene=EditorSceneManager.OpenScene(File.Exists(ScenePath)?ScenePath:CoopInstaller.ScenePath);
            var director=UnityEngine.Object.FindFirstObjectByType<CoopDirector>();var dungeon=UnityEngine.Object.FindFirstObjectByType<DungeonSession>();
            var run=director.GetComponent<CampaignRun>()??director.gameObject.AddComponent<CampaignRun>();run.Configure(dungeon,levels);EditorUtility.SetDirty(run);
            var progression=director.GetComponent<CoopProgression>();progression.Configure(catalog);
            var ps=new SerializedObject(progression);ps.FindProperty("profileDirectory").stringValue="CampaignProfiles-v1";ps.ApplyModifiedPropertiesWithoutUndo();
            var sessionData=new SerializedObject(dungeon);var biomes=sessionData.FindProperty("biomes");biomes.arraySize=3;
            string[] ids={"catacumbas","floresta-subterranea","fortaleza-vulcanica"};
            for(int i=0;i<3;i++)biomes.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<BiomeDefinition>("Assets/ForjaDasMares/Data/Dungeons/"+ids[i]+".asset");
            sessionData.ApplyModifiedPropertiesWithoutUndo();
            var generator=dungeon.Generator;generator.ConfigureStyle(AssetDatabase.LoadAssetAtPath<DungeonStyleDefinition>(GaleriasInstaller.StylePath));
            var mapData=new SerializedObject(generator);var rooms=mapData.FindProperty("roomDefinitions");
            for(int i=0;i<8;i++)rooms.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<RoomDefinition>("Assets/ForjaDasMares/Data/MapStyles/Galerias/Room_"+(RoomCategory)i+".asset");
            mapData.ApplyModifiedPropertiesWithoutUndo();
            var warrior=dungeon.Heroes.transform.Find("HeroVisual_warrior");
            if(warrior.GetComponentInChildren<HeroModelView>(true)==null)
            {
                var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BrasaModelInstaller.PrefabPath));model.transform.SetParent(warrior,false);model.name="Brasa_Model";
            }
            foreach(string label in new[]{"Body_Placeholder","Head_Placeholder","FacingMarker","GreatSword"}){var old=warrior.Find(label);if(old!=null)old.gameObject.SetActive(false);}
            // Network combat uses its existing authoritative basic/special commands.
            dungeon.Heroes.GetComponent<HeroCombat>().ConfigureFlow(null);
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
            var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(x=>x.path==ScenePath))scenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();
            Debug.Log("CAMPAIGN_INSTALLED levels=3 emptyStarterInventory=true rooms=12,15,18");
        }
        static ProgressionCatalog Catalog()
        {
            string path=DataRoot+"Catalog.asset";var data=AssetDatabase.LoadAssetAtPath<ProgressionCatalog>(path);if(data!=null)return data;
            data=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<ProgressionCatalog>("Assets/ForjaDasMares/Data/Progression/Catalog.asset"));
            data.name="CampaignCatalog";data.starterItems=Array.Empty<string>();data.inventoryCapacity=36;data.expeditionCapacity=32;
            data.enemy.itemChance=.015f;data.enemy.guaranteedItems=0;data.enemy.experience=3;data.enemy.coins=1;
            data.room.experience=12;data.room.coins=3;data.room.itemChance=0;data.room.guaranteedItems=0;
            data.chest.guaranteedItems=1;data.chest.itemChance=.15f;data.boss.guaranteedItems=1;data.boss.itemChance=.3f;data.completion.guaranteedItems=0;data.completion.itemChance=.5f;
            data.Validate();AssetDatabase.CreateAsset(data,path);return data;
        }
        static CampaignLevel[] Levels()
        {
            string[] names={"01 · Estrada das Cinzas","02 · Raízes do Breu","03 · Forja dos Dois Sóis"};
            var result=new CampaignLevel[3];
            var enemies=AssetDatabase.FindAssets("t:EnemyDefinition",new[]{"Assets/ForjaDasMares/Data/Enemies"}).Select(id=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(id))).ToArray();
            EnemyDefinition Enemy(EnemyKind kind)=>enemies.First(x=>x.kind==kind);
            var sourceBoss=AssetDatabase.LoadAssetAtPath<BiomeDefinition>("Assets/ForjaDasMares/Data/Dungeons/catacumbas.asset").boss;
            for(int tier=0;tier<3;tier++)
            {
                string path=DataRoot+"Level"+(tier+1)+".asset";var level=AssetDatabase.LoadAssetAtPath<CampaignLevel>(path);
                if(level!=null){result[tier]=level;continue;}
                level=ScriptableObject.CreateInstance<CampaignLevel>();level.levelId="campaign-"+(tier+1);level.displayName=names[tier];level.seed=17411+tier*331;level.biomeIndex=tier;
                level.description=tier==0?"Catacumbas · solo ou grupo de 1 a 4 · placas e flechas":tier==1?"Floresta subterrânea · arqueiros e emboscadas":"Fortaleza vulcânica · invocadores e corredores de risco";
                int count=12+tier*3;level.route=new Vector2Int[count];level.categories=new RoomCategory[count];
                for(int i=0;i<count;i++){int row=i/4,x=i%4;level.route[i]=new Vector2Int(row%2==0?x:3-x,row);level.categories[i]=i==0?RoomCategory.Entrance:i==count-1?RoomCategory.Boss:i==count-2?RoomCategory.Rest:i%6==2?RoomCategory.Event:i%6==4?RoomCategory.Trap:i%7==6?RoomCategory.Treasure:i%3==0?RoomCategory.HardCombat:RoomCategory.Combat;}
                level.plateHold=1.5f+tier*.6f;level.dartInterval=4.2f-tier*.5f;
                var hit=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/ForjaDasMares/Data/Dungeons/TrapHit.asset"));hit.damage=8+tier*3;hit.knockback=0;AssetDatabase.CreateAsset(hit,DataRoot+"DartHit"+tier+".asset");level.dartAttack=hit;
                level.mist=Material("Mist"+tier,"Forja/CampaignMist",tier==0?new Color(.15f,.21f,.23f):tier==1?new Color(.10f,.23f,.17f):new Color(.26f,.12f,.08f));
                level.challengeMaterial=Material("Challenge","Universal Render Pipeline/Unlit",new Color(.85f,.6f,.2f));level.atmosphere=tier==0?new Color(.12f,.18f,.20f):tier==1?new Color(.09f,.18f,.13f):new Color(.20f,.085f,.05f);
                level.normal=Encounter(tier,false,Enemy);level.hard=Encounter(tier,true,Enemy);
                level.boss=UnityEngine.Object.Instantiate(sourceBoss);level.boss.encounterId="campaign-boss-"+tier;level.boss.displayName="Guardião da etapa "+(tier+1);AssetDatabase.CreateAsset(level.boss,DataRoot+"Boss"+tier+".asset");
                level.Validate();AssetDatabase.CreateAsset(level,path);result[tier]=level;
            }
            return result;
        }
        static Material Material(string name,string shader,Color color)
        {
            string path=DataRoot+name+".mat";var data=AssetDatabase.LoadAssetAtPath<Material>(path);if(data!=null)return data;
            data=new Material(Shader.Find(shader));data.SetColor("_BaseColor",color);AssetDatabase.CreateAsset(data,path);return data;
        }
        static EncounterDefinition Encounter(int tier,bool hard,Func<EnemyKind,EnemyDefinition> enemy)
        {
            var data=ScriptableObject.CreateInstance<EncounterDefinition>();data.encounterId="campaign-"+tier+"-"+hard;data.displayName=hard?"Emboscada coordenada":"Patrulhas da estrada";data.maximumMobileEnemies=10;data.maximumPcEnemies=10;
            int count=3+(tier==2||hard&&tier>0?1:0);data.waves=new EncounterWave[count];
            for(int i=0;i<count;i++)
            {
                var entries=new System.Collections.Generic.List<SpawnEntry>{new SpawnEntry{enemy=enemy(EnemyKind.Raider),count=5,delay=1.8f},new SpawnEntry{enemy=enemy(EnemyKind.Archer),count=2,delay=1.8f},new SpawnEntry{enemy=enemy(EnemyKind.Guardian),count=hard?2:1,delay=1.8f}};
                if(tier>0)entries.Add(new SpawnEntry{enemy=enemy(EnemyKind.Unstable),count=tier,delay=1.8f});
                if(tier==2&&hard)entries.Add(new SpawnEntry{enemy=enemy(EnemyKind.Summoner),count=1,delay=1.8f});
                data.waves[i]=new EncounterWave{displayName="Onda "+(i+1),entries=entries.ToArray(),completion=WaveEndCondition.EliminateAll,delayBefore=3};
            }
            AssetDatabase.CreateAsset(data,DataRoot+"Encounter"+tier+(hard?"Hard":"Normal")+".asset");return data;
        }
        public static void Windows()=>Build(BuildTarget.StandaloneWindows64);
        public static void Android()=>Build(BuildTarget.Android);
        static void Build(BuildTarget target)
        {
            if(EditorUserBuildSettings.activeBuildTarget!=target)throw new InvalidOperationException("Incorrect active build target.");
            if(string.IsNullOrEmpty(Application.cloudProjectId))throw new InvalidOperationException("Unity Services project must remain linked.");
            string folder=target==BuildTarget.Android?"Builds/Atual/Android":"Builds/Atual/Windows";Directory.CreateDirectory(folder);
            try
            {
                // Windows packaged launchers can redirect the default Temp path while Java's
                // Unix-domain socket connect resolves the original path. Use a project-local
                // temporary directory for this Editor process, including Gradle shutdown.
                // User/system environment and the calling process remain unchanged.
                if(target==BuildTarget.Android&&Application.platform==RuntimePlatform.WindowsEditor)
                {string javaTemp=Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/JavaTmp"));Directory.CreateDirectory(javaTemp);Environment.SetEnvironmentVariable("TEMP",javaTemp);Environment.SetEnvironmentVariable("TMP",javaTemp);}
                PlayerSettings.productName="Forja das Mares - Campanha";PlayerSettings.bundleVersion="0.18.0";PlayerSettings.runInBackground=true;
                if(target==BuildTarget.Android){PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.forjadasmares.campaign");PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.Android.bundleVersionCode=46;PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)35;EditorUserBuildSettings.buildAppBundle=false;}
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath,CoopInstaller.ScenePath,GaleriasInstaller.DungeonScene,GaleriasInstaller.ArenaScene,ProgressionInstaller.DungeonScene,ProgressionInstaller.ArenaScene},target=target,locationPathName=folder+(target==BuildTarget.Android?"/ForjaDasMaresCampanha.apk":"/ForjaDasMaresCampanha.exe"),options=BuildOptions.Development});
                Debug.Log("CAMPAIGN_BUILD "+target+" "+report.summary.result+" errors="+report.summary.totalErrors);
                if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Campaign build failed; inspect the generated project and log.");
            }
            finally{AssetDatabase.SaveAssets();}
        }
    }
}
