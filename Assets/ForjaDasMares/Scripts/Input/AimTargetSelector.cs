using UnityEngine;
namespace ForjaDasMares
{
    // Retains a visible target until another is meaningfully closer. No per-scan allocations.
    public sealed class AimTargetSelector
    {
        public Health Target { get; private set; }
        Vector3 origin;
        float rangeSquared, nearestSquared, retainedSquared;
        int walls;
        Health nearest;
        bool retainedValid, retainedSeen;
        public void Reset() { Target = nearest = null; retainedValid = retainedSeen = false; }
        public void BeginScan(Vector3 playerPosition, float range, int geometry)
        {
            origin = playerPosition + Vector3.up; walls = geometry;
            rangeSquared = Mathf.Max(0, range) * Mathf.Max(0, range);
            nearestSquared = rangeSquared; nearest = null; retainedSeen = false;
            retainedValid = Eligible(Target, out retainedSquared);
        }
        bool Eligible(Health candidate, out float distanceSquared)
        {
            distanceSquared = float.PositiveInfinity;
            if (candidate == null || !candidate.gameObject.activeInHierarchy || !candidate.IsAlive || candidate.Faction != CombatFaction.Enemy) return false;
            Vector3 offset = candidate.transform.position - origin; offset.y = 0;
            distanceSquared = offset.sqrMagnitude;
            return distanceSquared <= rangeSquared && !Physics.Linecast(origin, candidate.transform.position + Vector3.up, walls, QueryTriggerInteraction.Ignore);
        }
        public void Consider(Health candidate)
        {
            if (candidate == Target) retainedSeen = true;
            if (Eligible(candidate, out float distance) && distance <= nearestSquared)
            { nearest = candidate; nearestSquared = distance; }
        }
        public Health EndScan(float switchRatio = .7f, float minimumAdvantage = .75f)
        {
            if (retainedValid && retainedSeen)
            {
                float current = Mathf.Sqrt(retainedSquared), next = Mathf.Sqrt(nearestSquared);
                if (nearest == null || nearest == Target || next >= current * Mathf.Clamp(switchRatio, .1f, .95f) || current - next < Mathf.Max(0, minimumAdvantage)) return Target;
            }
            Target = nearest; return Target;
        }
    }
}
