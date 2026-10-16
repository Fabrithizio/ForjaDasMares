using UnityEngine;
namespace ForjaDasMares
{
    public enum HeroState { Idle, Walk, Attack, Skill, Dodge, Hit, Dead, Victory }
    public sealed class HeroPresentation : MonoBehaviour
    {
        [SerializeField] Transform[] models;
        [SerializeField] PlayerMotor motor;
        [SerializeField] HeroCombat combat;
        [SerializeField] Health health;
        [SerializeField] Animator animator;
        Transform visible;
        HeroModelView visibleView;
        float hitTime, victoryTime;
        public HeroState State { get; private set; }
        public Transform Visible => visible;
        public bool ReplicaMode { get; set; }
        float replicaAnimation;
        public void ApplyReplica(HeroState value, float time) { State = value; replicaAnimation = Mathf.Clamp01(time); }
        public void Configure(Transform[] roots, PlayerMotor movement, HeroCombat attacks, Health hp)
        { models = roots; motor = movement; combat = attacks; health = hp; }
        void OnEnable() { if (health != null) health.Damaged += Hit; }
        void OnDisable() { if (health != null) health.Damaged -= Hit; }
        void Hit() { hitTime = 0.2f; victoryTime = 0; }
        public void Show(int slot)
        {
            for (int i = 0; i < models.Length; i++) models[i].gameObject.SetActive(i == slot);
            visible = models[Mathf.Clamp(slot, 0, models.Length - 1)]; visibleView = visible.GetComponentInChildren<HeroModelView>(true); ResetPose();
        }
        public void ShowVictory() { if (health.IsAlive) victoryTime = 2; }
        public void ResetPose() { hitTime = victoryTime = 0; if (visible != null) { visible.localPosition = Vector3.zero; visible.localRotation = Quaternion.identity; } }
        void LateUpdate()
        {
            if (visible == null) return;
            hitTime = Mathf.Max(0, hitTime - Time.deltaTime); victoryTime = Mathf.Max(0, victoryTime - Time.deltaTime);
            if (!ReplicaMode) State = !health.IsAlive ? HeroState.Dead : hitTime > 0 ? HeroState.Hit : motor.IsDodging ? HeroState.Dodge : combat.IsUsingSpecial ? HeroState.Skill : combat.IsAttacking ? HeroState.Attack : victoryTime > 0 ? HeroState.Victory : motor.PlanarVelocity.sqrMagnitude > 0.1f ? HeroState.Walk : HeroState.Idle;
            if (visibleView != null)
            {
                visible.localPosition = Vector3.zero; visible.localRotation = Quaternion.identity;
                visibleView.Animate(State, ReplicaMode ? replicaAnimation : combat.NormalizedTime, motor.PlanarVelocity.magnitude, health.IsAlive);
                return;
            }
            float bob = State == HeroState.Walk ? Mathf.Sin(Time.time * 16) * 0.05f : State == HeroState.Victory ? Mathf.Abs(Mathf.Sin(Time.time * 8)) * 0.25f : 0;
            float lean = State == HeroState.Dodge ? 35 : State == HeroState.Hit ? -18 : State == HeroState.Dead ? 85 : 0;
            float swing = State == HeroState.Attack || State == HeroState.Skill ? Mathf.Sin((ReplicaMode ? replicaAnimation : combat.NormalizedTime) * Mathf.PI * 2) * 45 : 0;
            visible.localPosition = Vector3.up * bob; visible.localRotation = Quaternion.Euler(lean, swing, 0);
            if (animator != null && animator.runtimeAnimatorController != null)
            { animator.SetFloat("Speed", motor.PlanarVelocity.magnitude); animator.SetInteger("State", (int)State); animator.SetBool("Alive", health.IsAlive); }
        }
    }
}
