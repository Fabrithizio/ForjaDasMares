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
            float efeitos=Volumes.De(Volumes.Efeitos);if(clip==null||efeitos<=0)return;
            if(instance==null)instance=new GameObject("BoundedCombatAudio_8Voices").AddComponent<CombatAudioPool>();
            var source=instance.voices[instance.next++%instance.voices.Length];source.clip=clip;source.volume=Mathf.Clamp01(volume)*efeitos;source.pitch=Mathf.Clamp(pitch,.7f,1.4f);source.Play();
        }
        // Effects turned down to 0% while something plays: it stops right away.
        float check;
        void Update(){if((check-=Time.unscaledDeltaTime)>0)return;check=.2f;if(Volumes.De(Volumes.Efeitos)>0)return;foreach(var v in voices)if(v!=null&&v.isPlaying)v.Stop();}
        void OnDestroy(){if(instance==this)instance=null;}
    }
}
