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
        AttackDefinition[] equipmentAttacks;
        float damageMultiplier = 1, attackSpeed = 1, cooldownMultiplier = 1, criticalChance, elementalDamage, strikeScale = 1;
        bool twinStrike, secondResolved;
        AttackDefinition legendaryEcho;
        bool echoResolved;
        public AttackDefinition BasicAttack => equipmentAttacks != null && equipmentAttacks.Length > 0 ? equipmentAttacks[0] : profile != null ? profile.attacks[0] : null;
        public float AttackSpeed => attackSpeed;
        public void ApplyEquipment(AttackDefinition[] attacks, StatSnapshot stats, float rarityPower, bool twin, AttackDefinition echo = null)
        { equipmentAttacks = attacks; damageMultiplier = stats.damageMultiplier * rarityPower; attackSpeed = stats.attackSpeed; cooldownMultiplier = stats.cooldownMultiplier; criticalChance = stats.criticalChance; elementalDamage = stats.elementalDamage; twinStrike = twin; legendaryEcho = echo; }
        public bool IsAttacking => active && !special;
        public bool IsUsingSpecial => active && special;
        public bool IsBusy => active;
        public float NormalizedTime => active ? Mathf.Clamp01(elapsed / length) : 0;
        public float SpecialCooldownRemaining => specialCooldown;
        public HeroDefinition Profile => profile;
        public CombatEffectPool Pool => pool;
        public void Configure(PlayerMotor player, Health owner, Transform strike, CombatEffectPool effects, int targetMask, int wallMask, int allyMask)
        { motor = player; health = owner; origin = strike; pool = effects; targets = targetMask; geometry = wallMask; allies = allyMask; }
        public void ApplyHero(HeroDefinition hero) { profile = hero; equipmentAttacks = null; legendaryEcho = null; damageMultiplier = attackSpeed = cooldownMultiplier = 1; criticalChance = elementalDamage = 0; twinStrike = false; combo = 0; ResetCombat(); }
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
        bool CanStart => profile != null && health.IsAlive && !health.IsStunned && !active && !motor.IsDodging && !motor.IsCharging;
        public bool TryAttack()
        {
            var attacks = equipmentAttacks ?? (profile != null ? profile.attacks : null);
            if (!CanStart || attacks == null || attacks.Length == 0) return false;
            currentAttack = attacks[combo % attacks.Length]; combo++;
            Begin(false, (currentAttack.windup + currentAttack.recovery + (twinStrike ? .08f : 0)) / attackSpeed); return true;
        }
        public bool TrySpecial()
        {
            if (!CanStart || specialCooldown > 0 || profile.special == null) return false;
            currentAttack = profile.special.hit;
            float extra = profile.special.effect == AbilityEffect.ProtectionCharge ? profile.special.chargeDuration : 0;
            specialCooldown = profile.special.cooldown * cooldownMultiplier;
            Begin(true, currentAttack.windup + currentAttack.recovery + extra); return true;
        }
        void Begin(bool ability, float duration)
        {
            active = true; special = ability; elapsed = 0; length = Mathf.Max(0.05f, duration); resolved = chargeWindow = false;
            facing = transform.forward; facing.y = 0; facing.Normalize();
            strikeScale = damageMultiplier * (1 + elementalDamage) * (Random.value < criticalChance ? 1.5f : 1);
            secondResolved = echoResolved = false; motor.LockFacing(currentAttack.windup / (ability ? 1 : attackSpeed)); resolver.Clear();
        }
        public void Simulate(float deltaTime)
        {
            specialCooldown = Mathf.Max(0, specialCooldown - Mathf.Max(0, deltaTime));
            if (!health.IsAlive || health.IsStunned || motor.IsDodging) { active = false; chargeWindow = false; return; }
            if (!active) return;
            elapsed += Mathf.Max(0, deltaTime);
            float windup = currentAttack.windup / (special ? 1 : attackSpeed);
            if (!resolved && elapsed >= windup)
            { resolved = true; if (special) ResolveSpecial(); else ResolveBasic(); }
            if (!special && twinStrike && !secondResolved && elapsed >= windup + .08f / attackSpeed)
            { secondResolved = true; ResolveBasic(); }
            if (chargeWindow && elapsed <= currentAttack.windup + profile.special.chargeDuration)
                resolver.Area(origin.position + facing * 0.4f, 1.4f, facing, 180f, currentAttack, health, targets, geometry, false, strikeScale);
            if (elapsed >= length) { active = false; chargeWindow = false; }
        }
        void ResolveBasic()
        {
            if (currentAttack.shape == AttackShape.Projectile)
            {
                Vector3 start = origin.position + facing * 0.05f;
                if (!Physics.Linecast(origin.position, start, geometry))
                    for (int i = 0; i < Mathf.Clamp(currentAttack.projectileCount, 1, 5); i++)
                    { float angle = (i - (currentAttack.projectileCount - 1) * .5f) * currentAttack.projectileSpread; pool.Shoot(start, Quaternion.Euler(0, angle, 0) * facing, currentAttack, health, targets, geometry, strikeScale); }
            }
            else resolver.Area(origin.position, currentAttack.range, facing, currentAttack.arc, currentAttack, health, targets, geometry, true, strikeScale);
            if (legendaryEcho != null && combo % 3 == 0 && !echoResolved)
            { echoResolved = true; resolver.Area(origin.position, legendaryEcho.range, facing, 360, legendaryEcho, health, targets, geometry, true, strikeScale); }
        }
        void ResolveSpecial()
        {
            AbilityDefinition ability = profile.special;
            if (ability.effect == AbilityEffect.Trap)
            {
                Vector3 point = origin.position + facing * ability.placementDistance;
                if (Physics.Linecast(origin.position, point, geometry, QueryTriggerInteraction.Ignore) || Physics.CheckSphere(point, 0.2f, geometry, QueryTriggerInteraction.Ignore)) return;
                pool.PlaceZone(point, ability.radius, ability.trapLifetime, currentAttack, health, targets, geometry, true, strikeScale);
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
                resolver.Area(origin.position, ability.radius, facing, 360f, currentAttack, health, targets, geometry, true, strikeScale);
                pool.PlaceZone(transform.position, ability.radius, 0.6f, currentAttack, health, targets, geometry, false);
            }
        }
        public void ResetCombat() { active = special = resolved = chargeWindow = false; combo = 0; elapsed = specialCooldown = 0; resolver.Clear(); if (pool != null) pool.Clear(); }
    }
}
