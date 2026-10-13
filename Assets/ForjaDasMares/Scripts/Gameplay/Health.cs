using System;
using UnityEngine;

namespace ForjaDasMares
{
    public enum CombatFaction { Training, Player, Enemy }
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(1)] int maximum = 100;
        int current;
        [SerializeField] CombatFaction faction;
        [SerializeField] PlayerMotor dodgeSource;
        float armor, protection, protectionTime, controlTime, slow = 1f;
        bool rooted;
        public int Current => current;
        public int Maximum => maximum;
        public bool IsAlive => current > 0;
        public float ControlMultiplier => controlTime <= 0f ? 1f : rooted ? 0f : slow;
        public float ProtectionRemaining => protectionTime;
        public CombatFaction Faction => faction;
        public event Action Changed;
        public event Action Damaged;
        public event Action<Vector3, float> Impact;
        void Awake() => Restore();
        void Update()
        {
            if (protectionTime > 0f) { protectionTime = Mathf.Max(0, protectionTime - Time.deltaTime); if (protectionTime == 0) Changed?.Invoke(); }
            if (controlTime > 0f) { controlTime = Mathf.Max(0, controlTime - Time.deltaTime); if (controlTime == 0) Changed?.Invoke(); }
        }
        public void Configure(int hitPoints, CombatFaction team, float reduction = 0f, PlayerMotor dodge = null)
        { maximum = Mathf.Max(1, hitPoints); faction = team; armor = Mathf.Clamp(reduction, 0, 0.75f); dodgeSource = dodge; Restore(); }
        public bool CanReceiveFrom(Health source) => IsAlive && source != this && (source == null || faction != CombatFaction.Player || source.faction != CombatFaction.Player);
        public void Protect(float reduction, float duration)
        { protection = Mathf.Clamp(reduction, 0, 0.8f); protectionTime = Mathf.Max(0, duration); Changed?.Invoke(); }
        public bool ApplyHit(AttackDefinition attack, Vector3 direction, Health source)
        {
            if (!CanReceiveFrom(source) || !ApplyDamage(attack.damage)) return false;
            if (attack.control != ControlEffect.None && attack.controlDuration > 0f)
            { rooted = attack.control == ControlEffect.Root; slow = Mathf.Clamp(attack.slowMultiplier, 0.05f, 1f); controlTime = attack.controlDuration; Changed?.Invoke(); }
            if (attack.knockback > 0) Impact?.Invoke(direction, attack.knockback);
            return true;
        }
        public bool ApplyDamage(int damage)
        {
            if (damage <= 0 || current <= 0 || (dodgeSource != null && dodgeSource.IsInvulnerable)) return false;
            int finalDamage = Mathf.Max(1, Mathf.CeilToInt(damage * (1f - armor) * (1f - (protectionTime > 0 ? protection : 0f))));
            current = Mathf.Max(0, current - finalDamage);
            Damaged?.Invoke();
            Changed?.Invoke();
            return true;
        }
        public void Restore() { current = maximum; protectionTime = controlTime = 0f; Changed?.Invoke(); }
    }
}
