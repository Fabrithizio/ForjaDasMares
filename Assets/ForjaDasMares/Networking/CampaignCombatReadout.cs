using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    // Reuses a bounded pool of UI labels. No instantiation per impact and no camera shake.
    public sealed class CampaignCombatReadout : MonoBehaviour
    {
        sealed class Track {public Health hp;public int previous;public Action changed;}
        sealed class Popup {public Text label;public Vector3 point;public float remaining;public Color color;}
        readonly List<Track> tracks=new List<Track>(128);
        readonly Popup[] pool=new Popup[20];
        CoopDirector director;CoopProfileChannel profiles;RectTransform root;Camera cameraView;Text toast;
        float toastTime,scan;string runId;int coins,xp,items;
        AudioSource audioSource;float nextSound;
        public void Build(RectTransform parent,CoopDirector owner)
        {
            root=parent;director=owner;owner.GetComponent<CampaignLootDrops>()?.ConfigureReadout(this);profiles=owner.GetComponent<CoopProfileChannel>();cameraView=Camera.main;
            audioSource=gameObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;audioSource.spatialBlend=0;audioSource.volume=.55f;
            for(int i=0;i<pool.Length;i++)
            {var label=RusticHudTheme.Label(root,"Impact_"+i,Vector2.one*.5f,Vector2.zero,new Vector2(130,38),"",24);label.alignment=TextAnchor.MiddleCenter;label.fontStyle=FontStyle.Bold;label.gameObject.SetActive(false);pool[i]=new Popup{label=label};}
            toast=RusticHudTheme.Label(root,"RewardNotice",new Vector2(.5f,0),new Vector2(0,110),new Vector2(650,48),"",20);toast.alignment=TextAnchor.MiddleCenter;toast.color=RusticHudTheme.Gold;
            owner.Dungeon.Generated+=Bind;Bind();
        }
        void Bind()
        {
            Unbind();
            for(int i=0;i<4;i++)Watch(director.Actor(i)?.Health);
            foreach(var enemy in director.Dungeon.Encounter.Enemies.Actors)Watch(enemy.Health);
            if(director.Campaign.Adventure!=null)foreach(var prop in director.Campaign.Adventure.Props)Watch(prop.Health);
        }
        void Watch(Health hp)
        {
            if(hp==null)return;var track=new Track{hp=hp,previous=hp.Current};
            track.changed=()=>Changed(track);hp.Changed+=track.changed;tracks.Add(track);
        }
        void Changed(Track track)
        {
            int delta=track.hp.Current-track.previous;track.previous=track.hp.Current;
            if(delta==0||director.Phase!=CoopPhase.Playing)return;
            var local=director.Actor(director.LocalSlot);if(local==null||(track.hp.transform.position-local.transform.position).sqrMagnitude>400)return;
            if(track.hp.Faction==CombatFaction.Player&&track.hp!=local.Health&&delta<0)return;
            if(delta>0&&track.hp.Faction!=CombatFaction.Player)return; // pool resets are not heals
            var level=director.Campaign.Level;
            Sound(delta>0?level.healSound:track.hp.Faction==CombatFaction.Training&&!track.hp.IsAlive?level.crateSound:level.hitSound);
            foreach(var value in pool)if(value.remaining<=0)
            {
                value.remaining=.75f;value.point=track.hp.transform.position+Vector3.up*1.8f;
                value.color=delta>0?new Color(.35f,1,.55f):track.hp.Faction==CombatFaction.Player?new Color(1,.35f,.3f):RusticHudTheme.Parchment;
                value.label.fontSize=24;value.label.rectTransform.sizeDelta=new Vector2(130,38);value.label.text=(delta>0?"+":"")+delta;value.label.color=value.color;value.label.gameObject.SetActive(true);break;
            }
        }
        void LateUpdate()
        {
            if(director==null)return;float dt=Time.unscaledDeltaTime;
            bool visible=director.Phase==CoopPhase.Playing&&!(GetComponent<CampaignSessionMenu>()?.IsOpen??false)&&!(GetComponent<CoopInventoryHud>()?.IsOpen??false);
            foreach(var value in pool)
            {
                if(value.remaining<=0)continue;value.remaining=Mathf.Max(0,value.remaining-dt);value.point+=Vector3.up*(dt*.65f);
                Vector3 screen=cameraView.WorldToScreenPoint(value.point);
                bool show=visible&&value.remaining>0&&screen.z>0;value.label.gameObject.SetActive(show);
                if(show&&RectTransformUtility.ScreenPointToLocalPointInRectangle(root,screen,null,out Vector2 point))
                {((RectTransform)value.label.transform).anchoredPosition=point;var color=value.color;color.a=Mathf.Min(1,value.remaining*4);value.label.color=color;}
            }
            scan-=dt;if(scan<=0){scan=.15f;Rewards();}
            toastTime=Mathf.Max(0,toastTime-dt);toast.gameObject.SetActive(visible&&toastTime>0);
        }
        public void ShowDrop(Vector3 point,int gold,int experience,int rarity)
        {
            foreach(var value in pool)if(value.remaining<=0){value.remaining=1.1f;value.point=point+Vector3.up*2;value.color=RusticHudTheme.Gold;value.label.text="+"+gold+" ouro\n+"+experience+" XP";if(rarity>=0)value.label.text+="\n"+RusticHudTheme.RarityName((ItemRarity)rarity);value.label.fontSize=16;value.label.rectTransform.sizeDelta=new Vector2(170,72);value.label.gameObject.SetActive(true);break;}
        }
        void Rewards()
        {
            var run=profiles.LocalProfile?.expedition;if(run==null){runId=null;return;}
            if(runId!=run.runId){runId=run.runId;coins=run.coins;xp=run.experience;items=run.items.Count;return;}
            int c=run.coins-coins,e=run.experience-xp,n=run.items.Count-items;
            if(c>0||e>0||n>0)
            {
                string text=(c>0?"+"+c+" ouro   ":"")+(e>0?"+"+e+" XP   ":"");
                if(n>0){var item=run.items[run.items.Count-1];text+="Saque: "+(profiles.Catalog.Find(item.definitionId)?.displayName??item.definitionId)+" · "+RusticHudTheme.RarityName(item.rarity);}
                toast.text=text;toastTime=3.5f;if(n>0)Sound(director.Campaign.Level.lootSound);
            }
            coins=run.coins;xp=run.experience;items=run.items.Count;
        }
        void Unbind(){foreach(var track in tracks)if(track.hp!=null)track.hp.Changed-=track.changed;tracks.Clear();}
        void Sound(AudioClip clip){if(clip==null||Time.unscaledTime<nextSound)return;nextSound=Time.unscaledTime+.07f;audioSource.PlayOneShot(clip);}
        void OnDestroy(){if(director!=null)director.Dungeon.Generated-=Bind;Unbind();}
    }
}
