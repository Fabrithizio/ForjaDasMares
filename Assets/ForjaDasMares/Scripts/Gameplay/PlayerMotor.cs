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
        CharacterController controller;
        IPlayerCommands commands;
        Vector3 velocity, dodgeDirection;
        float verticalSpeed, dodgeRemaining, cooldownRemaining;

        public float Speed => speed;
        public bool IsDodging => dodgeRemaining > 0f;
        public IPlayerCommands Commands => commands;
        public void Configure(MonoBehaviour source, Transform basis) { commandSource = source; cameraBasis = basis; }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            commands = commandSource as IPlayerCommands;
            if (commands == null || cameraBasis == null)
            { Debug.LogError("PlayerMotor requires an IPlayerCommands component and camera basis.", this); enabled = false; }
        }

        void Update()
        {
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
            if (cooldownRemaining > 0f || IsDodging) return false;
            dodgeDirection = direction.sqrMagnitude > 0.01f ? WorldDirection(direction).normalized : transform.forward;
            dodgeRemaining = dodgeDuration;
            cooldownRemaining = dodgeDuration + dodgeCooldown;
            return true;
        }

        // The same simulation entry point can later consume host-validated commands.
        public void Simulate(float deltaTime, Vector2 move)
        {
            if (deltaTime <= 0f || controller == null || !controller.enabled) return;
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - deltaTime);
            Vector3 desired = WorldDirection(Vector2.ClampMagnitude(move, 1f)) * speed;
            float rate = move.sqrMagnitude > 0.0001f ? acceleration : deceleration;
            velocity = Vector3.MoveTowards(velocity, desired, rate * deltaTime);
            float dashTime = Mathf.Min(deltaTime, dodgeRemaining);
            Vector3 displacement = dodgeDirection * (dodgeSpeed * dashTime) + velocity * (deltaTime - dashTime);
            dodgeRemaining = Mathf.Max(0f, dodgeRemaining - deltaTime);
            if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            verticalSpeed += gravity * deltaTime;
            displacement.y = verticalSpeed * deltaTime;
            controller.Move(displacement);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = wasEnabled;
            velocity = Vector3.zero;
            verticalSpeed = dodgeRemaining = cooldownRemaining = 0f;
            Physics.SyncTransforms();
        }
    }
}
