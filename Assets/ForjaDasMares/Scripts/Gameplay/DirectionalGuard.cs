using UnityEngine;
namespace ForjaDasMares
{
    public sealed class DirectionalGuard : MonoBehaviour, IHitDefense
    {
        [SerializeField] EnemyBrain brain;
        public void Configure(EnemyBrain owner) => brain = owner;
        public float DamageMultiplier(Vector3 incomingDirection, Health source, AttackDefinition attack)
        {
            if (brain == null || !brain.GuardRaised) return 1;
            incomingDirection.y = 0;
            if (incomingDirection.sqrMagnitude < 0.001f) return 1;
            return Vector3.Dot(transform.forward, -incomingDirection.normalized) >= Mathf.Cos(brain.Profile.blockArc * 0.5f * Mathf.Deg2Rad) ? 1 - brain.Profile.blockReduction : 1;
        }
    }
}
