using System;
using UnityEngine;
namespace ForjaDasMares
{
    public enum WaveEndCondition { EliminateAll, SurviveDuration, DefeatBoss }
    [Serializable] public sealed class SpawnEntry
    {
        public EnemyDefinition enemy;
        [Min(1)] public int count = 1;
        [Min(0)] public float delay = 0.4f;
    }
    [Serializable] public sealed class EncounterWave
    {
        [Range(0,3)] public int reinforceThreshold;
        public string displayName;
        public SpawnEntry[] entries;
        public WaveEndCondition completion;
        [Min(0)] public float delayBefore = 1, survivalDuration = 20;
    }
    [CreateAssetMenu(menuName = "Forja/Encounter")]
    public sealed class EncounterDefinition : ScriptableObject
    {
        public string encounterId, displayName;
        public EncounterWave[] waves;
        [Min(1)] public int maximumMobileEnemies = 12, maximumPcEnemies = 18;
    }
}
