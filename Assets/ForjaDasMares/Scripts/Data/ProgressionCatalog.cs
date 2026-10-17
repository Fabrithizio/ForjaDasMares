using System;
using UnityEngine;
namespace ForjaDasMares
{
    [Serializable] public sealed class RarityRule
    {
        public ItemRarity rarity;
        [Min(0)] public int weight;
        [Range(1, 1.5f)] public float power = 1;
        [Range(0, 3)] public int affixCount;
        public Color color = Color.white;
    }
    [Serializable] public sealed class AffixRule
    { public ItemStat stat; public float minimum, maximum; }
    [Serializable] public sealed class RewardRule
    {
        public string rewardId;
        [Min(0)] public int experience, coins, guaranteedItems;
        [Range(0, 1)] public float itemChance;
        public ItemRarity minimumRarity;
    }
    [Serializable] public sealed class HeroEquipmentRule
    { public string heroId; public HeroAffinity affinity; public bool allowSecondaryWeapon; }
    [Serializable] public sealed class CompanionSettings
    {
        public string displayName = "Lume, vaga-lume de pedra";
        [Min(1)] public int healing = 4, charges = 12;
        [Min(1)] public float cooldown = 10;
        [Min(.1f)] public float speed = 5;
        [Min(1)] public float teleportDistance = 8;
    }
    [CreateAssetMenu(menuName = "Forja/Progression/Catalog")]
    public sealed class ProgressionCatalog : ScriptableObject
    {
        [Range(8, 100)] public int inventoryCapacity = 24;
        [Range(4, 64)] public int expeditionCapacity = 32;
        [Range(1, 20)] public int maximumLevel = 20;
        public EquipmentDefinition[] equipment;
        public string[] starterItems;
        public RarityRule[] rarities;
        public AffixRule[] affixes;
        public HeroEquipmentRule[] heroes;
        public RewardRule enemy, boss, chest, room, completion;
        public RewardRule supplies=new RewardRule{rewardId="supplies",experience=1,coins=2,itemChance=.03f};
        public RewardRule exploration=new RewardRule{rewardId="exploration",experience=30,coins=15,guaranteedItems=1,minimumRarity=ItemRarity.Rare};
        public AttackDefinition legendaryEcho;
        public CompanionSettings companion = new CompanionSettings();
        public EquipmentDefinition Find(string id) { if (equipment != null) foreach (var item in equipment) if (item != null && item.itemId == id) return item; return null; }
        public HeroEquipmentRule Hero(string id) { if (heroes != null) foreach (var rule in heroes) if (rule.heroId == id) return rule; return null; }
        public RarityRule Rarity(ItemRarity value) { if (rarities != null) foreach (var rule in rarities) if (rule.rarity == value) return rule; return null; }
        public bool IsRewardRule(RewardRule rule)=>rule!=null&&(rule==enemy||rule==boss||rule==chest||rule==room||rule==completion||rule==supplies||rule==exploration);
        public void Validate()
        {
            if (equipment == null || equipment.Length < 12 || rarities == null || rarities.Length != 5 || heroes == null) throw new InvalidOperationException("Progression catalog is incomplete.");
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (var item in equipment)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.itemId) || !ids.Add(item.itemId) || item.minimumLevel < 1 || item.bonuses == null || !Enum.IsDefined(typeof(EquipmentSlot), item.slot) || !Enum.IsDefined(typeof(WeaponCategory), item.category)) throw new InvalidOperationException("Invalid or duplicate item definition.");
                foreach (var bonus in item.bonuses) if (!Enum.IsDefined(typeof(ItemStat), bonus.stat) || float.IsNaN(bonus.value) || bonus.value < 0 || bonus.value > StatModifier.Limit(bonus.stat)) throw new InvalidOperationException("Invalid equipment modifier.");
                if (item.slot == EquipmentSlot.MainHand && (item.attacks == null || item.attacks.Length == 0)) throw new InvalidOperationException("Weapon has no real attack: " + item.itemId);
                if (item.attacks != null) foreach (var attack in item.attacks) if (attack == null || attack.range <= 0 || attack.damage <= 0 || attack.projectileCount < 1 || attack.projectileCount > 5) throw new InvalidOperationException("Invalid weapon attack.");
            }
            int total = 0; var levels = new System.Collections.Generic.HashSet<ItemRarity>();
            foreach (var rarity in rarities) { if (rarity == null || !Enum.IsDefined(typeof(ItemRarity), rarity.rarity) || !levels.Add(rarity.rarity) || rarity.weight < 0 || rarity.weight > 100000 || float.IsNaN(rarity.power) || rarity.power < 1 || rarity.power > 1.5f || rarity.affixCount < 0 || rarity.affixCount > 3) throw new InvalidOperationException("Invalid rarity."); total += rarity.weight; }
            if (total <= 0 || affixes == null || affixes.Length < 3) throw new InvalidOperationException("No valid loot distribution.");
            var stats = new System.Collections.Generic.HashSet<ItemStat>();
            foreach (var affix in affixes) if (affix == null || !Enum.IsDefined(typeof(ItemStat), affix.stat) || !stats.Add(affix.stat) || float.IsNaN(affix.minimum) || float.IsNaN(affix.maximum) || affix.minimum < 0 || affix.maximum < affix.minimum || affix.maximum > StatModifier.Limit(affix.stat)) throw new InvalidOperationException("Invalid affix range.");
            var heroIds = new System.Collections.Generic.HashSet<string>();
            foreach (var hero in heroes) if (hero == null || string.IsNullOrWhiteSpace(hero.heroId) || !heroIds.Add(hero.heroId) || hero.affinity == 0 || (hero.affinity & ~HeroAffinity.All) != 0) throw new InvalidOperationException("Invalid hero equipment rule.");
            if (inventoryCapacity < 8 || inventoryCapacity > 100 || expeditionCapacity < 4 || expeditionCapacity > 64 || maximumLevel < 1 || maximumLevel > 20 || starterItems == null || starterItems.Length > inventoryCapacity) throw new InvalidOperationException("Invalid progression capacity.");
            foreach (var rule in new[] { enemy, boss, chest, room, completion, supplies, exploration }) if (rule == null || rule.experience < 0 || rule.coins < 0 || rule.guaranteedItems < 0 || rule.guaranteedItems > 2 || rule.itemChance < 0 || rule.itemChance > 1) throw new InvalidOperationException("Invalid reward table.");
            foreach (var id in starterItems) if (Find(id) == null) throw new InvalidOperationException("Missing starter item.");
        }
    }
}
