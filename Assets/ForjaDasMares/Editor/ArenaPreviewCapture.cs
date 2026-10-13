using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ForjaDasMares.Editor
{
    public static class ArenaPreviewCapture
    {
        const string PendingKey = "ForjaArenaCapturePending";
        const string OutputKey = "ForjaArenaCaptureOutput";
        static double startTime;

        public static void Start()
        {
            string output = null;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-arenaPreviewPath") output = args[i + 1];
            if (string.IsNullOrEmpty(output)) throw new ArgumentException("Provide -arenaPreviewPath <absolute png path>.");
            SessionState.SetString(OutputKey, output);
            SessionState.SetBool("ForjaPvePreview", Array.IndexOf(args, "-pvePreview") >= 0);
            bool dungeon = Array.IndexOf(args, "-dungeonPreview") >= 0;
            SessionState.SetInt("ForjaDungeonPreviewBiome", dungeon ? 0 : -1);
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-dungeonBiome") SessionState.SetInt("ForjaDungeonPreviewBiome", int.Parse(args[i + 1]));
            SessionState.SetBool(PendingKey, true);
            EditorSceneManager.OpenScene(dungeon ? DungeonInstaller.ScenePath : ArenaBuilder.ScenePath);
            startTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += CaptureWhenReady;
            EditorApplication.EnterPlaymode();
        }

        [InitializeOnLoadMethod]
        static void ResumeAfterReload()
        {
            if (!SessionState.GetBool(PendingKey, false)) return;
            startTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += CaptureWhenReady;
        }
        static void CaptureWhenReady()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup - startTime < 2d) return;
            int biome = SessionState.GetInt("ForjaDungeonPreviewBiome", -1);
            if (biome >= 0)
            {
                SessionState.SetInt("ForjaDungeonPreviewBiome", -1);
                UnityEngine.Object.FindFirstObjectByType<DungeonSession>().ConfigureTest(12345, true, biome);
                startTime = EditorApplication.timeSinceStartup;
                return;
            }
            if (SessionState.GetBool("ForjaPvePreview", false))
            {
                SessionState.SetBool("ForjaPvePreview", false);
                UnityEngine.Object.FindFirstObjectByType<EncounterController>().StartSelected();
                startTime = EditorApplication.timeSinceStartup;
                return;
            }
            EditorApplication.update -= CaptureWhenReady;
            SessionState.SetBool(PendingKey, false);
            try
            {
                ArenaCamera follow = UnityEngine.Object.FindFirstObjectByType<ArenaCamera>();
                Camera camera = follow.GetComponent<Camera>();
                camera.aspect = 16f / 9f; follow.Snap();
                Canvas hud = UnityEngine.Object.FindFirstObjectByType<ArenaHud>().GetComponent<Canvas>();
                hud.renderMode = RenderMode.ScreenSpaceCamera; hud.worldCamera = camera; hud.planeDistance = 1f;
                var texture = new RenderTexture(1280, 720, 24);
                camera.targetTexture = texture;
                Canvas.ForceUpdateCanvases();
                var request = new UniversalRenderPipeline.SingleCameraRequest { destination = texture };
                if (!RenderPipeline.SupportsRenderRequest(camera, request)) throw new InvalidOperationException("URP camera capture is not supported by the current pipeline.");
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderTexture previous = RenderTexture.active; RenderTexture.active = texture;
                var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
                string output = SessionState.GetString(OutputKey, "");
                Directory.CreateDirectory(Path.GetDirectoryName(output)); File.WriteAllBytes(output, image.EncodeToPNG());
                RenderTexture.active = previous; camera.targetTexture = null;
                texture.Release(); UnityEngine.Object.Destroy(texture); UnityEngine.Object.Destroy(image);
                Debug.Log("ARENA_PREVIEW_OK " + output);
                EditorApplication.Exit(0);
            }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }
    }
}
