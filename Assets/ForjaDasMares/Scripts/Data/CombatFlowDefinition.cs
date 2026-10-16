using UnityEngine;
namespace ForjaDasMares
{
    [CreateAssetMenu(menuName = "Forja/Combat flow")]
    public sealed class CombatFlowDefinition : ScriptableObject
    {
        [Min(.2f)] public float holdDuration = 1.35f;
        [Min(.1f)] public float heavyCooldown = 3;
        [Min(.1f)] public float heavyDuration = .8f;
        [Min(.05f)] public float heavyWindup = .4f;
        [Min(1)] public float heavyMultiplier = 2.4f;
        [Min(1)] public float heavyRangeMultiplier = 1.25f;
        [Min(1)] public int hitsForSuper = 16;
        [Min(.1f)] public float superInputBuffer = 1.5f;
        public AttackDefinition superAttack;
    }
}
