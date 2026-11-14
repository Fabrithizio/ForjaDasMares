using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // Forja do mestre artesão (inspired by DDTank): the bag opens beside it, items go to the slots by click or drag.
    // Weapon in the centre, four stones around it, luck and protection below, chance bar and animations.
    public sealed class ForgePanel : MonoBehaviour
    {
        const string LuckId = "sorte", SealId = "selo";
        static readonly Color GoldBorder = new Color(.86f, .66f, .32f, 1f);
        static readonly Color EmptyBorder = new Color(.55f, .45f, .3f, .55f);
        static readonly Color WinColor = new Color(1f, .82f, .35f), LoseColor = new Color(1f, .35f, .3f);

        RegionalInventory inventory;
        RectTransform full, window, forgeArea, strengthenView, fusionView, bagGrid, tooltip, effects;
        RoundedHudGraphic glowRing;
        ForgeSlot weaponSlot, luckSlot, sealSlot, fusionInput, fusionOutput;
        readonly ForgeSlot[] stoneSlots = new ForgeSlot[4];
        readonly List<ForgeSlot> bagSlots = new List<ForgeSlot>();
        Text chanceText, riskText, versionText, statusText, coinsText, tooltipText, bigText;
        RectTransform chanceFill;
        RoundedHudGraphic chanceFillGraphic;
        Button forgeButton, fuseButton, tabStrengthen, tabFusion, tabWeapons, tabMaterials;
        Texture2D[] stoneArt = new Texture2D[ForgeRules.StoneLevels];
        Texture2D luckArt, sealArt;

        string weaponId;
        int stoneLevel, stoneCount, fusionLevel;
        bool useLuck, useSeal, busy, fusionMode, materialsTab;

        static readonly Vector2 WindowSize = new Vector2(1070, 670);
        GameObject hiddenBehind;
        public bool IsOpen => gameObject.activeSelf;

        public static ForgePanel Build(Transform parent, RegionalInventory source)
        {
            var root = RusticHudTheme.Rect(parent, "Forja", Vector2.zero, Vector2.zero, Vector2.zero);
            root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            var shade = root.gameObject.AddComponent<Image>();
            shade.color = new Color(.005f, .01f, .015f, .6f);
            var panel = root.gameObject.AddComponent<ForgePanel>();
            panel.inventory = source;
            panel.full = root;
            panel.LoadArt();
            panel.BuildLayout();
            root.gameObject.SetActive(false);
            return panel;
        }

        void LoadArt()
        {
            for (int i = 0; i < stoneArt.Length; i++) stoneArt[i] = Resources.Load<Texture2D>("Forge/pedra_nv" + (i + 1));
            luckArt = Resources.Load<Texture2D>("Forge/amuleto_sorte");
            sealArt = Resources.Load<Texture2D>("Forge/selo_protecao");
        }

        // ---------- Layout ----------

        void BuildLayout()
        {
            window = RusticHudTheme.Rect(full, "Janela", Vector2.one * .5f, Vector2.zero, WindowSize);

            forgeArea = Box(window, "Forja", new Vector2(0, 0), new Vector2(600, 670));
            Title(forgeArea, "FORJA", "Mestre Artesão Téo");
            tabStrengthen = Tab(forgeArea, "AbaFortalecer", 330, "FORTALECER", () => SetMode(false));
            tabFusion = Tab(forgeArea, "AbaFusao", 460, "FUSÃO", () => SetMode(true));

            strengthenView = Area(forgeArea, "Fortalecer");
            var ringRect = RusticHudTheme.Rect(strengthenView, "Anel", new Vector2(0, 1), Vector2.zero, new Vector2(280, 280));
            ringRect.pivot = Vector2.one * .5f;
            ringRect.anchoredPosition = new Vector2(300, -340);
            var ring = ringRect.gameObject.AddComponent<RoundedHudGraphic>();
            ring.radius = 140; ring.border = 3; ring.color = new Color(.04f, .09f, .1f, .55f); ring.borderColor = new Color(.86f, .66f, .32f, .35f); ring.raycastTarget = false;
            var glowRect = RusticHudTheme.Rect(strengthenView, "Brilho", new Vector2(0, 1), new Vector2(300, -340), new Vector2(230, 230));
            glowRect.pivot = Vector2.one * .5f;
            glowRing = glowRect.gameObject.AddComponent<RoundedHudGraphic>();
            glowRing.radius = 115; glowRing.border = 0; glowRing.color = new Color(1f, .75f, .35f, 0f); glowRing.raycastTarget = false;

            weaponSlot = Slot(strengthenView, "Arma", 300 - 80, -340 + 80, 160, null);
            weaponSlot.clicked = _ => { if (!busy) { weaponId = null; Refresh(); } };
            Vector2[] stonePositions = { new Vector2(300, -200), new Vector2(160, -340), new Vector2(440, -340), new Vector2(300, -480) };
            for (int i = 0; i < 4; i++)
            {
                stoneSlots[i] = Slot(strengthenView, "Pedra" + i, stonePositions[i].x - 42, stonePositions[i].y + 42, 84, null);
                stoneSlots[i].clicked = _ => { if (!busy && stoneCount > 0) { stoneCount--; if (stoneCount == 0) stoneLevel = 0; Refresh(); } };
            }
            luckSlot = Slot(strengthenView, "Sorte", 34, -400, 76, "SORTE");
            luckSlot.clicked = _ => { if (!busy) { useLuck = false; Refresh(); } };
            sealSlot = Slot(strengthenView, "Selo", 490, -400, 76, "PROTEÇÃO");
            sealSlot.clicked = _ => { if (!busy) { useSeal = false; Refresh(); } };

            versionText = Label(strengthenView, "", 20, 124, 560, 22, 15, TextAnchor.MiddleCenter);
            versionText.color = RusticHudTheme.Muted;
            chanceText = Label(strengthenView, "", 40, 530, 520, 32, 24, TextAnchor.MiddleCenter);
            var barBack = RusticHudTheme.Rect(strengthenView, "BarraChance", new Vector2(0, 1), new Vector2(60, -566), new Vector2(480, 14));
            var back = RusticHudTheme.Panel(barBack, new Color(.02f, .04f, .05f, 1), 8); back.borderColor = new Color(.86f, .66f, .32f, .5f);
            chanceFill = RusticHudTheme.Rect(barBack, "Preenchimento", new Vector2(0, .5f), new Vector2(2, 0), new Vector2(0, 10));
            chanceFill.pivot = new Vector2(0, .5f);
            chanceFillGraphic = chanceFill.gameObject.AddComponent<RoundedHudGraphic>();
            chanceFillGraphic.radius = 6; chanceFillGraphic.border = 0; chanceFillGraphic.raycastTarget = false;
            riskText = Label(strengthenView, "", 40, 584, 520, 20, 14, TextAnchor.MiddleCenter);
            riskText.color = RusticHudTheme.Muted;
            forgeButton = RusticHudTheme.Button(strengthenView, "Forjar", new Vector2(0, 1), new Vector2(190, -612), new Vector2(220, 46), "FORJAR", () => { if (!busy) StartCoroutine(ForgeRoutine()); });
            forgeButton.GetComponentInChildren<Text>().fontSize = 22;
            forgeButton.GetComponent<RoundedHudGraphic>().borderColor = GoldBorder;

            fusionView = Area(forgeArea, "Fusao");
            Label(fusionView, "Junte " + ForgeRules.FusionCost + " pedras iguais para criar 1 pedra do nível seguinte.", 30, 120, 540, 26, 16, TextAnchor.MiddleCenter).color = RusticHudTheme.Muted;
            fusionInput = Slot(fusionView, "Entrada", 90, -210, 150, "PEDRAS");
            fusionInput.clicked = _ => { if (!busy) { fusionLevel = 0; Refresh(); } };
            Label(fusionView, "→", 255, 265, 90, 60, 44, TextAnchor.MiddleCenter).color = RusticHudTheme.Gold;
            fusionOutput = Slot(fusionView, "Saida", 360, -210, 150, "RESULTADO");
            fuseButton = RusticHudTheme.Button(fusionView, "Fundir", new Vector2(0, 1), new Vector2(190, -470), new Vector2(220, 52), "FUNDIR", Fuse);
            fuseButton.GetComponentInChildren<Text>().fontSize = 22;
            fuseButton.GetComponent<RoundedHudGraphic>().borderColor = GoldBorder;

            
            var bagArea = Box(window, "Mochila", new Vector2(610, 0), new Vector2(460, 670));
            Title(bagArea, "MOCHILA", "Clique ou arraste para a Forja");
            statusText = Label(bagArea, "", 20, 560, 420, 40, 15, TextAnchor.MiddleCenter);
            coinsText = Label(bagArea, "", 240, 20, 195, 30, 18, TextAnchor.MiddleRight);
            coinsText.color = RusticHudTheme.Gold;
            tabWeapons = Tab(bagArea, "AbaArmas", 25, "ARMAS", () => { materialsTab = false; Refresh(); });
            tabMaterials = Tab(bagArea, "AbaMateriais", 152, "MATERIAIS", () => { materialsTab = true; Refresh(); });
            bagGrid = RusticHudTheme.Rect(bagArea, "Grade", new Vector2(0, 1), new Vector2(18, -130), new Vector2(424, 424));
            for (int row = 0; row < 6; row++)
                for (int column = 0; column < 6; column++)
                {
                    var slot = Slot(bagGrid, "Item" + bagSlots.Count, column * 72, -row * 72, 64, null);
                    slot.fromBag = true;
                    slot.clicked = BagClicked;
                    slot.dropped = (source, target) => ReturnToBag(source);
                    bagSlots.Add(slot);
                }
            var close = RusticHudTheme.Button(bagArea, "Fechar", new Vector2(0, 1), new Vector2(305, -612), new Vector2(135, 46), "VOLTAR", Close);
            close.GetComponentInChildren<Text>().fontSize = 17;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var test = RusticHudTheme.Button(bagArea, "Teste", new Vector2(0, 1), new Vector2(18, -612), new Vector2(270, 46), "TESTE · GANHAR PEDRAS", () =>
            {
                if (busy) return;
                inventory.Service.GrantTestForgeMaterials();
                Status(inventory.Service.Message, RusticHudTheme.Muted);
                Refresh();
            });
            test.GetComponentInChildren<Text>().fontSize = 14;
#endif
            effects = RusticHudTheme.Rect(window, "Efeitos", Vector2.zero, Vector2.zero, Vector2.zero);
            effects.anchorMax = Vector2.one; effects.offsetMin = effects.offsetMax = Vector2.zero;
            effects.pivot = Vector2.one * .5f;
            bigText = RusticHudTheme.Label(effects, "Resultado", new Vector2(0, 1), new Vector2(300, -340), new Vector2(560, 120), "", 64, TextAnchor.MiddleCenter);
            ((RectTransform)bigText.transform).pivot = Vector2.one * .5f;
            var outline = bigText.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(0, 0, 0, .95f); outline.effectDistance = new Vector2(3, -3);
            bigText.fontStyle = FontStyle.Bold;

            tooltip = RusticHudTheme.Rect(window, "Detalhes", new Vector2(0, 1), Vector2.zero, new Vector2(330, 420));
            var tip = RusticHudTheme.Panel(tooltip, new Color(.02f, .045f, .055f, .98f), 14); tip.borderColor = GoldBorder; tip.border = 2;
            tooltipText = RusticHudTheme.Label(tooltip, "Texto", new Vector2(0, 1), new Vector2(16, -12), new Vector2(298, 396), "", 14, TextAnchor.UpperLeft);
            tooltipText.verticalOverflow = VerticalWrapMode.Truncate;
            tooltipText.horizontalOverflow = HorizontalWrapMode.Wrap;
            tooltipText.supportRichText = true;
            tooltip.gameObject.SetActive(false);
        }

        static RectTransform Box(RectTransform parent, string name, Vector2 position, Vector2 size)
        {
            var rect = RusticHudTheme.Rect(parent, name, new Vector2(0, 1), new Vector2(position.x, -position.y), size);
            var graphic = RusticHudTheme.Panel(rect, new Color(.025f, .06f, .075f, .98f), 22);
            graphic.gradient = true; graphic.topColor = new Color(.05f, .12f, .14f, .98f);
            graphic.borderColor = GoldBorder; graphic.border = 2.5f;
            return rect;
        }

        static RectTransform Area(RectTransform parent, string name)
        {
            var rect = RusticHudTheme.Rect(parent, name, Vector2.zero, Vector2.zero, Vector2.zero);
            rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        static void Title(RectTransform parent, string title, string subtitle)
        {
            Label(parent, title, 25, 14, 360, 40, 30, TextAnchor.MiddleLeft).color = RusticHudTheme.Gold;
            Label(parent, subtitle, 27, 50, 360, 22, 15, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;
        }

        Button Tab(RectTransform parent, string name, float x, string text, UnityEngine.Events.UnityAction action)
        {
            var button = RusticHudTheme.Button(parent, name, new Vector2(0, 1), new Vector2(x, -78), new Vector2(122, 38), text, action);
            button.GetComponentInChildren<Text>().fontSize = 14;
            return button;
        }

        ForgeSlot Slot(Transform parent, string name, float x, float y, float size, string label)
        {
            var slot = ForgeSlot.Create(parent, name, new Vector2(x, y), size, label);
            slot.dragLayer = full;
            slot.hovered = ShowDetails;
            slot.unhovered = _ => tooltip.gameObject.SetActive(false);
            if (!slot.fromBag) slot.dropped = (source, target) => { if (source.fromBag) BagClicked(source); };
            return slot;
        }

        static Text Label(RectTransform parent, string value, float x, float y, float width, float height, int font, TextAnchor align) =>
            RusticHudTheme.Label(parent, "Texto", new Vector2(0, 1), new Vector2(x, -y), new Vector2(width, height), value, font, align);

        // ---------- Open / close ----------

        public void Open()
        {
            if (inventory == null || inventory.Service == null) return;
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            window.localScale = Vector3.one * UiWindowScale.For(full, WindowSize);
            var behind = transform.parent != null ? transform.parent.Find("ShopPanel") : null;
            hiddenBehind = behind != null && behind.gameObject.activeSelf ? behind.gameObject : null;
            if (hiddenBehind != null) hiddenBehind.SetActive(false);
            busy = false;
            bigText.text = "";
            glowRing.color = new Color(1f, .75f, .35f, 0f);
            tooltip.gameObject.SetActive(false);
            if (weaponId == null) weaponId = EquippedWeaponId();
            Status("Coloque a arma no centro e as pedras em volta.", RusticHudTheme.Muted);
            SetMode(false);
        }

        public void Close()
        {
            if (busy) return;
            tooltip.gameObject.SetActive(false);
            gameObject.SetActive(false);
            if (hiddenBehind != null) hiddenBehind.SetActive(true);
            hiddenBehind = null;
        }

        void SetMode(bool fusion)
        {
            if (busy) return;
            fusionMode = fusion;
            if (fusion) materialsTab = true;
            strengthenView.gameObject.SetActive(!fusion);
            fusionView.gameObject.SetActive(fusion);
            Refresh();
        }

        string EquippedWeaponId()
        {
            var profile = inventory.Service.Profile;
            foreach (var set in profile.loadouts)
                if (set != null && set.items != null && set.items.Length > 0 && profile.Item(set.items[0]) != null && WeaponDefinition(profile.Item(set.items[0])) != null)
                    return set.items[0];
            return null;
        }

        EquipmentDefinition WeaponDefinition(ItemInstance item)
        {
            var definition = item != null ? inventory.Catalog.Find(item.definitionId) : null;
            return definition != null && definition.slot == EquipmentSlot.MainHand ? definition : null;
        }

        ItemInstance Weapon => weaponId == null ? null : inventory.Service.Profile.inventory.Find(x => x.instanceId == weaponId);

        // ---------- Interaction ----------

        void BagClicked(ForgeSlot slot)
        {
            if (busy || slot.content == null) return;
            var service = inventory.Service;
            if (slot.content is ItemInstance item)
            {
                if (WeaponDefinition(item) == null) return;
                weaponId = item.instanceId;
                SetMode(false);
                return;
            }
            if (slot.content is int level)
            {
                if (fusionMode)
                {
                    fusionLevel = level < ForgeRules.StoneLevels ? level : 0;
                    if (fusionLevel == 0) Status("Pedras Nv" + ForgeRules.StoneLevels + " já são as mais fortes.", RusticHudTheme.Muted);
                }
                else if (stoneLevel != level) { stoneLevel = level; stoneCount = 1; }
                else if (stoneCount < ForgeRules.MaxStonesPerTry && service.ForgeStoneCount(level) > stoneCount) stoneCount++;
                Refresh();
                return;
            }
            if (slot.content is string id && !fusionMode)
            {
                if (id == LuckId) useLuck = true;
                if (id == SealId) useSeal = true;
                Refresh();
            }
        }

        void ReturnToBag(ForgeSlot source)
        {
            if (busy || source.fromBag) return;
            source.clicked?.Invoke(source);
        }

        // ---------- Drawing ----------

        void Refresh()
        {
            var service = inventory.Service;
            var profile = service.Profile;
            if (Weapon == null) weaponId = null;
            if (stoneLevel > 0 && service.ForgeStoneCount(stoneLevel) < stoneCount) stoneCount = service.ForgeStoneCount(stoneLevel);
            if (stoneCount <= 0) { stoneCount = 0; stoneLevel = 0; }
            if (useLuck && profile.luckCharms <= 0) useLuck = false;
            if (useSeal && profile.protectionSeals <= 0) useSeal = false;
            if (fusionLevel > 0 && service.ForgeStoneCount(fusionLevel) < ForgeRules.FusionCost) fusionLevel = 0;
            coinsText.text = "Ouro  " + profile.coins;
            Highlight(tabStrengthen, !fusionMode); Highlight(tabFusion, fusionMode);
            Highlight(tabWeapons, !materialsTab); Highlight(tabMaterials, materialsTab);

            var weapon = Weapon;
            int level = weapon != null ? weapon.forgeLevel : 0;
            if (weapon == null) weaponSlot.SetEmpty(EmptyBorder);
            else weaponSlot.Show(weapon, WeaponTexture(weapon, out Rect uv), uv, RarityColor(weapon), "", "+" + level);
            for (int i = 0; i < 4; i++)
            {
                if (i < stoneCount) stoneSlots[i].Show(stoneLevel, stoneArt[stoneLevel - 1], ForgeSlot.FullUv, StoneColor(stoneLevel), "", "Nv" + stoneLevel);
                else stoneSlots[i].SetEmpty(EmptyBorder);
            }
            if (useLuck) luckSlot.Show(LuckId, luckArt, ForgeSlot.FullUv, GoldBorder); else luckSlot.SetEmpty(EmptyBorder);
            if (useSeal) sealSlot.Show(SealId, sealArt, ForgeSlot.FullUv, GoldBorder); else sealSlot.SetEmpty(EmptyBorder);

            float chance = weapon != null && stoneCount > 0 ? ForgeRules.Chance(level, stoneLevel, stoneCount, useLuck) : 0f;
            if (weapon == null) { versionText.text = "Escolha uma arma na mochila"; chanceText.text = ""; riskText.text = ""; }
            else if (level >= ForgeRules.MaxLevel) { versionText.text = "Nível máximo · forma final"; chanceText.text = "+12"; riskText.text = ""; }
            else
            {
                versionText.text = "+" + level + "  →  +" + (level + 1) + "      Forma " + ForgeRules.VisualVersion(level) + " de 3" +
                    (ForgeRules.VisualVersion(level + 1) != ForgeRules.VisualVersion(level) ? "  ·  muda de forma no +" + (level + 1) + "!" : "");
                if (stoneCount == 0) { chanceText.text = "Coloque pedras"; riskText.text = ""; }
                else if (chance <= 0) { chanceText.text = "Pedra fraca demais"; riskText.text = "Use pedras de nível maior para chegar ao +" + (level + 1) + "."; }
                else
                {
                    chanceText.text = "CHANCE  " + Percent(chance);
                    bool drops = ForgeRules.DropsOnFailure(level);
                    riskText.text = !drops ? "Se falhar: perde só as pedras" : useSeal ? "Se falhar: o Selo segura o nível" : "Se falhar: a arma cai para +" + (level - 1);
                }
            }
            chanceText.color = chance >= 60 ? new Color(.55f, .95f, .6f) : chance >= 25 ? RusticHudTheme.Gold : chance > 0 ? LoseColor : RusticHudTheme.Muted;
            chanceFill.sizeDelta = new Vector2(476f * Mathf.Clamp01(chance / 100f), 10);
            chanceFillGraphic.color = chanceText.color;
            forgeButton.interactable = !busy && weapon != null && level < ForgeRules.MaxLevel && chance > 0 && profile.expedition == null;

            if (fusionLevel > 0)
            {
                fusionInput.Show(fusionLevel, stoneArt[fusionLevel - 1], ForgeSlot.FullUv, StoneColor(fusionLevel), ForgeRules.FusionCost + "×", "Nv" + fusionLevel);
                fusionOutput.Show(fusionLevel + 1, stoneArt[fusionLevel], ForgeSlot.FullUv, StoneColor(fusionLevel + 1), "1×", "Nv" + (fusionLevel + 1), true);
            }
            else { fusionInput.SetEmpty(EmptyBorder); fusionOutput.SetEmpty(EmptyBorder); }
            fuseButton.interactable = !busy && fusionLevel > 0;

            FillBag();
        }

        void FillBag()
        {
            var service = inventory.Service;
            var profile = service.Profile;
            int index = 0;
            if (!materialsTab)
            {
                foreach (var item in profile.inventory)
                {
                    if (index >= bagSlots.Count) break;
                    if (WeaponDefinition(item) == null) continue;
                    bagSlots[index++].Show(item, WeaponTexture(item, out Rect uv), uv, RarityColor(item), "", item.forgeLevel > 0 ? "+" + item.forgeLevel : "", item.instanceId == weaponId);
                }
            }
            else
            {
                for (int level = 1; level <= ForgeRules.StoneLevels && index < bagSlots.Count; level++)
                {
                    int owned = service.ForgeStoneCount(level);
                    int reserved = !fusionMode && level == stoneLevel ? stoneCount : fusionMode && level == fusionLevel ? ForgeRules.FusionCost : 0;
                    if (owned <= 0) continue;
                    bagSlots[index++].Show(level, stoneArt[level - 1], ForgeSlot.FullUv, StoneColor(level), (owned - reserved).ToString(), "Nv" + level, owned - reserved <= 0);
                }
                if (profile.luckCharms > 0 && index < bagSlots.Count)
                    bagSlots[index++].Show(LuckId, luckArt, ForgeSlot.FullUv, GoldBorder, (profile.luckCharms - (useLuck ? 1 : 0)).ToString(), "", useLuck && profile.luckCharms == 1);
                if (profile.protectionSeals > 0 && index < bagSlots.Count)
                    bagSlots[index++].Show(SealId, sealArt, ForgeSlot.FullUv, GoldBorder, (profile.protectionSeals - (useSeal ? 1 : 0)).ToString(), "", useSeal && profile.protectionSeals == 1);
            }
            for (; index < bagSlots.Count; index++) bagSlots[index].SetEmpty(new Color(.35f, .3f, .22f, .45f));
        }

        void Highlight(Button button, bool active)
        {
            var graphic = button.GetComponent<RoundedHudGraphic>();
            graphic.borderColor = active ? GoldBorder : new Color(.49f, .38f, .23f, .6f);
            graphic.border = active ? 2.5f : 1.5f;
            graphic.SetVerticesDirty();
            button.GetComponentInChildren<Text>().color = active ? RusticHudTheme.Gold : RusticHudTheme.Muted;
        }

        Texture WeaponTexture(ItemInstance item, out Rect uv)
        {
            var definition = inventory.Catalog.Find(item.definitionId);
            uv = ForgeSlot.SpriteUv(definition != null ? definition.icon : null);
            return definition != null && definition.icon != null ? definition.icon.texture : null;
        }

        Color RarityColor(ItemInstance item)
        {
            var rarity = inventory.Catalog.Rarity(item.rarity);
            var color = rarity != null ? rarity.color : GoldBorder;
            color.a = 1;
            return color;
        }

        static Color StoneColor(int level) =>
            level == 1 ? new Color(.45f, .85f, .5f) : level == 2 ? new Color(.45f, .85f, .95f) : level == 3 ? new Color(1f, .5f, .62f) : level == 4 ? new Color(.68f, .45f, 1f) : new Color(1f, .78f, .35f);

        void ShowDetails(ForgeSlot slot)
        {
            if (busy) return;
            string text = null;
            if (slot.content is ItemInstance item)
            {
                text = "<b>FORJA +" + item.forgeLevel + "</b>  ·  forma " + ForgeRules.VisualVersion(item.forgeLevel) + " de 3\n\n" + ItemDescriptions.Summary(inventory.Catalog, item);
            }
            else if (slot.content is int level)
            {
                text = "<b>PEDRA DA FORJA NV" + level + "</b>\n\nChance de cada pedra:\n";
                int[] targets = { 3, 6, 9, 11, 12 };
                string[] names = { "+1 a +3", "+4 a +6", "+7 a +9", "+10 e +11", "+12" };
                for (int i = 0; i < targets.Length; i++)
                {
                    float value = ForgeRules.StoneValue(targets[i], level);
                    text += names[i] + ":  " + (value > 0 ? Percent(value) : "não serve") + "\n";
                }
                text += "\nUse até " + ForgeRules.MaxStonesPerTry + " por tentativa.\nFusão: " + ForgeRules.FusionCost + " iguais viram 1 do nível seguinte.";
            }
            else if (slot.content is string id)
                text = id == LuckId ? "<b>AMULETO DA SORTE</b>\n\nSoma +5% de chance a uma tentativa da Forja.\nÉ gasto ao forjar."
                    : "<b>SELO DE PROTEÇÃO</b>\n\nSe a Forja falhar a partir do +" + ForgeRules.DropFromLevel + ", a arma NÃO cai de nível.\nSó é gasto quando salva a arma.";
            if (text == null) return;
            tooltipText.text = text;
            var corners = new Vector3[4];
            slot.Rect.GetWorldCorners(corners);
            Vector2 local = window.InverseTransformPoint(corners[2]);
            float x = local.x + window.rect.width * .5f + 12;
            float y = local.y - window.rect.height * .5f;
            if (x + tooltip.sizeDelta.x > window.rect.width) x = local.x + window.rect.width * .5f - slot.Rect.rect.width - tooltip.sizeDelta.x - 12;
            y = Mathf.Clamp(y, -window.rect.height + tooltip.sizeDelta.y, 0);
            tooltip.anchoredPosition = new Vector2(Mathf.Max(0, x), y);
            tooltip.SetAsLastSibling();
            tooltip.gameObject.SetActive(true);
        }

        void Status(string text, Color color)
        {
            statusText.text = text;
            statusText.color = color;
        }

        static string Percent(float value) => (Mathf.Abs(value - Mathf.Round(value)) < .01f ? Mathf.RoundToInt(value).ToString() : value.ToString("0.0")) + "%";

        // ---------- Animations ----------

        IEnumerator ForgeRoutine()
        {
            var weapon = Weapon;
            if (weapon == null || stoneCount == 0) yield break;
            busy = true;
            tooltip.gameObject.SetActive(false);
            forgeButton.interactable = false;
            int before = weapon.forgeLevel;
            Status("Forjando...", RusticHudTheme.Gold);

            // Stones fly into the weapon while the glow grows and the weapon trembles.
            var center = weaponSlot.Rect;
            Vector2 home = center.anchoredPosition;
            for (int i = 0; i < stoneCount; i++) StartCoroutine(Fly(stoneSlots[i], center, .45f + i * .08f));
            if (useLuck) StartCoroutine(Fly(luckSlot, center, .6f));
            float time = 0, duration = 1.35f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float k = time / duration;
                glowRing.color = new Color(1f, .75f, .35f, Mathf.Lerp(.05f, .75f, k) * (.75f + .25f * Mathf.Sin(time * 30)));
                glowRing.rectTransform.localScale = Vector3.one * (1f + .08f * Mathf.Sin(time * 18));
                center.anchoredPosition = home + Random.insideUnitCircle * (2f + 5f * k);
                if (Random.value < .45f) Spark(CenterOf(center), new Color(1f, Random.Range(.6f, .9f), .3f), 6, 160, .5f);
                yield return null;
            }
            center.anchoredPosition = home;

            var service = inventory.Service;
            bool saved = service.Forge(weapon.instanceId, stoneLevel, stoneCount, useLuck, useSeal, out bool success);
            var after = Weapon;
            int now = after != null ? after.forgeLevel : before;
            if (saved) { stoneCount = Mathf.Min(stoneCount, service.ForgeStoneCount(stoneLevel)); useLuck = false; if (now == before && ForgeRules.DropsOnFailure(before) && useSeal) useSeal = false; }
            Refresh();
            if (!saved) { glowRing.color = new Color(1, .75f, .35f, 0); Status(service.Message, RusticHudTheme.Muted); busy = false; Refresh(); yield break; }

            string roll = "";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            roll = "\n<size=12>Teste · sorteio " + service.LastForgeRoll.ToString("0.0") + " · precisava ser menor que " + service.LastForgeChance.ToString("0.0") + "</size>";
#endif
            if (success)
            {
                Status(service.Message + roll, WinColor);
                yield return Result("+" + now, WinColor, true, ForgeRules.VisualVersion(now) != ForgeRules.VisualVersion(before) ? "NOVA FORMA!" : null);
            }
            else
            {
                Status(service.Message + roll, LoseColor);
                yield return Result(now < before ? "FALHOU  −1" : "FALHOU", LoseColor, false, null);
            }
            busy = false;
            Refresh();
        }

        IEnumerator Result(string text, Color color, bool success, string extra)
        {
            Vector2 point = CenterOf(weaponSlot.Rect);
            var flash = Circle(point, 40, color);
            int particles = success ? 34 : 22;
            for (int i = 0; i < particles; i++)
                Spark(point, success ? new Color(1f, Random.Range(.7f, 1f), Random.Range(.3f, .6f)) : new Color(Random.Range(.3f, .6f), .12f, .1f, .9f),
                    success ? Random.Range(6, 12) : Random.Range(14, 26), success ? Random.Range(200, 380) : Random.Range(60, 140), success ? .9f : 1.3f, !success);
            bigText.text = text + (extra != null ? "\n<size=30>" + extra + "</size>" : "");
            bigText.color = color;
            Vector2 windowHome = window.anchoredPosition;
            float time = 0, duration = 1.1f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(time / duration);
                float pop = k < .25f ? Mathf.Lerp(.3f, 1.25f, k / .25f) : Mathf.Lerp(1.25f, 1f, (k - .25f) / .25f);
                bigText.rectTransform.localScale = Vector3.one * Mathf.Max(1f, pop);
                if (k < .25f) bigText.rectTransform.localScale = Vector3.one * pop;
                flash.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, success ? 9f : 6f, k);
                var c = color; c.a = (1 - k) * .55f; flash.color = c;
                glowRing.color = new Color(color.r, color.g, color.b, (1 - k) * .8f);
                if (!success) window.anchoredPosition = windowHome + (k < .4f ? Random.insideUnitCircle * 9f * (1 - k / .4f) : Vector2.zero);
                yield return null;
            }
            window.anchoredPosition = windowHome;
            Destroy(flash.gameObject);
            glowRing.color = new Color(1, .75f, .35f, 0);
            yield return new WaitForSecondsRealtime(.7f);
            float fade = 0;
            while (fade < .35f) { fade += Time.unscaledDeltaTime; var c = bigText.color; c.a = 1 - fade / .35f; bigText.color = c; yield return null; }
            bigText.text = "";
        }

        IEnumerator Fly(ForgeSlot from, RectTransform to, float duration)
        {
            if (from.picture.texture == null) yield break;
            var copy = RusticHudTheme.Rect(effects, "Voando", new Vector2(0, 1), Vector2.zero, from.Rect.sizeDelta * .8f).gameObject.AddComponent<RawImage>();
            copy.texture = from.picture.texture; copy.uvRect = from.picture.uvRect; copy.raycastTarget = false;
            copy.rectTransform.pivot = Vector2.one * .5f;
            Vector2 start = CenterOf(from.Rect), end = CenterOf(to);
            from.picture.color = new Color(1, 1, 1, .25f);
            float time = 0;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float k = Mathf.SmoothStep(0, 1, time / duration);
                copy.rectTransform.anchoredPosition = Vector2.Lerp(start, end, k) + Vector2.up * Mathf.Sin(k * Mathf.PI) * 40;
                copy.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, .3f, k);
                copy.color = new Color(1, 1, 1, 1 - k * .7f);
                yield return null;
            }
            Destroy(copy.gameObject);
        }

        // Position of a rect's centre inside the effects layer (top-left anchored coordinates).
        Vector2 CenterOf(RectTransform rect)
        {
            Vector3 world = rect.TransformPoint(rect.rect.center);
            Vector2 local = effects.InverseTransformPoint(world);
            return new Vector2(local.x + effects.rect.width * .5f, local.y - effects.rect.height * .5f);
        }

        RoundedHudGraphic Circle(Vector2 point, float size, Color color)
        {
            var rect = RusticHudTheme.Rect(effects, "Particula", new Vector2(0, 1), point, Vector2.one * size);
            rect.pivot = Vector2.one * .5f;
            var graphic = rect.gameObject.AddComponent<RoundedHudGraphic>();
            graphic.radius = size * .5f; graphic.border = 0; graphic.color = color; graphic.raycastTarget = false;
            return graphic;
        }

        void Spark(Vector2 point, Color color, float size, float speed, float life, bool rises = false)
        {
            var graphic = Circle(point, size, color);
            Vector2 direction = rises ? new Vector2(Random.Range(-.6f, .6f), 1f).normalized : Random.insideUnitCircle.normalized;
            StartCoroutine(Move(graphic, direction * speed, life, rises));
        }

        IEnumerator Move(RoundedHudGraphic graphic, Vector2 velocity, float life, bool grows)
        {
            float time = 0;
            var start = graphic.color;
            while (time < life && graphic != null)
            {
                time += Time.unscaledDeltaTime;
                float k = time / life;
                graphic.rectTransform.anchoredPosition += velocity * Time.unscaledDeltaTime * (1 - k * .6f);
                if (grows) graphic.rectTransform.localScale = Vector3.one * (1 + k * 1.5f);
                var c = start; c.a = start.a * (1 - k); graphic.color = c;
                yield return null;
            }
            if (graphic != null) Destroy(graphic.gameObject);
        }

        void Fuse()
        {
            if (busy || fusionLevel == 0) return;
            StartCoroutine(FuseRoutine());
        }

        IEnumerator FuseRoutine()
        {
            busy = true;
            int level = fusionLevel;
            StartCoroutine(Fly(fusionInput, fusionOutput.Rect, .55f));
            yield return new WaitForSecondsRealtime(.55f);
            var service = inventory.Service;
            bool done = service.FuseStones(level);
            Status(service.Message, done ? WinColor : RusticHudTheme.Muted);
            if (done)
            {
                Vector2 point = CenterOf(fusionOutput.Rect);
                for (int i = 0; i < 22; i++) Spark(point, StoneColor(level + 1), Random.Range(6, 11), Random.Range(160, 300), .8f);
            }
            fusionInput.picture.color = Color.white;
            busy = false;
            Refresh();
        }
    }
}
