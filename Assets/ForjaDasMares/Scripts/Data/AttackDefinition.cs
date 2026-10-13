using UnityEngine;

namespace ForjaDasMares
{
    public enum AttackShape { Arc, Projectile }
    public enum ControlEffect { None, Slow, Root }
    [CreateAssetMenu(menuName = "Forja/Attack")]
    public sealed class AttackDefinition : ScriptableObject
    {
        public AttackShape shape;
        [Min(1)] public int damage = 26;
        [Min(0.1f)] public float range = 2.8f;
        [Range(1, 360)] public float arc = 120f;
        [Min(0)] public float windup = 0.1f;
        [Min(0.05f)] public float recovery = 0.35f;
        [Min(0)] public float knockback = 2f;
        [Min(0.1f)] public float projectileSpeed = 15f;
        [Min(0.02f)] public float projectileRadius = 0.15f;
        [Min(0)] public float splashRadius;
        public ControlEffect control;
        [Min(0)] public float controlDuration;
        [Range(0.05f, 1)] public float slowMultiplier = 0.5f;
        public Color color = Color.yellow;
    }
}
