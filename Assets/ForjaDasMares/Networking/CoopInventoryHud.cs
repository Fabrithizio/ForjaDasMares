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
        ScrollRect resultScroll;Text resultText;RectTransform resultContent;
        RectTransform detailViewport;
        Button inventoryButton,resultButton,stockView,previous,next,closeButton;bool deliveries,expeditionView;Button expeditionButton;readonly Image[] icons=new Image[8];
        System.Collections.Generic.List<ItemInstance> SourceItems=>expeditionView?channel.LocalProfile?.expedition?.items??emptyItems:deliveries?channel.LocalProfile?.deliveries:channel.LocalProfile?.inventory;EquipmentPaperDoll paperDoll;readonly Button[] equipmentSlots=new Button[5];
        static readonly System.Collections.Generic.List<ItemInstance> emptyItems=new System.Collections.Generic.List<ItemInstance>();
        Button classFilter,heroChoice,profileSource,secondaryEquip; readonly Button[] categoryTabs=new Button[6];
        bool management;int category=-1;bool classOnly=true;
        CoopActor ActiveActor=>director.Actor(director.LocalSlot>=0?director.LocalSlot:0);
        readonly System.Collections.Generic.List<ItemInstance> filtered=new System.Collections.Generic.List<ItemInstance>();
        System.Collections.Generic.List<ItemInstance> CurrentItems=>filtered;
        void FilterItems()
        {
            filtered.Clear();var source=SourceItems;if(source==null)return;
            var hero=channel.Catalog.Hero(ActiveActor.Roster.Current.heroId);
            foreach(var item in source){var def=channel.Catalog.Find(item.definitionId);if(def==null||category>=0&&(int)def.slot!=category&&!(category==1&&def.category==WeaponCategory.Daggers&&hero!=null&&hero.allowSecondaryWeapon)||classOnly&&hero!=null&&(def.compatibleHeroes&hero.affinity)==0)continue;filtered.Add(item);}
        }
        bool CanManageNow=>channel.HomeMode||director.LocalSlot>=0&&ActiveActor.Health.IsAlive&&director.Phase!=CoopPhase.Loading&&(director.EncounterStatus!=EncounterState.Spawning&&director.EncounterStatus!=EncounterState.Fighting||director.Dungeon.OpenLandscape&&!director.Dungeon.Encounter.Enemies.ThreatNear(ActiveActor.transform.position,12));
        int page, revision = -1,lastHero=-1; string selected, pendingDiscard; bool results;
        public void Configure(CoopDirector session) => director = session;
        void Start()
        {
            channel = director.GetComponent<CoopProfileChannel>(); font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var parent = GetComponent<CoopHud>().SafeRoot;
            inventoryButton=TopRight(parent, "INVENTÁRIO", -119, () => Show(false));
            resultButton=TopRight(parent, "RESULTADO", -171, () => Show(true));
            overlay = new GameObject("CoopInventory", typeof(RectTransform), typeof(Image)); overlay.transform.SetParent(parent, false);
            var full = (RectTransform)overlay.transform; full.anchorMin = Vector2.zero; full.anchorMax = Vector2.one; full.offsetMin = full.offsetMax = Vector2.zero; overlay.GetComponent<Image>().color = new Color(.025f, .045f, .065f, .98f);
            header = Label(overlay.transform, "", new Vector2(30, -20), new Vector2(1000, 50), 23);
            closeButton=Button(overlay.transform, "FECHAR", new Vector2(1080, -20), new Vector2(150, 44), Close);
            for (int i = 0; i < rows.Length; i++)
            { int row = i; rows[i] = Button(overlay.transform, "", new Vector2(30, -95 - i * 57), new Vector2(560, 50), () => Select(row)); }
            previous=Button(overlay.transform, "ANTERIOR", new Vector2(30, -560), new Vector2(170, 42), () => { page = Mathf.Max(0, page - 1); selected = null; Refresh(); });
            next=Button(overlay.transform, "PRÓXIMA", new Vector2(215, -560), new Vector2(170, 42), () => { var items = CurrentItems; if (items != null && (page + 1) * 8 < items.Count) page++; selected = null; Refresh(); });
            detail = Label(overlay.transform, "", new Vector2(620, -95), new Vector2(600, 415), 19);
            equip = Button(overlay.transform, "EQUIPAR", new Vector2(620, -535), new Vector2(185, 48), Equip);
            remove = Button(overlay.transform, "DESEQUIPAR", new Vector2(825, -535), new Vector2(185, 48), Unequip);
            discard = Button(overlay.transform, "DESCARTAR", new Vector2(1030, -535), new Vector2(185, 48), Discard);
            notice = Label(overlay.transform, "", new Vector2(30, -620), new Vector2(1180, 75), 18);
            if(director.Campaign!=null)BuildPaperDoll();
            overlay.SetActive(false);
        }
        void BuildPaperDoll()
        {
            var panel=(RectTransform)overlay.transform;panel.anchorMin=panel.anchorMax=panel.pivot=Vector2.one*.5f;panel.anchoredPosition=Vector2.zero;panel.sizeDelta=new Vector2(1280,700);
            overlay.GetComponent<Image>().color=RusticHudTheme.Ink;RusticHudTheme.Skin(closeButton);RusticHudTheme.Skin(previous);RusticHudTheme.Skin(next);RusticHudTheme.Panel(panel,RusticHudTheme.Ink,22);
            stockView=Button(overlay.transform,"ENTREGAS",new Vector2(400,-560),new Vector2(170,42),()=>{deliveries=!deliveries;expeditionView=false;selected=null;page=0;Refresh();});RusticHudTheme.Skin(stockView);
            for(int i=0;i<rows.Length;i++)
            {
                var r=(RectTransform)rows[i].transform;r.anchoredPosition=new Vector2(30+i%4*118,-128-i/4*125);r.sizeDelta=new Vector2(110,112);RusticHudTheme.Skin(rows[i]);
                var text=rows[i].GetComponentInChildren<Text>();var tr=(RectTransform)text.transform;tr.sizeDelta=new Vector2(106,48);tr.anchoredPosition=new Vector2(2,-62);text.fontSize=13;
                var iconRect=RusticHudTheme.Rect(rows[i].transform,"ItemIcon",new Vector2(.5f,1),new Vector2(0,-5),new Vector2(54,54));icons[i]=iconRect.gameObject.AddComponent<Image>();icons[i].preserveAspect=true;icons[i].raycastTarget=false;
            }
            string[] categories={"TODOS","ARMAS","SECUND.","ARMADURA","ACESSÓRIO","ARTEFATO"};
            for(int i=0;i<6;i++){int slot=i-1;var tab=categoryTabs[i]=Button(overlay.transform,categories[i],new Vector2(30+i*79,-80),new Vector2(75,34),()=>{category=slot;page=0;selected=null;Refresh();});RusticHudTheme.Skin(tab);tab.GetComponentInChildren<Text>().fontSize=10;}
            classFilter=Button(overlay.transform,"FILTRO DE CLASSE",new Vector2(30,-48),new Vector2(240,26),()=>{classOnly=!classOnly;page=0;selected=null;Refresh();});RusticHudTheme.Skin(classFilter);classFilter.GetComponentInChildren<Text>().fontSize=12;
            profileSource=Button(overlay.transform,"PERFIL",new Vector2(550,-58),new Vector2(650,20),()=>{channel.Hub.Toggle();selected=null;page=0;Refresh();});RusticHudTheme.Skin(profileSource);profileSource.GetComponentInChildren<Text>().fontSize=12;
            heroChoice=Button(overlay.transform,"TROCAR HERÓI",new Vector2(820,-20),new Vector2(240,34),()=>{director.SelectHero((director.SelectedHero+1)%director.HeroCount);selected=null;Refresh();});RusticHudTheme.Skin(heroChoice);heroChoice.GetComponentInChildren<Text>().fontSize=13;
            expeditionButton=Button(overlay.transform,"SAQUE ATUAL",new Vector2(400,-610),new Vector2(170,42),()=>{expeditionView=!expeditionView;deliveries=false;selected=null;Refresh();});RusticHudTheme.Skin(expeditionButton);
            var viewport=RusticHudTheme.Rect(overlay.transform,"ResultScroll",new Vector2(0,1),new Vector2(40,-125),new Vector2(1160,430));viewport.gameObject.AddComponent<RectMask2D>();viewport.gameObject.AddComponent<Image>().color=Color.clear;
            resultScroll=viewport.gameObject.AddComponent<ScrollRect>();resultScroll.viewport=viewport;resultScroll.horizontal=false;resultScroll.movementType=ScrollRect.MovementType.Clamped;resultScroll.scrollSensitivity=32;
            resultText=RusticHudTheme.Label(viewport,"ResultContent",new Vector2(0,1),Vector2.zero,new Vector2(1130,430),"",20);resultContent=(RectTransform)resultText.transform;resultScroll.content=resultContent;resultScroll.gameObject.SetActive(false);
            var previewRect=RusticHudTheme.Rect(overlay.transform,"HeroPreview",new Vector2(0,1),new Vector2(745,-112),new Vector2(185,280));
            var raw=previewRect.gameObject.AddComponent<RawImage>();raw.raycastTarget=false;paperDoll=overlay.AddComponent<EquipmentPaperDoll>();paperDoll.Configure(raw);
            Vector2[] points={new Vector2(540,-130),new Vector2(965,-130),new Vector2(540,-255),new Vector2(965,-255),new Vector2(750,-405)};
            for(int i=0;i<5;i++)
            {
                int slot=i;equipmentSlots[i]=Button(overlay.transform,"EquipmentSlot_"+i,points[i],new Vector2(175,100),()=>SelectEquipped(slot));RusticHudTheme.Skin(equipmentSlots[i]);
                var text=equipmentSlots[i].GetComponentInChildren<Text>();((RectTransform)text.transform).sizeDelta=new Vector2(175,100);text.fontSize=15;
            }
            detailViewport=RusticHudTheme.Rect(overlay.transform,"ItemDetailsScroll",new Vector2(0,1),new Vector2(30,-382),new Vector2(475,165));detailViewport.gameObject.AddComponent<RectMask2D>();detailViewport.gameObject.AddComponent<Image>().color=Color.clear;
            var scroll=detailViewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=detailViewport;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=28;
            detail.transform.SetParent(detailViewport,false);var content=(RectTransform)detail.transform;content.anchoredPosition=Vector2.zero;content.sizeDelta=new Vector2(450,165);detail.fontSize=17;scroll.content=content;
            secondaryEquip=Button(overlay.transform,"USAR NA SECUNDÁRIA",new Vector2(965,-395),new Vector2(240,50),()=>{if(selected!=null)channel.Request(CoopInventoryAction.Equip,selected,EquipmentSlot.OffHand);});RusticHudTheme.Skin(secondaryEquip);secondaryEquip.GetComponentInChildren<Text>().fontSize=13;
            var noticeRect=(RectTransform)notice.transform;noticeRect.anchoredPosition=new Vector2(30,-665);noticeRect.sizeDelta=new Vector2(1180,30);notice.fontSize=14;
            var info=(RectTransform)detail.transform;info.anchoredPosition=new Vector2(30,-382);info.sizeDelta=new Vector2(475,170);detail.fontSize=16;
            foreach(var button in new[]{equip,remove,discard}){var rect=(RectTransform)button.transform;rect.anchoredPosition=new Vector2(rect.anchoredPosition.x,-560);RusticHudTheme.Skin(button);}
        }
        void SelectEquipped(int slot)
        {deliveries=false;expeditionView=false;var actor=ActiveActor;var set=channel.LocalProfile?.Loadout(actor.Roster.Current.heroId);selected=set!=null?set.items[slot]:null;pendingDiscard=null;Refresh();}
        public bool IsOpen=>overlay!=null&&overlay.activeSelf;
        public void OpenInventory()=>Show(false);
        public void CloseInventory(){if(IsOpen)Close();}
        void Show(bool result)
        {
            if (channel.LocalProfile == null || director.LocalSlot < 0&&!channel.HomeMode) return;
            GetComponent<CampaignSessionMenu>()?.Close();
            results = result; deliveries=false; expeditionView=false;selected = pendingDiscard = null; page = 0; overlay.SetActive(true); overlay.transform.SetAsLastSibling(); director.SetInputBlocked(true); Refresh();
        }
        Button TopRight(Transform parent,string label,float y,UnityEngine.Events.UnityAction action)
        {
            var button=Button(parent,label,Vector2.zero,new Vector2(190,44),action);var rect=(RectTransform)button.transform;
            if(director.Campaign!=null){rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.zero;rect.anchoredPosition=new Vector2(30,label=="INVENTÁRIO"?20:75);RusticHudTheme.Skin(button);}
            else{rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one;rect.anchoredPosition=new Vector2(-15,y);}return button;
        }
        void Close() { overlay.SetActive(false); pendingDiscard = null; director.SetInputBlocked(false); }
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
            if (revision != channel.Revision||management!=CanManageNow||lastHero!=ActiveActor.Roster.Selected) Refresh();
        }
        void Select(int row)
        { var items = CurrentItems; int index = page * 8 + row; if (index >= items.Count) return; selected = items[index].instanceId; pendingDiscard = null; Refresh(); }
        void Refresh()
        {
            var profile = channel.LocalProfile; if (profile == null) return; revision = channel.Revision;lastHero=ActiveActor.Roster.Selected;management=CanManageNow;FilterItems();
            if(profileSource!=null){profileSource.gameObject.SetActive(channel.HomeMode&&!results);profileSource.GetComponentInChildren<Text>().text=channel.Hub.Description;profileSource.interactable=channel.Hub.HasRemote;}
            if(classFilter!=null){classFilter.GetComponentInChildren<Text>().text=classOnly?"SÓ ESTA CLASSE":"TODAS AS CLASSES";classFilter.gameObject.SetActive(!results);heroChoice.gameObject.SetActive(!results);heroChoice.interactable=channel.HomeMode||director.Phase==CoopPhase.Lobby;heroChoice.GetComponentInChildren<Text>().text="HERÓI · "+ActiveActor.Roster.Current.displayName;for(int i=0;i<categoryTabs.Length;i++){categoryTabs[i].gameObject.SetActive(!results);categoryTabs[i].interactable=category!=i-1;}}
            if(CurrentItems!=null)page=Mathf.Clamp(page,0,Mathf.Max(0,(CurrentItems.Count-1)/8));
            if(expeditionButton!=null){expeditionButton.gameObject.SetActive(!results);expeditionButton.interactable=profile.expedition!=null;expeditionButton.GetComponentInChildren<Text>().text=expeditionView?"VER MOCHILA":"SAQUE ATUAL · "+(profile.expedition?.items.Count??0);}
            header.text = results ? "RESULTADO COOPERATIVO" : expeditionView?"SAQUE ATUAL — pode equipar fora de combate":deliveries?"ENTREGAS GUARDADAS — "+channel.DeliveryCount:(channel.HomeMode?"PERFIL · NÍVEL "+ProgressionStats.Level(profile.experience)+" · ":"EQUIPAMENTO · ") + profile.inventory.Count + "/" + channel.Catalog.inventoryCapacity;
            for (int i = 0; i < rows.Length; i++)
            {
                int index = page * 8 + i; bool visible = !results && index < CurrentItems.Count; rows[i].gameObject.SetActive(visible);
                if (visible) { var item = CurrentItems[index]; rows[i].GetComponentInChildren<Text>().text = channel.Catalog.Find(item.definitionId).displayName + "\n" + RusticHudTheme.RarityName(item.rarity);if(icons[i]!=null){icons[i].sprite=channel.Catalog.Find(item.definitionId).icon;icons[i].enabled=icons[i].sprite!=null;}var color=channel.Catalog.Rarity(item.rarity).color;rows[i].GetComponentInChildren<Text>().color=color; }
            }
            previous.gameObject.SetActive(!results);next.gameObject.SetActive(!results);previous.interactable=page>0;next.interactable=CurrentItems!=null&&(page+1)*8<CurrentItems.Count;
            equip.gameObject.SetActive(!results); remove.gameObject.SetActive(!results); discard.gameObject.SetActive(!results);
            var actor = ActiveActor; var value = deliveries?CurrentItems.Find(item=>item.instanceId==selected):profile.Item(selected);
            if (results)
            {
                var result = profile.lastResult;
                detail.text = result != null ? result.outcome + "\nXP " + result.experience + " | moedas " + result.coins + "\nItens obtidos: " + result.items.Count : "Nenhuma partida concluída nesta sessão.";
                if (result != null) foreach (var item in result.items) detail.text += "\n" + channel.Catalog.Find(item.definitionId).displayName + " — " + RusticHudTheme.RarityName(item.rarity);
            }
            else if (value == null)
            {
                var stats=ProgressionStats.Calculate(actor.Roster.Current,profile,channel.Catalog);
                detail.text=actor.Roster.Current.displayName+" · nível "+stats.level+"\nVida "+stats.maximumHealth+" · defesa "+(stats.defense*100).ToString("F0")+"%\nDano ×"+stats.damageMultiplier.ToString("F2")+" · cadência ×"+stats.attackSpeed.ToString("F2")+"\nCrítico "+(stats.criticalChance*100).ToString("F0")+"% · velocidade "+stats.speed.ToString("F1")+"\nXP "+profile.experience+" · moedas "+profile.coins+"\nSelecione um item para comparar. Use os filtros de slot e classe.";
            }
            else
            {
                var definition = channel.Catalog.Find(value.definitionId); var before = ProgressionStats.Calculate(actor.Roster.Current, profile, channel.Catalog);
                var after = ProgressionStats.Calculate(actor.Roster.Current, profile, channel.Catalog, selected, definition.slot, true);

                var currentItem=profile.Item(profile.Loadout(actor.Roster.Current.heroId)?.items[0]);var currentWeapon=currentItem!=null?channel.Catalog.Find(currentItem.definitionId):null;
                var currentAttack=currentWeapon!=null?currentWeapon.attacks[0]:actor.Roster.Current.attacks[0];var comparedAttack=definition.slot==EquipmentSlot.MainHand?definition.attacks[0]:currentAttack;
                float currentPower=currentItem!=null?channel.Catalog.Rarity(currentItem.rarity).power:1,comparedPower=definition.slot==EquipmentSlot.MainHand?channel.Catalog.Rarity(value.rarity).power:currentPower;
                int baseBefore=Mathf.RoundToInt(currentAttack.damage*ProgressionStats.StrikeMultiplier(before.damageMultiplier*currentPower,before.elementalDamage,false));int baseAfter=Mathf.RoundToInt(comparedAttack.damage*ProgressionStats.StrikeMultiplier(after.damageMultiplier*comparedPower,after.elementalDamage,false));
                detail.text=definition.displayName+" · "+RusticHudTheme.RarityName(value.rarity)+"\n"+RusticHudTheme.SlotName(definition.slot)+" · nível "+definition.minimumLevel+"\nClasse: "+RusticHudTheme.ClassNames(definition.compatibleHeroes)+"\nGolpe base: "+baseBefore+" → "+baseAfter+" (antes de defesa/crítico)\nVida "+before.maximumHealth+" → "+after.maximumHealth+"\nDefesa "+(before.defense*100).ToString("F0")+"% → "+(after.defense*100).ToString("F0")+"%\nCadência ×"+before.attackSpeed.ToString("F2")+" → ×"+after.attackSpeed.ToString("F2")+"\nCrítico "+(before.criticalChance*100).ToString("F0")+"% → "+(after.criticalChance*100).ToString("F0")+"%\nVelocidade "+before.speed.ToString("F1")+" → "+after.speed.ToString("F1")+"\n"+definition.description;
                if(definition.ability!=null)detail.text+="\nHabilidade: "+definition.ability.displayName+" · "+definition.ability.cooldown.ToString("F1")+"s de recuperação";
                if(definition.attacks!=null&&definition.attacks.Length>0){var attack=definition.attacks[0];detail.text+="\nAlcance "+attack.range.ToString("F1")+"m · sequência "+definition.attacks.Length+" ataques\nComo usar: equipe e segure ATAQUE.\nSegure para o pesado; Q/SUPER quando cheio.";}
                if (!ProgressionStats.CanEquip(profile, channel.Catalog, actor.Roster.Current.heroId, value, definition.slot, out string reason)) detail.text += "\n" + reason;
                var loadout = profile.Loadout(actor.Roster.Current.heroId); if (loadout != null) for (int i = 0; i < loadout.items.Length; i++) if (loadout.items[i] == selected) detail.text += "\nEQUIPADO: " + RusticHudTheme.SlotName((EquipmentSlot)i);
            }
            if(resultScroll!=null)
            {
                resultScroll.gameObject.SetActive(results);detail.gameObject.SetActive(!results);
                if(results){resultText.text=detail.text+"\n\nTodos os itens recebidos estão na mochila ou no estoque de entregas.";resultContent.sizeDelta=new Vector2(1130,Mathf.Max(430,resultText.preferredHeight+24));}
            }
            if(deliveries&&value!=null)detail.text=channel.Catalog.Find(value.definitionId).displayName+"\n"+RusticHudTheme.RarityName(value.rarity)+"\nGuardado no estoque. Receba na mochila antes de equipar.\nEspaços livres: "+(channel.Catalog.inventoryCapacity-profile.inventory.Count);
            if(stockView!=null){stockView.gameObject.SetActive(!results);stockView.interactable=deliveries||channel.DeliveryCount>0;stockView.GetComponentInChildren<Text>().text=deliveries?"VOLTAR À MOCHILA":"ENTREGAS · "+channel.DeliveryCount;}
            equip.GetComponentInChildren<Text>().text=deliveries?"RECEBER ITEM":"EQUIPAR";
            if(paperDoll!=null)
            {
                var detailRect=(RectTransform)detail.transform;detailRect.anchoredPosition=Vector2.zero;detailRect.sizeDelta=new Vector2(450,Mathf.Max(165,detail.preferredHeight+20));detailViewport.gameObject.SetActive(!results);
                paperDoll.SetVisible(!results);paperDoll.Refresh(actor.Presentation,profile,actor.Roster.Current.heroId);
                string[] names={"ARMA PRINCIPAL","SECUNDÁRIA","ARMADURA","ACESSÓRIO","ARTEFATO"};var set=profile.Loadout(actor.Roster.Current.heroId);
                for(int i=0;i<5;i++)
                {
                    equipmentSlots[i].gameObject.SetActive(!results);var equipped=set!=null?profile.Item(set.items[i]):null;
                    equipmentSlots[i].GetComponentInChildren<Text>().text=names[i]+"\n"+(equipped!=null?channel.Catalog.Find(equipped.definitionId).displayName:i==0?"Sem arma equipada":"Vazio");
                }
                if(!results&&!deliveries&&!expeditionView&&profile.inventory.Count==0)detail.text+="Sua mochila está vazia. Vença inimigos, abra baús e veja SAQUE ATUAL para equipar durante esta tentativa.\nSaque desta tentativa: "+(profile.expedition?.items.Count??0)+" itens. Eles são guardados no resultado.";
            }
            if(secondaryEquip!=null)
            {
                bool dagger=value!=null&&channel.Catalog.Find(value.definitionId).category==WeaponCategory.Daggers;
                secondaryEquip.gameObject.SetActive(!results&&!deliveries&&dagger);bool allowed=ProgressionStats.CanEquip(profile,channel.Catalog,actor.Roster.Current.heroId,value,EquipmentSlot.OffHand,out string reason);secondaryEquip.interactable=management&&dagger&&allowed;
                if(dagger)detail.text+="\nMão secundária: "+(allowed?"disponível; exige outra instância da adaga":reason);
            }
            if(detailViewport!=null)detail.rectTransform.sizeDelta=new Vector2(450,Mathf.Max(165,detail.preferredHeight+20));
            bool canEquip=value!=null&&ProgressionStats.CanEquip(profile,channel.Catalog,actor.Roster.Current.heroId,value,channel.Catalog.Find(value.definitionId).slot,out _);
            equip.interactable=!results&&management&&(deliveries?value!=null&&profile.inventory.Count<channel.Catalog.inventoryCapacity:canEquip);remove.interactable=!results&&!deliveries&&management&&value!=null;discard.interactable=remove.interactable&&!(channel.HomeMode&&channel.Hub.UsingRemote);if(channel.HomeMode&&channel.Hub.UsingRemote&&deliveries)equip.interactable=false;
            discard.GetComponentInChildren<Text>().text = pendingDiscard == selected && selected != null ? "CONFIRMAR?" : "DESCARTAR";
            notice.text = channel.Message ?? "Organize por slot e compare os atributos. Em partida, equipe fora de perigo. O perfil da sala é guardado pelo host; o perfil inicial é deste aparelho.";
        }
        void Equip() { if(deliveries){if(selected!=null)channel.Request(CoopInventoryAction.ClaimDelivery,selected,EquipmentSlot.MainHand);return;} var item = channel.LocalProfile?.Item(selected); if (item == null) return; pendingDiscard = null; channel.Request(CoopInventoryAction.Equip, selected, channel.Catalog.Find(item.definitionId).slot); }
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
