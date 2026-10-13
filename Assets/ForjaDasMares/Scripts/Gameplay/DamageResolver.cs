using System;
using UnityEngine;

namespace ForjaDasMares
{
    public sealed class DamageResolver
    {
        readonly Collider[] contacts = new Collider[64];
        readonly Health[] seen = new Health[64];
        int seenCount;
        public void Clear() { Array.Clear(seen, 0, seenCount); seenCount = 0; }
        public void Remember(Health target) { if (seenCount < seen.Length) seen[seenCount++] = target; }
        public int Area(Vector3 origin, float radius, Vector3 direction, float arc, AttackDefinition attack, Health source, int targets, int geometry, bool clear = true, float damageScale = 1)
        {
            if (clear) Clear();
            int count = Physics.OverlapSphereNonAlloc(origin, radius, contacts, targets, QueryTriggerInteraction.Ignore), hits = 0;
            float dot = Mathf.Cos(arc * 0.5f * Mathf.Deg2Rad);
            for (int i = 0; i < count; i++)
            {
                Health health = contacts[i].GetComponentInParent<Health>();
                if (health == null || !health.CanReceiveFrom(source)) continue;
                bool duplicate = false; for (int j = 0; j < seenCount; j++) if (seen[j] == health) { duplicate = true; break; }
                if (duplicate) continue;
                Vector3 point = contacts[i].bounds.center, delta = point - origin; delta.y = 0;
                if (delta.sqrMagnitude > radius * radius || (delta.sqrMagnitude > 0.001f && Vector3.Dot(direction, delta.normalized) < dot)) continue;
                if (Physics.Linecast(origin, point, geometry, QueryTriggerInteraction.Ignore)) continue;
                Remember(health);
                if (health.ApplyHit(attack, delta.sqrMagnitude > 0.001f ? delta.normalized : direction, source, damageScale)) hits++;
            }
            Array.Clear(contacts, 0, count); return hits;
        }
    }
}
