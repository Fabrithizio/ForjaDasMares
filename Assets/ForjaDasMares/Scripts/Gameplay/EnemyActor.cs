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
        float recycleTime,arrival;
        public float SpawnRemaining=>arrival;
        EnemyArrivalVisual arrivalView;
        void ArrivalView(){if(arrivalView==null){arrivalView=GetComponent<EnemyArrivalVisual>();if(arrivalView==null)arrivalView=gameObject.AddComponent<EnemyArrivalVisual>();arrivalView.Configure(this);}}
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
            Bind(); live = false; profile = data; pool = registry; summoner = owner; recycleTime = 0;arrival=Mathf.Clamp(data.spawnWarning,0,2);ArrivalView();(GetComponent<ActorImpactFeedback>()??gameObject.AddComponent<ActorImpactFeedback>()).Configure(health,data.feedback);
            brain.enabled=combat.enabled=motor.enabled=enabled=true;
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
        void Update() { arrival=Mathf.Max(0,arrival-Time.deltaTime);if (!live && recycleTime > 0) { recycleTime -= Time.deltaTime; if (recycleTime <= 0) Recycle(); } }
        public void Recycle()
        {
            live = false; recycleTime = arrival = 0; combat.Cancel(); brain.ClearTarget(); motor.Deactivate(); summoner = null;
            gameObject.SetActive(false);
        }
        public void SetReplica(EnemyDefinition data, int hitPoints, int maximum, EnemyState state,float warning=0,bool opening=false)
        {
            live = false; profile = data; recycleTime = 0;arrival=Mathf.Clamp(warning,0,2);ArrivalView();(GetComponent<ActorImpactFeedback>()??gameObject.AddComponent<ActorImpactFeedback>()).Configure(health,data.feedback);
            brain.enabled = combat.enabled = motor.enabled = enabled = false;
            motor.Deactivate(); gameObject.SetActive(true);
            health.SetReplicaState(hitPoints, maximum);
            presentation.ShowReplica(data, state, hitPoints > 0,opening);
        }
        void OnDestroy() { if (bound && health != null) health.Changed -= OnHealthChanged; }
    }
}
