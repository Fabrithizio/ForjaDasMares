using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ForjaDasMares.Networking.Editor
{
    // Creates the "Inicio" scene (first scene of the game) once, and keeps it first in the build list.
    [InitializeOnLoad]
    public static class StartMenuInstaller
    {
        public const string ScenePath = "Assets/ForjaDasMares/Scenes/Inicio.unity";

        static StartMenuInstaller()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (!File.Exists(ScenePath)) Create();
                else EnsureFirstInBuild();
            };
        }

        [MenuItem("Forja/Recriar cena Início")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Inicio").AddComponent<StartMenu>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            EnsureFirstInBuild();
            Debug.Log("INICIO_SCENE_READY " + ScenePath);
        }

        static void EnsureFirstInBuild()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Count > 0 && scenes[0].path == ScenePath && scenes[0].enabled) return;
            scenes.RemoveAll(entry => entry.path == ScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
