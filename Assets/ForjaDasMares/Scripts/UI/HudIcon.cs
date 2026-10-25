using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares
{
    public enum HudSymbol { Sword, Rune, Dodge, Bag, Menu, Hero, Interact, Compass, Fury, Shield, Armor, Accessory, Artifact, Heart, Target, Hourglass }
    public sealed class HudIcon : MaskableGraphic
    {
        public HudSymbol symbol;
        void Line(VertexHelper vh, Vector2 a, Vector2 b, float width = 2.2f)
        {
            Rect rect = rectTransform.rect; float scale = Mathf.Min(rect.width, rect.height) / 32;
            a = rect.center + a * scale; b = rect.center + b * scale;
            Vector2 direction=(b-a).normalized,perpendicular=new Vector2(-direction.y,direction.x);
            float half=width*scale*.5f,feather=1f/Mathf.Max(.25f,canvas!=null?canvas.scaleFactor:1);
            Vector2 normal=perpendicular*half,outer=perpendicular*(half+feather),cap=direction*feather;
            int start=vh.currentVertCount;Color transparent=color;transparent.a=0;
            vh.AddVert(a+outer-cap,transparent,Vector2.zero);vh.AddVert(b+outer+cap,transparent,Vector2.zero);
            vh.AddVert(b-outer+cap,transparent,Vector2.zero);vh.AddVert(a-outer-cap,transparent,Vector2.zero);
            vh.AddVert(a+normal,color,Vector2.zero);vh.AddVert(b+normal,color,Vector2.zero);
            vh.AddVert(b-normal,color,Vector2.zero);vh.AddVert(a-normal,color,Vector2.zero);
            for(int i=0;i<4;i++){int next=(i+1)%4;vh.AddTriangle(start+i,start+next,start+4+i);vh.AddTriangle(start+next,start+4+next,start+4+i);}
            vh.AddTriangle(start+4,start+5,start+6);vh.AddTriangle(start+4,start+6,start+7);
        }
        void Ring(VertexHelper vh, float radius)
        { for (int i = 0; i < 24; i++) { float a = i * Mathf.PI / 12, b = (i + 1) * Mathf.PI / 12; Line(vh, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, 1.5f); } }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            switch (symbol)
            {
                case HudSymbol.Heart:
                    Line(vh,new Vector2(0,-12),new Vector2(-11,0));Line(vh,new Vector2(-11,0),new Vector2(-11,7));Line(vh,new Vector2(-11,7),new Vector2(-6,11));Line(vh,new Vector2(-6,11),new Vector2(0,6));Line(vh,new Vector2(0,6),new Vector2(6,11));Line(vh,new Vector2(6,11),new Vector2(11,7));Line(vh,new Vector2(11,7),new Vector2(11,0));Line(vh,new Vector2(11,0),new Vector2(0,-12));break;
                case HudSymbol.Target:
                    Ring(vh,10);Ring(vh,4);Line(vh,new Vector2(-14,0),new Vector2(-7,0));Line(vh,new Vector2(7,0),new Vector2(14,0));Line(vh,new Vector2(0,7),new Vector2(0,14));Line(vh,new Vector2(0,-14),new Vector2(0,-7));break;
                case HudSymbol.Hourglass:
                    Line(vh,new Vector2(-10,12),new Vector2(10,12));Line(vh,new Vector2(-10,-12),new Vector2(10,-12));Line(vh,new Vector2(-8,10),new Vector2(8,-10));Line(vh,new Vector2(8,10),new Vector2(-8,-10));Line(vh,new Vector2(-8,-8),new Vector2(8,-8));break;
                case HudSymbol.Shield:
                    Line(vh,new Vector2(-11,10),new Vector2(11,10));Line(vh,new Vector2(11,10),new Vector2(8,-5));Line(vh,new Vector2(8,-5),new Vector2(0,-13));Line(vh,new Vector2(0,-13),new Vector2(-8,-5));Line(vh,new Vector2(-8,-5),new Vector2(-11,10));Line(vh,new Vector2(0,7),new Vector2(0,-9));break;
                case HudSymbol.Armor:
                    Line(vh,new Vector2(-5,11),new Vector2(-13,6));Line(vh,new Vector2(-13,6),new Vector2(-8,0));Line(vh,new Vector2(-8,0),new Vector2(-7,-12));Line(vh,new Vector2(-7,-12),new Vector2(7,-12));Line(vh,new Vector2(7,-12),new Vector2(8,0));Line(vh,new Vector2(8,0),new Vector2(13,6));Line(vh,new Vector2(13,6),new Vector2(5,11));Line(vh,new Vector2(5,11),new Vector2(0,7));Line(vh,new Vector2(0,7),new Vector2(-5,11));Line(vh,new Vector2(-7,-4),new Vector2(7,-4));break;
                case HudSymbol.Accessory:
                    Ring(vh,10);Line(vh,new Vector2(-5,9),new Vector2(0,14));Line(vh,new Vector2(0,14),new Vector2(5,9));break;
                case HudSymbol.Artifact:
                    Line(vh,new Vector2(0,14),new Vector2(-9,1));Line(vh,new Vector2(-9,1),new Vector2(0,-14));Line(vh,new Vector2(0,-14),new Vector2(9,1));Line(vh,new Vector2(9,1),new Vector2(0,14));Line(vh,new Vector2(0,14),new Vector2(0,-14));Line(vh,new Vector2(-9,1),new Vector2(9,1));break;
                case HudSymbol.Fury:
                    Line(vh,new Vector2(0,-12),new Vector2(-9,-5),3);Line(vh,new Vector2(-9,-5),new Vector2(-7,6),3);Line(vh,new Vector2(-7,6),new Vector2(-2,2),3);Line(vh,new Vector2(-2,2),new Vector2(1,13),3);Line(vh,new Vector2(1,13),new Vector2(9,0),3);Line(vh,new Vector2(9,0),new Vector2(8,-6),3);Line(vh,new Vector2(8,-6),new Vector2(0,-12),3);Line(vh,new Vector2(0,-5),new Vector2(1,3),3);break;
                case HudSymbol.Sword:
                    Line(vh, new Vector2(-10,-11), new Vector2(10,11),3); Line(vh,new Vector2(-8,-1),new Vector2(1,-9));
                    Line(vh,new Vector2(2,9),new Vector2(10,11)); Line(vh,new Vector2(10,11),new Vector2(9,3)); break;
                case HudSymbol.Rune:
                    Line(vh,new Vector2(4,13),new Vector2(-7,1),3); Line(vh,new Vector2(-7,1),new Vector2(6,1),3);
                    Line(vh,new Vector2(6,1),new Vector2(-4,-13),3); break;
                case HudSymbol.Dodge:
                    for(int i=0;i<2;i++){float x=i*11-7;Line(vh,new Vector2(x-5,-10),new Vector2(x+4,0),3);Line(vh,new Vector2(x+4,0),new Vector2(x-5,10),3);}break;
                case HudSymbol.Bag:
                    Line(vh,new Vector2(-10,-11),new Vector2(10,-11));Line(vh,new Vector2(-10,-11),new Vector2(-9,7));Line(vh,new Vector2(10,-11),new Vector2(9,7));
                    Line(vh,new Vector2(-9,7),new Vector2(9,7));Line(vh,new Vector2(-5,7),new Vector2(-4,12));Line(vh,new Vector2(-4,12),new Vector2(4,12));Line(vh,new Vector2(4,12),new Vector2(5,7));Line(vh,new Vector2(-7,0),new Vector2(7,0));break;
                case HudSymbol.Hero:
                    Line(vh,new Vector2(-10,8),new Vector2(0,13));Line(vh,new Vector2(0,13),new Vector2(10,8));Line(vh,new Vector2(10,8),new Vector2(8,-8));
                    Line(vh,new Vector2(8,-8),new Vector2(0,-13));Line(vh,new Vector2(0,-13),new Vector2(-8,-8));Line(vh,new Vector2(-8,-8),new Vector2(-10,8));Line(vh,new Vector2(-6,2),new Vector2(6,2));Line(vh,new Vector2(0,2),new Vector2(0,-7));break;
                case HudSymbol.Menu:
                    for(int i=-1;i<=1;i++)Line(vh,new Vector2(-10,i*7),new Vector2(10,i*7),2.8f);break;
                case HudSymbol.Interact:
                    Ring(vh,11);Line(vh,new Vector2(0,-5),new Vector2(0,5));Line(vh,new Vector2(-5,0),new Vector2(5,0));break;
                case HudSymbol.Compass:
                    Ring(vh,12);Line(vh,new Vector2(5,7),new Vector2(-5,-7));Line(vh,new Vector2(-5,-7),new Vector2(-2,5));Line(vh,new Vector2(-2,5),new Vector2(5,7));break;
            }
        }
    }
}
