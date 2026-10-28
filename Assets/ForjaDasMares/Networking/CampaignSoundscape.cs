using UnityEngine;
namespace ForjaDasMares.Networking
{
    // Local presentation only: five bounded voices, no network audio or singleton.
    public sealed class CampaignSoundscape:MonoBehaviour
    {
        public const string MusicKey="CampaignMusicVolume",AmbienceKey="CampaignAmbienceVolume";
        CoopDirector session;SoundscapeLibrary library;LoopBus music,ambience;AudioSource accent;
        CoopPhase previousPhase;bool phaseKnown;float poll,musicGain,ambienceGain;
        public AudioClip CurrentMusic=>music?.Clip;
        public AudioClip CurrentAmbience=>ambience?.Clip;
        public string Context {get;private set;}
        public bool MusicPlaying=>music!=null&&music.Playing;
        public bool AmbiencePlaying=>ambience!=null&&ambience.Playing;
        public int MusicSamplePosition=>music!=null?music.SamplePosition:0;
        public static float Volume(string key)=>Mathf.Clamp01(PlayerPrefs.GetFloat(key,key==MusicKey?.70f:.65f));
        public static float CycleVolume(string key)
        {float value=Volume(key);value=value<.01f?1:Mathf.Max(0,value-.2f);PlayerPrefs.SetFloat(key,value);PlayerPrefs.Save();return value;}
        public void Configure(CoopDirector owner,SoundscapeLibrary data)
        {
            if(music!=null)return;session=owner;library=data;
            if(library==null||library.menu==null||library.biomes==null||library.biomes.Length!=3){Debug.LogError("Campaign soundscape library missing or incomplete.",this);enabled=false;return;}
            music=new LoopBus(transform,"Music",160);ambience=new LoopBus(transform,"Ambience",200);accent=Voice(transform,"SoundscapeAccent",140);RefreshNow();
        }
        public void RefreshNow()
        {
            if(session==null||library==null)return;
            var phase=session.Phase;
            bool world=session.Campaign!=null&&(phase==CoopPhase.Loading||phase==CoopPhase.Playing||phase==CoopPhase.Victory||phase==CoopPhase.Defeat);
            int index=world?Mathf.Clamp(session.Campaign.Level.biomeIndex,0,library.biomes.Length-1):-1;
            var cue=index<0?library.menu:library.biomes[index];
            int fight=session.Campaign!=null?session.Campaign.FightRoomId:-1;var rooms=session.Dungeon.Generator.Rooms;
            var room=rooms!=null&&fight>=0&&fight<rooms.Length?rooms[fight]:session.Dungeon.FightRoom;
            bool boss=phase==CoopPhase.Playing&&room!=null&&room.node.category==RoomCategory.Boss&&(session.EncounterStatus==EncounterState.Spawning||session.EncounterStatus==EncounterState.Fighting);
            music.Switch(boss?library.bossMusic:cue.music);ambience.Switch(cue.ambience);Context=boss?"Chefe":cue.displayName;
            musicGain=cue.musicGain*(phase==CoopPhase.Defeat?.55f:1);ambienceGain=cue.ambienceGain;
            if(phaseKnown&&phase!=previousPhase)
            {
                var clip=phase==CoopPhase.Victory?library.victory:phase==CoopPhase.Defeat?library.defeat:phase==CoopPhase.Playing&&(previousPhase==CoopPhase.Lobby||previousPhase==CoopPhase.Loading||previousPhase==CoopPhase.Connecting)?library.start:null;
                if(clip!=null){accent.volume=.22f*Volume(MusicKey);accent.Stop();accent.clip=clip;accent.Play();}
            }
            phaseKnown=true;previousPhase=phase;
        }
        void Update()
        {
            if(library==null||music==null)return;poll-=Time.unscaledDeltaTime;if(poll<=0){poll=.2f;RefreshNow();}
            music.Tick(Time.unscaledDeltaTime/library.crossfade,accent.isPlaying?0:musicGain*Volume(MusicKey));
            ambience.Tick(Time.unscaledDeltaTime/library.crossfade,ambienceGain*Volume(AmbienceKey));
            accent.volume=.22f*Volume(MusicKey);
        }
        void OnDisable(){music?.Stop();ambience?.Stop();if(accent!=null)accent.Stop();poll=0;}
        static AudioSource Voice(Transform parent,string name,int priority)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.dopplerLevel=0;source.priority=priority;source.volume=0;source.ignoreListenerPause=true;return source;
        }
        sealed class LoopBus
        {
            readonly AudioSource[] voices;float level;int current;AudioClip requested;
            public AudioClip Clip=>requested;
            public bool Playing=>voices[current].isPlaying;
            public int SamplePosition=>voices[current].timeSamples;
            public LoopBus(Transform root,string name,int priority){voices=new[]{Voice(root,name+"A",priority),Voice(root,name+"B",priority)};foreach(var v in voices)v.loop=true;}
            public void Switch(AudioClip clip){requested=clip;}
            public void Tick(float step,float gain)
            {
                var voice=voices[current];
                if(voice.clip!=requested)
                {
                    level=Mathf.MoveTowards(level,0,step*2);voice.volume=level*gain;
                    if(level>0)return;
                    voice.Stop();voice.clip=null;current=1-current;voice=voices[current];
                    voice.Stop();voice.clip=requested;voice.volume=0;
                }
                if(requested==null)return;
                if(!voice.isPlaying)voice.Play();
                level=Mathf.MoveTowards(level,1,step*2);voice.volume=level*gain;
            }
            public void Stop(){foreach(var v in voices){v.Stop();v.clip=null;v.volume=0;}level=0;}

        }
    }
}
