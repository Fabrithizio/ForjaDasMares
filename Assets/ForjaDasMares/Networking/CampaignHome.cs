using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    // Local address book of invitations. No invented online presence or remote friend accounts.
    public sealed class CampaignHome : MonoBehaviour
    {
        [Serializable] public sealed class Invitation { public string name,code; }
        [Serializable] public sealed class Invitations { public List<Invitation> entries=new List<Invitation>(); }
        internal static bool EnterExpeditionRoutes; const string InviteKey="CampaignInvitations-v1",VolumeKey="CampaignMasterVolume";
        Invitations saved=new Invitations();CoopDirector director;CoopCloud cloud;Action play;
        GameObject main,friends,options,confirm,creation,account;Text notice;InputField alias,code;Button[] entries=new Button[6];
        public void Build(RectTransform parent,CoopDirector owner,Action campaign,Action profile)
        {
            director=owner;cloud=owner.GetComponent<CoopCloud>();play=campaign;
            try{saved=JsonUtility.FromJson<Invitations>(PlayerPrefs.GetString(InviteKey,""))??new Invitations();}catch(ArgumentException){saved=new Invitations();}
            saved.entries??=new List<Invitation>();saved.entries.RemoveAll(x=>x==null||string.IsNullOrWhiteSpace(x.name)||string.IsNullOrWhiteSpace(x.code)||x.name.Length>24||x.code.Length>12);if(saved.entries.Count>6)saved.entries.RemoveRange(6,saved.entries.Count-6);
            AudioListener.volume=Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey,.8f));
            MenuArtwork.Fill(parent,"HomeKeyArt","MenuArt/RoutesBackdrop",Color.white);
            main=Page(parent,"Home");friends=Page(parent,"Friends");options=Page(parent,"Options");confirm=Page(parent,"ConfirmExit");
            Label(main,"FORJA\nDAS MARÉS",64,62,640,155,62);
            Label(main,"O MUNDO ALÉM DO REFÚGIO",68,220,550,38,21).color=RusticHudTheme.Gold;
            Label(main,"Comece sua jornada no Refúgio.",68,264,550,40,19);
            Button(main,"Profile",65,403,470,"PERSONAGEM E EQUIPAMENTOS",()=>profile());
            Button(main,"ActOne",65,330,470,string.IsNullOrEmpty(CharacterName)?"JOGAR RPG":"CONTINUAR RPG",()=>{if(string.IsNullOrEmpty(LocalAccountId))Show(account);else if(string.IsNullOrEmpty(CharacterName))Show(creation);else EnterRpg();});
            BuildCharacterCreation(parent);
            BuildAccountCreation(parent);
            Button(main,"NewLocalAccount",65,620,470,"CRIAR OUTRO PERFIL LOCAL",()=>Show(account));
            Button(main,"Friends",65,476,470,"AMIGOS E CONVITES",()=>Show(friends));
            Button(main,"Settings",65,549,225,"CONFIGURAÇÕES",()=>Show(options));
            Button(main,"Quit",310,549,225,"SAIR",()=>Show(confirm));

            var badge=RusticHudTheme.Rect(main.transform,"VersionCard",new Vector2(1,0),new Vector2(-40,40),new Vector2(310,65));RusticHudTheme.Panel(badge,new Color(.025f,.05f,.055f,.85f),16);
            RusticHudTheme.Label(badge,"Version",Vector2.one*.5f,Vector2.zero,new Vector2(280,60),"Versão "+Application.version,17,TextAnchor.MiddleCenter);
            Label(friends,"AMIGOS E CONVITES",40,30,1100,65,32);
            Label(friends,"Agenda neste aparelho: guarde o nome e o código que seu amigo compartilhar.\nCódigos expiram quando o host encerra a sala. Sem presença online ou contas de amigos ainda.",40,100,1180,70,18);
            alias=Field(friends,"FriendName",40,195,330,"Nome do amigo",24);code=Field(friends,"FriendCode",390,195,240,"Código da sala",12);
            Button(friends,"SaveInvite",650,195,240,"SALVAR CONVITE",Save);
            Button(friends,"ClearInvites",910,195,320,"LIMPAR AGENDA",()=>{saved.entries.Clear();Persist();Refresh();});
            for(int i=0;i<entries.Length;i++){int slot=i;entries[i]=Button(friends,"SavedInvite_"+i,40,280+i*49,1190,"",()=>{if(slot>=saved.entries.Count||cloud.Busy)return;string invitation=saved.entries[slot].code;play();cloud.Join(invitation);});((RectTransform)entries[i].transform).sizeDelta=new Vector2(1190,43);}
            notice=Label(friends,"",40,585,1180,40,18);Button(friends,"Back",40,640,350,"VOLTAR AO INÍCIO",()=>Show(main));Refresh();
            Label(options,"CONFIGURAÇÕES",50,40,1100,70,34);
            Label(options,"Volume geral",50,165,500,60,24);
            Button(options,"VolumeDown",50,235,240,"DIMINUIR",()=>Volume(-.1f));Button(options,"VolumeUp",320,235,240,"AUMENTAR",()=>Volume(.1f));
            volume=Label(options,"",600,235,400,60,24);Volume(0);
            var graphics=Button(options,"GraphicsQuality",50,310,510,"GRÁFICOS: "+PresentationQuality.ModeName(PresentationQuality.Selected),()=>{var quality=director.GetComponent<PresentationQuality>();if(quality==null)quality=UnityEngine.Object.FindFirstObjectByType<PresentationQuality>();quality?.Cycle();options.transform.Find("GraphicsQuality").GetComponentInChildren<Text>().text="GRÁFICOS: "+PresentationQuality.ModeName(PresentationQuality.Selected);});
            Label(options,"Nítido: bordas mais suaves. Econômico: resolução menor para aparelhos lentos.",50,380,1150,45,18);
            SoundVolumeButton(options,"MusicVolume",50,430,510,"MÚSICA",CampaignSoundscape.MusicKey);
            SoundVolumeButton(options,"AmbienceVolume",590,430,510,"AMBIENTE",CampaignSoundscape.AmbienceKey);
            Label(options,"Mouse RPG: chão para andar; segure no inimigo para atacar; Shift mantém parado.\nClique no chão para andar; NPCs para conversar. Sem WASD.\nBotão direito/E: habilidade · espaço: esquiva · roda: zoom · Q: super.\nCelular mantém joystick e botões.",50,500,1150,105,18);
            Button(options,"Back",50,620,500,"VOLTAR AO INÍCIO",()=>Show(main));
            Label(confirm,"ENCERRAR O JOGO?",150,220,1000,100,36);
            Button(confirm,"Cancel",150,400,430,"CONTINUAR AQUI",()=>Show(main));Button(confirm,"Confirm",690,400,430,"ENCERRAR",Application.Quit);
            Show(main); if(EnterExpeditionRoutes){EnterExpeditionRoutes=false;play();}
        }
        Text volume;
        void RefreshAudioLabels()
        {
            var music=options.transform.Find("MusicVolume");var ambient=options.transform.Find("AmbienceVolume");
            var mode=options.transform.Find("MovementMode");if(mode!=null)mode.GetComponentInChildren<Text>().text="CONTROLES: "+DesktopRpgControls.ModeName;
            if(music!=null)music.GetComponentInChildren<Text>().text="MÚSICA: "+Mathf.RoundToInt(CampaignSoundscape.Volume(CampaignSoundscape.MusicKey)*100)+"% · AJUSTAR";
            if(ambient!=null)ambient.GetComponentInChildren<Text>().text="AMBIENTE: "+Mathf.RoundToInt(CampaignSoundscape.Volume(CampaignSoundscape.AmbienceKey)*100)+"% · AJUSTAR";
        }
        static void SoundVolumeButton(GameObject page,string name,float x,float y,float width,string label,string key)
        {
            Button button=null;button=Button(page,name,x,y,width,label+": "+Mathf.RoundToInt(CampaignSoundscape.Volume(key)*100)+"% · AJUSTAR",()=>{float value=CampaignSoundscape.CycleVolume(key);button.GetComponentInChildren<Text>().text=label+": "+Mathf.RoundToInt(value*100)+"% · AJUSTAR";});
        }
        void Volume(float step){AudioListener.volume=Mathf.Clamp01(AudioListener.volume+step);PlayerPrefs.SetFloat(VolumeKey,AudioListener.volume);PlayerPrefs.Save();if(volume!=null)volume.text=Mathf.RoundToInt(AudioListener.volume*100)+"%";}
        void Show(GameObject page){main.SetActive(page==main);friends.SetActive(page==friends);options.SetActive(page==options);confirm.SetActive(page==confirm);if(account!=null)account.SetActive(page==account);if(creation!=null){creation.SetActive(page==creation);if(page==creation){var roster=director.Actor(0)?.Roster;if(roster!=null)creation.GetComponentInChildren<EquipmentPaperDoll>().Refresh(roster.GetComponent<HeroPresentation>(),new PlayerProfile(),roster.Current.heroId);}}if(page==options){volume.text=Mathf.RoundToInt(AudioListener.volume*100)+"%";RefreshAudioLabels();}}
        public static string LocalAccountId=>PlayerPrefs.GetString("Forja.LocalAccount.Id","");
        public static string LocalAccountFolder=>Guid.TryParseExact(LocalAccountId,"N",out _)
            ?System.IO.Path.Combine(Application.persistentDataPath,"RpgAccounts",LocalAccountId):null;
        void BuildAccountCreation(Transform parent)
        {
            account=Page(parent,"CreateLocalAccount");
            Label(account,"CRIAR PERFIL LOCAL",110,105,950,65,38).color=RusticHudTheme.Gold;
            Label(account,"Sua aventura começa com um perfil salvo neste aparelho.",110,185,950,48,23);
            var login=Field(account,"AccountName",110,260,620,"Nome do perfil",24);
            var error=Label(account,"",110,340,950,52,20);
            Button(account,"BackFromAccount",110,470,220,"VOLTAR",()=>Show(main));
            Button(account,"ConfirmAccount",360,470,390,"CRIAR PERFIL",()=>
            {
                string name=login.text.Trim();
                if(name.Length<2){error.text="Digite um nome com pelo menos dois caracteres.";return;}
                string id=Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString("Forja.LocalAccount."+id+".Name",name);
                PlayerPrefs.SetString("Forja.LocalAccount.Id",id);
                PlayerPrefs.Save();
                Show(creation);
            });
            account.SetActive(false);
        }
        static string CharacterKey
        {
            get
            {
                if(LocalAccountFolder!=null)return "Forja.RpgCharacter."+LocalAccountId;
                var args=Environment.GetCommandLineArgs();
                int i=Array.IndexOf(args,"-netIdentity");
                return i>=0&&i+1<args.Length&&Guid.TryParse(args[i+1],out var id)
                    ?"Forja.RpgCharacter."+id.ToString("N"):"Forja.RpgCharacter";
            }
        }
        public static string CharacterName=>PlayerPrefs.GetString(CharacterKey+".Name","");
        public static int CharacterClass=>PlayerPrefs.GetInt(CharacterKey+".Class",2);
        void EnterRpg()
        {
            director.Close();
            var oldHud=GetComponentInParent<CoopHud>();
            if(oldHud!=null)oldHud.gameObject.SetActive(false);
            if(director.Manager!=null)Destroy(director.Manager.gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("RefugioUltimoSino");
        }
        void BuildCharacterCreation(Transform parent)
        {
            creation=Page(parent,"CreateCharacter");
            Label(creation,"CRIAR PERSONAGEM",65,55,690,64,38).color=RusticHudTheme.Gold;
            Label(creation,"Escolha um nome e como quer jogar.",65,125,680,45,20);
            var nameInput=Field(creation,"CharacterName",65,180,620,"Nome do personagem",24);
            var selection=Label(creation,"",65,468,650,53,19);
            var error=Label(creation,"",65,526,660,44,18);
            var frame=RusticHudTheme.Rect(creation.transform,"CharacterPreview",new Vector2(0,1),new Vector2(785,-90),new Vector2(360,480));
            var image=frame.gameObject.AddComponent<RawImage>();
            var preview=frame.gameObject.AddComponent<EquipmentPaperDoll>();
            preview.Configure(image);
            var empty=new PlayerProfile();
            string[] names={"Guerreiro","Patrulheiro","Arcanista","Cavaleiro Rúnico"};
            string[] roles={"Combate próximo com golpes de impacto.","Mobilidade e ataques à distância.","Magia de alcance e controle.","Resistência e proteção na linha de frente."};
            int chosen=Mathf.Clamp(CharacterClass,0,names.Length-1);
            void Choose(int index)
            {
                chosen=index;
                director.SelectHero(index);
                selection.text=names[index]+"\n"+roles[index];
                var roster=director.Actor(0)?.Roster;
                if(roster!=null)preview.Refresh(roster.GetComponent<HeroPresentation>(),empty,roster.Current.heroId);
            }
            var cards=Resources.Load<Texture2D>("MenuArt/ClassCards");
            float[] left={.045f,.107f,.171f,.234f};
            for(int i=0;i<names.Length;i++)
            {
                int index=i;
                float x=65+i*165;
                var card=RusticHudTheme.Rect(creation.transform,"ClassPortrait"+i,new Vector2(0,1),new Vector2(x,-253),new Vector2(150,138));
                var portrait=card.gameObject.AddComponent<RawImage>();
                portrait.texture=cards;
                portrait.uvRect=new Rect(left[i],.686f,.057f,.147f);
                portrait.raycastTarget=false;
                Button(creation,"Class"+i,x,394,150,names[i],()=>Choose(index)).GetComponentInChildren<Text>().fontSize=16;
            }
            Button(creation,"BackFromCreation",65,595,225,"VOLTAR",()=>Show(main));
            Button(creation,"ConfirmCharacter",750,595,420,"CRIAR E ENTRAR",()=>
            {
                string name=nameInput.text.Trim();
                if(name.Length<2){error.text="Digite um nome com pelo menos duas letras ou caracteres.";return;}
                foreach(char c in name)
                    if(!char.IsLetterOrDigit(c)&&c!=' '&&c!='-'&&c!='\''){error.text="Use letras, números, espaços, hífen ou apóstrofo.";return;}
                PlayerPrefs.SetInt(CharacterKey+".Class",chosen);
                PlayerPrefs.SetString(CharacterKey+".Name",name);
                PlayerPrefs.Save();
                EnterRpg();
            });
            Choose(chosen);
            creation.SetActive(false);
        }
        void Save()
        {
            string name=alias.text.Trim(),invite=code.text.Trim().ToUpperInvariant();
            if(name.Length==0||invite.Length<4){notice.text="Informe o nome e um código válido compartilhado pelo host.";return;}
            foreach(char c in invite)if(!char.IsLetterOrDigit(c)){notice.text="Código deve conter somente letras e números.";return;}
            saved.entries.RemoveAll(x=>x.name==name||x.code==invite);saved.entries.Insert(0,new Invitation{name=name,code=invite});
            if(saved.entries.Count>6)saved.entries.RemoveAt(6);Persist();Refresh();notice.text="Convite salvo. Toque nele para entrar.";
        }
        void Persist(){PlayerPrefs.SetString(InviteKey,JsonUtility.ToJson(saved));PlayerPrefs.Save();}
        void Refresh(){for(int i=0;i<entries.Length;i++){entries[i].gameObject.SetActive(i<saved.entries.Count);if(i<saved.entries.Count)entries[i].GetComponentInChildren<Text>().text=saved.entries[i].name+" · "+saved.entries[i].code+"    ENTRAR NA SALA";}}
        static GameObject Page(Transform parent,string name)
        {var r=RusticHudTheme.Rect(parent,name,Vector2.one*.5f,Vector2.zero,new Vector2(1280,720));if(name!="Home")RusticHudTheme.Panel(r,new Color(.025f,.05f,.045f,.96f),25);return r.gameObject;}
        static Text Label(GameObject p,string s,float x,float y,float w,float h,int font)=>RusticHudTheme.Label(p.transform,"Text",new Vector2(0,1),new Vector2(x,-y),new Vector2(w,h),s,font);
        static Button Button(GameObject p,string name,float x,float y,float width,string text,UnityEngine.Events.UnityAction action)=>RusticHudTheme.Button(p.transform,name,new Vector2(0,1),new Vector2(x,-y),new Vector2(width,60),text,action);
        static InputField Field(GameObject p,string name,float x,float y,float width,string placeholder,int limit)
        {var r=RusticHudTheme.Rect(p.transform,name,new Vector2(0,1),new Vector2(x,-y),new Vector2(width,60));r.gameObject.AddComponent<Image>().color=RusticHudTheme.Surface;var f=r.gameObject.AddComponent<InputField>();f.textComponent=Label(r.gameObject,"",12,0,width-24,60,22);var hint=Label(r.gameObject,placeholder,12,0,width-24,60,18);hint.color=RusticHudTheme.Muted;f.placeholder=hint;f.characterLimit=limit;return f;}
    }
}
