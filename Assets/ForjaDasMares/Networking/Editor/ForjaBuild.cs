using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ForjaDasMares.Networking.Editor
{
    // Builds the game (Windows and the phone APK) with only the scenes the game uses today:
    // start menu, home village (prologue), the Refúgio and the places of the world map.
    public static class ForjaBuild
    {
        public const string Inicio = "Assets/ForjaDasMares/Scenes/Inicio.unity";
        public const string Vila = "Assets/ForjaDasMares/Scenes/RefugioAntigo.unity";
        public const string Cidade = "Assets/ForjaDasMares/Scenes/RefugioUltimoSino.unity";

        static string[] Cenas() =>
            new[] { Inicio, Vila, Cidade }.Concat(AreasInstaller.ScenePaths).Where(File.Exists).Distinct().ToArray();

        [MenuItem("Forja/Gerar jogo/Windows (Builds/Atual)")]
        public static void Windows() => Build(BuildTarget.StandaloneWindows64);

        [MenuItem("Forja/Gerar jogo/APK do celular (Builds/Atual/Android)")]
        public static void Android()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android && !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            { EditorUtility.DisplayDialog("Forja", "Não consegui trocar a plataforma para Android.", "OK"); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            bool ok = Build(BuildTarget.Android);
            string apk = Path.GetFullPath("Builds/Atual/Android/ForjaDasMares.apk");
            if (ok && File.Exists(apk)) { Debug.Log("APK_PRONTO " + apk); EditorUtility.RevealInFinder(apk); }
            else EditorUtility.DisplayDialog("Forja", "O APK NÃO foi gerado. Veja o erro vermelho no Console.", "OK");
        }

        // The game makes some materials while it runs (gate arrows, water, effects) by shader name. A shader only goes
        // into the phone build if some asset uses it, so one small material per shader lives in Resources.
        static void GarantirShaders()
        {
            const string folder = "Assets/ForjaDasMares/Resources/ShadersDoJogo";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/ForjaDasMares/Resources", "ShadersDoJogo");
            foreach (var name in new[] { "Universal Render Pipeline/Lit", "Universal Render Pipeline/Unlit", "Universal Render Pipeline/Particles/Unlit", "Universal Render Pipeline/Simple Lit" })
            {
                var shader = Shader.Find(name); if (shader == null) continue;
                string path = folder + "/" + name.Replace("Universal Render Pipeline/", "").Replace("/", "_") + ".mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(path) == null) AssetDatabase.CreateAsset(new Material(shader), path);
            }
            AssetDatabase.SaveAssets();
        }

        static bool Build(BuildTarget target)
        {
            if (EditorUserBuildSettings.activeBuildTarget != target)
            { EditorUtility.DisplayDialog("Forja", "Troque a plataforma para " + target + " em File → Build Profiles antes de gerar.", "OK"); return false; }
            GarantirShaders();
            string folder = target == BuildTarget.Android ? "Builds/Atual/Android" : "Builds/Atual/Windows";
            Directory.CreateDirectory(folder);
            try
            {
                // Windows packaged launchers can redirect the default Temp path; Gradle needs a stable one.
                if (target == BuildTarget.Android && Application.platform == RuntimePlatform.WindowsEditor)
                {
                    string javaTemp = Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/JavaTmp"));
                    Directory.CreateDirectory(javaTemp);
                    Environment.SetEnvironmentVariable("TEMP", javaTemp); Environment.SetEnvironmentVariable("TMP", javaTemp);
                }
                PlayerSettings.productName = "Forja das Mares"; PlayerSettings.runInBackground = true;
                if (target == BuildTarget.Android)
                {
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.forjadasmares.campaign");
                    PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                    PlayerSettings.Android.bundleVersionCode = Math.Max(55, PlayerSettings.Android.bundleVersionCode + 1);
                    PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)35;
                    EditorUserBuildSettings.buildAppBundle = false;
                }
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = Cenas(), target = target, options = BuildOptions.Development,
                    locationPathName = folder + (target == BuildTarget.Android ? "/ForjaDasMares.apk" : "/ForjaDasMares.exe")
                });
                Debug.Log("JOGO_GERADO " + target + " " + report.summary.result + " erros=" + report.summary.totalErrors);
                return report.summary.result == BuildResult.Succeeded;
            }
            finally { AssetDatabase.SaveAssets(); }
        }
    }
}
