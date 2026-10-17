using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
namespace ForjaDasMares
{
    // An isolated render layer previews only the visual; no second gameplay actor.
    public sealed class EquipmentPaperDoll : MonoBehaviour
    {
        RawImage image;GameObject world,model;Camera preview;RenderTexture texture;string signature;
        public void Configure(RawImage target)
        {
            image=target;if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;texture=new RenderTexture(256,384,16,RenderTextureFormat.ARGB32);texture.Create();image.texture=texture;
            world=new GameObject("EquipmentPreviewWorld");world.transform.position=new Vector3(-20000,-20000,-20000);
            var cameraObject=new GameObject("PreviewCamera");cameraObject.transform.SetParent(world.transform,false);preview=cameraObject.AddComponent<Camera>();
            preview.targetTexture=texture;preview.cullingMask=1<<31;preview.clearFlags=CameraClearFlags.SolidColor;preview.backgroundColor=new Color(.07f,.09f,.075f,1);preview.orthographic=true;preview.orthographicSize=1.15f;preview.nearClipPlane=.1f;preview.farClipPlane=10;preview.allowHDR=false;preview.allowMSAA=false;
            cameraObject.transform.localPosition=new Vector3(0,1.15f,4);cameraObject.transform.rotation=Quaternion.LookRotation(world.transform.position+Vector3.up-cameraObject.transform.position);
            var lamp=new GameObject("PreviewLight");lamp.transform.SetParent(world.transform,false);lamp.transform.localRotation=Quaternion.Euler(38,-25,0);var light=lamp.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.4f;light.cullingMask=1<<31;light.shadows=LightShadows.None;
            if(Camera.main!=null)Camera.main.cullingMask&=~(1<<31);
        }
        public void SetVisible(bool visible){if(image!=null)image.gameObject.SetActive(visible);if(preview!=null)preview.enabled=visible;}
        public void Refresh(HeroPresentation actor,PlayerProfile profile,string hero)
        {
            if(preview==null||actor==null||actor.Visible==null||!image.gameObject.activeInHierarchy)return;preview.enabled=true;
            string next=hero;var set=profile.Loadout(hero);if(set!=null)foreach(string item in set.items)next+=":"+item;
            if(next==signature&&model!=null)return;signature=next;
            if(model!=null){model.SetActive(false);Destroy(model);}model=Instantiate(actor.Visible.gameObject,world.transform);model.SetActive(true);model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.Euler(0,-18,0);
            foreach(var t in model.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
            foreach(var collider in model.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            foreach(var behavior in model.GetComponentsInChildren<MonoBehaviour>(true))behavior.enabled=false;
            foreach(var renderer in model.GetComponentsInChildren<Renderer>(true)){renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;}
            var animator=model.GetComponentInChildren<Animator>();if(animator!=null){animator.Rebind();animator.Play("Base Layer.Idle");}
        }
        void OnDisable(){if(preview!=null)preview.enabled=false;}
        void OnDestroy(){if(world!=null)Destroy(world);if(texture!=null){texture.Release();Destroy(texture);}}
    }
}
