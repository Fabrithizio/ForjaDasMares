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
        [Header("Stable framing")]
        [SerializeField, Min(0f)] float obstructionReleaseDelay = 0.2f;
        [SerializeField, Min(0.05f)] float visibilityRadius = 0.3f;
        [SerializeField, Range(0.15f, 0.75f)] float obstructionOpacity = 0.35f;
        [SerializeField, Min(0.01f)] float zoomOutTime = 0.28f;
        [SerializeField] LayerMask blockers;
        [SerializeField, Range(30f, 80f)] float verticalFieldOfView = 48f;
        [SerializeField, Min(0.5f)] float referenceAspect = 1.777778f;
        Vector3 smoothedFocus, smoothVelocity;float impulse,impulseTime;
        public void Impact(float strength){if(PlayerPrefs.GetInt("CampaignCameraShake",0)==1){impulse=Mathf.Max(impulse,Mathf.Clamp(strength,0,.06f));impulseTime=.12f;}}
        Camera cameraComponent;
        bool boundFocus;
        float armDistance, armVelocity;
        readonly CameraOcclusion occlusion = new CameraOcclusion();
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
            occlusion.Restore();
            smoothedFocus = Focus();
            smoothVelocity = Vector3.zero;
            ApplyPose(0f, true);
        }
        void LateUpdate()
        {
            smoothedFocus = Vector3.SmoothDamp(smoothedFocus, Focus(), ref smoothVelocity, followTime, Mathf.Infinity, Time.deltaTime);
            // This is a virtual tracking point, not the camera lens. Crossing a crate
            // must not reset the spring and repeatedly snap the view to the player.
            ApplyPose(Time.deltaTime, false);
        }
        void ApplyPose(float deltaTime, bool snap)
        {
            // Preserve horizontal framing on narrow screens instead of cropping the arena.
            float aspect = Mathf.Max(0.1f, cameraComponent.aspect);
            float tangent = Mathf.Tan(verticalFieldOfView * Mathf.Deg2Rad * 0.5f);
            cameraComponent.fieldOfView = Mathf.Clamp(2f * Mathf.Atan(tangent * Mathf.Max(1f, referenceAspect / aspect)) * Mathf.Rad2Deg, 30f, 95f);
            float armLength = Mathf.Max(0.1f, offset.magnitude);
            // A sight-line obstruction must not change the pitch or zoom. Only an
            // actual overlap at the lens may retract the arm for camera safety.
            Vector3 direction = offset.sqrMagnitude > 0.001f ? offset.normalized : Vector3.up;
            float safeDistance = armLength;
            if (Physics.CheckSphere(smoothedFocus + direction * armLength, collisionRadius, blockers, QueryTriggerInteraction.Ignore))
                safeDistance = ClearDistance(direction, armLength);
            if (snap || safeDistance < armDistance) { armDistance = safeDistance; armVelocity = 0f; }
            else if (deltaTime > 0f) armDistance = Mathf.Min(safeDistance, Mathf.SmoothDamp(armDistance, safeDistance, ref armVelocity, zoomOutTime, Mathf.Infinity, deltaTime));
            impulseTime=Mathf.Max(0,impulseTime-Time.unscaledDeltaTime);Vector3 shake=impulseTime>0?new Vector3(Mathf.Sin(Time.unscaledTime*90),0,Mathf.Cos(Time.unscaledTime*73))*impulse*(impulseTime/.12f):Vector3.zero;
            transform.position = smoothedFocus + direction * armDistance+shake;
            transform.rotation = Quaternion.LookRotation(-direction, Mathf.Abs(direction.y) > 0.999f ? Vector3.forward : Vector3.up);
            occlusion.Update(transform.position, target.position, blockers, visibilityRadius, obstructionReleaseDelay, Time.unscaledTime, obstructionOpacity);
        }
        void OnDisable() => occlusion.Restore();
        void OnDestroy() => occlusion.Restore();
        float ClearDistance(Vector3 direction, float armLength)
        {
            if (Physics.SphereCast(smoothedFocus, collisionRadius, direction, out RaycastHit hit, armLength, blockers, QueryTriggerInteraction.Ignore))
                return Mathf.Max(0.01f, hit.distance - collisionPadding);
            return armLength;
        }
    }
}
