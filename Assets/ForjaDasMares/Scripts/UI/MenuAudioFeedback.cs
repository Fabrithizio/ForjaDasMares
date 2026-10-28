using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ForjaDasMares
{
    // A single short UI voice per scene; mouse, touch click and keyboard submit share it.
    public sealed class MenuAudioFeedback:MonoBehaviour,IPointerEnterHandler,ISelectHandler
    {
        Button button;
        void Awake(){button=GetComponent<Button>();button.onClick.AddListener(Click);}
        void OnDestroy(){if(button!=null)button.onClick.RemoveListener(Click);}
        void Click(){if(button!=null&&button.IsInteractable())MenuAudioVoice.Play(true);}
        public void OnPointerEnter(PointerEventData e){Hover();}
        public void OnSelect(BaseEventData e){Hover();}
        void Hover(){if(button!=null&&button.IsInteractable())MenuAudioVoice.Play(false);}
    }
    public sealed class MenuAudioVoice:MonoBehaviour
    {
        static MenuAudioVoice instance;AudioSource source;AudioClip hover,click;float lastHover=-1;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){instance=null;}
        void Awake(){source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.ignoreListenerPause=true;hover=Tone("Menu hover",660,.045f);click=Tone("Menu click",440,.085f);}
        static AudioClip Tone(string name,float hz,float duration)
        {
            const int rate=22050;var data=new float[Mathf.CeilToInt(rate*duration)];
            for(int i=0;i<data.Length;i++){float t=(float)i/rate;data[i]=.28f*Mathf.Sin(2*Mathf.PI*hz*t)*Mathf.Sin(Mathf.PI*i/(data.Length-1))*Mathf.Exp(-t*35);}
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        public static void Play(bool pressed)
        {
            float volume=Mathf.Clamp01(PlayerPrefs.GetFloat("CampaignEffectsVolume",.8f));if(volume<=0)return;
            if(instance==null)instance=new GameObject("MenuAudio_OneVoice").AddComponent<MenuAudioVoice>();
            if(!pressed&&Time.unscaledTime-instance.lastHover<.09f)return;
            instance.lastHover=Time.unscaledTime;instance.source.Stop();instance.source.clip=pressed?instance.click:instance.hover;instance.source.volume=volume*(pressed?.22f:.10f);instance.source.Play();
        }
        void OnDestroy(){if(instance==this)instance=null;if(hover!=null)Destroy(hover);if(click!=null)Destroy(click);}
    }
}
