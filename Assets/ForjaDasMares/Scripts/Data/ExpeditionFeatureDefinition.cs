using UnityEngine;
namespace ForjaDasMares
{
    public enum OptionalEventKind { Waves, Defend, Escort }
    [CreateAssetMenu(menuName="Forja/Expedition features")]
    public sealed class ExpeditionFeatureDefinition:ScriptableObject
    {
        public OptionalEventKind optionalEvent;
        public EnemyDefinition[] enemies;
        public EnemyDefinition miniboss;public GameObject barrelModel,altarModel,cacheModel,challengeModel,wallModel,bridgeModel;[Range(3,20)]public int bridgeDamage=10;
        [Range(4,16)]public int enemyBudget=8;
        [Range(20,90)]public float timeLimit=65;
        [Range(10,30)]public float defenseDuration=22;
        [Range(60,400)]public int objectiveHealth=180;
        [Range(10,60)]public int barrelDamage=30;
        [Range(1,4)]public float barrelRadius=2.8f;
    }
}
