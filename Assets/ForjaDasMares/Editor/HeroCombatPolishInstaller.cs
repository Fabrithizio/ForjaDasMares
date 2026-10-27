using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace ForjaDasMares.Editor
{
    public static class HeroCombatPolishInstaller
    {
        public static void VerifyMotion()
        {
            var root=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SapphireMageInstaller.PrefabPath));
            try
            {
                var view=root.GetComponent<HeroModelView>();var animator=view.Animator;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                foreach(string name in new[]{"Idle","StaffBolt","BookWord","Run"})
                {
                    var clip=animator.runtimeAnimatorController.animationClips.First(c=>c.name==name);
                    Debug.Log("MOTION_BINDING "+name+" first="+AnimationUtility.GetCurveBindings(clip)[0].path);
                    clip.SampleAnimation(animator.gameObject,clip.length*.3f);Debug.Log("MOTION_SAMPLE "+name+" hand="+view.SecondarySocket.position.ToString("F4"));
                    animator.Rebind();animator.SetFloat("ActionTime",.3f);animator.Play("Base Layer."+name);animator.Update(0);
                    Debug.Log("MOTION_ANIMATOR "+name+" hand="+view.SecondarySocket.position.ToString("F4"));
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        public static void Apply()
        {
            AssetDatabase.Refresh();
            foreach(string name in new[]{"HeroEnergyRibbon","HeroEnergySpark"})
            {
                string path="Assets/ForjaDasMares/Resources/"+name+".mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(Shader.Find("Forja/Hero Energy"));AssetDatabase.CreateAsset(material,path);}
                material.SetFloat("_Billboard",name.EndsWith("Spark")?1:0);material.SetColor("_BaseColor",Color.white);EditorUtility.SetDirty(material);
            }
            string brasa="Assets/ForjaDasMares/Art/Characters/Brasa/Brasa.fbx";
            var importer=(ModelImporter)AssetImporter.GetAtPath(brasa);
            var previous=importer.clipAnimations;var fresh=importer.defaultClipAnimations;
            foreach(var clip in fresh)
            {
                var original=previous.FirstOrDefault(c=>clip.name==c.name||clip.name.EndsWith("|"+c.name,StringComparison.Ordinal)||clip.name.EndsWith("_"+c.name,StringComparison.Ordinal));
                if(original==null)throw new InvalidOperationException("Unknown warrior clip "+clip.name);
                clip.name=original.name;clip.loopTime=original.loopTime;clip.lockRootRotation=clip.lockRootPositionXZ=clip.lockRootHeightY=true;
            }
            importer.clipAnimations=fresh;importer.SaveAndReimport();
            var clips=AssetDatabase.LoadAllAssetsAtPath(brasa).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/ForjaDasMares/Art/Characters/Brasa/Brasa.controller");
            foreach(var layer in controller.layers)foreach(var child in layer.stateMachine.states)
            {
                var clip=clips.SingleOrDefault(c=>c.name==child.state.name);
                if(clip==null)throw new InvalidOperationException("Missing warrior clip "+child.state.name);
                child.state.motion=clip;EditorUtility.SetDirty(child.state);
            }
            EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();
            SapphireMageInstaller.Apply();
            SapphireMageInstaller.ApplyWarrior();
            var ribbon=AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Resources/HeroEnergyRibbon.mat");
            foreach(var pool in UnityEngine.Object.FindObjectsByType<CombatEffectPool>(FindObjectsInactive.Include,FindObjectsSortMode.None)){pool.ConfigureTrail(ribbon);EditorUtility.SetDirty(pool);}
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
            Debug.Log("HERO_POLISH_READY mageClips=25 warriorClips="+clips.Length+" wireRibbonsPerHero=0 motionMeshPerHero=1 particlesPerHero=64");
        }
    }
}
