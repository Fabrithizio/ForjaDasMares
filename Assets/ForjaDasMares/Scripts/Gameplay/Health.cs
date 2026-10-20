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
        [SerializeField] MonoBehaviour defenseSource;
        IHitDefense defense;
        float armor, protection, protectionTime, controlTime, stunTime, slow = 1f;
        bool rooted;
        public int Current => current;
        public int Maximum => maximum;
        public bool IsAlive => current > 0;
        public float ControlMultiplier => stunTime > 0 ? 0 : controlTime <= 0f ? 1f : rooted ? 0f : slow;
        public bool IsStunned => stunTime > 0;
        public float ProtectionRemaining => protectionTime;
        public CombatFaction Faction => faction;
        public event Action Changed;
        public event Action Damaged;
        public event Action<Vector3, float> Impact;
        public event Action<Health> DealtHit;
        void Awake() { defense = defenseSource as IHitDefense; Restore(); }
        void Update()
        {
            if (protectionTime > 0f) { protectionTime = Mathf.Max(0, protectionTime - Time.deltaTime); if (protectionTime == 0) Changed?.Invoke(); }
            if (controlTime > 0f) { controlTime = Mathf.Max(0, controlTime - Time.deltaTime); if (controlTime == 0) Changed?.Invoke(); }
            if (stunTime > 0) { stunTime = Mathf.Max(0, stunTime - Time.deltaTime); if (stunTime == 0) Changed?.Invoke(); }
        }
        public void Configure(int hitPoints, CombatFaction team, float reduction = 0f, PlayerMotor dodge = null)
        { maximum = Mathf.Max(1, hitPoints); faction = team; armor = Mathf.Clamp(reduction, 0, 0.75f); dodgeSource = dodge; Restore(); }
        public bool CanReceiveFrom(Health source) => IsAlive && source != this && (source == null || source.faction != faction);
        public void ConfigureDefense(MonoBehaviour modifier) { defenseSource = modifier; defense = modifier as IHitDefense; }
        public void Stun(float duration) { stunTime = Mathf.Max(stunTime, Mathf.Max(0, duration)); Changed?.Invoke(); }
        public void Protect(float reduction, float duration)
        { protection = Mathf.Clamp(reduction, 0, 0.8f); protectionTime = Mathf.Max(0, duration); Changed?.Invoke(); }
        public void Push(Vector3 direction,float force){if(IsAlive)Impact?.Invoke(direction,Mathf.Clamp(force,0,5));}
        public bool ApplyHit(AttackDefinition attack, Vector3 direction, Health source, float damageScale = 1)
        {
            float build=source!=null?source.GetComponent<CombatBuildEffects>()?.Against(this)??1:1;
            float multiplier = defense != null ? Mathf.Clamp01(defense.DamageMultiplier(direction, source, attack)) : 1;
            if (!CanReceiveFrom(source) || multiplier <= 0 || !ApplyDamage(Mathf.Max(1, Mathf.RoundToInt(attack.damage * Mathf.Max(0, damageScale) * multiplier * build)))) return false;
            if (attack.control != ControlEffect.None && attack.controlDuration > 0f)
            { rooted = attack.control == ControlEffect.Root; slow = Mathf.Clamp(attack.slowMultiplier, 0.05f, 1f); controlTime = attack.controlDuration; Changed?.Invoke(); }
            if (attack.knockback > 0) Impact?.Invoke(direction, attack.knockback);
            if (source != null) source.DealtHit?.Invoke(this);
            return true;
        }
        public bool ApplyDamage(int damage)
        {
            if(damage<=0||current<=0)return false;if(dodgeSource!=null&&dodgeSource.IsInvulnerable){dodgeSource.ConfirmPerfectDodge();return false;}
            int finalDamage = ProgressionStats.MitigatedDamage(damage,armor*(GetComponent<CombatAilments>()?.DefenseScale??1),Mathf.Max(protectionTime>0?protection:0,GetComponent<CombatBuildEffects>()?.Shelter??0));
            current = Mathf.Max(0, current - finalDamage);
            Damaged?.Invoke();
            Changed?.Invoke();
            return true;
        }
        public void Restore() { GetComponent<CombatAilments>()?.Clear();current = maximum; protectionTime = controlTime = stunTime = 0f; Changed?.Invoke(); }
        public void ApplyAttributes(int hitPoints, float reduction)
        { maximum = Mathf.Max(1, hitPoints); armor = Mathf.Clamp(reduction, 0, .75f); current = Mathf.Min(current, maximum); Changed?.Invoke(); }
        public bool Heal(int amount)
        { if (!IsAlive || amount <= 0 || current >= maximum) return false; current = Mathf.Min(maximum, current + amount); Changed?.Invoke(); return true; }
        public bool Revive(float fraction)
        {
            if (IsAlive || float.IsNaN(fraction) || float.IsInfinity(fraction) || fraction <= 0) return false;
            current = Mathf.Max(1, Mathf.RoundToInt(maximum * Mathf.Clamp01(fraction)));
            protectionTime = controlTime = stunTime = 0; Changed?.Invoke(); return true;
        }
        // Presentation-only replicas call this after an authenticated host snapshot.
        public void SetReplicaState(int hitPoints, int capacity)
        { int previous = current; maximum = Mathf.Clamp(capacity, 1, 100000); current = Mathf.Clamp(hitPoints, 0, maximum); if (previous > current) Damaged?.Invoke(); Changed?.Invoke(); }
    }
}
