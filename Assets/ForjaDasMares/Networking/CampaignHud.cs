using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    public sealed class CampaignHud : MonoBehaviour
    {
        CoopDirector director;CoopCloud cloud;ArenaInput controls;CampaignRun campaign;
        RectTransform root;GameObject map,side,touch;Text title,details,state,party,combat;InputField code;
        Button create,find,join,start,hero,leave;
        readonly Button[] nodes=new Button[3];float refresh;uint cleared;
        public void Build(RectTransform parent,CoopDirector owner,ArenaInput input)
        {
            root=parent;director=owner;controls=input;campaign=owner.Campaign;cloud=owner.GetComponent<CoopCloud>();
            var background=RusticHudTheme.Rect(root,"CampaignMap",new Vector2(0,1),new Vector2(20,-25),new Vector2(860,655));map=background.gameObject;RusticHudTheme.Panel(background,new Color(.10f,.16f,.13f,.98f),24);
            Label(background,"CAMPANHA · AS ROTAS DA MARÉ",30,-25,790,55,28);
            Label(background,"Escolha uma fase · encontre um grupo ou convide alguém pelo código",30,-85,790,50,17);
            Vector2[] points={new Vector2(45,-345),new Vector2(312,-255),new Vector2(580,-165)};
            for(int i=0;i<3;i++)
            {
                if(i<2)
                {
                    Vector2 a=points[i]+new Vector2(105,-65),b=points[i+1]+new Vector2(105,-65),delta=b-a;
                    var road=RusticHudTheme.Rect(background,"Road_"+i,new Vector2(0,1),(a+b)*.5f,new Vector2(delta.magnitude,8));road.pivot=Vector2.one*.5f;road.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);RusticHudTheme.Panel(road,new Color(.48f,.37f,.20f),3);
                }
                int level=i;nodes[i]=RusticHudTheme.Button(background,"Level_"+(i+1),new Vector2(0,1),points[i],new Vector2(225,160),campaign.At(i).displayName+"\n\n"+(i==0?"INICIAL":i==1?"VETERANO":"DESAFIADOR"),()=>director.SelectCampaignLevel(level));
                var text=nodes[i].GetComponentInChildren<Text>();text.fontSize=18;RusticHudTheme.Icon(nodes[i].transform,HudSymbol.Compass,new Vector2(.5f,0),new Vector2(0,6),26);
            }
            Label(background,"1  →  2  →  3",40,-535,790,40,22);
            Label(background,"Duas a quatro pessoas · inimigos, baús, placas simultâneas e flechas com aviso.\nTrês fases liberadas para testes. O host guarda o progresso do grupo.",40,-585,790,55,16);
            var panel=RusticHudTheme.Rect(root,"CampaignSession",new Vector2(0,1),new Vector2(900,-25),new Vector2(350,655));side=panel.gameObject;RusticHudTheme.Panel(panel,RusticHudTheme.Ink,24);
            title=Label(panel,"",20,-15,310,55,23);details=Label(panel,"",20,-72,310,85,16);
            hero=Button(panel,"Hero",20,-165,310,42,"HERÓI",()=>director.SelectHero((director.SelectedHero+1)%director.HeroCount));
            create=Button(panel,"Private",20,-220,310,48,"CRIAR SALA PRIVADA",()=>cloud.Create());
            find=Button(panel,"FindGroup",20,-278,310,48,"PROCURAR GRUPO DESTA FASE",()=>cloud.FindCampaignGroup());
            code=Field(panel,20,-340,310,44);
            join=Button(panel,"JoinCode",20,-395,310,44,"ENTRAR PELO CÓDIGO",()=>cloud.Join(code.text));
            start=Button(panel,"Start",20,-453,310,48,"INICIAR COM O GRUPO",director.StartGame);
            state=Label(panel,"",20,-510,310,110,16);
            party=Label(root,"",28,-60,450,130,18);
            combat=Label(root,"",390,-25,550,90,18);
            leave=RusticHudTheme.Button(root,"CampaignLeave",new Vector2(1,1),new Vector2(-20,-20),new Vector2(190,44),"VOLTAR AO MAPA",director.Close);
            RusticHudTheme.Button(root,"CopyInvite",new Vector2(1,1),new Vector2(-220,-20),new Vector2(160,44),"COPIAR CÓDIGO",()=>{if(!string.IsNullOrEmpty(director.Pin))GUIUtility.systemCopyBuffer=director.Pin;});
            touch=RusticHudTheme.Rect(root,"CampaignControls",Vector2.zero,Vector2.zero,Vector2.zero).gameObject;
            var tr=(RectTransform)touch.transform;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;
            if(Application.isMobilePlatform)
            {
                var stick=RusticHudTheme.Rect(touch.transform,"MoveStick",Vector2.zero,new Vector2(190,170),new Vector2(170,170));stick.pivot=Vector2.one*.5f;
                var image=stick.gameObject.AddComponent<Image>();image.color=new Color(.13f,.18f,.15f,.65f);
                var thumb=RusticHudTheme.Rect(stick,"Thumb",Vector2.one*.5f,Vector2.zero,new Vector2(55,55));thumb.gameObject.AddComponent<Image>().color=RusticHudTheme.Gold;
                stick.gameObject.AddComponent<TouchMoveStick>().Configure(controls,thumb);
                Touch("Ataque",-70,55,HeroButtons.Attack,true);Touch("Esquiva",-240,55,HeroButtons.Dodge,false);Touch("Habilidade",-70,140,HeroButtons.Special,false);Touch("Interagir",-240,140,HeroButtons.Interact,false);
            }
            else
            {
                var dock=RusticHudTheme.Rect(touch.transform,"DesktopDock",new Vector2(.5f,0),new Vector2(0,20),new Vector2(650,70));dock.pivot=new Vector2(.5f,0);RusticHudTheme.Panel(dock,RusticHudTheme.Ink);
                Label(dock,"WASD · mover    Mouse · segure para atacar    Espaço · esquiva\nE · habilidade       F · interagir       Mochila · equipar fora do combate",15,-8,620,55,17);
            }
        }
        void Touch(string label,float x,float y,HeroButtons action,bool held)
        {
            var button=RusticHudTheme.Button(touch.transform,label,new Vector2(1,0),new Vector2(x,y),new Vector2(145,72),label,()=>{if(!held)director.Request(action);});
            if(held)button.gameObject.AddComponent<HoldAttackButton>().Configure(controls);
        }
        void Update()
        {
            if(root==null)return;refresh-=Time.unscaledDeltaTime;if(refresh>0)return;refresh=.12f;
            bool active=director.Phase==CoopPhase.Playing;
            bool offline=director.Phase==CoopPhase.Offline||director.Phase==CoopPhase.Closed;
            map.SetActive(!active);side.SetActive(!active);party.gameObject.SetActive(active);combat.gameObject.SetActive(active);touch.SetActive(active);
            title.text=campaign.Level.displayName;details.text=campaign.Level.description+"\nSeleção do host: fase "+(campaign.Selected+1);
            for(int i=0;i<3;i++){nodes[i].interactable=offline&&!cloud.Busy;nodes[i].GetComponentInChildren<RoundedHudGraphic>().borderColor=i==campaign.Selected?RusticHudTheme.Gold:RusticHudTheme.Copper;}
            hero.GetComponentInChildren<Text>().text="HERÓI · "+director.HeroName;hero.interactable=offline||director.Phase==CoopPhase.Lobby;
            create.gameObject.SetActive(offline);find.gameObject.SetActive(offline);join.gameObject.SetActive(offline);code.gameObject.SetActive(offline);
            create.interactable=find.interactable=join.interactable=offline&&!cloud.Busy&&!director.Manager.IsListening;
            start.gameObject.SetActive(!offline);start.interactable=director.IsHost&&director.Phase==CoopPhase.Lobby&&director.ConnectedCount>=2;
            state.text=offline?cloud.Message:director.Message+"\nCódigo: "+director.Pin+"\nGrupo "+director.ConnectedCount+"/4"+(director.ConnectedCount<2?" · aguardando parceiro":"");
            string allies="";for(int i=0;i<4;i++)if(director.IsConnected(i)){var actor=director.Actor(i);allies+=(i==director.LocalSlot?"VOCÊ":"ALIADO "+(i+1))+" · "+actor.Roster.Current.displayName+"\n"+actor.Health.Current+" / "+actor.Health.Maximum+" VIDA\n";}
            party.text=allies;
            var profile=director.GetComponent<CoopProfileChannel>().LocalProfile;
            if(profile!=null)cleared=profile.campaignCleared;
            for(int i=0;i<3;i++)nodes[i].GetComponentInChildren<Text>().text=campaign.At(i).displayName+"\n\n"+((cleared&(1u<<i))!=0?"CONCLUÍDA":i==0?"INICIAL":i==1?"VETERANO":"DESAFIADOR");
            combat.text=campaign.Level.displayName+"\n"+campaign.Objective+(profile!=null?"\nMoedas: "+(profile.expedition?.coins??profile.coins)+" · saque: "+(profile.expedition?.items.Count??0):"");
            if(active&&director.ConnectedCount<2)combat.text="Parceiro desconectado: retorne ao mesmo código em até 90s.\nAs placas exigem dois heróis vivos.";
            foreach(var enemy in director.Dungeon.Encounter.Enemies.Actors)if(enemy.IsAlive&&enemy.Profile.kind==EnemyKind.Boss){combat.text+="\nCHEFE · "+enemy.Health.Current+" / "+enemy.Health.Maximum;break;}
        }
        static Text Label(Transform p,string s,float x,float y,float w,float h,int font)=>RusticHudTheme.Label(p,"Text",new Vector2(0,1),new Vector2(x,y),new Vector2(w,h),s,font);
        static Button Button(Transform p,string n,float x,float y,float w,float h,string s,UnityEngine.Events.UnityAction a)=>RusticHudTheme.Button(p,n,new Vector2(0,1),new Vector2(x,y),new Vector2(w,h),s,a);
        static InputField Field(Transform parent,float x,float y,float w,float h)
        {
            var rect=RusticHudTheme.Rect(parent,"InvitationCode",new Vector2(0,1),new Vector2(x,y),new Vector2(w,h));var image=rect.gameObject.AddComponent<Image>();image.color=new Color(.22f,.25f,.20f);
            var text=Label(rect,"",10,0,w-20,h,23);var field=rect.gameObject.AddComponent<InputField>();field.textComponent=text;field.characterLimit=12;
            var placeholder=Label(rect,"CÓDIGO DO CONVITE",10,0,w-20,h,16);placeholder.color=RusticHudTheme.Muted;field.placeholder=placeholder;return field;
        }
    }
}
