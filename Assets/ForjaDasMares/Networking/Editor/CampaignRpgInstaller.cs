using UnityEditor;
using UnityEngine;
namespace ForjaDasMares.Networking.Editor
{
    public static class CampaignRpgInstaller
    {
        public static void Upgrade()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ProgressionCatalog>("Assets/ForjaDasMares/Data/Campaign/Forge/Catalog.asset");
            if(catalog==null)throw new System.InvalidOperationException("Existing campaign catalog was not found.");
            catalog.talents=HeroTalents.Defaults();catalog.salvageBase=2;catalog.talentResetGold=25;
            catalog.enemy.experience=2;catalog.enemy.coins=2;catalog.enemy.itemChance=.10f;
            catalog.veteranEnemy=new RewardRule{rewardId="Veterano",experience=3,coins=3,itemChance=.14f,instantCurrency=true};
            catalog.challengeEnemy=new RewardRule{rewardId="Desafiador",experience=4,coins=4,itemChance=.19f,minimumRarity=ItemRarity.Uncommon,instantCurrency=true};
            catalog.summonedEnemy=new RewardRule{rewardId="Invocado",experience=1,coins=1,instantCurrency=true};
            foreach(var rule in new[]{catalog.enemy,catalog.boss,catalog.miniboss,catalog.chest,catalog.completion,catalog.exploration,catalog.supplies})rule.instantCurrency=true;
            catalog.Validate();EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();Debug.Log("RPG012_UPGRADE_OK");
        }
    }
}
