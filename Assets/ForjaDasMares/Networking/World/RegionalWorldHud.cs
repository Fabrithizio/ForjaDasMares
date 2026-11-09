using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace ForjaDasMares.Networking
{
    [DefaultExecutionOrder(260)]public sealed class RegionalWorldHud:MonoBehaviour
    {
        [SerializeField]RegionalWorld world;
        Text life,objective,hint,controlHint,storyNotice,healthCaption,superCaption;RectTransform healthFill,superFill;EquipmentPaperDoll portrait;PlayerProfile previewProfile=new PlayerProfile();TownHub town;Button movementMode;GameObject death;float poll;InputAction interact, weaponModeKey;
        readonly InputAction[] skillKeys = new InputAction[3];
        readonly Button[] skillButtons = new Button[3];
        readonly Text[] skillLabels = new Text[3];
        public void Configure(RegionalWorld value){world=value;}
        static readonly Vector2 Top=new Vector2(0,1);
        Text Label(Transform parent,string name,Vector2 position,Vector2 size,string text,int font=18)=>RusticHudTheme.Label(parent,name,Top,position,size,text,font);
        void Start()
        {
            var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;gameObject.AddComponent<GraphicRaycaster>();
            var scale=gameObject.AddComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1280,720);scale.matchWidthOrHeight=.5f;
            var root=RusticHudTheme.Rect(transform,"RegionSafeArea",Vector2.zero,Vector2.zero,Vector2.zero);root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
            town=world.GetComponent<TownHub>();BuildVitals(root);if(town!=null)BuildShop(root);if(world.Journey!=null)BuildJournal(root);
            BuildStoryPanels(root);
            BuildDamageNumbers(root);
            movementMode=RusticHudTheme.Button(root,"MovementHelp",Top,new Vector2(22,-118),new Vector2(300,36),"CONTROLES · AJUDA",()=>{controlHint.gameObject.SetActive(!controlHint.gameObject.activeSelf);});
            controlHint=Label(root,"ControlHint",new Vector2(22,-158),new Vector2(430,56),"",14);
            var quest=RusticHudTheme.Rect(root,"Quest",new Vector2(1,1),new Vector2(-20,-82),new Vector2(350,135));RusticHudTheme.Panel(quest,new Color(.025f,.05f,.045f,.92f),16);objective=Label(quest,"Objective",new Vector2(14,-12),new Vector2(322,115),"",16);
            var map=RusticHudTheme.Rect(root,"LocalMap",new Vector2(1,1),new Vector2(-20,world.Journey!=null?-80:-220),new Vector2(175,175));RusticHudTheme.Panel(map,new Color(.025f,.05f,.045f,.9f),16);var mapContent=RusticHudTheme.Rect(map,"Markers",Vector2.zero,new Vector2(8,8),new Vector2(159,159));mapContent.gameObject.AddComponent<RegionalMap>().Configure(world);
            mapPanel=map.gameObject;mapLegend=Label(root,"MapLegend",new Vector2(1060,world.Journey!=null?-258:-403),new Vector2(200,42),"Branco: você · verde: refúgio\nDourado: missão · azul: passagem",11).gameObject;
            hint=Label(root,"Interact",new Vector2(350,-625),new Vector2(580,35),"",18);hint.alignment=TextAnchor.MiddleCenter;hint.color=RusticHudTheme.Gold;var promptRect=(RectTransform)hint.transform;promptRect.anchorMin=promptRect.anchorMax=promptRect.pivot=new Vector2(.5f,0);promptRect.anchoredPosition=new Vector2(0,174);
            desktopLegend=Label(root,"Controls",new Vector2(22,-674),new Vector2(1200,35),"Mouse: mover e atacar · Shift: atacar parado · botão direito / E: habilidade · Espaço: esquiva · roda: zoom · F: interagir",13).gameObject;
            Label(root,"Scope",new Vector2(22,-640),new Vector2(320,24),world.Definition.displayName.ToUpperInvariant()+" · PRÉVIA LOCAL",12).color=RusticHudTheme.Muted;
            if(world.Journey!=null)
            {
                var dialogue = RusticHudTheme.Rect(root,"StoryNotice",Top,new Vector2(20,-116),new Vector2(315,126));
                RusticHudTheme.Panel(dialogue,RusticHudTheme.Ink,14);
                var heading = Label(dialogue,"StoryTitle",new Vector2(13,-6),new Vector2(237,29),"ACOMPANHAMENTO",13);
                heading.color = RusticHudTheme.Gold;
                storyNotice = Label(dialogue,"Message",new Vector2(13,-37),new Vector2(289,77),"",15);
                RusticHudTheme.Button(dialogue,"ToggleStory",Vector2.one,new Vector2(-7,-4),new Vector2(42,29),"−",() =>
                {
                    storyCollapsed = !storyCollapsed;
                    storyNotice.gameObject.SetActive(!storyCollapsed);
                    dialogue.sizeDelta = new Vector2(315,storyCollapsed ? 38 : 126);
                    dialogue.Find("ToggleStory").GetComponentInChildren<Text>().text = storyCollapsed ? "+" : "−";
                });
                movementMode.gameObject.SetActive(false);
                controlHint.gameObject.SetActive(false);
            }
            death=RusticHudTheme.Rect(root,"Death",Vector2.one*.5f,Vector2.zero,new Vector2(450,180)).gameObject;RusticHudTheme.Panel((RectTransform)death.transform,RusticHudTheme.Ink,20);
            Label(death.transform,"Title",new Vector2(25,-20),new Vector2(400,60),"Você caiu. Retorne ao refúgio.",23);RusticHudTheme.Button(death.transform,"Respawn",Top,new Vector2(25,-100),new Vector2(400,50),"REAPARECER",world.Respawn);death.SetActive(false);
            interact=new InputAction("RegionalInteract",InputActionType.Button);interact.AddBinding("<Keyboard>/f");interact.AddBinding("<Gamepad>/buttonWest");interact.performed+=OnInteract;interact.Enable();
            weaponModeKey = new InputAction("RegionalWeaponMode", InputActionType.Button, "<Keyboard>/tab");
            weaponModeKey.performed += OnWeaponMode;
            weaponModeKey.Enable();
            for (int i = 0; i < skillKeys.Length; i++)
            {
                int slot = i;
                var action = new InputAction("RpgSkill"+i, InputActionType.Button, "<Keyboard>/"+(i+1));
                action.performed += _ => CastRegionalSkill(slot);
                skillKeys[i] = action;
                action.Enable();
            }
            world.Hero.GetComponent<ArenaInput>().PointerBlocked=p=>EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject();
            if(Application.isMobilePlatform)BuildTouchControls(root);else BuildPcActions(root);
            gameObject.AddComponent<CampaignPerformanceHud>().BuildRegional(root,world);
            gameObject.AddComponent<RegionalSettingsMenu>().Build(root,world);
            var inventory=world.GetComponent<RegionalInventory>();if(inventory!=null)gameObject.AddComponent<CoopInventoryHud>().Configure(inventory,root);
            if(town!=null&&System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-runTests")<0)StartCoroutine(PlayWorldIntro(root));
        }
        bool storyCollapsed;
        GameObject journalPanel;
        Text journalText; GameObject mapPanel,mapLegend,desktopLegend; public bool JournalOpen=>journalPanel!=null&&journalPanel.activeSelf;
        void BuildJournal(RectTransform root)
        {
            RusticHudTheme.IconButton(root,"OpenJournal",Vector2.one,new Vector2(-76,-20),HudSymbol.Scroll,()=>
            {
                if(JournalOpen){CloseJournal();return;} journalPanel.SetActive(true);var input=world.Hero.GetComponent<ArenaInput>();input.BlockCommands(true);world.Hero.GetComponent<PlayerMotor>().StopWalking();
                journalPanel.transform.SetAsLastSibling();
                RefreshJournal();
            });
            var full=RusticHudTheme.Rect(root,"MissionJournal",Vector2.zero,Vector2.zero,Vector2.zero);
            full.anchorMax=Vector2.one;
            full.offsetMin=full.offsetMax=Vector2.zero;
            full.gameObject.AddComponent<Image>().color=new Color(.015f,.025f,.02f,.9f);
            journalPanel=full.gameObject;
            var panel=RusticHudTheme.Rect(full,"JournalPanel",Vector2.one*.5f,Vector2.zero,new Vector2(760,460));
            RusticHudTheme.Panel(panel,RusticHudTheme.Ink,24);
            Label(panel,"JournalTitle",new Vector2(28,-20),new Vector2(520,44),"DIÁRIO DE MISSÕES",27);
            journalText=Label(panel,"JournalEntries",new Vector2(28,-80),new Vector2(690,260),"",21);
            RusticHudTheme.Button(panel,"CloseJournal",Top,new Vector2(490,-375),new Vector2(235,48),"VOLTAR",CloseJournal);
            journalPanel.SetActive(false);
        }
        public void CloseJournal()
        {
            if(!JournalOpen)return;
            journalPanel.SetActive(false);
            world.Hero.GetComponent<ArenaInput>().BlockCommands(false);
        }
        void RefreshJournal()
        {
            if(journalPanel==null||!journalPanel.activeSelf)return;
            var journey=world.Journey;
            string origin = "\nRefúgio do Último Sino · Nara, perto da tenda.\n";
            var combat = world.Hero.GetComponent<HeroCombat>();
            string techniques = "";
            for (int i = 0; i < journey.UnlockedSkills; i++)
                if (combat.RegionalSkill(i) is AbilityDefinition skill) techniques += "\n" + (i+1) + " · " + skill.displayName;
            journalText.text = journey.Completed
                ? "CONCLUÍDAS\n" + journey.Definition.title + origin + "Obtido: 1 Fragmento de Memória. Catacumbas liberadas." + techniques
                : journey.Accepted
                    ? "EM ANDAMENTO\n" + journey.Definition.title + origin + journey.Objective
                      + "\nRecompensa: 1 Fragmento de Memória e acesso às Catacumbas." + techniques
                    : "EM ANDAMENTO\nNenhuma missão aceita. Converse com Nara no Refúgio.\n\nCONCLUÍDAS\nNenhuma missão concluída.";
        }

        GameObject storyModal, expeditionModal;
        Text dialogueBody, dialogueContinue, areaName;
        int dialoguePage;
        static readonly string[] NaraLines =
        {
            "Bem-vindo ao Refúgio, viajante. Pode respirar. Se veio pela estrada, deve estar querendo saber por que aqui dentro é tão calmo.",
            "O santuário mantém a Distorção do lado de fora. Lá fora, até quem cresceu nesta costa pode perder o rumo. Mandei um grupo investigar, mas ainda não voltou.",
            "Eles passaram pelo cais, pela pedreira e pelo jardim. Se quiser ajudar, pegue a estrada sul. Procure pistas em dois desses lugares e leve o que encontrar ao Daro."
        };

        void BuildStoryPanels(RectTransform root)
        {
            var strip = RusticHudTheme.Rect(root, "AreaBanner", new Vector2(.5f,1), new Vector2(0,-20), new Vector2(370,42));
            strip.pivot = new Vector2(.5f,1);
            RusticHudTheme.Panel(strip, RusticHudTheme.Ink, 12);
            areaName = Label(strip,"CurrentArea",new Vector2(8,-3),new Vector2(354,36),"",20);
            areaName.alignment = TextAnchor.MiddleCenter;
            areaName.color = RusticHudTheme.Gold;

            if (world.Journey != null)
            {
                var panel = BuildModal(root, "Nara · Cartógrafa", out storyModal);
                dialogueBody = Label(panel,"Dialogue",new Vector2(28,-78),new Vector2(584,145),"",23);
                var next = RusticHudTheme.Button(panel,"ContinueDialogue",Top,new Vector2(295,-228),new Vector2(315,52),"CONTINUAR",AdvanceDialogue);
                dialogueContinue = next.GetComponentInChildren<Text>();
                RusticHudTheme.Button(panel,"DeclineDialogue",Top,new Vector2(28,-228),new Vector2(235,52),"AGORA NÃO",() => CloseStoryModal());
                storyModal.SetActive(false);
            }
            if (town == null) return;
            var routes = BuildModal(root, "Expedições", out expeditionModal);
            Label(routes,"ExpeditionInfo",new Vector2(28,-78),new Vector2(584,145),
                "Desafios de combate para 1 a 4 jogadores.\n\nEstrada das Cinzas · Inicial\nRaízes do Breu · Veterano\nForja dos Dois Sóis · Desafiador",21);
            RusticHudTheme.Button(routes,"CancelExpedition",Top,new Vector2(28,-228),new Vector2(235,52),"VOLTAR",() => CloseStoryModal());
            RusticHudTheme.Button(routes,"ViewRoutes",Top,new Vector2(295,-228),new Vector2(315,52),"VER ROTAS E GRUPOS",() =>
            {
                CloseStoryModal();
                town.ConfirmExpeditions();
            });
            expeditionModal.SetActive(false);
        }

        void AdvanceDialogue()
        {
            if (dialoguePage < NaraLines.Length - 1)
            {
                dialogueBody.text = NaraLines[++dialoguePage];
                dialogueContinue.text = dialoguePage == NaraLines.Length - 1 ? "ACEITAR INVESTIGAÇÃO" : "CONTINUAR";
                return;
            }
            if (town != null) town.AcceptStoryOffer();
            else world.Journey.AcceptOffer();
            CloseStoryModal();
        }

        RectTransform BuildModal(RectTransform root, string title, out GameObject overlay)
        {
            var full = RusticHudTheme.Rect(root,title,Vector2.zero,Vector2.zero,Vector2.zero);
            full.anchorMax = Vector2.one;
            full.offsetMin = full.offsetMax = Vector2.zero;
            full.gameObject.AddComponent<Image>().color = new Color(0,0,0,.76f);
            overlay = full.gameObject;
            var panel = RusticHudTheme.Rect(full,"Panel",Vector2.one*.5f,Vector2.zero,new Vector2(640,310));
            RusticHudTheme.Panel(panel,RusticHudTheme.Ink,24);
            Label(panel,"Title",new Vector2(28,-22),new Vector2(584,44),title,28).color = RusticHudTheme.Gold;
            return panel;
        }

        public void OpenStoryDialogue()
        {
            if (!OpenModal(storyModal)) return;
            dialoguePage = 0;
            dialogueBody.text = NaraLines[0];
            dialogueContinue.text = "CONTINUAR";
        }

        public void OpenExpeditionPanel() => OpenModal(expeditionModal);

        bool OpenModal(GameObject panel)
        {
            var input = world.Hero.GetComponent<ArenaInput>();
            if (panel == null || input.CombatBlocked) return false;
            input.Desktop?.Cancel();
            world.Hero.GetComponent<PlayerMotor>().StopWalking();
            input.BlockCommands(true);
            panel.transform.SetAsLastSibling();
            panel.SetActive(true);
            return true;
        }

        public bool CloseStoryModal()
        {
            var panel = storyModal != null && storyModal.activeSelf ? storyModal : expeditionModal;
            if (panel == null || !panel.activeSelf) return false;
            panel.SetActive(false);
            world.Hero.GetComponent<ArenaInput>().BlockCommands(false);
            return true;
        }

        void RefreshArea()
        {
            string name = world.Definition.displayName;
            float nearest = 27f * 27f;
            if (town == null)
            {
                foreach (var mark in world.Definition.landmarks)
                {
                    var offset = mark.position - world.Hero.transform.position;
                    offset.y = 0;
                    if (offset.sqrMagnitude >= nearest) continue;
                    nearest = offset.sqrMagnitude;
                    name = mark.label;
                }
            }
            areaName.text = name;
        }

        readonly string[] shopItems={"cais-blade","sal-staff","vento-bow","estrada-charm"};
        readonly int[] shopPrices={45,45,45,35};
        GameObject shop;
        Text shopBalance,shopFeedback;
        Button[] shopButtons;
        RegionalInventory shopInventory;
        public bool ShopOpen=>shop!=null&&shop.activeSelf;
        void BuildShop(RectTransform root)
        {
            shopInventory=world.GetComponent<RegionalInventory>();
            if(shopInventory==null)return;
            var full=RusticHudTheme.Rect(root,"TownShop",Vector2.zero,Vector2.zero,Vector2.zero);
            full.anchorMax=Vector2.one;
            full.offsetMin=full.offsetMax=Vector2.zero;
            full.gameObject.AddComponent<Image>().color=new Color(.015f,.025f,.02f,.92f);
            shop=full.gameObject;
            var panel=RusticHudTheme.Rect(full,"ShopPanel",Vector2.one*.5f,Vector2.zero,new Vector2(820,530));
            RusticHudTheme.Panel(panel,RusticHudTheme.Ink,24);
            Label(panel,"ShopTitle",new Vector2(25,-20),new Vector2(540,42),"BANCA DO TÉO",28).color=RusticHudTheme.Gold;
            shopBalance=Label(panel,"ShopBalance",new Vector2(25,-66),new Vector2(750,40),"",19);
            shopFeedback=Label(panel,"ShopFeedback",new Vector2(25,-398),new Vector2(770,45),"",17);
            shopButtons=new Button[shopItems.Length];
            for(int i=0;i<shopItems.Length;i++)
            {
                int index=i;
                var item=shopInventory.Catalog.Find(shopItems[i]);
                if(item==null)continue;
                float row=118+i*66;
                Label(panel,"Item"+i,new Vector2(25,-row),new Vector2(540,47),item.displayName+" · Comum · Nível "+item.minimumLevel,18);
                shopButtons[i]=RusticHudTheme.Button(panel,"Buy"+i,Top,new Vector2(590,-row),new Vector2(200,45),"COMPRAR · "+shopPrices[i]+" OURO",()=>BuyShopItem(index));
            }
            RusticHudTheme.Button(panel,"CloseShop",Top,new Vector2(565,-456),new Vector2(225,46),"VOLTAR · ESC",CloseShop);
            shop.SetActive(false);
        }
        public void OpenShop()
        {
            if(shop==null||shopInventory.Service==null)return;
            CloseJournal();
            shop.transform.SetAsLastSibling();
            shop.SetActive(true);
            world.Hero.GetComponent<ArenaInput>().BlockCommands(true);
            world.Hero.GetComponent<PlayerMotor>().StopWalking();
            shopFeedback.text="Equipamentos comuns. Confira a compatibilidade na mochila.";
            RefreshShop();
        }
        public void CloseShop()
        {
            if(!ShopOpen)return;
            shop.SetActive(false);
            world.Hero.GetComponent<ArenaInput>().BlockCommands(false);
        }
        void BuyShopItem(int index)
        {
            if(!ShopOpen||!world.Hero.Health.IsAlive)return;
            var service=shopInventory.Service;
            service.BuyCommonEquipment(shopItems[index],shopPrices[index]);
            shopFeedback.text=service.Message;
            RefreshShop();
        }
        void RefreshShop()
        {
            var profile=shopInventory.Service.Profile;
            int used=InventoryRules.BagCount(profile);
            int capacity=shopInventory.Catalog.inventoryCapacity;
            shopBalance.text=profile.coins+" ouro   ·   Mochila "+used+" / "+capacity;
            for(int i=0;i<shopButtons.Length;i++)
                if(shopButtons[i]!=null)shopButtons[i].interactable=profile.coins>=shopPrices[i]&&used<capacity&&profile.expedition==null;
        }
        const string IntroSeenKey="Forja.WorldIntro.v3";
        System.Collections.IEnumerator PlayWorldIntro(RectTransform root)
        {
            if(PlayerPrefs.GetInt(IntroSeenKey+CampaignHome.LocalAccountId)!=0)yield break;
            var overlay=RusticHudTheme.Rect(root,"WorldIntro",Vector2.zero,Vector2.zero,Vector2.zero);
            overlay.anchorMax=Vector2.one;
            overlay.offsetMin=overlay.offsetMax=Vector2.zero;
            overlay.SetAsLastSibling();
            overlay.gameObject.AddComponent<Image>().color=Color.black;
            overlay.gameObject.AddComponent<RectMask2D>();
            MenuArtwork.Fill(overlay,"IntroArt","MenuArt/RoutesBackdrop",Color.white);
            var artwork=overlay.Find("IntroArt");
            var group=overlay.gameObject.AddComponent<CanvasGroup>();
            var caption=RusticHudTheme.Label(overlay,"Story",new Vector2(.5f,0),new Vector2(0,88),new Vector2(840,110),"",25,TextAnchor.MiddleCenter);
            var shade=caption.gameObject.AddComponent<Outline>();
            shade.effectColor=Color.black;
            bool skipped=false;
            RusticHudTheme.Button(overlay,"SkipIntro",new Vector2(1,1),new Vector2(-25,-22),new Vector2(195,48),"PULAR",()=>skipped=true);
            var input=world.Hero.GetComponent<ArenaInput>();
            input.BlockCommands(true);
            var frame=RusticHudTheme.Rect(overlay,"Video",Vector2.zero,Vector2.zero,Vector2.zero);
            frame.anchorMax=Vector2.one;
            frame.offsetMin=frame.offsetMax=Vector2.zero;
            frame.SetSiblingIndex(0);
            var screen=frame.gameObject.AddComponent<RawImage>();
            screen.raycastTarget=false;
            var aspect=frame.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent;
            aspect.aspectRatio=16f/9f;
            var texture=new RenderTexture(1280,720,0);
            screen.texture=texture;
            var video=overlay.gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();
            video.playOnAwake=false;
            video.source=UnityEngine.Video.VideoSource.Url;
            video.url=Application.streamingAssetsPath+"/Opening/Forja-das-Mares-Cinematica.mp4";
            video.renderMode=UnityEngine.Video.VideoRenderMode.RenderTexture;
            video.targetTexture=texture;
            video.audioOutputMode=UnityEngine.Video.VideoAudioOutputMode.Direct;
            bool finished=false,failed=false;
            video.loopPointReached+=_=>finished=true;
            video.errorReceived+=(_,message)=>{failed=true;Debug.LogWarning("Abertura: "+message);};
            video.Prepare();
            float deadline=Time.realtimeSinceStartup+8;
            while(!video.isPrepared&&!failed&&!skipped&&Time.realtimeSinceStartup<deadline)
            {
                if(Keyboard.current?.escapeKey.wasPressedThisFrame==true)skipped=true;
                yield return null;
            }
            if(video.isPrepared&&!failed&&!skipped)
            {
                if(artwork!=null)artwork.gameObject.SetActive(false);
                caption.transform.SetAsLastSibling();
                video.Play();
                deadline=Time.realtimeSinceStartup+40;
                while(!finished&&!failed&&!skipped&&Time.realtimeSinceStartup<deadline)
                {
                    double t=video.time;
                    caption.text=t<8?"No Refúgio, os caminhos ainda levam para casa. Dizem que é por causa do santuário.":t<19?"Lá fora, a Distorção muda as estradas e as lembranças de quem passa pela névoa.":t<29?"Quando a névoa baixa, ruínas reaparecem. Nas pedras, há marcas de alguém que chegou primeiro.":"";
                    if(Keyboard.current?.escapeKey.wasPressedThisFrame==true)skipped=true;
                    yield return null;
                }
            }
            video.Stop();
            video.targetTexture=null;
            Destroy(texture);
            overlay.gameObject.SetActive(false);
            Destroy(overlay.gameObject);
            input.BlockCommands(false);
            if(finished||skipped)PlayerPrefs.SetInt(IntroSeenKey+CampaignHome.LocalAccountId,1);
            PlayerPrefs.Save();
        }

        void BuildVitals(RectTransform root)
        {
            var panel=RusticHudTheme.Rect(root,"HeroVitals",Top,new Vector2(18,-16),new Vector2(360,92));RusticHudTheme.Panel(panel,new Color(.025f,.045f,.055f,.94f),20);
            var frame=RusticHudTheme.Rect(panel,"Portrait",Top,new Vector2(8,-8),new Vector2(74,74));RusticHudTheme.Panel(frame,RusticHudTheme.Ink,37);frame.gameObject.AddComponent<Mask>().showMaskGraphic=true;
            var image=RusticHudTheme.Rect(frame,"Hero",Vector2.one*.5f,Vector2.zero,new Vector2(68,68)).gameObject.AddComponent<RawImage>();image.raycastTarget=false;portrait=frame.gameObject.AddComponent<EquipmentPaperDoll>();portrait.Configure(image,true);
            life=Label(panel,"HeroName",new Vector2(94,-7),new Vector2(252,26),"",18);life.color=RusticHudTheme.Gold;
            var track=RusticHudTheme.Rect(panel,"HealthTrack",Top,new Vector2(94,-36),new Vector2(238,16));track.gameObject.AddComponent<Image>().color=new Color(.18f,.05f,.055f);
            healthFill=RusticHudTheme.Rect(track,"Fill",Top,Vector2.zero,new Vector2(238,14));healthFill.gameObject.AddComponent<Image>().color=new Color(.76f,.12f,.18f);
            healthCaption=Label(panel,"HealthValue",new Vector2(96,-32),new Vector2(230,23),"",13);
            var focus=RusticHudTheme.Rect(panel,"FocusTrack",Top,new Vector2(94,-64),new Vector2(238,5));focus.gameObject.AddComponent<Image>().color=RusticHudTheme.Surface;
            superFill=RusticHudTheme.Rect(focus,"Fill",Top,Vector2.zero,new Vector2(0,5));superFill.gameObject.AddComponent<Image>().color=new Color(.18f,.6f,.85f);
            superCaption=Label(panel,"SuperValue",new Vector2(94,-70),new Vector2(238,16),"",11);
        }
        Button RoundAction(RectTransform root,string name,Vector2 position,float size,string text,HudSymbol symbol,UnityEngine.Events.UnityAction action)
        {
            var button=RusticHudTheme.Button(root,name,new Vector2(1,0),position,Vector2.one*size,"",action);var surface=button.GetComponent<RoundedHudGraphic>();surface.radius=size/2;surface.borderColor=RusticHudTheme.Gold;surface.color=new Color(.035f,.065f,.09f,.94f);
            RusticHudTheme.Icon(button.transform,symbol,Vector2.one*.5f,new Vector2(0,10),size*.43f);var caption=button.GetComponentInChildren<Text>();caption.text=text;caption.fontSize=12;caption.rectTransform.anchoredPosition=new Vector2(0,-size*.28f);caption.rectTransform.sizeDelta=new Vector2(size-6,22);return button;
        }
        void BuildPcActions(RectTransform root)
        {
            var input=world.Hero.GetComponent<ArenaInput>();
            RoundAction(root,"RegionalDodge",new Vector2(-158,20),64,"ESQUIVA",HudSymbol.Dodge,input.RequestDodge);
            RoundAction(root,"RegionalAttack",new Vector2(-84,20),64,"ATAQUE",HudSymbol.Sword,input.RequestAttack);
            RoundAction(root,"RegionalInteract",new Vector2(-232,20),64,"USAR",HudSymbol.Interact,world.Interact);
            BuildSkillBar(root,120);
        }
        Button attackMode;
        void BuildSkillBar(RectTransform root,float height)
        {
            var input=world.Hero.GetComponent<ArenaInput>();
            var combat=world.Hero.GetComponent<HeroCombat>();
            for (int i = 0; i < skillButtons.Length; i++)
            {
                int slot = i;
                var symbol = i == 0 ? HudSymbol.Target : i == 1 ? HudSymbol.Fury : HudSymbol.Rune;
                skillButtons[i] = RoundAction(root,"SkillSlot"+(i+1),new Vector2(-315+75*i,height),64,"",symbol,() => CastRegionalSkill(slot));
                skillLabels[i] = skillButtons[i].GetComponentInChildren<Text>();
            }
            attackMode=RoundAction(root,"AttackMode",new Vector2(-88,height),68,"NORMAL",HudSymbol.Sword,combat.ToggleWeaponMode);
        }
        void BuildTouchControls(RectTransform root)
        {
            var input=world.Hero.GetComponent<ArenaInput>();var stick=RusticHudTheme.Rect(root,"RegionalStick",Vector2.zero,new Vector2(120,170),new Vector2(160,160));stick.pivot=Vector2.one*.5f;
            var ring=stick.gameObject.AddComponent<RoundedHudGraphic>();ring.color=new Color(.12f,.19f,.16f,.65f);ring.radius=80;
            var thumb=RusticHudTheme.Rect(stick,"Thumb",Vector2.one*.5f,Vector2.zero,new Vector2(52,52));var cap=thumb.gameObject.AddComponent<RoundedHudGraphic>();cap.color=RusticHudTheme.Gold;cap.radius=26;cap.raycastTarget=false;
            stick.gameObject.AddComponent<TouchMoveStick>().Configure(input,thumb);
            RoundAction(root,"RegionalAttack",new Vector2(-22,70),112,"ATAQUE",HudSymbol.Sword,input.RequestAttack).gameObject.AddComponent<HoldAttackButton>().Configure(input);
            RoundAction(root,"RegionalDodge",new Vector2(-148,50),78,"ESQUIVA",HudSymbol.Dodge,input.RequestDodge);
            RoundAction(root,"RegionalInteract",new Vector2(-242,64),74,"USAR",HudSymbol.Interact,world.Interact);
            BuildSkillBar(root,205);
            movementMode.gameObject.SetActive(false);
            controlHint.gameObject.SetActive(false);
            foreach(string name in new[]{"Controls","Scope","MapLegend"}){var label=root.Find(name);if(label!=null)label.gameObject.SetActive(false);}
            var prompt=(RectTransform)hint.transform;prompt.anchorMin=prompt.anchorMax=prompt.pivot=new Vector2(.5f,0);prompt.anchoredPosition=new Vector2(0,24);
        }
        void CastRegionalSkill(int slot)
        {
            var input = world.Hero.GetComponent<ArenaInput>();
            if (input.CombatBlocked) return;
            var combat = world.Hero.GetComponent<HeroCombat>();
            if (combat.TryRegionalSkill(slot)) input.Desktop?.StopForAbility();
        }

        void OnWeaponMode(InputAction.CallbackContext _)
        {
            if (!world.Hero.Health.IsAlive || world.Hero.GetComponent<ArenaInput>().CombatBlocked) return;
            world.Hero.GetComponent<HeroCombat>().ToggleWeaponMode();
        }

        void OnInteract(InputAction.CallbackContext _){if((journalPanel==null||!journalPanel.activeSelf)&&!world.Hero.GetComponent<ArenaInput>().CombatBlocked)world.Interact();}
        void Update()
        {
            var combat=world.Hero.GetComponent<HeroCombat>();
            if (attackMode != null)
            {
                attackMode.interactable = combat.CanUseWeapon;
                attackMode.GetComponentInChildren<Text>().text = (combat.HeavyMode ? "PESADO" : "NORMAL") + (Application.isMobilePlatform ? "" : " · TAB");
                for (int i = 0; i < skillButtons.Length; i++)
                {
                    bool unlocked = combat.RegionalSkillUnlocked(i);
                    float remaining = combat.RegionalSkillCooldown(i);
                    skillButtons[i].interactable = unlocked && remaining <= 0 && world.Hero.Health.IsAlive;
                    string role = i == 0 ? "ALVO" : i == 1 ? "ÁREA" : "CONTROLE";
                    skillLabels[i].text = !unlocked ? (i+1)+" · VAZIO" : remaining > 0 ? Mathf.CeilToInt(remaining)+"s" : (i+1)+" · "+role;
                }
            }
            poll-=Time.unscaledDeltaTime;if(poll>0)return;poll=.1f;
            RefreshArea();
            movementMode.GetComponentInChildren<Text>().text="CONTROLES · AJUDA";
            controlHint.text=Application.isMobilePlatform?"Joystick: mover · ataque: mira automática\nInteragir: conversar e investigar":"Chão: andar · alvo: atacar sem perseguir\nShift + ataque: disparar parado sem alvo";
            var hero=world.Hero;life.text=string.IsNullOrEmpty(CampaignHome.CharacterName)?hero.Current.displayName:CampaignHome.CharacterName+" · "+hero.Current.displayName;
            healthCaption.text="VIDA  "+hero.Health.Current+" / "+hero.Health.Maximum;healthFill.sizeDelta=new Vector2(238f*hero.Health.Current/Mathf.Max(1,hero.Health.Maximum),14);
            float focus=hero.GetComponent<HeroCombat>().SuperProgress;superFill.sizeDelta=new Vector2(238*focus,5);superCaption.text="SUPER "+Mathf.RoundToInt(focus*100)+"%";
            portrait.Refresh(hero.GetComponent<HeroPresentation>(),world.GetComponent<RegionalInventory>()?.Service?.Profile??previewProfile,hero.Current.heroId);
            objective.text=world.Definition.displayName.ToUpperInvariant()+"\nPatrulhas derrotadas: "+world.ClearedCount+" / "+world.Definition.camps.Length+"\n"+(world.PortalReady?"O portal dos desafios está aberto.":"Derrote a primeira patrulha de cada local.");
            var navigationNotice=world.Hero.GetComponent<ArenaInput>().Desktop?.Notice;hint.text=string.IsNullOrEmpty(navigationNotice)?world.InteractionHint:navigationNotice;death.SetActive(!hero.Health.IsAlive);
            if(world.Journey!=null){objective.text=world.Journey.Definition.title.ToUpperInvariant()+"\n"+world.Journey.Objective;hint.text=string.IsNullOrEmpty(world.Journey.Hint)?world.InteractionHint:world.Journey.Hint;if(storyNotice!=null)storyNotice.text=world.Journey.Notice ?? world.Journey.Objective;}
            if(town!=null){if(storyNotice!=null)storyNotice.text=town.CurrentNotice;hint.text=town.Hint;bool established=world.Journey!=null&&world.Journey.Accepted;mapPanel.SetActive(established);mapLegend.SetActive(established&&!Application.isMobilePlatform);desktopLegend.SetActive(false);} if(world.Journey!=null){objective.transform.parent.gameObject.SetActive(false);desktopLegend.SetActive(false);RefreshJournal();}
            if(Application.isMobilePlatform)hint.text=hint.text.Replace("F · ","INTERAGIR · ");
        }
        void LateUpdate() => UpdateDamageNumbers();

        const float DamageLifetime = .92f;
        sealed class DamagePopup
        {
            public Text Text;
            public Health Victim;
            public bool CrystalMarker;
            public Vector3 Position;
            public float Age, Drift;
            public Color Tint;
        }
        DamagePopup[] damagePopups;
        RectTransform damageLayer;
        Camera damageCamera;
        int nextPopup;

        void BuildDamageNumbers(RectTransform root)
        {
            damageLayer = RusticHudTheme.Rect(root,"DamageNumbers",Vector2.zero,Vector2.zero,Vector2.zero);
            damageLayer.anchorMax = Vector2.one;
            damageLayer.offsetMin = damageLayer.offsetMax = Vector2.zero;
            damageLayer.SetAsFirstSibling();
            damageCamera = Camera.main;
            damagePopups = new DamagePopup[24];
            for (int i=0;i<damagePopups.Length;i++)
            {
                var label = RusticHudTheme.Label(damageLayer,"Damage",Vector2.zero,Vector2.zero,new Vector2(165,52),"",25,TextAnchor.MiddleCenter);
                label.rectTransform.pivot = Vector2.one * .5f;
                label.fontStyle = FontStyle.Bold;
                var outline = label.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0,0,0,.9f);
                outline.effectDistance = new Vector2(2,-2);
                label.gameObject.SetActive(false);
                damagePopups[i] = new DamagePopup { Text = label, Age = DamageLifetime };
            }
            Health.DamageTaken += ShowDamage;
            Health.CrystalShattered += ShowShatter;
            Health.CrystalApplied += ShowCrystal;
        }

        void ShowDamage(Health victim, int amount, bool critical)
        {
            bool incoming = victim == world.Hero.Health;
            if (!incoming && victim.Faction != CombatFaction.Enemy && victim.Faction != CombatFaction.Training) return;
            if (amount <= 0 || damageCamera == null) return;
            var popup = damagePopups[nextPopup];
            nextPopup = (nextPopup + 1) % damagePopups.Length;
            popup.Victim = victim;
            popup.CrystalMarker = false;
            popup.Position = victim.transform.position + Vector3.up * (incoming ? 2f : 1.55f);
            popup.Age = 0;
            popup.Drift = (nextPopup % 5 - 2) * 15;
            popup.Tint = incoming ? new Color(1,.32f,.32f) : critical ? new Color(1,.8f,.24f) : RusticHudTheme.Parchment;
            popup.Text.rectTransform.sizeDelta = new Vector2(165,52);
            popup.Text.fontSize = critical ? 29 : 24;
            popup.Text.text = (incoming ? "−" : "") + amount + (critical ? "  CRÍTICO" : "");
        }

        void ShowCrystal(Health victim)
        {
            if (damageCamera == null) return;
            foreach (var existing in damagePopups)
                if (existing.CrystalMarker && existing.Victim == victim && existing.Age < DamageLifetime) return;
            var popup = damagePopups[nextPopup];
            nextPopup = (nextPopup + 1) % damagePopups.Length;
            popup.Victim = victim;
            popup.CrystalMarker = true;
            popup.Age = 0;
            popup.Drift = 0;
            popup.Tint = new Color(.56f,.9f,1f);
            popup.Text.fontSize = 17;
            popup.Text.text = "◆ CRISTALIZADO";
            popup.Text.rectTransform.sizeDelta = new Vector2(180,36);
        }

        void ShowShatter(Health victim)
        {
            var popup = damagePopups[(nextPopup + damagePopups.Length - 1) % damagePopups.Length];
            if (popup.Victim != victim || popup.Age != 0) return;
            popup.Tint = new Color(.56f,.9f,1f);
            popup.Text.fontSize = 21;
            popup.Text.text += "\nESTILHAÇO PRISMÁTICO";
            popup.Text.rectTransform.sizeDelta = new Vector2(270,62);
        }

        void UpdateDamageNumbers()
        {
            if (damageCamera == null || damagePopups == null) return;
            foreach (var popup in damagePopups)
            {
                if (popup.Age >= DamageLifetime) continue;
                if (popup.CrystalMarker)
                {
                    if (popup.Victim == null || !popup.Victim.isActiveAndEnabled || !popup.Victim.IsCrystallized)
                    {
                        popup.Age = DamageLifetime;
                        popup.Text.gameObject.SetActive(false);
                        continue;
                    }
                    popup.Position = popup.Victim.transform.position + Vector3.up * 2.3f;
                }
                else popup.Age += Time.unscaledDeltaTime;
                Vector3 view = damageCamera.WorldToViewportPoint(popup.Position);
                bool visible = popup.Age < DamageLifetime && view.z > 0 && view.x > 0 && view.x < 1 && view.y > 0 && view.y < 1;
                popup.Text.gameObject.SetActive(visible);
                if (!visible) continue;
                float t = popup.Age / DamageLifetime;
                var rect = popup.Text.rectTransform;
                rect.anchoredPosition = Vector2.Scale(new Vector2(view.x,view.y),damageLayer.rect.size)
                    + new Vector2(popup.Drift * (.65f + t),42f * t);
                float punch = Mathf.Sin(Mathf.Clamp01(t * 5f) * Mathf.PI) * .28f;
                rect.localScale = Vector3.one * (1 + punch);
                Color color = popup.Tint;
                color.a = 1 - Mathf.InverseLerp(.56f,1,t);
                popup.Text.color = color;
            }
        }

        void OnDestroy(){weaponModeKey?.Dispose();foreach(var key in skillKeys)key?.Dispose();Health.DamageTaken-=ShowDamage;Health.CrystalShattered-=ShowShatter;Health.CrystalApplied-=ShowCrystal;if(interact!=null){interact.performed-=OnInteract;interact.Dispose();}if(world!=null&&world.Hero!=null)world.Hero.GetComponent<ArenaInput>().PointerBlocked=null;}
    }
    public sealed class RegionalMap:MaskableGraphic
    {
        RegionalWorld world;float poll;public void Configure(RegionalWorld value){world=value;raycastTarget=false;}
        void Update(){poll-=Time.unscaledDeltaTime;if(poll<=0){poll=.15f;SetVerticesDirty();}}
        Vector2 Point(Vector3 p){var b=world.Definition.bounds;return new Vector2((p.x-b.min.x)/b.size.x,(p.z-b.min.z)/b.size.z)*rectTransform.rect.size+rectTransform.rect.min;}
        void Dot(VertexHelper vh,Vector3 point,Color tint,float size){Vector2 p=Point(point);int n=vh.currentVertCount;vh.AddVert(p+new Vector2(-size,-size),tint,Vector2.zero);vh.AddVert(p+new Vector2(-size,size),tint,Vector2.zero);vh.AddVert(p+new Vector2(size,size),tint,Vector2.zero);vh.AddVert(p+new Vector2(size,-size),tint,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
        void Path(VertexHelper vh,Vector3 a,Vector3 b)
        {Vector2 start=Point(a),end=Point(b),delta=end-start;var side=new Vector2(-delta.y,delta.x).normalized*.8f;int n=vh.currentVertCount;var tint=new Color(.65f,.53f,.3f,.8f);vh.AddVert(start-side,tint,Vector2.zero);vh.AddVert(start+side,tint,Vector2.zero);vh.AddVert(end+side,tint,Vector2.zero);vh.AddVert(end-side,tint,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
        void DrawQuestMarkers(VertexHelper vh)
        {
            var journey = world.Journey;
            if (journey == null || !journey.Accepted || journey.Completed) return;
            foreach (var point in journey.Definition.points)
            {
                if (journey.ClueCount >= journey.Definition.requiredClues)
                {
                    if (point.interaction == RegionalInteraction.Artificer)
                        Dot(vh, point.position, RusticHudTheme.Gold, 7);
                }
                else if (point.interaction == RegionalInteraction.Clue && world.Definition.regionId == "costa-ato1" && !journey.HasClue(point.clue))
                    Dot(vh, point.position, RusticHudTheme.Gold, 7);
            }
        }
        protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(world==null)return;if(world.Definition.connections!=null&&world.Definition.connections.Length>0){foreach(var edge in world.Definition.connections)Path(vh,edge.from,edge.to);}else for(int i=1;i<world.Definition.route.Length;i++)Path(vh,world.Definition.route[i-1],world.Definition.route[i]);Dot(vh,world.Definition.refuge,Color.green,4);for(int i=0;i<world.Definition.camps.Length;i++)Dot(vh,world.Definition.camps[i].center,world.IsCampCleared(i)?Color.gray:new Color(1,.4f,.2f),4);if(world.Definition.landmarks!=null)foreach(var mark in world.Definition.landmarks)Dot(vh,mark.position,mark.color,3);DrawQuestMarkers(vh);Dot(vh,world.Definition.portal,Color.cyan,4);Dot(vh,world.Hero.transform.position,Color.white,3);}
    }
}
