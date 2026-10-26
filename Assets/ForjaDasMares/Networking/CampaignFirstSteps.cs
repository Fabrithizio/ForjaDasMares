using System;
using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    // Local guidance only. Observes actual actions and rewards; never grants progress.
    public sealed class CampaignFirstSteps:MonoBehaviour
    {
        CoopDirector session;CoopProfileChannel profiles;GameObject panel;Text message;string key;int completed;bool dismissed,hasPosition;Vector3 initial;float timer,distanceMoved;readonly string[] initialGear=new string[5];bool hasGear,persist;
        static readonly string[] Prompts={"MOVIMENTO · WASD/setas ou analógico. Explore alguns passos.","COMBATE · segure o ataque para continuar o combo. Mire antes de lançar.","ESQUIVA · Espaço ou botão de esquiva. Evite as áreas de perigo.","RECOMPENSAS · inimigos concedem ouro e XP automaticamente; equipamentos podem cair.","EQUIPAMENTO · abra Personagem e selecione um item para comparar e equipar.","TALENTOS · ao subir de nível, escolha uma melhoria em Atributos e Talentos."};
        public void Build(Transform parent,CoopDirector owner)
        {
            session=owner;profiles=owner.GetComponent<CoopProfileChannel>();string identity=PlayerPrefs.GetString("CoopParticipantIdentity","local");var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-netIdentity")identity=args[i+1];key="CampaignGuide-v2-"+identity;persist=Array.IndexOf(args,"-runTests")<0;
            completed=persist?PlayerPrefs.GetInt(key,0)&63:0;dismissed=persist&&PlayerPrefs.GetInt(key+"-hidden",0)==1;
            var r=RusticHudTheme.Rect(parent,"FirstSteps",new Vector2(.5f,0),new Vector2(0,215),new Vector2(550,75));panel=r.gameObject;RusticHudTheme.Panel(r,RusticHudTheme.Ink,16);
            message=RusticHudTheme.Label(r,"Hint",new Vector2(0,1),new Vector2(14,-8),new Vector2(475,59),"",16);
            RusticHudTheme.Button(r,"Dismiss",new Vector2(1,1),new Vector2(-8,-12),new Vector2(40,42),"×",()=>{dismissed=true;if(persist){PlayerPrefs.SetInt(key+"-hidden",1);PlayerPrefs.Save();}panel.SetActive(false);});panel.SetActive(false);
        }
        public void RestartGuide(){completed=0;dismissed=false;hasPosition=hasGear=false;distanceMoved=0;if(persist){PlayerPrefs.SetInt(key,0);PlayerPrefs.SetInt(key+"-hidden",0);PlayerPrefs.Save();}}
        void Mark(int bit){int next=completed|1<<bit;if(next==completed)return;completed=next;if(persist){PlayerPrefs.SetInt(key,completed);PlayerPrefs.Save();}}
        void Update()
        {
            if(panel==null)return;timer-=Time.unscaledDeltaTime;if(timer>0)return;timer=.25f;
            bool playing=session.Phase==CoopPhase.Playing&&session.LocalSlot>=0;panel.SetActive(false);if(!playing||dismissed||completed==63)return;
            var actor=session.Actor(session.LocalSlot);var p=profiles.LocalProfile;if(actor==null||p==null)return;
            if(!hasPosition){initial=actor.transform.position;hasPosition=true;}
            Vector3 travel=actor.transform.position-initial;travel.y=0;initial=actor.transform.position;
            if(travel.sqrMagnitude<9)distanceMoved+=travel.magnitude;
            if(distanceMoved>=2)Mark(0);
            if(actor.Presentation.State==HeroState.Attack)Mark(1);
            if(actor.DodgeCooldown>0)Mark(2);
            if(p.expedition!=null&&(p.expedition.coins>0||p.expedition.experience>0||p.expedition.items.Count>0))Mark(3);
            var set=p.Loadout(actor.Roster.Current.heroId);
            if(set!=null)
            {
                if(!hasGear){for(int i=0;i<initialGear.Length;i++)initialGear[i]=set.items[i];hasGear=true;}
                else for(int i=0;i<initialGear.Length;i++)if(initialGear[i]!=set.items[i]){Mark(4);break;}
            }
            if(HeroTalents.Spent(p,actor.Roster.Current.heroId)>0)Mark(5);
            bool hidden=GetComponent<CoopInventoryHud>()?.IsOpen==true||GetComponent<CampaignSessionMenu>()?.IsOpen==true;
            if(hidden||completed==63||!actor.Health.IsAlive)return;
            int step=0;while(step<6&&(completed&(1<<step))!=0)step++;
            string prompt=Prompts[step];if(Application.isMobilePlatform){if(step==0)prompt="MOVIMENTO · arraste o joystick à esquerda para explorar.";if(step==2)prompt="ESQUIVA · use o botão à direita para evitar áreas de perigo.";}
            message.text="PRIMEIROS PASSOS  "+(step+1)+"/6\n"+prompt;panel.SetActive(true);
        }
    }
}
