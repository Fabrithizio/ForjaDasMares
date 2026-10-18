using UnityEngine;
namespace ForjaDasMares
{
    public enum EnemyState { Idle, Patrol, Detect, Chase, Attack, Retreat, Stunned, Dead }
    [DefaultExecutionOrder(-40)]
    public sealed class EnemyBrain : MonoBehaviour
    {
        EnemyActor actor;
        Health target;
        int geometry;
        Vector3 home, lastKnown, patrol;
        float perception, idleTime, forgetTime, detectTime;
        bool visible, aware;
        int bossIndex;
        [SerializeField] EnemyState state;
        public EnemyState State => state;
        public EnemyDefinition Profile => actor != null ? actor.Profile : null;
        public Health Target => target;
        public bool GuardRaised => Profile != null && Profile.kind == EnemyKind.Guardian && actor.IsAlive && !actor.Health.IsStunned && state != EnemyState.Attack && state != EnemyState.Dead;
        public int BossPhase => Profile != null && Profile.boss != null && (float)actor.Health.Current / actor.Health.Maximum <= Profile.boss.phaseTwoThreshold ? 2 : 1;
        public void Prepare(EnemyActor owner, Health enemyTarget, int walls)
        {
            actor = owner; target = enemyTarget; geometry = walls; home = patrol = transform.position;
            perception = Random.value * 0.2f; idleTime = 0.7f; forgetTime = detectTime = 0; aware = visible = false; bossIndex = 0; state = EnemyState.Idle;
        }
        public bool CanSeeTarget()
        {
            if (target == null || !target.IsAlive) return false;
            return !Physics.Linecast(transform.position + Vector3.up, target.transform.position + Vector3.up, geometry, QueryTriggerInteraction.Ignore);
        }
        void Update()
        {
            if (actor == null || !actor.IsAlive) return;
            if(actor.SpawnRemaining>0){actor.Motor.Stop();return;}
            if (!actor.Combat.IsBusy)
            {
                Health selected = actor.SelectTarget();
                if (selected != target) { target = selected; actor.Combat.SetTarget(selected); perception = 0; }
            }
            if (target == null) { actor.Motor.Stop(); actor.Motor.Tick(Time.deltaTime); return; }
            float delta = Time.deltaTime; actor.Combat.Simulate(delta);
            if (!actor.IsAlive || target == null) return;
            if (actor.Health.IsStunned) { state = EnemyState.Stunned; actor.Combat.Cancel(); actor.Motor.Stop(); actor.Motor.Tick(delta); return; }
            if (!target.IsAlive) { state = EnemyState.Idle; actor.Motor.Stop(); actor.Motor.Tick(delta); return; }
            float distance = PlanarDistance(transform.position, target.transform.position);
            perception -= delta;
            if (perception <= 0)
            {
                perception = Profile.perceptionInterval; visible = distance <= Profile.detectionRange && CanSeeTarget();
                if (visible) { lastKnown = target.transform.position; forgetTime = 3; if (!aware) { aware = true; detectTime = 0.25f; state = EnemyState.Detect; } }
            }
            if (actor.Combat.IsBusy) { state = EnemyState.Attack; actor.Motor.Stop(); actor.Motor.Tick(delta); return; }
            if (aware)
            {
                forgetTime -= delta;
                if (distance > Profile.forgetRange || (!visible && forgetTime <= 0)) { aware = false; idleTime = 0.6f; }
            }
            if (!aware) { Patrol(delta); actor.Motor.Tick(delta); return; }
            if (detectTime > 0) { detectTime -= delta; state = EnemyState.Detect; actor.Motor.Stop(); actor.Motor.Face(lastKnown, delta); actor.Motor.Tick(delta); return; }
            actor.Motor.Face(lastKnown, delta);
            bool ranged = Profile.kind == EnemyKind.Archer || Profile.kind == EnemyKind.Summoner;
            if (ranged && distance < Profile.retreatRange)
            {
                Vector3 away = transform.position - target.transform.position; away.y = 0; if (away.sqrMagnitude < 0.01f) away = -transform.forward;
                state = EnemyState.Retreat; actor.Motor.MoveTo(transform.position + away.normalized * 4);
            }
            else if (visible && CanSeeTarget() && TryAttack(distance)) { state = EnemyState.Attack; actor.Motor.Stop(); }
            else if (ranged && visible && distance <= Profile.preferredRange) { state = EnemyState.Idle; actor.Motor.Stop(); }
            else { state = EnemyState.Chase; actor.Motor.MoveTo(lastKnown); }
            actor.Motor.Tick(delta);
        }
        bool TryAttack(float distance)
        {
            if (Profile.kind != EnemyKind.Boss) return distance <= Profile.attack.range && actor.Combat.TryAttack();
            var moves = Profile.boss.attacks;
            for (int i = 0; i < moves.Length; i++)
            {
                int index = (bossIndex + i) % moves.Length;
                if (distance <= moves[index].range && actor.Combat.TryBossAttack(moves[index], BossPhase == 2)) { bossIndex = (index + 1) % moves.Length; return true; }
            }
            return false;
        }
        void Patrol(float delta)
        {
            if (idleTime > 0) { idleTime -= delta; state = EnemyState.Idle; actor.Motor.Stop(); return; }
            if (PlanarDistance(transform.position, patrol) < 0.5f || state != EnemyState.Patrol)
            { float angle = Random.Range(0, Mathf.PI * 2); patrol = home + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * Profile.patrolRadius; }
            state = EnemyState.Patrol; actor.Motor.MoveTo(patrol); actor.Motor.Face(patrol, delta);
        }
        public void Die() { state = EnemyState.Dead; target = null; }
        public void ClearTarget() { target = null; aware = visible = false; }
        public static float PlanarDistance(Vector3 a, Vector3 b) { a.y = b.y = 0; return Vector3.Distance(a, b); }
    }
}
