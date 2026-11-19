using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ForjaDasMares.Networking.Editor
{
    // One click: opens the first scene and starts the game straight on the character creation screen
    // (the current character is only replaced if "CRIAR E ENTRAR" is pressed).
    public static class TestarCriacao
    {
        [MenuItem("Forja/Personagem/Testar tela de criação")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene("Assets/ForjaDasMares/Scenes/Inicio.unity");
            PlayerPrefs.SetInt(StartMenu.TestCreationKey, 1); PlayerPrefs.Save();
            EditorApplication.isPlaying = true;
        }
    }
}
