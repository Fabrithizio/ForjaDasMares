using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ForjaDasMares.Editor
{
    // Visual-only integration. Original Prisma assets remain available for rollback.
    public static class SapphireMageInstaller
    {
        static string Root="Assets/ForjaDasMares/Art/Characters/SapphireMage/";
        static string Prefix="SapphireMage";
        static bool warrior;
        static string ActivePrefab=>Root+Prefix+".prefab";
        public const string PrefabPath="Assets/ForjaDasMares/Art/Characters/SapphireMage/SapphireMage.prefab";
        public const string ScenePath="Assets/ForjaDasMares/Scenes/CampaignCoop.unity";
        [MenuItem("Forja das Mares/Characters/Install Sapphire Mage visual preview")]
        public static void Apply()=>ApplyModel(false);
        public static void ApplyWarrior()=>ApplyModel(true);
        static void ApplyModel(bool isWarrior)
        {
            warrior=isWarrior;Prefix=warrior?"EmberHero":"SapphireMage";
            Root="Assets/ForjaDasMares/Art/Characters/"+Prefix+"/";
            if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            AssetDatabase.Refresh();
            foreach(string map in new[]{"Albedo.png","Normal.png","MetallicSource.png","RoughnessSource.png"})ConfigureTexture(Root+map,map=="Albedo.png",map=="Normal.png",map.EndsWith("Source.png"));
            PackSurface();
            string materialPath=Root+Prefix+"_URP.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,materialPath);}
            material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Albedo.png"));material.SetColor("_BaseColor",Color.white);
            material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Normal.png"));material.SetFloat("_BumpScale",1);material.EnableKeyword("_NORMALMAP");
            material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"MetallicSmoothness.png"));material.SetFloat("_Smoothness",1);material.SetFloat("_Metallic",1);material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetFloat("_Cull",0);EditorUtility.SetDirty(material);
            string animatedName=warrior?"EmberAnimated.fbx":"SapphireAnimated.fbx";
            bool animated=File.Exists(Root+animatedName);
            string modelPath=Root+(animated?animatedName:Prefix+".fbx");
            var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);
            importer.animationType=ModelImporterAnimationType.Generic;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation=true;importer.optimizeGameObjects=false;importer.preserveHierarchy=true;importer.isReadable=false;importer.importCameras=importer.importLights=false;importer.addCollider=false;
            importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();
            var clips=importer.defaultClipAnimations;
            string[] names={"Idle","Walk","Run","Attack","Attack2","Attack3","Charged","Skill","Super","Dodge","Hit","Dead","Victory","StaffBolt","StaffReturn","StaffRelease","HeavyStaff","BookWord","BookPage","BookSeal","HeavyBook","OrbCast","OrbReturn","OrbBurst","HeavyOrb"};
            if(warrior)names=new[]{"Idle","Walk","Run","Attack","Attack2","Attack3","Charged","Skill","Super","Dodge","Hit","Dead","Victory","SwordSlash","SwordReturn","SwordThrust","HeavySword","AxeCleave","AxeBackhand","AxeSpin","HeavyAxe","HammerSweep","HammerBack","HammerSlam","HeavyHammer"};
            foreach(var clip in clips)
            {
                clip.name=animated?names.First(n=>clip.name==n||clip.name.EndsWith("|"+n,StringComparison.Ordinal)||clip.name.EndsWith("_"+n,StringComparison.Ordinal)):"MeshyRestPose";
                clip.loopTime=clip.name=="Idle"||clip.name=="Walk"||clip.name=="Run"||clip.name=="Victory"||!animated;
                clip.lockRootRotation=clip.lockRootPositionXZ=clip.lockRootHeightY=true;
            }
            importer.clipAnimations=clips;importer.animationCompression=ModelImporterAnimationCompression.Off;importer.SaveAndReimport();
            var motions=AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();
            var rest=motions.Single(c=>c.name==(animated?"Idle":"MeshyRestPose"));
            string controllerPath=Root+Prefix+(animated?".controller":"Preview.controller");
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            if(controller.layers.Length==0)controller.AddLayer("Base Layer");
            var machine=controller.layers[0].stateMachine;var idle=machine.states.Select(s=>s.state).FirstOrDefault(s=>s.name=="Idle")??machine.AddState("Idle");idle.motion=rest;machine.defaultState=idle;EditorUtility.SetDirty(controller);
            var root=new GameObject(Prefix+"_Model");
            try
            {
                var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));model.transform.SetParent(root.transform,false);
                if(animated)ConfigureAnimation(controller,motions,names,model.transform);
                var animator=model.GetComponentInChildren<Animator>()??model.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.Rebind();animator.Update(0);
                var skin=model.GetComponentsInChildren<SkinnedMeshRenderer>();if(skin.Length!=1)throw new InvalidOperationException("Expected a single supplied skin.");
                var head=Find(model.transform,"Head");var face=Find(model.transform,"headfront");
                if(head!=null&&face!=null){var forward=face.position-head.position;forward.y=0;if(forward.sqrMagnitude>.00001f)model.transform.rotation=Quaternion.FromToRotation(forward.normalized,Vector3.forward)*model.transform.rotation;}
                var bounds=skin[0].bounds;if(bounds.size.y<.01f)throw new InvalidOperationException("Invalid model height.");
                model.transform.localScale*=(warrior?1.8f:1.7f)/bounds.size.y;bounds=skin[0].bounds;model.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
                foreach(var mesh in skin){mesh.sharedMaterials=new[]{material};mesh.updateWhenOffscreen=false;if(mesh.bones.Any(b=>b==null))throw new InvalidOperationException("Unbound bone.");}
                Transform right=Socket(model.transform,"RightHand","WeaponSocket_R"),left=Socket(model.transform,"LeftHand","WeaponSocket_L");
                var starter=AssetDatabase.LoadAssetAtPath<EquipmentDefinition>("Assets/ForjaDasMares/Data/Campaign/Forge/Starter_"+(warrior?"warrior":"arcanist")+".asset");
                if(starter==null||starter.visualMesh==null)throw new InvalidOperationException("Missing actual starter staff asset.");
                var view=root.AddComponent<HeroModelView>();view.ConfigureEquipmentFit(true);
                if(warrior){var fit=new SerializedObject(view);fit.FindProperty("otherWeaponLength").floatValue=1.10f;fit.ApplyModifiedPropertiesWithoutUndo();}
                var weapon=new GameObject("PreviewDefaultStaff",typeof(MeshFilter),typeof(MeshRenderer));weapon.GetComponent<MeshFilter>().sharedMesh=starter.visualMesh;weapon.GetComponent<MeshRenderer>().sharedMaterials=starter.visualMaterials;
                EquipmentVisualPlacement.Mount(weapon.transform,right,true,Vector3.zero,Quaternion.identity,starter);weapon.AddComponent<EquipmentMeshView>().Apply(starter);
                if(!warrior&&Vector3.Dot(weapon.transform.up,Vector3.up)<.9f)throw new InvalidOperationException("Staff is not upright.");
                view.Configure(animator,right,left,weapon,animated);
                if(animated)
                {
                    root.AddComponent<BrasaFootPlant>().Configure(view,Find(model.transform,"LeftUpLeg"),Find(model.transform,"LeftLeg"),Find(model.transform,"LeftFoot"),Find(model.transform,"RightUpLeg"),Find(model.transform,"RightLeg"),Find(model.transform,"RightFoot"));
                    var settings=new SerializedObject(view);settings.FindProperty("walkStride").floatValue=.77f;settings.FindProperty("runStride").floatValue=2.43f;settings.ApplyModifiedPropertiesWithoutUndo();
                }
                int checkedItems=0;
                foreach(string guid in AssetDatabase.FindAssets("t:EquipmentDefinition",new[]{"Assets/ForjaDasMares/Data"}))
                {
                    var item=AssetDatabase.LoadAssetAtPath<EquipmentDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                    if(item.visualMesh==null)continue;
                    float factor=view.EquipmentScale(item);
                    if(float.IsNaN(factor)||factor<=0||factor>1)throw new InvalidOperationException("Invalid equipment fit: "+item.name);
                    EquipmentVisualPlacement.Mount(weapon.transform,right,true,Vector3.zero,Quaternion.identity,item);
                    weapon.GetComponent<EquipmentMeshView>().Apply(item);
                    Vector3 actual=weapon.transform.lossyScale,expected=item.authoredScale*factor;
                    if((actual-expected).sqrMagnitude>.0001f)throw new InvalidOperationException("Skeleton changed equipment dimensions: "+item.name);
                    checkedItems++;
                }
                EquipmentVisualPlacement.Mount(weapon.transform,right,true,Vector3.zero,Quaternion.identity,starter);weapon.GetComponent<EquipmentMeshView>().Apply(starter);
                Debug.Log("HERO_EQUIPMENT_FIT model="+Prefix+" checked="+checkedItems+" bodyHeight="+(warrior?1.8f:1.7f));
                if(!warrior)root.AddComponent<PrismaCastingView>().Configure(view,AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Art/Arsenal/CombatTrail.mat"));
                PrefabUtility.SaveAsPrefabAsset(root,ActivePrefab);
                Debug.Log("SAPPHIRE_PREFAB height="+skin[0].bounds.size.y+" triangles="+skin[0].sharedMesh.triangles.Length/3+" bones="+skin[0].bones.Length+" authoredAnimations="+animated+" clips="+motions.Length);
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
            var scene=EditorSceneManager.OpenScene(ScenePath);int replaced=0;
            foreach(var roster in UnityEngine.Object.FindObjectsByType<HeroRoster>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                var visual=roster.transform.Find(warrior?"HeroVisual_warrior":"HeroVisual_arcanist");if(visual==null)continue;
                var views=visual.GetComponentsInChildren<HeroModelView>(true);if(views.Length>1)throw new InvalidOperationException("Resolve duplicate mage skins before integration.");
                if(views.Length==1)UnityEngine.Object.DestroyImmediate(views[0].gameObject);
                foreach(string name in new[]{"Body_Placeholder","Head_Placeholder","FacingMarker","Staff","Focus","GreatSword","Shield"}){var part=visual.Find(name);if(part!=null)part.gameObject.SetActive(false);}
                var replacement=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ActivePrefab));replacement.transform.SetParent(visual,false);replaced++;
            }
            if(replaced!=1)throw new InvalidOperationException("Expected one campaign template; got "+replaced);
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("SAPPHIRE_READY sceneTemplates="+replaced+" logicAndDataUnchanged=true");
        }
        static Transform Find(Transform root,string suffix)=>root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==suffix||t.name.EndsWith(":"+suffix,StringComparison.Ordinal));
        static void ConfigureAnimation(AnimatorController controller,AnimationClip[] clips,string[] names,Transform model)
        {
            foreach(string p in new[]{"Speed","ActionTime","GaitTime"})if(!controller.parameters.Any(x=>x.name==p))controller.AddParameter(p,AnimatorControllerParameterType.Float);
            if(!controller.parameters.Any(x=>x.name=="State"))controller.AddParameter("State",AnimatorControllerParameterType.Int);
            if(!controller.parameters.Any(x=>x.name=="Alive"))controller.AddParameter("Alive",AnimatorControllerParameterType.Bool);
            var mask=AssetDatabase.LoadAssetAtPath<AvatarMask>(Root+Prefix+"UpperBody.mask");
            if(mask==null){mask=new AvatarMask();AssetDatabase.CreateAsset(mask,Root+Prefix+"UpperBody.mask");}
            var transforms=model.GetComponentsInChildren<Transform>(true);var spine=Find(model,"Spine");mask.transformCount=transforms.Length;
            for(int i=0;i<transforms.Length;i++){mask.SetTransformPath(i,AnimationUtility.CalculateTransformPath(transforms[i],model));mask.SetTransformActive(i,transforms[i]==spine||transforms[i].IsChildOf(spine));}
            EditorUtility.SetDirty(mask);
            if(controller.layers.Length<2)controller.AddLayer("Upper Body");
            var layers=controller.layers;layers[1].avatarMask=mask;layers[1].defaultWeight=0;controller.layers=layers;
            for(int layer=0;layer<2;layer++)foreach(string name in names)
            {
                var motion=clips.Single(c=>c.name==name);if(AnimationUtility.GetCurveBindings(motion).Length==0)throw new InvalidOperationException("Empty clip "+name);
                var machine=controller.layers[layer].stateMachine;
                var state=machine.states.Select(x=>x.state).FirstOrDefault(x=>x.name==name)??machine.AddState(name);state.motion=motion;state.writeDefaultValues=true;
                bool gait=name=="Walk"||name=="Run";
                state.timeParameterActive=gait||name!="Idle"&&name!="Dead"&&name!="Victory";
                state.timeParameter=gait?"GaitTime":"ActionTime";
                if(name=="Idle")machine.defaultState=state;
            }
            EditorUtility.SetDirty(controller);
        }
        static Transform Socket(Transform model,string bone,string name)
        {
            var hand=Find(model,bone);if(hand==null)throw new InvalidOperationException("Missing hand: "+bone);
            var socket=new GameObject(name).transform;socket.SetParent(hand,false);
            var knuckle=Find(model,bone.Replace("Hand","HandMiddle1"));socket.position=knuckle!=null?Vector3.Lerp(hand.position,knuckle.position,.55f):hand.position;
            socket.rotation=Quaternion.Euler(warrior?-45:90,0,0);return socket;
        }
        static void ConfigureTexture(string path,bool srgb,bool normal,bool readable)
        {
            var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;t.sRGBTexture=srgb;t.mipmapEnabled=true;t.isReadable=readable;
            t.textureCompression=readable?TextureImporterCompression.Uncompressed:TextureImporterCompression.CompressedHQ;t.maxTextureSize=2048;
            t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=1024,format=TextureImporterFormat.ASTC_6x6,compressionQuality=80});t.SaveAndReimport();
        }
        static void PackSurface()
        {
            Texture2D metal=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"MetallicSource.png"),rough=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"RoughnessSource.png");
            if(metal.width!=rough.width||metal.height!=rough.height)throw new InvalidOperationException("Surface maps have different dimensions.");
            var m=metal.GetPixels32();var r=rough.GetPixels32();for(int i=0;i<m.Length;i++)m[i]=new Color32(m[i].r,m[i].r,m[i].r,(byte)(255-r[i].r));
            var packed=new Texture2D(metal.width,metal.height,TextureFormat.RGBA32,false,true);
            try{packed.SetPixels32(m);packed.Apply();File.WriteAllBytes(Root+"MetallicSmoothness.png",packed.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(packed);}
            AssetDatabase.ImportAsset(Root+"MetallicSmoothness.png");ConfigureTexture(Root+"MetallicSmoothness.png",false,false,false);
            foreach(string name in new[]{"MetallicSource.png","RoughnessSource.png"}){var importer=(TextureImporter)AssetImporter.GetAtPath(Root+name);importer.isReadable=false;importer.SaveAndReimport();}
        }
    }
}
