using UnityEngine;
namespace ForjaDasMares
{
    public sealed class SupportCompanion : MonoBehaviour
    {
        [SerializeField] Transform owner;
        [SerializeField] Health health;
        [SerializeField] ProgressionCatalog catalog;
        [SerializeField] LayerMask geometry;
        ProfileService profile;
        int charges;
        float cooldown;
        public int RemainingCharges => charges;
        public float CooldownRemaining => cooldown;
        public void Configure(Transform player, Health hp, ProgressionCatalog data, int walls) { owner = player; health = hp; catalog = data; geometry = walls; }
        public void Initialize(ProfileService service) { profile = service; ResetBudget(); }
        public void ResetBudget() { charges = catalog.companion.charges; cooldown = catalog.companion.cooldown; }
        void Update()
        {
            if (profile == null || owner == null) return;
            bool visible = profile.Profile.options.companionEnabled && health.IsAlive;
            foreach (Transform child in transform) if (child.gameObject.activeSelf != visible) child.gameObject.SetActive(visible);
            if (!visible) return;
            Vector3 point = owner.position - owner.forward * 1.3f + owner.right * .7f + Vector3.up * 1.5f;
            float distance = Vector3.Distance(transform.position, point);
            if (distance > catalog.companion.teleportDistance || Physics.Linecast(owner.position + Vector3.up, point, geometry)) point = owner.position + Vector3.up * 1.7f;
            transform.position = distance > catalog.companion.teleportDistance ? point : Vector3.MoveTowards(transform.position, point, catalog.companion.speed * Time.deltaTime);
            cooldown = Mathf.Max(0, cooldown - Time.deltaTime);
            if (cooldown == 0 && charges > 0 && Vector3.Distance(owner.position, transform.position) < 3 && !Physics.Linecast(owner.position + Vector3.up, transform.position, geometry) && health.Heal(catalog.companion.healing))
            { charges--; cooldown = catalog.companion.cooldown; }
        }
    }
}
