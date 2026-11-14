using UnityEngine;

namespace ForjaDasMares
{
    // Standard size for game windows: about 80% of the screen on PC and 90% on phones, always fitting inside.
    public static class UiWindowScale
    {
        public static float For(RectTransform parent, Vector2 designSize)
        {
            float share = Application.isMobilePlatform ? .92f : .8f;
            Vector2 area = parent.rect.size;
            if (area.x <= 0 || area.y <= 0 || designSize.x <= 0 || designSize.y <= 0) return 1f;
            return Mathf.Min(share * area.x / designSize.x, share * area.y / designSize.y);
        }
    }
}
