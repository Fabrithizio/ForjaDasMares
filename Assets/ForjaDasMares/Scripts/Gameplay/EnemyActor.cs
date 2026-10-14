using UnityEngine;
namespace ForjaDasMares
{
    public sealed class EnemyActor : MonoBehaviour
    {
        [SerializeField] Health health;
        [SerializeField] EnemyBrain brain;
        [SerializeField] EnemyCombat combat;
        [SerializeField] EnemyMotor motor;
        [SerializeField] EnemyPresentation presentation;
        [SerializeField] DirectionalGuard guard;
        EnemyPool pool;
        EnemyActor summoner;
        EnemyDefinition profile;
        bool live, bound;
        float recycleTime;
        public Health Health => health;
        public EnemyBrain Brain => brain;
        public EnemyCombat Combat => combat;
        public EnemyMotor Motor => motor;
        public EnemyDefinition Profile => profile;
        public Health SelectTarget() => pool != null ? pool.SelectTarget(transform.position) : null;
        public EnemyActor Summoner => summoner;
        public bool IsAlive => live && health.IsAlive;
        public bool IsSpawned => gameObject.activeSelf;
        public void Configure(Health hp, EnemyBrain ai, EnemyCombat attacks, EnemyMotor movement, EnemyPresentation view, DirectionalGuard defense)
        { health = hp; brain = ai; combat = attacks; motor = movement; presentation = view; guard = defense; }
        void Awake() => Bind();
        void Bind() { if (bound) return; health.Changed += OnHealthChanged; bound = true; }
        public bool Activate(EnemyDefinition data, Vector3 position, Health target, EnemyPool registry, CombatEffectPool shots, TelegraphPool warnings, int targets, int geometry, EnemyActor owner)
        {
            Bind(); live = false; profile = data; pool = registry; summoner = owner; recycleTime = 0;
            transform.SetPositionAndRotation(position, Quaternion.identity);
            guard.Configure(brain); health.ConfigureDefense(guard); health.Configure(data.maximumHealth, CombatFaction.Enemy, data.armor);
            motor.Prepare(data); combat.Prepare(this, target, registry, shots, warnings, targets, geometry); brain.Prepare(this, target, geometry); presentation.Prepare(data);
            live = true; gameObject.SetActive(true);
            if (!motor.Activate()) { Recycle(); return false; }
            return true;
        }
        void OnHealthChanged()
        {
            if (!live || health.IsAlive) return;
            live = false; recycleTime = 0.6f; combat.Cancel(); motor.Deactivate(); brain.Die();
            pool.NotifyDeath(this);
        }
        void Update() { if (!live && recycleTime > 0) { recycleTime -= Time.deltaTime; if (recycleTime <= 0) Recycle(); } }
        public void Recycle()
        {
            live = false; recycleTime = 0; combat.Cancel(); brain.ClearTarget(); motor.Deactivate(); summoner = null;
            gameObject.SetActive(false);
        }
        void OnDestroy() { if (bound && health != null) health.Changed -= OnHealthChanged; }
    }
}
