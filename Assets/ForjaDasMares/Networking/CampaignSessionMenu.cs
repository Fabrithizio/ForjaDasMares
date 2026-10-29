using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    // Local overlay only: the host simulation and the other players continue running.
    public sealed class CampaignSessionMenu : MonoBehaviour
    {
        CoopDirector director; CoopCloud cloud; GameObject overlay,room,friends,options,confirm,expedition;
        readonly Text[] routeNodes=new Text[24];Text routeTitle;
        Text roomInfo,friendInfo,feedback,volume,opacity,warning; Button visibility;
        CanvasGroup controls; float refresh;InputAction menuAction;bool menuRequested;
        public bool IsOpen => overlay!=null&&overlay.activeSelf;
        public void Build(RectTransform parent,CoopDirector owner,CanvasGroup touch)
        {
            director=owner;cloud=owner.GetComponent<CoopCloud>();controls=touch;
            controls.alpha=Mathf.Clamp(PlayerPrefs.GetFloat("CampaignControlOpacity",.85f),.35f,1);
            var full=RusticHudTheme.Rect(parent,"SessionMenu",Vector2.zero,Vector2.zero,Vector2.zero);
            full.anchorMax=Vector2.one;full.offsetMin=full.offsetMax=Vector2.zero;
            full.gameObject.AddComponent<Image>().color=new Color(.015f,.025f,.02f,.88f);overlay=full.gameObject;
            var panel=RusticHudTheme.Rect(full,"MenuPanel",Vector2.one*.5f,Vector2.zero,new Vector2(1120,650));RusticHudTheme.Panel(panel,RusticHudTheme.Ink,24);
            Label(panel,"MENU DA EXPEDIÇÃO",30,18,780,48,28);
            Label(panel,"Menu local · a partida online continua. Abra em um lugar seguro.",30,65,1040,38,17);
            Button(panel,"Resume",850,18,240,Application.isMobilePlatform?"VOLTAR AO JOGO":"VOLTAR · ESC",Close);
            room=Page(panel,"Room");friends=Page(panel,"Invitations");options=Page(panel,"Settings");confirm=Page(panel,"LeaveConfirmation");expedition=Page(panel,"Expedition");
            Button(panel,"RoomTab",30,115,245,"SALA E GRUPO",()=>Show(room));
            Button(panel,"FriendsTab",305,115,245,"CONVITES",()=>{RefreshFriends();Show(friends);});
            Button(panel,"SettingsTab",855,115,235,"CONFIGURAÇÕES",()=>Show(options));
            Button(panel,"ExpeditionTab",580,115,245,"MAPA E OBJETIVOS",()=>{RefreshRoute();Show(expedition);});
            routeTitle=Label(expedition.transform,"",30,170,1040,60,20);
            for(int i=0;i<routeNodes.Length;i++)
            {
                var node=RusticHudTheme.Rect(expedition.transform,"RouteNode_"+i,new Vector2(0,1),Vector2.zero,new Vector2(230,54));RusticHudTheme.Panel(node,RusticHudTheme.Surface,10);
                routeNodes[i]=Label(node,"",6,2,218,50,15);
            }
            Label(expedition.transform,"Dourado: setor atual · verde: concluído · cinza: ainda por explorar.\nEntrem juntos no próximo setor para iniciar o encontro. O mapa não teleporta o grupo.",30,565,1040,55,16);
            roomInfo=Label(room.transform,"",30,185,1040,220,22);
            Button(room.transform,"CopyCode",30,415,330,"COPIAR CÓDIGO",()=>Copy(false));
            Button(room.transform,"CopyInvitation",395,415,330,"COPIAR CONVITE",()=>Copy(true));
            visibility=Button(room.transform,"Visibility",760,415,330,"",()=>cloud.ToggleVisibility());
            Button(room.transform,"Inventory",30,485,330,"PERSONAGEM E EQUIPAMENTOS",()=>{Close();GetComponent<CoopInventoryHud>()?.OpenInventory();});
            Button(room.transform,"Leave",760,485,330,"SAIR DA SALA…",()=>{warning.text=director.IsHost?"ENCERRAR A SALA?\nVocê é o host: todos serão desconectados.\nO saque temporário ainda não concluído não é garantido.":"SAIR DA SALA?\nOs demais continuam jogando. Para voltar, use o código\ne mantenha o host e esta tentativa abertos.";Show(confirm);});
            feedback=Label(panel,"",30,575,1040,48,18);
            friendInfo=Label(friends.transform,"",30,180,1040,315,18);
            Button(friends.transform,"CopyFriendInvite",30,500,500,"COPIAR CONVITE PARA COMPARTILHAR",()=>Copy(true));
            Button(options.transform,"Guide",30,510,500,"REVER PRIMEIROS PASSOS",()=>{GetComponent<CampaignFirstSteps>()?.RestartGuide();Close();},40);
            Button(options.transform,"Performance",560,510,470,"DIAGNÓSTICO DE DESEMPENHO",()=>{GetComponent<CampaignPerformanceHud>()?.Toggle();Close();},40);
            Label(options.transform,"VOLUME GERAL",30,180,480,35,22);
            Button(options.transform,"VolumeDown",30,230,210,"− VOLUME",()=>Volume(-.1f));
            Button(options.transform,"VolumeUp",260,230,210,"+ VOLUME",()=>Volume(.1f));
            volume=Label(options.transform,"",500,230,150,48,24);
            Label(options.transform,"VISIBILIDADE DOS CONTROLES",30,305,700,35,22);
            Button(options.transform,"OpacityDown",30,350,210,"MAIS DISCRETO",()=>Opacity(-.1f));
            Button(options.transform,"OpacityUp",260,350,210,"MAIS VISÍVEL",()=>Opacity(.1f));
            opacity=Label(options.transform,"",500,350,150,48,24);
            Button(options.transform,"Shake",700,230,330,"TREMOR: "+(PlayerPrefs.GetInt("CampaignCameraShake",0)==1?"SIM":"NÃO"),()=>ToggleOption("CampaignCameraShake","Shake","TREMOR"));
            Button(options.transform,"Vibration",700,350,330,"VIBRAÇÃO: "+(PlayerPrefs.GetInt("CampaignVibration",0)==1?"SIM":"NÃO"),()=>ToggleOption("CampaignVibration","Vibration","VIBRAÇÃO"));
            Button(options.transform,"Effects",30,560,500,"EFEITOS: "+Mathf.RoundToInt(PlayerPrefs.GetFloat("CampaignEffectsVolume",.8f)*100)+"%",()=>{float v=PlayerPrefs.GetFloat("CampaignEffectsVolume",.8f);v=v<.01f?1:Mathf.Max(0,v-.2f);PlayerPrefs.SetFloat("CampaignEffectsVolume",v);PlayerPrefs.Save();options.transform.Find("Effects").GetComponentInChildren<Text>().text="EFEITOS: "+Mathf.RoundToInt(v*100)+"%";});
            Button(options.transform,"GraphicsQuality",560,560,470,"GRÁFICOS: "+PresentationQuality.ModeName(PresentationQuality.Selected),()=>{GetComponent<PresentationQuality>()?.Cycle();options.transform.Find("GraphicsQuality").GetComponentInChildren<Text>().text="GRÁFICOS: "+PresentationQuality.ModeName(PresentationQuality.Selected);});
            SoundVolumeButton(options.transform,"MusicVolume",30,612,500,"MÚSICA",CampaignSoundscape.MusicKey);
            SoundVolumeButton(options.transform,"AmbienceVolume",560,612,470,"AMBIENTE",CampaignSoundscape.AmbienceKey);
            Button(options.transform,"MovementMode",560,510,470,"CONTROLES: "+DesktopRpgControls.ModeName,()=>{DesktopRpgControls.ToggleMode();options.transform.Find("MovementMode").GetComponentInChildren<Text>().text="CONTROLES: "+DesktopRpgControls.ModeName;},36);
            Label(options.transform,"PC: F8 alterna mouse RPG / WASD · Shift: parado · roda: zoom\nWASD/setas · mouse segurado: ataque · E: habilidade\nEspaço: esquiva · F: interagir · G / direcional ↑: marcar · Esc: menu · Q: SUPER\nCelular: joystick e botões à direita. Menu no canto superior direito.\nSegure ataque para o combo; solte com carga para antecipar o pesado.",30,410,1040,100,16);
            warning=Label(confirm.transform,"",60,215,1000,180,25);
            Button(confirm.transform,"CancelLeave",60,445,450,"CONTINUAR NA SALA",()=>Show(room));
            Button(confirm.transform,"ConfirmLeave",580,445,450,"CONFIRMAR SAÍDA",()=>{Close();director.Close();});
            Volume(0);Opacity(0);overlay.SetActive(false);
        }
        public void Open()
        {
            if(overlay==null||director.Phase==CoopPhase.Offline||director.Phase==CoopPhase.Closed)return;
            GetComponent<CoopInventoryHud>()?.CloseInventory();overlay.SetActive(true);overlay.transform.SetAsLastSibling();
            controls.interactable=controls.blocksRaycasts=false;director.SetInputBlocked(true);feedback.text="";Show(room);RefreshRoom();
        }
        public void Close()
        {
            if(overlay==null)return;overlay.SetActive(false);controls.interactable=controls.blocksRaycasts=true;
            if(director!=null)director.SetInputBlocked(false);
        }
        void Update()
        {
            if(overlay==null)return;
            if(menuRequested)
            {menuRequested=false;var inventory=GetComponent<CoopInventoryHud>();if(inventory!=null&&inventory.IsOpen)inventory.CloseInventory();else if(IsOpen)Close();else Open();}
            if(!IsOpen)return;
            if(director.Phase==CoopPhase.Offline||director.Phase==CoopPhase.Closed){Close();return;}
            refresh-=Time.unscaledDeltaTime;if(refresh<=0){refresh=.2f;RefreshRoom();if(expedition.activeSelf)RefreshRoute();}
        }
        void RefreshRoom()
        {
            string text="CÓDIGO DA SALA   "+(string.IsNullOrEmpty(director.Pin)?"Aguardando…":director.Pin)+"\n";
            text+=(cloud.PublicRoom?"Aberta para grupos":"Sala privada")+" · "+director.ConnectedCount+"/4 jogadores\n";
            for(int i=0;i<4;i++)if(director.IsConnected(i)){var a=director.Actor(i);text+="\n"+(i==director.LocalSlot?"Você":"Aliado "+(i+1))+" · "+a.Roster.Current.displayName+" · "+a.Health.Current+"/"+a.Health.Maximum+" vida";}
            roomInfo.text=text;visibility.interactable=director.IsHost&&cloud.CanChangeVisibility&&!cloud.Busy;
            visibility.GetComponentInChildren<Text>().text=director.IsHost?(cloud.PublicRoom?"TORNAR PRIVADA":"ABRIR PARA GRUPOS"):"VISIBILIDADE: HOST";
        }
        void RefreshRoute()
        {
            var rooms=director.Phase==CoopPhase.Lobby?null:director.Dungeon.Generator.Rooms;
            routeTitle.text=director.Campaign.Level.displayName+" · "+Mathf.FloorToInt(director.Campaign.Completion*100)+"%\n"+(director.Campaign.Adventure?.Objective??"Inicie a fase para explorar.");
            for(int i=0;i<routeNodes.Length;i++)
            {
                bool exists=rooms!=null&&i<rooms.Length;routeNodes[i].transform.parent.gameObject.SetActive(exists);if(!exists)continue;
                var value=rooms[i];var rect=(RectTransform)routeNodes[i].transform.parent;rect.anchoredPosition=new Vector2(45+value.node.coordinate.x*255,-245-value.node.coordinate.y*62);
                string category=value.node.category==RoomCategory.Entrance?"Entrada":value.node.category==RoomCategory.Boss?"Chefe":value.node.category==RoomCategory.Combat?"Combate":value.node.category==RoomCategory.HardCombat?"Emboscada":value.node.category==RoomCategory.Event?"Placas":value.node.category==RoomCategory.Trap?"Armadilhas":value.node.category==RoomCategory.Treasure?"Tesouro":"Descanso";
                routeNodes[i].text=(i+1)+" · "+category+"\n"+(value==director.Dungeon.CurrentRoom?"VOCÊS ESTÃO AQUI":value.completed?"Concluído":value.entered?"Explorado":"A explorar");
                routeNodes[i].color=value==director.Dungeon.CurrentRoom?RusticHudTheme.Gold:value.completed?new Color(.4f,1,.6f):RusticHudTheme.Muted;
            }
        }
        void Copy(bool invitation)
        {
            if(string.IsNullOrEmpty(director.Pin)){feedback.text="O código ainda não está disponível.";return;}
            GUIUtility.systemCopyBuffer=invitation?"Vamos jogar Forja das Marés "+Application.version+"! Campanha → Entrar pelo código: "+director.Pin+". Mantenho a sala aberta para você.":director.Pin;
            feedback.text=invitation?"Convite copiado. Cole na conversa com seu amigo.":"Código copiado: "+director.Pin;
        }
        void RefreshFriends()
        {
            string text="CONVIDE PELO CÓDIGO DA SALA\nCompartilhe o convite na conversa com seu amigo.\nAgenda local deste aparelho (sem presença online):\n";
            try{var saved=JsonUtility.FromJson<CampaignHome.Invitations>(PlayerPrefs.GetString("CampaignInvitations-v1",""));if(saved?.entries!=null)foreach(var entry in saved.entries)if(entry!=null)text+="\n"+entry.name+" · "+entry.code;}
            catch(System.ArgumentException){text+="\nAgenda indisponível.";}
            friendInfo.text=text+"\n\nPara entrar em outra sala, saia da atual e use Amigos no início.\nCódigos deixam de valer quando o host encerra a sala.";
        }
        void ToggleOption(string key,string button,string label){int v=1-PlayerPrefs.GetInt(key,0);PlayerPrefs.SetInt(key,v);PlayerPrefs.Save();options.transform.Find(button).GetComponentInChildren<Text>().text=label+": "+(v==1?"SIM":"NÃO");}
        void Volume(float step){AudioListener.volume=Mathf.Clamp01(AudioListener.volume+step);PlayerPrefs.SetFloat("CampaignMasterVolume",AudioListener.volume);PlayerPrefs.Save();volume.text=Mathf.RoundToInt(AudioListener.volume*100)+"%";}
        void RefreshAudioLabels()
        {
            var music=options.transform.Find("MusicVolume");var ambient=options.transform.Find("AmbienceVolume");
            var mode=options.transform.Find("MovementMode");if(mode!=null)mode.GetComponentInChildren<Text>().text="CONTROLES: "+DesktopRpgControls.ModeName;
            if(music!=null)music.GetComponentInChildren<Text>().text="MÚSICA: "+Mathf.RoundToInt(CampaignSoundscape.Volume(CampaignSoundscape.MusicKey)*100)+"% · AJUSTAR";
            if(ambient!=null)ambient.GetComponentInChildren<Text>().text="AMBIENTE: "+Mathf.RoundToInt(CampaignSoundscape.Volume(CampaignSoundscape.AmbienceKey)*100)+"% · AJUSTAR";
        }
        static void SoundVolumeButton(Transform page,string name,float x,float y,float width,string label,string key)
        {
            Button button=null;button=Button(page,name,x,y,width,label+": "+Mathf.RoundToInt(CampaignSoundscape.Volume(key)*100)+"% · AJUSTAR",()=>{float value=CampaignSoundscape.CycleVolume(key);button.GetComponentInChildren<Text>().text=label+": "+Mathf.RoundToInt(value*100)+"% · AJUSTAR";},32);
        }
        void Opacity(float step){controls.alpha=Mathf.Clamp(controls.alpha+step,.35f,1);PlayerPrefs.SetFloat("CampaignControlOpacity",controls.alpha);PlayerPrefs.Save();opacity.text=Mathf.RoundToInt(controls.alpha*100)+"%";}
        void Show(GameObject page){room.SetActive(page==room);friends.SetActive(page==friends);options.SetActive(page==options);confirm.SetActive(page==confirm);expedition.SetActive(page==expedition);feedback.text="";if(page==options){volume.text=Mathf.RoundToInt(AudioListener.volume*100)+"%";RefreshAudioLabels();}}
        static GameObject Page(Transform parent,string name){var r=RusticHudTheme.Rect(parent,name,Vector2.zero,Vector2.zero,Vector2.zero);r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r.gameObject;}
        static Text Label(Transform p,string text,float x,float y,float w,float h,int size)=>RusticHudTheme.Label(p,"Text",new Vector2(0,1),new Vector2(x,-y),new Vector2(w,h),text,size);
        static Button Button(Transform p,string name,float x,float y,float w,string text,UnityEngine.Events.UnityAction action,float height=50)=>RusticHudTheme.Button(p,name,new Vector2(0,1),new Vector2(x,-y),new Vector2(w,height),text,action);
        void OnEnable()
        {
            if(menuAction==null){menuAction=new InputAction("SessionMenu",InputActionType.Button);menuAction.AddBinding("<Keyboard>/escape");menuAction.AddBinding("<Gamepad>/start");menuAction.performed+=MenuPressed;}
            menuAction.Enable();
        }
        void MenuPressed(InputAction.CallbackContext context)=>menuRequested=true;
        void OnDisable(){menuAction?.Disable();menuRequested=false;if(IsOpen)Close();}
        void OnDestroy(){menuAction?.Dispose();}
    }
}
