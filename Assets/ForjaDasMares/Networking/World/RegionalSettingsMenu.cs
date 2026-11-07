using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
namespace ForjaDasMares.Networking
{
    // Local settings overlay. Never changes timeScale or freezes other players.
    public sealed class RegionalSettingsMenu:MonoBehaviour
    {
        RegionalWorld world;GameObject overlay;InputAction escape;bool requested,closeInventory;Text[] values=new Text[4];Text quality;Button qualityButton;
        readonly string[] keys={"CampaignMasterVolume",CampaignSoundscape.MusicKey,CampaignSoundscape.AmbienceKey,"CampaignEffectsVolume"};
        readonly string[] names={"GERAL","MÚSICA","AMBIENTE","EFEITOS"};
        public bool IsOpen=>overlay!=null&&overlay.activeSelf;
        public void Build(RectTransform root,RegionalWorld owner)
        {
            world=owner;
            RusticHudTheme.Button(root,"RegionalSettingsButton",new Vector2(1,1),new Vector2(-240,-20),new Vector2(170,42),"CONFIGURAÇÕES",Open);
            var full=RusticHudTheme.Rect(root,"RegionalSettings",Vector2.zero,Vector2.zero,Vector2.zero);full.anchorMax=Vector2.one;full.offsetMin=full.offsetMax=Vector2.zero;
            full.gameObject.AddComponent<Image>().color=new Color(.01f,.02f,.035f,.92f);overlay=full.gameObject;
            var panel=RusticHudTheme.Rect(full,"SettingsPanel",Vector2.one*.5f,Vector2.zero,new Vector2(940,600));RusticHudTheme.Panel(panel,RusticHudTheme.Ink,26);
            Text Label(string id,string text,float x,float y,float width,int font=20)=>RusticHudTheme.Label(panel,id,new Vector2(0,1),new Vector2(x,-y),new Vector2(width,38),text,font);
            Button Button(string id,string text,float x,float y,float width,UnityEngine.Events.UnityAction action)=>RusticHudTheme.Button(panel,id,new Vector2(0,1),new Vector2(x,-y),new Vector2(width,44),text,action);
            Label("Title","CONFIGURAÇÕES",28,20,650,28).color=RusticHudTheme.Gold;
            Button("CloseRegionalSettings","VOLTAR · ESC",710,20,200,Close);
            Label("Scope","Preferências deste aparelho · o mundo continua em movimento.",28,76,880,17);
            for(int i=0;i<4;i++)
            {
                int index=i;float y=136+i*70;Label("AudioLabel"+i,names[i],32,y,240);values[i]=Label("AudioValue"+i,"",520,y,120,23);
                Button("AudioDown"+i,"−",350,y,100,()=>ChangeVolume(index,-.1f));Button("AudioUp"+i,"+",690,y,100,()=>ChangeVolume(index,.1f));
            }
            qualityButton=Button("RegionalGraphics","",30,432,565,()=>{var q=world.GetComponentInChildren<PresentationQuality>();if(q==null&&Camera.main!=null)q=Camera.main.GetComponent<PresentationQuality>();q?.Cycle();Refresh();});
            quality=qualityButton.GetComponentInChildren<Text>();
            Button("RegionalFps","MOSTRAR / OCULTAR FPS · F8",615,432,295,()=>{GetComponent<CampaignPerformanceHud>()?.Toggle();Close();});
            Label("Controls","Chão: caminhar · alvo: atacar parado · Shift: atacar sem alvo\nI: inventário · E: habilidade · Espaço: esquiva · F: interagir",30,500,880,17).rectTransform.sizeDelta=new Vector2(880,70);
            escape=new InputAction("RegionalSettingsEscape",InputActionType.Button,"<Keyboard>/escape");escape.performed+=_=>{var hud=GetComponent<RegionalWorldHud>();if(hud!=null&&hud.JournalOpen){hud.CloseJournal();return;}closeInventory=GetComponent<CoopInventoryHud>()?.IsOpen==true;requested=true;};escape.Enable();overlay.SetActive(false);Refresh();
        }
        void ChangeVolume(int i,float step)
        {
            float value=Mathf.Clamp01(PlayerPrefs.GetFloat(keys[i],i==0?.8f:i==1?.7f:i==2?.65f:.8f)+step);PlayerPrefs.SetFloat(keys[i],value);PlayerPrefs.Save();if(i==0)AudioListener.volume=value;Refresh();
        }
        void Refresh(){for(int i=0;i<4;i++)values[i].text=Mathf.RoundToInt(PlayerPrefs.GetFloat(keys[i],i==0?.8f:i==1?.7f:i==2?.65f:.8f)*100)+"%";quality.text="GRÁFICOS · "+PresentationQuality.ModeName(PresentationQuality.Selected);}
        public void Open(){if(overlay==null)return;GetComponent<RegionalWorldHud>()?.CloseJournal();GetComponent<CoopInventoryHud>()?.CloseInventory();overlay.SetActive(true);overlay.transform.SetAsLastSibling();SetBlocked(true);Refresh();}
        public void Close(){if(overlay==null)return;overlay.SetActive(false);SetBlocked(false);}
        void SetBlocked(bool blocked){if(world==null||world.Hero==null)return;world.Hero.GetComponent<ArenaInput>().BlockCommands(blocked);world.Hero.GetComponent<PlayerMotor>().StopWalking();}
        void Update(){if(!requested)return;requested=false;if(closeInventory){closeInventory=false;GetComponent<CoopInventoryHud>()?.CloseInventory();return;}if(IsOpen)Close();else if(GetComponent<CoopInventoryHud>()?.IsOpen!=true)Open();}
        void OnDestroy(){escape?.Dispose();if(IsOpen)SetBlocked(false);}
    }
}
