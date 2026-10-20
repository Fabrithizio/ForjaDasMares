using System;
using UnityEngine;

namespace ForjaDasMares
{
    [Serializable]
    public sealed class DodgeSettings
    {
        [Min(0.05f)] public float duration = 0.22f;
        [Min(0.1f)] public float speed = 12f;
        [Min(0)] public float cooldown = 0.8f;
        [Min(0)] public float invulnerableStart = 0.03f;
        [Min(0)] public float invulnerableEnd = 0.15f;
    }
    [CreateAssetMenu(menuName = "Forja/Hero")]
    public sealed class HeroDefinition : ScriptableObject
    {
        public string heroId, displayName, role;
        [TextArea] public string strengths, weaknesses;
        [Min(1)] public int maximumHealth = 140;
        [Range(0, 0.75f)] public float armor;
        [Min(0.1f)] public float speed = 5.8f;
        [Min(1)] public float acceleration = 40f, deceleration = 50f;
        public DodgeSettings dodge = new DodgeSettings();
        public AttackDefinition[] attacks;
        public AbilityDefinition special;
        public CombatFlowDefinition onlineFlow;
        public Material attackAreaMaterial;
        public Color color = Color.cyan;
        public int visualSlot;
    }
}
