using UnityEngine;
namespace ForjaDasMares
{
    public struct StatSnapshot
    {
        public int maximumHealth, level;
        public float damageMultiplier, defense, speed, criticalChance, attackSpeed, cooldownMultiplier, elementalDamage, recovery, block;
    }
    public static class ProgressionStats
    {
        public static StatSnapshot WithSoloBonus(StatSnapshot value)
        {value.maximumHealth=Mathf.RoundToInt(value.maximumHealth*1.1f);value.damageMultiplier*=1.1f;return value;}
        public static float ModifierLimit(ItemStat stat) => StatModifier.Limit(stat);
        public static int RequiredExperience(int level) => 60 + 35 * (level - 1);
        public static int Level(int total, int maximum = 20) { int level = 1; while (level < maximum && total >= RequiredExperience(level)) { total -= RequiredExperience(level); level++; } return level; }
        public static int ExperienceInLevel(int total, int maximum = 20) { int level = 1; while (level < maximum && total >= RequiredExperience(level)) { total -= RequiredExperience(level); level++; } return total; }
        public static StatSnapshot Calculate(HeroDefinition hero, PlayerProfile profile, ProgressionCatalog catalog, string replaceId = null, EquipmentSlot replaceSlot = EquipmentSlot.MainHand, bool preview = false)
        {
            var sum = new float[9]; var set = profile.Loadout(hero.heroId); float block = 0;
            for (int slot = 0; slot < 5; slot++)
            {
                if (preview && replaceSlot == EquipmentSlot.MainHand && slot == 1)
                {
                    var candidate = profile.Item(replaceId); var main = candidate != null ? catalog.Find(candidate.definitionId) : null;
                    if (main == null || main.twoHanded || !main.allowsOffHand) continue;
                }
                string id = preview && slot == (int)replaceSlot ? replaceId : set != null ? set.items[slot] : null;
                var instance = profile.Item(id); if (instance == null) continue; var data = catalog.Find(instance.definitionId); if (data == null) continue;
                var rarity = catalog.Rarity(instance.rarity);
                foreach (var bonus in data.bonuses) sum[(int)bonus.stat] += Mathf.Clamp(bonus.value, 0, ModifierLimit(bonus.stat)) * rarity.power;
                foreach (var bonus in instance.modifiers) sum[(int)bonus.stat] += Mathf.Clamp(bonus.value, 0, ModifierLimit(bonus.stat));
                if (data.category == WeaponCategory.Shield) block = Mathf.Clamp(data.frontalBlock * rarity.power, 0, .5f);
            }
            int level = Level(profile.experience, catalog.maximumLevel);
            return new StatSnapshot
            {
                level = level, maximumHealth = Mathf.RoundToInt(hero.maximumHealth * (1 + .03f * (level - 1)) + Mathf.Min(sum[0], 160)),
                damageMultiplier = 1 + .025f * (level - 1) + Mathf.Min(sum[1], .5f),
                defense = Mathf.Min(.55f, 1 - (1 - hero.armor) * (1 - sum[2] / (sum[2] + 100))),
                speed = hero.speed * (1 + Mathf.Min(sum[3], .25f)), criticalChance = Mathf.Min(sum[4], .3f), attackSpeed = 1 + Mathf.Min(sum[5], .45f),
                cooldownMultiplier = 1 - Mathf.Min(sum[6], .35f), elementalDamage = Mathf.Min(sum[7], .25f), recovery = Mathf.Min(sum[8], 2), block = block
            };
        }
        public static bool CanEquip(PlayerProfile profile, ProgressionCatalog catalog, string heroId, ItemInstance item, EquipmentSlot slot, out string reason)
        {
            reason = ""; if (item == null || profile.Item(item.instanceId) == null) { reason = "Item não está no inventário."; return false; }
            var data = catalog.Find(item.definitionId); var hero = catalog.Hero(heroId);
            if (data == null || hero == null || (data.compatibleHeroes & hero.affinity) == 0) { reason = "Incompatível com este herói."; return false; }
            if (Level(profile.experience, catalog.maximumLevel) < data.minimumLevel) { reason = "Requer nível " + data.minimumLevel; return false; }
            var loadout = profile.Loadout(heroId); var main = loadout != null ? profile.Item(loadout.items[0]) : null; var mainData = main != null ? catalog.Find(main.definitionId) : null;
            bool secondary = slot == EquipmentSlot.OffHand && data.slot == EquipmentSlot.MainHand && data.category == WeaponCategory.Daggers && hero.allowSecondaryWeapon;
            if (slot != data.slot && !secondary) { reason = "Slot incompatível."; return false; }
            if (slot == EquipmentSlot.OffHand && (mainData == null || mainData.twoHanded || !mainData.allowsOffHand)) { reason = "A arma principal não permite secundária."; return false; }
            if (loadout != null) for (int i = 0; i < 5; i++) if (i != (int)slot && loadout.items[i] == item.instanceId) { reason = "Este item já ocupa outro slot."; return false; }
            return true;
        }
    }
}
