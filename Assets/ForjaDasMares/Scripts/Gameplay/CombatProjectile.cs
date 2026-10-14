using UnityEngine;

namespace ForjaDasMares
{
    public sealed class CombatProjectile : MonoBehaviour
    {
        readonly RaycastHit[] hits = new RaycastHit[24];
        readonly DamageResolver resolver = new DamageResolver();
        MaterialPropertyBlock properties;
        AttackDefinition attack;
        Health owner;
        Vector3 direction;
        float distance;
        float damageScale;
        int targets, geometry;
        public Color Tint => attack != null ? attack.color : Color.white;
        public bool OwnedBy(Health source) => gameObject.activeSelf && owner == source;
        void OnDisable() { owner = null; attack = null; resolver.Clear(); System.Array.Clear(hits, 0, hits.Length); }
        public void Fire(Vector3 position, Vector3 forward, AttackDefinition data, Health source, int targetMask, int wallMask, float scale = 1)
        {
            if (properties == null) properties = new MaterialPropertyBlock();
            transform.position = position; forward.y = 0; direction = forward.normalized;
            attack = data; owner = source; targets = targetMask; geometry = wallMask; distance = 0f;
            damageScale = scale;
            transform.localScale = Vector3.one * (data.projectileRadius * 2f);
            properties.SetColor("_BaseColor", data.color); GetComponent<Renderer>().SetPropertyBlock(properties);
            gameObject.SetActive(true);
        }
        void Update()
        {
            if (owner == null || !owner.IsAlive) { gameObject.SetActive(false); return; }
            float step = Mathf.Min(attack.projectileSpeed * Time.deltaTime, attack.range - distance);
            if (step <= 0f) { gameObject.SetActive(false); return; }
            int count = Physics.SphereCastNonAlloc(transform.position, attack.projectileRadius, direction, hits, step, targets | geometry, QueryTriggerInteraction.Ignore);
            int closest = -1; float nearest = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Health victim = hits[i].collider.GetComponentInParent<Health>();
                if (victim != null && !victim.CanReceiveFrom(owner)) continue;
                if (hits[i].distance < nearest) { closest = i; nearest = hits[i].distance; }
            }
            if (closest >= 0)
            {
                transform.position += direction * nearest;
                Health victim = hits[closest].collider.GetComponentInParent<Health>();
                resolver.Clear();
                if (victim != null) { resolver.Remember(victim); victim.ApplyHit(attack, direction, owner, damageScale); }
                if (attack.splashRadius > 0) resolver.Area(transform.position, attack.splashRadius, direction, 360f, attack, owner, targets, geometry, false, damageScale);
                gameObject.SetActive(false);
            }
            else { transform.position += direction * step; distance += step; }
            System.Array.Clear(hits, 0, count);
        }
    }
}
