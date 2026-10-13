using System;
using UnityEngine;

namespace ForjaDasMares
{
    public sealed class MeleeAttack : MonoBehaviour
    {
        [SerializeField] PlayerMotor motor;
        [SerializeField] MeleeDefinition definition;
        [SerializeField] Transform strikeOrigin;
        [SerializeField] LayerMask targets;
        [SerializeField] LayerMask occluders;
        readonly Collider[] contacts = new Collider[32];
        readonly Health[] hitTargets = new Health[32];
        float elapsed;
        bool attacking, resolved;
        public bool IsAttacking => attacking;
        public float NormalizedTime => attacking ? Mathf.Clamp01(elapsed / (definition.windup + definition.recovery)) : 0f;
        public event Action AttackStarted;
        public void Configure(PlayerMotor playerMotor, MeleeDefinition data, Transform origin, LayerMask targetMask, LayerMask blockerMask)
        { motor = playerMotor; definition = data; strikeOrigin = origin; targets = targetMask; occluders = blockerMask; }
        void Awake()
        {
            if (motor == null || definition == null || strikeOrigin == null)
            { Debug.LogError("MeleeAttack requires motor, melee definition and strike origin.", this); enabled = false; }
        }
        void Update()
        {
            if (motor.IsDodging) { motor.Commands.TakeAttack(); ResetAttack(); return; }
            if (motor.Commands.TakeAttack()) TryBeginAttack();
            Simulate(Time.deltaTime);
        }
        public bool TryBeginAttack()
        {
            if (attacking || motor.IsDodging) return false;
            attacking = true; resolved = false; elapsed = 0f;
            AttackStarted?.Invoke();
            return true;
        }
        public void Simulate(float deltaTime)
        {
            if (!attacking) return;
            elapsed += Mathf.Max(0f, deltaTime);
            if (!resolved && elapsed >= definition.windup) { resolved = true; Strike(); }
            if (elapsed >= definition.windup + definition.recovery) attacking = false;
        }
        void Strike()
        {
            int count = Physics.OverlapSphereNonAlloc(strikeOrigin.position, definition.range, contacts, targets, QueryTriggerInteraction.Ignore);
            int hitCount = 0;
            float minDot = Mathf.Cos(definition.arcDegrees * 0.5f * Mathf.Deg2Rad);
            for (int i = 0; i < count; i++)
            {
                Health health = contacts[i].GetComponentInParent<Health>();
                if (health == null || health.Current <= 0) continue;
                bool alreadyHit = false;
                for (int j = 0; j < hitCount; j++) if (hitTargets[j] == health) { alreadyHit = true; break; }
                if (alreadyHit) continue;
                Vector3 point = contacts[i].bounds.center;
                Vector3 direction = point - strikeOrigin.position; direction.y = 0f;
                if (direction.sqrMagnitude > definition.range * definition.range || (direction.sqrMagnitude > 0.001f && Vector3.Dot(transform.forward, direction.normalized) < minDot)) continue;
                if (Physics.Linecast(strikeOrigin.position, point, occluders, QueryTriggerInteraction.Ignore)) continue;
                // Only this local resolver changes target health. Later run it on the host.
                if (health.ApplyDamage(definition.damage)) hitTargets[hitCount++] = health;
            }
            Array.Clear(contacts, 0, count);
            Array.Clear(hitTargets, 0, hitCount);
        }
        public void ResetAttack() { attacking = resolved = false; elapsed = 0f; }
        void OnDrawGizmosSelected()
        {
            if (definition == null || strikeOrigin == null) return;
            Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.6f);
            Gizmos.DrawWireSphere(strikeOrigin.position, definition.range);
        }
    }
}
