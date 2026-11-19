using UnityEditor;
using UnityEditor.SceneManagement;

namespace ForjaDasMares.Networking.Editor
{
    // When code changes while the game is running, Unity stops the game and recompiles,
    // instead of recompiling in the middle of the game (which leaves everything broken with red errors).
    // Play always starts at the start screen (Inicio), like the real game, whatever scene is open in the editor.
    // Turn it off in "Forja/Play começa pela tela inicial" to test one scene directly.
    [InitializeOnLoad]
    static class ForjaEditorPrefs
    {
        const string PlayFromStart = "ForjaPlayFromStart", MenuPath = "Forja/Play começa pela tela inicial";

        static ForjaEditorPrefs()
        {
            // 0 = recompile and keep playing, 1 = recompile after playing, 2 = stop playing and recompile
            if (EditorPrefs.GetInt("ScriptCompilationDuringPlay", 0) != 2) EditorPrefs.SetInt("ScriptCompilationDuringPlay", 2);
            EditorApplication.delayCall += Apply;
        }

        static void Apply()
        {
            bool on = EditorPrefs.GetBool(PlayFromStart, true);
            EditorSceneManager.playModeStartScene = on ? AssetDatabase.LoadAssetAtPath<SceneAsset>(StartMenuInstaller.ScenePath) : null;
            Menu.SetChecked(MenuPath, on);
        }

        [MenuItem(MenuPath, priority = 900)]
        static void Toggle()
        {
            EditorPrefs.SetBool(PlayFromStart, !EditorPrefs.GetBool(PlayFromStart, true));
            Apply();
        }
    }
}
