using UnityEngine;
namespace ForjaDasMares
{
    public sealed class DungeonTrap : MonoBehaviour
    {
        [SerializeField, Min(0.6f)] float warningTime = 1.1f;
        [SerializeField, Min(1)] float interval = 3;
        [SerializeField, Min(0.5f)] float radius = 1.4f;
        readonly DamageResolver resolver = new DamageResolver();
        DungeonSession session;
        DungeonRoom room;
        TelegraphPool warnings;
        AttackDefinition hit;
        float timer, warning;
        bool signaling, disarmed;
        public bool Disarmed => disarmed;
        public void Configure(DungeonSession owner, DungeonRoom context, TelegraphPool signs, AttackDefinition data) { session = owner; room = context; warnings = signs; hit = data; timer = 1; }
        void Update()
        {
            if (disarmed || session.CurrentRoom != room || !session.HasLivingPlayers) return;
            if (signaling)
            {
                warning -= Time.deltaTime; if (warning > 0) return;
                warnings.ReleaseOwner(this); signaling = false; timer = interval;
                resolver.Area(transform.position + Vector3.up, radius, Vector3.forward, 360, hit, null, 1 << 11, 1 << 9);
            }
            else
            {
                timer -= Time.deltaTime; if (timer > 0) return;
                if (warnings.Acquire(TelegraphShape.Circle, transform.position, Vector3.forward, radius, 0, 360, warningTime, Color.red, this) != null) { signaling = true; warning = warningTime; }
            }
        }
        public void Disarm() { disarmed = true; signaling = false; warnings.ReleaseOwner(this); }
        void OnDisable() { if (warnings != null) warnings.ReleaseOwner(this); signaling = false; }
    }
}
