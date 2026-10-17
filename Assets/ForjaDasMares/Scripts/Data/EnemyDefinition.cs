using UnityEngine;
namespace ForjaDasMares
{
    public enum EnemyKind { Raider, Guardian, Archer, Unstable, Summoner, Minion, Boss }
    [CreateAssetMenu(menuName = "Forja/Enemy")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        public string enemyId, displayName;
        public EnemyKind kind;
        public GameObject prefab;
        [Min(1)] public int maximumHealth = 40, poolCapacity = 4;
        [Range(0, 0.75f)] public float armor;
        [Min(0.1f)] public float speed = 3.5f, acceleration = 24, height = 1.6f, radius = 0.35f;
        public int agentTypeId;
        [Min(0.1f)] public float detectionRange = 16, forgetRange = 20, perceptionInterval = 0.2f, patrolRadius = 2;
        [Min(0)] public float preferredRange = 6, retreatRange = 4, attackCooldown = 1.2f;
        public AttackDefinition attack;
        [Range(10, 180)] public float blockArc = 120;
        [Range(0, 0.95f)] public float blockReduction = 0.75f;
        [Range(0, 1)] public float knockbackResistance;
        [Min(0)] public float staggerDuration = 0.15f;
        public EnemyDefinition summonedEnemy;
        [Min(1)] public int summonPerCast = 1, maxOwnedSummons = 2, summonBudget = 6;
        [Min(0.1f)] public float summonCooldown = 4;
        public BossDefinition boss;
        public bool miniboss;
        public Color color = Color.green;
    }
}
