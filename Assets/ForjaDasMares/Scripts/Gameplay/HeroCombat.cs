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
        [SerializeField] CombatFlowDefinition flow;
        float heldTime, heavyCooldown, superBuffer;
        int superHits;
        bool heavy, ultimate;
        public bool EnhancedControls => flow != null;
        public float ChargeProgress => flow == null ? 0 : Mathf.Clamp01(heldTime / Mathf.Max(.2f, flow.holdDuration));
        public float SuperProgress => flow == null ? 0 : Mathf.Clamp01((float)superHits / Mathf.Max(1,flow.hitsForSuper));
        public bool SuperReady => flow != null && SuperProgress >= 1;
        public bool SuperQueued => superBuffer > 0;
        public bool IsHeavy => active && heavy;
        public bool IsSuper => active && ultimate;
        public int AnimationVariant { get; private set; }
        public int ActionVersion { get; private set; }
        public float ImpactTime => active ? (heavy ? Mathf.Max(.05f,flow.heavyWindup) : currentAttack.windup / (special ? 1 : attackSpeed)) / length : .25f;
        public float EffectRange => currentAttack != null ? currentAttack.range * (heavy && flow != null ? Mathf.Max(1,flow.heavyRangeMultiplier) : 1) : 2;
        public void ConfigureFlow(CombatFlowDefinition settings) { flow = settings; superBuffer = heldTime = heavyCooldown = 0; superHits = 0; }
        void OnEnable() { if(health != null) { health.DealtHit -= ConfirmedHit; health.DealtHit += ConfirmedHit; } }
        void OnDisable() { if(health != null) health.DealtHit -= ConfirmedHit; heldTime = superBuffer = 0; }
        void ConfirmedHit(Health target)
        { if(flow != null && !ultimate && health.IsAlive && target.Faction == CombatFaction.Enemy) superHits = Mathf.Min(Mathf.Max(1,flow.hitsForSuper),superHits + 1); }
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
        public void AddTargets(int mask)=>targets|=mask;
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
                bool pressed = input.TakeAttack();
                var continuous = motor.Commands as IContinuousCombatCommands;
                if(continuous != null && continuous.CombatBlocked) heldTime = superBuffer = 0;
                else ProcessAttackInput(continuous != null && continuous.AttackHeld, pressed, continuous != null && continuous.TakeSuper(), Time.deltaTime);
            }
            Simulate(Time.deltaTime);
        }
        public void ProcessAttackInput(bool held, bool pressed, bool superRequested, float deltaTime)
        {
            if (!health.IsAlive || health.IsStunned || motor.IsDodging) { heldTime = superBuffer = 0; return; }
            if(flow != null && held && heavyCooldown <= 0) heldTime = Mathf.Min(flow.holdDuration, heldTime + Mathf.Max(0,deltaTime));
            else if(!held) heldTime = 0;
            if(flow != null && superRequested && SuperReady)superBuffer = Mathf.Max(.1f,flow.superInputBuffer);
            if(superBuffer > 0 && TrySuper()){superBuffer = 0;return;}
            if(flow != null && held && ChargeProgress >= 1 && TryHeavy()) return;
            if(pressed || held) TryAttack();
        }
        bool CanStart => profile != null && health.IsAlive && !health.IsStunned && !active && !motor.IsDodging && !motor.IsCharging;
        public bool TryAttack()
        {
            var attacks = equipmentAttacks ?? (profile != null ? profile.attacks : null);
            if (!CanStart || attacks == null || attacks.Length == 0) return false;
            currentAttack = attacks[combo % attacks.Length]; combo++;
            Begin(false, (currentAttack.windup + currentAttack.recovery + (twinStrike ? .08f : 0)) / attackSpeed); AnimationVariant = (combo-1)%3; return true;
        }
        public bool TryHeavy()
        {
            if(flow == null || heavyCooldown > 0 || !CanStart || BasicAttack == null) return false;
            currentAttack = BasicAttack; Begin(false, Mathf.Max(flow.heavyWindup+.1f,flow.heavyDuration)); heavy = true; AnimationVariant = 3;
            strikeScale *= Mathf.Max(1,flow.heavyMultiplier); heldTime = 0; heavyCooldown = Mathf.Max(.1f,flow.heavyCooldown); motor.LockFacing(flow.heavyWindup); return true;
        }
        public bool TrySuper()
        {
            if(!CanStart || !SuperReady || flow.superAttack == null) return false;
            currentAttack = flow.superAttack; Begin(true, currentAttack.windup + currentAttack.recovery); ultimate = true; AnimationVariant = 4;
            superHits = 0; heldTime = superBuffer = 0; motor.LockFacing(currentAttack.windup); return true;
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
            heavy = ultimate = false; AnimationVariant = 0; ActionVersion++;
            facing = transform.forward; facing.y = 0; facing.Normalize();
            strikeScale = damageMultiplier * (1 + elementalDamage) * (Random.value < criticalChance ? 1.5f : 1);
            secondResolved = echoResolved = false; motor.LockFacing(currentAttack.windup / (ability ? 1 : attackSpeed)); resolver.Clear();
        }
        public void Simulate(float deltaTime)
        {
            specialCooldown = Mathf.Max(0, specialCooldown - Mathf.Max(0, deltaTime));
            heavyCooldown = Mathf.Max(0, heavyCooldown - Mathf.Max(0,deltaTime));
            superBuffer = Mathf.Max(0,superBuffer - Mathf.Max(0,deltaTime));
            if (!health.IsAlive || health.IsStunned || motor.IsDodging) { active = false; chargeWindow = false; return; }
            if (!active) return;
            elapsed += Mathf.Max(0, deltaTime);
            float windup = heavy ? Mathf.Max(.05f,flow.heavyWindup) : currentAttack.windup / (special ? 1 : attackSpeed);
            if (!resolved && elapsed >= windup)
            { resolved = true; if (ultimate) ResolveSuper(); else if (special) ResolveSpecial(); else ResolveBasic(); }
            if (!special && !heavy && twinStrike && !secondResolved && elapsed >= windup + .08f / attackSpeed)
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
            else resolver.Area(origin.position, currentAttack.range * (heavy ? Mathf.Max(1,flow.heavyRangeMultiplier) : 1), facing, heavy ? Mathf.Max(160,currentAttack.arc) : currentAttack.arc, currentAttack, health, targets, geometry, true, strikeScale);
            if(heavy) pool.PlaceZone(transform.position + facing, 1.5f,.25f,currentAttack,health,targets,geometry,false);
            if (legendaryEcho != null && combo % 3 == 0 && !echoResolved)
            { echoResolved = true; resolver.Area(origin.position, legendaryEcho.range, facing, 360, legendaryEcho, health, targets, geometry, true, strikeScale); }
        }
        void ResolveSuper()
        {
            resolver.Area(origin.position, currentAttack.range, facing,360,currentAttack,health,targets,geometry,true,strikeScale);
            pool.PlaceZone(transform.position,currentAttack.range,.45f,currentAttack,health,targets,geometry,false);
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
        public void ResetCombat() { active = special = resolved = chargeWindow = heavy = ultimate = false; combo = superHits = 0; superBuffer = heldTime = heavyCooldown = elapsed = specialCooldown = 0; resolver.Clear(); if (pool != null) pool.ClearOwned(health); }
    }
}
