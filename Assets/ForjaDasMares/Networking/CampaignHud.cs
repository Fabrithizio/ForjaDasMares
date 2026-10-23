using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    public sealed class CampaignHud : MonoBehaviour
    {
        CoopDirector director;CoopCloud cloud;ArenaInput controls;CampaignRun campaign;
        RectTransform root,healthFill,superFill;GameObject map,side,touch,backdrop,vitals,objective;Text title,details,state,party,combat;InputField code;
        Button create,find,join,start,hero,leave,copyInvite,visibility,liveVisibility,homeButton,lobbyCopy;
        Text actionStatus; Button dodgeAction,specialAction,superAction,attackAction; GameObject home; CampaignSessionMenu sessionMenu; Button menuButton; Text roomCode;
        EquipmentPaperDoll portrait; CampaignHeroPicker heroPicker; Button solo; GameObject loading,group; Text loadingText,routeInfo,homeNotice; readonly Text[] members=new Text[4];bool soloPending;
        CoopPhase previousPhase=CoopPhase.Offline;
        public bool AtHome {get;private set;}=true;
        readonly Button[] nodes=new Button[3];float refresh;uint cleared;
        public void Build(RectTransform parent,CoopDirector owner,ArenaInput input)
        {
            root=parent;director=owner;controls=input;campaign=owner.Campaign;gameObject.AddComponent<CampaignAdventureHud>().Configure(owner,owner.GetComponent<CampaignAdventureChannel>(),parent);cloud=owner.GetComponent<CoopCloud>();
            var shade=RusticHudTheme.Rect(root,"CampaignBackdrop",Vector2.zero,Vector2.zero,Vector2.zero);shade.anchorMax=Vector2.one;shade.offsetMin=shade.offsetMax=Vector2.zero;MenuArtwork.Fill(shade,"JourneyBackdrop","MenuArt/RoutesBackdrop",new Color(.48f,.55f,.55f,1));backdrop=shade.gameObject;
            var content=RusticHudTheme.Rect(root,"CampaignContent",Vector2.one*.5f,Vector2.zero,new Vector2(1280,720));
            var background=RusticHudTheme.Rect(content,"CampaignMap",new Vector2(0,1),new Vector2(20,-25),new Vector2(860,655));map=background.gameObject;RusticHudTheme.Panel(background,new Color(.10f,.16f,.13f,.98f),24);
            Label(background,"AS ROTAS DA MARÉ",30,-16,790,54,30);
            Label(background,"1 · Escolha a fase     2 · Escolha seu herói     3 · Entre na expedição",30,-76,800,40,17);
            var worldMap=RusticHudTheme.Rect(background,"ExpeditionArtwork",new Vector2(0,1),new Vector2(20,-126),new Vector2(820,345));
            MenuArtwork.Fill(worldMap,"OriginalMap","MenuArt/CampaignMap",new Color(.76f,.82f,.80f,1));
            Vector2[] points={new Vector2(40,-345),new Vector2(318,-285),new Vector2(595,-185)};
            for(int i=0;i<3;i++)
            {
                int level=i;nodes[i]=RusticHudTheme.Button(background,"Level_"+(i+1),new Vector2(0,1),points[i],new Vector2(215,106),campaign.At(i).displayName,()=>director.SelectCampaignLevel(level));
                var text=nodes[i].GetComponentInChildren<Text>();text.fontSize=18;
            }
            routeInfo=Label(background,"",30,-482,800,125,18);
            Label(background,"Todas as fases estão disponíveis para teste. Sem bônus por jogar solo.",30,-613,800,30,16);
            var groupRect=RusticHudTheme.Rect(background,"LobbyParty",new Vector2(0,1),new Vector2(25,-477),new Vector2(810,160));group=groupRect.gameObject;
            RusticHudTheme.Panel(groupRect,new Color(.035f,.065f,.06f,1),18);
            Label(groupRect,"SEU GRUPO · ATÉ 4 JOGADORES",15,-8,780,40,20);
            for(int i=0;i<4;i++)
            {
                var card=RusticHudTheme.Rect(groupRect,"PartySlot_"+i,new Vector2(0,1),new Vector2(15+i*197,-55),new Vector2(188,90));RusticHudTheme.Panel(card,RusticHudTheme.Surface,12);
                members[i]=Label(card,"",10,-5,168,80,16);
            }
            var panel=RusticHudTheme.Rect(content,"CampaignSession",new Vector2(0,1),new Vector2(900,-25),new Vector2(350,655));side=panel.gameObject;RusticHudTheme.Panel(panel,RusticHudTheme.Ink,24);
            title=Label(panel,"",20,-15,310,55,23);details=Label(panel,"",20,-72,310,85,16);
            hero=Button(panel,"Hero",20,-180,310,42,"ESCOLHER HERÓI",()=>heroPicker.Open());
            solo=Button(panel,"Solo",20,-237,310,44,"JOGAR SOLO",BeginSolo);
            create=Button(panel,"Private",20,-291,310,44,"CRIAR SALA PRIVADA",()=>cloud.Create());
            visibility=Button(panel,"Visibility",20,-237,310,44,"ABRIR SALA PARA GRUPOS",()=>cloud.ToggleVisibility());
            find=Button(panel,"FindGroup",20,-345,310,44,"PROCURAR GRUPO",()=>cloud.FindCampaignGroup());
            code=Field(panel,20,-400,310,42);
            join=Button(panel,"JoinCode",20,-452,310,44,"ENTRAR PELO CÓDIGO",()=>cloud.Join(code.text));
            start=Button(panel,"Start",20,-345,310,52,"INICIAR COM O GRUPO",()=>{if(director.Phase==CoopPhase.Lobby)director.StartGame();else director.ReturnToCampaignMap();});
            state=Label(panel,"",20,-510,310,130,16);
            var invite=Button(panel,"LobbyCopyCode",20,-291,310,44,"COPIAR CÓDIGO",()=>{if(!string.IsNullOrEmpty(director.Pin))GUIUtility.systemCopyBuffer=director.Pin;});
            lobbyCopy=invite;
            var life=RusticHudTheme.Rect(root,"CampaignVitals",new Vector2(0,1),new Vector2(190,-18),new Vector2(280,115));RusticHudTheme.Panel(life,RusticHudTheme.Ink,20);vitals=life.gameObject;
            var head=RusticHudTheme.Rect(life,"HeroPortrait",new Vector2(0,1),new Vector2(8,-8),new Vector2(64,70));portrait=life.gameObject.AddComponent<EquipmentPaperDoll>();portrait.Configure(head.gameObject.AddComponent<RawImage>(),true);
            party=Label(life,"",82,-8,190,79,13);
            var bar=RusticHudTheme.Rect(life,"HealthTrack",new Vector2(0,1),new Vector2(10,-92),new Vector2(260,10));bar.gameObject.AddComponent<Image>().color=RusticHudTheme.Surface;
            healthFill=RusticHudTheme.Rect(bar,"Health",new Vector2(0,1),Vector2.zero,new Vector2(260,10));healthFill.gameObject.AddComponent<Image>().color=new Color(.75f,.15f,.14f);
            var superTrack=RusticHudTheme.Rect(life,"SuperTrack",new Vector2(0,1),new Vector2(10,-107),new Vector2(260,4));superTrack.gameObject.AddComponent<Image>().color=RusticHudTheme.Surface;
            superFill=RusticHudTheme.Rect(superTrack,"Super",new Vector2(0,1),Vector2.zero,new Vector2(0,4));superFill.gameObject.AddComponent<Image>().color=RusticHudTheme.Gold;
            var goal=RusticHudTheme.Rect(root,"CampaignObjective",new Vector2(0,1),new Vector2(485,-18),new Vector2(350,90));RusticHudTheme.Panel(goal,RusticHudTheme.Ink,20);objective=goal.gameObject;
            combat=Label(goal,"",12,-8,328,76,13);
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
            var codePanel=RusticHudTheme.Rect(root,"LiveRoomCode",new Vector2(1,1),new Vector2(-20,-20),new Vector2(220,52));
            RusticHudTheme.Panel(codePanel,RusticHudTheme.Ink,18);
            roomCode=Label(codePanel,"",12,-6,196,40,15);
            menuButton=RusticHudTheme.Button(root,"SessionMenuButton",new Vector2(1,1),new Vector2(-20,-80),new Vector2(150,40),Application.isMobilePlatform?"MENU":"MENU · ESC",()=>sessionMenu.Open());
            var homeRect=RusticHudTheme.Rect(root,"InitialScreen",Vector2.zero,Vector2.zero,Vector2.zero);homeRect.anchorMax=Vector2.one;homeRect.offsetMin=homeRect.offsetMax=Vector2.zero;home=homeRect.gameObject;
            home.AddComponent<CampaignHome>().Build(homeRect,director,()=>AtHome=false,()=>GetComponent<CoopInventoryHud>().OpenInventory());
            sessionMenu=gameObject.AddComponent<CampaignSessionMenu>();sessionMenu.Build(root,director,controlsGroup);
            heroPicker=gameObject.AddComponent<CampaignHeroPicker>();heroPicker.Build(root,director);
            homeNotice=RusticHudTheme.Label(homeRect,"ConnectionNotice",new Vector2(1,1),new Vector2(-35,-35),new Vector2(600,110),"",18);
            var load=RusticHudTheme.Rect(root,"CampaignLoading",Vector2.zero,Vector2.zero,Vector2.zero);load.anchorMax=Vector2.one;load.offsetMin=load.offsetMax=Vector2.zero;load.gameObject.AddComponent<Image>().color=new Color(.01f,.02f,.025f,.94f);loading=load.gameObject;MenuArtwork.Fill(load,"LoadingArtwork","MenuArt/RoutesBackdrop",new Color(.42f,.48f,.48f,1));
            var loadPanel=RusticHudTheme.Rect(load,"LoadingCard",Vector2.one*.5f,Vector2.zero,new Vector2(840,350));RusticHudTheme.Panel(loadPanel,RusticHudTheme.Ink,24);
            Label(loadPanel,"PREPARANDO A EXPEDIÇÃO",35,-25,770,65,30);
            loadingText=Label(loadPanel,"",35,-105,770,135,20);
            Button(loadPanel,"CancelConnection",35,-265,770,52,"CANCELAR E VOLTAR",()=>{soloPending=false;cloud.Cancel();});loading.SetActive(false);
            gameObject.AddComponent<CampaignCombatReadout>().Build(root,director);
            var minimap=new GameObject("CampaignMinimap",typeof(RectTransform));minimap.AddComponent<CampaignMinimap>().Build(root,director);
        }
        public void BeginSolo()
        {
            if(!cloud.Configured||cloud.Busy||director.Manager.IsListening||director.Phase!=CoopPhase.Offline&&director.Phase!=CoopPhase.Closed)return;
            soloPending=true;cloud.Create();
        }
        public static bool ValidCode(string value)
        {if(string.IsNullOrWhiteSpace(value)||value.Trim().Length<4||value.Trim().Length>12)return false;foreach(char c in value.Trim())if(!(c>='A'&&c<='Z'||c>='a'&&c<='z'||c>='0'&&c<='9'))return false;return true;}
        public static string PhaseLabel(CoopPhase phase)=>phase==CoopPhase.Lobby?"PREPARAÇÃO":phase==CoopPhase.Loading?"CARREGANDO":phase==CoopPhase.Connecting?"CONECTANDO":phase==CoopPhase.Victory?"VITÓRIA":phase==CoopPhase.Defeat?"DERROTA":phase==CoopPhase.Playing?"EM PARTIDA":"ENCERRADA";
        public static string ConnectionStatus(CoopDirector owner,CoopCloud service)
        {
            if(service.Busy)return service.Message;
            if(!string.IsNullOrEmpty(service.LastError))return service.LastError+"\nTente novamente ou confira o código e a internet.";
            if(owner.Phase==CoopPhase.Closed)return ClosedNotice(owner.Message);
            return service.Configured?"Escolha como jogar. Solo também cria uma sala privada pela internet. Procurar grupo cria uma sala aberta se não encontrar vagas.":"Internet não configurada. Confira os serviços do projeto antes de conectar.";
        }
        public static string ClosedNotice(string message)
        {
            if(!string.IsNullOrEmpty(message)&&message.IndexOf("host shutting down",System.StringComparison.OrdinalIgnoreCase)>=0)return "O host encerrou a sala. Peça a ele para criar outra sala e compartilhar o novo código.";
            return message;
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
            if(soloPending&&!cloud.Busy)
            {
                if(director.Phase==CoopPhase.Lobby&&director.IsHost){soloPending=false;director.StartGame();}
                else if(director.Phase==CoopPhase.Closed)soloPending=false;
            }
            bool preparing=cloud.Busy||director.Phase==CoopPhase.Loading||director.Phase==CoopPhase.Connecting;
            loading.SetActive(preparing&&!active);if(preparing&&!active){loading.transform.SetAsLastSibling();loadingText.text=campaign.Level.displayName+"\n\n"+(cloud.Busy?cloud.Message:director.Message)+"\nAguarde a conexão e a confirmação do mapa pelo grupo.";}
            bool offline=director.Phase==CoopPhase.Offline||director.Phase==CoopPhase.Closed;
            if(offline&&previousPhase!=CoopPhase.Offline&&previousPhase!=CoopPhase.Closed)AtHome=true;previousPhase=director.Phase;
            if(!offline)AtHome=false;
            home.SetActive(AtHome&&!preparing);homeButton.gameObject.SetActive(offline&&!AtHome&&!preparing);
            backdrop.SetActive(!active);vitals.SetActive(active);objective.SetActive(active);map.SetActive(!active);side.SetActive(!active);party.gameObject.SetActive(active);combat.gameObject.SetActive(active);touch.SetActive(active);
            if(AtHome||preparing){map.SetActive(false);side.SetActive(false);}
            title.text=campaign.Level.displayName;details.text=campaign.Level.description;
            routeInfo.gameObject.SetActive(offline);group.SetActive(!offline);
            routeInfo.text="FASE "+(campaign.Selected+1)+" · "+(campaign.Selected==0?"INICIAL":campaign.Selected==1?"VETERANO":"DESAFIADOR")+" · "+campaign.Level.route.Length+" setores\nOuro e experiência nos inimigos. Equipamentos em drops, baús e chefes.\nEscolha Jogar solo, procurar grupo ou criar uma sala para convidar.";
            for(int i=0;i<members.Length;i++)members[i].text=director.IsConnected(i)?(i==director.LocalSlot?"VOCÊ":"ALIADO "+(i+1))+"\n"+director.Actor(i).Roster.Current.displayName+"\nCONECTADO":"VAGA LIVRE\nConvide pelo código";
            homeNotice.text=director.Phase==CoopPhase.Closed?ConnectionStatus(director,cloud):"";
            for(int i=0;i<3;i++)
            {
                nodes[i].interactable=(offline||director.IsHost&&director.Phase==CoopPhase.Lobby)&&!cloud.Busy;
                var surface=nodes[i].GetComponentInChildren<RoundedHudGraphic>();var color=i==campaign.Selected?RusticHudTheme.Gold:RusticHudTheme.Copper;
                if(surface.borderColor!=color){surface.borderColor=color;surface.SetVerticesDirty();}
            }
            leave.gameObject.SetActive((!offline||cloud.Busy)&&!active);copyInvite.gameObject.SetActive(false);
            menuButton.gameObject.SetActive(active);roomCode.transform.parent.gameObject.SetActive(active);
            roomCode.text="SALA  "+(string.IsNullOrEmpty(director.Pin)?"…":director.Pin)+"\n"+director.ConnectedCount+"/4 · "+(cloud.PublicRoom?"ABERTA":"PRIVADA");
            copyInvite.interactable=!string.IsNullOrEmpty(director.Pin);
            foreach(var button in new[]{leave,copyInvite})
            {
                var rect=(RectTransform)button.transform;rect.anchorMin=rect.anchorMax=rect.pivot=active?Vector2.one:new Vector2(1,0);
                rect.anchoredPosition=new Vector2(button==leave?-20:-220,active?-20:20);
            }
            hero.GetComponentInChildren<Text>().text="HERÓI · "+director.HeroName;hero.interactable=(offline||director.Phase==CoopPhase.Lobby)&&!cloud.Busy;
            solo.gameObject.SetActive(offline);solo.interactable=offline&&!cloud.Busy&&!director.Manager.IsListening;
            lobbyCopy.gameObject.SetActive(!offline);lobbyCopy.interactable=!string.IsNullOrEmpty(director.Pin);
            create.gameObject.SetActive(offline);find.gameObject.SetActive(offline);join.gameObject.SetActive(offline);code.gameObject.SetActive(offline);
            visibility.gameObject.SetActive(!offline&&director.IsHost);visibility.interactable=cloud.CanChangeVisibility&&!cloud.Busy;
            string visibilityLabel=cloud.PublicRoom?"TORNAR SALA PRIVADA":"ABRIR SALA PARA GRUPOS";
            visibility.GetComponentInChildren<Text>().text=visibilityLabel;liveVisibility.GetComponentInChildren<Text>().text=visibilityLabel;
            liveVisibility.gameObject.SetActive(false);liveVisibility.interactable=cloud.CanChangeVisibility&&!cloud.Busy;
            create.interactable=find.interactable=offline&&!cloud.Busy&&!director.Manager.IsListening;
            join.interactable=create.interactable&&ValidCode(code.text);
            start.gameObject.SetActive(!offline);start.interactable=director.IsHost&&!cloud.Busy&&(director.Phase==CoopPhase.Lobby&&director.ConnectedCount>=1||director.Phase==CoopPhase.Victory||director.Phase==CoopPhase.Defeat);
            start.GetComponentInChildren<Text>().text=director.Phase==CoopPhase.Lobby?(director.ConnectedCount==1?"INICIAR SOLO":"INICIAR COM O GRUPO"):"CONTINUAR CAMPANHA";
            state.text=offline?ConnectionStatus(director,cloud):"Sala "+(cloud.PublicRoom?"aberta para grupos":"privada")+" · "+PhaseLabel(director.Phase)+"\nCódigo: "+director.Pin+"\nGrupo "+director.ConnectedCount+"/4"+(director.ConnectedCount==1?" · pode jogar solo":"")+"\n"+(director.IsHost?"Você é o host: sair encerra a sala.":"Aguarde o host iniciar a partida.");
            var local=director.Actor(director.LocalSlot);var liveProfile=director.GetComponent<CoopProfileChannel>().LocalProfile;
            string allies="";for(int i=0;i<4;i++)if(i!=director.LocalSlot&&director.IsConnected(i)){var ally=director.Actor(i);allies+=" · P"+(i+1)+" "+ally.Health.Current;}
            party.text=local==null?"":local.Roster.Current.displayName+" · Nv "+(liveProfile!=null?ProgressionStats.Level(liveProfile.experience):1)+"\n"+local.Health.Current+" / "+local.Health.Maximum+" VIDA\n"+(liveProfile?.coins??0)+" OURO"+allies;
            healthFill.sizeDelta=new Vector2(local!=null&&local.Health.Maximum>0?260f*local.Health.Current/local.Health.Maximum:0,10);
            portrait.SetVisible(active);if(local!=null&&liveProfile!=null&&active)portrait.Refresh(local.Presentation,liveProfile,local.Roster.Current.heroId);
            if(local!=null&&active)
            {
                superFill.sizeDelta=new Vector2(local.SuperProgress*260,4);superFill.parent.gameObject.SetActive(local.Enhanced);
                if(superAction!=null){superAction.gameObject.SetActive(local.Enhanced);superAction.interactable=local.Health.IsAlive&&local.SuperProgress>=1&&local.FuryRemaining<=0;string name=local.Roster.Current.heroId=="warrior"?"FÚRIA":local.Roster.Current.heroId=="arcanist"?"PRISMA":"SUPER";superAction.GetComponentInChildren<Text>().text=local.FuryRemaining>0?"FÚRIA\n"+local.FuryRemaining.ToString("F1")+"s":name+"\n"+(local.SuperProgress>=1?"PRONTA":Mathf.FloorToInt(local.SuperProgress*100)+"%");}
                if(attackAction!=null){attackAction.interactable=local.Health.IsAlive;attackAction.GetComponentInChildren<Text>().text=local.Enhanced&&local.Charge>0?"Ataque\nCarga "+Mathf.FloorToInt(local.Charge*100)+"%":"Ataque\nSegure";}
                string skill=local.SpecialCooldown>0?local.SpecialCooldown.ToString("F1")+"s":"pronta",dodge=local.DodgeCooldown>0?local.DodgeCooldown.ToString("F1")+"s":"pronta";
                if(specialAction!=null){specialAction.GetComponentInChildren<Text>().text="Habilidade\n"+skill;specialAction.interactable=local.Health.IsAlive&&local.SpecialCooldown<=0;}
                if(dodgeAction!=null){dodgeAction.GetComponentInChildren<Text>().text="Esquiva\n"+dodge;dodgeAction.interactable=local.Health.IsAlive&&local.DodgeCooldown<=0;}
                if(actionStatus!=null)actionStatus.text="WASD · mover   Mouse · segure para atacar   F · interagir\nE · habilidade: "+skill+"   Espaço · esquiva: "+dodge+"   Esc · menu"+(local.Enhanced?"   Q · SUPER "+Mathf.FloorToInt(local.SuperProgress*100)+"%":"");
            }
            var profile=director.GetComponent<CoopProfileChannel>().LocalProfile;
            if(profile!=null)cleared=profile.campaignCleared;
            for(int i=0;i<3;i++)nodes[i].GetComponentInChildren<Text>().text=campaign.At(i).displayName+"\n\n"+((cleared&(1u<<i))!=0?"CONCLUÍDA":i==0?"INICIAL":i==1?"VETERANO":"DESAFIADOR");
            combat.text=campaign.Level.displayName+" · "+Mathf.FloorToInt(campaign.Completion*100)+"%\n"+campaign.Objective;
            if(active&&local!=null&&!local.Health.IsAlive)combat.text="VOCÊ CAIU · aguarde o grupo\n"+combat.text;
            if(active&&campaign.Adventure!=null){string nearby=campaign.Adventure.NearbyLabel(local?.Health);if(nearby.Length>0)combat.text+="\n"+nearby;}
            foreach(var enemy in director.Dungeon.Encounter.Enemies.Actors)if(enemy.gameObject.activeInHierarchy&&enemy.Health.IsAlive&&enemy.Profile!=null&&enemy.Profile.kind==EnemyKind.Boss){combat.text+="\n"+(enemy.Profile.miniboss?"ELITE · ":"CHEFE · ")+enemy.Profile.displayName+" "+enemy.Health.Current+"/"+enemy.Health.Maximum;break;}

        }
        static Text Label(Transform p,string s,float x,float y,float w,float h,int font)=>RusticHudTheme.Label(p,"Text",new Vector2(0,1),new Vector2(x,y),new Vector2(w,h),s,font);
        static Button Button(Transform p,string n,float x,float y,float w,float h,string s,UnityEngine.Events.UnityAction a)=>RusticHudTheme.Button(p,n,new Vector2(0,1),new Vector2(x,y),new Vector2(w,h),s,a);
        static InputField Field(Transform parent,float x,float y,float w,float h)
        {
            var rect=RusticHudTheme.Rect(parent,"InvitationCode",new Vector2(0,1),new Vector2(x,y),new Vector2(w,h));var image=rect.gameObject.AddComponent<Image>();image.color=new Color(.22f,.25f,.20f);
            var text=Label(rect,"",10,0,w-20,h,23);var field=rect.gameObject.AddComponent<InputField>();field.textComponent=text;field.characterLimit=12;field.characterValidation=InputField.CharacterValidation.Alphanumeric;
            var placeholder=Label(rect,"CÓDIGO DO CONVITE",10,0,w-20,h,16);placeholder.color=RusticHudTheme.Muted;field.placeholder=placeholder;return field;
        }
    }
}
