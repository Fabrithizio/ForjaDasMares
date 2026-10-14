using UnityEngine;
namespace ForjaDasMares
{
    // Optional authority-side party. Offline scenes continue using their original roster.
    public sealed class CombatParty : MonoBehaviour
    {
        readonly Health[] members = new Health[4];
        public bool AnyAlive { get { foreach (var hp in members) if (hp != null && hp.gameObject.activeInHierarchy && hp.IsAlive) return true; return false; } }
        public void Set(int slot, Health member) { if (slot < 0 || slot >= members.Length) throw new System.ArgumentOutOfRangeException(nameof(slot)); members[slot] = member; }
        public Health Get(int slot) => slot >= 0 && slot < members.Length ? members[slot] : null;
        public Health Nearest(Vector3 position)
        {
            Health result = null; float best = float.MaxValue;
            foreach (var hp in members)
            {
                if (hp == null || !hp.gameObject.activeInHierarchy || !hp.IsAlive) continue;
                float distance = (hp.transform.position - position).sqrMagnitude;
                if (distance < best) { best = distance; result = hp; }
            }
            return result;
        }
    }
}
