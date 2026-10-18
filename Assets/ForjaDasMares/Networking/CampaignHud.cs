using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    public sealed class CampaignHud : MonoBehaviour
    {
        CoopDirector director;CoopCloud cloud;ArenaInput controls;CampaignRun campaign;
        RectTransform root,healthFill,superFill;GameObject map,side,touch,backdrop,vitals,objective;Text title,details,state,party,combat;InputField code;
        Button create,find,join,start,hero,leave,copyInvite,visibility,liveVisibility,homeButton;
        Text actionStatus; Button dodgeAction,specialAction,superAction,attackAction; GameObject home; CampaignSessionMenu sessionMenu; Button menuButton; Text roomCode;
        CoopPhase previousPhase=CoopPhase.Offline;
        public bool AtHome {get;private set;}=true;
        readonly Button[] nodes=new Button[3];float refresh;uint cleared;
        public void Build(RectTransform parent,CoopDirector owner,ArenaInput input)
        {
            root=parent;director=owner;controls=input;campaign=owner.Campaign;cloud=owner.GetComponent<CoopCloud>();
            var shade=RusticHudTheme.Rect(root,"CampaignBackdrop",Vector2.zero,Vector2.zero,Vector2.zero);shade.anchorMax=Vector2.one;shade.offsetMin=shade.offsetMax=Vector2.zero;RusticHudTheme.Panel(shade,RusticHudTheme.Ink,0);backdrop=shade.gameObject;
            var content=RusticHudTheme.Rect(root,"CampaignContent",Vector2.one*.5f,Vector2.zero,new Vector2(1280,720));
            var background=RusticHudTheme.Rect(content,"CampaignMap",new Vector2(0,1),new Vector2(20,-25),new Vector2(860,655));map=background.gameObject;RusticHudTheme.Panel(background,new Color(.10f,.16f,.13f,.98f),24);
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
            Label(background,"Uma a quatro pessoas · prepare seus equipamentos no perfil.\nSala aberta recebe parceiros até 20% da fase. O host guarda o progresso.",40,-585,790,55,16);
            var panel=RusticHudTheme.Rect(content,"CampaignSession",new Vector2(0,1),new Vector2(900,-25),new Vector2(350,655));side=panel.gameObject;RusticHudTheme.Panel(panel,RusticHudTheme.Ink,24);
            title=Label(panel,"",20,-15,310,55,23);details=Label(panel,"",20,-72,310,85,16);
            hero=Button(panel,"Hero",20,-165,310,42,"HERÓI",()=>director.SelectHero((director.SelectedHero+1)%director.HeroCount));
            create=Button(panel,"Private",20,-220,310,48,"CRIAR SALA PRIVADA",()=>cloud.Create());
            visibility=Button(panel,"Visibility",20,-220,310,48,"ABRIR SALA PARA GRUPOS",()=>cloud.ToggleVisibility());
            find=Button(panel,"FindGroup",20,-278,310,48,"PROCURAR GRUPO DESTA FASE",()=>cloud.FindCampaignGroup());
            code=Field(panel,20,-340,310,44);
            join=Button(panel,"JoinCode",20,-395,310,44,"ENTRAR PELO CÓDIGO",()=>cloud.Join(code.text));
            start=Button(panel,"Start",20,-453,310,48,"INICIAR COM O GRUPO",()=>{if(director.Phase==CoopPhase.Lobby)director.StartGame();else director.ReturnToCampaignMap();});
            state=Label(panel,"",20,-510,310,110,16);
            var life=RusticHudTheme.Rect(root,"CampaignVitals",new Vector2(0,1),new Vector2(20,-20),new Vector2(285,154));RusticHudTheme.Panel(life,RusticHudTheme.Ink,20);vitals=life.gameObject;
            party=Label(life,"",14,-10,255,110,15);
            var bar=RusticHudTheme.Rect(life,"HealthTrack",new Vector2(0,1),new Vector2(14,-130),new Vector2(255,12));bar.gameObject.AddComponent<Image>().color=RusticHudTheme.Surface;
            healthFill=RusticHudTheme.Rect(bar,"Health",new Vector2(0,1),Vector2.zero,new Vector2(255,12));healthFill.gameObject.AddComponent<Image>().color=new Color(.75f,.15f,.14f);
            var superTrack=RusticHudTheme.Rect(life,"SuperTrack",new Vector2(0,1),new Vector2(14,-146),new Vector2(255,5));superTrack.gameObject.AddComponent<Image>().color=RusticHudTheme.Surface;
            superFill=RusticHudTheme.Rect(superTrack,"Super",new Vector2(0,1),Vector2.zero,new Vector2(0,5));superFill.gameObject.AddComponent<Image>().color=RusticHudTheme.Gold;
            var goal=RusticHudTheme.Rect(root,"CampaignObjective",new Vector2(0,1),new Vector2(325,-20),new Vector2(455,172));RusticHudTheme.Panel(goal,RusticHudTheme.Ink,20);objective=goal.gameObject;
            combat=Label(goal,"",15,-10,425,152,15);
            leave=RusticHudTheme.Button(root,"CampaignLeave",new Vector2(1,1),new Vector2(-20,-20),new Vector2(190,44),"MENU DA SALA",()=>sessionMenu.Open());
            copyInvite=RusticHudTheme.Button(root,"CopyInvite",new Vector2(1,1),new Vector2(-220,-20),new Vector2(160,44),"COPIAR CÓDIGO",()=>{if(!string.IsNullOrEmpty(director.Pin))GUIUtility.systemCopyBuffer=director.Pin;});
            liveVisibility=RusticHudTheme.Button(root,"LiveVisibility",new Vector2(1,1),new Vector2(-20,-75),new Vector2(390,44),"ABRIR SALA",()=>cloud.ToggleVisibility());
            homeButton=RusticHudTheme.Button(root,"HomeButton",new Vector2(1,0),new Vector2(-20,20),new Vector2(200,44),"INÍCIO",()=>AtHome=true);
            touch=RusticHudTheme.Rect(root,"CampaignControls",Vector2.zero,Vector2.zero,Vector2.zero).gameObject;
            var tr=(RectTransform)touch.transform;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;
            if(Application.isMobilePlatform)
            {
                var stick=RusticHudTheme.Rect(touch.transform,"MoveStick",Vector2.zero,new Vector2(190,170),new Vector2(170,170));stick.pivot=Vector2.one*.5f;
                var ring=stick.gameObject.AddComponent<RoundedHudGraphic>();ring.color=new Color(.13f,.18f,.15f,.65f);ring.radius=85;ring.borderColor=RusticHudTheme.Copper;
                var thumb=RusticHudTheme.Rect(stick,"Thumb",Vector2.one*.5f,Vector2.zero,new Vector2(55,55));var cap=thumb.gameObject.AddComponent<RoundedHudGraphic>();cap.color=RusticHudTheme.Gold;cap.radius=27;cap.raycastTarget=false;
                stick.gameObject.AddComponent<TouchMoveStick>().Configure(controls,thumb);
                Touch("Ataque",-70,55,HeroButtons.Attack,true);Touch("Esquiva",-240,55,HeroButtons.Dodge,false);Touch("Habilidade",-70,140,HeroButtons.Special,false);Touch("Interagir",-240,140,HeroButtons.Interact,false);Touch("Super",-410,55,HeroButtons.Super,false);
            }
            else
            {
                gameObject.AddComponent<CampaignAbilityBar>().Build(touch.transform,director,controls);
            }
            var controlsGroup=touch.AddComponent<CanvasGroup>();
            var codePanel=RusticHudTheme.Rect(root,"LiveRoomCode",new Vector2(1,1),new Vector2(-20,-20),new Vector2(280,62));
            RusticHudTheme.Panel(codePanel,RusticHudTheme.Ink,18);
            roomCode=Label(codePanel,"",12,-8,256,46,19);
            menuButton=RusticHudTheme.Button(root,"SessionMenuButton",new Vector2(1,1),new Vector2(-20,-94),new Vector2(180,48),Application.isMobilePlatform?"MENU":"MENU · ESC",()=>sessionMenu.Open());
            var homeRect=RusticHudTheme.Rect(root,"InitialScreen",Vector2.zero,Vector2.zero,Vector2.zero);homeRect.anchorMax=Vector2.one;homeRect.offsetMin=homeRect.offsetMax=Vector2.zero;home=homeRect.gameObject;
            home.AddComponent<CampaignHome>().Build(homeRect,director,()=>AtHome=false,()=>GetComponent<CoopInventoryHud>().OpenInventory());
            sessionMenu=gameObject.AddComponent<CampaignSessionMenu>();sessionMenu.Build(root,director,controlsGroup);
            gameObject.AddComponent<CampaignCombatReadout>().Build(root,director);
            var minimap=new GameObject("CampaignMinimap",typeof(RectTransform));minimap.AddComponent<CampaignMinimap>().Build(root,director);
        }
        void Touch(string label,float x,float y,HeroButtons action,bool held)
        {
            var button=RusticHudTheme.Button(touch.transform,label,new Vector2(1,0),new Vector2(x,y),new Vector2(145,72),label,()=>{if(!held)director.Request(action);});
            if(held)button.gameObject.AddComponent<HoldAttackButton>().Configure(controls);
            if(action==HeroButtons.Attack)attackAction=button;if(action==HeroButtons.Super)superAction=button;
            if(action==HeroButtons.Dodge)dodgeAction=button;if(action==HeroButtons.Special)specialAction=button;
        }
        void Update()
        {
            if(root==null)return;refresh-=Time.unscaledDeltaTime;if(refresh>0)return;refresh=.12f;
            bool active=director.Phase==CoopPhase.Playing;
            bool offline=director.Phase==CoopPhase.Offline||director.Phase==CoopPhase.Closed;
            if(offline&&previousPhase!=CoopPhase.Offline&&previousPhase!=CoopPhase.Closed)AtHome=true;previousPhase=director.Phase;
            if(!offline)AtHome=false;
            home.SetActive(AtHome);homeButton.gameObject.SetActive(offline&&!AtHome);
            backdrop.SetActive(!active);vitals.SetActive(active);objective.SetActive(active);map.SetActive(!active);side.SetActive(!active);party.gameObject.SetActive(active);combat.gameObject.SetActive(active);touch.SetActive(active);
            if(AtHome){map.SetActive(false);side.SetActive(false);}
            title.text=campaign.Level.displayName;details.text=campaign.Level.description+"\nSeleção do host: fase "+(campaign.Selected+1);
            for(int i=0;i<3;i++)
            {
                nodes[i].interactable=(offline||director.IsHost&&director.Phase==CoopPhase.Lobby)&&!cloud.Busy;
                var surface=nodes[i].GetComponentInChildren<RoundedHudGraphic>();var color=i==campaign.Selected?RusticHudTheme.Gold:RusticHudTheme.Copper;
                if(surface.borderColor!=color){surface.borderColor=color;surface.SetVerticesDirty();}
            }
            leave.gameObject.SetActive((!offline||cloud.Busy)&&!active);copyInvite.gameObject.SetActive(!offline&&!active);
            menuButton.gameObject.SetActive(active);roomCode.transform.parent.gameObject.SetActive(active);
            roomCode.text="SALA  "+(string.IsNullOrEmpty(director.Pin)?"…":director.Pin)+"\n"+director.ConnectedCount+"/4 · "+(cloud.PublicRoom?"ABERTA":"PRIVADA");
            copyInvite.interactable=!string.IsNullOrEmpty(director.Pin);
            foreach(var button in new[]{leave,copyInvite})
            {
                var rect=(RectTransform)button.transform;rect.anchorMin=rect.anchorMax=rect.pivot=active?Vector2.one:new Vector2(1,0);
                rect.anchoredPosition=new Vector2(button==leave?-20:-220,active?-20:20);
            }
            hero.GetComponentInChildren<Text>().text="HERÓI · "+director.HeroName;hero.interactable=offline||director.Phase==CoopPhase.Lobby;
            create.gameObject.SetActive(offline);find.gameObject.SetActive(offline);join.gameObject.SetActive(offline);code.gameObject.SetActive(offline);
            visibility.gameObject.SetActive(!offline&&director.IsHost);visibility.interactable=cloud.CanChangeVisibility&&!cloud.Busy;
            string visibilityLabel=cloud.PublicRoom?"TORNAR SALA PRIVADA":"ABRIR SALA PARA GRUPOS";
            visibility.GetComponentInChildren<Text>().text=visibilityLabel;liveVisibility.GetComponentInChildren<Text>().text=visibilityLabel;
            liveVisibility.gameObject.SetActive(false);liveVisibility.interactable=cloud.CanChangeVisibility&&!cloud.Busy;
            create.interactable=find.interactable=join.interactable=offline&&!cloud.Busy&&!director.Manager.IsListening;
            start.gameObject.SetActive(!offline);start.interactable=director.IsHost&&!cloud.Busy&&(director.Phase==CoopPhase.Lobby&&director.ConnectedCount>=1||director.Phase==CoopPhase.Victory||director.Phase==CoopPhase.Defeat);
            start.GetComponentInChildren<Text>().text=director.Phase==CoopPhase.Lobby?(director.ConnectedCount==1?"INICIAR SOLO":"INICIAR COM O GRUPO"):"CONTINUAR CAMPANHA";
            state.text=offline?cloud.Message:"Sala "+(cloud.PublicRoom?"aberta para grupos":"privada")+" · "+director.Phase+"\nCódigo: "+director.Pin+"\nGrupo "+director.ConnectedCount+"/4"+(director.ConnectedCount==1?" · pode jogar solo":"");
            string allies="";for(int i=0;i<4;i++)if(director.IsConnected(i)){var actor=director.Actor(i);allies+=(i==director.LocalSlot?"VOCÊ":"ALIADO "+(i+1))+" · "+actor.Health.Current+"/"+actor.Health.Maximum+" VIDA"+(i==director.LocalSlot?"\n"+actor.Roster.Current.displayName:"")+"\n";}
            party.text=allies;
            var local=director.Actor(director.LocalSlot);healthFill.sizeDelta=new Vector2(local!=null&&local.Health.Maximum>0?255f*local.Health.Current/local.Health.Maximum:0,12);
            if(local!=null&&active)
            {
                superFill.sizeDelta=new Vector2(local.SuperProgress*255,5);superFill.parent.gameObject.SetActive(local.Enhanced);
                if(superAction!=null){superAction.gameObject.SetActive(local.Enhanced);superAction.interactable=local.Health.IsAlive&&local.SuperProgress>=1;superAction.GetComponentInChildren<Text>().text=(local.FuryRemaining>0?"FÚRIA\n"+local.FuryRemaining.ToString("F1")+"s":"SUPER\n"+Mathf.FloorToInt(local.SuperProgress*100)+"%");}
                if(attackAction!=null){attackAction.interactable=local.Health.IsAlive;attackAction.GetComponentInChildren<Text>().text=local.Enhanced&&local.Charge>0?"Ataque\nCarga "+Mathf.FloorToInt(local.Charge*100)+"%":"Ataque\nSegure";}
                string skill=local.SpecialCooldown>0?local.SpecialCooldown.ToString("F1")+"s":"pronta",dodge=local.DodgeCooldown>0?local.DodgeCooldown.ToString("F1")+"s":"pronta";
                if(specialAction!=null){specialAction.GetComponentInChildren<Text>().text="Habilidade\n"+skill;specialAction.interactable=local.Health.IsAlive&&local.SpecialCooldown<=0;}
                if(dodgeAction!=null){dodgeAction.GetComponentInChildren<Text>().text="Esquiva\n"+dodge;dodgeAction.interactable=local.Health.IsAlive&&local.DodgeCooldown<=0;}
                if(actionStatus!=null)actionStatus.text="WASD · mover   Mouse · segure para atacar   F · interagir\nE · habilidade: "+skill+"   Espaço · esquiva: "+dodge+"   Esc · menu"+(local.Enhanced?"   Q · SUPER "+Mathf.FloorToInt(local.SuperProgress*100)+"%":"");
            }
            var profile=director.GetComponent<CoopProfileChannel>().LocalProfile;
            if(profile!=null)cleared=profile.campaignCleared;
            for(int i=0;i<3;i++)nodes[i].GetComponentInChildren<Text>().text=campaign.At(i).displayName+"\n\n"+((cleared&(1u<<i))!=0?"CONCLUÍDA":i==0?"INICIAL":i==1?"VETERANO":"DESAFIADOR");
            combat.text=campaign.Level.displayName+"\n"+campaign.Objective+(profile!=null?"\nMoedas: "+(profile.expedition?.coins??profile.coins)+" · saque: "+(profile.expedition?.items.Count??0):"");
            if(active)combat.text+="\nProgresso: "+Mathf.FloorToInt(campaign.Completion*100)+"%"+(campaign.Solo?" · SOLO":"")+(director.ConnectedCount>=4?" · grupo completo":campaign.Completion>=.2f?" · novas entradas encerradas":" · pode receber parceiros");
            if(active&&local!=null&&!local.Health.IsAlive)combat.text="VOCÊ CAIU · aguarde o grupo vencer para ser erguido.\n"+combat.text;
            if(active&&campaign.Adventure!=null)
            {
                combat.text+="\n"+campaign.Adventure.Objective;
                string nearby=campaign.Adventure.NearbyLabel(local?.Health);if(nearby.Length>0)combat.text+="\n"+nearby;
            }
            foreach(var enemy in director.Dungeon.Encounter.Enemies.Actors)if(enemy.gameObject.activeInHierarchy&&enemy.Health.IsAlive&&enemy.Profile!=null&&enemy.Profile.kind==EnemyKind.Boss){combat.text+="\n"+(enemy.Profile.miniboss?"ELITE · ":"CHEFE · ")+enemy.Profile.displayName+" · "+enemy.Health.Current+" / "+enemy.Health.Maximum;break;}
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
