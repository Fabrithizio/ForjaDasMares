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
        [Header("Obstruction response")]
        [SerializeField, Range(50f, 80f)] float maximumElevation = 75f;
        [SerializeField, Min(1f)] float elevationSpeed = 22f;
        [SerializeField, Min(1f)] float returnSpeed = 12f;
        [SerializeField, Min(0f)] float clearDelay = 0.35f;
        [SerializeField, Min(0.01f)] float zoomOutTime = 0.28f;
        [SerializeField] LayerMask blockers;
        [SerializeField, Range(30f, 80f)] float verticalFieldOfView = 48f;
        [SerializeField, Min(0.5f)] float referenceAspect = 1.777778f;
        Vector3 smoothedFocus, smoothVelocity;
        Camera cameraComponent;
        bool boundFocus;
        float elevation, armDistance, armVelocity, clearTime;
        Bounds focusBounds;
        public void SetFocusBounds(Bounds bounds, bool enabled) { focusBounds = bounds; boundFocus = enabled; }
        Vector3 Focus() => boundFocus ? focusBounds.ClosestPoint(target.position + lookOffset) : target.position + lookOffset;
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
            smoothedFocus = Focus();
            smoothVelocity = Vector3.zero;
            ApplyPose(0f, true);
        }
        void LateUpdate()
        {
            smoothedFocus = Vector3.SmoothDamp(smoothedFocus, Focus(), ref smoothVelocity, followTime, Mathf.Infinity, Time.deltaTime);
            if (Physics.CheckSphere(smoothedFocus, collisionRadius, blockers, QueryTriggerInteraction.Ignore))
            { smoothedFocus = target.position + lookOffset; smoothVelocity = Vector3.zero; }
            ApplyPose(Time.deltaTime, false);
        }
        void ApplyPose(float deltaTime, bool snap)
        {
            // Preserve horizontal framing on narrow screens instead of cropping the arena.
            float aspect = Mathf.Max(0.1f, cameraComponent.aspect);
            float tangent = Mathf.Tan(verticalFieldOfView * Mathf.Deg2Rad * 0.5f);
            cameraComponent.fieldOfView = Mathf.Clamp(2f * Mathf.Atan(tangent * Mathf.Max(1f, referenceAspect / aspect)) * Mathf.Rad2Deg, 30f, 95f);
            Vector3 horizontal = new Vector3(offset.x, 0f, offset.z).normalized;
            if (horizontal.sqrMagnitude < 0.001f) horizontal = Vector3.back;
            float armLength = Mathf.Max(0.1f, offset.magnitude);
            float baseElevation = Mathf.Atan2(Mathf.Max(0.01f, offset.y), new Vector2(offset.x, offset.z).magnitude) * Mathf.Rad2Deg;
            float ceiling = Mathf.Max(baseElevation, maximumElevation);
            float desiredElevation = baseElevation;
            float bestDistance = ClearDistance(Direction(horizontal, baseElevation), armLength);
            // Search small increments rather than switching between near-vertical presets.
            for (float candidate = Mathf.Min(baseElevation + 5f, ceiling); bestDistance < minimumFramingDistance && candidate > desiredElevation; candidate = Mathf.Min(candidate + 5f, ceiling))
            {
                float distance = ClearDistance(Direction(horizontal, candidate), armLength);
                if (distance > bestDistance) { desiredElevation = candidate; bestDistance = distance; }
                if (candidate >= ceiling) break;
            }
            if (snap) { elevation = desiredElevation; clearTime = 0f; armVelocity = 0f; }
            else
            {
                if (desiredElevation < elevation)
                {
                    clearTime += deltaTime;
                    if (clearTime < clearDelay) desiredElevation = elevation;
                }
                else clearTime = 0f;
                elevation = Mathf.MoveTowards(elevation, desiredElevation, (desiredElevation > elevation ? elevationSpeed : returnSpeed) * deltaTime);
            }
            Vector3 direction = Direction(horizontal, elevation);
            float safeDistance = ClearDistance(direction, armLength);
            // Collision safety takes priority while retracting; extend smoothly after clearance.
            // Never interpolate the camera through geometry to smooth an obstruction.
            if (snap || safeDistance < armDistance) { armDistance = safeDistance; armVelocity = 0f; }
            else if (deltaTime > 0f) armDistance = Mathf.Min(safeDistance, Mathf.SmoothDamp(armDistance, safeDistance, ref armVelocity, zoomOutTime, Mathf.Infinity, deltaTime));
            transform.position = smoothedFocus + direction * armDistance;
            transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);
        }
        static Vector3 Direction(Vector3 horizontal, float pitch)
        {
            float radians = pitch * Mathf.Deg2Rad;
            return horizontal * Mathf.Cos(radians) + Vector3.up * Mathf.Sin(radians);
        }
        float ClearDistance(Vector3 direction, float armLength)
        {
            if (Physics.SphereCast(smoothedFocus, collisionRadius, direction, out RaycastHit hit, armLength, blockers, QueryTriggerInteraction.Ignore))
                return Mathf.Max(0.01f, hit.distance - collisionPadding);
            return armLength;
        }
    }
}
