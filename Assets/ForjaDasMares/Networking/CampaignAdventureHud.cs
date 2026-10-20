using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    public sealed class CampaignAdventureHud:MonoBehaviour
    {
        CoopDirector session;CampaignAdventureChannel channel;RectTransform root;GameObject panel,pingBar;Text title,summary,eventInfo,counter,boons;Button use;
        static readonly string[] PingNames={"VENHA AQUI","FOCO NO ALVO","PERIGO","SAQUE AQUI"};static readonly Color[] PingColors={Color.cyan,new Color(1,.55f,.2f),new Color(1,.25f,.15f),RusticHudTheme.Gold};
        readonly Button[] choices=new Button[3];readonly Text[] markers=new Text[4];int nearby=-1,lastChallenge=-1;float resultUntil;string resultNotice;bool built;InputAction ping;
        public void Configure(CoopDirector owner,CampaignAdventureChannel network,RectTransform parent){session=owner;channel=network;root=parent;}
        void Build()
        {
            if(root==null||channel==null)return;built=true;
            var rect=RusticHudTheme.Rect(root,"NearbyAdventure",Vector2.one*.5f,new Vector2(0,-100),new Vector2(800,190));panel=rect.gameObject;RusticHudTheme.Panel(rect,RusticHudTheme.Ink,18).raycastTarget=true;
            title=RusticHudTheme.Label(rect,"Title",new Vector2(0,1),new Vector2(18,-12),new Vector2(760,36),"",22);
            summary=RusticHudTheme.Label(rect,"Summary",new Vector2(0,1),new Vector2(18,-52),new Vector2(760,45),"",16);
            for(int i=0;i<3;i++){int choice=i;choices[i]=RusticHudTheme.Button(rect,"Boon_"+i,new Vector2(0,1),new Vector2(18+i*255,-105),new Vector2(246,64),CombatPerkDescriptions.Boon(i),()=>channel.Request(AdventureAction.ChooseBoon,nearby,choice));choices[i].GetComponentInChildren<Text>().fontSize=16;}
            use=RusticHudTheme.Button(rect,"AcceptChallenge",new Vector2(0,1),new Vector2(18,-105),new Vector2(760,64),"",()=>channel.Request(AdventureAction.UseFeature,nearby));
            var bar=RusticHudTheme.Rect(root,"QuickPings",Vector2.zero,new Vector2(230,180),new Vector2(280,46));pingBar=bar.gameObject;
            string[] labels={"AQUI","ALVO","PERIGO","SAQUE"};for(int i=0;i<4;i++){int kind=i;var button=RusticHudTheme.Button(bar,"Ping_"+i,new Vector2(0,1),new Vector2(i*70,0),new Vector2(66,42),labels[i],()=>channel.Request(AdventureAction.Ping,0,kind));button.GetComponentInChildren<Text>().fontSize=12;}
            eventInfo=RusticHudTheme.Label(root,"OptionalEventProgress",new Vector2(.5f,1),new Vector2(0,-125),new Vector2(650,64),"",17);eventInfo.alignment=TextAnchor.MiddleCenter;
            counter=RusticHudTheme.Label(root,"PerfectDodge",new Vector2(.5f,0),new Vector2(0,150),new Vector2(550,36),"",19);counter.alignment=TextAnchor.MiddleCenter;counter.color=Color.cyan;
            boons=RusticHudTheme.Label(root,"ExpeditionBoons",new Vector2(0,1),new Vector2(190,-125),new Vector2(290,34),"",12);boons.raycastTarget=false;boons.color=RusticHudTheme.Gold;
            for(int i=0;i<4;i++){markers[i]=RusticHudTheme.Label(root,"PartyPing_"+i,Vector2.one*.5f,Vector2.zero,new Vector2(190,48),"",17);markers[i].alignment=TextAnchor.MiddleCenter;markers[i].raycastTarget=false;}
            ping=new InputAction("QuickPing",InputActionType.Button);ping.AddBinding("<Keyboard>/g");ping.AddBinding("<Gamepad>/dpad/up");ping.Enable();panel.SetActive(false);
        }
        void Update()
        {
            if(session==null)return;if(!built){Build();if(!built)return;}
            bool visible=session.Phase==CoopPhase.Playing&&session.LocalSlot>=0&&!(GetComponent<CoopInventoryHud>()?.IsOpen??false)&&!(GetComponent<CampaignSessionMenu>()?.IsOpen??false);
            pingBar.SetActive(visible);var features=session.Campaign?.Features;var actor=session.Actor(session.LocalSlot);panel.SetActive(false);eventInfo.gameObject.SetActive(false);counter.gameObject.SetActive(false);boons.gameObject.SetActive(false);
            for(int i=0;i<4;i++)markers[i].gameObject.SetActive(false);if(!visible||features==null||actor==null)return;
            var build=actor.GetComponent<CombatBuildEffects>();if(build!=null&&build.PackedBoons!=0){boons.gameObject.SetActive(true);boons.text="ALTARES · dano +"+Mathf.RoundToInt((build.Power-1)*100)+"% · ritmo +"+Mathf.RoundToInt((build.Tempo-1)*100)+"%\nProteção temporária "+Mathf.RoundToInt(build.Shelter*100)+"%";}
            if(ping.WasPressedThisFrame())channel.Request(AdventureAction.Ping,0,0);
            counter.gameObject.SetActive(actor.CounterReady);counter.text="ESQUIVA PERFEITA · próximo ataque +25%";
            foreach(var challenge in features.Nodes)if(challenge.kind==ExpeditionFeatureKind.Challenge&&challenge.state.stage!=lastChallenge){lastChallenge=challenge.state.stage;if(lastChallenge>=2){resultUntil=Time.unscaledTime+8;resultNotice=lastChallenge==4?"DESAFIO FALHOU · a campanha continua":lastChallenge==3?"DESAFIO VENCIDO · salvando recompensa…":"DESAFIO VENCIDO · recompensa recebida pelo grupo";}}
            if(Time.unscaledTime<resultUntil){eventInfo.gameObject.SetActive(true);eventInfo.text=resultNotice;}
            var active=features.Active;if(active!=null&&active.state.stage==1){eventInfo.gameObject.SetActive(true);eventInfo.text="DESAFIO OPCIONAL · "+Mathf.RoundToInt(active.state.progress*100)+"% · "+Mathf.CeilToInt(active.state.remaining)+"s\nObjetivo "+active.health.Current+" / "+active.health.Maximum+" vida";}
            // Replicas identify the active event from the authenticated snapshot, not local simulation.
            if(!session.IsHost)foreach(var feature in features.Nodes)if(feature.kind==ExpeditionFeatureKind.Challenge&&feature.state.stage==1){eventInfo.gameObject.SetActive(true);eventInfo.text="DESAFIO · "+Mathf.RoundToInt(feature.state.progress*100)+"% · "+Mathf.CeilToInt(feature.state.remaining)+"s · objetivo "+feature.health.Current;}
            var node=features.Nearby(actor.Health);
            if(node!=null&&actor.Health.IsAlive&&!(node.kind==ExpeditionFeatureKind.Altar&&(node.state.claimed&(1u<<session.LocalSlot))!=0)&&!session.Dungeon.Encounter.Enemies.ThreatNear(actor.transform.position,10))
            {
                nearby=node.state.id;panel.SetActive(true);bool altar=node.kind==ExpeditionFeatureKind.Altar;for(int i=0;i<3;i++)choices[i].gameObject.SetActive(altar);use.gameObject.SetActive(!altar);
                title.text=altar?"ALTAR DA EXPEDIÇÃO · escolha uma melhoria":node.kind==ExpeditionFeatureKind.Cache?"ESCONDERIJO ENCONTRADO":"DESAFIO OPCIONAL · "+(session.Campaign.Level.features.optionalEvent==OptionalEventKind.Waves?"ONDAS":session.Campaign.Level.features.optionalEvent==OptionalEventKind.Defend?"PROTEGER O SELO":"ESCOLTAR O GUIA");
                summary.text=altar?"Escolha individual, uma vez por altar. Termina ao sair desta expedição.":node.kind==ExpeditionFeatureKind.Cache?"Ouro, experiência e chance de equipamento. Recompensa única para o grupo.":"Ondas: derrote todos. Proteção: mantenha o objetivo vivo. Escolta: fique perto do guia.\nSair da área ou esgotar o tempo falha o desafio; a campanha continua.";
                use.GetComponentInChildren<Text>().text=node.kind==ExpeditionFeatureKind.Cache?"RECOLHER RECOMPENSA":"ACEITAR DESAFIO · recompensa extra ao vencer";
            }
            if(features.Pings==null)return;
            for(int i=0;i<4;i++){var mark=features.Pings[i];if(mark==null||mark.remaining<=0)continue;var screen=Camera.main.WorldToScreenPoint(mark.position+Vector3.up*1.6f);if(screen.z>0&&RectTransformUtility.ScreenPointToLocalPointInRectangle(root,screen,null,out var point)){markers[i].gameObject.SetActive(true);markers[i].rectTransform.anchoredPosition=point;markers[i].text="◆ "+PingNames[Mathf.Clamp(mark.kind,0,3)]+" · P"+(i+1);markers[i].color=PingColors[Mathf.Clamp(mark.kind,0,3)];}}
        }
        void OnEnable(){ping?.Enable();}
        void OnDisable(){ping?.Disable();}
        void OnDestroy(){ping?.Dispose();}
    }
}
