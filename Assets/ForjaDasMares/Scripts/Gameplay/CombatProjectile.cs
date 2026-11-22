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
        // Shots follow the ground: on hills they keep the same height above the floor, so they do not fly
        // over a creature standing lower (or into the slope of a creature standing higher).
        const int GroundMask = (1 << 0) | (1 << 8);
        float aboveGround = -1;
        float returnTime, spin;
        bool returning;
        readonly System.Collections.Generic.HashSet<Health> struck = new System.Collections.Generic.HashSet<Health>();
        float distance, outboundLimit;
        bool countedInFlight;
        static readonly Collider[] nearby = new Collider[32];
        static readonly System.Collections.Generic.Dictionary<Health,int> inFlight = new System.Collections.Generic.Dictionary<Health,int>();
        // True while a returning weapon (boomerang) thrown by this hero is still flying; the hand copy hides meanwhile.
        public static bool HasReturningInFlight(Health source) => source != null && inFlight.TryGetValue(source, out int count) && count > 0;
        float damageScale;
        bool criticalHit;
        int targets, geometry;
        public Color Tint => attack != null ? attack.color : Color.white;
        public bool OwnedBy(Health source) => gameObject.activeSelf && owner == source;
        void OnDisable() { if (countedInFlight && owner != null && inFlight.TryGetValue(owner, out int count)) { if (count <= 1) inFlight.Remove(owner); else inFlight[owner] = count - 1; } countedInFlight = false; owner = null; attack = null; struck.Clear(); resolver.Clear(); System.Array.Clear(hits, 0, hits.Length); }
        public void Fire(Vector3 position, Vector3 forward, AttackDefinition data, Health source, int targetMask, int wallMask, float scale = 1, bool critical = false)
        {
            if (properties == null) properties = new MaterialPropertyBlock();
            forward.y = 0;
            direction = forward.sqrMagnitude > .0001f ? forward.normalized : Vector3.forward;
            attack = data; owner = source; targets = targetMask; geometry = wallMask; distance = 0f;
            outboundLimit = data.range;
            if (data.returningProjectile) AimAtEnemy(position);
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
            launchPosition = position;
            aboveGround = Physics.Raycast(position, Vector3.down, out var floor, 6f, GroundMask, QueryTriggerInteraction.Ignore) ? position.y - floor.point.y : -1;
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
                transform.localScale = Vector3.one * (.84f * (data.projectileVisualScale > 0 ? data.projectileVisualScale : 1f));
            }
            var hearts=GetComponent<HeartFx>();
            if(data.heartFx!=null)
            {
                if(hearts==null)hearts=gameObject.AddComponent<HeartFx>();
                hearts.Play(data.heartFx);
                // Legendary: magic circle at the hero's feet on every throw.
                if(data.heartFx.legendary&&source!=null)HeartFx.Rune(source.transform.position,data.heartFx,2.6f,true);
                var plain=GetComponent<PooledProjectileTrail>();if(plain!=null)plain.Stop();
            }
            else
            {
                if(hearts!=null)hearts.Stop();
                var ribbon=effectPool!=null?effectPool.ProjectileTrailMaterial:null;
                if(ribbon!=null){var trail=GetComponent<PooledProjectileTrail>()??gameObject.AddComponent<PooledProjectileTrail>();trail.Configure(ribbon,data.color,data.projectileRadius,true,data.returningProjectile);}
            }
            if (data.returningProjectile && source != null && !countedInFlight)
            { inFlight.TryGetValue(source, out int count); inFlight[source] = count + 1; countedInFlight = true; }
        }
        // Boomerang: picks the enemy closest to the aimed direction (within 35 degrees) and flies straight to it,
        // turning back a little behind it. Without an enemy in front it flies its full range.
        void AimAtEnemy(Vector3 position)
        {
            int count = Physics.OverlapSphereNonAlloc(position, attack.range, nearby, targets, QueryTriggerInteraction.Ignore);
            Health best = null; float bestAngle = 35f, bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var candidate = nearby[i].GetComponentInParent<Health>();
                if (candidate == null || candidate == owner || !candidate.IsAlive || !candidate.CanReceiveFrom(owner)) continue;
                Vector3 toward = candidate.transform.position - position; toward.y = 0;
                float length = toward.magnitude;
                if (length < .05f || length > attack.range) continue;
                float angle = Vector3.Angle(direction, toward);
                if (angle > 35f) continue;
                if (angle < bestAngle - 4f || (Mathf.Abs(angle - bestAngle) <= 4f && length < bestDistance))
                { best = candidate; bestAngle = angle; bestDistance = length; }
            }
            System.Array.Clear(nearby, 0, count);
            if (best == null) return;
            Vector3 aim = best.transform.position - position; aim.y = 0;
            direction = aim.normalized;
            outboundLimit = Mathf.Min(attack.range, bestDistance + 1f);
        }
        void Update()
        {
            if (owner == null || !owner.IsAlive) { gameObject.SetActive(false); return; }
            if (attack.returningProjectile) { UpdateReturnFlight(); return; }
            float step = Mathf.Min(attack.projectileSpeed * Time.deltaTime, attack.range - distance);
            if (step <= 0f) { gameObject.SetActive(false); return; }
            // Walls at the height of the shot; creatures with a standing column from the ground up, so a shot
            // never flies over (or under) a creature, whatever its size or the slope (seen from above, it is a hit).
            float wall = Physics.SphereCast(transform.position, attack.projectileRadius, direction, out var wallHit, step, geometry, QueryTriggerInteraction.Ignore) ? wallHit.distance : float.MaxValue;
            var p0 = transform.position;
            int count = Physics.CapsuleCastNonAlloc(p0 - Vector3.up * 1.4f, p0 + Vector3.up * 1.2f, Mathf.Max(attack.projectileRadius, .22f), direction, hits, step, targets, QueryTriggerInteraction.Ignore);
            int closest = -1; float nearest = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Health victim = hits[i].collider.GetComponentInParent<Health>();
                if (victim == null || !victim.CanReceiveFrom(owner)) continue;
                if (hits[i].distance < nearest && hits[i].distance <= wall) { closest = i; nearest = hits[i].distance; }
            }
            if (closest < 0 && wall <= step)
            {
                transform.position += direction * wall;
                if (attack.prismaticEffect == PrismaticEffect.Shatter) effectPool?.PrismBurst(transform.position, .55f, 0);
                if (attack.splashRadius > 0) resolver.Area(transform.position, attack.splashRadius, direction, 360f, attack, owner, targets, geometry, false, damageScale, criticalHit);
                System.Array.Clear(hits, 0, count);
                gameObject.SetActive(false);
                return;
            }
            if (closest >= 0)
            {
                transform.position += direction * nearest;
                Health victim = hits[closest].collider.GetComponentInParent<Health>();
                resolver.Clear();
                if (victim != null) { resolver.Remember(victim); victim.ApplyHit(attack, direction, owner, damageScale, criticalHit); if (attack.heartFx != null) HeartFx.Impact(transform.position, attack.heartFx); }
                if (attack.prismaticEffect == PrismaticEffect.Shatter)
                    effectPool?.PrismBurst(transform.position, .55f, 0);
                if (attack.splashRadius > 0) resolver.Area(transform.position, attack.splashRadius, direction, 360f, attack, owner, targets, geometry, false, damageScale, criticalHit);
                gameObject.SetActive(false);
            }
            else { transform.position += direction * step; distance += step; FollowGround(); }
            System.Array.Clear(hits, 0, count);
        }
        void FollowGround()
        {
            if (aboveGround < 0) return;
            var p = transform.position;
            if (!Physics.Raycast(p + Vector3.up * 1.5f, Vector3.down, out var floor, 5f, GroundMask, QueryTriggerInteraction.Ignore)) return;
            p.y = Mathf.MoveTowards(p.y, floor.point.y + aboveGround, 12f * Time.deltaTime);
            transform.position = p;
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
                    if (returnTime <= 2.5f && attack.splashRadius > 0)
                    {
                        resolver.Area(hand, attack.splashRadius, direction, 360, attack, owner, targets, geometry, false, damageScale, criticalHit);
                        if (attack.heartFx != null) HeartFx.Impact(hand, attack.heartFx);
                        else effectPool?.PrismBurst(hand - Vector3.up * .45f, attack.splashRadius, 6, .74f);
                    }
                    gameObject.SetActive(false);
                    return;
                }
                Vector3 home = hand - position;
                float turn = Mathf.Min(40, home.magnitude * 8);
                next = position + Quaternion.Euler(0, turn, 0) * home.normalized * step;
            }
            else
            {
                // Outbound: straight at the target, so the first enemy is always hit. The return curves around the side.
                distance = Mathf.Min(distance + step, outboundLimit);
                next = launchPosition + direction * distance;
            }

            Vector3 travel = next - position;
            float length = travel.magnitude;
            if (length <= .0001f) return;
            Vector3 heading = travel / length;
            float wallDistance = Physics.SphereCast(position, attack.projectileRadius, heading, out var wall,
                length, geometry, QueryTriggerInteraction.Ignore) ? wall.distance : length;
            int count = Physics.CapsuleCastNonAlloc(position - Vector3.up * 1.4f, position + Vector3.up * 1.2f, Mathf.Max(attack.projectileRadius, .22f), heading, hits,
                wallDistance, targets, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var target = hits[i].collider.GetComponentInParent<Health>();
                if (target != null && target.CanReceiveFrom(owner) && struck.Add(target))
                {
                    resolver.Remember(target);
                    target.ApplyHit(attack, heading, owner, damageScale, criticalHit);
                    if (attack.heartFx != null) HeartFx.Impact(position, attack.heartFx);
                    else effectPool?.PrismBurst(target.transform.position, .55f, 6, .34f);
                }
            }
            System.Array.Clear(hits, 0, count);
            transform.position = position + heading * Mathf.Max(0, wallDistance - (wall.collider != null ? .025f : 0));
            spin += Time.deltaTime * (attack.spinSpeed > 0 ? attack.spinSpeed : 1040);
            transform.rotation = Quaternion.AngleAxis(spin, Vector3.up) * Quaternion.Euler(90, 0, 0);
            if (wall.collider != null && returning) gameObject.SetActive(false);
            else if (wall.collider != null || distance >= outboundLimit) returning = true;
        }
    }
}
