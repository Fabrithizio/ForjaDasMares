using UnityEngine;

namespace ForjaDasMares
{
    [RequireComponent(typeof(Health), typeof(CapsuleCollider))]
    public sealed class TrainingTargetMotion : MonoBehaviour
    {
        [SerializeField] LayerMask geometry;
        Health health; CapsuleCollider capsule;
        Vector3 initial, velocity;
        public void Configure(int mask) => geometry = mask;
        void Awake() { initial = transform.position; health = GetComponent<Health>(); capsule = GetComponent<CapsuleCollider>(); }
        void OnEnable() { GetComponent<Health>().Impact += Receive; }
        void OnDisable() { if (health != null) health.Impact -= Receive; }
        void Receive(Vector3 direction, float force) { direction.y = 0; velocity += direction.normalized * force; velocity = Vector3.ClampMagnitude(velocity, 10f); }
        void Update()
        {
            Vector3 displacement = velocity * (Time.deltaTime * health.ControlMultiplier);
            if (displacement.sqrMagnitude > 0.000001f)
            {
                Vector3 center = transform.TransformPoint(capsule.center);
                float half = Mathf.Max(0, capsule.height * 0.5f - capsule.radius);
                float distance = displacement.magnitude;
                if (Physics.CapsuleCast(center - Vector3.up * half, center + Vector3.up * half, capsule.radius, displacement.normalized, out RaycastHit hit, distance, geometry, QueryTriggerInteraction.Ignore))
                { displacement = displacement.normalized * Mathf.Max(0, hit.distance - 0.03f); velocity = Vector3.zero; }
                transform.position += displacement;
            }
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, 9f * Time.deltaTime);
        }
        public void ResetTarget() { transform.position = initial; velocity = Vector3.zero; health.Restore(); Physics.SyncTransforms(); }
    }
}
