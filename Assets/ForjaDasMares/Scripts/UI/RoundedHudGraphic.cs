using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares
{
    // Original vector geometry: no external textures or runtime material copies.
    public sealed class RoundedHudGraphic : MaskableGraphic
    {
        public float radius = 14f, border = 1.5f;
        public Color borderColor = new Color(.49f, .38f, .23f, 1);
        const int Segments = 32;
        Vector2 Point(Rect rect, float inset, int index)
        {
            float r = Mathf.Max(0, Mathf.Min(radius - inset, Mathf.Min(rect.width, rect.height) * .5f - inset));
            int corner = index / 8; float angle = (corner * 90 + index % 8 * 90f / 7) * Mathf.Deg2Rad;
            Vector2 center = new Vector2(corner == 0 || corner == 3 ? rect.xMax - inset - r : rect.xMin + inset + r,
                corner < 2 ? rect.yMax - inset - r : rect.yMin + inset + r);
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); Rect rect = rectTransform.rect;
            if (rect.width <= 0 || rect.height <= 0) return;
            float inset = Mathf.Clamp(border, 0, Mathf.Min(rect.width, rect.height) * .49f);
            Color edge = borderColor; edge.a *= color.a;
            for (int i = 0; i < Segments; i++) vh.AddVert(Point(rect, 0, i), edge, Vector2.zero);
            for (int i = 0; i < Segments; i++) vh.AddVert(Point(rect, inset, i), color, Vector2.zero);
            vh.AddVert(rect.center, color, Vector2.zero);
            for (int i = 0; i < Segments; i++)
            {
                int next = (i + 1) % Segments;
                vh.AddTriangle(i, next, Segments + i); vh.AddTriangle(next, Segments + next, Segments + i);
                vh.AddTriangle(Segments + i, Segments + next, Segments * 2);
            }
        }
    }
}
