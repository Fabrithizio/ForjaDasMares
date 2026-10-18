using System;
using UnityEngine;
namespace ForjaDasMares
{
    public enum EquipmentSlot { MainHand, OffHand, Armor, Accessory, Artifact }
    public enum WeaponCategory { Sword, Axe, Hammer, Bow, Daggers, Staff, Grimoire, Shield, MagicWeapon, None }
    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }
    public enum ItemStat { Health, Damage, Defense, MoveSpeed, CriticalChance, AttackSpeed, CooldownReduction, ElementalDamage, Recovery }
    [Flags] public enum HeroAffinity { Warrior = 1, Ranger = 2, Arcanist = 4, RuneKnight = 8, All = 15 }
    [Serializable] public struct StatModifier
    {
        public ItemStat stat; public float value;
        public StatModifier(ItemStat type, float amount) { stat = type; value = amount; }
        public static float Limit(ItemStat stat) => stat == ItemStat.Health ? 80 : stat == ItemStat.Defense ? 40 : stat == ItemStat.Recovery ? 2 : .3f;
    }
    [CreateAssetMenu(menuName = "Forja/Progression/Equipment")]
    public sealed class EquipmentDefinition : ScriptableObject
    {
        public string itemId, displayName;
        public Sprite icon;
        [TextArea] public string description;
        public EquipmentSlot slot;
        public WeaponCategory category = WeaponCategory.None;
        public HeroAffinity compatibleHeroes = HeroAffinity.All;
        [Min(1)] public int minimumLevel = 1;
        public bool starter;
        public bool twoHanded, allowsOffHand, twinStrike;
        public AttackDefinition[] attacks;
        public AbilityDefinition ability; // Optional class-compatible replacement for the skill, not the ultimate.
        public StatModifier[] bonuses = Array.Empty<StatModifier>();
        [Range(0, .5f)] public float frontalBlock;
        public Color color = Color.white;
        public Vector3 visualScale = new Vector3(.15f, 1, .2f);
    }
}
