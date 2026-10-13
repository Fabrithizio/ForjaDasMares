using UnityEngine;

namespace ForjaDasMares
{
    public sealed class CombatEffectPool : MonoBehaviour
    {
        [SerializeField] Material material;
        [SerializeField, Min(1)] int projectileCapacity = 32, zoneCapacity = 12;
        CombatProjectile[] projectiles;
        CombatZone[] zones;
        public int ActiveProjectiles { get { int count = 0; if (projectiles != null) foreach (var shot in projectiles) if (shot.gameObject.activeSelf) count++; return count; } }
        public int ActiveZones { get { int count = 0; if (zones != null) foreach (var zone in zones) if (zone.gameObject.activeSelf) count++; return count; } }
        public void Configure(Material sharedMaterial) => material = sharedMaterial;
        void Awake()
        {
            projectiles = new CombatProjectile[projectileCapacity]; zones = new CombatZone[zoneCapacity];
            for (int i = 0; i < projectiles.Length; i++) projectiles[i] = Create("PooledProjectile_" + i, PrimitiveType.Sphere).AddComponent<CombatProjectile>();
            for (int i = 0; i < zones.Length; i++) zones[i] = Create("PooledZone_" + i, PrimitiveType.Cylinder).AddComponent<CombatZone>();
        }
        GameObject Create(string label, PrimitiveType shape)
        {
            GameObject item = GameObject.CreatePrimitive(shape); item.name = label; item.transform.SetParent(transform);
            Destroy(item.GetComponent<Collider>()); item.GetComponent<Renderer>().sharedMaterial = material;
            item.SetActive(false); return item;
        }
        public bool Shoot(Vector3 origin, Vector3 direction, AttackDefinition data, Health owner, int targets, int geometry)
        {
            foreach (CombatProjectile shot in projectiles)
                if (!shot.gameObject.activeSelf) { shot.Fire(origin, direction, data, owner, targets, geometry); return true; }
            return false;
        }
        public bool PlaceZone(Vector3 origin, float radius, float lifetime, AttackDefinition data, Health owner, int targets, int geometry, bool trap)
        {
            foreach (CombatZone zone in zones)
                if (!zone.gameObject.activeSelf) { zone.Place(origin, radius, lifetime, data, owner, targets, geometry, trap); return true; }
            return false;
        }
        public void Clear()
        {
            if (projectiles != null) foreach (var shot in projectiles) shot.gameObject.SetActive(false);
            if (zones != null) foreach (var zone in zones) zone.gameObject.SetActive(false);
        }
    }
}
