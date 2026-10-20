using UnityEngine;
namespace ForjaDasMares
{
    // Presentation only. Eight persistent voices per scene, never one object per hit.
    public sealed class CombatAudioPool:MonoBehaviour
    {
        static CombatAudioPool instance;readonly AudioSource[] voices=new AudioSource[8];int next;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset()=>instance=null;
        void Awake(){for(int i=0;i<voices.Length;i++){var source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;voices[i]=source;}}
        public static void Play(AudioClip clip,float volume,float pitch=1)
        {
            if(clip==null||PlayerPrefs.GetFloat("CampaignEffectsVolume",.8f)<=0)return;
            if(instance==null)instance=new GameObject("BoundedCombatAudio_8Voices").AddComponent<CombatAudioPool>();
            var source=instance.voices[instance.next++%instance.voices.Length];source.clip=clip;source.volume=Mathf.Clamp01(volume)*PlayerPrefs.GetFloat("CampaignEffectsVolume",.8f);source.pitch=Mathf.Clamp(pitch,.7f,1.4f);source.Play();
        }
        void OnDestroy(){if(instance==this)instance=null;}
    }
}
