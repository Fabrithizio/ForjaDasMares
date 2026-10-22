using UnityEngine;

namespace ForjaDasMares
{
    public sealed class CombatEffectPool : MonoBehaviour
    {
        [SerializeField] Material material;
        [SerializeField] Material projectileTrailMaterial;
        public Material ProjectileTrailMaterial=>projectileTrailMaterial;
        public void ConfigureTrail(Material value)=>projectileTrailMaterial=value;
        [SerializeField] Mesh projectileMesh, zoneMesh;
        [SerializeField, Min(1)] int projectileCapacity = 32, zoneCapacity = 12;
        CombatProjectile[] projectiles;
        CombatZone[] zones;
        public CombatProjectile[] Projectiles => projectiles;
        public CombatZone[] Zones => zones;
        public Material SharedMaterial => material;
        public int ActiveProjectiles { get { int count = 0; if (projectiles != null) foreach (var shot in projectiles) if (shot.gameObject.activeSelf) count++; return count; } }
        public int ActiveZones { get { int count = 0; if (zones != null) foreach (var zone in zones) if (zone.gameObject.activeSelf) count++; return count; } }
        public void Configure(Material sharedMaterial) => material = sharedMaterial;
        public void ConfigureMeshes(Mesh projectile, Mesh zone) { projectileMesh = projectile; zoneMesh = zone; }
        void Awake()
        {
            if (projectileMesh == null || zoneMesh == null || material == null) { Debug.LogError("Combat pool requires explicit meshes and material.", this); enabled = false; return; }
            projectiles = new CombatProjectile[projectileCapacity]; zones = new CombatZone[zoneCapacity];
            for (int i = 0; i < projectiles.Length; i++) projectiles[i] = Create("PooledProjectile_" + i, projectileMesh).AddComponent<CombatProjectile>();
            for (int i = 0; i < zones.Length; i++) zones[i] = Create("PooledZone_" + i, zoneMesh).AddComponent<CombatZone>();
        }
        GameObject Create(string label, Mesh mesh)
        {
            GameObject item = new GameObject(label, typeof(MeshFilter), typeof(MeshRenderer)); item.transform.SetParent(transform);
            item.GetComponent<MeshFilter>().sharedMesh = mesh; item.GetComponent<MeshRenderer>().sharedMaterial = material;
            item.SetActive(false); return item;
        }
        public bool Shoot(Vector3 origin, Vector3 direction, AttackDefinition data, Health owner, int targets, int geometry, float damageScale = 1)
        {
            if (projectiles == null) return false;
            foreach (CombatProjectile shot in projectiles)
                if (!shot.gameObject.activeSelf) { shot.Fire(origin, direction, data, owner, targets, geometry, damageScale); return true; }
            return false;
        }
        public bool PlaceZone(Vector3 origin, float radius, float lifetime, AttackDefinition data, Health owner, int targets, int geometry, bool trap, float damageScale = 1)
        {
            if (zones == null) return false;
            foreach (CombatZone zone in zones)
                if (!zone.gameObject.activeSelf) { zone.Place(origin, radius, lifetime, data, owner, targets, geometry, trap, damageScale); return true; }
            return false;
        }
        public void Clear()
        {
            if (projectiles != null) foreach (var shot in projectiles) shot.gameObject.SetActive(false);
            if (zones != null) foreach (var zone in zones) zone.gameObject.SetActive(false);
        }
        public bool HasActiveEffectsOwnedBy(Health owner)
        {
            if (owner == null) return false;
            if (projectiles != null) foreach (var shot in projectiles) if (shot.OwnedBy(owner)) return true;
            if (zones != null) foreach (var zone in zones) if (zone.OwnedBy(owner)) return true;
            return false;
        }
        public void ClearOwned(Health owner)
        {
            if (projectiles != null) foreach (var shot in projectiles) if (shot.OwnedBy(owner)) shot.gameObject.SetActive(false);
            if (zones != null) foreach (var zone in zones) if (zone.OwnedBy(owner)) zone.gameObject.SetActive(false);
        }
    }
}
