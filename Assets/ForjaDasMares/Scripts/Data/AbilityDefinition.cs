using UnityEngine;

namespace ForjaDasMares
{
    public enum AbilityEffect { AreaImpact, Trap, ElementalBurst, ProtectionCharge }
    [CreateAssetMenu(menuName = "Forja/Ability")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        public string displayName;
        public AbilityEffect effect;
        public AttackDefinition hit;
        [Min(0.1f)] public float cooldown = 5f;
        [Min(0.1f)] public float radius = 3.5f;
        [Min(0.1f)] public float trapLifetime = 6f;
        [Min(0.1f)] public float placementDistance = 2.2f;
        [Min(0.05f)] public float chargeDuration = 0.3f;
        [Min(0.1f)] public float chargeSpeed = 13f;
        [Min(0.1f)] public float protectionDuration = 3f;
        [Range(0, 0.8f)] public float protectionReduction = 0.6f;
    }
}
