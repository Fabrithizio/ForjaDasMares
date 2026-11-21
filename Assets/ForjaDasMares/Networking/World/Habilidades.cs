using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // Learned abilities and the two bars of 3 (decided with Fabricio):
    // - the hero carries 6 abilities in 2 bars of 3; only one bar is active; R (PC) or the button swaps (1 s wait);
    // - the HABILIDADES page of the backpack (Mochila) lists every learned ability; drag one (or tap it, then tap
    //   a slot) to put it in a bar. Later this page will also hold the creation of new abilities.
    public sealed class Habilidades : MonoBehaviour
    {
        public const int PorBarra = 3;
        const float SwapWait = 1f;

        RegionalWorld world; RegionalInventory inventory; HeroCombat combat; ArenaInput input;
        AbilityDefinition[] starters;
        readonly List<AbilityDefinition> learned = new List<AbilityDefinition>();
        readonly AbilityDefinition[][] bars = { new AbilityDefinition[PorBarra], new AbilityDefinition[PorBarra] };
        readonly AbilityDefinition[] kit = new AbilityDefinition[PorBarra]; // what HeroCombat uses (the active bar)
        int active; float swapReady; bool learnedReady;
        public int BarraAtiva => active;
        public static Habilidades Atual { get; private set; }

        public void Configure(RegionalWorld owner, RegionalInventory inv, AbilityDefinition[] starterSkills)
        {
            world = owner; inventory = inv; starters = starterSkills;
            combat = world.Hero.GetComponent<HeroCombat>(); input = world.Hero.GetComponent<ArenaInput>();
            combat.ConfigureRegionalSkills(kit, () => PorBarra);
            Atual = this;
            BuildHud();
            Refresh();
        }
        void OnDestroy() { if (Atual == this) Atual = null; swapKey?.Dispose(); }

        // ---------- What the hero knows ----------
        // Nobody starts with abilities. Each one is learned in the world (decided with Fabricio, 20/11):
        // Brotar = Iria (herb mission), Brasa = Téo (leather mission), Raízes = first touch on the Raiz-Mãe,
        // Rajada = Nara (after reaching the Nascente), Bolha d'Água = secret of the Nascente,
        // Marca do Raio = mini-boss of the Nascente, Pele de Gelo = secret of the Gruta da Cachoeira.
        public static bool Aprendeu(string id) => Historia.Feito("hab_" + id);
        static int versao = 1; int vista;
        public static void Aprender(string id)
        {
            if (Aprendeu(id)) return;
            AbilityDefinition a = null; foreach (var x in HabilidadesReais.Todas) if (x.scriptId == id) a = x;
            if (a == null) return;
            Historia.Marcar("hab_" + id); versao++;
            // Satisfying: a burst in the element's colour around the hero and the name on screen.
            var w = FindFirstObjectByType<RegionalWorld>();
            if (w != null && w.Hero != null)
            {
                var c = EfeitosDeHabilidade.Cor(a.elemento); var p = w.Hero.transform.position;
                EfeitosDeHabilidade.Coluna(p, 1.1f, c); EfeitosDeHabilidade.Explosao(p + Vector3.up, 1.4f, c, 40);
            }
            AvisoNaTela.Mostrar("Nova habilidade: " + a.displayName + "  ·  monte na mochila, aba Habilidades", 5);
        }
        void Refresh()
        {
            if (vista == versao) return;
            bool first = vista == 0; vista = versao;
            if (first)
            {
                // Saves from before this rule: what was already done still gives its ability (quietly).
                if (Historia.Feito("m1_feita")) Historia.Marcar("hab_brotar");
                if (Historia.Feito("m3_feita")) Historia.Marcar("hab_brasa");
                if (!string.IsNullOrEmpty(Historia.CenaDoNo)) Historia.Marcar("hab_raizes");
            }
            var before = new List<AbilityDefinition>(learned);
            learned.Clear();
            foreach (var a in HabilidadesReais.Todas) if (Aprendeu(a.scriptId)) learned.Add(a);
            Load();
            // A new one goes straight into the first free slot (active bar first).
            if (!first)
            {
                foreach (var a in learned)
                {
                    if (before.Contains(a) || System.Array.IndexOf(bars[0], a) >= 0 || System.Array.IndexOf(bars[1], a) >= 0) continue;
                    for (int k = 0; k < 2; k++)
                    {
                        var bar = bars[(active + k) % 2]; int free = System.Array.IndexOf(bar, null);
                        if (free >= 0) { bar[free] = a; break; }
                    }
                }
                Save();
            }
            learnedReady = true;
            Apply();
        }
        static AbilityDefinition Make(string name, AbilityEffect effect, MeleeFootprint footprint, int damage, float range, float arc, float cooldown, ControlEffect control, float controlTime, float knockback, Color color)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            var hit = ScriptableObject.CreateInstance<AttackDefinition>();
            ability.displayName = name; ability.effect = effect; ability.hit = hit; ability.cooldown = cooldown; ability.radius = range;
            hit.shape = AttackShape.Arc; hit.footprint = footprint; hit.damage = damage; hit.range = range; hit.arc = arc;
            hit.windup = .18f; hit.recovery = .26f; hit.knockback = knockback; hit.color = color;
            hit.control = control; hit.controlDuration = controlTime;
            return ability;
        }
        AbilityDefinition Find(string name) => learned.Find(a => a != null && a.displayName == name);

        // ---------- Saving (per account) ----------
        string Key => "Habilidades.Barras." + CampaignHome.LocalAccountId;
        [System.Serializable] sealed class Salvo { public string[] nomes = new string[PorBarra * 2]; public int ativa; }
        void Load()
        {
            System.Array.Clear(bars[0], 0, PorBarra); System.Array.Clear(bars[1], 0, PorBarra);
            var json = PlayerPrefs.GetString(Key, "");
            var saved = string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<Salvo>(json);
            if (saved?.nomes != null && saved.nomes.Length == PorBarra * 2)
            {
                for (int i = 0; i < PorBarra * 2; i++) bars[i / PorBarra][i % PorBarra] = string.IsNullOrEmpty(saved.nomes[i]) ? null : Find(saved.nomes[i]);
                active = Mathf.Clamp(saved.ativa, 0, 1);
            }
            bool empty = true;
            for (int i = 0; i < PorBarra * 2; i++) if (bars[i / PorBarra][i % PorBarra] != null) empty = false;
            if (empty)
            {
                // First time: the first 3 learned in bar 1, the next 3 in bar 2.
                for (int i = 0; i < PorBarra * 2 && i < learned.Count; i++) bars[i / PorBarra][i % PorBarra] = learned[i];
                active = 0;
            }
        }
        void Save()
        {
            var saved = new Salvo { ativa = active };
            for (int i = 0; i < PorBarra * 2; i++) saved.nomes[i] = bars[i / PorBarra][i % PorBarra]?.displayName ?? "";
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(saved)); PlayerPrefs.Save();
        }
        void Apply() { for (int i = 0; i < PorBarra; i++) kit[i] = bars[active][i]; RefreshHud(); RefreshPanel(); }

        // ---------- Swapping bars ----------
        public void Trocar()
        {
            if (!world.Hero.Health.IsAlive || input.CombatBlocked) return;
            if (Time.time < swapReady) return;
            swapReady = Time.time + SwapWait;
            active = 1 - active; Apply(); Save();
            if (swapButton != null) StartCoroutine(Pulse(swapButton.transform));
        }
        System.Collections.IEnumerator Pulse(Transform t)
        {
            for (float k = 0; k < 1; k += Time.unscaledDeltaTime / .2f) { t.localScale = Vector3.one * (1 + .12f * Mathf.Sin(k * Mathf.PI)); yield return null; }
            t.localScale = Vector3.one;
        }

        // ---------- HUD: only a small swap button (the bars are set up in the backpack) ----------
        InputAction swapKey; Button swapButton; Text swapText; RectTransform canvas;
        void BuildHud()
        {
            var go = new GameObject("HabilidadesUI", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var c = go.AddComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 40;
            var scaler = go.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            go.AddComponent<GraphicRaycaster>();
            canvas = (RectTransform)go.transform;
            // The I/II tab on the combat bar (RegionalWorldHud) calls Trocar(); here only the R key.
            swapKey = new InputAction("TrocarBarra", InputActionType.Button, "<Keyboard>/r"); swapKey.performed += _ => { if (!input.CombatBlocked) Trocar(); }; swapKey.Enable();
        }
        void RefreshHud()
        {
            if (swapText == null) return;
            swapText.text = "BARRA " + (active + 1) + "/2" + (Application.isMobilePlatform ? " · TROCAR" : " · R");
        }
        void Update()
        {
            if (world == null) return;
            Refresh();
            if (page != null && page.gameObject.activeInHierarchy)
            {
                if (!panelOpen) { panelOpen = true; selected = null; RefreshPanel(); }
                if (notice != null) notice.text = inventory != null && !inventory.CanManage ? inventory.BlockReason : selected != null ? Detalhe(selected) : Hint;
            }
            else panelOpen = false;
        }

        // ---------- The HABILIDADES screen ----------
        const string Hint = "Arraste uma habilidade até um espaço (ou toque nela e depois no espaço). Toque num espaço ocupado para esvaziar.";
        RectTransform page; bool panelOpen; RectTransform list; Text notice; RectTransform dragRoot;
        readonly Slot[][] slots = { new Slot[PorBarra], new Slot[PorBarra] };
        readonly Text[] barTitles = new Text[2];
        AbilityDefinition selected;
        // Builds the HABILIDADES page inside the backpack window (called by MochilaPanel).
        public void MontarNa(RectTransform area)
        {
            if (page == area) return;
            page = area;
            var box = RusticHudTheme.Rect(area, "Quadro", new Vector2(0, 1), new Vector2(0, -80), new Vector2(1180, 640));
            RusticHudTheme.Panel(box, RusticHudTheme.Ink, 22);
            var top = new Vector2(0, 1);
            RusticHudTheme.Label(box, "Aprendidas", top, new Vector2(30, -18), new Vector2(520, 30), "HABILIDADES APRENDIDAS", 19).color = RusticHudTheme.Gold;
            list = RusticHudTheme.Rect(box, "Lista", top, new Vector2(30, -56), new Vector2(560, 470));
            for (int b = 0; b < 2; b++)
            {
                barTitles[b] = RusticHudTheme.Label(box, "TituloBarra" + b, top, new Vector2(640, -18 - b * 200), new Vector2(500, 30), "", 19);
                for (int i = 0; i < PorBarra; i++)
                {
                    var r = RusticHudTheme.Rect(box, "Espaco" + b + i, top, new Vector2(640 + i * 172, -56 - b * 200), new Vector2(160, 150));
                    var slot = r.gameObject.AddComponent<Slot>(); slot.Build(this, b, i);
                    slots[b][i] = slot;
                }
            }
            var create = RusticHudTheme.Button(box, "Criar", top, new Vector2(640, -460), new Vector2(500, 46), "CRIAR HABILIDADE · em breve", () => { });
            create.interactable = false;
            notice = RusticHudTheme.Label(box, "Aviso", top, new Vector2(30, -560), new Vector2(1120, 60), Hint, 15);
            notice.horizontalOverflow = HorizontalWrapMode.Wrap; notice.verticalOverflow = VerticalWrapMode.Overflow;
            var root = area.GetComponentInParent<Canvas>();
            dragRoot = root != null ? (RectTransform)root.rootCanvas.transform : canvas;
            panelOpen = area.gameObject.activeInHierarchy;
            RefreshPanel();
        }
        void RefreshPanel()
        {
            if (page == null || !panelOpen) return;
            for (int i = list.childCount - 1; i >= 0; i--) Destroy(list.GetChild(i).gameObject);
            if (learned.Count == 0)
                RusticHudTheme.Label(list, "Vazio", new Vector2(0, 1), Vector2.zero, new Vector2(540, 80), "Você ainda não aprendeu nenhuma habilidade.", 17);
            for (int i = 0; i < learned.Count; i++)
            {
                var r = RusticHudTheme.Rect(list, "Hab" + i, new Vector2(0, 1), new Vector2((i % 2) * 280, -(i / 2) * 84), new Vector2(268, 74));
                r.gameObject.AddComponent<Card>().Build(this, learned[i]);
            }
            for (int b = 0; b < 2; b++)
            {
                barTitles[b].text = "BARRA " + (b + 1) + (b == active ? "  · em uso" : "");
                for (int i = 0; i < PorBarra; i++) slots[b][i].Show(bars[b][i]);
            }
        }
        bool CanEdit()
        {
            if (inventory == null || inventory.CanManage) return true;
            return false;
        }
        internal void Pick(AbilityDefinition ability) { if (!CanEdit()) return; selected = selected == ability ? null : ability; RefreshPanel(); }
        internal void Place(int bar, int index, AbilityDefinition ability)
        {
            if (!CanEdit()) return;
            if (ability == null)
            {
                if (selected != null) { ability = selected; selected = null; }
                else { bars[bar][index] = null; Changed(); return; }
            }
            // The same ability is not in two places: it moves.
            for (int b = 0; b < 2; b++) for (int i = 0; i < PorBarra; i++) if (bars[b][i] == ability) bars[b][i] = null;
            bars[bar][index] = ability;
            Changed();
        }
        void Changed() { Save(); Apply(); }
        // Details of the chosen ability, with the form of the weapon in hand.
        string Detalhe(AbilityDefinition a)
        {
            bool longe = combat != null && combat.RangedWeapon;
            string form = longe ? a.descricaoCajado : a.descricaoEspada;
            return a.displayName.ToUpperInvariant() + "  ·  " + Describe(a) + "\n" + (longe ? "Longe: " : "Perto: ") + form + "   (toque num espaço para colocar)";
        }
        internal static string Describe(AbilityDefinition a)
        {
            if (a == null) return "";
            string what = a.elemento != Elemento.Nenhum ? Nome(a.elemento) + " · " + Nome(a.subtipo) : a.hit != null && a.hit.control == ControlEffect.Root ? "Prende" : "Área";
            return what + " · " + a.cooldown.ToString("0") + "s · " + a.energyCost.ToString("0") + " fôlego";
        }

        // Rounded picture of an ability (top-left corner at 'at').
        static Image Picture(RectTransform parent, AbilityDefinition a, Vector2 at, float size, float radius)
        {
            var mask = RusticHudTheme.Rect(parent, "Imagem", new Vector2(0, 1), at, new Vector2(size, size));
            var shape = mask.gameObject.AddComponent<RoundedHudGraphic>(); shape.radius = radius; shape.border = 0; shape.raycastTarget = false;
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var art = RusticHudTheme.Rect(mask, "Arte", Vector2.one * .5f, Vector2.zero, new Vector2(size, size));
            var image = art.gameObject.AddComponent<Image>(); image.raycastTarget = false;
            var sprite = IconesDeHabilidade.De(a);
            image.sprite = sprite; mask.gameObject.SetActive(sprite != null);
            return image;
        }
        static string Nome(Elemento e) => e == Elemento.Agua ? "Água" : e.ToString();
        static string Nome(Subtipo s) => s == Subtipo.Protecao ? "Proteção" : s.ToString();

        // A learned ability: drag it to a slot, or tap it and then tap the slot.
        sealed class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            Habilidades owner; AbilityDefinition ability; RectTransform ghost;
            public void Build(Habilidades h, AbilityDefinition a)
            {
                owner = h; ability = a;
                var rect = (RectTransform)transform;
                var g = RusticHudTheme.Panel(rect, h.selected == a ? new Color(.16f, .26f, .2f, .97f) : RusticHudTheme.Surface, 14);
                g.raycastTarget = true; g.borderColor = h.selected == a ? RusticHudTheme.Gold : RusticHudTheme.Copper;
                var top = new Vector2(0, 1);
                // Clean card: picture and name; the details show at the bottom when it is chosen.
                Picture(rect, a, new Vector2(8, -8), 58, 10);
                RusticHudTheme.Label(rect, "Nome", top, new Vector2(76, -10), new Vector2(184, 30), a.displayName, 18).color = RusticHudTheme.Gold;
                RusticHudTheme.Label(rect, "Tipo", top, new Vector2(76, -40), new Vector2(184, 22), Nome(a.elemento) + " · " + Nome(a.subtipo), 13).color = RusticHudTheme.Muted;
            }
            public void OnPointerClick(PointerEventData e) { if (!e.dragging) owner.Pick(ability); }
            public void OnBeginDrag(PointerEventData e)
            {
                if (!owner.CanEdit()) return;
                ghost = RusticHudTheme.Rect(owner.dragRoot, "Arrastando", Vector2.one * .5f, Vector2.zero, new Vector2(240, 56));
                RusticHudTheme.Panel(ghost, new Color(.16f, .26f, .2f, .9f), 14).borderColor = RusticHudTheme.Gold;
                RusticHudTheme.Label(ghost, "Nome", Vector2.one * .5f, Vector2.zero, new Vector2(220, 40), ability.displayName, 17, TextAnchor.MiddleCenter);
                foreach (var gr in ghost.GetComponentsInChildren<Graphic>()) gr.raycastTarget = false;
                OnDrag(e);
            }
            public void OnDrag(PointerEventData e)
            {
                if (ghost == null) return;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(owner.dragRoot, e.position, null, out var local);
                ghost.SetAsLastSibling();
                ghost.anchorMin = ghost.anchorMax = ghost.pivot = Vector2.one * .5f; ghost.anchoredPosition = local;
            }
            public void OnEndDrag(PointerEventData e)
            {
                if (ghost != null) Destroy(ghost.gameObject);
                ghost = null;
                var over = e.pointerCurrentRaycast.gameObject;
                var slot = over != null ? over.GetComponentInParent<Slot>() : null;
                if (slot != null) owner.Place(slot.bar, slot.index, ability);
            }
        }

        // One of the 6 places (2 bars of 3).
        sealed class Slot : MonoBehaviour, IPointerClickHandler
        {
            Habilidades owner; internal int bar, index; Text name; Image picture; RoundedHudGraphic back;
            public void Build(Habilidades h, int b, int i)
            {
                owner = h; bar = b; index = i;
                var rect = (RectTransform)transform;
                back = RusticHudTheme.Panel(rect, new Color(.03f, .06f, .08f, .97f), 16); back.raycastTarget = true;
                RusticHudTheme.Label(rect, "Numero", new Vector2(0, 1), new Vector2(12, -8), new Vector2(60, 24), (i + 1).ToString(), 16).color = RusticHudTheme.Muted;
                picture = Picture(rect, null, new Vector2(30, -12), 100, 14);
                name = RusticHudTheme.Label(rect, "Nome", new Vector2(.5f, 0), new Vector2(0, 6), new Vector2(150, 30), "", 15, TextAnchor.MiddleCenter);
                name.horizontalOverflow = HorizontalWrapMode.Wrap;
            }
            public void Show(AbilityDefinition a)
            {
                name.text = a != null ? a.displayName : "vazio"; name.color = a != null ? RusticHudTheme.Gold : RusticHudTheme.Muted;
                var sprite = IconesDeHabilidade.De(a);
                picture.sprite = sprite; picture.transform.parent.gameObject.SetActive(sprite != null);
                back.borderColor = owner.selected != null ? RusticHudTheme.Gold : RusticHudTheme.Copper;
            }
            public void OnPointerClick(PointerEventData e) => owner.Place(bar, index, null);
        }
    }
}
