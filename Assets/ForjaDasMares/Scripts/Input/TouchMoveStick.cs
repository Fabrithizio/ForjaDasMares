using UnityEngine;
using UnityEngine.EventSystems;

namespace ForjaDasMares
{
    // Stable hit area: only the visual handle moves. The captured finger owns movement.
    public sealed class TouchMoveStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IInitializePotentialDragHandler
    {
        [SerializeField] ArenaInput input;
        [SerializeField] RectTransform handle;
        [SerializeField, Min(1)] float radius = 65f;
        [SerializeField, Range(0, 0.5f)] float deadzone = 0.1f;
        RectTransform area;
        int pointerId = int.MinValue;
        public float Radius => radius;
        public void Configure(ArenaInput reader, RectTransform thumb) { input = reader; handle = thumb; }
        void Awake() { area = (RectTransform)transform; }
        public void OnPointerDown(PointerEventData data)
        { if (pointerId != int.MinValue) return; pointerId = data.pointerId; Move(data); }
        public void OnDrag(PointerEventData data) { if (data.pointerId == pointerId) Move(data); }
        public void OnInitializePotentialDrag(PointerEventData data)=>data.useDragThreshold=false;
        public void OnPointerUp(PointerEventData data) { if (data.pointerId == pointerId) Release(); }
        void Move(PointerEventData data)
        {
            if (area == null) area = (RectTransform)transform;
            if (input == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(area, data.position, data.pressEventCamera, out Vector2 local)) return;
            Vector2 normalized = Vector2.ClampMagnitude(local / radius, 1f);
            if (handle != null) handle.anchoredPosition = normalized * radius;
            float magnitude = normalized.magnitude;
            Vector2 value = magnitude <= deadzone ? Vector2.zero : normalized.normalized * ((magnitude - deadzone) / (1f - deadzone));
            input.SetTouchMove(value, true);
        }
        public void Release()
        { pointerId = int.MinValue; if (input != null) input.SetTouchMove(Vector2.zero, false); if (handle != null) handle.anchoredPosition = Vector2.zero; }
        void OnDisable() => Release();
        void OnApplicationFocus(bool focused) { if (!focused) Release(); }
    }
}
