using System;
using UnityEngine;
namespace ForjaDasMares
{
    public enum EncounterState { Ready, Spawning, Fighting, Won, Failed }
    public sealed class EncounterController : MonoBehaviour
    {
        [SerializeField] EnemyPool enemies;
        [SerializeField] HeroRoster heroes;
        [SerializeField] ArenaSession arena;
        [SerializeField] EncounterDefinition[] presets;
        [SerializeField] Transform[] spawnPoints;
        EncounterDefinition current;
        Vector3[] overridePoints;
        int selected, waveIndex, entryIndex, spawnedInEntry, spawnSequence, bossReward;
        float timer, waveTime, spawnFailureTime;
        bool bossSpawned, bossDefeated;
        EnemyActor boss;
        public EncounterState State { get; private set; }
        public EnemyPool Enemies => enemies;
        public HeroRoster Heroes => heroes;
        public EncounterDefinition Current => current;
        public EncounterDefinition Selected => presets[selected];
        public EncounterDefinition[] Presets => presets;
        public int WaveNumber => waveIndex + 1;
        public int Seals { get; private set; }
        public EnemyActor Boss => boss;
        public event Action Completed, Failed;
        public void Configure(EnemyPool pool, HeroRoster player, ArenaSession reset, EncounterDefinition[] data, Transform[] sockets)
        { enemies = pool; heroes = player; arena = reset; presets = data; spawnPoints = sockets; }
        void Awake() { arena.Resetting += Cancel; heroes.Changed += Cancel; enemies.Spawned += Spawned; enemies.Died += Died; }
        void OnDestroy() { if (arena != null) arena.Resetting -= Cancel; if (heroes != null) heroes.Changed -= Cancel; if (enemies != null) { enemies.Spawned -= Spawned; enemies.Died -= Died; } }
        public void NextPreset() { if (State == EncounterState.Spawning || State == EncounterState.Fighting) return; selected = (selected + 1) % presets.Length; }
        public void StartSelected() { overridePoints = null; Begin(Selected, true); }
        public void SetSpawnPoints(Vector3[] positions) => overridePoints = positions;
        public void Begin(EncounterDefinition data, bool resetPlayer = false)
        {
            if (resetPlayer) arena.ResetArena(); else Cancel();
            current = data; waveIndex = entryIndex = spawnedInEntry = spawnSequence = bossReward = 0; bossSpawned = bossDefeated = false; boss = null;
            if (current == null || current.waves == null || current.waves.Length == 0) { Fail(); return; }
            enemies.SetLimit(Application.isMobilePlatform ? data.maximumMobileEnemies : data.maximumPcEnemies);
            NextWave();
        }
        void NextWave() { entryIndex = spawnedInEntry = 0; waveTime = spawnFailureTime = 0; timer = current.waves[waveIndex].delayBefore; State = EncounterState.Spawning; }
        void Update()
        {
            if (State != EncounterState.Spawning && State != EncounterState.Fighting) return;
            if (!heroes.Health.IsAlive) { Fail(); return; }
            float dt = Time.deltaTime; waveTime += dt; timer -= dt;
            var wave = current.waves[waveIndex];
            if (State == EncounterState.Spawning && timer <= 0)
            {
                if (entryIndex >= wave.entries.Length) State = EncounterState.Fighting;
                else
                {
                    var entry = wave.entries[entryIndex];
                    Vector3 position = overridePoints != null && overridePoints.Length > 0 ? overridePoints[spawnSequence % overridePoints.Length] : spawnPoints[spawnSequence % spawnPoints.Length].position;
                    if (enemies.TrySpawn(entry.enemy, position) != null)
                    {
                        spawnSequence++; spawnedInEntry++; spawnFailureTime = 0; timer = entry.delay;
                        if (spawnedInEntry >= entry.count) { entryIndex++; spawnedInEntry = 0; }
                    }
                    else
                    {
                        // Full pools wait for space; invalid geometry fails visibly instead of granting a false victory.
                        timer = 0.2f; if (enemies.ActiveCount < enemies.Limit) spawnFailureTime += 0.2f;
                        if (spawnFailureTime > 8) { Debug.LogError("Encounter could not place its configured enemy.", this); Fail(); return; }
                    }
                }
            }
            if (State == EncounterState.Fighting && enemies.ActiveCount == 0 && (wave.completion != WaveEndCondition.SurviveDuration || waveTime >= wave.survivalDuration) && (wave.completion != WaveEndCondition.DefeatBoss || bossSpawned && bossDefeated))
            {
                waveIndex++; if (waveIndex < current.waves.Length) NextWave();
                else { State = EncounterState.Won; Seals += bossReward; heroes.Victory(); Completed?.Invoke(); }
            }
        }
        void Spawned(EnemyActor actor) { if (actor.Profile.kind == EnemyKind.Boss) { boss = actor; bossSpawned = true; } }
        void Died(EnemyActor actor) { if (actor == boss) { bossDefeated = true; bossReward = actor.Profile.boss.rewardSeals; } }
        void Fail() { State = EncounterState.Failed; enemies.Clear(); boss = null; Failed?.Invoke(); }
        public void Cancel() { State = EncounterState.Ready; current = null; boss = null; bossReward = 0; enemies.Clear(); }
    }
}
