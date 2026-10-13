using UnityEngine;

namespace ForjaDasMares
{
    public sealed class HeroCombat : MonoBehaviour
    {
        [SerializeField] PlayerMotor motor;
        [SerializeField] Health health;
        [SerializeField] Transform origin;
        [SerializeField] CombatEffectPool pool;
        [SerializeField] LayerMask targets, geometry, allies;
        readonly DamageResolver resolver = new DamageResolver();
        readonly Collider[] nearbyAllies = new Collider[24];
        HeroDefinition profile;
        AttackDefinition currentAttack;
        Vector3 facing;
        float elapsed, length, specialCooldown;
        int combo;
        bool active, special, resolved, chargeWindow;
        public bool IsAttacking => active && !special;
        public bool IsUsingSpecial => active && special;
        public bool IsBusy => active;
        public float NormalizedTime => active ? Mathf.Clamp01(elapsed / length) : 0;
        public float SpecialCooldownRemaining => specialCooldown;
        public HeroDefinition Profile => profile;
        public CombatEffectPool Pool => pool;
        public void Configure(PlayerMotor player, Health owner, Transform strike, CombatEffectPool effects, int targetMask, int wallMask, int allyMask)
        { motor = player; health = owner; origin = strike; pool = effects; targets = targetMask; geometry = wallMask; allies = allyMask; }
        public void ApplyHero(HeroDefinition hero) { profile = hero; combo = 0; ResetCombat(); }
        void Awake()
        {
            if (motor == null || health == null || origin == null || pool == null)
            { Debug.LogError("HeroCombat has incomplete scene references.", this); enabled = false; }
        }
        void Update()
        {
            if (motor.Commands is IHeroCommands input)
            {
                if (input.TakeSpecial()) TrySpecial();
                if (input.TakeAttack()) TryAttack();
            }
            Simulate(Time.deltaTime);
        }
        bool CanStart => profile != null && health.IsAlive && !active && !motor.IsDodging && !motor.IsCharging;
        public bool TryAttack()
        {
            if (!CanStart || profile.attacks == null || profile.attacks.Length == 0) return false;
            currentAttack = profile.attacks[combo % profile.attacks.Length]; combo++;
            Begin(false, currentAttack.windup + currentAttack.recovery); return true;
        }
        public bool TrySpecial()
        {
            if (!CanStart || specialCooldown > 0 || profile.special == null) return false;
            currentAttack = profile.special.hit;
            float extra = profile.special.effect == AbilityEffect.ProtectionCharge ? profile.special.chargeDuration : 0;
            specialCooldown = profile.special.cooldown;
            Begin(true, currentAttack.windup + currentAttack.recovery + extra); return true;
        }
        void Begin(bool ability, float duration)
        {
            active = true; special = ability; elapsed = 0; length = Mathf.Max(0.05f, duration); resolved = chargeWindow = false;
            facing = transform.forward; facing.y = 0; facing.Normalize();
            motor.LockFacing(currentAttack.windup); resolver.Clear();
        }
        public void Simulate(float deltaTime)
        {
            specialCooldown = Mathf.Max(0, specialCooldown - Mathf.Max(0, deltaTime));
            if (!health.IsAlive || motor.IsDodging) { active = false; chargeWindow = false; return; }
            if (!active) return;
            elapsed += Mathf.Max(0, deltaTime);
            if (!resolved && elapsed >= currentAttack.windup)
            { resolved = true; if (special) ResolveSpecial(); else ResolveBasic(); }
            if (chargeWindow && elapsed <= currentAttack.windup + profile.special.chargeDuration)
                resolver.Area(origin.position + facing * 0.4f, 1.4f, facing, 180f, currentAttack, health, targets, geometry, false);
            if (elapsed >= length) { active = false; chargeWindow = false; }
        }
        void ResolveBasic()
        {
            if (currentAttack.shape == AttackShape.Projectile)
            {
                Vector3 start = origin.position + facing * 0.05f;
                if (!Physics.Linecast(origin.position, start, geometry)) pool.Shoot(start, facing, currentAttack, health, targets, geometry);
            }
            else resolver.Area(origin.position, currentAttack.range, facing, currentAttack.arc, currentAttack, health, targets, geometry);
        }
        void ResolveSpecial()
        {
            AbilityDefinition ability = profile.special;
            if (ability.effect == AbilityEffect.Trap)
            {
                Vector3 point = origin.position + facing * ability.placementDistance;
                if (Physics.Linecast(origin.position, point, geometry, QueryTriggerInteraction.Ignore) || Physics.CheckSphere(point, 0.2f, geometry, QueryTriggerInteraction.Ignore)) return;
                pool.PlaceZone(point, ability.radius, ability.trapLifetime, currentAttack, health, targets, geometry, true);
            }
            else if (ability.effect == AbilityEffect.ProtectionCharge)
            {
                health.Protect(ability.protectionReduction, ability.protectionDuration);
                int count = Physics.OverlapSphereNonAlloc(origin.position, ability.radius, nearbyAllies, allies, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count; i++)
                {
                    Health ally = nearbyAllies[i].GetComponentInParent<Health>();
                    if (ally != null && ally.Faction == CombatFaction.Player && ally.IsAlive && !Physics.Linecast(origin.position, nearbyAllies[i].bounds.center, geometry))
                        ally.Protect(ability.protectionReduction, ability.protectionDuration);
                }
                System.Array.Clear(nearbyAllies, 0, count);
                chargeWindow = motor.StartCharge(facing, ability.chargeDuration, ability.chargeSpeed);
                pool.PlaceZone(transform.position, ability.radius, 0.6f, currentAttack, health, targets, geometry, false);
            }
            else
            {
                resolver.Area(origin.position, ability.radius, facing, 360f, currentAttack, health, targets, geometry);
                pool.PlaceZone(transform.position, ability.radius, 0.6f, currentAttack, health, targets, geometry, false);
            }
        }
        public void ResetCombat() { active = special = resolved = chargeWindow = false; elapsed = specialCooldown = 0; resolver.Clear(); if (pool != null) pool.Clear(); }
    }
}
