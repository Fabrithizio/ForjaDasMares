using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares
{
    public static class HealthBarGeometry
    {
        // A sprite-less Image ignores Filled mode. Resize an actual left-anchored quad instead.
        public static float Initialize(Image image)
        {
            var rect = image.rectTransform; float width = rect.sizeDelta.x;
            image.type = Image.Type.Simple; image.raycastTarget = false;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 0.5f); rect.anchoredPosition = Vector2.zero;
            return width;
        }
        public static void Set(Image image, int current, int maximum, float fullWidth)
        {
            float fraction = maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0;
            image.fillAmount = fraction; Vector2 size = image.rectTransform.sizeDelta; size.x = fullWidth * fraction; image.rectTransform.sizeDelta = size;
        }
    }
}
