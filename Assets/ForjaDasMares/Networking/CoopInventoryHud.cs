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
        int page, revision = -1; string selected, pendingDiscard; bool results;
        public void Configure(CoopDirector session) => director = session;
        void Start()
        {
            channel = director.GetComponent<CoopProfileChannel>(); font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var parent = GetComponent<CoopHud>().SafeRoot;
            TopRight(parent, "INVENTÁRIO", -119, () => Show(false));
            TopRight(parent, "RESULTADO", -171, () => Show(true));
            overlay = new GameObject("CoopInventory", typeof(RectTransform), typeof(Image)); overlay.transform.SetParent(parent, false);
            var full = (RectTransform)overlay.transform; full.anchorMin = Vector2.zero; full.anchorMax = Vector2.one; full.offsetMin = full.offsetMax = Vector2.zero; overlay.GetComponent<Image>().color = new Color(.025f, .045f, .065f, .98f);
            header = Label(overlay.transform, "", new Vector2(30, -20), new Vector2(1000, 50), 23);
            Button(overlay.transform, "FECHAR", new Vector2(1080, -20), new Vector2(150, 44), Close);
            for (int i = 0; i < rows.Length; i++)
            { int row = i; rows[i] = Button(overlay.transform, "", new Vector2(30, -95 - i * 57), new Vector2(560, 50), () => Select(row)); }
            Button(overlay.transform, "ANTERIOR", new Vector2(30, -560), new Vector2(170, 42), () => { page = Mathf.Max(0, page - 1); selected = null; Refresh(); });
            Button(overlay.transform, "PRÓXIMA", new Vector2(215, -560), new Vector2(170, 42), () => { var items = channel.LocalProfile?.inventory; if (items != null && (page + 1) * 8 < items.Count) page++; selected = null; Refresh(); });
            detail = Label(overlay.transform, "", new Vector2(620, -95), new Vector2(600, 415), 19);
            equip = Button(overlay.transform, "EQUIPAR", new Vector2(620, -535), new Vector2(185, 48), Equip);
            remove = Button(overlay.transform, "DESEQUIPAR", new Vector2(825, -535), new Vector2(185, 48), Unequip);
            discard = Button(overlay.transform, "DESCARTAR", new Vector2(1030, -535), new Vector2(185, 48), Discard);
            notice = Label(overlay.transform, "", new Vector2(30, -620), new Vector2(1180, 75), 18); overlay.SetActive(false);
        }
        void Show(bool result)
        {
            if (channel.LocalProfile == null || director.LocalSlot < 0) return;
            results = result; selected = pendingDiscard = null; page = 0; overlay.SetActive(true); overlay.transform.SetAsLastSibling(); director.SetInputBlocked(true); Refresh();
        }
        void TopRight(Transform parent, string label, float y, UnityEngine.Events.UnityAction action)
        { var rect = (RectTransform)Button(parent, label, Vector2.zero, new Vector2(190, 44), action).transform; rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one; rect.anchoredPosition = new Vector2(-15, y); }
        void Close() { overlay.SetActive(false); pendingDiscard = null; director.SetInputBlocked(false); }
        void Update()
        {
            if (overlay == null || !overlay.activeSelf) return;
            if (director.LocalSlot < 0) { Close(); return; }
            if (revision != channel.Revision) Refresh();
        }
        void Select(int row)
        { var items = channel.LocalProfile.inventory; int index = page * 8 + row; if (index >= items.Count) return; selected = items[index].instanceId; pendingDiscard = null; Refresh(); }
        void Refresh()
        {
            var profile = channel.LocalProfile; if (profile == null) return; revision = channel.Revision;
            header.text = results ? "RESULTADO COOPERATIVO" : "INVENTÁRIO DA SESSÃO — " + profile.inventory.Count + "/" + channel.Catalog.inventoryCapacity;
            for (int i = 0; i < rows.Length; i++)
            {
                int index = page * 8 + i; bool visible = !results && index < profile.inventory.Count; rows[i].gameObject.SetActive(visible);
                if (visible) { var item = profile.inventory[index]; rows[i].GetComponentInChildren<Text>().text = channel.Catalog.Find(item.definitionId).displayName + " — " + item.rarity; }
            }
            equip.gameObject.SetActive(!results); remove.gameObject.SetActive(!results); discard.gameObject.SetActive(!results);
            var actor = director.Actor(director.LocalSlot); var value = profile.Item(selected);
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
            equip.interactable = remove.interactable = discard.interactable = !results && value != null;
            discard.GetComponentInChildren<Text>().text = pendingDiscard == selected && selected != null ? "CONFIRMAR?" : "DESCARTAR";
            notice.text = channel.Message ?? "Gerencie fora de encontros; equipar não cura. O progresso cooperativo é armazenado no host, separado do perfil local.";
        }
        void Equip() { var item = channel.LocalProfile?.Item(selected); if (item == null) return; pendingDiscard = null; channel.Request(CoopInventoryAction.Equip, selected, channel.Catalog.Find(item.definitionId).slot); }
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
