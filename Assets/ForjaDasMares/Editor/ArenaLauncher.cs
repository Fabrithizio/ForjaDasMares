using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForjaDasMares.Editor
{
    [InitializeOnLoad]
    public static class ArenaLauncher
    {
        static ArenaLauncher()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += OpenOnEmptyStartup;
            EditorApplication.playModeStateChanged += GuardEmptyPlay;
        }

        static bool IsEmptyTemplate()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.IsNullOrEmpty(scene.path)) return false;
            GameObject[] roots = scene.GetRootGameObjects();
            if (roots.Length > 2) return false;
            foreach (GameObject root in roots)
            {
                if (root.transform.childCount != 0 ||
                    (root.GetComponent<Camera>() == null && root.GetComponent<Light>() == null)) return false;
                foreach (Component component in root.GetComponents<Component>())
                    if (!(component is Transform || component is Camera || component is Light || component is AudioListener ||
                          component is UnityEngine.Rendering.Universal.UniversalAdditionalCameraData ||
                          component is UnityEngine.Rendering.Universal.UniversalAdditionalLightData)) return false;
            }
            return true;
        }

        static void OpenOnEmptyStartup()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && IsEmptyTemplate() &&
                !SceneManager.GetActiveScene().isDirty && File.Exists(ArenaBuilder.ScenePath))
                EditorSceneManager.OpenScene(ArenaBuilder.ScenePath);
        }

        static void GuardEmptyPlay(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode || !IsEmptyTemplate()) return;
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () =>
            {
                if (OpenArena()) EditorApplication.EnterPlaymode();
            };
        }

        [MenuItem("Forja das Mares/Open playable arena _F6")]
        public static void OpenFromMenu() => OpenArena();

        static bool OpenArena()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return false;
            if (!File.Exists(ArenaBuilder.ScenePath))
            {
                Debug.LogError("ArenaTest is missing. Use Create test arena from the same menu.");
                return false;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            EditorSceneManager.OpenScene(ArenaBuilder.ScenePath);
            return true;
        }
    }
}
