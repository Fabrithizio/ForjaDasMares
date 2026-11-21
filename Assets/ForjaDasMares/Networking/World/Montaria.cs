using UnityEngine;

namespace ForjaDasMares.Networking
{
    // The blue wolf under the hero: walks/runs with the hero's speed, and the hero rides it.
    // The rider is placed every frame on the wolf's back bones (between hips and chest), so he goes up and down
    // with the wolf's steps, in the middle of the saddle. The wolf appears with a burst of light (RegionalWorld.Chamar).
    public sealed class MontariaLobo : MonoBehaviour
    {
        // Where the saddle is between the hips (0) and the chest (1), and how high the rider's hips sit above that line.
        const float PontoDaSela = -.1f, AlturaDoQuadril = .5f;
        HeroRoster hero; PlayerMotor motor; Animator wolf; Transform body; HeroModelView view;
        Transform hips, chest; float acima; string playing; float smoothSpeed;

        public void Configure(HeroRoster owner, GameObject model)
        {
            hero = owner; motor = owner.GetComponent<PlayerMotor>();
            wolf = model.GetComponentInChildren<Animator>();
            foreach (var t in model.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "hips") hips = t;
                else if (t.name == "chest") chest = t;
            }
            // Height of the saddle's top above the back line, measured in the rest pose.
            var rs = model.GetComponentsInChildren<Renderer>();
            float top = .8f;
            if (rs.Length > 0) { var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); top = (b.max.y - owner.transform.position.y) * .62f; }
            acima = hips != null && chest != null ? top - (Selim().y - owner.transform.position.y) : top;
        }

        Vector3 Selim() => Vector3.LerpUnclamped(hips.position, chest.position, PontoDaSela);

        void OnEnable() { playing = null; smoothSpeed = 0; if (wolf != null) Tocar("Idle"); }

        void LateUpdate()
        {
            if (hero == null) return;
            // The rider's body can be rebuilt when the equipment changes.
            if (body == null) foreach (var t in hero.GetComponentsInChildren<Transform>()) if (t.name == "Expedicionario") { body = t; break; }
            if (view == null || !view.isActiveAndEnabled) view = hero.GetComponentInChildren<HeroModelView>();
            if (view != null) view.PoseFixa = "Sentado";
            if (body != null)
            {
                if (hips != null && chest != null) body.position = Selim() + Vector3.up * (acima - AlturaDoQuadril);
                else body.localPosition = new Vector3(0, acima - AlturaDoQuadril, 0);
            }
            // Smooth speed and two thresholds, so the wolf does not flicker between walking and running.
            float speed = motor != null ? motor.PlanarVelocity.magnitude : 0;
            smoothSpeed = Mathf.Lerp(smoothSpeed, speed, 1 - Mathf.Exp(-10 * Time.deltaTime));
            string want = playing == "Run" ? (smoothSpeed > 3.5f ? "Run" : smoothSpeed > .3f ? "Walk" : "Idle")
                                           : (smoothSpeed > 5f ? "Run" : smoothSpeed > .4f ? "Walk" : "Idle");
            Tocar(want);
            // The legs keep up with the ground (a bigger wolf takes longer steps).
            if (wolf != null) wolf.speed = want == "Run" ? Mathf.Clamp(smoothSpeed / 8f, .75f, 1.5f) : want == "Walk" ? Mathf.Clamp(smoothSpeed / 3f, .7f, 1.5f) : 1;
        }

        void OnDisable()
        {
            if (body != null) body.localPosition = Vector3.zero;
            if (view != null) view.PoseFixa = null;
            if (wolf != null) wolf.speed = 1;
        }

        void Tocar(string state)
        {
            if (wolf == null || playing == state || !wolf.HasState(0, Animator.StringToHash(state))) return;
            wolf.CrossFadeInFixedTime(state, .25f); playing = state;
        }
    }
}
