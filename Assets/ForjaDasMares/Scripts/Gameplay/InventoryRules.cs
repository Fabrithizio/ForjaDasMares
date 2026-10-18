using UnityEngine;
namespace ForjaDasMares
{
    public static class InventoryRules
    {
        public static bool Equipped(PlayerProfile profile,string id)
        {if(string.IsNullOrEmpty(id))return false;foreach(var set in profile.loadouts)foreach(var value in set.items)if(value==id)return true;return false;}
        public static int BagCount(PlayerProfile profile)
        {int count=0;foreach(var item in profile.inventory)if(!Equipped(profile,item.instanceId))count++;return count;}
        public static int SellValue(ProgressionCatalog catalog,ItemInstance item)
        {if(item==null)return 0;var definition=catalog.Find(item.definitionId);return definition==null?0:Mathf.Clamp(catalog.salvageBase*(1+(int)item.rarity*2)+definition.minimumLevel,1,1000);}
    }
}
