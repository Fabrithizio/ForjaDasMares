using UnityEngine;
namespace ForjaDasMares
{
    public sealed class EnemyCombat : MonoBehaviour
    {
        readonly DamageResolver resolver = new DamageResolver();
        readonly Vector3[] marks = new Vector3[3];
        EnemyActor actor;
        Health target;
        EnemyPool enemies;
        CombatEffectPool projectiles;
        TelegraphPool warnings;
        AttackDefinition attack;
        BossAttack bossMove;
        Vector3 origin, direction;
        int targets, geometry, summonCount;
        float elapsed, windup, recovery, cooldown, scale, radius;
        bool busy, resolved, summoning;
        public bool IsBusy => busy;
        public void SetTarget(Health player) { if (!busy) target = player; }
        public float WarningRemaining => busy && !resolved ? Mathf.Max(0, windup - elapsed) : 0;
        public BossAttackKind? CurrentBossAttack => bossMove != null && busy ? bossMove.kind : (BossAttackKind?)null;
        public void Prepare(EnemyActor owner, Health player, EnemyPool registry, CombatEffectPool shots, TelegraphPool signs, int playerTargets, int walls)
        { actor = owner; target = player; enemies = registry; projectiles = shots; warnings = signs; targets = playerTargets; geometry = walls; cooldown = 0; summonCount = 0; Cancel(); }
        bool CanStart(float range)
        { return actor.IsAlive && target != null && target.IsAlive && !actor.Health.IsStunned && !busy && cooldown <= 0 && EnemyBrain.PlanarDistance(transform.position, target.transform.position) <= range && !Physics.Linecast(transform.position + Vector3.up, target.transform.position + Vector3.up, geometry, QueryTriggerInteraction.Ignore); }
        void Begin(AttackDefinition hit, float delay, float recover)
        {
            attack = hit; windup = Mathf.Max(0.4f, delay); recovery = recover; elapsed = 0; resolved = false; busy = true; scale = 1;
            origin = transform.position + Vector3.up;
            direction = target.transform.position - transform.position; direction.y = 0; direction = direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
            transform.rotation = Quaternion.LookRotation(direction); resolver.Clear();
        }
        public bool TryAttack()
        {
            var data = actor.Profile;
            if (!CanStart(data.attack.range)) return false;
            bossMove = null; summoning = data.kind == EnemyKind.Summoner && data.summonedEnemy != null && enemies.CountOwned(actor) < data.maxOwnedSummons && summonCount < data.summonBudget;
            Begin(data.attack, data.attack.windup, data.attack.recovery); radius = data.attack.range;
            var shape = summoning || data.kind == EnemyKind.Unstable ? TelegraphShape.Circle : data.attack.shape == AttackShape.Projectile ? TelegraphShape.Line : TelegraphShape.Cone;
            if (warnings.Acquire(shape, transform.position, direction, summoning ? 1.2f : shape == TelegraphShape.Line ? data.attack.projectileRadius + 0.1f : radius, radius, data.attack.arc, windup, data.color, this) == null) { Cancel(); return false; }
            return true;
        }
        public bool TryBossAttack(BossAttack move, bool phaseTwo)
        {
            if (move == null || move.hit == null || !CanStart(move.range)) return false;
            Begin(move.hit, move.warning * (phaseTwo ? actor.Profile.boss.phaseTwoTiming : 1), move.recovery);
            bossMove = move; summoning = false; scale = phaseTwo ? actor.Profile.boss.phaseTwoDamage : 1;
            radius = move.radius * (phaseTwo ? actor.Profile.boss.phaseTwoRadius : 1);
            if (move.kind == BossAttackKind.GroundMarks)
            {
                Vector3 side = Vector3.Cross(Vector3.up, direction);
                for (int i = 0; i < 3; i++)
                {
                    marks[i] = target.transform.position + side * ((i - 1) * radius * 2.1f);
                    if (warnings.Acquire(TelegraphShape.Circle, marks[i], direction, radius, 0, 360, windup, move.warningColor, this) == null) { Cancel(); return false; }
                }
            }
            else if (warnings.Acquire(move.kind == BossAttackKind.Charge ? TelegraphShape.Line : TelegraphShape.Cone, transform.position, direction, radius, move.length + (move.kind == BossAttackKind.Charge ? radius : 0), move.arc, windup, move.warningColor, this) == null) { Cancel(); return false; }
            return true;
        }
        public void Simulate(float deltaTime)
        {
            cooldown = Mathf.Max(0, cooldown - deltaTime);
            if (!busy) return;
            elapsed += deltaTime;
            if (!resolved && elapsed >= windup)
            {
                resolved = true; warnings.ReleaseOwner(this);
                if (bossMove != null) ResolveBoss(); else ResolveNormal();
            }
            if (busy && resolved && bossMove != null && bossMove.kind == BossAttackKind.Charge && actor.Motor.IsRushing)
                resolver.Area(transform.position + Vector3.up, radius, direction, 180, attack, actor.Health, targets, geometry, false, scale);
            float rushLength = bossMove != null && bossMove.kind == BossAttackKind.Charge ? bossMove.length / bossMove.chargeSpeed : 0;
            if (busy && elapsed >= windup + recovery + rushLength)
            { busy = false; cooldown = bossMove != null ? bossMove.cooldown : summoning ? actor.Profile.summonCooldown : actor.Profile.attackCooldown; resolver.Clear(); }
        }
        void ResolveNormal()
        {
            if (summoning)
            {
                for (int i = 0; i < actor.Profile.summonPerCast && summonCount < actor.Profile.summonBudget && enemies.CountOwned(actor) < actor.Profile.maxOwnedSummons; i++)
                    if (enemies.TrySpawn(actor.Profile.summonedEnemy, transform.position + direction * 1.5f + transform.right * i, actor) != null) summonCount++;
            }
            else if (attack.shape == AttackShape.Projectile) projectiles.Shoot(origin + direction * 0.1f, direction, attack, actor.Health, targets, geometry);
            else resolver.Area(origin, radius, direction, actor.Profile.kind == EnemyKind.Unstable ? 360 : attack.arc, attack, actor.Health, targets, geometry);
            if (actor.Profile.kind == EnemyKind.Unstable) actor.Health.ApplyDamage(actor.Health.Maximum * 4);
        }
        void ResolveBoss()
        {
            switch (bossMove.kind)
            {
                case BossAttackKind.Sweep: resolver.Area(origin, radius, direction, bossMove.arc, attack, actor.Health, targets, geometry, true, scale); break;
                case BossAttackKind.GroundMarks:
                    for (int i = 0; i < 3; i++)
                        if (!Physics.Linecast(origin, marks[i] + Vector3.up, geometry, QueryTriggerInteraction.Ignore)) resolver.Area(marks[i] + Vector3.up, radius, direction, 360, attack, actor.Health, targets, geometry, i == 0, scale);
                    break;
                case BossAttackKind.Charge: actor.Motor.StartRush(direction, bossMove.length / bossMove.chargeSpeed, bossMove.chargeSpeed); break;
            }
        }
        public void Cancel()
        { busy = resolved = false; bossMove = null; attack = null; elapsed = 0; resolver.Clear(); if (warnings != null) warnings.ReleaseOwner(this); }
        void OnDisable() { Cancel(); target = null; actor = null; enemies = null; }
    }
}
