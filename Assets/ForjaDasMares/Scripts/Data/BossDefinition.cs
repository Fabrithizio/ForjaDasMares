using System;
using UnityEngine;
namespace ForjaDasMares
{
    public enum BossAttackKind { Sweep, GroundMarks, Charge }
    [Serializable]
    public sealed class BossAttack
    {
        public string displayName;
        public BossAttackKind kind;
        public AttackDefinition hit;
        [Min(0.4f)] public float warning = 0.85f;
        [Min(0.1f)] public float recovery = 0.8f, cooldown = 1.6f, range = 7, radius = 3, length = 7, chargeSpeed = 8;
        [Range(20, 180)] public float arc = 120;
        public Color warningColor = new Color(1, 0.25f, 0.05f);
    }
    [CreateAssetMenu(menuName = "Forja/Boss")]
    public sealed class BossDefinition : ScriptableObject
    {
        public BossAttack[] attacks;
        [Range(0.1f, 0.9f)] public float phaseTwoThreshold = 0.5f;
        [Range(0.6f, 1)] public float phaseTwoTiming = 0.85f;
        [Range(1, 2)] public float phaseTwoDamage = 1.15f, phaseTwoRadius = 1.15f;
        [Min(1)] public int rewardSeals = 25;
    }
}
