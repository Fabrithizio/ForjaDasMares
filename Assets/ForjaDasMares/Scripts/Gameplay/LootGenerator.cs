using System;
using System.Collections.Generic;
namespace ForjaDasMares
{
    public static class LootGenerator
    {
        public static ItemInstance Create(ProgressionCatalog catalog, string definitionId, ItemRarity rarity, Random random)
        {
            if (catalog.Find(definitionId) == null || catalog.Rarity(rarity) == null) throw new ArgumentException("Unknown loot definition.");
            var result = new ItemInstance { instanceId = Guid.NewGuid().ToString("N"), definitionId = definitionId, rarity = rarity };
            var choices = new List<AffixRule>(catalog.affixes);
            for (int i = 0; i < catalog.Rarity(rarity).affixCount && choices.Count > 0; i++)
            {
                int pick = random.Next(choices.Count); var affix = choices[pick]; choices.RemoveAt(pick);
                result.modifiers.Add(new StatModifier(affix.stat, (float)(affix.minimum + random.NextDouble() * (affix.maximum - affix.minimum))));
            }
            return result;
        }
        public static ItemRarity RollRarity(ProgressionCatalog catalog, Random random, ItemRarity minimum = ItemRarity.Common)
        {
            int weight = 0; foreach (var rule in catalog.rarities) if (rule.rarity >= minimum) weight += rule.weight;
            if (weight == 0) throw new InvalidOperationException("Reward has no rarity weights.");
            int value = random.Next(weight); foreach (var rule in catalog.rarities) if (rule.rarity >= minimum) { value -= rule.weight; if (value < 0) return rule.rarity; }
            throw new InvalidOperationException("Invalid weighted roll.");
        }
        public static List<ItemInstance> RewardItems(ProgressionCatalog catalog, RewardRule table, Random random,string heroId=null)
        {
            int count = table.guaranteedItems + (random.NextDouble() < table.itemChance ? 1 : 0); var result = new List<ItemInstance>(count);
            var eligible=new List<EquipmentDefinition>();var rule=heroId!=null?catalog.Hero(heroId):null;
            foreach(var item in catalog.equipment)if(!item.starter&&(table.maximumRequiredLevel==0||item.minimumLevel<=table.maximumRequiredLevel)&&(!table.weaponsOnly||item.slot==EquipmentSlot.MainHand)&&(rule==null||(item.compatibleHeroes&rule.affinity)!=0))eligible.Add(item);
            if(eligible.Count==0)return result;
            for (int i = 0; i < count; i++)
            {
                var definition = eligible[random.Next(eligible.Count)];
                result.Add(Create(catalog, definition.itemId, table.fixedRarity?table.minimumRarity:RollRarity(catalog, random, table.minimumRarity), random));
            }
            return result;
        }
    }
}
