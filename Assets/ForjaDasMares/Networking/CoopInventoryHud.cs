using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares.Networking
{
    // Session inventory reads the host mirror; every mutation sends a validated intention.
    [DefaultExecutionOrder(260)] public sealed class CoopInventoryHud : MonoBehaviour
    {
        [SerializeField] CoopDirector director;
        CoopProfileChannel channel; Font font; GameObject overlay; Text detail, header, notice;
        readonly Button[] rows = new Button[8]; Button equip, remove, discard;
        Image inspectedIcon,comparedIcon;CampaignCharacterSheet sheet;GameObject itemPopup;Text ownDetails,otherDetails,itemNotice;string compared;Button compareButton;Text comparisonSummary,comparisonHeading;InventoryComparisonView visualComparison;bool touchLayout;int PageSize=>touchLayout?4:8;
        ScrollRect resultScroll;Text resultText;RectTransform resultContent;
        RectTransform detailViewport;
        Button inventoryButton,resultButton,stockView,previous,next,closeButton;bool deliveries,expeditionView;Button expeditionButton;readonly Image[] icons=new Image[8];
        System.Collections.Generic.List<ItemInstance> SourceItems=>expeditionView?channel.LocalProfile?.expedition?.items??emptyItems:deliveries?channel.LocalProfile?.deliveries:channel.LocalProfile?.inventory;EquipmentPaperDoll paperDoll;readonly Button[] equipmentSlots=new Button[5];readonly Image[] equippedIcons=new Image[5];readonly HudIcon[] emptySlots=new HudIcon[5];
        static readonly System.Collections.Generic.List<ItemInstance> emptyItems=new System.Collections.Generic.List<ItemInstance>();
        Button classFilter,heroChoice,profileSource,secondaryEquip; readonly Button[] categoryTabs=new Button[6];
        string lastNotice, selectionMessage, itemContextNotice, pendingClaim;
        Button claimAll;Text capacity;RawImage characterPreview;bool bulkPending;int bulkRevision;string bulkMessage;float bulkUntil;
        int selectionRevision;float claimUntil;bool advancedDetails,itemContextBlocked;
        Button bagSource,advancedButton,resetComparison,clearFilters;Text pageLabel,emptyHint,sourceHint;
        readonly Text[] rowBadges=new Text[8];
        System.Comparison<ItemInstance> itemOrder;
        bool management;int category=-1;bool classOnly=true;
        CoopActor ActiveActor=>director.Actor(director.LocalSlot>=0?director.LocalSlot:0);
        readonly System.Collections.Generic.List<ItemInstance> filtered=new System.Collections.Generic.List<ItemInstance>();
        System.Collections.Generic.List<ItemInstance> CurrentItems=>filtered;
        void FilterItems()
        {
            filtered.Clear();var source=SourceItems;if(source==null)return;
            var hero=channel.Catalog.Hero(ActiveActor.Roster.Current.heroId);
            foreach(var item in source){if(!deliveries&&InventoryRules.Equipped(channel.LocalProfile,item.instanceId))continue;var def=channel.Catalog.Find(item.definitionId);if(def==null||category>=0&&(int)def.slot!=category&&!(category==1&&def.category==WeaponCategory.Daggers&&hero!=null&&hero.allowSecondaryWeapon)||classOnly&&hero!=null&&(def.compatibleHeroes&hero.affinity)==0)continue;filtered.Add(item);}
            if(itemOrder!=null)filtered.Sort(itemOrder);
        }
        bool CanManageNow
        {
            get
            {
                if (channel.HomeMode) return true;
                if (director.LocalSlot < 0) return false;
                if (director.Manager.IsServer) return channel.CanManageLocal;
                var actor = ActiveActor;
                if (actor == null) return false;
                if (director.Phase == CoopPhase.Lobby) return true;
                // Replicas cannot predict every host-owned effect. The host validates again
                // and returns its reason if an effect is still active when the request arrives.
                return actor.Health.IsAlive && director.Phase != CoopPhase.Loading && director.Phase != CoopPhase.Connecting
                    && (director.EncounterStatus != EncounterState.Spawning && director.EncounterStatus != EncounterState.Fighting
                        || director.Dungeon.OpenLandscape && !director.Dungeon.Encounter.Enemies.ThreatNear(actor.transform.position, 12));
            }
        }
        int page, revision = -1,lastHero=-1; string selected, pendingDiscard; bool results;
        public void Configure(CoopDirector session) => director = session;
        void Start()
        {
            itemOrder=CompareInventoryItems;
            channel = director.GetComponent<CoopProfileChannel>(); font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var parent = GetComponent<CoopHud>().SafeRoot;
            inventoryButton=TopRight(parent, "INVENTÁRIO", -119, () => Show(false));
            resultButton=TopRight(parent, "RESULTADO", -171, () => Show(true));
            overlay = new GameObject("CoopInventory", typeof(RectTransform), typeof(Image)); overlay.transform.SetParent(parent, false);
            var full = (RectTransform)overlay.transform; full.anchorMin = Vector2.zero; full.anchorMax = Vector2.one; full.offsetMin = full.offsetMax = Vector2.zero; overlay.GetComponent<Image>().color = new Color(.025f, .045f, .065f, .98f);
            header = Label(overlay.transform, "", new Vector2(30, -20), new Vector2(1000, 50), 23);
            closeButton=Button(overlay.transform, "FECHAR", new Vector2(1080, -20), new Vector2(150, 44), Close);
            for (int i = 0; i < rows.Length; i++)
            { int row = i; rows[i] = Button(overlay.transform, "", new Vector2(30, -95 - i * 57), new Vector2(560, 50), () => Select(row)); rows[i].name="InventoryItem_"+i; }
            previous=Button(overlay.transform, "ANTERIOR", new Vector2(30, -560), new Vector2(170, 42), () => { page = Mathf.Max(0, page - 1); selected = null; Refresh(); });
            next=Button(overlay.transform, "PRÓXIMA", new Vector2(215, -560), new Vector2(170, 42), () => { var items = CurrentItems; if (items != null && (page + 1) * PageSize < items.Count) page++; selected = null; Refresh(); });
            detail = Label(overlay.transform, "", new Vector2(620, -95), new Vector2(600, 415), 19);
            equip = Button(overlay.transform, "EQUIPAR", new Vector2(620, -535), new Vector2(185, 48), Equip);
            remove = Button(overlay.transform, "DESEQUIPAR", new Vector2(825, -535), new Vector2(185, 48), Unequip);
            discard = Button(overlay.transform, "DESCARTAR", new Vector2(1030, -535), new Vector2(185, 48), Discard);
            notice = Label(overlay.transform, "", new Vector2(30, -620), new Vector2(1180, 75), 18);
            if(director.Campaign!=null){BuildPaperDoll();BuildItemPopup();}
            overlay.SetActive(false);
        }
        void BuildPaperDoll()
        {
            touchLayout=Application.isMobilePlatform||Screen.width<1000;
            var panel=(RectTransform)overlay.transform;panel.anchorMin=panel.anchorMax=panel.pivot=Vector2.one*.5f;panel.anchoredPosition=Vector2.zero;panel.sizeDelta=new Vector2(1280,700);
            overlay.GetComponent<Image>().color=RusticHudTheme.Ink;RusticHudTheme.Skin(closeButton);RusticHudTheme.Skin(previous);RusticHudTheme.Skin(next);RusticHudTheme.Panel(panel,new Color(.035f,.06f,.05f,.72f),22);
            var backdrop=MenuArtwork.Fill(panel,"InventoryBackdrop","MenuArt/RoutesBackdrop",new Color(.65f,.7f,.68f,1));backdrop.transform.SetAsFirstSibling();
            var bagFrame=RusticHudTheme.Rect(panel,"BagFrame",new Vector2(0,1),new Vector2(18,-75),new Vector2(525,590));RusticHudTheme.Panel(bagFrame,new Color(.035f,.065f,.055f,.9f),18);bagFrame.SetSiblingIndex(2);
            var heroFrame=RusticHudTheme.Rect(panel,"HeroFrame",new Vector2(0,1),new Vector2(550,-95),new Vector2(665,450));RusticHudTheme.Panel(heroFrame,new Color(.035f,.065f,.055f,.82f),18);heroFrame.SetSiblingIndex(3);
            bagSource=Button(overlay.transform,"MOCHILA",new Vector2(30,-555),new Vector2(160,42),()=>SelectSource(0));bagSource.name="BagSource";RusticHudTheme.Skin(bagSource);
            stockView=Button(overlay.transform,"ENTREGAS",new Vector2(370,-555),new Vector2(160,42),()=>SelectSource(2));stockView.name="DeliverySource";RusticHudTheme.Skin(stockView);
            for(int i=0;i<rows.Length;i++)
            {
                var r=(RectTransform)rows[i].transform;r.anchoredPosition=new Vector2(30+i%(touchLayout?2:4)*(touchLayout?256:128),-128-i/(touchLayout?2:4)*125);r.sizeDelta=new Vector2(touchLayout?248:120,120);RusticHudTheme.Skin(rows[i]);
                var text=rows[i].GetComponentInChildren<Text>();var tr=(RectTransform)text.transform;
                tr.sizeDelta=touchLayout?new Vector2(146,76):new Vector2(114,31);tr.anchoredPosition=touchLayout?new Vector2(96,-8):new Vector2(3,-69);text.fontSize=touchLayout?18:13;text.resizeTextForBestFit=true;text.resizeTextMinSize=touchLayout?16:12;text.resizeTextMaxSize=touchLayout?18:13;
                var iconRect=RusticHudTheme.Rect(rows[i].transform,"ItemIcon",new Vector2(0,1),touchLayout?new Vector2(8,-8):new Vector2(27,-3),Vector2.one*(touchLayout?80:66));icons[i]=iconRect.gameObject.AddComponent<Image>();icons[i].preserveAspect=true;icons[i].raycastTarget=false;
                rowBadges[i]=RusticHudTheme.Label(rows[i].transform,"ItemState",new Vector2(0,1),touchLayout?new Vector2(96,-86):new Vector2(3,-101),touchLayout?new Vector2(146,29):new Vector2(114,16),"",touchLayout?13:10);rowBadges[i].alignment=TextAnchor.MiddleCenter;
            }
            string[] categories={"TODOS","ARMAS","SECUND.","ARMADURA","ACESSÓRIO","ARTEFATO"};
            for(int i=0;i<6;i++){int slot=i-1;var tab=categoryTabs[i]=Button(overlay.transform,categories[i],new Vector2(30+i*79,-80),new Vector2(75,34),()=>{category=slot;page=0;selected=null;Refresh();});RusticHudTheme.Skin(tab);tab.GetComponentInChildren<Text>().fontSize=10;}
            classFilter=Button(overlay.transform,"FILTRO DE CLASSE",new Vector2(30,-48),new Vector2(240,26),()=>{classOnly=!classOnly;page=0;selected=null;Refresh();});RusticHudTheme.Skin(classFilter);classFilter.GetComponentInChildren<Text>().fontSize=12;
            profileSource=Button(overlay.transform,"PERFIL",new Vector2(560,-58),new Vector2(640,28),()=>{channel.Hub.Toggle();selected=null;page=0;Refresh();});RusticHudTheme.Skin(profileSource);profileSource.GetComponentInChildren<Text>().fontSize=14;
            heroChoice=Button(overlay.transform,"TROCAR HERÓI",new Vector2(820,-20),new Vector2(240,34),()=>{director.SelectHero((director.SelectedHero+1)%director.HeroCount);selected=null;Refresh();});RusticHudTheme.Skin(heroChoice);heroChoice.GetComponentInChildren<Text>().fontSize=13;
            expeditionButton=Button(overlay.transform,"SAQUE",new Vector2(200,-555),new Vector2(160,42),()=>SelectSource(1));expeditionButton.name="ExpeditionSource";RusticHudTheme.Skin(expeditionButton);
            previous.GetComponent<RectTransform>().anchoredPosition=new Vector2(30,-610);previous.GetComponent<RectTransform>().sizeDelta=new Vector2(120,42);
            next.GetComponent<RectTransform>().anchoredPosition=new Vector2(410,-610);next.GetComponent<RectTransform>().sizeDelta=new Vector2(120,42);
            previous.GetComponentInChildren<Text>().rectTransform.sizeDelta=new Vector2(120,42);next.GetComponentInChildren<Text>().rectTransform.sizeDelta=new Vector2(120,42);
            previous.GetComponentInChildren<Text>().fontSize=14;next.GetComponentInChildren<Text>().fontSize=14;
            pageLabel=RusticHudTheme.Label(overlay.transform,"InventoryPage",new Vector2(0,1),new Vector2(155,-610),new Vector2(250,42),"",15);pageLabel.alignment=TextAnchor.MiddleCenter;
            emptyHint=RusticHudTheme.Label(overlay.transform,"EmptyInventory",new Vector2(0,1),new Vector2(30,-140),new Vector2(505,165),"",21);
            clearFilters=RusticHudTheme.Button(overlay.transform,"ClearInventoryFilters",new Vector2(0,1),new Vector2(30,-315),new Vector2(505,42),"MOSTRAR TODOS OS ITENS",()=>{classOnly=false;category=-1;page=0;selected=null;Refresh();});
            sourceHint=RusticHudTheme.Label(overlay.transform,"InventorySourceHint",new Vector2(0,1),new Vector2(580,-540),new Vector2(600,110),"",17);
            capacity=RusticHudTheme.Label(overlay.transform,"BagCapacity",new Vector2(0,1),new Vector2(285,-48),new Vector2(250,26),"",16);
            claimAll=RusticHudTheme.Button(overlay.transform,"ClaimAllDeliveries",new Vector2(0,1),new Vector2(580,-610),new Vector2(600,44),"RECEBER TODOS",ClaimAll);
            var viewport=RusticHudTheme.Rect(overlay.transform,"ResultScroll",new Vector2(0,1),new Vector2(40,-125),new Vector2(1160,430));viewport.gameObject.AddComponent<RectMask2D>();viewport.gameObject.AddComponent<Image>().color=Color.clear;
            resultScroll=viewport.gameObject.AddComponent<ScrollRect>();resultScroll.viewport=viewport;resultScroll.horizontal=false;resultScroll.movementType=ScrollRect.MovementType.Clamped;resultScroll.scrollSensitivity=32;
            resultText=RusticHudTheme.Label(viewport,"ResultContent",new Vector2(0,1),Vector2.zero,new Vector2(1130,430),"",20);resultContent=(RectTransform)resultText.transform;resultScroll.content=resultContent;resultScroll.gameObject.SetActive(false);
            var previewRect=RusticHudTheme.Rect(overlay.transform,"HeroPreview",new Vector2(0,1),new Vector2(785,-112),new Vector2(185,280));
            var raw=characterPreview=previewRect.gameObject.AddComponent<RawImage>();raw.raycastTarget=false;paperDoll=overlay.AddComponent<EquipmentPaperDoll>();paperDoll.Configure(raw);
            Vector2[] points={new Vector2(580,-130),new Vector2(1005,-130),new Vector2(580,-255),new Vector2(1005,-255),new Vector2(800,-405)};
            for(int i=0;i<5;i++)
            {
                int slot=i;equipmentSlots[i]=Button(overlay.transform,"EquipmentSlot_"+i,points[i],new Vector2(175,100),()=>SelectEquipped(slot));RusticHudTheme.Skin(equipmentSlots[i]);
                var text=equipmentSlots[i].GetComponentInChildren<Text>();((RectTransform)text.transform).sizeDelta=new Vector2(175,22);((RectTransform)text.transform).anchoredPosition=new Vector2(0,-76);text.fontSize=12;text.text="";
                var iconRect=RusticHudTheme.Rect(equipmentSlots[i].transform,"EquippedItemIcon",Vector2.one*.5f,new Vector2(0,8),new Vector2(64,64));equippedIcons[i]=iconRect.gameObject.AddComponent<Image>();equippedIcons[i].preserveAspect=true;equippedIcons[i].raycastTarget=false;
                var silhouette=RusticHudTheme.Rect(equipmentSlots[i].transform,"EmptySlot",Vector2.one*.5f,new Vector2(0,8),new Vector2(50,50));emptySlots[i]=silhouette.gameObject.AddComponent<HudIcon>();emptySlots[i].symbol=i==0?HudSymbol.Sword:i==1?HudSymbol.Shield:i==2?HudSymbol.Armor:i==3?HudSymbol.Accessory:HudSymbol.Artifact;emptySlots[i].color=new Color(.5f,.53f,.47f,.5f);emptySlots[i].raycastTarget=false;
            }
            detailViewport=RusticHudTheme.Rect(overlay.transform,"ItemDetailsScroll",new Vector2(0,1),new Vector2(30,-382),new Vector2(475,165));detailViewport.gameObject.AddComponent<RectMask2D>();detailViewport.gameObject.AddComponent<Image>().color=Color.clear;
            var scroll=detailViewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=detailViewport;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=28;
            detail.transform.SetParent(detailViewport,false);var content=(RectTransform)detail.transform;content.anchoredPosition=Vector2.zero;content.sizeDelta=new Vector2(450,165);detail.fontSize=17;scroll.content=content;
            secondaryEquip=Button(overlay.transform,"USAR NA SECUNDÁRIA",new Vector2(965,-395),new Vector2(240,50),()=>{if(selected!=null)channel.Request(CoopInventoryAction.Equip,selected,EquipmentSlot.OffHand);});RusticHudTheme.Skin(secondaryEquip);secondaryEquip.GetComponentInChildren<Text>().fontSize=13;
            var noticeRect=(RectTransform)notice.transform;noticeRect.anchoredPosition=new Vector2(30,-665);noticeRect.sizeDelta=new Vector2(1180,30);notice.fontSize=14;
            var info=(RectTransform)detail.transform;info.anchoredPosition=new Vector2(30,-382);info.sizeDelta=new Vector2(475,170);detail.fontSize=16;
            foreach(var button in new[]{equip,remove,discard}){var rect=(RectTransform)button.transform;rect.anchoredPosition=new Vector2(rect.anchoredPosition.x,-560);RusticHudTheme.Skin(button);}
        }
        void BuildItemPopup()
        {
            sheet=overlay.AddComponent<CampaignCharacterSheet>();sheet.Build(overlay.transform,director,channel,characterPreview);
            var rect=RusticHudTheme.Rect(overlay.transform,"ItemComparison",Vector2.one*.5f,Vector2.zero,new Vector2(1160,620));itemPopup=rect.gameObject;var surface=RusticHudTheme.Panel(rect,new Color(.065f,.079f,.067f,1),20);surface.raycastTarget=true;
            ownDetails=ScrollableDetails(rect,"SelectedItem",new Vector2(595,-80));otherDetails=ScrollableDetails(rect,"ComparedItem",new Vector2(25,-80));
            var summary=RusticHudTheme.Rect(rect,"ComparisonSummary",new Vector2(0,1),new Vector2(25,-350),new Vector2(1105,125));summary.gameObject.AddComponent<RectMask2D>();summary.gameObject.AddComponent<Image>().color=Color.clear;RusticHudTheme.Panel(summary,new Color(.08f,.13f,.105f,1),14);
            var summaryScroll=summary.gameObject.AddComponent<ScrollRect>();summaryScroll.viewport=summary;summaryScroll.horizontal=false;summaryScroll.movementType=ScrollRect.MovementType.Clamped;
            comparisonSummary=RusticHudTheme.Label(summary,"Changes",new Vector2(0,1),new Vector2(14,-8),new Vector2(1075,115),"",18);comparisonSummary.alignment=TextAnchor.UpperLeft;summaryScroll.content=comparisonSummary.rectTransform;
            RusticHudTheme.Label(rect,"Title",new Vector2(0,1),new Vector2(675,-15),new Vector2(280,55),"SELECIONADO",22);comparisonHeading=RusticHudTheme.Label(rect,"ComparisonTitle",new Vector2(0,1),new Vector2(105,-15),new Vector2(450,55),"EQUIPADO ATUALMENTE",22);
            RusticHudTheme.Button(rect,"CloseItem",new Vector2(1,1),new Vector2(-20,-15),new Vector2(140,40),"VOLTAR",()=>{itemPopup.SetActive(false);selected=null;Refresh();});
            compareButton=RusticHudTheme.Button(rect,"CompareOther",new Vector2(0,1),new Vector2(595,-545),new Vector2(320,40),"OUTRO ITEM DE REFERÊNCIA",CycleCompare);
            resetComparison=RusticHudTheme.Button(rect,"CompareEquipped",new Vector2(0,1),new Vector2(925,-545),new Vector2(205,40),"USAR EQUIPADO",CompareEquipped);
            advancedButton=RusticHudTheme.Button(rect,"ToggleAdvanced",new Vector2(0,1),new Vector2(25,-545),new Vector2(535,40),"VER DETALHES AVANÇADOS",()=>{advancedDetails=!advancedDetails;Refresh();ResetInspectionScroll();});
            compareButton.gameObject.SetActive(false);resetComparison.gameObject.SetActive(false);
            RusticHudTheme.Label(rect,"ComparisonLegend",new Vector2(0,1),new Vector2(595,-545),new Vector2(535,40),"▲ Melhora    ▼ Piora    — Sem alteração",17);
            compareButton.GetComponentInChildren<Text>().fontSize=14;resetComparison.GetComponentInChildren<Text>().fontSize=14;advancedButton.GetComponentInChildren<Text>().fontSize=16;
            var icon=RusticHudTheme.Rect(rect,"InspectedIcon",new Vector2(0,1),new Vector2(595,-10),new Vector2(68,68));inspectedIcon=icon.gameObject.AddComponent<Image>();inspectedIcon.preserveAspect=true;inspectedIcon.raycastTarget=false;var otherIcon=RusticHudTheme.Rect(rect,"ComparedIcon",new Vector2(0,1),new Vector2(25,-10),new Vector2(68,68));comparedIcon=otherIcon.gameObject.AddComponent<Image>();comparedIcon.preserveAspect=true;comparedIcon.raycastTarget=false;
            itemNotice=RusticHudTheme.Label(rect,"EquipmentNotice",new Vector2(0,1),new Vector2(25,-590),new Vector2(1100,26),"",17);
            visualComparison=new InventoryComparisonView();visualComparison.Build(rect,touchLayout);
            var back=rect.Find("CloseItem").GetComponent<RectTransform>();back.anchoredPosition=new Vector2(-20,-545);back.SetAsLastSibling();
            var legend=rect.Find("ComparisonLegend").GetComponent<RectTransform>();legend.sizeDelta=new Vector2(350,40);legend.GetComponent<Text>().fontSize=14;
            itemPopup.SetActive(false);
        }
        Text ScrollableDetails(Transform parent,string name,Vector2 pos)
        {
            var viewport=RusticHudTheme.Rect(parent,name,new Vector2(0,1),pos,new Vector2(535,260));viewport.gameObject.AddComponent<RectMask2D>();viewport.gameObject.AddComponent<Image>().color=Color.clear;RusticHudTheme.Panel(viewport,RusticHudTheme.Surface,14);
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=28;
            var text=RusticHudTheme.Label(viewport,"Content",new Vector2(0,1),new Vector2(12,-10),new Vector2(505,400),"",19);text.alignment=TextAnchor.UpperLeft;scroll.content=text.rectTransform;return text;
        }
        void OpenItem()
        {
            if(itemPopup==null||selected==null)return;advancedDetails=false;selectionMessage=channel.Message;selectionRevision=channel.Revision;compared=null;var p=channel.LocalProfile;var item=p.Item(selected)??CurrentItems.Find(x=>x.instanceId==selected);if(item==null)return;
            var set=p.Loadout(ActiveActor.Roster.Current.heroId);compared=set?.items[(int)channel.Catalog.Find(item.definitionId).slot];if(compared==selected)compared=null;
            visualComparison?.Reset();itemPopup.SetActive(true);itemPopup.transform.SetAsLastSibling();Refresh();ResetInspectionScroll();
        }
        void CycleCompare() => CompareEquipped();
        void SelectEquipped(int slot)
        {
            deliveries=expeditionView=false;var set=channel.LocalProfile?.Loadout(ActiveActor.Roster.Current.heroId);
            selected=set?.items[slot];pendingDiscard=null;
            if(string.IsNullOrEmpty(selected)){category=slot;classOnly=true;page=0;channel.SetNotice(RusticHudTheme.SlotName((EquipmentSlot)slot)+" vazio. Selecione um item compatível na mochila.");}
            Refresh();OpenItem();
        }
        public bool IsOpen=>overlay!=null&&overlay.activeSelf;
        public void OpenInventory()=>Show(false);
        public void CloseInventory(){if(IsOpen)Close();}
        void Show(bool result)
        {
            if (channel.LocalProfile == null || director.LocalSlot < 0&&!channel.HomeMode) return;
            GetComponent<CampaignSessionMenu>()?.Close();
            sheet?.Hide();if(itemPopup!=null)itemPopup.SetActive(false);results = result; deliveries=false; expeditionView=false;selected = pendingDiscard = pendingClaim = null; page = 0; overlay.SetActive(true); overlay.transform.SetAsLastSibling(); director.SetInputBlocked(true); Refresh();
        }
        Button TopRight(Transform parent,string label,float y,UnityEngine.Events.UnityAction action)
        {
            var button=Button(parent,label,Vector2.zero,new Vector2(190,44),action);var rect=(RectTransform)button.transform;
            if(director.Campaign!=null){rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.zero;rect.anchoredPosition=new Vector2(30,label=="INVENTÁRIO"?20:75);RusticHudTheme.Skin(button);}
            else{rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one;rect.anchoredPosition=new Vector2(-15,y);}return button;
        }
        void Close() { sheet?.Hide();if(itemPopup!=null)itemPopup.SetActive(false);overlay.SetActive(false); pendingDiscard = pendingClaim = null; director.SetInputBlocked(false); }
        void Update()
        {
            if(inventoryButton!=null)
            {
                inventoryButton.interactable=director.LocalSlot>=0||channel.HomeMode;
                if(director.Campaign!=null){var campaignHud=GetComponent<CampaignHud>();inventoryButton.gameObject.SetActive(false);}
                if(director.Campaign!=null)resultButton.gameObject.SetActive(director.Phase==CoopPhase.Victory||director.Phase==CoopPhase.Defeat);
            }
            if (overlay == null || !overlay.activeSelf) return;
            if (director.LocalSlot < 0&&!channel.HomeMode) { Close(); return; }
            if(pendingClaim!=null)
            {
                if(channel.LocalProfile?.Item(pendingClaim)!=null)
                {string received=pendingClaim;SelectSource(0);selected=received;Refresh();OpenItem();}
                else if(Time.unscaledTime>claimUntil){pendingClaim=null;Refresh();}
            }
            if(bulkPending&&(channel.Revision!=bulkRevision||channel.Message!=bulkMessage||Time.unscaledTime>bulkUntil))
            {bulkPending=false;Refresh();}
            RefreshEquipmentNotice();
            if (revision != channel.Revision||lastNotice!=channel.Message||management!=CanManageNow||lastHero!=ActiveActor.Roster.Selected) Refresh();
        }
        void Select(int row)
        { var items = CurrentItems; int index = page * PageSize + row; if (index >= items.Count) return; selected = items[index].instanceId; pendingDiscard = null; Refresh();OpenItem(); }
        void Refresh()
        {
            var profile = channel.LocalProfile; if (profile == null) return; lastNotice=channel.Message;revision = channel.Revision;lastHero=ActiveActor.Roster.Selected;management=CanManageNow;FilterItems();
            if(profileSource!=null){profileSource.gameObject.SetActive(channel.HomeMode&&!results);profileSource.GetComponentInChildren<Text>().text=channel.Hub.Description;profileSource.interactable=channel.Hub.HasRemote;}
            if(classFilter!=null){classFilter.GetComponentInChildren<Text>().text=classOnly?"CLASSE: "+RusticHudTheme.ClassNames(channel.Catalog.Hero(ActiveActor.Roster.Current.heroId).affinity).ToUpperInvariant():"TODAS AS CLASSES";classFilter.gameObject.SetActive(!results);heroChoice.gameObject.SetActive(!results);heroChoice.interactable=channel.HomeMode||director.Phase==CoopPhase.Lobby;heroChoice.GetComponentInChildren<Text>().text="HERÓI · "+ActiveActor.Roster.Current.displayName;for(int i=0;i<categoryTabs.Length;i++){categoryTabs[i].gameObject.SetActive(!results);categoryTabs[i].interactable=category!=i-1;}}
            if(CurrentItems!=null)page=Mathf.Clamp(page,0,Mathf.Max(0,(CurrentItems.Count-1)/PageSize));
            RefreshNavigation(profile);
            header.text = results ? "RESULTADO DA PARTIDA" : "PERSONAGEM · NV "+ProgressionStats.Level(profile.experience)+" · "+profile.coins+" OURO";
            for (int i = 0; i < rows.Length; i++)
            {
                int index = page * PageSize + i; bool visible = !results && i<PageSize && index < CurrentItems.Count; rows[i].gameObject.SetActive(visible);
                if (visible)
                {
                    var item=CurrentItems[index];var definition=channel.Catalog.Find(item.definitionId);var label=rows[i].GetComponentInChildren<Text>();
                    label.text=definition.displayName;label.color=channel.Catalog.Rarity(item.rarity).color;
                    var surface=rows[i].GetComponentInChildren<RoundedHudGraphic>();if(surface!=null){surface.borderColor=item.instanceId==selected?RusticHudTheme.Gold:Color.Lerp(RusticHudTheme.Copper,channel.Catalog.Rarity(item.rarity).color,.65f);surface.border=item.instanceId==selected?3:1.5f;surface.SetVerticesDirty();}
                    if(icons[i]!=null){icons[i].sprite=definition.icon;icons[i].enabled=icons[i].sprite!=null;}
                    if(rowBadges[i]!=null)
                    {
                        bool compatible=(definition.compatibleHeroes&channel.Catalog.Hero(ActiveActor.Roster.Current.heroId).affinity)!=0;
                        bool level=ProgressionStats.Level(profile.experience,channel.Catalog.maximumLevel)>=definition.minimumLevel;
                        rowBadges[i].text=!compatible?"OUTRA CLASSE":!level?"REQUER NV "+definition.minimumLevel:RusticHudTheme.RarityName(item.rarity)+" · "+ShortSlot(definition.slot);
                        rowBadges[i].color=compatible&&level?RusticHudTheme.Muted:new Color(1,.58f,.42f);
                        icons[i].color=compatible&&level?Color.white:new Color(.6f,.6f,.6f,1);
                    }
                }
            }
            previous.gameObject.SetActive(!results);next.gameObject.SetActive(!results);previous.interactable=page>0;next.interactable=CurrentItems!=null&&(page+1)*PageSize<CurrentItems.Count;
            equip.gameObject.SetActive(!results); remove.gameObject.SetActive(!results); discard.gameObject.SetActive(!results);
            var actor = ActiveActor; var value = deliveries?CurrentItems.Find(item=>item.instanceId==selected):profile.Item(selected);
            if (results)
            {
                var result = profile.lastResult;
                detail.text = result != null ? result.outcome + "\nXP " + result.experience + " | moedas " + result.coins + "\nItens obtidos: " + result.items.Count : "Nenhuma partida concluída nesta sessão.";
                if (result != null) foreach (var item in result.items) detail.text += "\n" + channel.Catalog.Find(item.definitionId).displayName + " — " + RusticHudTheme.RarityName(item.rarity);
            }
            else if(value==null)detail.text="Selecione um item para ver classe, slot, bônus e comparação.";
            else detail.text=ItemDescriptions.Describe(channel.Catalog,value)+"\n\n"+ItemDescriptions.Preview(actor.Roster.Current,profile,channel.Catalog,value);
            if(resultScroll!=null)
            {
                resultScroll.gameObject.SetActive(results);detail.gameObject.SetActive(!results);
                if(results){resultText.text=detail.text+"\n\nTodos os itens recebidos estão na mochila ou no estoque de entregas.";resultContent.sizeDelta=new Vector2(1130,Mathf.Max(430,resultText.preferredHeight+24));}
            }
            if(deliveries&&value!=null)detail.text=channel.Catalog.Find(value.definitionId).displayName+"\n"+RusticHudTheme.RarityName(value.rarity)+"\nGuardado no estoque. Receba na mochila antes de equipar.\nEspaços livres: "+(channel.Catalog.inventoryCapacity-InventoryRules.BagCount(profile));

            equip.GetComponentInChildren<Text>().text=deliveries?"RECEBER ITEM":"EQUIPAR";
            if(paperDoll!=null)
            {
                var detailRect=(RectTransform)detail.transform;detailRect.anchoredPosition=Vector2.zero;detailRect.sizeDelta=new Vector2(450,Mathf.Max(165,detail.preferredHeight+20));detailViewport.gameObject.SetActive(!results);
                paperDoll.SetVisible(!results);paperDoll.Refresh(actor.Presentation,profile,actor.Roster.Current.heroId);
                string[] names={"ARMA PRINCIPAL","SECUNDÁRIA","ARMADURA","ACESSÓRIO","ARTEFATO"};var set=profile.Loadout(actor.Roster.Current.heroId);
                for(int i=0;i<5;i++)
                {
                    equipmentSlots[i].gameObject.SetActive(!results);var equipped=set!=null?profile.Item(set.items[i]):null;
                    var definition=equipped!=null?channel.Catalog.Find(equipped.definitionId):null;equippedIcons[i].sprite=definition!=null?definition.icon:null;equippedIcons[i].enabled=equippedIcons[i].sprite!=null;emptySlots[i].gameObject.SetActive(!equippedIcons[i].enabled);equipmentSlots[i].GetComponentInChildren<Text>().text=names[i];equipmentSlots[i].GetComponent<Image>().color=equipped!=null?Color.Lerp(RusticHudTheme.Surface,channel.Catalog.Rarity(equipped.rarity).color,.18f):RusticHudTheme.Surface;
                }
                if(!results&&!deliveries&&!expeditionView&&InventoryRules.BagCount(profile)==0)detail.text+="Sua mochila está vazia. Vença inimigos, abra baús e veja SAQUE ATUAL para equipar durante esta tentativa.\nSaque desta tentativa: "+(profile.expedition?.items.Count??0)+" itens. Eles são guardados no resultado.";
            }
            if(secondaryEquip!=null)
            {
                bool dagger=value!=null&&channel.Catalog.Find(value.definitionId).category==WeaponCategory.Daggers;
                secondaryEquip.gameObject.SetActive(!results&&!deliveries&&dagger);bool allowed=ProgressionStats.CanEquip(profile,channel.Catalog,actor.Roster.Current.heroId,value,EquipmentSlot.OffHand,out string reason);secondaryEquip.interactable=management&&dagger&&allowed;
                if(dagger)detail.text+="\nMão secundária: "+(allowed?"disponível; exige outra instância da adaga":reason);
            }
            if(detailViewport!=null)detail.rectTransform.sizeDelta=new Vector2(450,Mathf.Max(165,detail.preferredHeight+20));
            bool canEquip=value!=null&&ProgressionStats.CanEquip(profile,channel.Catalog,actor.Roster.Current.heroId,value,channel.Catalog.Find(value.definitionId).slot,out _);
            equip.interactable=!results&&management&&(deliveries?value!=null&&InventoryRules.BagCount(profile)<channel.Catalog.inventoryCapacity:canEquip);remove.interactable=!results&&!deliveries&&management&&value!=null;discard.interactable=remove.interactable&&!(channel.HomeMode&&channel.Hub.UsingRemote);if(channel.HomeMode&&channel.Hub.UsingRemote&&deliveries)equip.interactable=false;
            discard.GetComponentInChildren<Text>().text = pendingDiscard == selected && selected != null ? "CONFIRMAR RECICLAGEM" : "RECICLAR · "+InventoryRules.SellValue(channel.Catalog,value)+" OURO";
            if(itemPopup!=null)
            {
                sheet.SetOverviewVisible(!results);detailViewport.gameObject.SetActive(false);
                bool open=itemPopup.activeSelf&&!results&&value!=null;if(itemPopup.activeSelf&&!open)itemPopup.SetActive(false);
                foreach(var button in new[]{equip,remove,discard}){button.gameObject.SetActive(open);button.transform.SetParent(itemPopup.transform,false);}
                equip.GetComponent<RectTransform>().anchoredPosition=new Vector2(25,-490);remove.GetComponent<RectTransform>().anchoredPosition=new Vector2(225,-490);discard.GetComponent<RectTransform>().anchoredPosition=new Vector2(425,-490);discard.GetComponent<RectTransform>().sizeDelta=new Vector2(310,44);discard.GetComponentInChildren<Text>().rectTransform.sizeDelta=new Vector2(310,44);
                if(secondaryEquip!=null){secondaryEquip.transform.SetParent(itemPopup.transform,false);secondaryEquip.GetComponent<RectTransform>().anchoredPosition=new Vector2(760,-490);secondaryEquip.gameObject.SetActive(open&&!deliveries&&channel.Catalog.Find(value.definitionId).category==WeaponCategory.Daggers);}
                remove.interactable=management&&!deliveries&&value!=null&&EquippedHere(profile,selected);if(!deliveries&&InventoryRules.Equipped(profile,selected))equip.interactable=false;
                discard.interactable=management&&!deliveries&&value!=null&&!InventoryRules.Equipped(profile,selected)&&!(channel.HomeMode&&channel.Hub.UsingRemote);
                if(open)
                {
                    RefreshInspection(profile,actor,value);
                    bool equippedHere=!deliveries&&EquippedHere(profile,value.instanceId);
                    equip.gameObject.SetActive(!equippedHere);remove.gameObject.SetActive(equippedHere);discard.gameObject.SetActive(!equippedHere&&!deliveries);
                    var primary=equippedHere?remove:equip;primary.GetComponent<RectTransform>().anchoredPosition=new Vector2(25,-490);primary.GetComponent<RectTransform>().sizeDelta=new Vector2(320,48);primary.GetComponentInChildren<Text>().rectTransform.sizeDelta=new Vector2(320,48);
                    discard.GetComponent<RectTransform>().anchoredPosition=new Vector2(365,-490);discard.GetComponent<RectTransform>().sizeDelta=new Vector2(330,48);discard.GetComponentInChildren<Text>().rectTransform.sizeDelta=new Vector2(330,48);
                    var surface=primary.GetComponentInChildren<RoundedHudGraphic>();if(surface!=null){surface.color=new Color(.33f,.24f,.1f,1);surface.borderColor=RusticHudTheme.Gold;surface.border=2;surface.SetVerticesDirty();}
                }

            }
            RefreshEquipmentNotice();
            notice.text = channel.Message ?? "Organize por slot e compare os atributos. Em partida, equipe fora de perigo. O perfil da sala é guardado pelo host; o perfil inicial é deste aparelho.";
        }
        void RefreshEquipmentNotice()
        {
            if(notice!=null)notice.gameObject.SetActive(sheet==null||!sheet.IsOpen);
            if(itemNotice==null||!itemPopup.activeSelf)return;
            string message=itemContextNotice;bool blocked=itemContextBlocked;
            if(!channel.HomeMode&&director.Manager.IsServer&&!channel.CanManageLocal){message=channel.LocalManagementBlockReason;blocked=true;}
            else if(!itemContextBlocked&&pendingDiscard!=selected&&(channel.Message!=selectionMessage||channel.Revision!=selectionRevision))
            {if(!string.IsNullOrEmpty(channel.Message))message=channel.Message;}
            itemNotice.text=message;itemNotice.color=pendingDiscard==selected&&selected!=null?RusticHudTheme.Gold:blocked?new Color(1,.6f,.45f):RusticHudTheme.Parchment;
        }
        void ClaimAll()
        {
            if(bulkPending||pendingClaim!=null)return;
            bulkPending=true;bulkRevision=channel.Revision;bulkMessage=channel.Message;bulkUntil=Time.unscaledTime+8;
            channel.Request(CoopInventoryAction.ClaimAllDeliveries,null,EquipmentSlot.MainHand);
            Refresh();
        }
        void Equip()
        {
            if(deliveries)
            {
                if(selected!=null&&pendingClaim==null){pendingClaim=selected;claimUntil=Time.unscaledTime+5;channel.Request(CoopInventoryAction.ClaimDelivery,selected,EquipmentSlot.MainHand);}
                return;
            }
            var item=channel.LocalProfile?.Item(selected);if(item==null)return;
            pendingDiscard=null;channel.Request(CoopInventoryAction.Equip,selected,channel.Catalog.Find(item.definitionId).slot);
        }
        void ResetInspectionScroll()
        {
            foreach(var text in new[]{ownDetails,otherDetails})if(text!=null)text.rectTransform.anchoredPosition=new Vector2(12,-10);
            if(comparisonSummary!=null)comparisonSummary.rectTransform.anchoredPosition=new Vector2(14,-8);
        }
        bool EquippedHere(PlayerProfile profile,string id)=>InventoryRules.EquippedBy(profile,ActiveActor.Roster.Current.heroId,id);
        static string ShortSlot(EquipmentSlot slot)=>slot==EquipmentSlot.MainHand?"ARMA":slot==EquipmentSlot.OffHand?"SECUND.":slot==EquipmentSlot.Armor?"ARMADURA":slot==EquipmentSlot.Accessory?"ACESSÓRIO":"ARTEFATO";
        int CompareInventoryItems(ItemInstance a,ItemInstance b)
        {
            var x=channel.Catalog.Find(a.definitionId);var y=channel.Catalog.Find(b.definitionId);
            int order=x.slot.CompareTo(y.slot);if(order!=0)return order;
            order=b.rarity.CompareTo(a.rarity);if(order!=0)return order;
            order=string.CompareOrdinal(x.displayName,y.displayName);return order!=0?order:string.CompareOrdinal(a.instanceId,b.instanceId);
        }
        int AvailableItems(System.Collections.Generic.List<ItemInstance> items,bool includeEquipped=false)
        {if(items==null)return 0;int count=0;foreach(var item in items)if(includeEquipped||!InventoryRules.Equipped(channel.LocalProfile,item.instanceId))count++;return count;}
        void SelectSource(int source)
        {
            deliveries=source==2;expeditionView=source==1;selected=pendingDiscard=pendingClaim=null;page=0;
            if(itemPopup!=null)itemPopup.SetActive(false);Refresh();
        }
        void CompareEquipped()
        {
            var item=channel.LocalProfile?.Item(selected)??CurrentItems.Find(x=>x.instanceId==selected);if(item==null)return;
            compared=channel.LocalProfile.Loadout(ActiveActor.Roster.Current.heroId)?.items[(int)channel.Catalog.Find(item.definitionId).slot];
            if(compared==selected)compared=null;Refresh();
        }
        void RefreshNavigation(PlayerProfile profile)
        {
            if(bagSource==null)return;
            bagSource.gameObject.SetActive(!results);stockView.gameObject.SetActive(!results);expeditionButton.gameObject.SetActive(!results);
            bagSource.interactable=deliveries||expeditionView;stockView.interactable=!deliveries;
            expeditionButton.interactable=!expeditionView&&profile.expedition!=null;
            int used=InventoryRules.BagCount(profile),limit=channel.Catalog.inventoryCapacity,free=Mathf.Max(0,limit-used);
            bagSource.GetComponentInChildren<Text>().text="MOCHILA · "+used+"/"+limit;
            capacity.gameObject.SetActive(!results);capacity.text="Mochila: "+used+"/"+limit+(free==0?" · CHEIA":"");capacity.color=free==0?new Color(1,.65f,.4f):RusticHudTheme.Parchment;
            claimAll.gameObject.SetActive(!results&&deliveries);claimAll.interactable=management&&!bulkPending&&pendingClaim==null&&free>0&&channel.DeliveryCount>0&&!(channel.HomeMode&&channel.Hub.UsingRemote);
            claimAll.GetComponentInChildren<Text>().text=bulkPending?"RECEBENDO...":"RECEBER TODOS ("+Mathf.Min(free,channel.DeliveryCount)+")";
            stockView.GetComponentInChildren<Text>().text="ENTREGAS · "+channel.DeliveryCount;
            expeditionButton.GetComponentInChildren<Text>().text="SAQUE · "+AvailableItems(profile.expedition?.items);
            foreach(var button in new[]{bagSource,stockView,expeditionButton})button.GetComponentInChildren<Text>().fontSize=13;
            bool empty=!results&&CurrentItems.Count==0;emptyHint.gameObject.SetActive(empty);
            bool filters=category!=-1||classOnly;int available=AvailableItems(SourceItems,deliveries);
            emptyHint.text=available>0?"Nenhum item corresponde aos filtros.\nTente outra categoria ou mostre todas as classes.":deliveries?"Nenhuma entrega aguardando.\nItens que não couberem na mochila ficam guardados aqui.":expeditionView?"Nenhum item solto no saque.\nDerrote inimigos e abra baús; os equipados ficam no herói.":"Sua mochila está vazia.\nBusque equipamentos nos inimigos e baús. Os equipados ficam ao redor do herói.";
            clearFilters.gameObject.SetActive(empty&&available>0&&filters);
            pageLabel.gameObject.SetActive(!results);pageLabel.text="Página "+(page+1)+" / "+Mathf.Max(1,(CurrentItems.Count+PageSize-1)/PageSize)+" · "+CurrentItems.Count+" itens";
            sourceHint.gameObject.SetActive(!results);
            sourceHint.text=deliveries?"ENTREGAS\nItens guardados por falta de espaço. Receba um item para levá-lo à mochila; depois escolha se deseja equipar.":expeditionView?"SAQUE DA PARTIDA\nItens obtidos nesta expedição. Você pode equipar fora de perigo. Ao terminar, eles são guardados no resultado.":"MOCHILA\nItens guardados neste perfil. Clique em um item para inspecionar; os equipados aparecem nos slots ao redor do herói.";
            if(deliveries)
            {
                sourceHint.rectTransform.sizeDelta=new Vector2(600,65);
                sourceHint.text="Entregas: "+channel.DeliveryCount+" · Vagas livres na mochila: "+free+".\nReceber todos inclui todas as categorias; o que não couber fica guardado.";
                if(channel.HomeMode&&channel.Hub.UsingRemote)sourceHint.text="ENTREGAS DA ÚLTIMA SALA\nVolte à sala de origem para receber estes itens.";
            }
            else
            {
                sourceHint.rectTransform.sizeDelta=new Vector2(600,110);
                if(!expeditionView)sourceHint.text="MOCHILA · "+used+"/"+limit+"\nLimite compartilhado entre todas as categorias. Equipados não ocupam espaço.\nSelecione uma peça para comparar com o equipamento atual.";
            }
        }
        void RefreshInspection(PlayerProfile profile,CoopActor actor,ItemInstance value)
        {
            var definition=channel.Catalog.Find(value.definitionId);
            // Resolve again on every profile refresh: a previous selection must never
            // become the reference after equipment, hero or source changes.
            var set=profile.Loadout(actor.Roster.Current.heroId);
            int slot=(int)definition.slot;
            if(set!=null)for(int i=0;i<set.items.Length;i++)if(set.items[i]==value.instanceId){slot=i;break;}
            compared=set!=null&&slot<set.items.Length?set.items[slot]:null;
            var other=profile.Item(compared);
            inspectedIcon.sprite=definition.icon;inspectedIcon.enabled=inspectedIcon.sprite!=null;
            comparedIcon.sprite=other!=null?channel.Catalog.Find(other.definitionId)?.icon:null;comparedIcon.enabled=comparedIcon.sprite!=null;
            comparisonHeading.text=other==null?"SLOT VAZIO":other.instanceId==value.instanceId?"JÁ EQUIPADO":"EQUIPADO ATUALMENTE";
            string state=deliveries?"Guardado nas entregas; receba na mochila para equipar.":ItemDescriptions.EquipmentState(profile,channel.Catalog,actor.Roster.Current,value);
            ownDetails.text=state+"\n\n"+(advancedDetails?ItemDescriptions.Describe(channel.Catalog,value):ItemDescriptions.Summary(channel.Catalog,value));
            otherDetails.text=other==null?"<b>"+RusticHudTheme.SlotName((EquipmentSlot)slot)+"</b>\n\nNenhum item equipado neste slot.\n\nA comparação usa somente o equipamento deste herói; outros itens da mochila não são usados como referência.":advancedDetails?ItemDescriptions.Describe(channel.Catalog,other):ItemDescriptions.Summary(channel.Catalog,other);
            comparisonSummary.text="<b>AO EQUIPAR · ALTERAÇÕES NA FICHA DO HERÓI</b>\n"+(deliveries?"Prévia informativa. Receber leva à mochila; não equipa automaticamente.\n":"")+(other!=null&&other.instanceId==value.instanceId?"Este item já está equipado. Não há troca a comparar.":advancedDetails?ItemDescriptions.Preview(actor.Roster.Current,profile,channel.Catalog,value):ItemDescriptions.QuickPreview(actor.Roster.Current,profile,channel.Catalog,value));
            comparisonSummary.rectTransform.sizeDelta=new Vector2(1075,Mathf.Max(115,comparisonSummary.preferredHeight+20));
            advancedButton.GetComponentInChildren<Text>().text=advancedDetails?"VOLTAR AO RESUMO":"VER DETALHES AVANÇADOS";
            foreach(var text in new[]{ownDetails,otherDetails})text.rectTransform.sizeDelta=new Vector2(505,Mathf.Max(260,text.preferredHeight+30));
            compareButton.interactable=false;resetComparison.interactable=false;
            visualComparison.SetVisible(!advancedDetails);
            ownDetails.transform.parent.gameObject.SetActive(advancedDetails);otherDetails.transform.parent.gameObject.SetActive(advancedDetails);comparisonSummary.transform.parent.gameObject.SetActive(advancedDetails);
            inspectedIcon.gameObject.SetActive(advancedDetails);comparedIcon.gameObject.SetActive(advancedDetails);comparisonHeading.gameObject.SetActive(advancedDetails);itemPopup.transform.Find("Title").gameObject.SetActive(advancedDetails);
            visualComparison.Refresh(channel.Catalog,profile,actor.Roster.Current,value,other,(EquipmentSlot)slot,deliveries);
            itemContextBlocked=false;
            if(pendingDiscard==selected&&selected!=null){itemContextNotice="Reciclar este item remove a peça e devolve "+InventoryRules.SellValue(channel.Catalog,value)+" ouro. Clique em CONFIRMAR RECICLAGEM.";return;}
            if(deliveries)
            {
                bool full=InventoryRules.BagCount(profile)>=channel.Catalog.inventoryCapacity;
                itemContextBlocked=full||channel.HomeMode&&channel.Hub.UsingRemote;
                itemContextNotice=full?"Mochila cheia. Recicle ou equipe uma peça para liberar espaço.":channel.HomeMode&&channel.Hub.UsingRemote?"Receba este item na sala de origem.":"Clique em RECEBER ITEM. Ele será levado à mochila, sem equipar automaticamente.";
                if(pendingClaim!=null){equip.interactable=false;equip.GetComponentInChildren<Text>().text="RECEBENDO...";}
            }
            else
            {
                bool equipped=EquippedHere(profile,value.instanceId);
                itemContextBlocked=!equipped&&!ProgressionStats.CanEquip(profile,channel.Catalog,actor.Roster.Current.heroId,value,definition.slot,out _);
                itemContextNotice=itemContextBlocked?state:equipped?"Este item já está equipado. Use DESEQUIPAR para devolvê-lo à mochila.":"Clique em EQUIPAR para usar este item. A comparação mostra as mudanças na sua ficha.";
            }
        }
        void Unequip()
        { var set = channel.LocalProfile?.Loadout(ActiveActor.Roster.Current.heroId); if (set == null) return; for (int i = 0; i < set.items.Length; i++) if (set.items[i] == selected) { channel.Request(CoopInventoryAction.Unequip, null, (EquipmentSlot)i); break; } }
        void Discard()
        { if (selected == null) return; if (pendingDiscard != selected) { pendingDiscard = selected; Refresh(); return; } channel.Request(CoopInventoryAction.Discard, selected, EquipmentSlot.MainHand, true); pendingDiscard = null; }
        Text Label(Transform parent, string value, Vector2 position, Vector2 size, int scale = 19)
        { var go = new GameObject("Text", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = size; var text = go.GetComponent<Text>(); text.font = font; text.fontSize = scale; text.text = value; text.color = Color.white; text.raycastTarget = false; return text; }
        Button Button(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        { var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = size; go.GetComponent<Image>().color = new Color(.13f, .3f, .4f); var button = go.GetComponent<Button>(); button.onClick.AddListener(action); var text = Label(go.transform, label, Vector2.zero, size); text.alignment = TextAnchor.MiddleCenter; return button; }
        void OnDisable() { if (director != null) director.SetInputBlocked(false); }
    }
}
