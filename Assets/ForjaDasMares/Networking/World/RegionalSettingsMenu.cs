using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
namespace ForjaDasMares.Networking
{
    // Local settings overlay. Never changes timeScale or freezes other players.
    public sealed class RegionalSettingsMenu:MonoBehaviour
    {
        RegionalWorld world;GameObject overlay;InputAction escape, fullscreenKey;bool requested,closeInventory;Text[] values=new Text[4];Text quality;Button qualityButton;
        readonly string[] keys={"CampaignMasterVolume",CampaignSoundscape.MusicKey,CampaignSoundscape.AmbienceKey,"CampaignEffectsVolume"};
        readonly string[] names={"GERAL","MÚSICA","AMBIENTE","EFEITOS"};
        public bool IsOpen=>overlay!=null&&overlay.activeSelf;
        public void Build(RectTransform root,RegionalWorld owner)
        {
            world=owner;
            RusticHudTheme.IconButton(root,"RegionalSettingsButton",Vector2.one,new Vector2(-20,-20),HudSymbol.Settings,Open);
            var full=RusticHudTheme.Rect(root,"RegionalSettings",Vector2.zero,Vector2.zero,Vector2.zero);full.anchorMax=Vector2.one;full.offsetMin=full.offsetMax=Vector2.zero;
            full.gameObject.AddComponent<Image>().color=new Color(.01f,.02f,.035f,.92f);overlay=full.gameObject;
            var panel=RusticHudTheme.Rect(full,"SettingsPanel",Vector2.one*.5f,Vector2.zero,new Vector2(940,670));RusticHudTheme.Panel(panel,RusticHudTheme.Ink,26);
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
            Button("RegionalFps","MOSTRAR / OCULTAR FPS",615,432,295,()=>{GetComponent<CampaignPerformanceHud>()?.Toggle();Close();});
            Button("ReturnToMenu","VOLTAR AO MENU",675,505,235,world.ReturnToStartMenu);
            Button("Layout","EDITAR LAYOUT DO HUD",UnityEngine.Device.Application.isMobilePlatform?30:490,585,UnityEngine.Device.Application.isMobilePlatform?620:420,()=>{Close();LayoutDoHud.Atual?.Editar();});
            if(!UnityEngine.Device.Application.isMobilePlatform){Button("Teclas","TECLAS · ESCOLHER AS TECLAS DE CADA AÇÃO",30,505,620,AbrirTeclas);BuildTeclas(full);}
            if (!UnityEngine.Device.Application.isMobilePlatform)
            {
                Button("Fullscreen","TELA CHEIA / JANELA",30,585,440,ToggleFullscreen);
                fullscreenKey = Teclas.Criar("tela");
                fullscreenKey.performed += _ => ToggleFullscreen();
                fullscreenKey.Enable();
            }
            escape=new InputAction("RegionalSettingsEscape",InputActionType.Button,"<Keyboard>/escape");escape.performed+=_=>{if(LayoutDoHud.Atual!=null&&LayoutDoHud.Atual.Editando)return;if(Teclas.Escutando||Teclas.ParouNoQuadro>=Time.frameCount-1)return;if(teclas!=null&&teclas.activeSelf){teclas.SetActive(false);return;}var hud=GetComponent<RegionalWorldHud>();if(hud?.IntroOpen==true)return;if(hud!=null&&hud.CloseStoryModal())return;if(hud!=null&&hud.ShopOpen){hud.CloseShop();return;}if(hud!=null&&hud.JournalOpen){hud.CloseJournal();return;}closeInventory=GetComponent<CoopInventoryHud>()?.IsOpen==true;requested=true;};escape.Enable();overlay.SetActive(false);Refresh();
        }
        // ---------- Keys (PC): one button per action; click it and press the new key ----------
        GameObject teclas;Text[] teclaTexto;Text teclaAviso;
        void BuildTeclas(RectTransform full)
        {
            var panel=RusticHudTheme.Rect(full,"TeclasPanel",Vector2.one*.5f,Vector2.zero,new Vector2(940,670));RusticHudTheme.Panel(panel,RusticHudTheme.Ink,26);teclas=panel.gameObject;
            RusticHudTheme.Label(panel,"Titulo",new Vector2(0,1),new Vector2(28,-20),new Vector2(600,38),"TECLAS",28).color=RusticHudTheme.Gold;
            RusticHudTheme.Button(panel,"Fechar",new Vector2(0,1),new Vector2(710,-20),new Vector2(200,44),"VOLTAR · ESC",()=>teclas.SetActive(false));
            teclaAviso=RusticHudTheme.Label(panel,"Aviso",new Vector2(0,1),new Vector2(28,-70),new Vector2(880,30),"Clique na tecla de uma ação e aperte a nova. Se outra ação usava essa tecla, as duas trocam.",16);
            teclaAviso.color=RusticHudTheme.Muted;
            int n=Teclas.Todas.Length,metade=(n+1)/2;teclaTexto=new Text[n];
            for(int i=0;i<n;i++)
            {
                int index=i;var acao=Teclas.Todas[i];float x=i<metade?30:490,y=112+(i%metade)*58;
                RusticHudTheme.Label(panel,"Nome"+i,new Vector2(0,1),new Vector2(x,-y),new Vector2(250,44),acao.nome,17,TextAnchor.MiddleLeft);
                var b=RusticHudTheme.Button(panel,"Tecla"+i,new Vector2(0,1),new Vector2(x+260,-y),new Vector2(160,44),"",()=>Escolher(index));
                teclaTexto[i]=b.GetComponentInChildren<Text>();
            }
            RusticHudTheme.Button(panel,"Padrao",new Vector2(0,1),new Vector2(30,-600),new Vector2(300,44),"VOLTAR AO PADRÃO",()=>{Teclas.Padrao();teclaAviso.text="Teclas de volta ao padrão.";AtualizarTeclas();});
            teclas.SetActive(false);
        }
        void AbrirTeclas(){if(teclas==null)return;teclas.SetActive(true);teclas.transform.SetAsLastSibling();AtualizarTeclas();}
        void AtualizarTeclas(){for(int i=0;i<teclaTexto.Length;i++)teclaTexto[i].text=Teclas.Nome(Teclas.Todas[i].id);}
        void Escolher(int i)
        {
            if(Teclas.Escutando)return;
            var acao=Teclas.Todas[i];teclaTexto[i].text="...";teclaAviso.text="Aperte a nova tecla para "+acao.nome.ToUpperInvariant()+" · Esc cancela";
            Teclas.Escutar(path=>
            {
                if(path==null){teclaAviso.text="Nada mudou.";AtualizarTeclas();return;}
                var outra=Teclas.QuemUsa(path,acao.id);var antiga=Teclas.Nome(acao.id);
                Teclas.Mudar(acao.id,path);
                teclaAviso.text=acao.nome+": "+Teclas.Nome(acao.id)+(outra!=null?"   ·   "+Teclas.NomeDaAcao(outra)+" passou para "+antiga:"");
                AtualizarTeclas();
            });
        }
        void ChangeVolume(int i,float step)
        {
            float value=Mathf.Clamp01(PlayerPrefs.GetFloat(keys[i],i==0?.8f:i==1?.7f:i==2?.65f:.8f)+step);PlayerPrefs.SetFloat(keys[i],value);PlayerPrefs.Save();if(i==0)AudioListener.volume=value;Refresh();
        }
        void Refresh(){for(int i=0;i<4;i++)values[i].text=Mathf.RoundToInt(PlayerPrefs.GetFloat(keys[i],i==0?.8f:i==1?.7f:i==2?.65f:.8f)*100)+"%";quality.text="GRÁFICOS · "+PresentationQuality.ModeName(PresentationQuality.Selected);}
        static void ToggleFullscreen()
        {
            if (Screen.fullScreen)
                Screen.SetResolution(Mathf.Min(1280,Display.main.systemWidth),Mathf.Min(720,Display.main.systemHeight),FullScreenMode.Windowed);
            else
                Screen.SetResolution(Display.main.systemWidth,Display.main.systemHeight,FullScreenMode.FullScreenWindow);
        }
        public void Open(){if(overlay==null)return;GetComponent<RegionalWorldHud>()?.CloseJournal();GetComponent<CoopInventoryHud>()?.CloseInventory();overlay.SetActive(true);overlay.transform.SetAsLastSibling();SetBlocked(true);Refresh();}
        public void Close(){if(overlay==null)return;if(teclas!=null)teclas.SetActive(false);overlay.SetActive(false);SetBlocked(false);}
        void SetBlocked(bool blocked){if(world==null||world.Hero==null)return;world.Hero.GetComponent<ArenaInput>().BlockCommands(blocked);world.Hero.GetComponent<PlayerMotor>().StopWalking();}
        void Update(){if(!requested)return;requested=false;if(closeInventory){closeInventory=false;GetComponent<CoopInventoryHud>()?.CloseInventory();return;}if(IsOpen)Close();else if(GetComponent<CoopInventoryHud>()?.IsOpen!=true)Open();}
        void OnDestroy(){fullscreenKey?.Dispose();escape?.Dispose();if(IsOpen)SetBlocked(false);}
    }
}
