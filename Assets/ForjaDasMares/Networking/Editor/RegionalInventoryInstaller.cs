using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ForjaDasMares.Networking.Editor
{
    public static class RegionalInventoryInstaller
    {
        [MenuItem("Forja/World/Integrar inventario RPG")]
        public static void Apply()
        {
            EditorSceneManager.OpenScene(CampaignInstaller.ScenePath);
            var campaign=UnityEngine.Object.FindFirstObjectByType<CoopProgression>();
            if(campaign==null||campaign.Catalog==null)throw new InvalidOperationException("Existing campaign progression missing");
            var catalog=campaign.Catalog;
            string profileDirectory=new SerializedObject(campaign).FindProperty("profileDirectory").stringValue;
            Debug.Log("REGIONAL_PROFILE_SOURCE catalog="+AssetDatabase.GetAssetPath(catalog)+" directory="+profileDirectory);
            foreach(string path in new[]{TownInstaller.ScenePath,ActOneInstaller.ScenePath,RegionalWorldInstaller.ScenePath,CatacombPreviewInstaller.ScenePath})
            {
                var scene=EditorSceneManager.OpenScene(path);var world=UnityEngine.Object.FindFirstObjectByType<RegionalWorld>();if(world==null)throw new InvalidOperationException("No regional world in "+path);
                var inventory=world.GetComponent<RegionalInventory>();if(inventory==null)inventory=world.gameObject.AddComponent<RegionalInventory>();inventory.Configure(catalog,profileDirectory);EditorUtility.SetDirty(inventory);
                EditorSceneManager.SaveScene(scene);Debug.Log("REGIONAL_INVENTORY_CONFIGURED "+path);
            }
            AssetDatabase.SaveAssets();
        }
    }
}


