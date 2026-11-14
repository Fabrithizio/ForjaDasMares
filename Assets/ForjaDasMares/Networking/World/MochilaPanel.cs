using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // Personagem e Mochila (RPG regional). Opens on the character with the equipment around it and the
    // inventory beside it. Status, Talentos, Runas and Recursos are tabs.
    // Same visual kit as the Forja (ForgeSlot, rounded gold frames, UiWindowScale).
    public sealed class MochilaPanel : MonoBehaviour
    {
        static readonly Color GoldBorder = new Color(.86f, .66f, .32f, 1f);
        static readonly Color EmptyBorder = new Color(.55f, .45f, .3f, .55f);
        static readonly Color LockedBorder = new Color(.35f, .32f, .28f, .6f);
        static readonly Color Cyan = new Color(.4f, .9f, .95f);
        static readonly Vector2 WindowSize = new Vector2(1180, 760);

        // Equipment spaces shown around the character. slot == null means "not created yet" (locked).
        sealed class EquipSpace
        {
            public string label; public EquipmentSlot? slot; public HudSymbol symbol;
            public EquipSpace(string l, EquipmentSlot? s, HudSymbol h) { label = l; slot = s; symbol = h; }
        }
        static readonly EquipSpace[] Spaces =
        {
            new EquipSpace("Capacete", null, HudSymbol.Hero),
            new EquipSpace("Armadura", EquipmentSlot.Armor, HudSymbol.Armor),
            new EquipSpace("Luvas", null, HudSymbol.Shield),
            new EquipSpace("Botas", null, HudSymbol.Dodge),
            new EquipSpace("Arma", EquipmentSlot.MainHand, HudSymbol.Sword),
            new EquipSpace("Acessório", EquipmentSlot.Accessory, HudSymbol.Accessory),
            new EquipSpace("Acessório 2", null, HudSymbol.Accessory),
            new EquipSpace("Artefato", EquipmentSlot.Artifact, HudSymbol.Artifact),
        };
        static readonly string[] FilterNames = { "TODOS", "ARMAS", "ARMADURAS", "ACESSÓRIOS", "ARTEFATOS" };
        static readonly EquipmentSlot?[] FilterSlots = { null, EquipmentSlot.MainHand, EquipmentSlot.Armor, EquipmentSlot.Accessory, EquipmentSlot.Artifact };
        static readonly string[] SortNames = { "RARIDADE", "NÍVEL", "TIPO" };
        const int Columns = 7, Rows = 6;

        RegionalInventory inventory;
        Action<bool> blockInput;
        Action<int> openLegacy;
        RectTransform full, window, equipPage, statusPage, resourcePage, talentPage, runePage, tooltip;
        // Talentos
        Text talentPoints, talentFeedback, resetLabel;
        Button resetButton;
        readonly Text[] talentRanks = new Text[6], talentTotals = new Text[6];
        readonly Button[] talentButtons = new Button[6];
        readonly RoundedHudGraphic[][] talentPips = new RoundedHudGraphic[6][];
        bool pendingReset;
        // Runas
        ForgeSlot runeSocket, runeStored;
        HudIcon runeSocketIcon, runeStoredIcon, runeBigIcon;
        Text runeTitle, runeLevel, runeFeedback, runeEquipLabel, runeImproveLabel;
        readonly Text[] runeLevelLines = new Text[3];
        readonly RoundedHudGraphic[] runePips = new RoundedHudGraphic[3];
        Button runeEquip, runeImprove;
        Text tooltipText, nameText, levelText, xpText, countText, goldText, noticeText, actionLabel, discardLabel, sortLabel;
        RectTransform xpFill;
        readonly ForgeSlot[] equipSlots = new ForgeSlot[Spaces.Length];
        readonly List<ForgeSlot> bagSlots = new List<ForgeSlot>();
        readonly Button[] pageTabs = new Button[5];
        readonly Button[] filterTabs = new Button[FilterNames.Length];
        readonly Text[] chipValues = new Text[6];
        readonly List<Text> statValues = new List<Text>();
        readonly Text[] stoneCounts = new Text[ForgeRules.StoneLevels];
        Text luckCount, sealCount, deliveryCount, resourceGold;
        Button actionButton, discardButton, claimButton;
        RawImage portrait;
        EquipmentPaperDoll paperDoll;
        readonly List<ItemInstance> shown = new List<ItemInstance>();

        int page, filter, sort, lastRevision = -1;
        string selected, pendingDiscard, lastNotice;
        float lastClickTime; string lastClickId;

        public bool IsOpen => gameObject.activeSelf;

        public static MochilaPanel Build(Transform parent, RegionalInventory source, Action<bool> block, Action<int> legacy)
        {
            var root = RusticHudTheme.Rect(parent, "Mochila", Vector2.zero, Vector2.zero, Vector2.zero);
            root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            root.gameObject.AddComponent<Image>().color = new Color(.005f, .01f, .015f, .62f);
            var panel = root.gameObject.AddComponent<MochilaPanel>();
            panel.inventory = source; panel.full = root; panel.blockInput = block; panel.openLegacy = legacy;
            panel.BuildLayout();
            root.gameObject.SetActive(false);
            return panel;
        }

        // ---------- Layout ----------

        void BuildLayout()
        {
            window = RusticHudTheme.Rect(full, "Janela", Vector2.one * .5f, Vector2.zero, WindowSize);

            var header = Box(window, "Cabecalho", 0, 0, 1180, 70, 18);
            RusticHudTheme.Icon(header, HudSymbol.Hero, Top, new Vector2(20, -13), 44);
            Label(header, "PERSONAGEM", 74, 8, 260, 34, 28, TextAnchor.MiddleLeft).color = RusticHudTheme.Gold;
            Label(header, "Equipamentos, status e recursos", 76, 40, 260, 20, 13, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;
            string[] tabs = { "EQUIPAMENTO", "STATUS", "TALENTOS", "RUNAS", "RECURSOS" };
            HudSymbol[] tabIcons = { HudSymbol.Sword, HudSymbol.Scroll, HudSymbol.Fury, HudSymbol.Rune, HudSymbol.Coin };
            for (int i = 0; i < tabs.Length; i++)
            {
                int pick = i;
                pageTabs[i] = RusticHudTheme.Button(header, "Aba" + i, Top, new Vector2(340 + i * 150, -13), new Vector2(144, 44), tabs[i], () => SetPage(pick));
                var text = pageTabs[i].GetComponentInChildren<Text>();
                text.fontSize = 14; text.rectTransform.anchoredPosition = new Vector2(14, 0); text.rectTransform.sizeDelta = new Vector2(110, 40);
                RusticHudTheme.Icon(pageTabs[i].transform, tabIcons[i], new Vector2(0, .5f), new Vector2(10, 0), 22);
            }
            var close = RusticHudTheme.Button(header, "Fechar", Top, new Vector2(1118, -13), new Vector2(46, 44), "X", Close);
            close.GetComponentInChildren<Text>().fontSize = 20;

            BuildEquipPage();
            BuildStatusPage();
            BuildTalentPage();
            BuildRunePage();
            BuildResourcePage();

            noticeText = Label(window, "", 20, 726, 1140, 28, 16, TextAnchor.MiddleCenter);
            noticeText.color = RusticHudTheme.Parchment;
            var shadow = noticeText.gameObject.AddComponent<Outline>(); shadow.effectColor = new Color(0, 0, 0, .9f);

            tooltip = RusticHudTheme.Rect(window, "Detalhes", Top, Vector2.zero, new Vector2(330, 430));
            var tip = RusticHudTheme.Panel(tooltip, new Color(.02f, .045f, .055f, .98f), 14); tip.borderColor = GoldBorder; tip.border = 2;
            tooltipText = RusticHudTheme.Label(tooltip, "Texto", Top, new Vector2(16, -12), new Vector2(298, 406), "", 14, TextAnchor.UpperLeft);
            tooltipText.horizontalOverflow = HorizontalWrapMode.Wrap; tooltipText.supportRichText = true;
            tooltip.gameObject.SetActive(false);
        }

        void BuildEquipPage()
        {
            equipPage = Area(window, "Equipamento");

            // Left: the character, framed by the equipment.
            var hero = Box(equipPage, "Heroi", 0, 80, 580, 640, 22);
            var art = MenuArtwork.Fill(hero, "Fundo", "MenuArt/RoutesBackdrop", new Color(.55f, .62f, .7f, 1));
            art.rectTransform.offsetMin = new Vector2(6, 176); art.rectTransform.offsetMax = new Vector2(-6, -6);
            art.transform.SetAsFirstSibling(); hero.Find("RusticSurface")?.SetAsFirstSibling();
            var portraitRect = RusticHudTheme.Rect(hero, "Personagem", Top, new Vector2(125, -10), new Vector2(330, 440));
            portrait = portraitRect.gameObject.AddComponent<RawImage>(); portrait.raycastTarget = false;
            paperDoll = portraitRect.gameObject.AddComponent<EquipmentPaperDoll>();
            paperDoll.Configure(portrait);
            paperDoll.SetBackdrop(new Color(0, 0, 0, 0));

            for (int i = 0; i < Spaces.Length; i++)
            {
                bool right = i >= 4; int row = i % 4;
                var slot = Slot(hero, "Equip_" + Spaces[i].label, right ? 470 : 22, -(18 + row * 112), 88, Spaces[i].label);
                int index = i;
                slot.clicked = s => { if (s.HasContent) Select((string)s.content, true); };
                slot.dropped = (source, target) => { if (source.fromBag) EquipFromBag((string)source.content, index); };
                equipSlots[i] = slot;
                if (Spaces[i].slot == null) DrawLock(slot.Rect);
                else
                {
                    var hint = RusticHudTheme.Icon(slot.Rect, Spaces[i].symbol, Vector2.one * .5f, Vector2.zero, 42);
                    hint.name = "Dica"; hint.color = new Color(.6f, .55f, .4f, .35f); hint.transform.SetSiblingIndex(1);
                }
            }

            // Identity and the main status under the character.
            var band = RusticHudTheme.Rect(hero, "Identidade", Top, new Vector2(6, -468), new Vector2(568, 166));
            var bandBack = RusticHudTheme.Panel(band, new Color(.02f, .045f, .055f, .96f), 16); bandBack.border = 0;
            nameText = Label(band, "", 18, 8, 330, 30, 22, TextAnchor.MiddleLeft); nameText.color = RusticHudTheme.Gold;
            levelText = Label(band, "", 350, 8, 200, 30, 18, TextAnchor.MiddleRight); levelText.color = Cyan;
            var track = RusticHudTheme.Rect(band, "BarraXp", Top, new Vector2(18, -44), new Vector2(532, 8));
            var trackBack = RusticHudTheme.Panel(track, new Color(.06f, .1f, .12f, 1), 4); trackBack.border = 0;
            xpFill = RusticHudTheme.Rect(track, "Preenchimento", new Vector2(0, .5f), Vector2.zero, new Vector2(0, 8)); xpFill.pivot = new Vector2(0, .5f);
            var fill = xpFill.gameObject.AddComponent<RoundedHudGraphic>(); fill.radius = 4; fill.border = 0; fill.color = new Color(.25f, .75f, 1f); fill.raycastTarget = false;
            xpText = Label(band, "", 18, 54, 532, 16, 12, TextAnchor.MiddleRight); xpText.color = RusticHudTheme.Muted;
            string[] chipNames = { "Vida", "Dano", "Defesa", "Crítico", "Ataques/s", "Movimento" };
            HudSymbol[] chipIcons = { HudSymbol.Heart, HudSymbol.Sword, HudSymbol.Shield, HudSymbol.Target, HudSymbol.Hourglass, HudSymbol.Dodge };
            for (int i = 0; i < 6; i++)
            {
                int column = i % 3, row = i / 3;
                var chip = RusticHudTheme.Rect(band, "Status" + i, Top, new Vector2(14 + column * 182, -76 - row * 44), new Vector2(174, 38));
                var back = RusticHudTheme.Panel(chip, new Color(.04f, .08f, .095f, 1), 10); back.borderColor = new Color(.86f, .66f, .32f, .3f); back.border = 1;
                RusticHudTheme.Icon(chip, chipIcons[i], new Vector2(0, .5f), new Vector2(8, 0), 22);
                Label(chip, chipNames[i], 36, 0, 80, 38, 13, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;
                chipValues[i] = Label(chip, "", 96, 0, 70, 38, 16, TextAnchor.MiddleRight);
                chipValues[i].fontStyle = FontStyle.Bold;
            }

            // Right: inventory.
            var bag = Box(equipPage, "Inventario", 590, 80, 590, 640, 22);
            RusticHudTheme.Icon(bag, HudSymbol.Bag, Top, new Vector2(20, -14), 36);
            Label(bag, "INVENTÁRIO", 64, 10, 220, 30, 24, TextAnchor.MiddleLeft).color = RusticHudTheme.Gold;
            Label(bag, "Itens que o personagem carrega", 66, 38, 260, 18, 13, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;
            countText = Label(bag, "", 300, 12, 110, 30, 18, TextAnchor.MiddleRight); countText.color = RusticHudTheme.Parchment;
            RusticHudTheme.Icon(bag, HudSymbol.Coin, Top, new Vector2(430, -17), 24);
            goldText = Label(bag, "", 458, 12, 112, 30, 17, TextAnchor.MiddleLeft); goldText.color = RusticHudTheme.Gold;
            for (int i = 0; i < FilterNames.Length; i++)
            {
                int pick = i;
                filterTabs[i] = RusticHudTheme.Button(bag, "Filtro" + i, Top, new Vector2(20 + i * 111, -66), new Vector2(105, 34), FilterNames[i], () => { filter = pick; page = 0; Refresh(); });
                filterTabs[i].GetComponentInChildren<Text>().fontSize = 12;
            }
            // The grid grows with the bag capacity; extra rows scroll.
            int rows = Mathf.Max(Rows, Mathf.CeilToInt(inventory.Catalog.inventoryCapacity / (float)Columns));
            var viewport = RusticHudTheme.Rect(bag, "Visor", Top, new Vector2(29, -112), new Vector2(Columns * 76, Rows * 76));
            viewport.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            viewport.gameObject.AddComponent<RectMask2D>();
            var grid = RusticHudTheme.Rect(viewport, "Grade", Top, Vector2.zero, new Vector2(Columns * 76, rows * 76));
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport; scroll.content = grid; scroll.horizontal = false; scroll.vertical = rows > Rows;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 30;
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < Columns; column++)
                {
                    var slot = Slot(grid, "Item" + bagSlots.Count, column * 76, -row * 76, 68, null);
                    slot.fromBag = true;
                    slot.clicked = BagClicked;
                    slot.dropped = (source, target) => { if (!source.fromBag) Unequip((string)source.content); };
                    bagSlots.Add(slot);
                }

            actionButton = ActionButton(bag, "Equipar", 20, 186, HudSymbol.Sword, Act, out actionLabel);
            var sortButton = ActionButton(bag, "Ordenar", 214, 182, HudSymbol.Scroll, () => { sort = (sort + 1) % SortNames.Length; Refresh(); }, out sortLabel);
            discardButton = ActionButton(bag, "Descartar", 404, 166, HudSymbol.Coin, Discard, out discardLabel);
            sortButton.name = "Ordenar";
        }

        void BuildStatusPage()
        {
            statusPage = Area(window, "Status");
            var box = Box(statusPage, "Quadro", 0, 80, 1180, 640, 22);
            Label(box, "VISÃO GERAL DO PERSONAGEM", 26, 14, 600, 32, 22, TextAnchor.MiddleLeft).color = RusticHudTheme.Gold;
            Label(box, "Valores atuais com equipamento, talentos e nível.", 28, 44, 600, 20, 14, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;
            HudSymbol[] icons = { HudSymbol.Heart, HudSymbol.Sword, HudSymbol.SpellBurst, HudSymbol.Target, HudSymbol.Hourglass, HudSymbol.Shield, HudSymbol.Dodge, HudSymbol.Compass, HudSymbol.SpellBolt, HudSymbol.Fury, HudSymbol.Artifact, HudSymbol.Hero };
            string[] names = { "Vida máxima", "Dano por golpe", "Dano crítico", "Chance de crítico", "Ataques por segundo", "Defesa", "Velocidade", "Recarga de habilidades", "Dano elemental", "Recuperação", "Bloqueio frontal", "Forja da arma" };
            string[] notes = { "Quanto dano você aguenta.", "Golpe normal da arma equipada.", "Dano quando o golpe é crítico.", "Chance de cada golpe ser crítico.", "Velocidade dos ataques básicos.", "Dano recebido que é reduzido.", "Velocidade de movimento.", "Habilidades voltam mais rápido.", "Bônus de dano mágico.", "Vida recuperada com o tempo.", "Reduz golpes vindos de frente.", "Nível da arma na Forja do Téo." };
            for (int i = 0; i < names.Length; i++)
            {
                int column = i % 2, row = i / 2;
                var line = RusticHudTheme.Rect(box, "Linha" + i, Top, new Vector2(24 + column * 570, -80 - row * 88), new Vector2(560, 78));
                var back = RusticHudTheme.Panel(line, new Color(.04f, .08f, .095f, 1), 14); back.borderColor = new Color(.86f, .66f, .32f, .3f); back.border = 1;
                RusticHudTheme.Icon(line, icons[i], new Vector2(0, .5f), new Vector2(16, 0), 38);
                Label(line, names[i], 70, 10, 300, 28, 19, TextAnchor.MiddleLeft);
                Label(line, notes[i], 70, 40, 320, 24, 13, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;
                var value = Label(line, "", 380, 0, 164, 78, 24, TextAnchor.MiddleRight);
                value.color = Cyan; value.fontStyle = FontStyle.Bold;
                statValues.Add(value);
            }
        }

        void BuildResourcePage()
        {
            resourcePage = Area(window, "Recursos");
            var box = Box(resourcePage, "Quadro", 0, 80, 1180, 640, 22);
            Label(box, "RECURSOS E MOEDAS", 26, 14, 600, 32, 22, TextAnchor.MiddleLeft).color = RusticHudTheme.Gold;
            Label(box, "Moedas e materiais guardados pelo personagem.", 28, 44, 600, 20, 14, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;

            var gold = RusticHudTheme.Rect(box, "Ouro", Top, new Vector2(24, -84), new Vector2(360, 110));
            var goldBack = RusticHudTheme.Panel(gold, new Color(.13f, .095f, .04f, 1), 16); goldBack.borderColor = GoldBorder; goldBack.border = 2;
            RusticHudTheme.Icon(gold, HudSymbol.Coin, new Vector2(0, .5f), new Vector2(22, 0), 52);
            resourceGold = Label(gold, "", 92, 18, 250, 44, 34, TextAnchor.MiddleLeft); resourceGold.color = RusticHudTheme.Gold;
            Label(gold, "Ouro", 94, 62, 250, 24, 16, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;

            var deliveries = RusticHudTheme.Rect(box, "Entregas", Top, new Vector2(400, -84), new Vector2(360, 110));
            var deliveryBack = RusticHudTheme.Panel(deliveries, new Color(.04f, .08f, .095f, 1), 16); deliveryBack.borderColor = new Color(.86f, .66f, .32f, .4f);
            RusticHudTheme.Icon(deliveries, HudSymbol.Bag, new Vector2(0, .5f), new Vector2(22, 0), 52);
            deliveryCount = Label(deliveries, "", 92, 18, 250, 44, 30, TextAnchor.MiddleLeft);
            Label(deliveries, "Itens em ENTREGAS (mochila cheia)", 94, 62, 260, 24, 14, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;
            claimButton = RusticHudTheme.Button(box, "PegarEntregas", Top, new Vector2(776, -116), new Vector2(240, 46), "PEGAR ENTREGAS",
                () => inventory.Request(CoopInventoryAction.ClaimAllDeliveries, null, EquipmentSlot.MainHand, false, 0));

            Label(box, "MATERIAIS DA FORJA", 26, 216, 600, 30, 18, TextAnchor.MiddleLeft).color = RusticHudTheme.Gold;
            for (int i = 0; i < ForgeRules.StoneLevels; i++)
            {
                var slot = ForgeSlot.Create(box, "Pedra" + (i + 1), new Vector2(24 + i * 150, -256), 110, "Pedra Nv" + (i + 1));
                var texture = Resources.Load<Texture2D>("Forge/pedra_nv" + (i + 1));
                slot.Show("pedra", texture, ForgeSlot.FullUv, StoneColor(i + 1));
                stoneCounts[i] = slot.count; slot.count.fontSize = 20;
            }
            var luck = ForgeSlot.Create(box, "Amuleto", new Vector2(24 + 5 * 150, -256), 110, "Amuleto da Sorte");
            luck.Show("sorte", Resources.Load<Texture2D>("Forge/amuleto_sorte"), ForgeSlot.FullUv, GoldBorder);
            luckCount = luck.count; luck.count.fontSize = 20;
            var seal = ForgeSlot.Create(box, "Selo", new Vector2(24 + 6 * 150, -256), 110, "Selo de Proteção");
            seal.Show("selo", Resources.Load<Texture2D>("Forge/selo_protecao"), ForgeSlot.FullUv, GoldBorder);
            sealCount = seal.count; seal.count.fontSize = 20;
            Label(box, "Pedras, amuletos e selos são usados na Forja do mestre artesão de cada cidade.", 26, 420, 1100, 24, 15, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;
        }


        // ---------- Talentos ----------

        static readonly HudSymbol[] TalentIcons = { HudSymbol.Heart, HudSymbol.Sword, HudSymbol.Shield, HudSymbol.Dodge, HudSymbol.Target, HudSymbol.Hourglass };
        static readonly Color[] TalentColors =
        {
            new Color(.95f, .35f, .38f), new Color(1f, .62f, .25f), new Color(.35f, .65f, 1f),
            new Color(.35f, .9f, .55f), new Color(.75f, .45f, 1f), new Color(1f, .85f, .35f)
        };

        void BuildTalentPage()
        {
            talentPage = Area(window, "Talentos");
            var box = Box(talentPage, "Quadro", 0, 80, 1180, 640, 22);
            Label(box, "TALENTOS", 26, 14, 400, 32, 22, TextAnchor.MiddleLeft).color = RusticHudTheme.Gold;
            Label(box, "Ganhe 1 ponto a cada nível e fortaleça o seu personagem.", 28, 44, 600, 20, 14, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;
            var pointsBox = RusticHudTheme.Rect(box, "Pontos", Top, new Vector2(760, -16), new Vector2(390, 52));
            var pointsBack = RusticHudTheme.Panel(pointsBox, new Color(.03f, .11f, .12f, 1), 14); pointsBack.borderColor = new Color(.4f, .9f, .95f, .6f); pointsBack.border = 2;
            talentPoints = Label(pointsBox, "", 0, 0, 390, 52, 22, TextAnchor.MiddleCenter);
            talentPoints.color = Cyan; talentPoints.fontStyle = FontStyle.Bold;

            var rules = inventory.Catalog.talents;
            for (int i = 0; i < 6 && i < rules.Length; i++)
            {
                int index = i, column = i % 3, row = i / 3;
                var card = RusticHudTheme.Rect(box, "Talento" + i, Top, new Vector2(24 + column * 380, -88 - row * 214), new Vector2(368, 200));
                var back = RusticHudTheme.Panel(card, new Color(.035f, .075f, .09f, 1), 16);
                back.gradient = true; back.topColor = new Color(.06f, .12f, .14f, 1);
                back.borderColor = Color.Lerp(TalentColors[i], GoldBorder, .5f); back.border = 2;
                var tile = RusticHudTheme.Rect(card, "Icone", Top, new Vector2(16, -16), new Vector2(78, 78));
                var tileBack = tile.gameObject.AddComponent<RoundedHudGraphic>();
                tileBack.radius = 16; tileBack.border = 2.5f; tileBack.color = new Color(TalentColors[i].r * .2f, TalentColors[i].g * .2f, TalentColors[i].b * .2f, 1);
                tileBack.borderColor = TalentColors[i]; tileBack.raycastTarget = false;
                var icon = RusticHudTheme.Icon(tile, TalentIcons[i], Vector2.one * .5f, Vector2.zero, 46); icon.color = TalentColors[i];
                Label(card, rules[i].displayName, 108, 14, 180, 30, 22, TextAnchor.MiddleLeft).color = RusticHudTheme.Parchment;
                talentRanks[i] = Label(card, "", 270, 14, 82, 30, 18, TextAnchor.MiddleRight);
                Label(card, "+" + TalentValue(rules[i], 1) + " " + StatName(rules[i].stat) + " por ponto", 108, 46, 250, 22, 14, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;
                talentPips[i] = new RoundedHudGraphic[Mathf.Max(1, rules[i].maximumRank)];
                for (int p = 0; p < talentPips[i].Length; p++)
                {
                    var pip = RusticHudTheme.Rect(card, "Ponto" + p, Top, new Vector2(108 + p * 48, -78), new Vector2(42, 12));
                    var g = pip.gameObject.AddComponent<RoundedHudGraphic>(); g.radius = 6; g.border = 1; g.raycastTarget = false;
                    talentPips[i][p] = g;
                }
                talentTotals[i] = Label(card, "", 16, 104, 336, 26, 16, TextAnchor.MiddleLeft);
                talentButtons[i] = RusticHudTheme.Button(card, "Melhorar", Top, new Vector2(16, -142), new Vector2(336, 44), "MELHORAR",
                    () => { pendingReset = false; inventory.Request(CoopInventoryAction.SpendTalent, null, EquipmentSlot.MainHand, false, (int)inventory.Catalog.talents[index].id); });
                talentButtons[i].GetComponent<RoundedHudGraphic>().borderColor = TalentColors[i];
            }

            resetButton = RusticHudTheme.Button(box, "Redistribuir", Top, new Vector2(24, -530), new Vector2(360, 46), "", ResetTalents);
            resetLabel = resetButton.GetComponentInChildren<Text>();
            resetButton.GetComponent<RoundedHudGraphic>().borderColor = GoldBorder;
            talentFeedback = Label(box, "", 404, 522, 750, 62, 15, TextAnchor.MiddleLeft);
            talentFeedback.color = RusticHudTheme.Muted;
        }

        static string StatName(ItemStat stat) =>
            stat == ItemStat.Health ? "Vida" : stat == ItemStat.Damage ? "Dano" : stat == ItemStat.Defense ? "Defesa"
            : stat == ItemStat.AttackSpeed ? "Vel. de ataque" : stat == ItemStat.CriticalChance ? "Crítico"
            : stat == ItemStat.CooldownReduction ? "Recarga" : stat == ItemStat.MoveSpeed ? "Movimento" : "Bônus";

        static string TalentValue(TalentRule rule, int ranks)
        {
            float value = rule.perRank * ranks;
            bool flat = rule.stat == ItemStat.Health || rule.stat == ItemStat.Defense;
            return flat ? Mathf.RoundToInt(value).ToString() : (value * 100).ToString("0.#") + "%";
        }

        void RefreshTalents(PlayerProfile profile, ProgressionCatalog catalog)
        {
            int free = HeroTalents.Available(profile, HeroId, catalog);
            talentPoints.text = free == 1 ? "1 PONTO LIVRE" : free + " PONTOS LIVRES";
            var rules = catalog.talents;
            for (int i = 0; i < 6 && i < rules.Length; i++)
            {
                int rank = HeroTalents.Rank(profile, HeroId, rules[i].id), max = rules[i].maximumRank;
                talentRanks[i].text = rank + " / " + max;
                talentRanks[i].color = rank >= max ? RusticHudTheme.Gold : RusticHudTheme.Parchment;
                for (int p = 0; p < talentPips[i].Length; p++)
                {
                    bool on = p < rank;
                    talentPips[i][p].color = on ? TalentColors[i] : new Color(.06f, .1f, .12f, 1);
                    talentPips[i][p].borderColor = on ? Color.white * .9f : new Color(.4f, .45f, .45f, .5f);
                    talentPips[i][p].SetVerticesDirty();
                }
                talentTotals[i].text = "Atual: <color=#7EE8F0>+" + TalentValue(rules[i], rank) + " " + StatName(rules[i].stat) + "</color>";
                talentTotals[i].supportRichText = true;
                bool can = free > 0 && rank < max;
                talentButtons[i].interactable = can;
                talentButtons[i].GetComponentInChildren<Text>().text = rank >= max ? "NÍVEL MÁXIMO" : free > 0 ? "MELHORAR  +" + TalentValue(rules[i], 1) : "SEM PONTOS LIVRES";
            }
            int cost = HeroTalents.ResetCost(profile, HeroId, catalog);
            bool spent = HeroTalents.Spent(profile, HeroId) > 0;
            resetButton.interactable = spent;
            resetLabel.text = !spent ? "REDISTRIBUIR PONTOS" : pendingReset ? "CONFIRMAR · " + cost + " OURO" : "REDISTRIBUIR · " + cost + " OURO";
            talentFeedback.text = "1 ponto por nível depois do primeiro. Redistribuir devolve todos os pontos e custa ouro.";
        }

        void ResetTalents()
        {
            if (!pendingReset) { pendingReset = true; Refresh(); return; }
            pendingReset = false;
            inventory.Request(CoopInventoryAction.ResetTalents, null, EquipmentSlot.MainHand, true, 0);
            Refresh();
        }

        // ---------- Runas ----------

        static readonly string[] RuneEffects =
        {
            "Nível 1 · recolhe o ouro ao passar por cima.",
            "Nível 2 · atrai o ouro em até 2,5 m.",
            "Nível 3 · atrai o ouro em até 4,5 m."
        };

        void BuildRunePage()
        {
            runePage = Area(window, "Runas");
            var sockets = Box(runePage, "Encaixes", 0, 80, 580, 640, 22);
            Label(sockets, "RUNAS", 26, 14, 400, 32, 22, TextAnchor.MiddleLeft).color = RusticHudTheme.Gold;
            Label(sockets, "Arraste uma runa da coleção para o encaixe.", 28, 44, 520, 20, 14, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;

            Label(sockets, "ENCAIXES ATIVOS", 26, 86, 400, 26, 17, TextAnchor.MiddleLeft).color = RusticHudTheme.Gold;
            runeSocket = RuneSlot(sockets, "Encaixe1", 26, -124, "Encaixe 1", out runeSocketIcon);
            runeSocket.dropped = (source, target) => { if (source == runeStored) ToggleRune(); };
            for (int i = 1; i < 4; i++)
            {
                var locked = ForgeSlot.Create(sockets, "Encaixe" + (i + 1), new Vector2(26 + i * 136, -124), 110, "Em breve");
                locked.SetEmpty(LockedBorder);
                DrawLock(locked.Rect);
            }

            Label(sockets, "COLEÇÃO", 26, 300, 400, 26, 17, TextAnchor.MiddleLeft).color = RusticHudTheme.Gold;
            runeStored = RuneSlot(sockets, "Colecao", 26, -338, "Guardada", out runeStoredIcon);
            runeStored.fromBag = true;
            runeStored.dropped = (source, target) => { if (source == runeSocket) ToggleRune(); };
            for (int i = 1; i < 4; i++)
            {
                var empty = ForgeSlot.Create(sockets, "Colecao" + (i + 1), new Vector2(26 + i * 136, -338), 110, null);
                empty.SetEmpty(EmptyBorder);
            }
            Label(sockets, "Novas runas aparecem nas cartas das instâncias.", 26, 510, 530, 22, 14, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;

            var details = Box(runePage, "Detalhes", 590, 80, 590, 640, 22);
            var tile = RusticHudTheme.Rect(details, "Runa", Top, new Vector2(26, -26), new Vector2(130, 130));
            var tileBack = tile.gameObject.AddComponent<RoundedHudGraphic>();
            tileBack.radius = 22; tileBack.border = 3; tileBack.gradient = true;
            tileBack.color = new Color(.04f, .1f, .12f, 1); tileBack.topColor = new Color(.1f, .2f, .2f, 1); tileBack.borderColor = GoldBorder; tileBack.raycastTarget = false;
            runeBigIcon = RusticHudTheme.Icon(tile, HudSymbol.Rune, Vector2.one * .5f, Vector2.zero, 86);
            runeTitle = Label(details, "Runa de Coleta", 176, 30, 390, 36, 26, TextAnchor.MiddleLeft); runeTitle.color = RusticHudTheme.Gold;
            runeLevel = Label(details, "", 178, 70, 390, 26, 17, TextAnchor.MiddleLeft); runeLevel.color = Cyan;
            for (int i = 0; i < 3; i++)
            {
                var pip = RusticHudTheme.Rect(details, "Nivel" + i, Top, new Vector2(178 + i * 64, -112), new Vector2(56, 14));
                var g = pip.gameObject.AddComponent<RoundedHudGraphic>(); g.radius = 7; g.border = 1; g.raycastTarget = false;
                runePips[i] = g;
            }
            Label(details, "EFEITO POR NÍVEL", 26, 190, 400, 26, 17, TextAnchor.MiddleLeft).color = RusticHudTheme.Gold;
            for (int i = 0; i < 3; i++)
            {
                var line = RusticHudTheme.Rect(details, "Efeito" + i, Top, new Vector2(26, -226 - i * 62), new Vector2(538, 54));
                var back = RusticHudTheme.Panel(line, new Color(.04f, .08f, .095f, 1), 12); back.border = 1; back.borderColor = new Color(.86f, .66f, .32f, .3f);
                runeLevelLines[i] = Label(line, RuneEffects[i], 16, 0, 510, 54, 16, TextAnchor.MiddleLeft);
            }
            runeEquip = ActionButtonAt(details, "EquiparRuna", 26, -440, 538, HudSymbol.Rune, ToggleRune, out runeEquipLabel);
            runeImprove = ActionButtonAt(details, "MelhorarRuna", 26, -496, 538, HudSymbol.Coin, UpgradeRune, out runeImproveLabel);
            runeFeedback = Label(details, "", 26, 556, 538, 50, 15, TextAnchor.MiddleCenter);
            runeFeedback.color = RusticHudTheme.Muted;
        }

        ForgeSlot RuneSlot(RectTransform parent, string name, float x, float y, string caption, out HudIcon icon)
        {
            var slot = ForgeSlot.Create(parent, name, new Vector2(x, y), 110, caption);
            slot.dragLayer = full;
            icon = RusticHudTheme.Icon(slot.Rect, HudSymbol.Rune, Vector2.one * .5f, Vector2.zero, 64);
            return slot;
        }

        static Button ActionButtonAt(RectTransform parent, string name, float x, float y, float width, HudSymbol symbol, UnityEngine.Events.UnityAction action, out Text label)
        {
            var button = RusticHudTheme.Button(parent, name, Top, new Vector2(x, y), new Vector2(width, 46), "", action);
            label = button.GetComponentInChildren<Text>();
            label.fontSize = 17;
            RusticHudTheme.Icon(button.transform, symbol, new Vector2(0, .5f), new Vector2(16, 0), 24);
            button.GetComponent<RoundedHudGraphic>().borderColor = GoldBorder;
            return button;
        }

        void RefreshRunes(PlayerProfile profile)
        {
            var service = inventory.Service;
            int level = service.CollectionRuneLevel;
            bool equipped = service.CollectionRuneEquipped;
            bool stored = level > 0 && !equipped;
            Color runeColor = new Color(.95f, .78f, .4f);
            if (equipped) runeSocket.Show("runa", null, ForgeSlot.FullUv, GoldBorder, "", "Nv" + level); else runeSocket.SetEmpty(EmptyBorder);
            if (stored) runeStored.Show("runa", null, ForgeSlot.FullUv, GoldBorder, "", "Nv" + level); else runeStored.SetEmpty(EmptyBorder);
            runeSocketIcon.gameObject.SetActive(equipped); runeSocketIcon.color = runeColor;
            runeStoredIcon.gameObject.SetActive(stored); runeStoredIcon.color = runeColor;
            runeBigIcon.color = level > 0 ? runeColor : new Color(.45f, .45f, .45f, .6f);
            runeLevel.text = level == 0 ? "Ainda não obtida" : "Nível " + level + " de 3 · " + (equipped ? "equipada" : "guardada");
            for (int i = 0; i < 3; i++)
            {
                bool on = i < level;
                runePips[i].color = on ? runeColor : new Color(.06f, .1f, .12f, 1);
                runePips[i].borderColor = on ? Color.white * .9f : new Color(.4f, .45f, .45f, .5f);
                runePips[i].SetVerticesDirty();
                runeLevelLines[i].color = i == level - 1 ? RusticHudTheme.Gold : i < level ? RusticHudTheme.Parchment : RusticHudTheme.Muted;
                runeLevelLines[i].fontStyle = i == level - 1 ? FontStyle.Bold : FontStyle.Normal;
            }
            runeEquip.interactable = level > 0;
            runeEquipLabel.text = level == 0 ? "RUNA NÃO OBTIDA" : equipped ? "DESEQUIPAR" : "EQUIPAR";
            runeImprove.interactable = level > 0 && level < 3 && profile.coins >= service.CollectionRuneUpgradeCost;
            runeImproveLabel.text = level == 0 ? "RUNA BLOQUEADA" : level >= 3 ? "NÍVEL MÁXIMO" : "MELHORAR · " + service.CollectionRuneUpgradeCost + " OURO";
            if (string.IsNullOrEmpty(runeFeedback.text) || level == 0)
                runeFeedback.text = level == 0 ? "A Runa de Coleta pode aparecer nas cartas das Catacumbas." : "";
        }

        void ToggleRune()
        {
            if (!inventory.CanManage) { inventory.SetNotice(inventory.BlockReason); return; }
            bool saved = inventory.Service.ToggleCollectionRune();
            runeFeedback.text = saved ? inventory.Service.Message : "Não foi possível mudar a runa agora.";
            Refresh();
        }

        void UpgradeRune()
        {
            if (!inventory.CanManage) { inventory.SetNotice(inventory.BlockReason); return; }
            bool saved = inventory.Service.UpgradeCollectionRune();
            runeFeedback.text = saved ? inventory.Service.Message : "Ouro insuficiente para melhorar.";
            Refresh();
        }

        // ---------- Small builders ----------

        static readonly Vector2 Top = new Vector2(0, 1);

        static RectTransform Box(RectTransform parent, string name, float x, float y, float width, float height, float radius)
        {
            var rect = RusticHudTheme.Rect(parent, name, Top, new Vector2(x, -y), new Vector2(width, height));
            var graphic = RusticHudTheme.Panel(rect, new Color(.025f, .06f, .075f, .98f), radius);
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

        static Text Label(RectTransform parent, string value, float x, float y, float width, float height, int font, TextAnchor align) =>
            RusticHudTheme.Label(parent, "Texto", Top, new Vector2(x, -y), new Vector2(width, height), value, font, align);

        ForgeSlot Slot(Transform parent, string name, float x, float y, float size, string label)
        {
            var slot = ForgeSlot.Create(parent, name, new Vector2(x, y), size, label);
            slot.dragLayer = full;
            slot.hovered = ShowDetails;
            slot.unhovered = _ => { if (selected == null) tooltip.gameObject.SetActive(false); else ShowSelectedDetails(); };
            return slot;
        }

        static Button ActionButton(RectTransform parent, string name, float x, float width, HudSymbol symbol, UnityEngine.Events.UnityAction action, out Text label)
        {
            var button = RusticHudTheme.Button(parent, name, Top, new Vector2(x, -584), new Vector2(width, 44), "", action);
            label = button.GetComponentInChildren<Text>();
            label.fontSize = 15; label.rectTransform.anchoredPosition = new Vector2(14, 0);
            RusticHudTheme.Icon(button.transform, symbol, new Vector2(0, .5f), new Vector2(14, 0), 22);
            button.GetComponent<RoundedHudGraphic>().borderColor = GoldBorder;
            return button;
        }

        // A small padlock drawn with rounded shapes (no texture needed).
        static void DrawLock(RectTransform slot)
        {
            var shackle = RusticHudTheme.Rect(slot, "Cadeado_Arco", Vector2.one * .5f, new Vector2(0, 9), new Vector2(26, 30));
            var arc = shackle.gameObject.AddComponent<RoundedHudGraphic>();
            arc.radius = 13; arc.border = 4; arc.color = new Color(0, 0, 0, 0); arc.borderColor = new Color(.62f, .56f, .45f, .8f); arc.raycastTarget = false;
            var body = RusticHudTheme.Rect(slot, "Cadeado_Corpo", Vector2.one * .5f, new Vector2(0, -8), new Vector2(36, 28));
            var box = body.gameObject.AddComponent<RoundedHudGraphic>();
            box.radius = 6; box.border = 0; box.color = new Color(.62f, .56f, .45f, .9f); box.raycastTarget = false;
            var hole = RusticHudTheme.Rect(body, "Fechadura", Vector2.one * .5f, new Vector2(0, 1), new Vector2(7, 11));
            var key = hole.gameObject.AddComponent<RoundedHudGraphic>();
            key.radius = 3.5f; key.border = 0; key.color = new Color(.1f, .1f, .1f, .9f); key.raycastTarget = false;
        }

        static Color StoneColor(int level) =>
            level == 1 ? new Color(.35f, .9f, .45f) : level == 2 ? new Color(.35f, .85f, 1f) : level == 3 ? new Color(1f, .45f, .7f)
            : level == 4 ? new Color(.7f, .45f, 1f) : new Color(1f, .8f, .35f);

        // ---------- Open / close ----------

        public void Open()
        {
            if (inventory == null || inventory.Service == null) return;
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            window.localScale = Vector3.one * UiWindowScale.For(full, WindowSize);
            blockInput?.Invoke(true);
            selected = pendingDiscard = null;
            lastNotice = inventory.Notice;
            noticeText.text = "";
            paperDoll.SetVisible(true);
            SetPage(0);
        }

        public void OpenPage(int value)
        {
            if (!IsOpen) Open();
            SetPage(Mathf.Clamp(value, 0, 4));
        }

        public void Close()
        {
            if (!IsOpen) return;
            tooltip.gameObject.SetActive(false);
            paperDoll.SetVisible(false);
            gameObject.SetActive(false);
            blockInput?.Invoke(false);
        }

        void SetPage(int value)
        {
            page = value;
            pendingReset = false;
            equipPage.gameObject.SetActive(page == 0);
            statusPage.gameObject.SetActive(page == 1);
            talentPage.gameObject.SetActive(page == 2);
            runePage.gameObject.SetActive(page == 3);
            resourcePage.gameObject.SetActive(page == 4);
            tooltip.gameObject.SetActive(false);
            for (int i = 0; i < pageTabs.Length; i++) Highlight(pageTabs[i], i == page);
            Refresh();
        }

        void Update()
        {
            if (inventory == null || inventory.Service == null) return;
            if (inventory.Revision != lastRevision) Refresh();
            if (inventory.Notice != lastNotice)
            {
                lastNotice = inventory.Notice;
                noticeText.text = lastNotice;
            }
        }

        // ---------- Refresh ----------

        PlayerProfile Profile => inventory.Service.Profile;
        string HeroId => inventory.World.Hero.Current.heroId;

        void Refresh()
        {
            if (inventory == null || inventory.Service == null) return;
            lastRevision = inventory.Revision;
            var profile = Profile;
            var catalog = inventory.Catalog;
            var hero = inventory.World.Hero.Current;
            var stats = hero != null ? ProgressionStats.Calculate(hero, profile, catalog) : default;

            if (page == 0) RefreshEquipment(profile, catalog, hero, stats);
            else if (page == 1) RefreshStatus(profile, catalog, stats);
            else if (page == 2) RefreshTalents(profile, catalog);
            else if (page == 3) RefreshRunes(profile);
            else if (page == 4) RefreshResources(profile);
        }

        void RefreshEquipment(PlayerProfile profile, ProgressionCatalog catalog, HeroDefinition hero, StatSnapshot stats)
        {
            string name = CampaignHome.CharacterName;
            nameText.text = string.IsNullOrEmpty(name) ? (hero != null ? hero.displayName : "Herói") : name;
            int level = ProgressionStats.Level(profile.experience, catalog.maximumLevel);
            levelText.text = "Nível " + level;
            int inLevel = ProgressionStats.ExperienceInLevel(profile.experience, catalog.maximumLevel);
            int needed = ProgressionStats.RequiredExperience(level);
            bool max = level >= catalog.maximumLevel;
            xpText.text = max ? "Nível máximo" : "XP " + inLevel + " / " + needed;
            xpFill.sizeDelta = new Vector2(532 * (max ? 1 : Mathf.Clamp01((float)inLevel / needed)), 8);

            chipValues[0].text = stats.maximumHealth.ToString();
            chipValues[1].text = stats.damage.ToString();
            chipValues[2].text = Mathf.RoundToInt(Mathf.Clamp(stats.defense, 0, .75f) * 100) + "%";
            chipValues[3].text = Mathf.RoundToInt(stats.criticalChance * 100) + "%";
            chipValues[4].text = stats.attacksPerSecond.ToString("0.00");
            chipValues[5].text = stats.speed.ToString("0.0");

            var set = profile.Loadout(HeroId);
            for (int i = 0; i < Spaces.Length; i++)
            {
                var slot = equipSlots[i];
                var hint = slot.Rect.Find("Dica");
                if (Spaces[i].slot == null) { slot.SetEmpty(LockedBorder); continue; }
                var item = set != null ? profile.Item(set.items[(int)Spaces[i].slot.Value]) : null;
                if (item == null) { slot.SetEmpty(EmptyBorder); if (hint != null) hint.gameObject.SetActive(true); continue; }
                if (hint != null) hint.gameObject.SetActive(false);
                var texture = ItemTexture(item, out Rect uv);
                slot.Show(item.instanceId, texture, uv, item.instanceId == selected ? Color.white : RarityColor(item), "", item.forgeLevel > 0 ? "+" + item.forgeLevel : "");
            }

            shown.Clear();
            foreach (var item in profile.inventory)
            {
                if (InventoryRules.Equipped(profile, item.instanceId)) continue;
                var definition = catalog.Find(item.definitionId);
                if (definition == null) continue;
                if (FilterSlots[filter] != null && definition.slot != FilterSlots[filter].Value) continue;
                shown.Add(item);
            }
            shown.Sort(Compare);
            for (int i = 0; i < bagSlots.Count; i++)
            {
                var slot = bagSlots[i];
                if (i >= shown.Count) { slot.frame.border = 2f; slot.SetEmpty(EmptyBorder); continue; }
                var item = shown[i];
                bool usable = ProgressionStats.CanEquip(profile, catalog, HeroId, item, catalog.Find(item.definitionId).slot, out _);
                var texture = ItemTexture(item, out Rect uv);
                slot.Show(item.instanceId, texture, uv, item.instanceId == selected ? Color.white : RarityColor(item), "", item.forgeLevel > 0 ? "+" + item.forgeLevel : "", !usable);
                slot.frame.border = item.instanceId == selected ? 3.5f : 2f;
                slot.frame.SetVerticesDirty();
            }
            countText.text = InventoryRules.BagCount(profile) + " / " + catalog.inventoryCapacity;
            goldText.text = profile.coins.ToString("N0");
            for (int i = 0; i < filterTabs.Length; i++) Highlight(filterTabs[i], i == filter);
            sortLabel.text = "ORDEM: " + SortNames[sort];

            var chosen = selected != null ? profile.Item(selected) : null;
            if (chosen == null) selected = null;
            bool equipped = chosen != null && InventoryRules.Equipped(profile, chosen.instanceId);
            actionLabel.text = equipped ? "DESEQUIPAR" : "EQUIPAR";
            actionButton.interactable = chosen != null;
            discardButton.interactable = chosen != null && !equipped;
            discardLabel.text = chosen != null && pendingDiscard == chosen.instanceId
                ? "CONFIRMAR +" + InventoryRules.SellValue(catalog, chosen)
                : "RECICLAR";
            paperDoll.Refresh(inventory.World.Hero.GetComponent<HeroPresentation>(), profile, HeroId);
        }

        void RefreshStatus(PlayerProfile profile, ProgressionCatalog catalog, StatSnapshot stats)
        {
            var set = profile.Loadout(HeroId);
            var weapon = set != null ? profile.Item(set.items[0]) : null;
            string[] values =
            {
                stats.maximumHealth.ToString(),
                stats.damage.ToString(),
                stats.criticalDamage.ToString(),
                Mathf.RoundToInt(stats.criticalChance * 100) + "%",
                stats.attacksPerSecond.ToString("0.00"),
                Mathf.RoundToInt(Mathf.Clamp(stats.defense, 0, .75f) * 100) + "%",
                stats.speed.ToString("0.0") + " m/s",
                "-" + Mathf.RoundToInt((1 - stats.cooldownMultiplier) * 100) + "%",
                "+" + Mathf.RoundToInt(stats.elementalDamage * 100) + "%",
                stats.recovery.ToString("0.0") + "/s",
                Mathf.RoundToInt(stats.block * 100) + "%",
                weapon != null ? "+" + weapon.forgeLevel : "—",
            };
            for (int i = 0; i < statValues.Count; i++) statValues[i].text = values[i];
        }

        void RefreshResources(PlayerProfile profile)
        {
            resourceGold.text = profile.coins.ToString("N0");
            deliveryCount.text = profile.deliveries.Count.ToString();
            claimButton.interactable = profile.deliveries.Count > 0;
            for (int i = 0; i < stoneCounts.Length; i++) stoneCounts[i].text = inventory.Service.ForgeStoneCount(i + 1).ToString();
            luckCount.text = profile.luckCharms.ToString();
            sealCount.text = profile.protectionSeals.ToString();
        }

        int Compare(ItemInstance a, ItemInstance b)
        {
            var catalog = inventory.Catalog;
            var da = catalog.Find(a.definitionId); var db = catalog.Find(b.definitionId);
            int result = 0;
            if (sort == 0) result = b.rarity.CompareTo(a.rarity);
            else if (sort == 1) result = db.minimumLevel.CompareTo(da.minimumLevel);
            else result = da.slot.CompareTo(db.slot);
            if (result == 0) result = b.forgeLevel.CompareTo(a.forgeLevel);
            if (result == 0) result = string.Compare(da.displayName, db.displayName, StringComparison.Ordinal);
            return result;
        }

        static void Highlight(Button button, bool active)
        {
            var graphic = button.GetComponent<RoundedHudGraphic>();
            graphic.borderColor = active ? GoldBorder : RusticHudTheme.Copper;
            graphic.border = active ? 2.5f : 1.5f;
            graphic.topColor = active ? new Color(.2f, .17f, .08f, 1) : new Color(.084f, .16f, .19f, RusticHudTheme.Surface.a);
            graphic.SetVerticesDirty();
            var text = button.GetComponentInChildren<Text>();
            if (text != null) text.color = active ? RusticHudTheme.Gold : RusticHudTheme.Parchment;
        }

        Texture ItemTexture(ItemInstance item, out Rect uv)
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

        // ---------- Actions ----------

        void BagClicked(ForgeSlot slot)
        {
            if (!slot.HasContent) { selected = pendingDiscard = null; tooltip.gameObject.SetActive(false); Refresh(); return; }
            string id = (string)slot.content;
            bool doubleClick = id == lastClickId && Time.unscaledTime - lastClickTime < .35f;
            lastClickId = id; lastClickTime = Time.unscaledTime;
            if (doubleClick) { selected = id; Act(); return; }
            Select(id, false);
        }

        void Select(string id, bool fromEquipment)
        {
            selected = id; pendingDiscard = null;
            Refresh();
            ShowSelectedDetails();
        }

        void Act()
        {
            var item = selected != null ? Profile.Item(selected) : null;
            if (item == null) return;
            if (InventoryRules.Equipped(Profile, item.instanceId)) { Unequip(item.instanceId); return; }
            var definition = inventory.Catalog.Find(item.definitionId);
            if (definition == null) return;
            inventory.Request(CoopInventoryAction.Equip, item.instanceId, definition.slot, false, 0);
        }

        void EquipFromBag(string id, int space)
        {
            var item = Profile.Item(id);
            var definition = item != null ? inventory.Catalog.Find(item.definitionId) : null;
            if (definition == null) return;
            if (Spaces[space].slot == null) { inventory.SetNotice(Spaces[space].label + ": espaço ainda bloqueado. Chega em uma atualização futura."); return; }
            if (Spaces[space].slot.Value != definition.slot) { inventory.SetNotice("Este item não vai no espaço " + Spaces[space].label + "."); return; }
            selected = id;
            inventory.Request(CoopInventoryAction.Equip, id, definition.slot, false, 0);
        }

        void Unequip(string id)
        {
            var set = Profile.Loadout(HeroId);
            if (set == null) return;
            for (int i = 0; i < set.items.Length; i++)
                if (set.items[i] == id) { inventory.Request(CoopInventoryAction.Unequip, null, (EquipmentSlot)i, false, 0); return; }
        }

        void Discard()
        {
            if (selected == null) return;
            if (pendingDiscard != selected) { pendingDiscard = selected; Refresh(); return; }
            inventory.Request(CoopInventoryAction.Discard, selected, EquipmentSlot.MainHand, true, 0);
            selected = pendingDiscard = null;
            tooltip.gameObject.SetActive(false);
            Refresh();
        }

        // ---------- Details ----------

        void ShowDetails(ForgeSlot slot)
        {
            var item = slot.content is string id ? Profile.Item(id) : null;
            if (item == null) return;
            ShowItem(item, slot.fromBag);
        }

        void ShowSelectedDetails()
        {
            var item = selected != null ? Profile.Item(selected) : null;
            if (item == null) { tooltip.gameObject.SetActive(false); return; }
            ShowItem(item, !InventoryRules.Equipped(Profile, item.instanceId));
        }

        void ShowItem(ItemInstance item, bool fromBag)
        {
            if (page != 0) return;
            ItemInstance compare = null;
            if (fromBag)
            {
                var definition = inventory.Catalog.Find(item.definitionId);
                var set = Profile.Loadout(HeroId);
                if (definition != null && set != null) compare = Profile.Item(set.items[(int)definition.slot]);
            }
            tooltipText.text = ItemDescriptions.Describe(inventory.Catalog, item, compare);
            // Bag item: details over the character side. Equipped item: details over the bag side.
            tooltip.anchoredPosition = fromBag ? new Vector2(236, -100) : new Vector2(614, -100);
            tooltip.gameObject.SetActive(true);
            tooltip.SetAsLastSibling();
        }
    }
}
