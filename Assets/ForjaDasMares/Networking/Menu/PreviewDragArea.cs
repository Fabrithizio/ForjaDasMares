using UnityEngine;
using UnityEngine.EventSystems;

namespace ForjaDasMares.Networking
{
    // Picture of the 3D character on the menu: drag to turn it, mouse wheel to zoom (two fingers on the phone are read by StartMenu).
    public sealed class PreviewDragArea : MonoBehaviour, IDragHandler, IScrollHandler
    {
        public System.Action<float> Dragged, Scrolled;

        public void OnDrag(PointerEventData eventData)
        {
            var screen = UnityEngine.InputSystem.Touchscreen.current;
            int fingers = 0;
            if (screen != null) foreach (var touch in screen.touches) if (touch.press.isPressed) fingers++;
            if (fingers >= 2) return;
            Dragged?.Invoke(eventData.delta.x);
        }

        public void OnScroll(PointerEventData eventData)
        {
            float y = eventData.scrollDelta.y;
            if (Mathf.Abs(y) > .01f) Scrolled?.Invoke(Mathf.Sign(y));
        }
    }
}
