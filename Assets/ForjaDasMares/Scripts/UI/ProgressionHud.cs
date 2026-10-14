using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ForjaDasMares
{
    [DefaultExecutionOrder(200)]
    public sealed class ProgressionHud : MonoBehaviour
    {
        enum Page { Inventory, Equipment, Deliveries, Expedition, Result, Options }
        [SerializeField] ProgressionSceneBridge bridge;
        [SerializeField] Canvas canvas;
        [SerializeField] InputActionAsset actions;
        [SerializeField] ArenaInput commands;
        InputActionAsset runtime;
        readonly List<RectTransform> hitAreas = new List<RectTransform>(64);
        readonly List<Text> labels = new List<Text>(64);
        readonly List<int> sizes = new List<int>(64);
        readonly List<ItemInstance> visibleItems = new List<ItemInstance>(64);
        readonly Button[] rows = new Button[8];
        GameObject overlay, confirmation;
        Text status, title, detail, comparison, message, counter, resultSummary;
        Button primary, secondary, discard;
        Button closeButton, confirmButton, cancelButton;
        RectTransform panel;
        Page page;
        int pageIndex, slot;
        string selected, resultSeen, pendingDiscard;
        float poll;
        bool closeRequested;
        public bool IsOpen => overlay != null && overlay.activeSelf;
        public void Configure(ProgressionSceneBridge game, Canvas target, ArenaInput input, InputActionAsset controls)
        { bridge = game; canvas = target; commands = input; actions = controls; }
        void Awake()
        {
            Build(); commands.PointerBlocked = PointerBlocked;
            foreach (var button in canvas.GetComponentsInChildren<Button>(true)) { var rect = button.transform as RectTransform; if (!hitAreas.Contains(rect)) hitAreas.Add(rect); }
            if (actions != null)
            {
                runtime = Instantiate(actions); runtime.FindAction("Inventory", true).performed += InventoryKey; runtime.FindAction("Close", true).performed += CloseKey;
            }
            if (bridge.Service != null) { resultSeen = bridge.Service.Profile.lastResult?.runId; bridge.Service.Changed += Changed; }
            if (bridge.Service != null) Changed();
        }
        void OnEnable() => runtime?.Enable();
        void OnDisable() { runtime?.Disable(); Close(); }
        void OnDestroy()
        {
            if (bridge != null && bridge.Service != null) bridge.Service.Changed -= Changed;
            if (commands != null) commands.PointerBlocked = null;
            if (runtime != null) Destroy(runtime);
        }
        void InventoryKey(InputAction.CallbackContext _) { if (IsOpen) Close(); else OpenInventory(); }
        void CloseKey(InputAction.CallbackContext _) { if (confirmation.activeSelf) { confirmation.SetActive(false); pendingDiscard = null; } else closeRequested = true; }
        bool PointerBlocked(Vector2 point)
        { if (IsOpen) return true; foreach (var rect in hitAreas) if (rect != null && rect.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rect, point)) return true; return false; }
        void Update()
        {
            if (closeRequested) { closeRequested = false; Close(); }
            if (panel != null)
            {
                var rect = (RectTransform)canvas.transform; Rect safe = Screen.safeArea;
                float safeWidth = rect.rect.width * safe.width / Mathf.Max(1, Screen.width), safeHeight = rect.rect.height * safe.height / Mathf.Max(1, Screen.height);
                float scale = Mathf.Min(1, (safeWidth - 24) / 1120, (safeHeight - 24) / 650); panel.localScale = Vector3.one * Mathf.Max(.1f, scale);
            }
            poll -= Time.unscaledDeltaTime; if (poll > 0) return; poll = .2f;
            if (bridge.Service == null) { status.text = "PROGRESSÃO: " + bridge.Error; return; }
            var profile = bridge.Service.Profile; int level = ProgressionStats.Level(profile.experience, bridge.Catalog.maximumLevel); var run = profile.expedition;
            status.text = "NÍVEL " + level + " | XP " + ProgressionStats.ExperienceInLevel(profile.experience, bridge.Catalog.maximumLevel) + "/" + ProgressionStats.RequiredExperience(level) + " | MOEDAS " + profile.coins + (run != null ? " | EXPEDIÇÃO +" + run.experience + "XP / " + run.coins + " moedas / " + run.items.Count + " itens" : "") + " | LUME " + bridge.Companion.RemainingCharges;
        }
        void Changed()
        {
            if (bridge == null || bridge.Closing) return;
            var profile = bridge.Service.Profile; AudioListener.volume = profile.options.volume;
            for (int i = 0; i < labels.Count; i++) labels[i].fontSize = profile.options.largeText ? sizes[i] + 2 : sizes[i];
            if (profile.lastResult != null && profile.lastResult.runId != resultSeen)
            { resultSeen = profile.lastResult.runId; if (profile.lastResult.items.Count > 0 || profile.lastResult.experience > 0 || profile.lastResult.coins > 0 || profile.lastResult.outcome == "Derrota") Show(Page.Result); }
            Refresh();
        }
        public void OpenInventory()
        { if (!bridge.CanManage) { status.text = "Termine o encontro e aguarde os ataques para gerenciar equipamentos."; poll = 3; return; } Show(Page.Inventory); }
        void Show(Page value)
        {
            if (bridge.Service == null) return; page = value; pageIndex = 0; selected = null; pendingDiscard = null; confirmation.SetActive(false);
            overlay.SetActive(true); overlay.transform.SetAsLastSibling(); bridge.BlockCommands(true);
            foreach (var stick in canvas.GetComponentsInChildren<TouchMoveStick>(true)) stick.Release();
            Refresh(); if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(rows[0].gameObject.activeSelf ? rows[0].gameObject : closeButton.gameObject);
        }
        public void Close()
        { if (overlay != null) overlay.SetActive(false); if (bridge != null && commands != null) bridge.BlockCommands(false); }
        void Tab(Page value) { page = value; pageIndex = 0; selected = null; pendingDiscard = null; confirmation.SetActive(false); Refresh(); }
        void ChangePage(int direction) { pageIndex = Mathf.Clamp(pageIndex + direction, 0, Mathf.Max(0, (visibleItems.Count - 1) / rows.Length)); selected = null; Refresh(); }
        void Refresh()
        {
            if (bridge.Service == null || overlay == null || !IsOpen) return;
            var profile = bridge.Service.Profile; visibleItems.Clear();
            if (page == Page.Inventory) visibleItems.AddRange(profile.inventory);
            else if (page == Page.Deliveries) visibleItems.AddRange(profile.deliveries);
            else if (page == Page.Expedition && profile.expedition != null) visibleItems.AddRange(profile.expedition.items);
            else if (page == Page.Result && profile.lastResult != null) visibleItems.AddRange(profile.lastResult.items);
            title.text = bridge.Heroes.Current.displayName + " — " + PageName(page);
            message.text = bridge.Service.Message;
            counter.text = page == Page.Inventory ? "Espaço " + profile.inventory.Count + "/" + bridge.Catalog.inventoryCapacity : "Página " + (pageIndex + 1) + "/" + Mathf.Max(1, Mathf.CeilToInt(visibleItems.Count / 8f));
            resultSummary.text = page == Page.Result ? profile.lastResult != null ? profile.lastResult.outcome + " | " + profile.lastResult.location + "\nRecebidos: " + profile.lastResult.experience + " XP, " + profile.lastResult.coins + " moedas, " + profile.lastResult.items.Count + " itens. Estoque cheio: itens enviados para Entregas." : "Nenhuma expedição encerrada." : page == Page.Expedition ? "Saque temporário: vitória concede tudo; derrota/abandono guarda itens e metade de XP/moedas." : "Equipar só fora de encontros. Armas de duas mãos guardam a secundária. Equipar não cura.";
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i]; row.onClick.RemoveAllListeners();
                if (page == Page.Equipment)
                {
                    row.gameObject.SetActive(i < 5); if (i >= 5) continue;
                    int index = i; var set = profile.Loadout(bridge.Heroes.Current.heroId); string id = set != null ? set.items[i] : null; var item = profile.Item(id);
                    RowText(row, SlotName((EquipmentSlot)i) + ": " + (item != null ? Name(item) : "Padrão do herói"), item);
                    row.onClick.AddListener(() => { slot = index; selected = id; Details(); });
                }
                else if (page == Page.Options)
                {
                    row.gameObject.SetActive(i < 3); if (i >= 3) continue; int option = i;
                    RowText(row, i == 0 ? "Volume: " + Mathf.RoundToInt(profile.options.volume * 100) + "% (alterar)" : i == 1 ? "Mascote: " + (profile.options.companionEnabled ? "ligado" : "desligado") : "Texto: " + (profile.options.largeText ? "ampliado" : "normal"), null);
                    row.onClick.AddListener(() => Option(option));
                }
                else
                {
                    int index = pageIndex * 8 + i; row.gameObject.SetActive(index < visibleItems.Count); if (index >= visibleItems.Count) continue;
                    var item = visibleItems[index]; string id = item.instanceId; RowText(row, Name(item), item); row.onClick.AddListener(() => { selected = id; Details(); });
                }
            }
            Details();
        }
        string Name(ItemInstance item) => bridge.Catalog.Find(item.definitionId).displayName + " — " + RarityName(item.rarity);
        static string RarityName(ItemRarity value) => new[] { "Comum", "Incomum", "Rara", "Épica", "Lendária" }[(int)value];
        static string PageName(Page value) => new[] { "Inventário", "Equipamentos", "Entregas", "Saque", "Resultado", "Opções" }[(int)value];
        static string SlotName(EquipmentSlot value) => new[] { "Principal", "Secundária", "Armadura", "Acessório", "Artefato" }[(int)value];
        void RowText(Button row, string text, ItemInstance item) { var label = row.GetComponentInChildren<Text>(); label.text = text; label.color = item != null ? bridge.Catalog.Rarity(item.rarity).color : Color.white; }
        void Details()
        {
            var profile = bridge.Service.Profile; var item = page == Page.Equipment ? profile.Item(selected) : visibleItems.Find(value => value.instanceId == selected);
            primary.interactable = secondary.interactable = discard.interactable = false; primary.GetComponentInChildren<Text>().text = page == Page.Equipment ? "DESEQUIPAR" : page == Page.Deliveries ? "RECEBER" : "EQUIPAR";
            if (item == null) { detail.text = page == Page.Options ? "As opções são gravadas no perfil local. Mascote não perde o cooldown nem ganha cargas ao alternar." : "Selecione um item. Os itens equipados continuam ocupando um espaço do inventário."; comparison.text = StatsText(bridge.Equipment.Stats); return; }
            var data = bridge.Catalog.Find(item.definitionId); var text = new StringBuilder(); text.AppendLine(Name(item)).AppendLine(data.description).AppendLine("Slot: " + SlotName(data.slot) + " | Nível " + data.minimumLevel);
            if (data.attacks != null && data.attacks.Length > 0) { var attack = data.attacks[0]; text.AppendLine((attack.shape == AttackShape.Arc ? "Golpe" : "Projétil") + " | alcance " + attack.range.ToString("0.0") + " | dano base " + attack.damage + " | tempo " + (attack.windup + attack.recovery).ToString("0.00") + "s"); text.AppendLine(attack.shape == AttackShape.Arc ? "Arco " + attack.arc.ToString("0") + "°" + (data.twinStrike ? " | dois golpes" : "") : "Projéteis " + attack.projectileCount + " | velocidade " + attack.projectileSpeed.ToString("0.0")); }
            foreach (var bonus in data.bonuses) text.AppendLine(ModifierText(bonus)); foreach (var bonus in item.modifiers) text.AppendLine("Afixo " + ModifierText(bonus));
            if (item.rarity == ItemRarity.Legendary && data.slot == EquipmentSlot.MainHand) text.AppendLine("Eco rúnico: pulso lento a cada três ataques básicos.");
            detail.text = text.ToString();
            bool owned = profile.Item(item.instanceId) != null; string reason = "";
            bool valid = owned && ProgressionStats.CanEquip(profile, bridge.Catalog, bridge.Heroes.Current.heroId, item, data.slot, out reason);
            var preview = valid ? ProgressionStats.Calculate(bridge.Heroes.Current, profile, bridge.Catalog, item.instanceId, data.slot, true) : bridge.Equipment.Stats;
            comparison.text = "ATUAL → APÓS EQUIPAR\n" + Compare(bridge.Equipment.Stats, preview) + (valid ? "" : "\n" + (owned ? reason : "Receba este item antes de equipar."));
            primary.interactable = page == Page.Deliveries ? profile.inventory.Count < bridge.Catalog.inventoryCapacity : bridge.CanManage && (page == Page.Equipment ? owned : valid && page != Page.Expedition);
            secondary.interactable = bridge.CanManage && owned && page == Page.Inventory && ProgressionStats.CanEquip(profile, bridge.Catalog, bridge.Heroes.Current.heroId, item, EquipmentSlot.OffHand, out _);
            discard.interactable = bridge.CanManage && owned && page == Page.Inventory;
        }
        static string StatsText(StatSnapshot s) => "Vida " + s.maximumHealth + " | dano ×" + s.damageMultiplier.ToString("0.00") + "\nDefesa " + s.defense.ToString("P0") + " | movimento " + s.speed.ToString("0.0") + "\nCrítico " + s.criticalChance.ToString("P0") + " | ataque ×" + s.attackSpeed.ToString("0.00") + "\nCooldown ×" + s.cooldownMultiplier.ToString("0.00") + " | elemental " + s.elementalDamage.ToString("P0") + "\nRecuperação " + s.recovery.ToString("0.0") + "HP/s | bloqueio " + s.block.ToString("P0");
        static readonly string[] StatNames = { "Vida", "Dano", "Defesa", "Movimento", "Crítico", "Velocidade de ataque", "Redução de cooldown", "Dano elemental", "Recuperação" };
        static string ModifierText(StatModifier value) => StatNames[(int)value.stat] + " +" + (value.stat == ItemStat.Health || value.stat == ItemStat.Defense ? value.value.ToString("0.#") : value.stat == ItemStat.Recovery ? value.value.ToString("0.#") + " HP/s" : value.value.ToString("P0"));
        static string Compare(StatSnapshot a, StatSnapshot b) => "Vida " + a.maximumHealth + " → " + b.maximumHealth + " | dano ×" + a.damageMultiplier.ToString("0.00") + " → ×" + b.damageMultiplier.ToString("0.00") + "\nDefesa " + a.defense.ToString("P0") + " → " + b.defense.ToString("P0") + " | vel. " + a.speed.ToString("0.0") + " → " + b.speed.ToString("0.0") + "\nCrítico " + a.criticalChance.ToString("P0") + " → " + b.criticalChance.ToString("P0") + " | ataque ×" + a.attackSpeed.ToString("0.00") + " → ×" + b.attackSpeed.ToString("0.00") + "\nCooldown ×" + a.cooldownMultiplier.ToString("0.00") + " → ×" + b.cooldownMultiplier.ToString("0.00") + " | elem. " + a.elementalDamage.ToString("P0") + " → " + b.elementalDamage.ToString("P0") + "\nRecup. " + a.recovery.ToString("0.0") + " → " + b.recovery.ToString("0.0") + "HP/s | bloqueio " + a.block.ToString("P0") + " → " + b.block.ToString("P0");
        void Apply()
        {
            var item = bridge.Service.Profile.Item(selected);
            if (page == Page.Deliveries) bridge.Service.ClaimDelivery(selected);
            else if (page == Page.Equipment) bridge.Unequip((EquipmentSlot)slot);
            else if (item != null) bridge.Equip(selected, bridge.Catalog.Find(item.definitionId).slot);
            Refresh();
        }
        void DiscardRequest() { if (selected == null) return; pendingDiscard = selected; confirmation.SetActive(true); confirmation.transform.SetAsLastSibling(); if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(confirmButton.gameObject); }
        public void ConfirmDiscard() { if (pendingDiscard != null && bridge.CanManage) bridge.Service.Discard(pendingDiscard, true); pendingDiscard = selected = null; confirmation.SetActive(false); Refresh(); }
        void Option(int index)
        {
            var options = bridge.Service.Profile.options; float volume = index == 0 ? options.volume <= .01f ? 1 : Mathf.Max(0, options.volume - .1f) : options.volume;
            bridge.Service.SetOptions(volume, index == 1 ? !options.companionEnabled : options.companionEnabled, index == 2 ? !options.largeText : options.largeText); Refresh();
        }
        RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size, Vector2? anchor = null)
        { var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = anchor ?? new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = size; return rect; }
        Text Label(Transform parent, string name, Vector2 position, Vector2 size, int font = 18, Vector2? anchor = null)
        { var value = Rect(parent, name, position, size, anchor).gameObject.AddComponent<Text>(); value.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); value.fontSize = font; value.color = Color.white; value.raycastTarget = false; value.horizontalOverflow = HorizontalWrapMode.Wrap; labels.Add(value); sizes.Add(font); return value; }
        Button Button(Transform parent, string name, Vector2 position, Vector2 size, string text, UnityEngine.Events.UnityAction action, Vector2? anchor = null)
        {
            var rect = Rect(parent, name, position, size, anchor); var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.14f, .25f, .34f, .98f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; if (action != null) button.onClick.AddListener(action);
            var label = Label(rect, "Label", Vector2.zero, size, 18); label.text = text; label.alignment = TextAnchor.MiddleCenter; hitAreas.Add(rect); return button;
        }
        void Build()
        {
            status = Label(canvas.transform, "ProgressionStatus", new Vector2(0, -125), new Vector2(830, 24), 16, new Vector2(.5f, 1)); status.alignment = TextAnchor.MiddleCenter;
            Button(canvas.transform, "Inventory", new Vector2(15, -310), new Vector2(190, 40), "INVENTÁRIO / I", OpenInventory);
            Button(canvas.transform, "RunResult", new Vector2(15, -355), new Vector2(190, 40), "RESULTADO", () => Show(Page.Result));
            Button(canvas.transform, "ProfileOptions", new Vector2(15, -400), new Vector2(190, 40), "OPÇÕES", () => Show(Page.Options));
            overlay = Rect(canvas.transform, "InventoryOverlay", Vector2.zero, Vector2.zero).gameObject; var shade = overlay.AddComponent<Image>(); shade.color = new Color(.015f, .025f, .045f, .96f); var full = (RectTransform)overlay.transform; full.anchorMin = Vector2.zero; full.anchorMax = Vector2.one; full.offsetMin = full.offsetMax = Vector2.zero;
            panel = Rect(overlay.transform, "InventoryPanel", Vector2.zero, new Vector2(1120, 650), Vector2.one * .5f); panel.gameObject.AddComponent<Image>().color = new Color(.035f, .06f, .085f);
            title = Label(panel, "Heading", new Vector2(20, -12), new Vector2(900, 32), 22);
            closeButton = Button(panel, "Close", new Vector2(990, -12), new Vector2(110, 35), "FECHAR", Close);
            for (int i = 0; i < 6; i++) { Page tab = (Page)i; Button(panel, "Tab_" + tab, new Vector2(20 + i * 181, -56), new Vector2(175, 42), PageName(tab), () => Tab(tab)); }
            for (int i = 0; i < rows.Length; i++) rows[i] = Button(panel, "ItemRow_" + i, new Vector2(20, -110 - i * 45), new Vector2(525, 42), "", null);
            Button(panel, "PreviousPage", new Vector2(20, -480), new Vector2(110, 40), "ANTERIOR", () => ChangePage(-1)); Button(panel, "NextPage", new Vector2(140, -480), new Vector2(110, 40), "PRÓXIMA", () => ChangePage(1)); counter = Label(panel, "Capacity", new Vector2(270, -485), new Vector2(275, 35));
            detail = Label(panel, "ItemDetails", new Vector2(570, -110), new Vector2(525, 230), 17); comparison = Label(panel, "Comparison", new Vector2(570, -345), new Vector2(525, 170), 17);
            primary = Button(panel, "ApplyItem", new Vector2(570, -525), new Vector2(170, 42), "EQUIPAR", Apply);
            secondary = Button(panel, "EquipSecondary", new Vector2(750, -525), new Vector2(170, 42), "SECUNDÁRIA", () => { bridge.Equip(selected, EquipmentSlot.OffHand); Refresh(); });
            discard = Button(panel, "DiscardItem", new Vector2(930, -525), new Vector2(165, 42), "DESCARTAR", DiscardRequest);
            resultSummary = Label(panel, "ResultSummary", new Vector2(20, -535), new Vector2(525, 100), 17); message = Label(panel, "Feedback", new Vector2(570, -578), new Vector2(525, 62), 17);
            confirmation = Rect(panel, "ConfirmDiscard", Vector2.zero, new Vector2(1120, 650), Vector2.one * .5f).gameObject; confirmation.AddComponent<Image>().color = new Color(.015f, .02f, .025f, .95f);
            var dialog = Rect(confirmation.transform, "Dialog", Vector2.zero, new Vector2(570, 220), Vector2.one * .5f); dialog.gameObject.AddComponent<Image>().color = new Color(.12f, .07f, .08f);
            Label(dialog, "Question", new Vector2(25, -25), new Vector2(525, 80), 22).text = "Descartar o item selecionado permanentemente?\nEsta ação exige confirmação.";
            confirmButton = Button(dialog, "Confirm", new Vector2(25, -130), new Vector2(245, 55), "SIM, DESCARTAR", ConfirmDiscard);
            cancelButton = Button(dialog, "Cancel", new Vector2(295, -130), new Vector2(245, 55), "CANCELAR", () => { pendingDiscard = null; confirmation.SetActive(false); });
            var navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnLeft = cancelButton, selectOnRight = cancelButton, selectOnUp = cancelButton, selectOnDown = cancelButton }; confirmButton.navigation = navigation;
            navigation.selectOnLeft = navigation.selectOnRight = navigation.selectOnUp = navigation.selectOnDown = confirmButton; cancelButton.navigation = navigation;
            confirmation.SetActive(false); overlay.SetActive(false);
        }
    }
}
