using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForjaDasMares.Editor
{
    // Ao abrir o projeto numa cena vazia, abre o menu principal do jogo (CampaignCoop).
    // F6 abre o menu principal a qualquer momento.
    [InitializeOnLoad]
    public static class ArenaLauncher
    {
        public const string MainScene = "Assets/ForjaDasMares/Scenes/CampaignCoop.unity";

        static ArenaLauncher()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += OpenOnEmptyStartup;
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
                !SceneManager.GetActiveScene().isDirty && File.Exists(MainScene))
                EditorSceneManager.OpenScene(MainScene);
        }

        [MenuItem("Forja/Abrir menu principal do jogo _F6")]
        public static void OpenFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(MainScene);
        }
    }
}
