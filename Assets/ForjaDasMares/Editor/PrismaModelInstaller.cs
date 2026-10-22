using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ForjaDasMares.Editor
{
    public static class PrismaModelInstaller
    {
        const string Root="Assets/ForjaDasMares/Art/Characters/Prisma/";
        public const string PrefabPath=Root+"Prisma.prefab";
        public const string ControllerPath=Root+"Prisma.controller";
        [Serializable] sealed class Palette { public Entry[] materials; }
        [Serializable] sealed class Entry { public string name;public float[] color;public float roughness,metallic; }
        static readonly string[] Names={"Idle","Walk","Attack","Skill","Dodge","Hit","Dead","Victory","Attack2","Attack3","Charged","Super","Run","StaffBolt","StaffReturn","StaffRelease","HeavyStaff","BookWord","BookPage","BookSeal","HeavyBook","OrbCast","OrbReturn","OrbBurst","HeavyOrb"};
        [MenuItem("Forja das Mares/Characters/Install Prisma model and casting kit")]
        public static void Apply()
        {
            if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            AssetDatabase.Refresh();
            if(!File.Exists(Root+"Prisma.fbx"))throw new FileNotFoundException("Generate Prisma in Blender first.");
            var palette=JsonUtility.FromJson<Palette>(File.ReadAllText(Root+"palette.json"));
            Directory.CreateDirectory(Root+"Materials");AssetDatabase.Refresh();
            var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"Prisma.fbx");
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
                string name=Names.FirstOrDefault(n=>clip.name==n||clip.name.EndsWith("|"+n,StringComparison.Ordinal)||clip.name.EndsWith("_"+n,StringComparison.Ordinal));
                if(name==null)continue;clip.name=name;clip.loopTime=name=="Idle"||name=="Walk"||name=="Run"||name=="Victory";
                clip.lockRootRotation=clip.lockRootPositionXZ=clip.lockRootHeightY=true;
            }
            importer.clipAnimations=settings;importer.SaveAndReimport();
            var clips=AssetDatabase.LoadAllAssetsAtPath(Root+"Prisma.fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();
            Debug.Log("PRISMA_IMPORTED_CLIPS "+string.Join(",",clips.Select(c=>c.name)));
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            if(!controller.parameters.Any(p=>p.name=="State"))controller.AddParameter("State",AnimatorControllerParameterType.Int);
            if(!controller.parameters.Any(p=>p.name=="Speed"))controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
            if(!controller.parameters.Any(p=>p.name=="Alive"))controller.AddParameter("Alive",AnimatorControllerParameterType.Bool);
            if(!controller.parameters.Any(p=>p.name=="ActionTime"))controller.AddParameter("ActionTime",AnimatorControllerParameterType.Float);
            if(!controller.parameters.Any(p=>p.name=="GaitTime"))controller.AddParameter("GaitTime",AnimatorControllerParameterType.Float);
            var machine=controller.layers[0].stateMachine;
            foreach(var transition in machine.anyStateTransitions)machine.RemoveAnyStateTransition(transition);
            for(int i=0;i<Names.Length;i++)
            {
                var clip=clips.SingleOrDefault(c=>c.name==Names[i]);
                if(clip==null||AnimationUtility.GetCurveBindings(clip).Length==0)throw new InvalidOperationException("Missing real bone clip: "+Names[i]);
                var state=machine.states.Select(s=>s.state).FirstOrDefault(s=>s.name==Names[i])??machine.AddState(Names[i],new Vector3(240*(i%4),100*(i/4),0));
                state.motion=clip;
                state.writeDefaultValues=true;
                state.timeParameterActive=Names[i]!="Idle"&&Names[i]!="Walk"&&Names[i]!="Run"&&Names[i]!="Dead"&&Names[i]!="Victory"||Names[i]=="Skill"||Names[i]=="Charged"||Names[i]=="Super"||Names[i]=="Dodge"||Names[i]=="Hit";
                if(state.timeParameterActive)state.timeParameter="ActionTime";
                if(Names[i]=="Walk"||Names[i]=="Run"){state.timeParameter="GaitTime";state.timeParameterActive=true;}
                if(i==0)machine.defaultState=state;
                if(i==1){state.speedParameter="Speed";state.speedParameterActive=true;}
            }
            EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();
            var prefabRoot=new GameObject("Prisma_Model");
            try
            {
                var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prisma.fbx"));
                model.transform.SetParent(prefabRoot.transform,false);model.transform.localScale=Vector3.one*.86f;model.transform.localPosition=Vector3.down*.045f;
                var animator=model.GetComponent<Animator>()??model.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                var hand=Find(model.transform,"Hand.R");var off=Find(model.transform,"Hand.L");var sword=Find(model.transform,"PrismaStaff");
                if(hand==null||off==null||sword==null)throw new InvalidOperationException("Missing hand socket or sword in FBX.");
                // A generic transform mask keeps the hips and legs driven by locomotion.
                string maskPath=Root+"PrismaUpperBody.mask";
                var mask=AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);
                if(mask==null){mask=new AvatarMask();AssetDatabase.CreateAsset(mask,maskPath);}
                var transforms=model.GetComponentsInChildren<Transform>(true);mask.transformCount=transforms.Length;
                var spine=Find(model.transform,"Spine");
                for(int j=0;j<transforms.Length;j++)
                {mask.SetTransformPath(j,AnimationUtility.CalculateTransformPath(transforms[j],model.transform));mask.SetTransformActive(j,transforms[j]==spine||transforms[j].IsChildOf(spine));}
                EditorUtility.SetDirty(mask);
                var layers=controller.layers;
                if(layers.Length<2){controller.AddLayer("Upper Body");layers=controller.layers;}
                layers[1].avatarMask=mask;layers[1].defaultWeight=0;layers[1].blendingMode=AnimatorLayerBlendingMode.Override;
                controller.layers=layers;
                var upperMachine=layers[1].stateMachine;
                foreach(string actionName in Names)
                {
                    var upperState=upperMachine.states.Select(x=>x.state).FirstOrDefault(x=>x.name==actionName)??upperMachine.AddState(actionName);
                    upperState.motion=clips.Single(c=>c.name==actionName);upperState.writeDefaultValues=true;
                    upperState.timeParameterActive=actionName!="Idle"&&actionName!="Walk"&&actionName!="Run"&&actionName!="Dead"&&actionName!="Victory"||actionName=="Skill"||actionName=="Charged"||actionName=="Super"||actionName=="Hit";
                    if(upperState.timeParameterActive)upperState.timeParameter="ActionTime";
                    if(actionName=="Idle")upperMachine.defaultState=upperState;
                }
                EditorUtility.SetDirty(controller);
                var view=prefabRoot.AddComponent<HeroModelView>();view.Configure(animator,hand,off,sword.gameObject);
                prefabRoot.AddComponent<BrasaFootPlant>().Configure(view,Find(model.transform,"Thigh.L"),Find(model.transform,"Shin.L"),Find(model.transform,"Foot.L"),Find(model.transform,"Thigh.R"),Find(model.transform,"Shin.R"),Find(model.transform,"Foot.R"));
                foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>())
                {skin.updateWhenOffscreen=false;if(skin.sharedMesh==null||skin.bones.Length<1)throw new InvalidOperationException("Unbound skin.");}
                prefabRoot.AddComponent<PrismaCastingView>().Configure(view,AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Art/Arsenal/CombatTrail.mat"));
                PrefabUtility.SaveAsPrefabAsset(prefabRoot,PrefabPath);
            }
            finally{UnityEngine.Object.DestroyImmediate(prefabRoot);}
            AssetDatabase.SaveAssets();
            var feel=Feel();int count=0;
            foreach(string guid in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/ForjaDasMares/Scenes"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);var scene=EditorSceneManager.OpenScene(path);bool changed=false;
                foreach(var roster in UnityEngine.Object.FindObjectsByType<HeroRoster>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    var visual=roster.transform.Find("HeroVisual_arcanist");if(visual==null)continue;
                    foreach(string partName in new[]{"Body_Placeholder","Head_Placeholder","FacingMarker","Staff","Focus"})
                    {var old=visual.Find(partName);if(old!=null)old.gameObject.SetActive(false);}
                    var models=visual.GetComponentsInChildren<HeroModelView>(true);
                    if(models.Length>1)throw new InvalidOperationException("Inspect duplicate mage models before installing: "+path);
                    if(models.Length==1&&PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(models[0].gameObject)!=PrefabPath)throw new InvalidOperationException("Unexpected mage skin: "+path);
                    if(models.Length==0){var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));model.transform.SetParent(visual,false);model.name="Prisma_Model";}
                    changed=true;count++;
                }
                foreach(var pool in UnityEngine.Object.FindObjectsByType<CombatEffectPool>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {pool.ConfigureTrail(AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Art/Arsenal/CombatTrail.mat"));EditorUtility.SetDirty(pool);changed=true;}
                if(changed)EditorSceneManager.SaveScene(scene);
            }
            foreach(string guid in AssetDatabase.FindAssets("t:HeroDefinition",new[]{"Assets/ForjaDasMares/Data"}))
            {
                var hero=AssetDatabase.LoadAssetAtPath<HeroDefinition>(AssetDatabase.GUIDToAssetPath(guid));if(hero.heroId!="arcanist")continue;
                hero.feedback=feel;hero.strengths="Conjuração à distância; cajado glacial, grimório de selos e orbe de dispersão. Convergência prende grupos.";
                hero.weaknesses="Vida baixa; precisa manter distância. O pesado e o super têm preparação.";EditorUtility.SetDirty(hero);
            }
            UpgradeGrips();ConfigureWeapons();AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene("Assets/ForjaDasMares/Scenes/CampaignCoop.unity");
            Debug.Log("PRISMA_READY sceneActors="+count+" clips=25 authoredModel=true protocolUnchanged=true");
        }
        public static void UpgradeGrips()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ProgressionCatalog>("Assets/ForjaDasMares/Data/Campaign/Forge/Catalog.asset");
            foreach(var item in catalog.equipment.Where(x=>x.category==WeaponCategory.Staff||x.category==WeaponCategory.Grimoire||x.category==WeaponCategory.MagicWeapon))
            {
                item.customGrip=true;item.socketEuler=new Vector3(-90,0,0);
                item.socketPosition=new Vector3(0,0,item.category==WeaponCategory.Staff?.5f:item.category==WeaponCategory.MagicWeapon?.08f:0);
                EditorUtility.SetDirty(item);
            }
            AssetDatabase.SaveAssets();Debug.Log("PRISMA_GRIPS_CONFIGURED");
        }
        static CombatFeelDefinition Feel()
        {
            const string path=Root+"PrismaFeel.asset";var feel=AssetDatabase.LoadAssetAtPath<CombatFeelDefinition>(path);
            if(feel==null){feel=ScriptableObject.CreateInstance<CombatFeelDefinition>();AssetDatabase.CreateAsset(feel,path);}
            feel.swing=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/Prisma_Cast.wav");
            feel.impact=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/Prisma_Impact.wav");
            feel.heavy=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/Prisma_Heavy.wav");
            var brasa=AssetDatabase.FindAssets("t:CombatFeelDefinition",new[]{"Assets/ForjaDasMares/Data"}).Select(g=>AssetDatabase.LoadAssetAtPath<CombatFeelDefinition>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault();
            if(brasa!=null){feel.step=brasa.step;feel.reward=brasa.reward;feel.perfect=brasa.perfect;}
            feel.volume=.24f;feel.impactPause=.018f;feel.cameraImpulse=.01f;
            feel.trailMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Art/Arsenal/CombatTrail.mat");EditorUtility.SetDirty(feel);return feel;
        }
        static void ConfigureWeapons()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ProgressionCatalog>("Assets/ForjaDasMares/Data/Campaign/Forge/Catalog.asset");
            catalog.Find("initiation-arcanist").attacks[0].control=ControlEffect.None;EditorUtility.SetDirty(catalog.Find("initiation-arcanist").attacks[0]);
            foreach(var item in catalog.equipment.Where(x=>!x.starter&&(x.category==WeaponCategory.Staff||x.category==WeaponCategory.Grimoire||x.category==WeaponCategory.MagicWeapon)))
            {
                // Existing slow explosive focus is an intentional exception, not a generic frost staff.
                if(item.itemId=="cinderfocus")continue;
                bool staff=item.category==WeaponCategory.Staff,book=item.category==WeaponCategory.Grimoire;
                item.description=staff?"Cajado glacial. Projéteis precisos que retardam; o terceiro golpe amplia a explosão. Pulso glacial para abrir espaço.":book?"Grimório de brasas. Sequência de fogo com explosão final. E grava à frente um selo que prende quem entrar.":"Orbe de tempestade. Dispersão de projéteis no terceiro golpe; alcance menor. Ruptura de choque afasta inimigos próximos.";
                for(int i=0;i<item.attacks.Length;i++)
                {
                    var hit=item.attacks[i];hit.shape=AttackShape.Projectile;
                    hit.color=staff?new Color(.18f,.8f,1):book?new Color(1,.33f,.12f):new Color(.72f,.38f,1);
                    hit.projectileSpeed=staff?17:book?13:16;hit.range=staff?10:book?9:8;
                    hit.projectileCount=!staff&&!book&&i==2?3:1;hit.projectileSpread=12;
                    hit.damage=staff?(i==2?17:13):book?(i==2?18:13):i==2?8:13;
                    hit.splashRadius=staff?(i==2?1.3f:.55f):book?(i==2?1.7f:.8f):0;
                    hit.control=staff?ControlEffect.Slow:!book&&i==2?ControlEffect.Root:ControlEffect.None;
                    hit.controlDuration=staff?1.1f:.35f;hit.slowMultiplier=.65f;EditorUtility.SetDirty(hit);
                }
                if(item.ability!=null)
                {
                    var ability=item.ability;ability.displayName=staff?"Pulso Glacial":book?"Selo de Cinzas":"Ruptura de Tempestade";
                    ability.effect=book?AbilityEffect.Trap:AbilityEffect.ElementalBurst;ability.radius=book?2.5f:3;
                    ability.placementDistance=3.5f;ability.trapLifetime=6;ability.cooldown=book?9:8;
                    var hit=ability.hit;hit.shape=AttackShape.Arc;hit.damage=20;hit.range=ability.radius;
                    hit.control=book?ControlEffect.Root:staff?ControlEffect.Slow:ControlEffect.None;
                    hit.controlDuration=book?1.2f:1;hit.slowMultiplier=.5f;hit.knockback=staff?1:book?0:3;
                    hit.color=item.attacks[0].color;EditorUtility.SetDirty(hit);EditorUtility.SetDirty(ability);
                }
                EditorUtility.SetDirty(item);
            }
        }
        static Transform Find(Transform root,string name)
        {if(root.name==name)return root;foreach(Transform child in root){var found=Find(child,name);if(found!=null)return found;}return null;}
    }
}
