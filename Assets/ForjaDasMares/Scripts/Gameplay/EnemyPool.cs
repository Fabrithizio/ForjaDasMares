using System;
using UnityEngine;
using UnityEngine.AI;
namespace ForjaDasMares
{
    public sealed class EnemyPool : MonoBehaviour
    {
        [SerializeField] EnemyDefinition[] definitions;
        [SerializeField] HeroRoster heroes;
        [SerializeField] CombatEffectPool projectiles;
        [SerializeField] TelegraphPool warnings;
        [SerializeField] LayerMask geometry, spawnBlockers, targets;
        EnemyActor[] actors;
        EnemyDefinition[] slots;
        int limit = 12;
        public int Limit => limit;
        public EnemyActor[] Actors => actors;
        public int Capacity => actors != null ? actors.Length : 0;
        public EnemyDefinition DefinitionAt(int index) => slots[index];
        public int ActiveCount { get { int count = 0; if (actors != null) foreach (var actor in actors) if (actor.IsAlive) count++; return count; } }
        public event Action<EnemyActor> Spawned, Died;
        public CombatParty Party { get; set; }
        public bool HasLivingPlayers => Party != null ? Party.AnyAlive : heroes.Health.IsAlive;
        public Health SelectTarget(Vector3 position) => Party != null ? Party.Nearest(position) : heroes.Health;
        public void Configure(EnemyDefinition[] data, HeroRoster player, CombatEffectPool shots, TelegraphPool signs, int walls, int blockers, int playerTargets)
        { definitions = data; heroes = player; projectiles = shots; warnings = signs; geometry = walls; spawnBlockers = blockers; targets = playerTargets; }
        void Awake()
        {
            int total = 0; foreach (var data in definitions) total += data.poolCapacity;
            actors = new EnemyActor[total]; slots = new EnemyDefinition[total]; int slot = 0;
            foreach (var data in definitions)
                for (int i = 0; i < data.poolCapacity; i++)
                {
                    var go = Instantiate(data.prefab, transform); go.name = data.enemyId + "_Pooled_" + i; go.SetActive(false);
                    actors[slot] = go.GetComponent<EnemyActor>(); slots[slot] = data; slot++;
                }
        }
        public void SetLimit(int maximum) => limit = Mathf.Clamp(maximum, 1, Capacity);
        public bool HasAvailableSlot(EnemyDefinition data)
        {for(int i=0;i<actors.Length;i++)if(slots[i]==data&&!actors[i].IsSpawned)return true;return false;}
        public bool ThreatNear(Vector3 point,float radius)
        {foreach(var actor in actors)if(actor.gameObject.activeInHierarchy&&actor.Health.IsAlive&&(actor.transform.position-point).sqrMagnitude<radius*radius)return true;return false;}
        public EnemyActor TrySpawn(EnemyDefinition data, Vector3 desired, EnemyActor summoner = null)
        {
            if (ActiveCount >= limit || data == null || !HasLivingPlayers) return null;
            var filter = new NavMeshQueryFilter { agentTypeID = data.agentTypeId, areaMask = NavMesh.AllAreas };
            for (int attempt = 0; attempt < 12; attempt++)
            {
                float angle = attempt * 137.5f * Mathf.Deg2Rad; float radius = attempt == 0 ? 0 : 0.65f + 0.2f * attempt;
                Vector3 candidate = desired + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 0.6f, filter)) continue;
                Vector3 position = hit.position + Vector3.up * 0.06f;
                float r = data.radius + 0.04f;
                if (Physics.CheckCapsule(position + Vector3.up * r, position + Vector3.up * (data.height - r), r, spawnBlockers, QueryTriggerInteraction.Ignore)) continue;
                for (int i = 0; i < actors.Length; i++)
                    if (slots[i] == data && !actors[i].IsSpawned)
                    {
                        if (!actors[i].Activate(data, position, SelectTarget(position), this, projectiles, warnings, targets, geometry, summoner)) return null;
                        Spawned?.Invoke(actors[i]); return actors[i];
                    }
                return null;
            }
            return null;
        }
        public int CountOwned(EnemyActor owner) { int count = 0; foreach (var actor in actors) if (actor.IsAlive && actor.Summoner == owner) count++; return count; }
        public void NotifyDeath(EnemyActor actor)
        {
            projectiles.ClearOwned(actor.Health); warnings.ReleaseOwner(actor.Combat);
            foreach (var minion in actors) if (minion.IsSpawned && minion.Summoner == actor) minion.Recycle();
            Died?.Invoke(actor);
        }
        public void Clear()
        {
            if (actors != null) foreach (var actor in actors) if (actor.IsSpawned) actor.Recycle();
            warnings.Clear(); projectiles.Clear();
        }
    }
}
