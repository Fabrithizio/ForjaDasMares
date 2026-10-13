using UnityEngine;

namespace ForjaDasMares
{
    [RequireComponent(typeof(Camera))]
    public sealed class ArenaCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(9f, 13f, -9f);
        [SerializeField] Vector3 lookOffset = new Vector3(0f, 0.9f, 0f);
        [SerializeField, Min(0.01f)] float followTime = 0.12f;
        [SerializeField, Min(0.1f)] float collisionRadius = 0.2f;
        [SerializeField, Min(0f)] float collisionPadding = 0.1f;
        [SerializeField, Min(1f)] float minimumFramingDistance = 5f;
        [SerializeField] LayerMask blockers;
        [SerializeField, Range(30f, 80f)] float verticalFieldOfView = 48f;
        [SerializeField, Min(0.5f)] float referenceAspect = 1.777778f;
        Vector3 smoothedFocus, smoothVelocity;
        Camera cameraComponent;
        public Transform Target => target;
        public void Configure(Transform followTarget, LayerMask mask) { target = followTarget; blockers = mask; }
        void Awake()
        {
            cameraComponent = GetComponent<Camera>();
            if (target == null) { Debug.LogError("ArenaCamera requires a target.", this); enabled = false; }
        }
        void Start() => Snap();
        public void Snap()
        {
            if (target == null) return;
            if (cameraComponent == null) cameraComponent = GetComponent<Camera>();
            smoothedFocus = target.position + lookOffset;
            smoothVelocity = Vector3.zero;
            ApplyPose();
        }
        void LateUpdate()
        {
            smoothedFocus = Vector3.SmoothDamp(smoothedFocus, target.position + lookOffset, ref smoothVelocity, followTime, Mathf.Infinity, Time.deltaTime);
            if (Physics.CheckSphere(smoothedFocus, collisionRadius, blockers, QueryTriggerInteraction.Ignore))
            { smoothedFocus = target.position + lookOffset; smoothVelocity = Vector3.zero; }
            ApplyPose();
        }
        void ApplyPose()
        {
            // Preserve horizontal framing on narrow screens instead of cropping the arena.
            float aspect = Mathf.Max(0.1f, cameraComponent.aspect);
            float tangent = Mathf.Tan(verticalFieldOfView * Mathf.Deg2Rad * 0.5f);
            cameraComponent.fieldOfView = Mathf.Clamp(2f * Mathf.Atan(tangent * Mathf.Max(1f, referenceAspect / aspect)) * Mathf.Rad2Deg, 30f, 95f);
            Vector3 direction = offset.normalized;
            float armLength = offset.magnitude;
            float distance = ClearDistance(direction, armLength);
            // In this open arena, raise the view above low walls before zooming into the hero.
            // Horizontal yaw is preserved, so movement remains relative to the same screen axes.
            float factor = 1f;
            for (int i = 0; i < 4 && distance < minimumFramingDistance; i++)
            {
                factor *= 0.4f;
                Vector3 candidate = new Vector3(offset.x * factor, offset.y, offset.z * factor).normalized;
                float candidateDistance = ClearDistance(candidate, armLength);
                if (candidateDistance > distance) { direction = candidate; distance = candidateDistance; }
            }
            // Resolve obstruction immediately; only smooth the focus, never smooth through a wall.
            transform.position = smoothedFocus + direction * distance;
            transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);
        }
        float ClearDistance(Vector3 direction, float armLength)
        {
            if (Physics.SphereCast(smoothedFocus, collisionRadius, direction, out RaycastHit hit, armLength, blockers, QueryTriggerInteraction.Ignore))
                return Mathf.Max(0.01f, hit.distance - collisionPadding);
            return armLength;
        }
    }
}
