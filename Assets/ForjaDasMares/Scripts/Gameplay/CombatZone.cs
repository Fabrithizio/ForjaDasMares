using UnityEngine;

namespace ForjaDasMares
{
    public sealed class CombatZone : MonoBehaviour
    {
        readonly DamageResolver resolver = new DamageResolver();
        MaterialPropertyBlock properties;
        AttackDefinition attack; Health owner;
        float radius, remaining, poll;
        float damageScale;
        int targets, geometry;
        bool armed;
        public Color Tint => attack != null ? attack.color : Color.white;
        public bool OwnedBy(Health source) => gameObject.activeSelf && owner == source;
        void OnDisable() { owner = null; attack = null; resolver.Clear(); }
        public void Place(Vector3 point, float area, float lifetime, AttackDefinition data, Health source, int mask, int walls, bool trap, float scale = 1)
        {
            if (properties == null) properties = new MaterialPropertyBlock();
            point.y = 0.04f; transform.position = point; radius = area; remaining = lifetime; poll = 0;
            attack = data; owner = source; targets = mask; geometry = walls; armed = trap;
            damageScale = scale;
            transform.localScale = new Vector3(area * 2f, 0.025f, area * 2f);
            properties.SetColor("_BaseColor", data.color); GetComponent<Renderer>().SetPropertyBlock(properties);
            gameObject.SetActive(true);
        }
        void Update()
        {
            remaining -= Time.deltaTime;
            if (remaining <= 0 || owner == null || !owner.IsAlive) { gameObject.SetActive(false); return; }
            if (!armed) return;
            poll -= Time.deltaTime;
            if (poll > 0) return;
            poll = 0.08f;
            if (resolver.Area(transform.position + Vector3.up, radius, Vector3.forward, 360, attack, owner, targets, geometry, true, damageScale) > 0)
            { armed = false; remaining = 0.65f; }
        }
    }
}
