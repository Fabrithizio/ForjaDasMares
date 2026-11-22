using UnityEngine;
using UnityEngine.EventSystems;
namespace ForjaDasMares
{
    // Phone ATAQUE button. A quick tap locks the nearest creature on the screen and the hero keeps attacking it.
    // Holding (or dragging) shows a small target on the ground: the button works like a joystick to move it
    // onto any creature on the screen; letting go locks that one. One finger only; losing focus cancels.
    public sealed class HoldAttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, ISubmitHandler
    {
        const float HoldTime = .22f, DragStart = 18f, StickRadius = 110f;
        ArenaInput input;
        int pointer = int.MinValue;
        Vector2 start, current; float downAt; bool aiming;
        Transform ring; LineRenderer line; static readonly Color Red = new Color(1f, .32f, .25f);
        public void Configure(ArenaInput source) => input = source;
        DesktopRpgControls Controls => input != null ? input.Desktop : null;

        public void OnPointerDown(PointerEventData data)
        {
            if (pointer != int.MinValue || input == null || input.CombatBlocked) return;
            pointer = data.pointerId; start = current = data.position; downAt = Time.unscaledTime; aiming = false;
        }
        public void OnDrag(PointerEventData data) { if (data.pointerId == pointer) current = data.position; }
        public void OnPointerUp(PointerEventData data)
        {
            if (data.pointerId != pointer) return;
            var controls = Controls;
            if (controls != null) { if (aiming) controls.SoltarArrasto(); else controls.ToqueAtaque(); }
            else input?.RequestAttack();
            Release();
        }
        public void OnSubmit(BaseEventData data) { if (Controls != null) Controls.ToqueAtaque(); else input?.RequestAttack(); }

        void Update()
        {
            if (pointer == int.MinValue) { Show(false); return; }
            float scale = Mathf.Max(.5f, Screen.height / 1080f);
            if (!aiming && (Time.unscaledTime - downAt > HoldTime || (current - start).magnitude > DragStart * scale)) aiming = true;
            if (!aiming || Controls == null) return;
            Controls.MirarComArrasto((current - start) / (StickRadius * scale));
            var target = Controls.Mirando;
            Show(true, target != null ? target.transform.position : Controls.PontoDaMira, target != null);
        }

        void Show(bool on, Vector3 at = default, bool locked = false)
        {
            if (!on) { if (ring != null && ring.gameObject.activeSelf) ring.gameObject.SetActive(false); return; }
            if (ring == null)
            {
                ring = new GameObject("MiraDoCelular").transform;
                line = ring.gameObject.AddComponent<LineRenderer>();
                line.useWorldSpace = false; line.loop = true; line.positionCount = 32; line.widthMultiplier = .09f;
                line.sharedMaterial = EfeitosDeHabilidade.Brilho; line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                for (int i = 0; i < 32; i++) { float a = i * Mathf.PI * 2 / 32; line.SetPosition(i, new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a))); }
            }
            ring.gameObject.SetActive(true);
            ring.position = at + Vector3.up * .08f;
            float pulse = 1 + Mathf.Sin(Time.unscaledTime * 10) * .08f;
            ring.localScale = Vector3.one * (locked ? 1.1f : .7f) * pulse;
            var c = locked ? Red : new Color(1f, .85f, .6f, .8f); line.startColor = line.endColor = c;
        }

        void Release()
        {
            if (aiming && Controls != null && pointer != int.MinValue && Controls.MirandoAgora) Controls.SoltarArrasto();
            pointer = int.MinValue; aiming = false; Show(false);
        }
        void OnDisable() { Release(); }
        void OnDestroy() { if (ring != null) Destroy(ring.gameObject); }
        void OnApplicationFocus(bool focus) { if (!focus) Release(); }
    }
}
