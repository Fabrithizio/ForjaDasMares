using UnityEditor;

namespace ForjaDasMares.Networking.Editor
{
    // When code changes while the game is running, Unity stops the game and recompiles,
    // instead of recompiling in the middle of the game (which leaves everything broken with red errors).
    [InitializeOnLoad]
    static class ForjaEditorPrefs
    {
        static ForjaEditorPrefs()
        {
            // 0 = recompile and keep playing, 1 = recompile after playing, 2 = stop playing and recompile
            if (EditorPrefs.GetInt("ScriptCompilationDuringPlay", 0) != 2) EditorPrefs.SetInt("ScriptCompilationDuringPlay", 2);
        }
    }
}
