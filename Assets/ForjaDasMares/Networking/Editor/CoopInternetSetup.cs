using System.IO;
using UnityEditor;
using UnityEngine;
namespace ForjaDasMares.Networking.Editor
{
    public static class CoopInternetSetup
    {
        public const string SettingsPath = "Assets/ForjaDasMares/Resources/CoopInternet.asset";
        [MenuItem("Forja das Mares/Internet/Configuração")]
        public static void Configure()
        {
            var settings = AssetDatabase.LoadAssetAtPath<CoopInternetSettings>(SettingsPath);
            if (settings == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)); AssetDatabase.Refresh();
                settings = ScriptableObject.CreateInstance<CoopInternetSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath); AssetDatabase.SaveAssets();
            }
            Selection.activeObject = settings;
            Debug.Log("COOP_INTERNET_CONFIG environment=" + settings.Environment + " linked=" + CoopInternetSettings.HasLinkedProject(Application.cloudProjectId));
        }
    }
}
