using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares
{
    // Original vector geometry: no external textures or runtime material copies.
    public sealed class RoundedHudGraphic : MaskableGraphic
    {
        public float radius = 14f, border = 1.5f;
        public Color borderColor = new Color(.49f, .38f, .23f, 1);
        const int CornerSegments = 16, Segments = CornerSegments * 4;
        Vector2 Point(Rect rect, float inset, int index)
        {
            float r = Mathf.Max(0, Mathf.Min(radius - inset, Mathf.Min(rect.width, rect.height) * .5f - inset));
            int corner = index / CornerSegments; float angle = (corner * 90 + index % CornerSegments * 90f / (CornerSegments-1)) * Mathf.Deg2Rad;
            Vector2 center = new Vector2(corner == 0 || corner == 3 ? rect.xMax - inset - r : rect.xMin + inset + r,
                corner < 2 ? rect.yMax - inset - r : rect.yMin + inset + r);
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); Rect rect = rectTransform.rect;
            if (rect.width <= 0 || rect.height <= 0) return;
            float inset = Mathf.Clamp(border, 0, Mathf.Min(rect.width, rect.height) * .49f);
            float feather=1f/Mathf.Max(.25f,canvas!=null?canvas.scaleFactor:1);
            Color edge=borderColor;edge.a*=color.a;Color transparent=edge;transparent.a=0;
            if(inset<=0)edge=color;
            // One physical pixel of alpha coverage at both hard boundaries. Overlay UI
            // is not affected by the scene camera's MSAA/post-processing.
            for(int ring=0;ring<4;ring++)
            {
                float offset=ring==0?-feather:ring==1?0:ring==2?Mathf.Max(0,inset-feather*.5f):inset+feather*.5f;
                Color shade=ring==0?transparent:ring<3?edge:color;
                for(int i=0;i<Segments;i++)vh.AddVert(Point(rect,offset,i),shade,Vector2.zero);
            }
            vh.AddVert(rect.center,color,Vector2.zero);
            for(int i=0;i<Segments;i++)
            {
                int next=(i+1)%Segments;
                for(int ring=0;ring<3;ring++){int a=ring*Segments,b=a+Segments;vh.AddTriangle(a+i,a+next,b+i);vh.AddTriangle(a+next,b+next,b+i);}
                vh.AddTriangle(3*Segments+i,3*Segments+next,4*Segments);
            }
        }
    }
}
