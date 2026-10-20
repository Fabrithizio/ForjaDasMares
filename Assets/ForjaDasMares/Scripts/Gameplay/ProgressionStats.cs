using UnityEngine;
namespace ForjaDasMares
{
    public struct StatSnapshot
    {
        public int maximumHealth, level, baseDamage, damage, criticalDamage;
        public float criticalMultiplier, defensePoints, attacksPerSecond;
        public float damageMultiplier, defense, speed, criticalChance, attackSpeed, cooldownMultiplier, elementalDamage, recovery, block;
    }
    public static class ProgressionStats
    {
        public static StatSnapshot WithSoloBonus(StatSnapshot value)
        {value.maximumHealth=Mathf.RoundToInt(value.maximumHealth*1.1f);value.damageMultiplier*=1.1f;return value;}
        public static float ModifierLimit(ItemStat stat) => StatModifier.Limit(stat);
        public static int MitigatedDamage(int incoming,float defense,float protection=0)=>incoming<=0?0:Mathf.Max(1,Mathf.RoundToInt(incoming*(1-Mathf.Clamp(defense,0,.75f))*(1-Mathf.Clamp(protection,0,.8f))));
        public static int RequiredExperience(int level) => 60 + 35 * (level - 1);
        public static float StrikeMultiplier(float damageMultiplier,float elementalDamage,bool critical,float criticalMultiplier=1.5f)=>damageMultiplier*(1+elementalDamage)*(critical?Mathf.Clamp(criticalMultiplier,1,3):1);
        public static int Level(int total, int maximum = 20) { int level = 1; while (level < maximum && total >= RequiredExperience(level)) { total -= RequiredExperience(level); level++; } return level; }
        public static int ExperienceInLevel(int total, int maximum = 20) { int level = 1; while (level < maximum && total >= RequiredExperience(level)) { total -= RequiredExperience(level); level++; } return total; }
        public static StatSnapshot Calculate(HeroDefinition hero, PlayerProfile profile, ProgressionCatalog catalog, string replaceId = null, EquipmentSlot replaceSlot = EquipmentSlot.MainHand, bool preview = false)
        {
            var sum = new float[10]; var set = profile.Loadout(hero.heroId); float block = 0;
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
            foreach(var talent in catalog.talents)sum[(int)talent.stat]+=HeroTalents.Rank(profile,hero.heroId,talent.id)*talent.perRank;
            int level = Level(profile.experience, catalog.maximumLevel);
            var weaponInstance=profile.Item(preview&&replaceSlot==EquipmentSlot.MainHand?replaceId:set?.items[0]);var weapon=weaponInstance!=null?catalog.Find(weaponInstance.definitionId):null;
            int baseHit=weapon!=null?weapon.attacks[0].damage:hero.attacks[0].damage;float power=weaponInstance!=null?catalog.Rarity(weaponInstance.rarity).power:1;
            float multiplier=1+.025f*(level-1)+Mathf.Min(sum[1],.5f),elemental=Mathf.Min(sum[7],.25f),critical=Mathf.Clamp(catalog.baseCriticalMultiplier+sum[9],1,3);
            float points=Mathf.Clamp(hero.armor*100+sum[2],0,catalog.maximumDefensePoints);
            return new StatSnapshot
            {
                attacksPerSecond=(1+Mathf.Min(sum[5],.45f))/Mathf.Max(.05f,(weapon!=null?weapon.attacks[0]:hero.attacks[0]).windup+(weapon!=null?weapon.attacks[0]:hero.attacks[0]).recovery),baseDamage=baseHit,damage=Mathf.RoundToInt(baseHit*StrikeMultiplier(multiplier*power,elemental,false)),criticalDamage=Mathf.RoundToInt(baseHit*StrikeMultiplier(multiplier*power,elemental,true,critical)),criticalMultiplier=critical,defensePoints=points,
                level = level, maximumHealth = Mathf.RoundToInt(hero.maximumHealth * (1 + .03f * (level - 1)) + Mathf.Min(sum[0], 160)),
                damageMultiplier = 1 + .025f * (level - 1) + Mathf.Min(sum[1], .5f),
                defense = points/100f,
                speed = hero.speed * (1 + Mathf.Min(sum[3], .25f)), criticalChance = Mathf.Min(sum[4], .3f), attackSpeed = 1 + Mathf.Min(sum[5], .45f),
                cooldownMultiplier = 1 - Mathf.Min(sum[6], .35f), elementalDamage = Mathf.Min(sum[7], .25f), recovery = Mathf.Min(sum[8], 2), block = block
            };
        }
        public static bool CanEquip(PlayerProfile profile, ProgressionCatalog catalog, string heroId, ItemInstance item, EquipmentSlot slot, out string reason)
        {
            reason = ""; if (item == null || profile.Item(item.instanceId) == null) { reason = "Item não está no inventário."; return false; }
            foreach(var set in profile.loadouts)if(set.heroId!=heroId)foreach(var id in set.items)if(id==item.instanceId){reason="Equipado em outro herói. Desequipe primeiro.";return false;}
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
