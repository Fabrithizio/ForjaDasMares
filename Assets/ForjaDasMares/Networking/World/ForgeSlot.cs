using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // One square slot with an item picture (mochila or Forja). Supports click, drag and drop, and hover.
    public sealed class ForgeSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public RoundedHudGraphic frame;
        public RawImage picture;
        public Text count, badge, caption;
        public object content;
        public bool fromBag;
        public Action<ForgeSlot> clicked, hovered, unhovered;
        public Action<ForgeSlot, ForgeSlot> dropped; // (source, target)
        public RectTransform dragLayer;
        RawImage ghost;

        public RectTransform Rect => (RectTransform)transform;
        public bool HasContent => content != null;

        public static ForgeSlot Create(Transform parent, string name, Vector2 position, float size, string label = null)
        {
            var rect = RusticHudTheme.Rect(parent, name, new Vector2(0, 1), position, Vector2.one * size);
            var slot = rect.gameObject.AddComponent<ForgeSlot>();
            slot.frame = rect.gameObject.AddComponent<RoundedHudGraphic>();
            slot.frame.radius = size * .14f;
            slot.frame.border = 2f;
            slot.frame.gradient = true;
            slot.frame.raycastTarget = true;
            var art = RusticHudTheme.Rect(rect, "Arte", Vector2.one * .5f, Vector2.zero, Vector2.one * (size - 12));
            slot.picture = art.gameObject.AddComponent<RawImage>();
            slot.picture.raycastTarget = false;
            slot.count = RusticHudTheme.Label(rect, "Quantidade", new Vector2(1, 0), new Vector2(-4, 2), new Vector2(size * .7f, 22), "", 15, TextAnchor.LowerRight);
            slot.count.fontStyle = FontStyle.Bold;
            slot.badge = RusticHudTheme.Label(rect, "Nivel", new Vector2(0, 1), new Vector2(5, -3), new Vector2(size * .7f, 22), "", 15, TextAnchor.UpperLeft);
            slot.badge.fontStyle = FontStyle.Bold;
            slot.badge.color = RusticHudTheme.Gold;
            foreach (var text in new[] { slot.count, slot.badge })
            {
                var outline = text.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0, 0, 0, .9f);
                outline.effectDistance = new Vector2(1.2f, -1.2f);
            }
            if (!string.IsNullOrEmpty(label))
            {
                slot.caption = RusticHudTheme.Label(rect, "Legenda", new Vector2(.5f, 0), new Vector2(0, -24), new Vector2(size + 40, 22), label, 14, TextAnchor.MiddleCenter);
                slot.caption.color = RusticHudTheme.Muted;
            }
            slot.SetEmpty(new Color(.55f, .45f, .3f, .55f));
            return slot;
        }

        public void SetEmpty(Color border)
        {
            content = null;
            picture.texture = null;
            picture.enabled = false;
            count.text = badge.text = "";
            Paint(border, false);
        }

        public void Show(object value, Texture texture, Rect uv, Color border, string amount = "", string level = "", bool dim = false)
        {
            content = value;
            picture.enabled = texture != null;
            picture.texture = texture;
            picture.uvRect = uv;
            picture.color = dim ? new Color(1, 1, 1, .35f) : Color.white;
            count.text = amount;
            badge.text = level;
            Paint(border, true);
        }

        void Paint(Color border, bool filled)
        {
            frame.borderColor = border;
            frame.color = filled ? new Color(.035f, .085f, .1f, .97f) : new Color(.02f, .05f, .06f, .9f);
            frame.topColor = filled ? new Color(.07f, .16f, .18f, .97f) : new Color(.035f, .08f, .09f, .9f);
            frame.SetVerticesDirty();
        }

        public void OnPointerClick(PointerEventData eventData) { if (eventData.dragging) return; clicked?.Invoke(this); }
        public void OnPointerEnter(PointerEventData eventData) { if (HasContent) hovered?.Invoke(this); }
        public void OnPointerExit(PointerEventData eventData) => unhovered?.Invoke(this);

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!HasContent || picture.texture == null || dragLayer == null) return;
            float ratio = dragLayer.lossyScale.x > 0 ? Rect.lossyScale.x / dragLayer.lossyScale.x : 1f;
            var ghostRect = RusticHudTheme.Rect(dragLayer, "Arrastando", dragLayer.pivot, Vector2.zero, Rect.sizeDelta * .9f * ratio);
            ghostRect.pivot = Vector2.one * .5f;
            ghostRect.SetAsLastSibling();
            ghost = ghostRect.gameObject.AddComponent<RawImage>();
            ghost.texture = picture.texture;
            ghost.uvRect = picture.uvRect;
            ghost.color = new Color(1, 1, 1, .85f);
            ghost.raycastTarget = false;
            Follow(eventData);
        }

        public void OnDrag(PointerEventData eventData) { if (ghost != null) Follow(eventData); }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (ghost != null) Destroy(ghost.gameObject);
            ghost = null;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var source = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<ForgeSlot>() : null;
            if (source != null && source != this && source.HasContent) dropped?.Invoke(source, this);
        }

        void Follow(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, eventData.position, eventData.pressEventCamera, out Vector2 local))
                ((RectTransform)ghost.transform).anchoredPosition = local;
        }

        void OnDisable()
        {
            if (ghost != null) Destroy(ghost.gameObject);
            ghost = null;
        }

        public static Rect FullUv => new Rect(0, 0, 1, 1);

        public static Rect SpriteUv(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return FullUv;
            var r = sprite.textureRect;
            return new Rect(r.x / sprite.texture.width, r.y / sprite.texture.height, r.width / sprite.texture.width, r.height / sprite.texture.height);
        }
    }
}
