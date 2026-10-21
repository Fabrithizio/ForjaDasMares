using UnityEngine;

namespace ForjaDasMares
{
    [DefaultExecutionOrder(-100), RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("Explicit references")]
        [SerializeField] MonoBehaviour commandSource;
        [SerializeField] Transform cameraBasis;
        [Header("Movement (units/second)")]
        [SerializeField, Min(0.1f)] float speed = 6f;
        [SerializeField, Min(0.1f)] float acceleration = 40f;
        [SerializeField, Min(0.1f)] float deceleration = 50f;
        [SerializeField] float gravity = -25f;
        [Header("Dodge")]
        [SerializeField, Min(0.01f)] float dodgeDuration = 0.16f;
        [SerializeField, Min(0.1f)] float dodgeSpeed = 12f;
        [SerializeField, Min(0f)] float dodgeCooldown = 0.65f;
        [Header("Safe ground recovery")]
        [SerializeField] bool recoverFromFalls=true;
        [SerializeField] LayerMask walkableGround=1<<8;
        [SerializeField,Min(1)] float recoveryDepth=3;
        Vector3 safeGround;bool hasSafeGround;
        CharacterController controller;
        IPlayerCommands commands;
        Vector3 velocity, dodgeDirection, impulse;
        float verticalSpeed, dodgeRemaining, cooldownRemaining;
        Health health;
        float invulnerableStart, invulnerableEnd, facingLock;
        bool charge,perfectUsed;
        public bool ConfirmPerfectDodge(){if(!IsInvulnerable||perfectUsed||dodgeDuration-dodgeRemaining>invulnerableStart+.065f)return false;perfectUsed=true;var build=GetComponent<CombatBuildEffects>();if(build!=null)build.GrantCounter();var feel=GetComponent<HeroRoster>()?.Current.feedback;CombatAudioPool.Play(feel!=null?feel.perfect:null,.3f);return true;}
        float baseDodgeCooldown;

        public int FallRecoveries { get; private set; }
        public float Speed => speed;
        public bool IsDodging => dodgeRemaining > 0f && !charge;
        public float DodgeProgress => IsDodging ? Mathf.Clamp01(1-dodgeRemaining/Mathf.Max(.05f,dodgeDuration)) : 0;
        public bool IsCharging => dodgeRemaining > 0f && charge;
        public bool IsInvulnerable => IsDodging && dodgeDuration - dodgeRemaining >= invulnerableStart && dodgeDuration - dodgeRemaining < invulnerableEnd;
        public bool FacingLocked => facingLock > 0f || IsCharging;
        public Vector3 PlanarVelocity => velocity;
        public float DodgeCooldownRemaining => cooldownRemaining;
        public void ApplyHero(HeroDefinition profile, Health owner)
        {
            if (health != null) health.Impact -= ReceiveImpact;
            health = owner; speed = Mathf.Max(0.1f, profile.speed); acceleration = Mathf.Max(1, profile.acceleration); deceleration = Mathf.Max(1, profile.deceleration);
            health.Impact += ReceiveImpact;
            dodgeDuration = Mathf.Max(0.05f, profile.dodge.duration); dodgeSpeed = Mathf.Max(0.1f, profile.dodge.speed); dodgeCooldown = Mathf.Max(0, profile.dodge.cooldown);
            baseDodgeCooldown = dodgeCooldown;
            invulnerableStart = Mathf.Clamp(profile.dodge.invulnerableStart, 0, dodgeDuration);
            invulnerableEnd = Mathf.Clamp(profile.dodge.invulnerableEnd, invulnerableStart, dodgeDuration);
            velocity = impulse = Vector3.zero; dodgeRemaining = cooldownRemaining = facingLock = 0f; charge = false;
        }
        void ReceiveImpact(Vector3 direction, float force) { direction.y = 0; impulse = Vector3.ClampMagnitude(impulse + direction.normalized * force, 12); }
        void OnDestroy() { if (health != null) health.Impact -= ReceiveImpact; }
        public void LockFacing(float duration) => facingLock = Mathf.Max(facingLock, duration);
        public void ApplyMovementAttributes(float unitsPerSecond, float cooldownMultiplier)
        { speed = Mathf.Max(.1f, unitsPerSecond); if (health != null) dodgeCooldown = baseDodgeCooldown * Mathf.Clamp(cooldownMultiplier, .65f, 1); }
        public bool StartCharge(Vector3 direction, float duration, float chargeSpeed)
        {
            if ((health != null && (!health.IsAlive || health.ControlMultiplier == 0f)) || dodgeRemaining > 0) return false;
            direction.y = 0; dodgeDirection = direction.normalized * chargeSpeed / dodgeSpeed;
            dodgeRemaining = Mathf.Max(0.05f, duration); charge = true; return true;
        }
        public IPlayerCommands Commands => commands;
        public void Configure(MonoBehaviour source, Transform basis) { commandSource = source; cameraBasis = basis; commands = source as IPlayerCommands; }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            commands = commandSource as IPlayerCommands;
            if (commands == null || cameraBasis == null)
            { Debug.LogError("PlayerMotor requires an IPlayerCommands component and camera basis.", this); enabled = false; }
        }

        void Update()
        {
            if (health != null && !health.IsAlive) { commands.TakeDodge(); Simulate(Time.deltaTime, Vector2.zero); return; }
            if (commands.TakeDodge()) TryDodge(commands.Move);
            Simulate(Time.deltaTime, commands.Move);
        }

        public Vector3 WorldDirection(Vector2 move)
        {
            Vector3 forward = cameraBasis.forward; forward.y = 0f; forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            return Vector3.ClampMagnitude(right * move.x + forward * move.y, 1f);
        }

        public bool TryDodge(Vector2 direction)
        {
            if (cooldownRemaining > 0f || dodgeRemaining > 0f || (health != null && (!health.IsAlive || health.ControlMultiplier == 0f))) return false;
            dodgeDirection = direction.sqrMagnitude > 0.01f ? WorldDirection(direction).normalized : transform.forward;
            dodgeRemaining = dodgeDuration;perfectUsed=false;
            charge = false;
            cooldownRemaining = dodgeDuration + dodgeCooldown;
            return true;
        }

        // The same simulation entry point can later consume host-validated commands.
        public void Simulate(float deltaTime, Vector2 move)
        {
            if (deltaTime <= 0f || controller == null || !controller.enabled) return;
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - deltaTime);
            facingLock = Mathf.Max(0, facingLock - deltaTime);
            if (health != null && !health.IsAlive) { move = Vector2.zero; velocity = Vector3.zero; dodgeRemaining = 0; }
            Vector3 desired = WorldDirection(Vector2.ClampMagnitude(move, 1f)) * speed * (health != null ? health.ControlMultiplier : 1f);
            float rate = move.sqrMagnitude > 0.0001f ? acceleration : deceleration;
            velocity = Vector3.MoveTowards(velocity, desired, rate * deltaTime);
            float dashTime = Mathf.Min(deltaTime, dodgeRemaining);
            Vector3 displacement = dodgeDirection * (dodgeSpeed * dashTime) + velocity * (deltaTime - dashTime);
            dodgeRemaining = Mathf.Max(0f, dodgeRemaining - deltaTime);
            displacement += impulse * deltaTime; impulse = Vector3.MoveTowards(impulse, Vector3.zero, 20 * deltaTime);
            ActorMotion.Move(controller, displacement, deltaTime, ref verticalSpeed, gravity);
            if(controller.isGrounded)RememberSafeGround();
            else if(recoverFromFalls&&hasSafeGround&&transform.position.y<safeGround.y-recoveryDepth)
            {
                FallRecoveries++;float remainingCooldown=cooldownRemaining;
                Teleport(safeGround,transform.rotation);cooldownRemaining=remainingCooldown;
            }
        }

        void RememberSafeGround()
        {
            if(!recoverFromFalls)return;
            if(hasSafeGround&&(transform.position-safeGround).sqrMagnitude<.09f)return;
            if(Physics.Raycast(transform.position+Vector3.up*.35f,Vector3.down,out var ground,.8f,walkableGround,QueryTriggerInteraction.Ignore))
            {safeGround=ground.point+Vector3.up*.06f;hasSafeGround=true;}
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = wasEnabled;
            velocity = impulse = Vector3.zero;
            verticalSpeed = dodgeRemaining = cooldownRemaining = 0f;
            facingLock = 0f; charge = false;
            Physics.SyncTransforms();RememberSafeGround();
        }
    }
}
