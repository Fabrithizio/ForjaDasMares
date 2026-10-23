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
        const string InviteKey="CampaignInvitations-v1",VolumeKey="CampaignMasterVolume";
        Invitations saved=new Invitations();CoopDirector director;CoopCloud cloud;Action play;
        GameObject main,friends,options,confirm;Text notice;InputField alias,code;Button[] entries=new Button[6];
        public void Build(RectTransform parent,CoopDirector owner,Action campaign,Action profile)
        {
            director=owner;cloud=owner.GetComponent<CoopCloud>();play=campaign;
            try{saved=JsonUtility.FromJson<Invitations>(PlayerPrefs.GetString(InviteKey,""))??new Invitations();}catch(ArgumentException){saved=new Invitations();}
            saved.entries??=new List<Invitation>();saved.entries.RemoveAll(x=>x==null||string.IsNullOrWhiteSpace(x.name)||string.IsNullOrWhiteSpace(x.code)||x.name.Length>24||x.code.Length>12);if(saved.entries.Count>6)saved.entries.RemoveRange(6,saved.entries.Count-6);
            AudioListener.volume=Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey,.8f));
            MenuArtwork.Fill(parent,"HomeKeyArt","MenuArt/RoutesBackdrop",Color.white);
            main=Page(parent,"Home");friends=Page(parent,"Friends");options=Page(parent,"Options");confirm=Page(parent,"ConfirmExit");
            Label(main,"FORJA\nDAS MARÉS",64,62,640,155,62);
            Label(main,"AS ROTAS DA MARÉ",68,220,550,38,21).color=RusticHudTheme.Gold;
            Label(main,"Sua próxima expedição começa aqui.",68,264,550,40,19);
            var primary=Button(main,"Campaign",65,330,470,"JOGAR · CAMPANHA",()=>play());primary.GetComponent<RoundedHudGraphic>().borderColor=RusticHudTheme.Gold;
            Button(main,"Profile",65,403,470,"PERSONAGEM E EQUIPAMENTOS",()=>profile());
            Button(main,"Friends",65,476,470,"AMIGOS E CONVITES",()=>Show(friends));
            Button(main,"Settings",65,549,225,"CONFIGURAÇÕES",()=>Show(options));
            Button(main,"Quit",310,549,225,"SAIR",()=>Show(confirm));
            Label(main,"1–4 jogadores · PC e Android\nEscolha a fase e entre solo ou com seu grupo.",68,625,630,60,17);
            var badge=RusticHudTheme.Rect(main.transform,"VersionCard",new Vector2(1,0),new Vector2(-40,40),new Vector2(310,65));RusticHudTheme.Panel(badge,new Color(.025f,.05f,.055f,.85f),16);
            RusticHudTheme.Label(badge,"Version",Vector2.one*.5f,Vector2.zero,new Vector2(280,60),"EXPEDIÇÕES COOPERATIVAS\nVersão de testes "+Application.version,17,TextAnchor.MiddleCenter);
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
            Label(options,"PC: WASD/setas · mouse segurado para atacar · E habilidade · Espaço esquiva · F interagir\nCelular: joystick à esquerda; ataque, esquiva, habilidade e interação à direita.\nO host mantém os saves da campanha. Não há migração de host.",50,440,1150,150,20);
            Button(options,"Back",50,620,500,"VOLTAR AO INÍCIO",()=>Show(main));
            Label(confirm,"ENCERRAR O JOGO?",150,220,1000,100,36);
            Button(confirm,"Cancel",150,400,430,"CONTINUAR AQUI",()=>Show(main));Button(confirm,"Confirm",690,400,430,"ENCERRAR",Application.Quit);
            Show(main);
        }
        Text volume;
        void Volume(float step){AudioListener.volume=Mathf.Clamp01(AudioListener.volume+step);PlayerPrefs.SetFloat(VolumeKey,AudioListener.volume);PlayerPrefs.Save();if(volume!=null)volume.text=Mathf.RoundToInt(AudioListener.volume*100)+"%";}
        void Show(GameObject page){main.SetActive(page==main);friends.SetActive(page==friends);options.SetActive(page==options);confirm.SetActive(page==confirm);}
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
