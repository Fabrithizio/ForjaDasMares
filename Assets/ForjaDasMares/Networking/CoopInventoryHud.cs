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
        Button inventoryButton,resultButton,stockView;bool deliveries;
        System.Collections.Generic.List<ItemInstance> CurrentItems=>deliveries?channel.LocalProfile?.deliveries:channel.LocalProfile?.inventory;EquipmentPaperDoll paperDoll;readonly Button[] equipmentSlots=new Button[5];
        bool management;
        bool CanManageNow=>director.LocalSlot>=0&&director.Actor(director.LocalSlot).Health.IsAlive&&director.Phase!=CoopPhase.Loading&&director.EncounterStatus!=EncounterState.Spawning&&director.EncounterStatus!=EncounterState.Fighting;
        int page, revision = -1; string selected, pendingDiscard; bool results;
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
            Button(overlay.transform, "FECHAR", new Vector2(1080, -20), new Vector2(150, 44), Close);
            for (int i = 0; i < rows.Length; i++)
            { int row = i; rows[i] = Button(overlay.transform, "", new Vector2(30, -95 - i * 57), new Vector2(560, 50), () => Select(row)); }
            Button(overlay.transform, "ANTERIOR", new Vector2(30, -560), new Vector2(170, 42), () => { page = Mathf.Max(0, page - 1); selected = null; Refresh(); });
            Button(overlay.transform, "PRÓXIMA", new Vector2(215, -560), new Vector2(170, 42), () => { var items = CurrentItems; if (items != null && (page + 1) * 8 < items.Count) page++; selected = null; Refresh(); });
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
            overlay.GetComponent<Image>().color=Color.clear;RusticHudTheme.Panel(panel,RusticHudTheme.Ink,22);
            stockView=Button(overlay.transform,"ENTREGAS",new Vector2(400,-560),new Vector2(170,42),()=>{deliveries=!deliveries;selected=null;page=0;Refresh();});RusticHudTheme.Skin(stockView);
            for(int i=0;i<rows.Length;i++)
            {
                var r=(RectTransform)rows[i].transform;r.anchoredPosition=new Vector2(30+i%4*118,-105-i/4*125);r.sizeDelta=new Vector2(110,112);RusticHudTheme.Skin(rows[i]);
                var text=rows[i].GetComponentInChildren<Text>();var tr=(RectTransform)text.transform;tr.sizeDelta=r.sizeDelta;text.fontSize=15;
            }
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
            var info=(RectTransform)detail.transform;info.anchoredPosition=new Vector2(30,-382);info.sizeDelta=new Vector2(475,170);detail.fontSize=16;
            foreach(var button in new[]{equip,remove,discard}){var rect=(RectTransform)button.transform;rect.anchoredPosition=new Vector2(rect.anchoredPosition.x,-560);RusticHudTheme.Skin(button);}
        }
        void SelectEquipped(int slot)
        {deliveries=false;var actor=director.Actor(director.LocalSlot);var set=channel.LocalProfile?.Loadout(actor.Roster.Current.heroId);selected=set!=null?set.items[slot]:null;pendingDiscard=null;Refresh();}
        void Show(bool result)
        {
            if (channel.LocalProfile == null || director.LocalSlot < 0) return;
            results = result; deliveries=false; selected = pendingDiscard = null; page = 0; overlay.SetActive(true); overlay.transform.SetAsLastSibling(); director.SetInputBlocked(true); Refresh();
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
                inventoryButton.interactable=director.LocalSlot>=0;
                if(director.Campaign!=null)resultButton.gameObject.SetActive(director.Phase==CoopPhase.Victory||director.Phase==CoopPhase.Defeat);
            }
            if (overlay == null || !overlay.activeSelf) return;
            if (director.LocalSlot < 0) { Close(); return; }
            if (revision != channel.Revision||management!=CanManageNow) Refresh();
        }
        void Select(int row)
        { var items = CurrentItems; int index = page * 8 + row; if (index >= items.Count) return; selected = items[index].instanceId; pendingDiscard = null; Refresh(); }
        void Refresh()
        {
            var profile = channel.LocalProfile; if (profile == null) return; revision = channel.Revision;management=CanManageNow;
            if(CurrentItems!=null)page=Mathf.Clamp(page,0,Mathf.Max(0,(CurrentItems.Count-1)/8));
            header.text = results ? "RESULTADO COOPERATIVO" : deliveries?"ENTREGAS GUARDADAS — "+channel.DeliveryCount:"INVENTÁRIO DA SESSÃO — " + profile.inventory.Count + "/" + channel.Catalog.inventoryCapacity;
            for (int i = 0; i < rows.Length; i++)
            {
                int index = page * 8 + i; bool visible = !results && index < CurrentItems.Count; rows[i].gameObject.SetActive(visible);
                if (visible) { var item = CurrentItems[index]; rows[i].GetComponentInChildren<Text>().text = channel.Catalog.Find(item.definitionId).displayName + "\n" + item.rarity;var color=channel.Catalog.Rarity(item.rarity).color;rows[i].GetComponentInChildren<Text>().color=color; }
            }
            equip.gameObject.SetActive(!results); remove.gameObject.SetActive(!results); discard.gameObject.SetActive(!results);
            var actor = director.Actor(director.LocalSlot); var value = CurrentItems.Find(item=>item.instanceId==selected);
            if (results)
            {
                var result = profile.lastResult;
                detail.text = result != null ? result.outcome + "\nXP " + result.experience + " | moedas " + result.coins + "\nItens obtidos: " + result.items.Count : "Nenhuma partida concluída nesta sessão.";
                if (result != null) foreach (var item in result.items) detail.text += "\n" + channel.Catalog.Find(item.definitionId).displayName + " — " + item.rarity;
            }
            else if (value == null) detail.text = "Escolha um item. O host valida a equipagem e salva este perfil cooperativo.\n\nXP total " + profile.experience + " | moedas " + profile.coins + "\nEntregas guardadas: " + channel.DeliveryCount;
            else
            {
                var definition = channel.Catalog.Find(value.definitionId); var before = ProgressionStats.Calculate(actor.Roster.Current, profile, channel.Catalog);
                var after = ProgressionStats.Calculate(actor.Roster.Current, profile, channel.Catalog, selected, definition.slot, true);
                detail.text = definition.displayName + "\n" + definition.description + "\n\nVida " + before.maximumHealth + " → " + after.maximumHealth + "\nDano ×" + before.damageMultiplier.ToString("F2") + " → ×" + after.damageMultiplier.ToString("F2") + "\nVelocidade " + before.speed.ToString("F1") + " → " + after.speed.ToString("F1");
                if (!ProgressionStats.CanEquip(profile, channel.Catalog, actor.Roster.Current.heroId, value, definition.slot, out string reason)) detail.text += "\n" + reason;
                var loadout = profile.Loadout(actor.Roster.Current.heroId); if (loadout != null) for (int i = 0; i < loadout.items.Length; i++) if (loadout.items[i] == selected) detail.text += "\nEQUIPADO: " + (EquipmentSlot)i;
            }
            if(resultScroll!=null)
            {
                resultScroll.gameObject.SetActive(results);detail.gameObject.SetActive(!results);
                if(results){resultText.text=detail.text+"\n\nTodos os itens recebidos estão na mochila ou no estoque de entregas.";resultContent.sizeDelta=new Vector2(1130,Mathf.Max(430,resultText.preferredHeight+24));}
            }
            if(deliveries&&value!=null)detail.text=channel.Catalog.Find(value.definitionId).displayName+"\n"+value.rarity+"\nGuardado no estoque. Receba na mochila antes de equipar.\nEspaços livres: "+(channel.Catalog.inventoryCapacity-profile.inventory.Count);
            if(stockView!=null){stockView.gameObject.SetActive(!results);stockView.interactable=deliveries||channel.DeliveryCount>0;stockView.GetComponentInChildren<Text>().text=deliveries?"VOLTAR À MOCHILA":"ENTREGAS · "+channel.DeliveryCount;}
            equip.GetComponentInChildren<Text>().text=deliveries?"RECEBER ITEM":"EQUIPAR";
            if(paperDoll!=null)
            {
                var detailRect=(RectTransform)detail.transform;detailRect.anchoredPosition=results?new Vector2(40,-130):new Vector2(30,-382);detailRect.sizeDelta=results?new Vector2(1140,430):new Vector2(475,170);
                paperDoll.SetVisible(!results);paperDoll.Refresh(actor.Presentation,profile,actor.Roster.Current.heroId);
                string[] names={"ARMA PRINCIPAL","SECUNDÁRIA","ARMADURA","ACESSÓRIO","ARTEFATO"};var set=profile.Loadout(actor.Roster.Current.heroId);
                for(int i=0;i<5;i++)
                {
                    equipmentSlots[i].gameObject.SetActive(!results);var equipped=set!=null?profile.Item(set.items[i]):null;
                    equipmentSlots[i].GetComponentInChildren<Text>().text=names[i]+"\n"+(equipped!=null?channel.Catalog.Find(equipped.definitionId).displayName:i==0?"Ataque base do herói":"Vazio");
                }
                if(!results&&!deliveries&&profile.inventory.Count==0)detail.text="Sua mochila está vazia. O herói já tem ataque básico: vença inimigos e abra baús para conquistar equipamentos.\nSaque desta tentativa: "+(profile.expedition?.items.Count??0)+" itens. Eles são guardados no resultado.";
            }
            bool canEquip=value!=null&&ProgressionStats.CanEquip(profile,channel.Catalog,actor.Roster.Current.heroId,value,channel.Catalog.Find(value.definitionId).slot,out _);
            equip.interactable=!results&&management&&(deliveries?value!=null&&profile.inventory.Count<channel.Catalog.inventoryCapacity:canEquip);remove.interactable=discard.interactable=!results&&!deliveries&&management&&value!=null;
            discard.GetComponentInChildren<Text>().text = pendingDiscard == selected && selected != null ? "CONFIRMAR?" : "DESCARTAR";
            notice.text = channel.Message ?? "Gerencie fora de encontros; equipar não cura. O progresso cooperativo é armazenado no host, separado do perfil local.";
        }
        void Equip() { if(deliveries){if(selected!=null)channel.Request(CoopInventoryAction.ClaimDelivery,selected,EquipmentSlot.MainHand);return;} var item = channel.LocalProfile?.Item(selected); if (item == null) return; pendingDiscard = null; channel.Request(CoopInventoryAction.Equip, selected, channel.Catalog.Find(item.definitionId).slot); }
        void Unequip()
        { var set = channel.LocalProfile?.Loadout(director.Actor(director.LocalSlot).Roster.Current.heroId); if (set == null) return; for (int i = 0; i < set.items.Length; i++) if (set.items[i] == selected) { channel.Request(CoopInventoryAction.Unequip, null, (EquipmentSlot)i); break; } }
        void Discard()
        { if (selected == null) return; if (pendingDiscard != selected) { pendingDiscard = selected; Refresh(); return; } channel.Request(CoopInventoryAction.Discard, selected, EquipmentSlot.MainHand, true); pendingDiscard = null; }
        Text Label(Transform parent, string value, Vector2 position, Vector2 size, int scale = 19)
        { var go = new GameObject("Text", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = size; var text = go.GetComponent<Text>(); text.font = font; text.fontSize = scale; text.text = value; text.color = Color.white; text.raycastTarget = false; return text; }
        Button Button(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        { var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = size; go.GetComponent<Image>().color = new Color(.13f, .3f, .4f); var button = go.GetComponent<Button>(); button.onClick.AddListener(action); var text = Label(go.transform, label, Vector2.zero, size); text.alignment = TextAnchor.MiddleCenter; return button; }
        void OnDisable() { if (director != null) director.SetInputBlocked(false); }
    }
}
