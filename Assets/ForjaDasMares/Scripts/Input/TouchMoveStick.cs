using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ForjaDasMares
{
    // Stable capture zone; the base floats only when a gesture starts, never while dragging.
    public sealed class TouchMoveStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IInitializePotentialDragHandler
    {
        [SerializeField] ArenaInput input;
        [SerializeField] RectTransform handle;
        [SerializeField, Min(1)] float radius = 65f;
        [SerializeField, Range(0, 0.5f)] float deadzone = 0.1f;
        [Header("Limited floating area (relative to the touch panel)")]
        [SerializeField] bool floating = true;
        [SerializeField] Vector2 activationMin = new Vector2(0, 0);
        [SerializeField] Vector2 activationMax = new Vector2(.42f, .45f);
        [SerializeField] Vector2 edgePadding = new Vector2(16, 24);
        [SerializeField, Range(0, 1)] float idleOpacity = .35f;
        RectTransform area;
        RectTransform activationArea, parent;
        CanvasGroup visuals;
        Vector2 pressOrigin, heldCentre, homePosition, parentSize;
        int pointerId = int.MinValue;
        public float Radius => radius;
        public bool IsHeld => pointerId != int.MinValue;
        public RectTransform ActivationArea => activationArea;
        public void Configure(ArenaInput reader, RectTransform thumb) { input = reader; handle = thumb; }
        void Awake() { area = (RectTransform)transform; }
        void Start() { EnsureFloatingArea(); }
        void OnEnable() { if (activationArea != null) activationArea.gameObject.SetActive(true); }
        void EnsureFloatingArea()
        {
            if (!floating || activationArea != null) return;
            if (area == null) area = (RectTransform)transform;
            parent = area.parent as RectTransform;
            if (parent == null || parent.rect.width <= 0 || parent.rect.height <= 0) return;
            homePosition = area.anchoredPosition;
            var zone = new GameObject(name + "_FloatingArea", typeof(RectTransform), typeof(Image), typeof(FloatingStickArea));
            activationArea = (RectTransform)zone.transform;
            activationArea.SetParent(parent, false); activationArea.SetAsFirstSibling();
            activationArea.anchorMin = new Vector2(Mathf.Clamp(activationMin.x, 0, .3f), Mathf.Clamp(activationMin.y, 0, .2f));
            activationArea.anchorMax = new Vector2(Mathf.Clamp(activationMax.x, .3f, .48f), Mathf.Clamp(activationMax.y, .3f, .5f));
            activationArea.offsetMin = edgePadding; activationArea.offsetMax = -edgePadding;
            var image = zone.GetComponent<Image>(); image.color = Color.clear; image.raycastTarget = true;
            zone.GetComponent<FloatingStickArea>().Configure(this);
            visuals = GetComponent<CanvasGroup>();
            if (visuals == null) visuals = gameObject.AddComponent<CanvasGroup>();
            visuals.blocksRaycasts = false; visuals.alpha = idleOpacity;
        }
        public void OnPointerDown(PointerEventData data)
        {
            if (!isActiveAndEnabled || IsHeld) return;
            EnsureFloatingArea();
            if (activationArea != null)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(activationArea, data.position, data.pressEventCamera) ||
                    !RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, data.position, data.pressEventCamera, out pressOrigin)) return;
                var rect = activationArea.rect;
                Vector2 min = parent.InverseTransformPoint(activationArea.TransformPoint(rect.min));
                Vector2 max = parent.InverseTransformPoint(activationArea.TransformPoint(rect.max));
                Vector2 margin = area.rect.size * .5f;
                heldCentre = new Vector2(ClampCentre(pressOrigin.x, min.x, max.x, margin.x), ClampCentre(pressOrigin.y, min.y, max.y, margin.y));
                parentSize = parent.rect.size;
                area.position = parent.TransformPoint(heldCentre);
                visuals.alpha = 1;
            }
            pointerId = data.pointerId; Move(data);
        }
        static float ClampCentre(float value, float min, float max, float margin)
        { return max - min <= margin * 2 ? (min + max) * .5f : Mathf.Clamp(value, min + margin, max - margin); }
        public void OnDrag(PointerEventData data) { if (data.pointerId == pointerId) Move(data); }
        public void OnInitializePotentialDrag(PointerEventData data)=>data.useDragThreshold=false;
        public void OnPointerUp(PointerEventData data) { if (data.pointerId == pointerId) Release(); }
        void Move(PointerEventData data)
        {
            if (area == null) area = (RectTransform)transform;
            var reference = activationArea != null ? parent : area;
            if (input == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(reference, data.position, data.pressEventCamera, out Vector2 local)) return;
            // Pressing near an edge must not move the hero before the finger is dragged.
            if (activationArea != null) local -= pressOrigin;
            Vector2 normalized = Vector2.ClampMagnitude(local / radius, 1f);
            if (handle != null) handle.anchoredPosition = normalized * radius;
            float magnitude = normalized.magnitude;
            Vector2 value = magnitude <= deadzone ? Vector2.zero : normalized.normalized * ((magnitude - deadzone) / (1f - deadzone));
            input.SetTouchMove(value, true);
        }
        public void Release()
        {
            pointerId = int.MinValue;
            if (input != null) input.SetTouchMove(Vector2.zero, false);
            if (handle != null) handle.anchoredPosition = Vector2.zero;
            if (activationArea != null && area != null) area.anchoredPosition = homePosition;
            if (visuals != null) visuals.alpha = idleOpacity;
        }
        void LateUpdate()
        {
            if (!IsHeld || activationArea == null) return;
            if (parent.rect.size != parentSize) { Release(); return; }
            area.position = parent.TransformPoint(heldCentre);
        }
        void OnDisable() { Release(); if (activationArea != null) activationArea.gameObject.SetActive(false); }
        void OnDestroy() { if (activationArea != null) Destroy(activationArea.gameObject); }
        void OnApplicationFocus(bool focused) { if (!focused) Release(); }
        void OnApplicationPause(bool paused) { if (paused) Release(); }
    }
}

