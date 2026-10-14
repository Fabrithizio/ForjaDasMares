using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ForjaDasMares.Editor
{
    public static class ProjectTechnicalAudit
    {
        [Serializable] sealed class Report
        {
            public string unityVersion;
            public int scenes, prefabs, definitions, behaviours, missingScripts, missingReferences;
            public List<string> problems=new List<string>();
            public List<string> sceneComponents=new List<string>();
        }
        static readonly Dictionary<Type,string[]> Required=new Dictionary<Type,string[]>
        {
            {typeof(PlayerMotor),new[]{"commandSource","cameraBasis"}}, {typeof(ArenaInput),new[]{"actions"}}, {typeof(PlayerAim),new[]{"motor","viewCamera"}}, {typeof(ArenaCamera),new[]{"target"}},
            {typeof(ArenaSession),new[]{"player","spawn","combat","followCamera","trainingTarget"}}, {typeof(HeroRoster),new[]{"heroes","motor","combat","health","presentation"}}, {typeof(HeroCombat),new[]{"motor","health","origin","pool"}},
            {typeof(HeroPresentation),new[]{"models","motor","combat","health"}}, {typeof(CombatEffectPool),new[]{"material","projectileMesh","zoneMesh"}}, {typeof(EnemyPool),new[]{"definitions","heroes","projectiles","warnings"}},
            {typeof(EnemyActor),new[]{"health","brain","combat","motor","presentation","guard"}}, {typeof(DungeonGenerator),new[]{"cubeMesh","cylinderMesh","roomDefinitions","normalNavigation","bossNavigation","trapAttack"}},
            {typeof(DungeonSession),new[]{"generator","encounter","heroes","arena","cameraFollow","input","warnings","biomes","mainLight"}}, {typeof(ProgressionSceneBridge),new[]{"catalog","heroes","equipment","encounter","arena","companion","commands"}},
            {typeof(HeroEquipment),new[]{"heroes","motor","combat","mainVisual","secondaryVisual"}}, {typeof(SupportCompanion),new[]{"owner","health","catalog"}}, {typeof(ProgressionHud),new[]{"bridge","canvas","actions","commands"}}, {typeof(TouchMoveStick),new[]{"input","handle"}}
        };
        public static void Run()
        {
            var report=new Report {unityVersion=Application.unityVersion}; string output="Logs/TechnicalAudit.json"; var args=Environment.GetCommandLineArgs(); for(int i=0;i<args.Length-1;i++) if(args[i]=="-auditOutput") output=args[i+1];
            foreach(string guid in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/ForjaDasMares/Scenes"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid); var scene=EditorSceneManager.OpenScene(path); report.scenes++; var names=new HashSet<string>();
                foreach(var root in scene.GetRootGameObjects()) Scan(root,path,report,names); report.sceneComponents.Add(path+": "+string.Join(", ",names));
            }
            foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/ForjaDasMares"})) { string path=AssetDatabase.GUIDToAssetPath(guid); Scan(AssetDatabase.LoadAssetAtPath<GameObject>(path),path,report,null); report.prefabs++; }
            foreach(string guid in AssetDatabase.FindAssets("t:ScriptableObject",new[]{"Assets/ForjaDasMares/Data"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid); var data=AssetDatabase.LoadAssetAtPath<ScriptableObject>(path); report.definitions++; References(data,path,report);
                try
                {
                    if(data is ProgressionCatalog catalog) catalog.Validate();
                    if(data is HeroDefinition hero && (hero.attacks==null||hero.attacks.Length==0||hero.special==null||hero.special.hit==null)) throw new InvalidOperationException("Incomplete hero.");
                    if(data is EnemyDefinition enemy && enemy.prefab==null) throw new InvalidOperationException("Enemy has no prefab.");
                    if(data is EncounterDefinition encounter) foreach(var wave in encounter.waves) foreach(var entry in wave.entries) if(entry.enemy==null||entry.count<1) throw new InvalidOperationException("Invalid encounter entry.");
                    if(data is BiomeDefinition biome && (biome.floor==null||biome.wall==null||biome.boss==null||biome.rewards==null)) throw new InvalidOperationException("Incomplete biome.");
                }
                catch(Exception error) {report.problems.Add(path+": "+error.Message);}
            }
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))); File.WriteAllText(output,JsonUtility.ToJson(report,true));
            Debug.Log("TECHNICAL_AUDIT scenes="+report.scenes+" prefabs="+report.prefabs+" definitions="+report.definitions+" behaviours="+report.behaviours+" missingScripts="+report.missingScripts+" missingReferences="+report.missingReferences+" problems="+report.problems.Count);
            if(report.problems.Count>0) throw new InvalidOperationException("Technical audit found "+report.problems.Count+" problems. See "+output);
        }
        static void Scan(GameObject root,string path,Report report,HashSet<string> names)
        {
            foreach(var item in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                report.behaviours++; if(item==null) {report.missingScripts++; report.problems.Add(path+": missing MonoBehaviour."); continue;} names?.Add(item.GetType().Name); References(item,path+"/"+item.name,report);
                if(Required.TryGetValue(item.GetType(),out var fields))
                {
                    var serialized=new SerializedObject(item);
                    foreach(string name in fields)
                    {
                        var property=serialized.FindProperty(name); bool valid=property!=null;
                        if(valid && property.isArray) { valid=property.arraySize>0; for(int i=0;i<property.arraySize;i++) if(property.GetArrayElementAtIndex(i).propertyType==SerializedPropertyType.ObjectReference&&property.GetArrayElementAtIndex(i).objectReferenceValue==null) valid=false; }
                        else if(valid && property.propertyType==SerializedPropertyType.ObjectReference) valid=property.objectReferenceValue!=null;
                        if(!valid) {report.missingReferences++; report.problems.Add(path+"/"+item.name+" "+item.GetType().Name+"."+name+": required reference absent.");}
                    }
                }
            }
        }
        static void References(UnityEngine.Object item,string path,Report report)
        {
            if(item==null) {report.problems.Add(path+": asset absent.");return;}
            var serialized=new SerializedObject(item); var property=serialized.GetIterator();
            while(property.Next(true)) if(property.propertyType==SerializedPropertyType.ObjectReference&&property.objectReferenceValue==null&&property.objectReferenceInstanceIDValue!=0)
            {report.missingReferences++;report.problems.Add(path+": broken "+property.propertyPath);}
        }
    }
}
