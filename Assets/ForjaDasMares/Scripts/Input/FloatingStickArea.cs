using UnityEngine;
using UnityEngine.EventSystems;

namespace ForjaDasMares
{
    // This static, transparent hit area never follows the visual base or captures right-side buttons.
    public sealed class FloatingStickArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IInitializePotentialDragHandler
    {
        TouchMoveStick owner;
        public void Configure(TouchMoveStick stick) => owner = stick;
        public void OnPointerDown(PointerEventData data) { if (owner != null) owner.OnPointerDown(data); }
        public void OnDrag(PointerEventData data) { if (owner != null) owner.OnDrag(data); }
        public void OnPointerUp(PointerEventData data) { if (owner != null) owner.OnPointerUp(data); }
        public void OnInitializePotentialDrag(PointerEventData data) => data.useDragThreshold = false;
        void OnDisable() { if (owner != null) owner.Release(); }
    }
}
