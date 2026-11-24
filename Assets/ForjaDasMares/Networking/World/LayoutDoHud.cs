using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // The player's own HUD layout (Configurações › EDITAR LAYOUT): every main piece of the HUD can be dragged to
    // another place and made bigger or smaller. Phone and PC keep separate layouts (PlayerPrefs, this device).
    // Pieces never leave the screen.
    public sealed class LayoutDoHud : MonoBehaviour
    {
        // HUD pieces that can move (by the name they have in the HUD). The ones that do not exist on this device are skipped.
        static readonly string[] Nomes =
        {
            "HeroVitals", "StoryNotice", "Quest", "MiniMapa", "AcoesTopo", "AcoesBaixo", "OpenJournal",
            "SkillSlot1", "SkillSlot2", "SkillSlot3", "Super", "TrocarBarra", "Pocao", "Usar",
            "RegionalDodge", "RegionalStick", "RegionalAttack", "RegionalInteract",
        };
        static readonly Dictionary<string, string> Rotulos = new Dictionary<string, string>
        {
            {"HeroVitals","Vida"},{"StoryNotice","Acompanhamento"},{"Quest","Missão"},{"MiniMapa","Minimapa"},{"AcoesTopo","Montar"},
            {"AcoesBaixo","Botões"},{"OpenJournal","Diário"},{"SkillSlot1","Habilidade 1"},{"SkillSlot2","Habilidade 2"},{"SkillSlot3","Habilidade 3"},
            {"Super","Super"},{"TrocarBarra","Barra"},{"Pocao","Poção"},{"Usar","Usar"},{"CombatDock","Barra de combate"},{"RegionalDodge","Esquiva"},
            {"RegionalStick","Joystick"},{"RegionalAttack","Ataque"},{"RegionalInteract","Interagir"},
        };
        static bool Celular => UnityEngine.Device.Application.isMobilePlatform;
        static string Chave(string id) => "ForjaLayout_" + (Celular ? "cel_" : "pc_") + id;

        public static LayoutDoHud Atual { get; private set; }
        public bool Editando { get; private set; }
        RectTransform raiz; RegionalWorld world;
        readonly List<Peca> pecas = new List<Peca>();

        public void Configurar(RectTransform root, RegionalWorld owner) { raiz = root; world = owner; Atual = this; StartCoroutine(Juntar()); }
        void OnDestroy() { if (Atual == this) Atual = null; }

        // The HUD is made over a few frames (the potion, the minimap and the phone row come later).
        IEnumerator Juntar()
        {
            yield return null; yield return null; yield return new WaitForSecondsRealtime(.3f);
            var canvas = raiz.GetComponentInParent<Canvas>()?.rootCanvas;
            var onde = canvas != null ? canvas.transform : raiz;
            foreach (var nome in Nomes)
            {
                if (Celular && nome == "MiniMapa") continue; // on the phone it jumps to the middle when opened
                RectTransform achado = null;
                foreach (var t in onde.GetComponentsInChildren<RectTransform>(true)) if (t.name == nome) { achado = t; break; }
                if (achado == null) continue;
                // A piece inside another moving piece moves with it (on the phone the row holds the buttons).
                if (Celular && achado.parent != null && achado.parent.name == "AcoesBaixo") continue;
                var p = achado.gameObject.AddComponent<Peca>(); p.Iniciar(nome, Rotulos.TryGetValue(nome, out var r) ? r : nome, this);
                pecas.Add(p);
            }
        }

        // ---------- The editing mode ----------
        RectTransform barra; Text barraTexto; Peca escolhida; readonly Dictionary<Peca, Vector3> antes = new Dictionary<Peca, Vector3>();
        public void Editar()
        {
            if (Editando || raiz == null) return;
            Editando = true;
            world?.Hero?.GetComponent<ArenaInput>()?.BlockCommands(true);
            antes.Clear(); foreach (var p in pecas) if (p != null) antes[p] = p.Salvo;
            foreach (var p in pecas) if (p != null) p.Mostrar(true);
            if (barra == null) MontarBarra();
            barra.gameObject.SetActive(true); barra.SetAsLastSibling();
            Escolher(null);
        }
        void Sair(bool salvar)
        {
            if (!salvar) foreach (var kv in antes) if (kv.Key != null) kv.Key.Salvo = kv.Value;
            foreach (var p in pecas) if (p != null) { p.Mostrar(false); if (salvar) p.Gravar(); }
            PlayerPrefs.Save();
            barra.gameObject.SetActive(false);
            Editando = false; escolhida = null;
            world?.Hero?.GetComponent<ArenaInput>()?.BlockCommands(false);
        }
        void Padrao() { foreach (var p in pecas) if (p != null) p.Salvo = new Vector3(0, 0, 1); Escolher(escolhida); }
        void Tamanho(float passo)
        {
            if (escolhida == null) { barraTexto.text = "Toque numa peça primeiro."; return; }
            var s = escolhida.Salvo; s.z = Mathf.Clamp(Mathf.Round((s.z + passo) * 10) / 10f, .6f, 1.8f); escolhida.Salvo = s; escolhida.Prender();
            Escolher(escolhida);
        }
        internal void Escolher(Peca p)
        {
            escolhida = p;
            foreach (var x in pecas) if (x != null) x.Destacar(x == p);
            barraTexto.text = p == null ? "Arraste as peças para onde quiser. Toque numa para mudar o tamanho."
                : p.Rotulo + " · tamanho " + Mathf.RoundToInt(p.Salvo.z * 100) + "%";
        }
        void MontarBarra()
        {
            barra = RusticHudTheme.Rect(raiz, "EditorDeLayout", new Vector2(.5f, .5f), new Vector2(0, 40), new Vector2(760, 150));
            var fundo = RusticHudTheme.Panel(barra, RusticHudTheme.Ink, 18); fundo.borderColor = RusticHudTheme.Gold;
            RusticHudTheme.Label(barra, "Titulo", new Vector2(.5f, 1), new Vector2(0, -12), new Vector2(720, 34), "EDITANDO O LAYOUT", 24, TextAnchor.MiddleCenter).color = RusticHudTheme.Gold;
            barraTexto = RusticHudTheme.Label(barra, "Ajuda", new Vector2(.5f, 1), new Vector2(0, -48), new Vector2(720, 30), "", 17, TextAnchor.MiddleCenter);
            float y = 16, h = Celular ? 50 : 42;
            RusticHudTheme.Button(barra, "Menor", new Vector2(0, 0), new Vector2(16, y), new Vector2(70, h), "−", () => Tamanho(-.1f));
            RusticHudTheme.Button(barra, "Maior", new Vector2(0, 0), new Vector2(92, y), new Vector2(70, h), "+", () => Tamanho(.1f));
            RusticHudTheme.Button(barra, "Padrao", new Vector2(0, 0), new Vector2(178, y), new Vector2(170, h), "PADRÃO", Padrao);
            RusticHudTheme.Button(barra, "Cancelar", new Vector2(0, 0), new Vector2(364, y), new Vector2(170, h), "CANCELAR", () => Sair(false));
            RusticHudTheme.Button(barra, "Salvar", new Vector2(0, 0), new Vector2(550, y), new Vector2(194, h), "SALVAR", () => Sair(true));
            // The bar itself can be dragged out of the way.
            barra.gameObject.AddComponent<ArrastarBarra>();
        }
        sealed class ArrastarBarra : MonoBehaviour, IDragHandler
        {
            public void OnDrag(PointerEventData data)
            {
                var rt = (RectTransform)transform; float s = rt.lossyScale.x > 0 ? rt.lossyScale.x : 1;
                rt.anchoredPosition += data.delta / s;
            }
        }

        // One movable piece: keeps its own place (the HUD's) plus the player's shift and size.
        internal sealed class Peca : MonoBehaviour
        {
            public string Rotulo { get; private set; }
            string id; LayoutDoHud dono; RectTransform rt; Vector2 baseAt; Vector3 baseEscala; Vector2 aplicado; bool comecou;
            public Vector3 Salvo = new Vector3(0, 0, 1); // x, y: shift; z: size
            GameObject capa; RoundedHudGraphic contorno;

            public void Iniciar(string i, string rotulo, LayoutDoHud d)
            {
                id = i; Rotulo = rotulo; dono = d; rt = (RectTransform)transform;
                var s = PlayerPrefs.GetString(Chave(id), "");
                var partes = s.Split(';');
                if (partes.Length == 3 && float.TryParse(partes[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var x)
                    && float.TryParse(partes[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var y)
                    && float.TryParse(partes[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var z))
                    Salvo = new Vector3(x, y, Mathf.Clamp(z, .6f, 1.8f));
                baseAt = rt.anchoredPosition; baseEscala = rt.localScale; aplicado = baseAt; comecou = true;
                Aplicar(); Prender();
            }
            public void Gravar()
            {
                var c = System.Globalization.CultureInfo.InvariantCulture;
                if (Salvo == new Vector3(0, 0, 1)) PlayerPrefs.DeleteKey(Chave(id));
                else PlayerPrefs.SetString(Chave(id), Salvo.x.ToString(c) + ";" + Salvo.y.ToString(c) + ";" + Salvo.z.ToString(c));
            }
            // If the HUD itself moved the piece (opening a panel...), that is its new home; the player's shift stays on top.
            void LateUpdate() { if (!comecou) return; if ((rt.anchoredPosition - aplicado).sqrMagnitude > .01f) baseAt = rt.anchoredPosition; Aplicar(); }
            bool escalou;
            void Aplicar()
            {
                rt.anchoredPosition = aplicado = baseAt + new Vector2(Salvo.x, Salvo.y);
                // Size only when changed (so the buttons keep their little "press" bounce at 100%).
                if (Mathf.Abs(Salvo.z - 1) > .01f) { rt.localScale = baseEscala * Salvo.z; escalou = true; }
                else if (escalou) { rt.localScale = baseEscala; escalou = false; }
            }
            // Keeps the whole piece on the screen.
            public void Prender()
            {
                Aplicar();
                var canvas = rt.GetComponentInParent<Canvas>()?.rootCanvas; if (canvas == null) return;
                var tela = (RectTransform)canvas.transform;
                var cantos = new Vector3[4]; rt.GetWorldCorners(cantos);
                Vector2 min = tela.InverseTransformPoint(cantos[0]), max = tela.InverseTransformPoint(cantos[2]);
                var r = tela.rect; Vector2 mover = Vector2.zero;
                if (min.x < r.xMin) mover.x = r.xMin - min.x; else if (max.x > r.xMax) mover.x = r.xMax - max.x;
                if (min.y < r.yMin) mover.y = r.yMin - min.y; else if (max.y > r.yMax) mover.y = r.yMax - max.y;
                if (mover == Vector2.zero) return;
                // From screen units to the piece's parent units.
                var pai = rt.parent as RectTransform; float k = pai != null && pai.lossyScale.x > 0 ? tela.lossyScale.x / pai.lossyScale.x : 1;
                Salvo += new Vector3(mover.x * k, mover.y * k, 0); Aplicar();
            }
            public void Mover(Vector2 telaDelta)
            {
                var pai = rt.parent as RectTransform; float s = pai != null && pai.lossyScale.x > 0 ? pai.lossyScale.x : 1;
                Salvo += (Vector3)(telaDelta / s); Prender();
            }
            public void Mostrar(bool sim)
            {
                if (sim && capa == null)
                {
                    var c = RusticHudTheme.Rect(rt, "CapaDoLayout", Vector2.one * .5f, Vector2.zero, Vector2.zero);
                    c.anchorMin = Vector2.zero; c.anchorMax = Vector2.one; c.offsetMin = new Vector2(-4, -4); c.offsetMax = new Vector2(4, 4);
                    contorno = c.gameObject.AddComponent<RoundedHudGraphic>(); contorno.radius = 8; contorno.border = 2;
                    contorno.color = new Color(1f, .85f, .4f, .14f); contorno.borderColor = new Color(1f, .85f, .4f, .9f); contorno.raycastTarget = true;
                    var nome = RusticHudTheme.Label(c, "Nome", new Vector2(.5f, 1), new Vector2(0, 14), new Vector2(200, 22), Rotulo, 13, TextAnchor.MiddleCenter);
                    nome.color = RusticHudTheme.Gold; nome.raycastTarget = false;
                    c.gameObject.AddComponent<Pegar>().peca = this;
                    capa = c.gameObject;
                }
                if (capa != null) { capa.SetActive(sim); if (sim) capa.transform.SetAsLastSibling(); }
            }
            public void Destacar(bool sim) { if (contorno != null) contorno.color = new Color(1f, .85f, .4f, sim ? .38f : .14f); }
            internal void Escolhida() => dono.Escolher(this);
        }
        // The cover over a piece while editing: catches the touch (the button under it is not used) and drags.
        sealed class Pegar : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerClickHandler
        {
            public Peca peca;
            public void OnPointerDown(PointerEventData data) => peca.Escolhida();
            public void OnDrag(PointerEventData data) => peca.Mover(data.delta);
            public void OnPointerClick(PointerEventData data) { }
        }
    }
}
