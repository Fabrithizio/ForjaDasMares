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
            int count = Physics.OverlapSphereNonAlloc(origin, attack.footprint==MeleeFootprint.Line&&arc<359.9f?Mathf.Sqrt(radius*radius+attack.width*attack.width*.25f):radius, contacts, targets, QueryTriggerInteraction.Ignore), hits = 0;
            if(attack.maximumTargets>0)
            {
                // Limited strikes choose the closest physical target, independent of collider enumeration.
                for(int i=1;i<count;i++){var value=contacts[i];float distance=(value.bounds.center-origin).sqrMagnitude;int j=i-1;while(j>=0&&(contacts[j].bounds.center-origin).sqrMagnitude>distance){contacts[j+1]=contacts[j];j--;}contacts[j+1]=value;}
            }
            for (int i = 0; i < count; i++)
            {
                if(attack.maximumTargets>0&&seenCount>=attack.maximumTargets)break;
                Health health = contacts[i].GetComponentInParent<Health>();
                if (health == null || !health.CanReceiveFrom(source)) continue;
                bool duplicate = false; for (int j = 0; j < seenCount; j++) if (seen[j] == health) { duplicate = true; break; }
                if (duplicate) continue;
                Vector3 point = contacts[i].bounds.center, delta = point - origin; delta.y = 0;
                if (!MeleeGeometry.Contains(delta,direction,radius,arc,attack.width,attack.footprint)) continue;
                if (Physics.Linecast(origin, point, geometry, QueryTriggerInteraction.Ignore)) continue;
                Remember(health);
                if (health.ApplyHit(attack, delta.sqrMagnitude > 0.001f ? delta.normalized : direction, source, damageScale)) hits++;
            }
            Array.Clear(contacts, 0, count); return hits;
        }
    }
}
