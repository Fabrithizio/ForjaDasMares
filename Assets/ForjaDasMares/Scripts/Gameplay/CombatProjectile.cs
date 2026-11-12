using UnityEngine;

namespace ForjaDasMares
{
    public sealed class CombatProjectile : MonoBehaviour
    {
        readonly RaycastHit[] hits = new RaycastHit[24];
        readonly DamageResolver resolver = new DamageResolver();
        MaterialPropertyBlock properties;
        CombatEffectPool effectPool;
        AttackDefinition attack;
        Health owner;
        Vector3 direction, launchPosition;
        float returnTime, spin;
        bool returning;
        readonly System.Collections.Generic.HashSet<Health> struck = new System.Collections.Generic.HashSet<Health>();
        float distance;
        float damageScale;
        bool criticalHit;
        int targets, geometry;
        public Color Tint => attack != null ? attack.color : Color.white;
        public bool OwnedBy(Health source) => gameObject.activeSelf && owner == source;
        void OnDisable() { owner = null; attack = null; struck.Clear(); resolver.Clear(); System.Array.Clear(hits, 0, hits.Length); }
        public void Fire(Vector3 position, Vector3 forward, AttackDefinition data, Health source, int targetMask, int wallMask, float scale = 1, bool critical = false)
        {
            if (properties == null) properties = new MaterialPropertyBlock();
            forward.y = 0;
            direction = forward.sqrMagnitude > .0001f ? forward.normalized : Vector3.forward;
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
            attack = data; owner = source; targets = targetMask; geometry = wallMask; distance = 0f;
            launchPosition = position;
            returning = false;
            returnTime = spin = 0;
            damageScale = scale;
            criticalHit = critical;
            transform.localScale = Vector3.one * (data.projectileRadius * 2f);
            properties.SetColor("_BaseColor", data.color); GetComponent<Renderer>().SetPropertyBlock(properties);
            gameObject.SetActive(true);
            if (effectPool == null) effectPool = GetComponentInParent<CombatEffectPool>();
            effectPool?.ShapeProjectile(gameObject,data.prismaticEffect == PrismaticEffect.Shatter);
            if (data.projectileVisual != null)
            {
                GetComponent<MeshFilter>().sharedMesh = data.projectileVisual;
                var renderer = GetComponent<MeshRenderer>();
                renderer.sharedMaterials = data.projectileMaterials;
                renderer.SetPropertyBlock(null);
                transform.localScale = Vector3.one * .84f;
            }
            var ribbon=effectPool!=null?effectPool.ProjectileTrailMaterial:null;
            if(ribbon!=null){var trail=GetComponent<PooledProjectileTrail>()??gameObject.AddComponent<PooledProjectileTrail>();trail.Configure(ribbon,data.color,data.projectileRadius,true);}
        }
        void Update()
        {
            if (owner == null || !owner.IsAlive) { gameObject.SetActive(false); return; }
            if (attack.returningProjectile) { UpdateReturnFlight(); return; }
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
                if (victim != null) { resolver.Remember(victim); victim.ApplyHit(attack, direction, owner, damageScale, criticalHit); }
                if (attack.prismaticEffect == PrismaticEffect.Shatter)
                    effectPool?.PrismBurst(transform.position, .55f, 0);
                if (attack.splashRadius > 0) resolver.Area(transform.position, attack.splashRadius, direction, 360f, attack, owner, targets, geometry, false, damageScale, criticalHit);
                gameObject.SetActive(false);
            }
            else { transform.position += direction * step; distance += step; }
            System.Array.Clear(hits, 0, count);
        }
        void UpdateReturnFlight()
        {
            Vector3 position = transform.position;
            Vector3 next;
            float step = attack.projectileSpeed * Time.deltaTime;
            if (returning)
            {
                Vector3 hand = owner.transform.position;
                hand.y = launchPosition.y;
                returnTime += Time.deltaTime;
                if (returnTime > 2.5f || Vector3.Distance(hand, position) <= Mathf.Max(step, .35f))
                {
                    gameObject.SetActive(false);
                    return;
                }
                Vector3 home = hand - position;
                float turn = Mathf.Min(40, home.magnitude * 8);
                next = position + Quaternion.Euler(0, turn, 0) * home.normalized * step;
            }
            else
            {
                distance = Mathf.Min(distance + step, attack.range);
                float progress = distance / attack.range;
                Vector3 side = Vector3.Cross(Vector3.up, direction);
                next = launchPosition + direction * distance + side * (Mathf.Sin(progress * Mathf.PI) * attack.range * .23f);
            }

            Vector3 travel = next - position;
            float length = travel.magnitude;
            if (length <= .0001f) return;
            Vector3 heading = travel / length;
            float wallDistance = Physics.SphereCast(position, attack.projectileRadius, heading, out var wall,
                length, geometry, QueryTriggerInteraction.Ignore) ? wall.distance : length;
            int count = Physics.SphereCastNonAlloc(position, attack.projectileRadius, heading, hits,
                wallDistance, targets, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var target = hits[i].collider.GetComponentInParent<Health>();
                if (target != null && target.CanReceiveFrom(owner) && struck.Add(target))
                    target.ApplyHit(attack, heading, owner, damageScale, criticalHit);
            }
            System.Array.Clear(hits, 0, count);
            transform.position = position + heading * Mathf.Max(0, wallDistance - (wall.collider != null ? .025f : 0));
            spin += Time.deltaTime * 1040;
            transform.rotation = Quaternion.AngleAxis(spin, Vector3.up) * Quaternion.Euler(90, 0, 0);
            if (wall.collider != null && returning) gameObject.SetActive(false);
            else if (wall.collider != null || distance >= attack.range) returning = true;
        }
    }
}
