using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // A Nascente do Rio (Chapter 1). The plan of the place lives here, shared by the menu that builds it
    // (Forja/Mundo/Montar a Nascente do Rio) and by the game: the named corners, the river, the roads,
    // the ambush on the reed path, the secret of the Bolha d'Água and the drowned captain at the spring.
    // Compass: north = +Z (towards the Refúgio), east = +X. The ground goes from -64 to +64 on both axes.
    public static class NascenteDoRio
    {
        public const string Cena = "Area_Nascente";
        public enum Tipo { Seguro, Calmo, Perigo, Chefe, Segredo, Breu }
        public sealed class Canto
        {
            public readonly string nome, aviso; public readonly Vector2 centro; public readonly float raio; public readonly Tipo tipo;
            public Canto(string n, float x, float z, float r, Tipo t, string a) { nome = n; centro = new Vector2(x, z); raio = r; tipo = t; aviso = a; }
        }
        // Safe corners have no creatures at all: time to look at the map or the backpack.
        public static readonly Canto[] Cantos =
        {
            new Canto("Vau do Norte", 0, 48, 13, Tipo.Seguro, "Lugar calmo · sem bichos por perto"),
            new Canto("Bosque Sussurrante", -34, 22, 20, Tipo.Perigo, "Bichos tomados pelo Breu"),
            new Canto("Clareira Quieta", -22, -2, 9, Tipo.Seguro, "Lugar calmo · sem bichos por perto"),
            new Canto("Pedras Partidas", 36, 26, 14, Tipo.Perigo, "Cuidado"),
            new Canto("Vau das Pedras", 15.4f, 2, 6, Tipo.Calmo, "Água rasa"),
            new Canto("Toca do Breu", -48, -10, 12, Tipo.Perigo, "Cuidado"),
            new Canto("Trilha dos Juncos", -26, -33, 9, Tipo.Calmo, "Silêncio demais..."),
            new Canto("Poço da Nascente", 26, -38, 15, Tipo.Chefe, "Onde o rio nasce"),
            new Canto("Recanto Escondido", 46, -21, 5, Tipo.Segredo, "Um lugar que ninguém mapeou"),
            // Breu too strong: you can walk in, but it burns a part of your life every second, whatever your life is.
            // Places for much later, when the player learns to resist the Breu.
            new Canto("Fenda do Breu", 50, 48, 10, Tipo.Breu, "O Breu queima · volte quando souber resistir"),
            new Canto("Charco Negro", -50, -32, 9, Tipo.Breu, "O Breu queima · volte quando souber resistir"),
        };
        public const float QueimaDoBreu = .12f; // part of the maximum life lost per second inside

        // The spring (a pool under a small waterfall) and the river that runs north to the Refúgio.
        public static readonly Vector2 Poco = new Vector2(26, -28);
        public const float RaioDoPoco = 9, LarguraDoRio = 6;
        public static readonly Vector2 Cachoeira = new Vector2(38.5f, -28);
        public static readonly Vector2[] Rio =
        {
            new Vector2(26, -28), new Vector2(24, -19), new Vector2(19, -10), new Vector2(16, -1), new Vector2(13, 9),
            new Vector2(10, 21), new Vector2(8, 34), new Vector2(8, 48), new Vector2(9, 66), new Vector2(11, 98),
        };
        // Where the east road crosses the river: shallow water, walkable.
        public static readonly Vector2 Vau = new Vector2(15.4f, 2); public const float RaioDoVau = 3.6f;

        // Roads to the four exits (north: Refúgio, east: Campos, south: Gruta, southwest: Trilhos).
        public static readonly Vector2[][] Estradas =
        {
            // They go on past the edge of the place (to the end of the ground), so the way looks like it continues.
            new[] { new Vector2(0, 98), new Vector2(0, 64), new Vector2(-1, 46), new Vector2(-3, 28), new Vector2(-2, 12), new Vector2(0, 2) },
            new[] { new Vector2(0, 2), new Vector2(14, 2), new Vector2(30, 4), new Vector2(46, 1), new Vector2(64, 0), new Vector2(98, 0) },
            new[] { new Vector2(0, 2), new Vector2(-3, -14), new Vector2(-2, -34), new Vector2(0, -50), new Vector2(0, -64), new Vector2(0, -98) },
            new[] { new Vector2(-3, -14), new Vector2(-18, -26), new Vector2(-36, -42), new Vector2(-50, -52), new Vector2(-64, -64), new Vector2(-98, -98) },
        };

        public static readonly Vector3 Chegada = new Vector3(0, .1f, 46);
        // Side mission "Estandartes da Liga": a note at the abandoned camp and three banners on the way.
        public static readonly Vector3 Bilhete = new Vector3(-10.5f, 0, 44.6f);
        public static readonly Vector3[] Estandartes = { new Vector3(34, 0, 29), new Vector3(-44, 0, -15), new Vector3(-29, 0, -30) };
        public static readonly string[] Registros =
        {
            "\"Dia 3. Goblins nas pedras. Gente nossa ferida. Seguimos para oeste.\"",
            "\"Dia 4. Um selo preto no chão. O Breu sai dele. Não chegamos perto.\"",
            "\"Dia 5. O capitão saiu do poço. Recuamos pelos juncos. Quem achar isto: não vá sozinho.\"",
        };
        public static int EstandartesAchados { get { int n = 0; for (int i = 0; i < 3; i++) if (Historia.Feito("m5_e" + i)) n++; return n; } }
        public static readonly Vector3 Chefe = new Vector3(26, 0, -46);
        public static readonly Vector3 Segredo = new Vector3(46, 0, -21);
        public static readonly Vector3 Emboscada = new Vector3(-26, 0, -33);
        // Homes of the animals taken by the Breu (they wander around them), all in the Bosque Sussurrante.
        public static readonly Vector3[] Animais =
        {
            new Vector3(-30, 0, 14), new Vector3(-40, 0, 26), new Vector3(-26, 0, 30), new Vector3(-44, 0, 12), new Vector3(-36, 0, 34), new Vector3(-24, 0, 20),
        };

        public static float Distancia(Vector2 p, Vector2[] linha)
        {
            float best = float.MaxValue;
            for (int i = 1; i < linha.Length; i++)
            {
                Vector2 a = linha[i - 1], b = linha[i], ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(.0001f, ab.sqrMagnitude));
                best = Mathf.Min(best, Vector2.Distance(p, a + ab * t));
            }
            return best;
        }
        public static float AoRio(Vector2 p) => Distancia(p, Rio);
        public static float AEstrada(Vector2 p) { float best = float.MaxValue; foreach (var e in Estradas) best = Mathf.Min(best, Distancia(p, e)); return best; }
        public static Canto CantoEm(Vector2 p, Canto atual)
        {
            // Stays in the current corner a little past its edge, so walking on the border does not flash names.
            if (atual != null && Vector2.Distance(p, atual.centro) < atual.raio + 3) return atual;
            Canto best = null;
            foreach (var c in Cantos) if (Vector2.Distance(p, c.centro) < c.raio && (best == null || c.raio < best.raio)) best = c;
            return best;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded; Loaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); }
        static void Loaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == Cena && Object.FindFirstObjectByType<LugarDaNascente>() == null) new GameObject("LugarDaNascente").AddComponent<LugarDaNascente>();
        }
    }

    public sealed class LugarDaNascente : MonoBehaviour
    {
        static readonly Color Verde = new Color(.62f, 1f, .74f), Perigo = new Color(1f, .58f, .46f), Vermelho = new Color(1f, .36f, .3f), Agua = new Color(.55f, .88f, 1f), Breu = new Color(.62f, .3f, 1f);
        RegionalWorld world; NascenteDoRio.Canto atual;
        Text titulo, subtitulo; CanvasGroup faixa; float faixaTempo;
        readonly List<EnemyActor> emboscada = new List<EnemyActor>(); bool armada = true; float rearmar; bool chefeVisto;
        WorldClickTarget segredo;
        // Breu zones: burning, a purple veil on the screen, the mist and the dark crystal in the middle.
        Image veu; float queima; readonly List<WorldClickTarget> cristais = new List<WorldClickTarget>();

        IEnumerator Start()
        {
            for (int i = 0; i < 300 && (world = FindFirstObjectByType<RegionalWorld>()) == null; i++) yield return null;
            while (world != null && world.Hero == null) yield return null;
            if (world == null) yield break;
            yield return null;
            Faixa();
            world.Enemies.Spawned += Surgiu; world.Enemies.Died += Morreu;
            CriarSegredo();
            CriarMissaoDaLiga();
            // The river can be waded (30% slower); the pool of the spring is deep and stays closed.
            PassoNaAgua.Criar(p => NascenteDoRio.AoRio(p) < NascenteDoRio.LarguraDoRio * .5f + .2f || Vector2.Distance(p, NascenteDoRio.Poco) < NascenteDoRio.RaioDoPoco + .3f).transform.SetParent(transform, false);
            foreach (var c in NascenteDoRio.Cantos) if (c.tipo == NascenteDoRio.Tipo.Breu) CriarZonaDoBreu(c);
        }

        void OnDestroy()
        {
            if (world != null && world.Enemies != null) { world.Enemies.Spawned -= Surgiu; world.Enemies.Died -= Morreu; }
            if (segredo != null) WorldClickTarget.Extras.Remove(segredo);
            foreach (var c in cristais) WorldClickTarget.Extras.Remove(c);
            foreach (var c in liga) if (c != null) WorldClickTarget.Extras.Remove(c);
        }

        void Update()
        {
            if (world == null || world.Hero == null || faixa == null) return;
            var hero = world.Hero.transform.position; var p = new Vector2(hero.x, hero.z);
            // Name of the corner when you walk into it.
            var canto = NascenteDoRio.CantoEm(p, atual);
            if (canto != atual) { atual = canto; if (canto != null) Mostrar(canto.nome, canto.aviso, Cor(canto.tipo)); }
            // The drowned captain: his name the first time you get close.
            if (!chefeVisto && (hero - NascenteDoRio.Chefe).sqrMagnitude < 20 * 20) { chefeVisto = true; Mostrar("Capitão Afogado", "Tomado pelo Breu · o rio escurece por causa dele", Vermelho, 3.2f); }
            // Ambush on the reed path: once, then again a while after all of them fell.
            if (armada && world.Hero.Health.IsAlive && (hero - NascenteDoRio.Emboscada).sqrMagnitude < 6 * 6) StartCoroutine(Emboscar());
            if (!armada)
            {
                emboscada.RemoveAll(a => a == null || !a.IsAlive);
                if (emboscada.Count == 0) { rearmar -= Time.deltaTime; if (rearmar <= 0 && (hero - NascenteDoRio.Emboscada).sqrMagnitude > 20 * 20) armada = true; }
            }
            Queimar(p);
            // Banner: fade in, hold, fade out.
            if (faixaTempo > 0)
            {
                faixaTempo -= Time.unscaledDeltaTime;
                faixa.alpha = Mathf.Clamp01(Mathf.Min((3.6f - faixaTempo) / .35f, faixaTempo / .7f));
            }
            else if (faixa.alpha > 0) faixa.alpha = 0;
        }

        static Color Cor(NascenteDoRio.Tipo t) =>
            t == NascenteDoRio.Tipo.Seguro ? Verde : t == NascenteDoRio.Tipo.Perigo ? Perigo : t == NascenteDoRio.Tipo.Chefe ? Vermelho : t == NascenteDoRio.Tipo.Segredo ? Agua : t == NascenteDoRio.Tipo.Breu ? Breu : RusticHudTheme.Gold;

        // ---------- Name banner (top of the screen) ----------
        void Faixa()
        {
            var go = new GameObject("NomeDoCanto", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var canvas = go.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 450;
            var scaler = go.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            faixa = go.AddComponent<CanvasGroup>(); faixa.alpha = 0; faixa.interactable = false; faixa.blocksRaycasts = false;
            var root = (RectTransform)go.transform;
            titulo = RusticHudTheme.Label(root, "Titulo", new Vector2(.5f, 1), new Vector2(0, -150), new Vector2(1200, 70), "", 50, TextAnchor.MiddleCenter);
            titulo.fontStyle = FontStyle.Bold; titulo.raycastTarget = false;
            var o = titulo.gameObject.AddComponent<Outline>(); o.effectColor = new Color(0, 0, 0, .9f); o.effectDistance = new Vector2(2, -2);
            subtitulo = RusticHudTheme.Label(root, "Aviso", new Vector2(.5f, 1), new Vector2(0, -208), new Vector2(1200, 34), "", 22, TextAnchor.MiddleCenter);
            subtitulo.raycastTarget = false;
            var o2 = subtitulo.gameObject.AddComponent<Outline>(); o2.effectColor = new Color(0, 0, 0, .9f); o2.effectDistance = new Vector2(1, -1);
            // Purple veil over the whole screen while the Breu burns you (below the banner).
            var veil = new GameObject("VeuDoBreu", typeof(RectTransform)).GetComponent<RectTransform>();
            veil.SetParent(transform, false);
            var vc = veil.gameObject.AddComponent<Canvas>(); vc.renderMode = RenderMode.ScreenSpaceOverlay; vc.sortingOrder = 440;
            var full = new GameObject("Cor", typeof(RectTransform)).GetComponent<RectTransform>(); full.SetParent(veil, false);
            full.anchorMin = Vector2.zero; full.anchorMax = Vector2.one; full.offsetMin = full.offsetMax = Vector2.zero;
            veu = full.gameObject.AddComponent<Image>(); veu.color = new Color(.25f, .05f, .35f, 0); veu.raycastTarget = false;
        }

        // ---------- Breu zones ----------
        void Queimar(Vector2 p)
        {
            NascenteDoRio.Canto dentro = null;
            foreach (var c in NascenteDoRio.Cantos) if (c.tipo == NascenteDoRio.Tipo.Breu && Vector2.Distance(p, c.centro) < c.raio) dentro = c;
            var health = world.Hero.Health;
            bool burning = dentro != null && health.IsAlive;
            if (veu != null)
            {
                float target = burning ? .32f + Mathf.Sin(Time.time * 5) * .06f : 0;
                var col = veu.color; col.a = Mathf.MoveTowards(col.a, target, Time.deltaTime * (burning ? 1.2f : 2f)); veu.color = col;
            }
            if (!burning) { queima = 0; return; }
            queima -= Time.deltaTime;
            if (queima > 0) return;
            queima = .5f;
            // A part of the maximum life, never a fixed number: more life does not let you stay longer.
            int damage = Mathf.Max(1, Mathf.CeilToInt(health.Maximum * NascenteDoRio.QueimaDoBreu * .5f));
            health.ApplyPeriodicDamage(damage, "BREU");
            EfeitosDeHabilidade.Explosao(world.Hero.transform.position + Vector3.up * .4f, .5f, Breu, 6);
        }

        void CriarZonaDoBreu(NascenteDoRio.Canto zona)
        {
            var center = Chao(new Vector3(zona.centro.x, 0, zona.centro.y));
            var root = new GameObject("ZonaDoBreu_" + zona.nome).transform; root.position = center;
            // Dark mist rising from the ground all over the zone.
            var ps = root.gameObject.AddComponent<ParticleSystem>();
            var main = ps.main; main.startLifetime = 4f; main.startSpeed = .35f; main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.6f);
            main.startColor = new Color(.22f, .06f, .32f, .55f); main.maxParticles = 160; main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission; emission.rateOverTime = 30;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = zona.raio; shape.rotation = new Vector3(-90, 0, 0);
            var colorOver = ps.colorOverLifetime; colorOver.enabled = true;
            var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .3f), new GradientAlphaKey(0, 1) });
            colorOver.color = g;
            root.GetComponent<ParticleSystemRenderer>().sharedMaterial = EfeitosDeHabilidade.Brilho;
            var light = new GameObject("Luz").AddComponent<Light>(); light.transform.SetParent(root, false); light.transform.localPosition = Vector3.up * 2;
            light.type = LightType.Point; light.color = Breu; light.range = zona.raio * 1.4f; light.intensity = 2.2f; light.shadows = LightShadows.None;
            root.gameObject.AddComponent<Respirar>().luz = light;
            // The dark crystal in the middle: for later. Today it only tells you it is too strong.
            var cristal = new GameObject("CristalDoBreu"); cristal.transform.SetParent(root, false);
            var col = cristal.AddComponent<SphereCollider>(); col.radius = 1f; col.center = Vector3.up; WorldClickTarget.Atravessavel(col);
            var target = cristal.AddComponent<WorldClickTarget>(); target.Configure("Cristal do Breu", 2.6f);
            target.Interacting += actor => { if (world != null && actor == world.Hero.transform) Mostrar("Cristal do Breu", "Forte demais. Ainda não dá para tocar nisto.", Breu, 2.6f); };
            WorldClickTarget.Extras.Add(target); cristais.Add(target);
        }

        void Mostrar(string nome, string aviso, Color cor, float segundos = 3.6f)
        {
            if (titulo == null) return;
            titulo.text = nome.ToUpperInvariant(); titulo.color = cor;
            subtitulo.text = aviso ?? ""; subtitulo.color = Color.Lerp(cor, Color.white, .55f);
            faixaTempo = Mathf.Max(segundos, 1.2f);
        }

        // ---------- Ambush ----------
        IEnumerator Emboscar()
        {
            armada = false; rearmar = 120;
            Mostrar("Emboscada!", "O Breu sobe da lama", Vermelho, 2.4f);
            EfeitosDeHabilidade.Tremer(.35f);
            var pool = world.Enemies;
            var tipos = new List<EnemyDefinition>();
            foreach (var id in new[] { "raspador", "raspador", "fagulha", "guarda-mare" })
                for (int i = 0; i < pool.Capacity; i++) if (pool.DefinitionAt(i) != null && pool.DefinitionAt(i).enemyId == id) { tipos.Add(pool.DefinitionAt(i)); break; }
            var hero = world.Hero.transform;
            var frente = hero.forward; frente.y = 0; if (frente.sqrMagnitude < .01f) frente = Vector3.forward; frente.Normalize();
            for (int i = 0; i < tipos.Count; i++)
            {
                // Around the hero, mostly in front and on the sides: there is always a way out behind.
                float angle = -110 + 220f * i / Mathf.Max(1, tipos.Count - 1);
                var point = hero.position + Quaternion.Euler(0, angle, 0) * frente * 5.5f;
                EfeitosDeHabilidade.Explosao(point + Vector3.up * .3f, 1.1f, Breu, 26);
                yield return new WaitForSeconds(.18f);
                var actor = pool.TrySpawn(tipos[i], point);
                if (actor != null) emboscada.Add(actor);
            }
        }

        // ---------- The drowned captain ----------
        void Surgiu(EnemyActor actor)
        {
            if (actor == null || actor.Profile == null || actor.Profile.kind != EnemyKind.Boss) return;
            // Test size: a long fight, not an endless one.
            actor.Health.ApplyAttributes(380, 0); actor.Health.Restore();
        }

        void Morreu(EnemyActor actor)
        {
            if (actor == null) return;
            if (emboscada.Remove(actor)) world.GetComponent<RegionalInventory>()?.Service?.GrantRegionalEnemyExperience();
            if (actor.Profile != null && actor.Profile.kind == EnemyKind.Boss) StartCoroutine(ChefeCaiu(actor.transform.position));
        }

        IEnumerator ChefeCaiu(Vector3 where)
        {
            EfeitosDeHabilidade.Tremer(.6f);
            EfeitosDeHabilidade.Explosao(where + Vector3.up, 2.2f, Breu, 60);
            yield return new WaitForSeconds(.6f);
            bool primeira = !Historia.Feito("m4_fonte");
            if (primeira) { Historia.Marcar("m4_fonte"); Mostrar("O rio clareia", "Volte ao Refúgio e conte à Nara", Agua, 4.5f); }
            yield return new WaitForSeconds(1.2f);
            // The mark of lightning left where he fell: the Marca do Raio.
            if (!Habilidades.Aprendeu("marca")) Habilidades.Aprender("marca");
        }

        // ---------- Side mission: Estandartes da Liga ----------
        readonly List<WorldClickTarget> liga = new List<WorldClickTarget>();
        WorldClickTarget Alvo(string nome, Vector3 at, string texto, Color cor, out Light luz)
        {
            var go = new GameObject(nome); go.transform.position = Chao(at);
            var col = go.AddComponent<SphereCollider>(); col.radius = .9f; col.center = Vector3.up * .8f; WorldClickTarget.Atravessavel(col);
            luz = new GameObject("Luz").AddComponent<Light>(); luz.transform.SetParent(go.transform, false); luz.transform.localPosition = Vector3.up * 1.6f;
            luz.type = LightType.Point; luz.color = cor; luz.range = 4; luz.intensity = 1.6f; luz.shadows = LightShadows.None;
            go.AddComponent<Respirar>().luz = luz;
            var t = go.AddComponent<WorldClickTarget>(); t.Configure(texto, 2.6f);
            WorldClickTarget.Extras.Add(t); liga.Add(t);
            return t;
        }

        void CriarMissaoDaLiga()
        {
            if (Historia.Feito("m5_feita")) return;
            var cobre = Saque.Secundaria;
            if (!Historia.Feito("m5_aceita"))
            {
                var bilhete = Alvo("BilheteDaLiga", NascenteDoRio.Bilhete, "Ler o bilhete da Liga", cobre, out _);
                bilhete.Interacting += actor =>
                {
                    if (world == null || actor != world.Hero.transform || Historia.Feito("m5_aceita")) return;
                    Historia.Marcar("m5_aceita");
                    Mostrar("Estandartes da Liga", "Nova missão · ache os 3 estandartes da patrulha", cobre, 3.6f);
                    AvisoNaTela.Mostrar("\"Patrulha da Liga. Marcamos o caminho com três estandartes. Se não voltarmos, sigam as marcas.\"", 6);
                    WorldClickTarget.Extras.Remove(bilhete); Destroy(bilhete.gameObject);
                    CriarEstandartes();
                };
            }
            else CriarEstandartes();
        }

        void CriarEstandartes()
        {
            for (int i = 0; i < NascenteDoRio.Estandartes.Length; i++)
            {
                if (Historia.Feito("m5_e" + i)) continue;
                int index = i;
                var alvo = Alvo("Estandarte" + i, NascenteDoRio.Estandartes[i], "Examinar o estandarte", Saque.Secundaria, out _);
                alvo.Interacting += actor =>
                {
                    if (world == null || actor != world.Hero.transform || Historia.Feito("m5_e" + index)) return;
                    Historia.Marcar("m5_e" + index);
                    int n = NascenteDoRio.EstandartesAchados;
                    EfeitosDeHabilidade.Explosao(alvo.transform.position + Vector3.up, 1f, Saque.Secundaria, 24);
                    Mostrar("Estandarte " + n + "/3", NascenteDoRio.Registros[index], Saque.Secundaria, 5f);
                    WorldClickTarget.Extras.Remove(alvo); liga.Remove(alvo); Destroy(alvo.gameObject);
                    if (n >= 3 && !Historia.Feito("m5_feita"))
                    {
                        Historia.Marcar("m5_feita");
                        world.GetComponent<RegionalInventory>()?.Service?.CollectRegionalGold(60);
                        Saque.SoltarRecurso(Recursos.Couro, 1, world.Hero.transform.position + Vector3.up);
                        AvisoNaTela.Mostrar("Missão concluída: Estandartes da Liga · +60 de ouro", 4);
                    }
                };
            }
        }

        // ---------- Secret: the Bolha d'Água ----------
        void CriarSegredo()
        {
            if (Habilidades.Aprendeu("bolha")) return;
            var p = Chao(NascenteDoRio.Segredo);
            var go = new GameObject("SegredoDaBolha");
            go.transform.position = p;
            // Bubbles rising from a small spring among the rocks, and a soft blue light that breathes.
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main; main.startLifetime = 2.4f; main.startSpeed = .45f; main.startSize = new ParticleSystem.MinMaxCurve(.08f, .22f);
            main.startColor = new Color(.7f, .95f, 1f, .85f); main.maxParticles = 60; main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission; emission.rateOverTime = 14;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = .55f; shape.rotation = new Vector3(-90, 0, 0);
            var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, .6f, 1, 1.3f));
            var renderer = go.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = EfeitosDeHabilidade.Brilho;
            var light = new GameObject("Luz").AddComponent<Light>(); light.transform.SetParent(go.transform, false); light.transform.localPosition = Vector3.up * .8f;
            light.type = LightType.Point; light.color = Agua; light.range = 5; light.intensity = 1.6f; light.shadows = LightShadows.None;
            go.AddComponent<Respirar>().luz = light;
            segredo = go.AddComponent<WorldClickTarget>();
            segredo.Configure("Tocar as bolhas", 2.6f);
            var col = go.AddComponent<SphereCollider>(); col.radius = .9f; col.center = Vector3.up * .5f; col.isTrigger = false; go.layer = 0; WorldClickTarget.Atravessavel(col);
            segredo.Interacting += actor =>
            {
                if (world == null || actor != world.Hero.transform || Habilidades.Aprendeu("bolha")) return;
                EfeitosDeHabilidade.Explosao(go.transform.position + Vector3.up, 1.6f, Agua, 50);
                Habilidades.Aprender("bolha");
                WorldClickTarget.Extras.Remove(segredo);
                Destroy(go, .3f);
            };
            WorldClickTarget.Extras.Add(segredo);
        }

        static Vector3 Chao(Vector3 p)
        {
            if (Physics.Raycast(new Vector3(p.x, 40, p.z), Vector3.down, out var hit, 80, 1 << 8, QueryTriggerInteraction.Ignore)) return hit.point;
            return p;
        }

        sealed class Respirar : MonoBehaviour
        {
            public Light luz;
            void Update() { if (luz != null) luz.intensity = 1.2f + Mathf.Sin(Time.time * 2.2f) * .6f; }
        }
    }
}
