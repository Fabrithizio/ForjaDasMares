using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // Places the player has discovered (saved per character). The Refúgio is always known.
    public static class DescobertasDoMapa
    {
        public static bool RevelarTudo; // editor test only
        static HashSet<string> cache; static string cacheKey;
        static string Key => "ForjaMapa_" + (string.IsNullOrEmpty(CampaignHome.CharacterName) ? "padrao" : CampaignHome.CharacterName);
        static HashSet<string> Set()
        {
            if (cache == null || cacheKey != Key)
            {
                cacheKey = Key;
                cache = new HashSet<string>(PlayerPrefs.GetString(Key, "").Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
                cache.Add("refugio");
            }
            return cache;
        }
        public static bool Conhece(string id) => RevelarTudo || Set().Contains(id);
        public static void Descobrir(string id)
        {
            if (!Set().Add(id)) return;
            PlayerPrefs.SetString(Key, string.Join(",", cache)); PlayerPrefs.Save();
        }
    }

    // World map opened at the Estação do Último Sino (centre of the Refúgio). Official art + the 52 places of the
    // map guide; what was not discovered stays under the fog. Drag to move, wheel or pinch to zoom (PC and phone).
    public sealed class MapaDoMundo : MonoBehaviour
    {
        public readonly struct Lugar
        {
            public readonly string id, nome, regiao, tipo, texto; public readonly float x, y;
            public Lugar(string id, string nome, string regiao, string tipo, float x, float y, string texto)
            { this.id = id; this.nome = nome; this.regiao = regiao; this.tipo = tipo; this.x = x; this.y = y; this.texto = texto; }
        }
        public readonly struct Regiao
        {
            public readonly string nome, capitulo, nivel;
            public Regiao(string nome, string capitulo, string nivel) { this.nome = nome; this.capitulo = capitulo; this.nivel = nivel; }
        }
        public static readonly Dictionary<string, Regiao> Regioes = new Dictionary<string, Regiao>
        {
            {"c1", new Regiao("Coração Verde","Capítulo 1","1–15")},
            {"c2", new Regiao("Planalto Partido","Capítulo 2","15–30")},
            {"c3", new Regiao("Selva Profunda","Capítulo 3","30–45")},
            {"c4", new Regiao("Picos do Sino","Capítulo 4","45–60")},
        };
        // x, y = percent of the map image (0 = left/top).
        public static readonly Lugar[] Lugares =
        {
            new Lugar("refugio","Refúgio do Último Sino","c1","cidade",52.0f,44.0f,"Cidade nas raízes da árvore gigante, coração da teia. O sino rachado só traz, nunca leva. Onde todo Chegado cai."),
            new Lugar("bosque","Bosque Luzente","c1","lugar",44.0f,37.0f,"Primeira área: plantas que brilham, criaturas da teia, quase nenhum perigo. Ruína com o mapa gravado que mostra a vila do jogador."),
            new Lugar("nascente","Nascente do Rio","c1","lugar",53.0f,51.0f,"Água que brilha, nasce do coração. Pequenos segredos escondidos nas pedras."),
            new Lugar("trilhos","Trilhos Velhos","c1","lugar",42.0f,57.0f,"Trilho do povo antigo. Caminho rápido, vigiado por sentinelas flutuantes."),
            new Lugar("vale","Vale dos Cogumelos","c1","lugar",64.0f,47.0f,"Cogumelos enormes. Cogumelos saltadores agitados pelo cristal que vaza."),
            new Lugar("gruta","Gruta da Cachoeira","c1","segredo",56.0f,61.0f,"Passagem secreta atrás da cachoeira: atalho direto para a Vila da Foz."),
            new Lugar("estacao1","Estação Antiga","c1","estacao",37.0f,47.0f,"Guardada pelo Vigia da Estação. Religar dá a primeira viagem rápida."),
            new Lugar("campos","Campos Adormecidos","c1","lugar",61.0f,56.0f,"Fazendas abandonadas, natureza comum, bichos e bandidos. Mostra o que a drenagem fez."),
            new Lugar("mina","Mina de Cristal","c1","lugar",31.0f,37.0f,"Cristal vazando: magos e servos de cristal. Bom material para a Forja."),
            new Lugar("foz","Vila da Foz","c1","cidade",52.0f,68.0f,"Vila de pescadores que resiste na boca do rio, ao lado de uma estação morta."),
            new Lugar("costa","Costa dos Naufrágios","c1","lugar",44.0f,79.0f,"Onde o Breu começa: marinheiros afogados se levantam na areia."),
            new Lugar("cemiterio","Cemitério de Navios","c1","covil",66.0f,78.0f,"Navios afundados no Breu e o farol velho. Casa do Capitão Afogado; sob o farol, a estação do porto e o Faroleiro."),
            new Lugar("desfiladeiro","Desfiladeiro da Entrada","c2","lugar",28.0f,43.0f,"Passagem nas montanhas entre o Coração Verde e o oeste seco."),
            new Lugar("pouso","Pouso Alto","c2","cidade",31.0f,28.0f,"Cidade de caravanas na porta do planalto. Comércio entre o oeste e o centro."),
            new Lugar("areia","Mar de Areia Vermelha","c2","lugar",18.0f,45.0f,"Dunas vermelhas de terra drenada. Tempestades de areia e bichos enterrados."),
            new Lugar("oasis","Oásis Escondido","c2","segredo",22.0f,62.0f,"Bolsão de natureza viva no meio do deserto. Só aparece para quem explora."),
            new Lugar("ventania","Ventania","c2","cidade",12.0f,25.0f,"Cidade de moinhos no alto do planalto, sempre ventando."),
            new Lugar("canion","Cânion dos Ecos","c2","lugar",24.0f,68.0f,"Paredões que repetem vozes. Armadilhas antigas e emboscadas."),
            new Lugar("cantaria","Cantaria","c2","cidade",11.0f,55.0f,"Cidade escavada na rocha por pedreiros. Guardam pedras do império antigo."),
            new Lugar("ponte","Ponte Quebrada","c2","lugar",20.0f,29.0f,"Ponte enorme do império, partida ao meio. Travessia perigosa."),
            new Lugar("ruinas","Ruínas da Capital Antiga","c2","lugar",25.0f,20.0f,"A capital do império caído. Pistas sobre os Guardiões do Sino."),
            new Lugar("batalha","Campo da Última Batalha","c2","lugar",26.0f,48.0f,"Campo de batalha antigo: o Breu levanta soldados de pedra."),
            new Lugar("sal","Poços de Sal","c2","lugar",17.0f,53.0f,"Lagos secos de sal branco. Cristais raros e criaturas de casca."),
            new Lugar("ossada","Ossada dos Gigantes","c2","lugar",11.0f,40.0f,"Esqueletos de criaturas imensas que morreram quando a terra secou."),
            new Lugar("estacao2","Estação do Deserto","c2","estacao",32.0f,59.0f,"Estação meio enterrada na areia. Religar abre a viagem rápida do oeste."),
            new Lugar("cidadela","Cidadela Soterrada","c2","covil",33.0f,65.0f,"Fortaleza engolida pela areia. Covil do General de Pedra."),
            new Lugar("riolargo","Rio Largo","c3","lugar",63.0f,65.0f,"O rio vira largo e lento ao entrar na selva. Caminho de barco para o leste."),
            new Lugar("porto","Porto Raiz","c3","cidade",70.0f,66.0f,"Cidade de barcos e palafitas sobre as raízes. Porta da selva."),
            new Lugar("pantano","Pântano Luminoso","c3","lugar",77.0f,60.0f,"Água parada que brilha à noite. Bonito e traiçoeiro."),
            new Lugar("cachoeiras","Cachoeiras Gêmeas","c3","lugar",84.0f,37.0f,"Duas quedas enormes lado a lado, com cavernas atrás."),
            new Lugar("lago","Lago dos Espelhos","c3","lugar",77.0f,48.0f,"Lago parado que reflete o céu como espelho. Segredos no fundo."),
            new Lugar("mariposas","Ninho das Mariposas-Lua","c3","lugar",70.0f,33.0f,"Mariposas gigantes que brilham. Criaturas da teia, podem ser aliadas."),
            new Lugar("ilhas","Ilhas Flutuantes","c3","lugar",89.0f,20.0f,"Pedaços de terra que flutuam na energia da teia. Pontes de cipó."),
            new Lugar("ninho","Ninho","c3","cidade",80.0f,22.0f,"Cidade construída nas copas das árvores gigantes."),
            new Lugar("templo","Templo das Raízes","c3","lugar",75.0f,12.0f,"Templo antigo enrolado em raízes vivas. Pistas sobre a teia."),
            new Lugar("copas","Copas Suspensas","c3","lugar",65.0f,30.0f,"Caminhos pelos galhos, muito acima do chão."),
            new Lugar("aldeia","Aldeia Tecelã","c3","cidade",86.0f,46.0f,"Povo que ainda conversa com a teia. Sabem coisas que ninguém mais sabe."),
            new Lugar("estacao3","Estação da Selva","c3","estacao",70.0f,56.0f,"Estação tomada por cipós. Religar abre a viagem rápida do leste."),
            new Lugar("podre","Floresta Podre","c3","lugar",82.0f,71.0f,"Onde o Breu apodreceu as raízes: árvores mortas e bichos de galho podre."),
            new Lugar("raiz","Raiz Ferida","c3","covil",87.0f,60.0f,"A raiz maior, aberta e escura. Covil da Tecelã Partida."),
            new Lugar("passo","Passo Gelado","c4","lugar",46.0f,28.0f,"Passagem nas montanhas ao norte do Refúgio. Fechada até nível alto."),
            new Lugar("brasa","Vila Brasa","c4","cidade",38.0f,13.0f,"Vila de mineiros aquecida por cristais. Fumaça e luz no meio da neve."),
            new Lugar("lagogelo","Lago Congelado","c4","lugar",37.0f,19.0f,"Lago de gelo grosso. Coisas presas embaixo do gelo."),
            new Lugar("cristal","Floresta de Cristal","c4","lugar",68.0f,17.0f,"Árvores que viraram cristal. Brilha e canta com o vento."),
            new Lugar("minas","Minas Profundas","c4","lugar",40.0f,25.0f,"Minas fundas dos Guardiões. Cristais puros e perigos antigos."),
            new Lugar("auroras","Vale das Auroras","c4","segredo",62.0f,7.0f,"Vale escondido onde o céu acende à noite. O segredo mais bonito do norte."),
            new Lugar("estacao4","Estação do Cume","c4","estacao",42.0f,8.0f,"Estação no topo do mundo. Religar abre a viagem rápida do norte."),
            new Lugar("mosteiro","Mosteiro Alto","c4","cidade",50.0f,12.0f,"Monges que guardam os registros antigos dos Guardiões do Sino."),
            new Lugar("escadaria","Escadaria dos Mil Degraus","c4","lugar",49.0f,19.0f,"Subida longa até a cidadela, cheia de provas."),
            new Lugar("geleira","Geleira dos Sussurros","c4","lugar",73.0f,8.0f,"Gelo que sussurra vozes do passado."),
            new Lugar("guardioes","Cidadela dos Guardiões","c4","lugar",63.0f,20.0f,"Ruína da sede da ordem. A verdade sobre o Sineiro."),
            new Lugar("sino","Sino Primeiro","c4","covil",52.0f,5.0f,"O primeiro sino da rede. Covil do Sineiro tomado pelo Breu."),
        };

        static MapaDoMundo open;
        public static bool Aberto => open != null;
        RegionalWorld world;
        RectTransform viewport, content;
        Text title, region, body, status;
        Button travel;
        float zoom = 1, pinch;
        static readonly Color Fog = new Color(.02f, .04f, .07f, .92f);

        public static void Abrir(RegionalWorld world, bool estacao)
        {
            if (open != null) return;
            var go = new GameObject("MapaDoMundo");
            open = go.AddComponent<MapaDoMundo>();
            open.world = world;
            open.Build(estacao);
            var input = world.Hero.GetComponent<ArenaInput>();
            if (input != null) { input.Desktop?.Cancel(); input.BlockCommands(true); }
            world.Hero.GetComponent<PlayerMotor>()?.StopWalking();
        }

        public void Fechar()
        {
            if (world != null && world.Hero != null) world.Hero.GetComponent<ArenaInput>()?.BlockCommands(false);
            open = null;
            Destroy(gameObject);
        }

        void Build(bool estacao)
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 30000; // above the game HUD
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if (FindFirstObjectByType<EventSystem>() == null) new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            var root = (RectTransform)transform;

            var backdrop = Stretch(new GameObject("Fundo", typeof(RectTransform)).GetComponent<RectTransform>(), root, 0, 0, 0, 0);
            backdrop.gameObject.AddComponent<Image>().color = new Color(.02f, .035f, .05f, 1f);

            RusticHudTheme.Label(root, "Titulo", new Vector2(0, 1), new Vector2(330, -42), new Vector2(600, 44), "MAPA DO MUNDO", 34).color = RusticHudTheme.Gold;
            RusticHudTheme.Label(root, "Dica", new Vector2(0, 1), new Vector2(560, -78), new Vector2(1060, 28),
                estacao ? "Estação do Último Sino · toque num lugar descoberto · arraste para mover · roda ou pinça para aproximar"
                        : "Toque num lugar descoberto · arraste para mover · roda ou pinça para aproximar", 18).color = RusticHudTheme.Muted;
            RusticHudTheme.Button(root, "Fechar", new Vector2(1, 1), new Vector2(-60, -50), new Vector2(84, 56), "X", Fechar);

            // Map area (left) and information panel (right).
            viewport = Stretch(new GameObject("Viewport", typeof(RectTransform)).GetComponent<RectTransform>(), root, 36, 470, 104, 36);
            viewport.gameObject.AddComponent<RectMask2D>();
            var catcher = viewport.gameObject.AddComponent<Image>(); catcher.color = new Color(0, 0, 0, .35f);
            viewport.gameObject.AddComponent<Arrastar>().map = this;
            content = new GameObject("Mapa", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(viewport, false); content.anchorMin = content.anchorMax = content.pivot = new Vector2(.5f, .5f);
            var art = new GameObject("Arte", typeof(RectTransform)).GetComponent<RectTransform>();
            Stretch(art, content, 0, 0, 0, 0);
            var image = art.gameObject.AddComponent<RawImage>(); image.texture = Resources.Load<Texture2D>("Mapa/MapaMundi"); image.raycastTarget = false;
            if (image.texture == null) image.color = new Color(.1f, .25f, .3f);
            var fog = new GameObject("Nevoa", typeof(RectTransform)).GetComponent<RectTransform>();
            Stretch(fog, content, 0, 0, 0, 0);
            LoadAreas();
            var lit = new GameObject("Destaque", typeof(RectTransform)).GetComponent<RectTransform>();
            Stretch(lit, content, 0, 0, 0, 0);
            highlightImage = lit.gameObject.AddComponent<RawImage>(); highlightImage.texture = highlight; highlightImage.raycastTarget = false;
            if (highlight == null) highlightImage.enabled = false;
            var fogImage = fog.gameObject.AddComponent<RawImage>(); fogImage.texture = FogTexture(); fogImage.raycastTarget = false;
            if (fogImage.texture == null) fogImage.enabled = false;
            borders = new GameObject("Divisas", typeof(RectTransform)).GetComponent<RectTransform>();
            Stretch(borders, content, 0, 0, 0, 0);
            var lines = borders.gameObject.AddComponent<RawImage>(); lines.texture = Resources.Load<Texture2D>("Mapa/MapaDivisas"); lines.raycastTarget = false;
            if (lines.texture == null) lines.enabled = false;
            var names = new GameObject("NomesDasRegioes", typeof(RectTransform)).GetComponent<RectTransform>();
            Stretch(names, content, 0, 0, 0, 0);
            foreach (var (key, x, y) in RegionLabels)
            {
                var label = RusticHudTheme.Label(names, key, new Vector2(x / 100f, 1 - y / 100f), Vector2.zero, new Vector2(420, 40), Regioes[key].nome.ToUpperInvariant(), 26, TextAnchor.MiddleCenter);
                label.color = new Color(1, .96f, .86f, .82f); label.fontStyle = FontStyle.Bold;
                var o = label.gameObject.AddComponent<Outline>(); o.effectColor = new Color(0, 0, 0, .85f); o.effectDistance = new Vector2(2, -2);
            }
            // Only the travel points (cities and stations); every name shows when the pointer or finger is on it.
            var points = new GameObject("Pontos", typeof(RectTransform)).GetComponent<RectTransform>();
            Stretch(points, content, 0, 0, 0, 0);
            foreach (var lugar in Lugares)
                if ((lugar.tipo == "cidade" || lugar.tipo == "estacao") && DescobertasDoMapa.Conhece(lugar.id)) Point(points, lugar);
            tooltip = RusticHudTheme.Label(content, "NomeDaArea", new Vector2(.5f, .5f), Vector2.zero, new Vector2(360, 30), "", 20, TextAnchor.MiddleCenter);
            tooltipRect = (RectTransform)tooltip.transform;
            var tipOutline = tooltip.gameObject.AddComponent<Outline>(); tipOutline.effectColor = new Color(0, 0, 0, .95f); tipOutline.effectDistance = new Vector2(2, -2);
            tooltip.enabled = false;

            var panel = RusticHudTheme.Rect(root, "Info", new Vector2(1, .5f), new Vector2(-236, -34), new Vector2(420, 900));
            RusticHudTheme.Panel(panel, RusticHudTheme.Surface, 16);
            title = RusticHudTheme.Label(panel, "Nome", new Vector2(.5f, 1), new Vector2(0, -50), new Vector2(370, 70), "", 28, TextAnchor.UpperLeft);
            title.color = RusticHudTheme.Gold; title.horizontalOverflow = HorizontalWrapMode.Wrap; title.verticalOverflow = VerticalWrapMode.Overflow;
            region = RusticHudTheme.Label(panel, "Regiao", new Vector2(.5f, 1), new Vector2(0, -110), new Vector2(370, 50), "", 18, TextAnchor.UpperLeft);
            region.color = RusticHudTheme.Muted; region.horizontalOverflow = HorizontalWrapMode.Wrap;
            body = RusticHudTheme.Label(panel, "Texto", new Vector2(.5f, 1), new Vector2(0, -260), new Vector2(370, 240), "", 20, TextAnchor.UpperLeft);
            body.horizontalOverflow = HorizontalWrapMode.Wrap; body.verticalOverflow = VerticalWrapMode.Overflow;
            status = RusticHudTheme.Label(panel, "Situacao", new Vector2(.5f, 0), new Vector2(0, 230), new Vector2(370, 90), "", 19, TextAnchor.UpperLeft);
            status.color = RusticHudTheme.Gold; status.horizontalOverflow = HorizontalWrapMode.Wrap;
            travel = RusticHudTheme.Button(panel, "Viajar", new Vector2(.5f, 0), new Vector2(0, 140), new Vector2(370, 64), "VIAJAR", () => { });
            travel.interactable = false;
            RusticHudTheme.Label(panel, "Legenda", new Vector2(.5f, 0), new Vector2(0, 70), new Vector2(370, 60),
                "Sinos: pontos de viagem (dourado: cidade, verde-água: estação) · linhas finas: áreas · tracejado forte: regiões", 15, TextAnchor.MiddleLeft).color = RusticHudTheme.Muted;
            if (Application.isEditor)
                RusticHudTheme.Button(panel, "Teste", new Vector2(.5f, 0), new Vector2(0, 26), new Vector2(370, 40),
                    DescobertasDoMapa.RevelarTudo ? "(teste) esconder de novo" : "(teste) revelar tudo",
                    () => { DescobertasDoMapa.RevelarTudo = !DescobertasDoMapa.RevelarTudo; var w = world; bool s = estacao; Fechar(); Abrir(w, s); });
            Show(Lugares[0]);
        }

        RectTransform borders, selected;
        readonly Dictionary<string, Text> labels = new Dictionary<string, Text>();

        void Point(RectTransform parent, Lugar lugar)
        {
            bool city = lugar.tipo == "cidade";
            float size = city ? 62 : 50;
            var icon = RusticHudTheme.Rect(parent, lugar.id, new Vector2(lugar.x / 100f, 1 - lugar.y / 100f), new Vector2(0, size * .32f), Vector2.one * size);
            var image = icon.gameObject.AddComponent<RawImage>();
            image.texture = Resources.Load<Texture2D>(lugar.id == "refugio" ? "Mapa/IconeUltimoSino" : city ? "Mapa/IconeCidade" : "Mapa/IconeEstacao");
            image.raycastTarget = true;
            var button = icon.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f); colors.pressedColor = new Color(.8f, .8f, .8f); button.colors = colors;
            var chosen = lugar; button.onClick.AddListener(() => Show(chosen));
            var label = RusticHudTheme.Label(icon, "Nome", new Vector2(.5f, 1), new Vector2(0, 18), new Vector2(320, 30), lugar.nome, city ? 21 : 18, TextAnchor.MiddleCenter);
            label.color = city ? new Color(1, .93f, .74f) : Color.white;
            var outline = label.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(0, 0, 0, .95f); outline.effectDistance = new Vector2(1.8f, -1.8f);
            label.enabled = false;
            labels[lugar.id] = label;
            var hover = icon.gameObject.AddComponent<Dica>(); hover.label = label; hover.map = this; hover.id = lugar.id;
        }

        // Region borders, like the dashed lines of a real map (percent of the image).
        static readonly Vector2[][] Borders =
        {
            new[] { new Vector2(18, 14), new Vector2(26, 15), new Vector2(33, 18), new Vector2(36, 24), new Vector2(36, 27) },                 // north / west
            new[] { new Vector2(36, 27), new Vector2(42, 29), new Vector2(48, 30), new Vector2(55, 28), new Vector2(62, 25) },                 // north / centre
            new[] { new Vector2(62, 25), new Vector2(66, 23), new Vector2(71, 20), new Vector2(72, 10), new Vector2(76, 7), new Vector2(77, 3) }, // north / east
            new[] { new Vector2(36, 27), new Vector2(30, 33), new Vector2(27, 40), new Vector2(31, 46), new Vector2(36, 52), new Vector2(35, 62), new Vector2(38, 70), new Vector2(36, 76) }, // west / centre
            new[] { new Vector2(62, 25), new Vector2(62, 32), new Vector2(66, 40), new Vector2(67, 50), new Vector2(65, 58), new Vector2(60, 62), new Vector2(59, 68), new Vector2(63, 72), new Vector2(72, 73), new Vector2(74, 80) }, // centre / east
        };
        static readonly (string key, float x, float y)[] RegionLabels = { ("c4", 26, 7), ("c2", 14, 47), ("c1", 47, 53), ("c3", 86, 57) };

        void DrawBorders()
        {
            for (int i = borders.childCount - 1; i >= 0; i--) Destroy(borders.GetChild(i).gameObject);
            Vector2 size = content.sizeDelta;
            const float Dash = 9, Gap = 6;
            foreach (var line in Borders)
                for (int i = 0; i < line.Length - 1; i++)
                {
                    Vector2 a = new Vector2(line[i].x / 100 * size.x, -line[i].y / 100 * size.y), b = new Vector2(line[i + 1].x / 100 * size.x, -line[i + 1].y / 100 * size.y);
                    float length = Vector2.Distance(a, b), angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
                    for (float d = 0; d < length; d += Dash + Gap)
                    {
                        float piece = Mathf.Min(Dash, length - d);
                        Vector2 mid = Vector2.Lerp(a, b, (d + piece * .5f) / length);
                        Segment(mid, piece + 2, 5, angle, new Color(.08f, .04f, .03f, .7f));
                        Segment(mid, piece, 2.4f, angle, new Color(1, .93f, .82f, .95f));
                    }
                }
        }

        void Segment(Vector2 position, float length, float width, float angle, Color color)
        {
            var rect = new GameObject("Traco", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(borders, false); rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position; rect.sizeDelta = new Vector2(length, width); rect.localRotation = Quaternion.Euler(0, 0, angle);
            var image = rect.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false;
        }

        sealed class Dica : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
        {
            public Text label; public MapaDoMundo map; public string id;
            public void OnPointerEnter(PointerEventData data) => label.enabled = true;
            public void OnPointerExit(PointerEventData data) => label.enabled = map.selectedId == id;
        }

        string selectedId;

        void Show(Lugar lugar)
        {
            // The chosen point keeps its name visible (on the phone there is no hover).
            if (selectedId != null && labels.TryGetValue(selectedId, out var old)) old.enabled = false;
            selectedId = lugar.id;
            if (labels.TryGetValue(lugar.id, out var now)) now.enabled = true;
            picked = System.Array.IndexOf(Lugares, lugar); PaintHighlight();
            var r0 = Regioes.TryGetValue(lugar.regiao, out var region0) ? region0 : default;
            if (!DescobertasDoMapa.Conhece(lugar.id))
            {
                title.text = "Área não descoberta";
                region.text = r0.nome + " · " + r0.capitulo + " · nível " + r0.nivel;
                body.text = "Ninguém da Liga mapeou este lugar ainda. Vá até lá para revelar o que existe.";
                status.text = ""; travel.interactable = false;
                return;
            }
            title.text = lugar.nome;
            var r = Regioes.TryGetValue(lugar.regiao, out var found) ? found : default;
            string kind = lugar.tipo == "cidade" ? "Cidade" : lugar.tipo == "estacao" ? "Estação" : lugar.tipo == "covil" ? "Covil" : lugar.tipo == "segredo" ? "Segredo" : "Lugar";
            region.text = kind + " · " + r.nome + " · " + r.capitulo + " · nível " + r.nivel;
            body.text = lugar.texto;
            status.text = lugar.id == "refugio" ? "Você está aqui. A Estação do Último Sino só traz pessoas: o sino está rachado."
                : lugar.tipo == "estacao" ? "Estação ainda não religada. Limpe o Breu em volta e acorde o Vigia para viajar até aqui."
                : "Para viajar até aqui, religue a estação desta região.";
            travel.interactable = false;
        }

        // Areas: each of the 52 places owns a piece of land. MapaAreasDados.bytes = one byte per pixel (836 x 470, rows from
        // the top): the place number, 0 = sea. Raw bytes, so no texture compression can change the numbers.
        byte[] areas; const int areaWidth = 836, areaHeight = 470;
        Texture2D highlight; Color32[] highlightPixels; int hovered = -1, picked = -1;
        RawImage highlightImage; Text tooltip; RectTransform tooltipRect;

        void LoadAreas()
        {
            var map = Resources.Load<TextAsset>("Mapa/MapaAreasDados");
            if (map == null || map.bytes.Length != areaWidth * areaHeight) { Debug.LogWarning("Mapa: MapaAreasDados.bytes não encontrado."); return; }
            areas = map.bytes;
            highlight = new Texture2D(areaWidth, areaHeight, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            highlightPixels = new Color32[areas.Length];
        }

        int AreaAt(Vector2 screen)
        {
            if (areas == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(content, screen, null, out var local)) return -1;
            float u = local.x / content.rect.width + .5f, v = local.y / content.rect.height + .5f;
            if (u < 0 || u >= 1 || v < 0 || v >= 1) return -1;
            int id = areas[(int)((1 - v) * areaHeight) * areaWidth + (int)(u * areaWidth)];
            return id > 0 && id <= Lugares.Length ? id - 1 : -1;
        }

        // Light tint on the area under the pointer and on the chosen one.
        void PaintHighlight()
        {
            if (areas == null) return;
            var none = new Color32(0, 0, 0, 0); var soft = new Color32(255, 246, 220, 46); var strong = new Color32(255, 240, 200, 78);
            int h = hovered + 1, p = picked + 1;
            for (int i = 0; i < areas.Length; i++) { int id = Cell(i); highlightPixels[i] = id == p && p > 0 ? strong : id == h && h > 0 ? soft : none; }
            highlight.SetPixels32(highlightPixels); highlight.Apply(false);
        }

        // Dark over every area not discovered yet (the sea stays a little dark).
        Texture2D FogTexture()
        {
            if (areas == null) return null;
            var pixels = new Color32[areas.Length];
            byte dark = (byte)(Fog.a * 255), sea = (byte)(Fog.a * 255 * .55f);
            for (int i = 0; i < areas.Length; i++)
            {
                int id = Cell(i);
                bool known = id > 0 && id <= Lugares.Length && DescobertasDoMapa.Conhece(Lugares[id - 1].id);
                pixels[i] = new Color32((byte)(Fog.r * 255), (byte)(Fog.g * 255), (byte)(Fog.b * 255), known ? (byte)0 : id > 0 ? dark : sea);
            }
            var texture = new Texture2D(areaWidth, areaHeight, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            texture.SetPixels32(pixels); texture.Apply();
            return texture;
        }

        // Texture pixels go from the bottom row up; the bytes go from the top row down.
        int Cell(int pixel) => areas[(areaHeight - 1 - pixel / areaWidth) * areaWidth + pixel % areaWidth];

        public void ClickAt(Vector2 screen)
        {
            int area = AreaAt(screen);
            if (area >= 0) Show(Lugares[area]);
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) { Fechar(); return; }
            if (viewport.rect.width > 10)
            {
                // Fit the 16:9 art inside the map area (again every frame: the screen size can change).
                float w = viewport.rect.width, h = viewport.rect.height;
                var fit = w / h > 16f / 9 ? new Vector2(h * 16 / 9, h) : new Vector2(w, w * 9 / 16);
                if ((fit - content.sizeDelta).sqrMagnitude > .5f) { content.sizeDelta = fit; Pan(Vector2.zero); }
            }
            // Name of the area under the mouse (on the phone: the area you tap is chosen instead).
            if (Mouse.current != null && areas != null)
            {
                var screen = Mouse.current.position.ReadValue();
                int area = AreaAt(screen);
                if (area != hovered) { hovered = area; PaintHighlight(); }
                tooltip.enabled = area >= 0;
                if (area >= 0)
                {
                    tooltip.text = DescobertasDoMapa.Conhece(Lugares[area].id) ? Lugares[area].nome : "Área não descoberta";
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(content, screen, null, out var at)) tooltipRect.anchoredPosition = at + new Vector2(0, 34 / Mathf.Max(.01f, zoom));
                    tooltipRect.localScale = Vector3.one / zoom;
                }
            }
            var touch = Touchscreen.current;
            if (touch != null && touch.touches.Count > 1 && touch.touches[0].press.isPressed && touch.touches[1].press.isPressed)
            {
                float distance = Vector2.Distance(touch.touches[0].position.ReadValue(), touch.touches[1].position.ReadValue());
                if (pinch > 0) Zoom(distance / pinch);
                pinch = distance;
            }
            else pinch = 0;
        }

        public void Zoom(float factor)
        {
            zoom = Mathf.Clamp(zoom * factor, 1, 3.2f);
            content.localScale = Vector3.one * zoom;
            Pan(Vector2.zero);
        }

        public void Pan(Vector2 delta)
        {
            var size = content.sizeDelta * zoom; var view = viewport.rect.size;
            Vector2 limit = new Vector2(Mathf.Max(0, (size.x - view.x) * .5f), Mathf.Max(0, (size.y - view.y) * .5f));
            var p = content.anchoredPosition + delta;
            content.anchoredPosition = new Vector2(Mathf.Clamp(p.x, -limit.x, limit.x), Mathf.Clamp(p.y, -limit.y, limit.y));
        }

        sealed class Arrastar : MonoBehaviour, IDragHandler, IScrollHandler, IPointerClickHandler
        {
            public MapaDoMundo map;
            public void OnPointerClick(PointerEventData data) { if (!data.dragging) map.ClickAt(data.position); }
            public void OnDrag(PointerEventData data)
            {
                if (map.pinch > 0) return;
                var canvas = GetComponentInParent<Canvas>();
                map.Pan(data.delta / (canvas != null ? canvas.scaleFactor : 1));
            }
            public void OnScroll(PointerEventData data) { if (Mathf.Abs(data.scrollDelta.y) > .01f) map.Zoom(data.scrollDelta.y > 0 ? 1.15f : 1 / 1.15f); }
        }

        static RectTransform Stretch(RectTransform rect, Transform parent, float left, float right, float top, float bottom)
        {
            rect.SetParent(parent, false); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom); rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }
    }
}
