using UnityEngine;
namespace ForjaDasMares
{
    public sealed class EnemyPresentation : MonoBehaviour
    {
        [SerializeField] EnemyActor actor;
        [SerializeField] Transform model;
        [SerializeField] Renderer[] surfaces;
        [SerializeField] GameObject shield;
        MaterialPropertyBlock colors;
        Color baseColor;
        float flash, step;
        bool bound;
        EnemyModelMotion segmentedMotion;
        public Transform Model=>model;
        public void Configure(EnemyActor owner, Transform body, Renderer[] renderers, GameObject guard) { actor = owner; model = body; surfaces = renderers; shield = guard; }
        public void Prepare(EnemyDefinition data)
        {
            if (colors == null) colors = new MaterialPropertyBlock();
            if (!bound) { actor.Health.Damaged += Hit; bound = true; }
            baseColor = data.color; flash = step = 0; model.localPosition = Vector3.zero; model.localRotation = Quaternion.identity; Paint(baseColor);
            enabled=true;segmentedMotion=model.GetComponent<EnemyModelMotion>();if(segmentedMotion!=null)segmentedMotion.ClearReplicaState();
        }
        public void ShowReplica(EnemyDefinition data, EnemyState state, bool alive,bool opening=false)
        { if (colors == null) Prepare(data);if(segmentedMotion!=null)segmentedMotion.SetReplicaState(state); enabled = false; Paint(opening?Color.Lerp(data.color,new Color(1,.85f,.3f),.5f):data.color); if (shield != null) shield.SetActive(data.kind == EnemyKind.Guardian && alive && state != EnemyState.Attack && state != EnemyState.Stunned); model.localRotation = Quaternion.Euler(!alive ? 80 : state == EnemyState.Attack ? -12 : 0, 0, 0); }
        void Hit() => flash = 0.12f;
        void Paint(Color value) { colors.SetColor("_BaseColor", value); colors.SetColor("_Color", value); foreach (var surface in surfaces) surface.SetPropertyBlock(colors); }
        void LateUpdate()
        {
            if (colors == null) return;
            flash = Mathf.Max(0, flash - Time.deltaTime); step += actor.Motor.Velocity.magnitude * Time.deltaTime * 2;
            Paint(flash > 0 ? Color.white : actor.Profile.miniboss&&actor.Combat.IsRecovering?Color.Lerp(baseColor,new Color(1,.85f,.3f),.5f): actor.Brain.BossPhase == 2 ? Color.Lerp(baseColor, Color.red, 0.3f) : baseColor);
            if (shield != null) shield.SetActive(actor.Brain.GuardRaised);
            model.localPosition = Vector3.up * (actor.IsAlive ? Mathf.Abs(Mathf.Sin(step)) * 0.05f : 0.05f);
            model.localRotation = Quaternion.Euler(actor.IsAlive ? actor.Combat.WarningRemaining > 0 ? -12 : 0 : 80, 0, 0);
        }
        void OnDestroy() { if (bound && actor != null) actor.Health.Damaged -= Hit; }
    }
}
