using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // The places of the world that can be walked, and how they connect (claude/mundo-areas.md).
    // Compass: north of the world map = +Z in every scene, east = +X.
    // Up to 4 passages per place, only between places that share a border on the map.
    public static class AreasDoMundo
    {
        public enum Lado { N, NE, L, SE, S, SO, O, NO }
        public sealed class Saida
        {
            public Lado lado; public string para; public string trava; public Vector3? ponto;
            public Saida(Lado l, string destino, string travada = null, Vector3? onde = null) { lado = l; para = destino; trava = travada; ponto = onde; }
        }
        public sealed class Area
        {
            public string id, cena; public bool cidade; public Saida[] saidas;
            public Area(string i, string c, bool city, params Saida[] s) { id = i; cena = c; cidade = city; saidas = s; }
        }

        // Closed ways: only what the player sees. How to open them is told by people in the world, not here.
        public const string Frio = "Frio demais para seguir.\nÉ preciso se equipar melhor.";
        public const string Desmoronamento = "Pedras caídas fecham o caminho.";
        public const string Correnteza = "A correnteza não deixa passar.";

        public static readonly Area[] Todas =
        {
            // Refúgio: on the bridge over the river (south), just outside the west and east gates. North comes later.
            new Area("refugio","RefugioUltimoSino",true,
                new Saida(Lado.S,"nascente",null,new Vector3(0,3,-47)), new Saida(Lado.O,"bosque",null,new Vector3(-46,0,0)), new Saida(Lado.L,"vale",null,new Vector3(46,0,0))),
            new Area("bosque","Area_Bosque",false, new Saida(Lado.SE,"refugio"), new Saida(Lado.S,"trilhos"), new Saida(Lado.SO,"estacao1"), new Saida(Lado.N,"passo",Frio)),
            // From the Nascente there is no direct way to the Vila da Foz: through the Trilhos Velhos or the Gruta da Cachoeira.
            new Area("nascente","Area_Nascente",false, new Saida(Lado.N,"refugio"), new Saida(Lado.SO,"trilhos"), new Saida(Lado.S,"gruta"), new Saida(Lado.L,"campos")),
            new Area("vale","Area_Vale",false, new Saida(Lado.O,"refugio"), new Saida(Lado.SO,"campos")),
            new Area("campos","Area_Campos",false, new Saida(Lado.N,"vale"), new Saida(Lado.O,"nascente"), new Saida(Lado.SO,"gruta"), new Saida(Lado.SE,"riolargo",Correnteza)),
            new Area("gruta","Area_Gruta",false, new Saida(Lado.N,"nascente"), new Saida(Lado.NE,"campos"), new Saida(Lado.SO,"foz")),
            new Area("estacao1","Area_EstacaoAntiga",false, new Saida(Lado.NE,"bosque"), new Saida(Lado.SE,"trilhos"), new Saida(Lado.NO,"mina"), new Saida(Lado.O,"desfiladeiro",Desmoronamento)),
            new Area("mina","Area_Mina",false, new Saida(Lado.SE,"estacao1")),
            new Area("trilhos","Area_Trilhos",false, new Saida(Lado.N,"bosque"), new Saida(Lado.NO,"estacao1"), new Saida(Lado.NE,"nascente"), new Saida(Lado.SE,"foz")),
            new Area("foz","Area_Foz",true, new Saida(Lado.NO,"trilhos"), new Saida(Lado.NE,"gruta"), new Saida(Lado.SO,"costa"), new Saida(Lado.L,"cemiterio")),
            new Area("costa","Area_Costa",false, new Saida(Lado.NE,"foz")),
            new Area("cemiterio","Area_Cemiterio",false, new Saida(Lado.O,"foz"), new Saida(Lado.N,"riolargo",Correnteza)),
        };

        public static Area PorId(string id) { foreach (var a in Todas) if (a.id == id) return a; return null; }
        public static Area PorCena(string cena) { foreach (var a in Todas) if (a.cena == cena) return a; return null; }
        // Wild places (not cities): no free rest.
        public static bool Selvagem(string cena) { var a = PorCena(cena); return a != null && !a.cidade; }

        public static Vector3 Direcao(Lado lado)
        {
            switch (lado)
            {
                case Lado.N: return new Vector3(0, 0, 1);
                case Lado.NE: return new Vector3(1, 0, 1).normalized;
                case Lado.L: return new Vector3(1, 0, 0);
                case Lado.SE: return new Vector3(1, 0, -1).normalized;
                case Lado.S: return new Vector3(0, 0, -1);
                case Lado.SO: return new Vector3(-1, 0, -1).normalized;
                case Lado.O: return new Vector3(-1, 0, 0);
                default: return new Vector3(-1, 0, 1).normalized;
            }
        }

        // Where the exit is: fixed (cities) or the end of the road the sketch builder made (60 m out, square edge).
        public static Vector3 Ponto(Saida s)
        {
            if (s.ponto.HasValue) return s.ponto.Value;
            var d = Direcao(s.lado);
            return d * 60f / Mathf.Max(Mathf.Abs(d.x), Mathf.Abs(d.z));
        }

        public static string Nome(string id)
        {
            foreach (var l in MapaDoMundo.Lugares) if (l.id == id) return l.nome;
            return id;
        }

        // Where the player came from (to arrive at the matching exit of the next place).
        public static string VindoDe;

        // Last city the player entered (for "voltar à cidade").
        static string Who => string.IsNullOrEmpty(CampaignHome.CharacterName) ? "padrao" : CampaignHome.CharacterName;
        public static string UltimaCidade { get => PlayerPrefs.GetString("ForjaCidade_" + Who, "refugio"); set { PlayerPrefs.SetString("ForjaCidade_" + Who, value); PlayerPrefs.Save(); } }

        // Exits of a scene for the minimap: position and text.
        public static IEnumerable<(Vector3 ponto, string texto)> Placas(string cena)
        {
            var a = PorCena(cena);
            if (a == null) yield break;
            foreach (var s in a.saidas) yield return (Ponto(s), (s.trava != null ? "Fechado: " : "Saída para ") + Nome(s.para));
        }
    }

    // The exits of the current place: an arrow on the ground with the name of the next place.
    // Walking past it travels and you arrive at the matching exit. Closed exits only say what you see.
    // Outside cities: "voltar à cidade" (H or the button) takes you back to the last city after a short wait.
    public sealed class PortoesDoMundo : MonoBehaviour
    {
        sealed class Portao { public AreasDoMundo.Saida saida; public Vector3 ponto, fora; public Transform seta; public TextMesh nome; public WorldClickTarget alvo; }
        readonly List<Portao> portoes = new List<Portao>();
        RegionalWorld world; AreasDoMundo.Area area; bool saindo;
        Material ouro, fechado;

        public static void Criar(RegionalWorld world)
        {
            var area = AreasDoMundo.PorCena(SceneManager.GetActiveScene().name);
            if (area == null) return;
            var go = new GameObject("PortoesDoMundo");
            var p = go.AddComponent<PortoesDoMundo>(); p.world = world; p.area = area;
        }

        void Start()
        {
            DescobertasDoMapa.Descobrir(area.id);
            if (area.cidade) AreasDoMundo.UltimaCidade = area.id;
            // The old exit of the city (to the old coast) is replaced by these exits.
            foreach (var old in FindObjectsByType<TownRoadPassage>(FindObjectsSortMode.None)) { old.enabled = false; foreach (var c in old.GetComponents<Collider>()) c.enabled = false; }
            foreach (var t in FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None)) if (t.Label.IndexOf("Costa", StringComparison.OrdinalIgnoreCase) >= 0) t.enabled = false;
            ouro = new Material(Shader.Find("Universal Render Pipeline/Unlit")); ouro.SetColor("_BaseColor", new Color(1f, .82f, .38f));
            fechado = new Material(Shader.Find("Universal Render Pipeline/Unlit")); fechado.SetColor("_BaseColor", new Color(.75f, .32f, .28f));
            foreach (var s in area.saidas)
            {
                var ponto = AreasDoMundo.Ponto(s);
                if (NavMesh.SamplePosition(ponto, out var hit, ponto.y > 1 ? 3 : 6, NavMesh.AllAreas)) ponto = hit.position;
                var portao = new Portao { saida = s, ponto = ponto, fora = AreasDoMundo.Direcao(s.lado) };
                Seta(portao);
                portoes.Add(portao);
            }
            // Arrived from another place: start just inside the exit that leads back there.
            string vindo = AreasDoMundo.VindoDe; AreasDoMundo.VindoDe = null;
            if (!string.IsNullOrEmpty(vindo))
                foreach (var p in portoes)
                    if (p.saida.para == vindo)
                    {
                        var at = p.ponto - p.fora * 8f;
                        if (NavMesh.SamplePosition(at, out var hit, 5, NavMesh.AllAreas)) at = hit.position;
                        world.Hero.GetComponent<PlayerMotor>()?.Teleport(at, Quaternion.LookRotation(-p.fora));
                        FindFirstObjectByType<ArenaCamera>()?.Snap();
                        break;
                    }
            Botoes();
            Debug.Log("PORTOES " + area.id + " " + portoes.Count);
        }

        void Update()
        {
            if (world == null || world.Hero == null) return;
            foreach (var p in portoes) if (p.seta != null) p.seta.localPosition = new Vector3(0, .06f, Mathf.Sin(Time.time * 3) * .25f);
            if (saindo || !world.Hero.Health.IsAlive) return;
            if (!area.cidade && Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame) Voltar();
            if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame && !MapaDoMundo.Aberto && MapaDoMundo.FechadoNoQuadro != Time.frameCount) AbrirMapa();
            var hero = world.Hero.transform.position;
            foreach (var p in portoes)
            {
                Vector3 d = hero - p.ponto; d.y = 0;
                // The name shows only when you get close.
                if (p.nome != null) p.nome.gameObject.SetActive(d.magnitude < 11f);
                // F next to the arrow also travels (clicking/tapping the arrow walks there and travels).
                if (p.alvo != null && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame && p.alvo.InRange(world.Hero.transform)) p.alvo.TryUse(world.Hero.transform);
            }
        }

        void Ir(string destinoId, string vindoDe)
        {
            var destino = AreasDoMundo.PorId(destinoId);
            if (destino == null || !Application.CanStreamedLevelBeLoaded(destino.cena)) return;
            saindo = true;
            AreasDoMundo.VindoDe = vindoDe;
            DescobertasDoMapa.Descobrir(destino.id);
            world.Viajar(destino.cena);
        }

        // Arrow lying on the ground, pointing out, and the name of the next place above it (seen when close).
        // Travel only when the player clicks/taps the arrow (or presses F next to it), never by just walking past.
        void Seta(Portao p)
        {
            var raiz = new GameObject("Saida_" + p.saida.para).transform;
            raiz.SetParent(transform, false);
            raiz.position = p.ponto; raiz.rotation = Quaternion.LookRotation(p.fora);
            bool travado = p.saida.trava != null;
            var seta = new GameObject("Seta", typeof(MeshFilter), typeof(MeshRenderer)).transform;
            seta.SetParent(raiz, false);
            seta.GetComponent<MeshFilter>().sharedMesh = MalhaDaSeta();
            var r = seta.GetComponent<MeshRenderer>(); r.sharedMaterial = travado ? fechado : ouro; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            p.seta = seta;
            var texto = new GameObject("Nome").AddComponent<TextMesh>();
            texto.transform.SetParent(raiz, false);
            texto.transform.position = raiz.position + Vector3.up * 1.6f;
            texto.transform.rotation = Quaternion.Euler(40, 0, 0);
            texto.text = AreasDoMundo.Nome(p.saida.para) + (travado ? "\n" + p.saida.trava : "");
            texto.fontSize = 44; texto.characterSize = travado ? .055f : .075f; texto.anchor = TextAnchor.LowerCenter; texto.alignment = TextAlignment.Center;
            texto.color = travado ? new Color(1f, .7f, .62f) : new Color(1f, .93f, .72f);
            p.nome = texto; texto.gameObject.SetActive(false);
            if (travado) return;
            // Something to click: a flat box over the arrow (Default layer: does not change the walkable ground).
            var clique = new GameObject("Clique").AddComponent<BoxCollider>();
            clique.transform.SetParent(raiz, false); clique.transform.localPosition = new Vector3(0, .3f, 0);
            clique.size = new Vector3(3.2f, .6f, 4f); clique.gameObject.layer = 0;
            var alvo = raiz.gameObject.AddComponent<WorldClickTarget>();
            alvo.Configure("Ir para " + AreasDoMundo.Nome(p.saida.para), 3.2f);
            var destino = p.saida.para;
            alvo.Interacting += actor => { if (actor == world.Hero.transform && !saindo && world.Hero.Health.IsAlive) Ir(destino, area.id); };
            p.alvo = alvo;
        }

        static Mesh malha;
        static Mesh MalhaDaSeta()
        {
            if (malha != null) return malha;
            // Flat arrow pointing to +Z: a shaft and a head.
            var v = new[] {
                new Vector3(-.45f,0,-1.6f), new Vector3(.45f,0,-1.6f), new Vector3(.45f,0,.3f), new Vector3(-.45f,0,.3f),
                new Vector3(-1.2f,0,.3f), new Vector3(1.2f,0,.3f), new Vector3(0,0,1.8f) };
            malha = new Mesh { name = "Seta" };
            malha.vertices = v; malha.triangles = new[] { 0, 2, 1, 0, 3, 2, 4, 6, 5 };
            malha.RecalculateNormals(); malha.RecalculateBounds();
            return malha;
        }

        // ---------- Back to the last city ----------
        Text botaoTexto; bool voltando; int abriuEm;
        // World map from anywhere (to see where you are). Travel only at a Raiz-Mãe.
        void AbrirMapa() { if (MapaDoMundo.Aberto || saindo) return; abriuEm = Time.frameCount; MapaDoMundo.Abrir(world, false); }
        // Buttons beside the skill bar: the world map (all places) and back to the city (wild places only).
        void Botoes()
        {
            var go = new GameObject("VoltarCidadeUI", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var canvas = go.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 500;
            var scaler = go.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            go.AddComponent<GraphicRaycaster>();
            RusticHudTheme.Button(go.transform, "Mapa", new Vector2(.5f, 0), new Vector2(-420, 22), new Vector2(190, 56), "MAPA (K)", AbrirMapa);
            if (area.cidade) return;
            var b = RusticHudTheme.Button(go.transform, "Voltar", new Vector2(.5f, 0), new Vector2(430, 22), new Vector2(210, 56), "VOLTAR (H)", Voltar);
            botaoTexto = b.GetComponentInChildren<Text>();
        }

        void Voltar() { if (!voltando && !saindo) StartCoroutine(Voltando()); }

        System.Collections.IEnumerator Voltando()
        {
            voltando = true;
            var health = world.Hero.Health; bool ferido = false;
            Action hit = () => ferido = true; health.Damaged += hit;
            var start = world.Hero.transform.position;
            string antes = botaoTexto != null ? botaoTexto.text : "";
            // A short wait: it does not work to run away from a fight.
            for (float t = 3; t > 0; t -= Time.deltaTime)
            {
                if (botaoTexto != null) botaoTexto.text = "Voltando... " + Mathf.CeilToInt(t);
                if (ferido || (world.Hero.transform.position - start).sqrMagnitude > 1 || !health.IsAlive) break;
                yield return null;
            }
            health.Damaged -= hit;
            bool ok = !ferido && (world.Hero.transform.position - start).sqrMagnitude <= 1 && health.IsAlive;
            if (botaoTexto != null) botaoTexto.text = ok ? "Voltando..." : antes;
            voltando = false;
            if (!ok) yield break;
            var cidade = AreasDoMundo.PorId(AreasDoMundo.UltimaCidade) ?? AreasDoMundo.PorId("refugio");
            saindo = true;
            AreasDoMundo.VindoDe = null;
            if (cidade.id == "refugio") Historia.RenascerNoNo = true; // arrives at the Raiz-Mãe
            world.Viajar(cidade.cena);
        }
    }

    // Makes the water of a river flow (moves its texture slowly).
    public sealed class RioAnimado : MonoBehaviour
    {
        public Vector2 velocidade = new Vector2(.12f, .015f);
        Renderer view; Material own;
        void Start() { view = GetComponent<Renderer>(); if (view != null) own = view.material; }
        void Update() { if (own != null) own.SetTextureOffset("_BaseMap", velocidade * Time.time); }
        void OnDestroy() { if (own != null) Destroy(own); }
    }
}
