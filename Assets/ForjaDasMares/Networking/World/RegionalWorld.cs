using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ForjaDasMares.Networking
{
    // First region is a local exploration prototype. No public-session or economy claims.
    [DefaultExecutionOrder(220)]public sealed class RegionalWorld:MonoBehaviour
    {
        [SerializeField] RegionDefinition definition;
        [SerializeField] HeroRoster hero;[SerializeField] EnemyPool enemies;[SerializeField] ArenaCamera follow;
        [SerializeField] SoundscapeLibrary soundscape;
        readonly Dictionary<EnemyActor,int> ownership=new Dictionary<EnemyActor,int>(24);
        float[][] timers;EnemyActor[][] residents;int[] firstKills;bool[] cleared;bool[][] firstDefeated;bool travelling;float poll;
        public RegionDefinition Definition=>definition;public HeroRoster Hero=>hero;public EnemyPool Enemies=>enemies;
        public int ClearedCount{get{int n=0;if(cleared!=null)foreach(bool c in cleared)if(c)n++;return n;}}
        public bool PortalReady=>ClearedCount==definition.camps.Length;
        public bool IsCampCleared(int index)=>cleared!=null&&index>=0&&index<cleared.Length&&cleared[index];
        public float RefugeDistance=>Vector3.Distance(hero.transform.position,definition.refuge);
        public event Action Changed;
        public void Configure(RegionDefinition data,HeroRoster player,EnemyPool pool,ArenaCamera camera,SoundscapeLibrary audio){definition=data;hero=player;enemies=pool;follow=camera;soundscape=audio;}
        void Start()
        {
            hero.Select(2);hero.enabled=false;hero.GetComponent<HeroCombat>().ConfigureFlow(hero.Current.onlineFlow);hero.Changed+=HeroChanged;
            hero.GetComponent<PlayerMotor>().Teleport(definition.refuge,Quaternion.identity);follow.SetFocusBounds(definition.bounds,true);follow.Snap();
            residents=new EnemyActor[definition.camps.Length][];timers=new float[residents.Length][];firstKills=new int[residents.Length];cleared=new bool[residents.Length];firstDefeated=new bool[residents.Length][];
            for(int i=0;i<residents.Length;i++){residents[i]=new EnemyActor[definition.camps[i].enemies.Length];timers[i]=new float[residents[i].Length];firstDefeated[i]=new bool[residents[i].Length];}
            enemies.SetLimit(definition.activeEnemyLimit);enemies.Died+=EnemyDied;
            AudioListener.volume=Mathf.Clamp01(PlayerPrefs.GetFloat("CampaignMasterVolume",.8f));
            if(soundscape!=null){Voice("RegionalMusic",soundscape.biomes[0].music,.26f*CampaignSoundscape.Volume(CampaignSoundscape.MusicKey));Voice("RegionalAmbience",soundscape.menu.ambience,.16f*CampaignSoundscape.Volume(CampaignSoundscape.AmbienceKey));}
            Debug.Log("REGION_READY localPrototype=true camps="+residents.Length+" capacity="+enemies.Capacity);
        }
        void Voice(string label,AudioClip clip,float volume){var go=new GameObject(label);go.transform.SetParent(transform,false);var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=true;source.spatialBlend=0;source.clip=clip;source.volume=volume;source.Play();}
        void HeroChanged(){hero.GetComponent<HeroCombat>().ConfigureFlow(hero.Current.onlineFlow);}
        void EnemyDied(EnemyActor actor)
        {
            if(!ownership.TryGetValue(actor,out int camp))return;ownership.Remove(actor);
            var members=residents[camp];for(int i=0;i<members.Length;i++)if(members[i]==actor){members[i]=null;timers[camp][i]=definition.camps[camp].respawnSeconds;if(!firstDefeated[camp][i]){firstDefeated[camp][i]=true;firstKills[camp]++;}break;}
            if(!cleared[camp]){if(firstKills[camp]>=members.Length){cleared[camp]=true;Changed?.Invoke();}}
        }
        void Update()
        {
            if(residents==null||travelling)return;
            var commands=(IHeroCommands)hero.GetComponent<PlayerMotor>().Commands;
            if(commands.TakeReset())Respawn();int slot=commands.TakeHeroSlot();if(commands.TakeNextHero())slot=(hero.Selected+1)%hero.Count;if(slot>=0&&RefugeDistance<10)hero.Select(slot);
            if(!hero.Health.IsAlive)return;
            if(hero.transform.position.y< -5)Respawn();
            poll-=Time.deltaTime;if(poll>0)return;float elapsed=.25f-poll;poll=.25f;
            for(int c=0;c<residents.Length;c++)for(int i=0;i<residents[c].Length;i++)
            {
                timers[c][i]=Mathf.Max(0,timers[c][i]-elapsed);
                if(residents[c][i]!=null||timers[c][i]>0)continue;
                var data=definition.camps[c];var point=data.spawns[i%data.spawns.Length];float distance=(hero.transform.position-point).sqrMagnitude;
                if(distance>75*75||distance<8*8)continue;
                var actor=enemies.TrySpawn(data.enemies[i],point);if(actor!=null){residents[c][i]=actor;ownership[actor]=c;}
            }
        }
        public string InteractionHint=>RefugeDistance<5?"F · Descansar no refúgio":Vector3.Distance(hero.transform.position,definition.portal)<5?(PortalReady?"F · Entrar nos desafios":"Investigue os três acampamentos para abrir o portal"):"";
        public void Interact()
        {
            if(!hero.Health.IsAlive||travelling)return;
            if(RefugeDistance<5){hero.Health.Restore();Changed?.Invoke();}
            else if(PortalReady&&Vector3.Distance(hero.transform.position,definition.portal)<5)ReturnToCampaign();
        }
        public void Respawn(){hero.ResetHero();hero.GetComponent<PlayerMotor>().Teleport(definition.refuge,Quaternion.identity);follow.Snap();Changed?.Invoke();}
        public void ReturnToCampaign(){if(travelling)return;travelling=true;hero.GetComponent<ArenaInput>().BlockCommands(true);SceneManager.LoadSceneAsync("CampaignCoop");}
        void OnDestroy(){if(enemies!=null)enemies.Died-=EnemyDied;if(hero!=null)hero.Changed-=HeroChanged;ownership.Clear();}
    }
}
