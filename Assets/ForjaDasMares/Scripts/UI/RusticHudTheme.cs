using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace ForjaDasMares
{
    public static class RusticHudTheme
    {
        public static readonly Color Ink = new Color(.065f,.079f,.067f,.96f), Surface = new Color(.13f,.15f,.12f,.96f);
        public static readonly Color Copper = new Color(.65f,.47f,.28f), Gold = new Color(.91f,.74f,.43f), Parchment = new Color(.95f,.90f,.78f), Muted = new Color(.65f,.70f,.61f);
        public static RectTransform Rect(Transform parent,string name,Vector2 anchor,Vector2 position,Vector2 size)
        { var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=position;r.sizeDelta=size;return r; }
        public static RoundedHudGraphic Panel(RectTransform rect,Color color,float radius=14)
        {
            // Unity permits only one Graphic on each GameObject. Preserve the
            // existing Image references and put the new surface behind its content.
            if (rect.GetComponent<Graphic>() != null)
            {
                var surface=Rect(rect,"RusticSurface",Vector2.zero,Vector2.zero,Vector2.zero);
                surface.anchorMax=Vector2.one;surface.offsetMin=surface.offsetMax=Vector2.zero;surface.SetAsFirstSibling();rect=surface;
            }
            var g=rect.gameObject.AddComponent<RoundedHudGraphic>();g.color=color;g.radius=radius;g.borderColor=Copper;g.raycastTarget=false;return g;
        }
        public static Text Label(Transform parent,string name,Vector2 anchor,Vector2 position,Vector2 size,string value,int font=16,TextAnchor align=TextAnchor.MiddleLeft)
        { var t=Rect(parent,name,anchor,position,size).gameObject.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=font;t.color=Parchment;t.text=value;t.alignment=align;t.raycastTarget=false;t.verticalOverflow=VerticalWrapMode.Truncate;return t; }
        public static HudIcon Icon(Transform parent,HudSymbol symbol,Vector2 anchor,Vector2 position,float size)
        { var icon=Rect(parent,"Icon_"+symbol,anchor,position,Vector2.one*size).gameObject.AddComponent<HudIcon>();icon.symbol=symbol;icon.color=Gold;icon.raycastTarget=false;return icon; }
        public static void Skin(Button button)
        {
            var image=button.GetComponent<Image>();if(image!=null)image.enabled=false;
            var graphic=button.GetComponent<RoundedHudGraphic>()??Panel((RectTransform)button.transform,Surface);
            graphic.raycastTarget=true;button.targetGraphic=graphic;
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.2f,1.15f,1.05f);colors.pressedColor=new Color(.75f,.78f,.68f);colors.selectedColor=colors.highlightedColor;colors.disabledColor=new Color(.6f,.6f,.6f,.5f);colors.fadeDuration=.1f;button.colors=colors;
            foreach(var text in button.GetComponentsInChildren<Text>(true))text.color=Parchment;
        }
        public static Button Button(Transform parent,string name,Vector2 anchor,Vector2 position,Vector2 size,string label,UnityAction action)
        {
            var rect=Rect(parent,name,anchor,position,size);var button=rect.gameObject.AddComponent<Button>();Skin(button);
            Label(rect,"Label",Vector2.one*.5f,Vector2.zero,size-new Vector2(20,0),label,16,TextAnchor.MiddleCenter);
            button.onClick.AddListener(action);return button;
        }
    }
}
