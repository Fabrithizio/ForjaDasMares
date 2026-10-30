using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
namespace ForjaDasMares
{
    // An isolated render layer previews only the visual; no second gameplay actor.
    public sealed class EquipmentPaperDoll : MonoBehaviour
    {
        RawImage image;GameObject world,model;Camera preview;RenderTexture texture;string signature;bool portrait;int dirty;static int sequence;
        public void Configure(RawImage target,bool headOnly=false)
        {
            portrait=headOnly;image=target;if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;texture=new RenderTexture(portrait?192:384,portrait?192:512,16,RenderTextureFormat.ARGB32);texture.filterMode=FilterMode.Bilinear;texture.antiAliasing=portrait?1:4;texture.Create();image.texture=texture;
            world=new GameObject("EquipmentPreviewWorld");world.transform.position=new Vector3(-20000+(++sequence)*20,-20000,-20000);
            var cameraObject=new GameObject("PreviewCamera");cameraObject.transform.SetParent(world.transform,false);preview=cameraObject.AddComponent<Camera>();
            preview.targetTexture=texture;preview.cullingMask=1<<31;preview.clearFlags=CameraClearFlags.SolidColor;preview.backgroundColor=new Color(.07f,.09f,.075f,1);preview.orthographic=true;preview.orthographicSize=portrait ? .45f : 1.15f;preview.nearClipPlane=.1f;preview.farClipPlane=10;preview.allowHDR=false;preview.allowMSAA=true;
            var aa=preview.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();aa.renderPostProcessing=true;aa.antialiasing=UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraObject.transform.localPosition=new Vector3(0,portrait?1.65f:1.15f,4);cameraObject.transform.rotation=Quaternion.LookRotation(world.transform.position+Vector3.up*(portrait?1.65f:1)-cameraObject.transform.position);
            var lamp=new GameObject("PreviewLight");lamp.transform.SetParent(world.transform,false);lamp.transform.localPosition=new Vector3(-1,2,3);var light=lamp.AddComponent<Light>();light.type=LightType.Point;light.range=8;light.intensity=12;light.cullingMask=1<<31;light.shadows=LightShadows.None;
            if(Camera.main!=null)Camera.main.cullingMask&=~(1<<31);
        }
        public void SetBackdrop(Color color)
        {
            if(preview==null)return;preview.backgroundColor=color;
            if(color.a<1){var data=preview.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();data.renderPostProcessing=false;data.antialiasing=UnityEngine.Rendering.Universal.AntialiasingMode.None;
                }
        }
        public void SetVisible(bool visible){if(image!=null)image.gameObject.SetActive(visible);if(world!=null)world.SetActive(visible&&(!portrait||dirty>0));if(preview!=null)preview.enabled=visible&&(!portrait||dirty>0);}
        public void Refresh(HeroPresentation actor,PlayerProfile profile,string hero)
        {
            if(preview==null||actor==null||actor.Visible==null||!image.gameObject.activeInHierarchy)return;
            string next=hero;var set=profile.Loadout(hero);if(set!=null)foreach(string item in set.items)next+=":"+item;
            if(next==signature&&model!=null)return;signature=next;dirty=2;world.SetActive(true);preview.enabled=true;
            if(model!=null){model.SetActive(false);Destroy(model);}model=Instantiate(actor.Visible.gameObject,world.transform);model.SetActive(true);model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.Euler(0,-18,0);
            foreach(var t in model.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
            foreach(var collider in model.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            foreach(var behavior in model.GetComponentsInChildren<MonoBehaviour>(true))behavior.enabled=false;
            foreach(var line in model.GetComponentsInChildren<LineRenderer>(true))line.enabled=false;
            foreach(var effect in model.GetComponentsInChildren<TrailRenderer>(true)){effect.emitting=false;effect.Clear();effect.enabled=false;}
            foreach(var renderer in model.GetComponentsInChildren<Renderer>(true)){renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;}
            if(portrait){var renderers=model.GetComponentsInChildren<Renderer>();Bounds bounds=new Bounds();bool first=true;foreach(var renderer in renderers){if(!renderer.enabled||renderer is ParticleSystemRenderer||renderer is LineRenderer||renderer is TrailRenderer)continue;if(first){bounds=renderer.bounds;first=false;}else bounds.Encapsulate(renderer.bounds);}if(!first){var focus=bounds.center+Vector3.up*bounds.size.y*.28f;preview.orthographicSize=Mathf.Max(.12f,bounds.size.y*.25f);preview.transform.position=focus+Vector3.forward*4;preview.transform.rotation=Quaternion.LookRotation(focus-preview.transform.position);}}
            var animator=model.GetComponentInChildren<Animator>();if(animator!=null){animator.Rebind();animator.Play("Base Layer.Idle");}
        }
        void LateUpdate(){if(portrait&&preview!=null&&preview.enabled&&--dirty<=0){preview.enabled=false;world.SetActive(false);}}
        void OnDisable(){if(preview!=null)preview.enabled=false;if(world!=null)world.SetActive(false);}
        void OnDestroy(){if(world!=null)Destroy(world);if(texture!=null){texture.Release();Destroy(texture);}}
    }
}
