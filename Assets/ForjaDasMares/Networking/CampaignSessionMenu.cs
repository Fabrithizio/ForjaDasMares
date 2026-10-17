using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    // Local overlay only: the host simulation and the other players continue running.
    public sealed class CampaignSessionMenu : MonoBehaviour
    {
        CoopDirector director; CoopCloud cloud; GameObject overlay,room,friends,options,confirm;
        Text roomInfo,friendInfo,feedback,volume,opacity,warning; Button visibility;
        CanvasGroup controls; float refresh;
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
            Button(panel,"Resume",850,18,240,"VOLTAR · ESC",Close);
            room=Page(panel,"Room");friends=Page(panel,"Invitations");options=Page(panel,"Settings");confirm=Page(panel,"LeaveConfirmation");
            Button(panel,"RoomTab",30,115,330,"SALA E GRUPO",()=>Show(room));
            Button(panel,"FriendsTab",395,115,330,"AMIGOS E CONVITES",()=>{RefreshFriends();Show(friends);});
            Button(panel,"SettingsTab",760,115,330,"CONFIGURAÇÕES",()=>Show(options));
            roomInfo=Label(room.transform,"",30,185,1040,220,22);
            Button(room.transform,"CopyCode",30,415,330,"COPIAR CÓDIGO",()=>Copy(false));
            Button(room.transform,"CopyInvitation",395,415,330,"COPIAR CONVITE",()=>Copy(true));
            visibility=Button(room.transform,"Visibility",760,415,330,"",()=>cloud.ToggleVisibility());
            Button(room.transform,"Inventory",30,485,330,"ABRIR MOCHILA",()=>{Close();GetComponent<CoopInventoryHud>()?.OpenInventory();});
            Button(room.transform,"Leave",760,485,330,"SAIR DA SALA…",()=>{warning.text=director.IsHost?"ENCERRAR A SALA?\nVocê é o host: todos serão desconectados.\nO saque temporário ainda não concluído não é garantido.":"SAIR DA SALA?\nOs demais continuam jogando. Para voltar, use o código\ne mantenha o host e esta tentativa abertos.";Show(confirm);});
            feedback=Label(panel,"",30,575,1040,48,18);
            friendInfo=Label(friends.transform,"",30,180,1040,315,18);
            Button(friends.transform,"CopyFriendInvite",30,500,500,"COPIAR CONVITE PARA COMPARTILHAR",()=>Copy(true));
            Label(options.transform,"VOLUME GERAL",30,180,480,35,22);
            Button(options.transform,"VolumeDown",30,230,210,"− VOLUME",()=>Volume(-.1f));
            Button(options.transform,"VolumeUp",260,230,210,"+ VOLUME",()=>Volume(.1f));
            volume=Label(options.transform,"",500,230,150,48,24);
            Label(options.transform,"VISIBILIDADE DOS CONTROLES",30,305,700,35,22);
            Button(options.transform,"OpacityDown",30,350,210,"MAIS DISCRETO",()=>Opacity(-.1f));
            Button(options.transform,"OpacityUp",260,350,210,"MAIS VISÍVEL",()=>Opacity(.1f));
            opacity=Label(options.transform,"",500,350,150,48,24);
            Label(options.transform,"PC: WASD/setas · mouse segurado: ataque · E: habilidade\nEspaço: esquiva · F: interagir · Esc: menu\nCelular: joystick e botões à direita. Menu no canto superior direito.\nEnvie o APK ao amigo; ele entra em Campanha → Entrar pelo código.",30,425,1040,130,19);
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
            if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)
            {var inventory=GetComponent<CoopInventoryHud>();if(inventory!=null&&inventory.IsOpen)inventory.CloseInventory();else if(IsOpen)Close();else Open();}
            if(!IsOpen)return;
            if(director.Phase==CoopPhase.Offline||director.Phase==CoopPhase.Closed){Close();return;}
            refresh-=Time.unscaledDeltaTime;if(refresh<=0){refresh=.2f;RefreshRoom();}
        }
        void RefreshRoom()
        {
            string text="CÓDIGO DA SALA   "+(string.IsNullOrEmpty(director.Pin)?"Aguardando…":director.Pin)+"\n";
            text+=(cloud.PublicRoom?"Aberta para grupos":"Sala privada")+" · "+director.ConnectedCount+"/4 jogadores\n";
            for(int i=0;i<4;i++)if(director.IsConnected(i)){var a=director.Actor(i);text+="\n"+(i==director.LocalSlot?"Você":"Aliado "+(i+1))+" · "+a.Roster.Current.displayName+" · "+a.Health.Current+"/"+a.Health.Maximum+" vida";}
            roomInfo.text=text;visibility.interactable=director.IsHost&&cloud.CanChangeVisibility&&!cloud.Busy;
            visibility.GetComponentInChildren<Text>().text=director.IsHost?(cloud.PublicRoom?"TORNAR PRIVADA":"ABRIR PARA GRUPOS"):"VISIBILIDADE: HOST";
        }
        void Copy(bool invitation)
        {
            if(string.IsNullOrEmpty(director.Pin)){feedback.text="O código ainda não está disponível.";return;}
            GUIUtility.systemCopyBuffer=invitation?"Vamos jogar Forja das Marés 0.8.2! Campanha → Entrar pelo código: "+director.Pin+". Mantenho a sala aberta para você.":director.Pin;
            feedback.text=invitation?"Convite copiado. Cole na conversa com seu amigo.":"Código copiado: "+director.Pin;
        }
        void RefreshFriends()
        {
            string text="CONVIDE PELO CÓDIGO DA SALA\nCompartilhe o convite na conversa com seu amigo.\nAgenda local deste aparelho (sem presença online):\n";
            try{var saved=JsonUtility.FromJson<CampaignHome.Invitations>(PlayerPrefs.GetString("CampaignInvitations-v1",""));if(saved?.entries!=null)foreach(var entry in saved.entries)if(entry!=null)text+="\n"+entry.name+" · "+entry.code;}
            catch(System.ArgumentException){text+="\nAgenda indisponível.";}
            friendInfo.text=text+"\n\nPara entrar em outra sala, saia da atual e use Amigos no início.\nCódigos deixam de valer quando o host encerra a sala.";
        }
        void Volume(float step){AudioListener.volume=Mathf.Clamp01(AudioListener.volume+step);PlayerPrefs.SetFloat("CampaignMasterVolume",AudioListener.volume);PlayerPrefs.Save();volume.text=Mathf.RoundToInt(AudioListener.volume*100)+"%";}
        void Opacity(float step){controls.alpha=Mathf.Clamp(controls.alpha+step,.35f,1);PlayerPrefs.SetFloat("CampaignControlOpacity",controls.alpha);PlayerPrefs.Save();opacity.text=Mathf.RoundToInt(controls.alpha*100)+"%";}
        void Show(GameObject page){room.SetActive(page==room);friends.SetActive(page==friends);options.SetActive(page==options);confirm.SetActive(page==confirm);feedback.text="";}
        static GameObject Page(Transform parent,string name){var r=RusticHudTheme.Rect(parent,name,Vector2.zero,Vector2.zero,Vector2.zero);r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r.gameObject;}
        static Text Label(Transform p,string text,float x,float y,float w,float h,int size)=>RusticHudTheme.Label(p,"Text",new Vector2(0,1),new Vector2(x,-y),new Vector2(w,h),text,size);
        static Button Button(Transform p,string name,float x,float y,float w,string text,UnityEngine.Events.UnityAction action)=>RusticHudTheme.Button(p,name,new Vector2(0,1),new Vector2(x,-y),new Vector2(w,50),text,action);
        void OnDisable(){if(IsOpen)Close();}
    }
}
