using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ForjaDasMares.Networking.Editor
{
    // The home village of the prologue (the old city scene) must be in the build, and a menu to see the story again.
    [InitializeOnLoad]
    public static class HistoriaEditor
    {
        public const string VilaPath = "Assets/ForjaDasMares/Scenes/RefugioAntigo.unity";

        static HistoriaEditor() => EditorApplication.delayCall += EnsureInBuild;

        static void EnsureInBuild()
        {
            if (!System.IO.File.Exists(VilaPath)) return;
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == VilaPath && s.enabled)) return;
            scenes.RemoveAll(s => s.path == VilaPath);
            scenes.Add(new EditorBuildSettingsScene(VilaPath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        [MenuItem("Forja/História/Ver o começo de novo (abertura, vila e chegada)")]
        static void Restart()
        {
            Historia.Recomecar();
            Debug.Log("HISTORIA: o próximo Play começa pela abertura e pela vila.");
        }
    }
}
