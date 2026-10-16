using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ForjaDasMares.Editor
{
    public static class BrasaModelInstaller
    {
        const string Root="Assets/ForjaDasMares/Art/Characters/Brasa/";
        public const string PrefabPath=Root+"Brasa.prefab";
        public const string ControllerPath=Root+"Brasa.controller";
        [Serializable] sealed class Palette { public Entry[] materials; }
        [Serializable] sealed class Entry { public string name;public float[] color;public float roughness,metallic; }
        static readonly string[] Names={"Idle","Walk","Attack","Skill","Dodge","Hit","Dead","Victory","Attack2","Attack3","Charged","Super"};
        [MenuItem("Forja das Mares/Characters/Install Brasa in Galerias")]
        public static void Apply()
        {
            if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            AssetDatabase.Refresh();
            if(!File.Exists(Root+"Brasa.fbx"))throw new FileNotFoundException("Generate Brasa in Blender first.");
            var palette=JsonUtility.FromJson<Palette>(File.ReadAllText(Root+"palette.json"));
            Directory.CreateDirectory(Root+"Materials");AssetDatabase.Refresh();
            var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"Brasa.fbx");
            importer.animationType=ModelImporterAnimationType.Generic;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation=true;importer.optimizeGameObjects=false;importer.preserveHierarchy=true;
            importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.materialLocation=ModelImporterMaterialLocation.InPrefab;
            importer.animationCompression=ModelImporterAnimationCompression.Off;importer.isReadable=false;
            foreach(var entry in palette.materials)
            {
                string path=Root+"Materials/"+entry.name+".mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
                material.SetColor("_BaseColor",new Color(entry.color[0],entry.color[1],entry.color[2],entry.color[3]));
                material.SetFloat("_Smoothness",1-entry.roughness);material.SetFloat("_Metallic",entry.metallic);EditorUtility.SetDirty(material);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),entry.name),material);
            }
            importer.SaveAndReimport();
            var settings=importer.defaultClipAnimations;
            foreach(var clip in settings)
            {
                string name=Names.FirstOrDefault(n=>clip.name.EndsWith(n,StringComparison.Ordinal));
                if(name==null)continue;clip.name=name;clip.loopTime=name=="Idle"||name=="Walk"||name=="Victory";
                clip.lockRootRotation=clip.lockRootPositionXZ=clip.lockRootHeightY=true;
            }
            importer.clipAnimations=settings;importer.SaveAndReimport();
            var clips=AssetDatabase.LoadAllAssetsAtPath(Root+"Brasa.fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();
            Debug.Log("BRASA_IMPORTED_CLIPS "+string.Join(",",clips.Select(c=>c.name)));
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            if(!controller.parameters.Any(p=>p.name=="State"))controller.AddParameter("State",AnimatorControllerParameterType.Int);
            if(!controller.parameters.Any(p=>p.name=="Speed"))controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
            if(!controller.parameters.Any(p=>p.name=="Alive"))controller.AddParameter("Alive",AnimatorControllerParameterType.Bool);
            if(!controller.parameters.Any(p=>p.name=="ActionTime"))controller.AddParameter("ActionTime",AnimatorControllerParameterType.Float);
            var machine=controller.layers[0].stateMachine;
            foreach(var transition in machine.anyStateTransitions)machine.RemoveAnyStateTransition(transition);
            for(int i=0;i<Names.Length;i++)
            {
                var clip=clips.SingleOrDefault(c=>c.name==Names[i]);
                if(clip==null||AnimationUtility.GetCurveBindings(clip).Length==0)throw new InvalidOperationException("Missing real bone clip: "+Names[i]);
                var state=machine.states.Select(s=>s.state).FirstOrDefault(s=>s.name==Names[i])??machine.AddState(Names[i],new Vector3(240*(i%4),100*(i/4),0));
                state.motion=clip;
                state.writeDefaultValues=true;
                state.timeParameterActive=Names[i].StartsWith("Attack")||Names[i]=="Skill"||Names[i]=="Charged"||Names[i]=="Super"||Names[i]=="Dodge"||Names[i]=="Hit";
                if(state.timeParameterActive)state.timeParameter="ActionTime";
                if(i==0)machine.defaultState=state;
                if(i==1){state.speedParameter="Speed";state.speedParameterActive=true;}
            }
            EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();
            var prefabRoot=new GameObject("Brasa_Model");
            try
            {
                var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Brasa.fbx"));
                model.transform.SetParent(prefabRoot.transform,false);model.transform.localScale=Vector3.one*.86f;
                var animator=model.GetComponent<Animator>()??model.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                var hand=Find(model.transform,"Hand.R");var off=Find(model.transform,"Hand.L");var sword=Find(model.transform,"GreatSword");
                if(hand==null||off==null||sword==null)throw new InvalidOperationException("Missing hand socket or sword in FBX.");
                prefabRoot.AddComponent<HeroModelView>().Configure(animator,hand,off,sword.gameObject);
                foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>())
                {skin.updateWhenOffscreen=false;if(skin.sharedMesh==null||skin.bones.Length<1)throw new InvalidOperationException("Unbound skin.");}
                PrefabUtility.SaveAsPrefabAsset(prefabRoot,PrefabPath);
            }
            finally{UnityEngine.Object.DestroyImmediate(prefabRoot);}
            AssetDatabase.SaveAssets();
            foreach(string path in new[]{GaleriasInstaller.DungeonScene,GaleriasInstaller.ArenaScene})
            {
                var scene=EditorSceneManager.OpenScene(path);var heroes=UnityEngine.Object.FindFirstObjectByType<HeroRoster>();
                var visual=heroes.transform.Find("HeroVisual_warrior");if(visual==null)throw new InvalidOperationException("Existing warrior root missing.");
                foreach(string name in new[]{"Body_Placeholder","Head_Placeholder","FacingMarker","GreatSword"})
                {var old=visual.Find(name);if(old!=null)old.gameObject.SetActive(false);}
                // SaveAsPrefabAsset adopts the filename "Brasa". Find the actual component,
                // not a transient object name; previous name checks appended overlapping skins.
                var views=visual.GetComponentsInChildren<HeroModelView>(true);
                HeroModelView keep=null;
                foreach(var view in views)
                {
                    if(view.transform.parent!=visual||PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(view.gameObject)!=PrefabPath)
                        throw new InvalidOperationException("Unexpected authored model; inspect before replacing.");
                    if(keep==null)keep=view;
                    else UnityEngine.Object.DestroyImmediate(view.gameObject);
                }
                if(keep==null)
                {var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));instance.transform.SetParent(visual,false);keep=instance.GetComponent<HeroModelView>();}
                keep.name="Brasa_Model";keep.gameObject.SetActive(true);
                if(visual.GetComponentsInChildren<HeroModelView>(true).Length!=1)throw new InvalidOperationException("Duplicate Brasa skin.");
                var combat=heroes.GetComponent<HeroCombat>();combat.ConfigureFlow(Flow());EditorUtility.SetDirty(combat);
                string ribbonPath="Assets/ForjaDasMares/Data/MapStyles/Galerias/CombatRibbon.mat";
                var ribbon=AssetDatabase.LoadAssetAtPath<Material>(ribbonPath);
                if(ribbon==null){ribbon=new Material(Shader.Find("Universal Render Pipeline/Unlit"));AssetDatabase.CreateAsset(ribbon,ribbonPath);}
                var feedback=heroes.GetComponent<HeroPowerFeedback>()??heroes.gameObject.AddComponent<HeroPowerFeedback>();feedback.Configure(combat,ribbon);EditorUtility.SetDirty(feedback);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();Debug.Log("BRASA_MODEL_READY scenes=2 clips=12 singlePresentationOwner=true combatFlow=ready");
        }
        static CombatFlowDefinition Flow()
        {
            const string path="Assets/ForjaDasMares/Data/MapStyles/Galerias/CombatFlow.asset";
            var flow=AssetDatabase.LoadAssetAtPath<CombatFlowDefinition>(path);
            if(flow==null){flow=ScriptableObject.CreateInstance<CombatFlowDefinition>();AssetDatabase.CreateAsset(flow,path);}
            const string strikePath="Assets/ForjaDasMares/Data/MapStyles/Galerias/BrasaRupture.asset";
            var strike=AssetDatabase.LoadAssetAtPath<AttackDefinition>(strikePath);
            if(strike==null)
            {
                strike=ScriptableObject.CreateInstance<AttackDefinition>();strike.damage=120;strike.range=5.4f;strike.arc=360;strike.windup=.45f;strike.recovery=.9f;strike.knockback=6;strike.color=new Color(1,.5f,.08f);AssetDatabase.CreateAsset(strike,strikePath);
            }
            flow.superAttack=strike;EditorUtility.SetDirty(flow);return flow;
        }
        static Transform Find(Transform root,string name)
        {if(root.name==name)return root;foreach(Transform child in root){var found=Find(child,name);if(found!=null)return found;}return null;}
    }
}
