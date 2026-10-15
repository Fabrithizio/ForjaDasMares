using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking.Editor
{
    public static class CoopInstaller
    {
        public const string ScenePath = "Assets/ForjaDasMares/Scenes/CoopDungeon.unity";
        [MenuItem("Forja das Mares/Install private coop variant")]
        public static void Apply()
        {
            if (File.Exists(ScenePath))
            {
                var existing = EditorSceneManager.OpenScene(ScenePath);
                InstallProgression(Object.FindFirstObjectByType<CoopDirector>());
                EditorSceneManager.SaveScene(existing); AssetDatabase.SaveAssets(); Debug.Log("COOP_SCENE_UPDATED"); return;
            }
            var scene = EditorSceneManager.OpenScene(ForjaDasMares.Editor.DungeonInstaller.ScenePath);
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)) canvas.gameObject.SetActive(false);
            var root = new GameObject("PrivateCoopSystems"); var transport = root.AddComponent<UnityTransport>();
            var network = root.AddComponent<NetworkManager>(); network.NetworkConfig = new NetworkConfig { NetworkTransport = transport, EnableSceneManagement = false, ConnectionApproval = true, ForceSamePrefabs = false, TickRate = 30 };
            network.LogLevel = LogLevel.Normal;
            var director = root.AddComponent<CoopDirector>(); root.AddComponent<CoopSmokeProbe>(); var heroes = Object.FindFirstObjectByType<HeroRoster>();
            var input = heroes.GetComponent<ArenaInput>();
            director.Configure(network, transport, Object.FindFirstObjectByType<DungeonSession>(), Object.FindFirstObjectByType<ArenaSession>(), heroes, Object.FindFirstObjectByType<ArenaCamera>(), input, Object.FindFirstObjectByType<DungeonInteractionInput>(), Object.FindFirstObjectByType<CombatEffectPool>(), Object.FindFirstObjectByType<TelegraphPool>());
            var ui = new GameObject("CoopCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); ui.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; ui.AddComponent<CoopHud>().Configure(director, input);
            InstallProgression(director);
            EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets(); Debug.Log("COOP_INSTALL_OK");
        }
        static void InstallProgression(CoopDirector director)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ProgressionCatalog>("Assets/ForjaDasMares/Data/Progression/Catalog.asset");
            if (catalog == null || director == null) throw new System.InvalidOperationException("Missing cooperative catalog/director.");
            var progression = director.GetComponent<CoopProgression>() ?? director.gameObject.AddComponent<CoopProgression>(); progression.Configure(catalog);
            var channel = director.GetComponent<CoopProfileChannel>() ?? director.gameObject.AddComponent<CoopProfileChannel>(); channel.Configure(director, progression);
            var hud = Object.FindFirstObjectByType<CoopHud>();
            (hud.GetComponent<CoopInventoryHud>() ?? hud.gameObject.AddComponent<CoopInventoryHud>()).Configure(director);
        }
        public static void Windows()
        { BuildWindows("Builds/CoopWindows", "0.6.1"); }
        public static void InternetWindows()
        { ValidateInternet(); BuildWindows("Builds/InternetWindows", "0.6.2"); }
        static void ValidateInternet()
        {
            var settings = AssetDatabase.LoadAssetAtPath<CoopInternetSettings>(CoopInternetSetup.SettingsPath);
            if (!CoopInternetSettings.HasLinkedProject(Application.cloudProjectId) || settings == null || !settings.IsValid)
                throw new System.InvalidOperationException("Link the Unity Cloud project and configure Internet settings first.");
        }
        static void BuildWindows(string directory, string version)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64) throw new System.InvalidOperationException("Invoke Unity with -buildTarget Win64.");
            if (!File.Exists(ScenePath)) throw new FileNotFoundException("Install coop scene first.");
            Directory.CreateDirectory(directory);
            PlayerSettings.bundleVersion = version;
            PlayerSettings.runInBackground = true;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath, ForjaDasMares.Editor.ProgressionInstaller.DungeonScene, ForjaDasMares.Editor.ProgressionInstaller.ArenaScene }, locationPathName = Path.Combine(directory, "ForjaDasMaresCoop.exe"), target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            Debug.Log("COOP_BUILD " + report.summary.result + " errors=" + report.summary.totalErrors);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.InvalidOperationException("Coop build failed.");
        }
        public static void Android()
        { BuildAndroid("Builds/CoopAndroid", "0.6.1", 7); }
        public static void InternetAndroid()
        { ValidateInternet(); BuildAndroid("Builds/InternetAndroid", "0.6.2", 8); }
        static void BuildAndroid(string directory, string version, int versionCode)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) throw new System.InvalidOperationException("Invoke Unity with -buildTarget Android.");
            if (!File.Exists(ScenePath)) throw new FileNotFoundException("Install coop scene first.");
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)35;
            PlayerSettings.bundleVersion = version; PlayerSettings.Android.bundleVersionCode = versionCode;
            EditorUserBuildSettings.buildAppBundle = false; Directory.CreateDirectory(directory);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath, ForjaDasMares.Editor.ProgressionInstaller.DungeonScene, ForjaDasMares.Editor.ProgressionInstaller.ArenaScene }, locationPathName = Path.Combine(directory, "ForjaDasMaresCoop.apk"), target = BuildTarget.Android, options = BuildOptions.Development });
            Debug.Log("COOP_ANDROID_BUILD " + report.summary.result + " errors=" + report.summary.totalErrors);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.InvalidOperationException("Coop Android build failed. Inspect the log; preserve the generated Gradle project for toolchain diagnostics.");
        }
    }
}
