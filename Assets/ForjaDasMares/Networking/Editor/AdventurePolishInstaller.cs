using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace ForjaDasMares.Networking.Editor
{
    public static class AdventurePolishInstaller
    {
        const string Root="Assets/ForjaDasMares/";
        public static void Upgrade()
        {
            AssetDatabase.Refresh();PrepareEnvironment();string path=Root+"Data/Campaign/Forge/CombatFeel.asset";
            var data=AssetDatabase.LoadAssetAtPath<CombatFeelDefinition>(path);
            if(data==null){data=ScriptableObject.CreateInstance<CombatFeelDefinition>();AssetDatabase.CreateAsset(data,path);}
            data.impact=Clip("Impact");data.swing=Clip("Swing");data.heavy=Clip("Heavy");data.step=Clip("Step");data.reward=Clip("Reward");data.perfect=Clip("Perfect");
            string trailPath=Root+"Art/Arsenal/CombatTrail.mat";var trail=AssetDatabase.LoadAssetAtPath<Material>(trailPath);if(trail==null){trail=new Material(Shader.Find("Forja/Vertex Combat Ribbon"));AssetDatabase.CreateAsset(trail,trailPath);}data.trailMaterial=trail;EditorUtility.SetDirty(data);
            foreach(var guid in AssetDatabase.FindAssets("t:HeroDefinition",new[]{Root+"Data"})){var hero=AssetDatabase.LoadAssetAtPath<HeroDefinition>(AssetDatabase.GUIDToAssetPath(guid));hero.feedback=data;EditorUtility.SetDirty(hero);}
            foreach(var guid in AssetDatabase.FindAssets("t:EnemyDefinition",new[]{Root+"Data"})){var enemy=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(guid));enemy.feedback=data;EditorUtility.SetDirty(enemy);}
            foreach(var guid in AssetDatabase.FindAssets("t:EquipmentDefinition",new[]{Root+"Data/Campaign/Forge"})){var item=AssetDatabase.LoadAssetAtPath<EquipmentDefinition>(AssetDatabase.GUIDToAssetPath(guid));item.perk=item.starter?WeaponPerk.None:item.itemId=="wavecleaver"?WeaponPerk.Pull:item.category==WeaponCategory.Axe||item.category==WeaponCategory.Daggers?WeaponPerk.Bleed:item.category==WeaponCategory.Hammer?WeaponPerk.Fracture:item.category==WeaponCategory.Staff||item.category==WeaponCategory.MagicWeapon?WeaponPerk.Scorch:WeaponPerk.None;item.synergy=item.itemId=="retorno-seal"?BuildSynergy.ScorchedEdge:item.itemId=="estrada-charm"?BuildSynergy.ControlHunter:item.category==WeaponCategory.Shield?BuildSynergy.KindredGuard:BuildSynergy.None;EditorUtility.SetDirty(item);}
            for(int i=0;i<3;i++){string file=Root+"Data/Campaign/Forge/Features_"+i+".asset";var features=AssetDatabase.LoadAssetAtPath<ExpeditionFeatureDefinition>(file);if(features==null){features=ScriptableObject.CreateInstance<ExpeditionFeatureDefinition>();AssetDatabase.CreateAsset(features,file);}features.optionalEvent=(OptionalEventKind)i;features.enemyBudget=6+i*2;features.timeLimit=75;features.enemies=new[]{AssetDatabase.LoadAssetAtPath<EnemyDefinition>(Root+"Data/Campaign/Wilderness/Enemy_raspador.asset"),AssetDatabase.LoadAssetAtPath<EnemyDefinition>(Root+"Data/Campaign/Wilderness/Enemy_vigia-sombrio.asset")};features.miniboss=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(Root+"Data/Campaign/Wilderness/Elite"+i+".asset");features.miniboss.rewardTheme=i;EditorUtility.SetDirty(features.miniboss);features.barrelModel=Model("barrel");features.cacheModel=Model("crate");features.altarModel=Model("well");features.challengeModel=Model("seal");features.wallModel=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Art/Expedition014/CrackedWall.fbx");features.bridgeModel=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Art/Expedition014/FragileBridge.fbx");EditorUtility.SetDirty(features);var level=AssetDatabase.LoadAssetAtPath<CampaignLevel>(Root+"Data/Campaign/Level"+(i+1)+".asset");level.features=features;level.hitSound=data.impact;level.crateSound=data.heavy;level.healSound=data.perfect;level.lootSound=data.reward;EditorUtility.SetDirty(level);}
            foreach(var guid in AssetDatabase.FindAssets("t:ProgressionCatalog",new[]{Root+"Data"})){var catalog=AssetDatabase.LoadAssetAtPath<ProgressionCatalog>(AssetDatabase.GUIDToAssetPath(guid));catalog.Validate();EditorUtility.SetDirty(catalog);}
            AssetDatabase.SaveAssets();Debug.Log("ADVENTURE_POLISH_UPGRADE_OK");
        }
        static void PrepareEnvironment(){foreach(string name in new[]{"CrackedWall","FragileBridge"}){string path=Root+"Art/Expedition014/"+name+".fbx";var importer=AssetImporter.GetAtPath(path) as ModelImporter;if(importer==null)throw new System.InvalidOperationException("Missing original prop: "+path);foreach(var source in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Material>()){string file=Root+"Art/Expedition014/"+source.name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(file);if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.color=source.name.Contains("Stone")?new Color(.34f,.38f,.34f):source.name.Contains("Iron")?new Color(.18f,.2f,.21f):source.name.Contains("Light")?new Color(.49f,.29f,.1f):new Color(.28f,.13f,.055f);AssetDatabase.CreateAsset(mat,file);}importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),source.name),mat);}importer.SaveAndReimport();}}
        static GameObject Model(string name)=>AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Art/ExpeditionSet/"+name+".fbx");
        static AudioClip Clip(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/Combat014/"+name+".wav");
    }
}
