using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    public sealed class CampaignCharacterSheet:MonoBehaviour
    {
        CoopDirector session;CoopProfileChannel channel;GameObject popup;Text heading;Text[] stats=new Text[6],ranks=new Text[6];Button[] learn=new Button[6];GameObject[] overview=new GameObject[7];Button reset;bool confirm;float refresh;
        CoopActor Actor=>session.Actor(session.LocalSlot>=0?session.LocalSlot:0);
        public void Build(Transform parent,CoopDirector owner,CoopProfileChannel profiles)
        {
            session=owner;channel=profiles;
            for(int i=0;i<6;i++)
            {
                var rect=RusticHudTheme.Rect(parent,"AttributeCard_"+i,new Vector2(0,1),new Vector2(30+i%3*158,-382-i/3*58),new Vector2(150,52));RusticHudTheme.Panel(rect,RusticHudTheme.Surface,10);
                overview[i]=rect.gameObject;stats[i]=RusticHudTheme.Label(rect,"Value",new Vector2(0,1),new Vector2(8,-4),new Vector2(138,46),"",14);
            }
            overview[6]=RusticHudTheme.Button(parent,"Talents",new Vector2(0,1),new Vector2(30,-505),new Vector2(465,38),"ATRIBUTOS E TALENTOS",()=>{popup.SetActive(true);popup.transform.SetAsLastSibling();confirm=false;Refresh();}).gameObject;
            var modal=RusticHudTheme.Rect(parent,"CharacterTalents",new Vector2(.5f,.5f),Vector2.zero,new Vector2(1160,610));popup=modal.gameObject;var surface=RusticHudTheme.Panel(modal,new Color(.065f,.079f,.067f,1),20);surface.raycastTarget=true;
            heading=RusticHudTheme.Label(modal,"Summary",new Vector2(0,1),new Vector2(30,-20),new Vector2(925,135),"",19);
            RusticHudTheme.Button(modal,"CloseTalents",new Vector2(1,1),new Vector2(-20,-20),new Vector2(140,42),"VOLTAR",()=>popup.SetActive(false));
            for(int i=0;i<6;i++)
            {
                int pick=i;var card=RusticHudTheme.Rect(modal,"Talent_"+i,new Vector2(0,1),new Vector2(30+i%3*370,-175-i/3*155),new Vector2(350,140));RusticHudTheme.Panel(card,RusticHudTheme.Surface,12);
                ranks[i]=RusticHudTheme.Label(card,"Rank",new Vector2(0,1),new Vector2(14,-10),new Vector2(325,75),"",18);
                learn[i]=RusticHudTheme.Button(card,"Learn",new Vector2(0,1),new Vector2(14,-92),new Vector2(322,36),"MELHORAR · 1 PONTO",()=>{confirm=false;channel.Request(CoopInventoryAction.SpendTalent,null,EquipmentSlot.MainHand,false,pick);Refresh();});
            }
            reset=RusticHudTheme.Button(modal,"ResetTalents",new Vector2(0,1),new Vector2(30,-510),new Vector2(550,45),"",()=>{if(!confirm){confirm=true;Refresh();return;}channel.Request(CoopInventoryAction.ResetTalents,null,EquipmentSlot.MainHand,true);confirm=false;Refresh();});
            RusticHudTheme.Label(modal,"Rules",new Vector2(0,1),new Vector2(610,-508),new Vector2(510,80),"1 ponto por nível após o primeiro, por herói.\nModificadores compartilham os limites dos equipamentos.\nInvista fora de combate. Ouro é salvo neste perfil.",14);
            popup.SetActive(false);Refresh();
        }
        public void SetOverviewVisible(bool visible){foreach(var go in overview)if(go!=null)go.SetActive(visible);if(!visible)Hide();}
        public void Hide(){if(popup!=null)popup.SetActive(false);}
        void Update(){refresh-=Time.unscaledDeltaTime;if(refresh>0)return;refresh=.25f;Refresh();}
        void Refresh()
        {
            var p=channel.LocalProfile;if(p==null)return;var hero=Actor.Roster.Current;var c=channel.Catalog;var s=ProgressionStats.Calculate(hero,p,c);int points=HeroTalents.Available(p,hero.heroId,c);
            string[] labels={"VIDA MÁXIMA\n"+s.maximumHealth,"DANO\n×"+s.damageMultiplier.ToString("0.##"),"DEFESA\n"+(s.defense*100).ToString("0.#")+"%","ATAQUE\n×"+s.attackSpeed.ToString("0.##"),"CRÍTICO\n"+(s.criticalChance*100).ToString("0.#")+"%","MOVIMENTO\n"+s.speed.ToString("0.#")+" m/s"};for(int i=0;i<6;i++)stats[i].text=labels[i];
            if(!popup.activeSelf)return;
            heading.text=hero.displayName+" · NÍVEL "+s.level+" · "+points+" PONTOS LIVRES · "+p.coins+" OURO\nXP "+ProgressionStats.ExperienceInLevel(p.experience,c.maximumLevel)+" / "+(s.level==c.maximumLevel?"MÁXIMO":ProgressionStats.RequiredExperience(s.level).ToString())+"\nRecargas −"+((1-s.cooldownMultiplier)*100).ToString("0.#")+"% · elemental +"+(s.elementalDamage*100).ToString("0.#")+"% · regeneração "+s.recovery.ToString("0.#")+" vida/s\n"+(channel.HomeMode&&channel.Hub.UsingRemote?"Perfil da última sala: talentos só podem ser alterados com o host conectado.":channel.Message??"Escolha as melhorias para este herói.");
            bool allowed=!(channel.HomeMode&&channel.Hub.UsingRemote)&&(channel.HomeMode||session.GetComponent<CoopProgression>().CanManage(session.LocalSlot));
            for(int i=0;i<6;i++){var t=c.talents[i];int n=HeroTalents.Rank(p,hero.heroId,t.id);ranks[i].text=t.displayName+" · "+n+" / "+t.maximumRank+"\n+"+ItemDescriptions.Amount(t.stat,t.perRank)+" "+ItemDescriptions.Name(t.stat)+" por ponto";learn[i].interactable=allowed&&points>0&&n<t.maximumRank;}
            int cost=HeroTalents.ResetCost(p,hero.heroId,c);reset.GetComponentInChildren<Text>().text=(confirm?"CONFIRMAR REDISTRIBUIÇÃO · ":"REDISTRIBUIR PONTOS · ")+cost+" OURO";reset.interactable=allowed&&cost>0&&p.coins>=cost;
        }
    }
}
