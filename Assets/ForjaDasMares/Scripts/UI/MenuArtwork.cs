using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares
{
    // Original raster key art, cropped to fill without stretching. Never intercepts clicks.
    public sealed class MenuArtwork : RawImage
    {
        public void Configure(string resource, Color tint)
        { texture=Resources.Load<Texture2D>(resource);color=tint;raycastTarget=false;Crop(); }
        protected override void OnRectTransformDimensionsChange(){base.OnRectTransformDimensionsChange();Crop();}
        void Crop()
        {
            if(texture==null||rectTransform.rect.height<=0)return;
            float imageAspect=(float)texture.width/texture.height,viewAspect=rectTransform.rect.width/rectTransform.rect.height;
            uvRect=viewAspect>imageAspect?new Rect(0,(1-imageAspect/viewAspect)*.5f,1,imageAspect/viewAspect):new Rect((1-viewAspect/imageAspect)*.5f,0,viewAspect/imageAspect,1);
        }
        public static MenuArtwork Fill(Transform parent,string name,string resource,Color tint)
        {var rect=RusticHudTheme.Rect(parent,name,Vector2.zero,Vector2.zero,Vector2.zero);rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;var image=rect.gameObject.AddComponent<MenuArtwork>();image.Configure(resource,tint);return image;}
    }
}
