using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace ForjaDasMares
{
    // Owns a runtime copy; graphics preferences never mutate the imported pipeline asset.
    public sealed class PresentationQuality : MonoBehaviour
    {
        public const string Preference="CampaignGraphics-v1";
        UniversalRenderPipelineAsset original,copy;Camera sceneCamera;bool mobile;
        public static int Selected=>Mathf.Clamp(PlayerPrefs.GetInt(Preference,1),0,2);
        public static string ModeName(int index)=>index==0?"ECONÔMICO":index==1?"EQUILIBRADO":"NÍTIDO";
        public float RenderScale=>copy!=null?copy.renderScale:1;
        public void Configure(Camera camera)
        {
            sceneCamera=camera;mobile=Application.isMobilePlatform;
            if(copy==null){original=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;if(original!=null){copy=Instantiate(original);copy.name=original.name+" (runtime quality)";QualitySettings.renderPipeline=copy;}}
            Apply();
        }
        public void Cycle(){PlayerPrefs.SetInt(Preference,(Selected+1)%3);PlayerPrefs.Save();Apply();}
        public void Apply()
        {
            if(copy!=null){copy.renderScale=Selected==0?.8f:1;copy.msaaSampleCount=mobile?(Selected==2?4:2):1;}
            if(sceneCamera==null)return;
            sceneCamera.allowMSAA=mobile;
            var data=sceneCamera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;
            // PC uses Deferred: use SMAA, not an ignored MSAA checkbox. Mobile is Forward.
            data.antialiasing=mobile?AntialiasingMode.None:Selected==0?AntialiasingMode.FastApproximateAntialiasing:AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            data.antialiasingQuality=Selected==2?AntialiasingQuality.High:AntialiasingQuality.Medium;
        }
        void OnDestroy(){if(copy!=null){if(QualitySettings.renderPipeline==copy)QualitySettings.renderPipeline=original;Destroy(copy);}}
    }
}
