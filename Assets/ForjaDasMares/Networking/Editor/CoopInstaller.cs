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
            if (File.Exists(ScenePath)) { Debug.Log("COOP_SCENE_EXISTS"); return; }
            var scene = EditorSceneManager.OpenScene(ForjaDasMares.Editor.DungeonInstaller.ScenePath);
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)) canvas.gameObject.SetActive(false);
            var root = new GameObject("PrivateCoopSystems"); var transport = root.AddComponent<UnityTransport>();
            var network = root.AddComponent<NetworkManager>(); network.NetworkConfig = new NetworkConfig { NetworkTransport = transport, EnableSceneManagement = false, ConnectionApproval = true, ForceSamePrefabs = false, TickRate = 30 };
            network.LogLevel = LogLevel.Normal;
            var director = root.AddComponent<CoopDirector>(); root.AddComponent<CoopSmokeProbe>(); var heroes = Object.FindFirstObjectByType<HeroRoster>();
            var input = heroes.GetComponent<ArenaInput>();
            director.Configure(network, transport, Object.FindFirstObjectByType<DungeonSession>(), Object.FindFirstObjectByType<ArenaSession>(), heroes, Object.FindFirstObjectByType<ArenaCamera>(), input, Object.FindFirstObjectByType<DungeonInteractionInput>(), Object.FindFirstObjectByType<CombatEffectPool>(), Object.FindFirstObjectByType<TelegraphPool>());
            var ui = new GameObject("CoopCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); ui.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; ui.AddComponent<CoopHud>().Configure(director, input);
            EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets(); Debug.Log("COOP_INSTALL_OK");
        }
        public static void Windows()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64) throw new System.InvalidOperationException("Invoke Unity with -buildTarget Win64.");
            if (!File.Exists(ScenePath)) throw new FileNotFoundException("Install coop scene first.");
            Directory.CreateDirectory("Builds/CoopWindows");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath, ForjaDasMares.Editor.ProgressionInstaller.DungeonScene, ForjaDasMares.Editor.ProgressionInstaller.ArenaScene }, locationPathName = "Builds/CoopWindows/ForjaDasMaresCoop.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            Debug.Log("COOP_BUILD " + report.summary.result + " errors=" + report.summary.totalErrors);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.InvalidOperationException("Coop build failed.");
        }
    }
}
