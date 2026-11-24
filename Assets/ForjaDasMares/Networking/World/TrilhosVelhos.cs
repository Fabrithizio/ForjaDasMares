using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // Os Trilhos Velhos (Chapter 1, after the Nascente). The plan of the place, shared by the menu that builds it
    // (Forja/Mundo/Montar os Trilhos Velhos) and by the game: the old railway that crosses it from the Estação Antiga
    // (northwest) to the Vila da Foz (southeast), the branch line to the shunting yard where the guardian of the line
    // (the Custódio, taken by the Breu) stopped the Liga patrol, the named corners, the ambush in the narrow cut,
    // the stone map (Téo's side mission) and the pages of the line keeper's diary (side mission found in the world).
    // Compass: north = +Z (Bosque Luzente), east = +X. The playable ground goes from -64 to +64 on both axes.
    public static class TrilhosVelhos
    {
        public const string Cena = "Area_Trilhos";
        public sealed class Canto
        {
            public readonly string nome, aviso; public readonly Vector2 centro; public readonly float raio; public readonly NascenteDoRio.Tipo tipo;
            public Canto(string n, float x, float z, float r, NascenteDoRio.Tipo t, string a) { nome = n; centro = new Vector2(x, z); raio = r; tipo = t; aviso = a; }
        }
        public static readonly Canto[] Cantos =
        {
            new Canto("Mirante do Desvio", 34, 34, 10, NascenteDoRio.Tipo.Seguro, "Lugar calmo · sem bichos por perto"),
            new Canto("Casa do Guarda-Linha", -12, 40, 9, NascenteDoRio.Tipo.Seguro, "Lugar calmo · sem bichos por perto"),
            new Canto("Vagões Tombados", -36, 10, 12, NascenteDoRio.Tipo.Perigo, "Ferrugem e dentes"),
            new Canto("Bosque de Ferro", 36, 6, 12, NascenteDoRio.Tipo.Perigo, "Sentinelas da linha"),
            new Canto("Corte Estreito", 26, -21, 7, NascenteDoRio.Tipo.Calmo, "Silêncio demais..."),
            new Canto("Trincheira dos Ecos", 8, -44, 10, NascenteDoRio.Tipo.Perigo, "O eco responde antes de você"),
            new Canto("Pátio de Manobras", -26, -30, 13, NascenteDoRio.Tipo.Chefe, "Onde a patrulha parou"),
            new Canto("Recanto da Pedra Antiga", -43, -6, 5, NascenteDoRio.Tipo.Segredo, "Uma pedra mais velha que os trilhos"),
            new Canto("Vala de Piche", 50, -12, 8, NascenteDoRio.Tipo.Breu, "O Breu queima · volte quando souber resistir"),
            new Canto("Fosso Negro", -48, -48, 8, NascenteDoRio.Tipo.Breu, "O Breu queima · volte quando souber resistir"),
        };

        // The railway: the main line (northwest → southeast) and the branch to the shunting yard.
        // Both go on past the edge of the place, so the line looks like it continues.
        public static readonly Vector2[][] Trilhos =
        {
            new[] { new Vector2(-98, 98), new Vector2(-64, 64), new Vector2(-46, 46), new Vector2(-30, 28), new Vector2(-14, 12), new Vector2(4, 0),
                    new Vector2(20, -14), new Vector2(34, -30), new Vector2(46, -46), new Vector2(64, -64), new Vector2(98, -98) },
            new[] { new Vector2(4, 0), new Vector2(-6, -10), new Vector2(-16, -20), new Vector2(-26, -28), new Vector2(-32, -34) },
        };
        // Dirt roads to the north exit (Bosque) and the northeast exit (Nascente).
        public static readonly Vector2[][] Estradas =
        {
            new[] { new Vector2(0, 98), new Vector2(0, 64), new Vector2(-1, 50), new Vector2(-3, 34), new Vector2(0, 18), new Vector2(4, 0) },
            new[] { new Vector2(98, 98), new Vector2(64, 64), new Vector2(47, 47), new Vector2(32, 32), new Vector2(18, 14), new Vector2(4, 0) },
        };

        public static readonly Vector3 Chegada = new Vector3(36, .1f, 36);
        public static readonly Vector3 Chefe = new Vector3(-26, 0, -30);
        public static readonly Vector3 Emboscada = new Vector3(26, 0, -21);
        public static readonly Vector3 Pedra = new Vector3(-43, 0, -6);
        // Side mission "A Última Viagem": the note in the keeper's house and three pages of his diary.
        public static readonly Vector3 Bilhete = new Vector3(-9, 0, 37.5f);
        public static readonly Vector3[] Paginas = { new Vector3(-38, 0, 1), new Vector3(41, 0, 2), new Vector3(13, 0, -48) };
        public static readonly string[] Diario =
        {
            "\"O último trem passou cantando. Depois dele, o sino da Estação ficou mudo. Ninguém sabe por quê.\"",
            "\"O Custódio não deixa mais ninguém passar. Ele, que cuidava da linha. Os olhos dele ficaram roxos.\"",
            "\"Vou para a Estação. Se a Raiz-Mãe de lá ainda vive, alguém precisa contar a ela que o sino se calou.\"",
        };
        // Where the wind took the pages (a hint in the journal, so nobody gets stuck looking for the last one).
        static readonly string[] OndeFoi = { "a oeste, no meio do caminho", "a leste, perto dos trilhos", "ao sul, junto da cerca do fundo" };
        public static string Pista()
        {
            for (int i = 0; i < Paginas.Length; i++) if (!Historia.Feito("m8_p" + i)) return "uma folha voou " + OndeFoi[i];
            return null;
        }
        public static int PaginasAchadas { get { int n = 0; for (int i = 0; i < 3; i++) if (Historia.Feito("m8_p" + i)) n++; return n; } }

        public static float ALinha(Vector2 p) { float best = float.MaxValue; foreach (var t in Trilhos) best = Mathf.Min(best, NascenteDoRio.Distancia(p, t)); return best; }
        public static float AEstrada(Vector2 p) { float best = ALinha(p); foreach (var e in Estradas) best = Mathf.Min(best, NascenteDoRio.Distancia(p, e)); return best; }
        public static Canto CantoEm(Vector2 p, Canto atual)
        {
            if (atual != null && Vector2.Distance(p, atual.centro) < atual.raio + 3) return atual;
            Canto best = null;
            foreach (var c in Cantos) if (Vector2.Distance(p, c.centro) < c.raio && (best == null || c.raio < best.raio)) best = c;
            return best;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded; Loaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); }
        static void Loaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == Cena && Object.FindFirstObjectByType<LugarDosTrilhos>() == null) new GameObject("LugarDosTrilhos").AddComponent<LugarDosTrilhos>();
        }
    }

    public sealed class LugarDosTrilhos : MonoBehaviour
    {
        static readonly Color Verde = new Color(.62f, 1f, .74f), Perigo = new Color(1f, .58f, .46f), Vermelho = new Color(1f, .36f, .3f),
            Antigo = new Color(.72f, .9f, 1f), Breu = new Color(.62f, .3f, 1f), Ferrugem = new Color(1f, .7f, .45f);
        RegionalWorld world; TrilhosVelhos.Canto atual;
        Text titulo, subtitulo; CanvasGroup faixa; float faixaTempo;
        readonly List<EnemyActor> emboscada = new List<EnemyActor>(); bool armada = true; float rearmar; bool chefeVisto;
        Image veu; float queima;
        readonly List<WorldClickTarget> alvos = new List<WorldClickTarget>();

        IEnumerator Start()
        {
            for (int i = 0; i < 300 && (world = FindFirstObjectByType<RegionalWorld>()) == null; i++) yield return null;
            while (world != null && world.Hero == null) yield return null;
            if (world == null) yield break;
            yield return null;
            Faixa();
            world.Enemies.Spawned += Surgiu; world.Enemies.Died += Morreu;
            CriarPedra();
            CriarDiario();
            foreach (var c in TrilhosVelhos.Cantos) if (c.tipo == NascenteDoRio.Tipo.Breu) CriarZonaDoBreu(c);
            if (Historia.Feito("m6_aceita") && !Historia.Feito("m6_custodio")) Mostrar("Trilhos Velhos", "Siga a linha. Alguma coisa parou a patrulha.", RusticHudTheme.Gold, 4f);
        }

        void OnDestroy()
        {
            if (world != null && world.Enemies != null) { world.Enemies.Spawned -= Surgiu; world.Enemies.Died -= Morreu; }
            foreach (var a in alvos) if (a != null) WorldClickTarget.Extras.Remove(a);
        }

        void Update()
        {
            if (world == null || world.Hero == null || faixa == null) return;
            var hero = world.Hero.transform.position; var p = new Vector2(hero.x, hero.z);
            var canto = TrilhosVelhos.CantoEm(p, atual);
            if (canto != atual) { atual = canto; if (canto != null) Mostrar(canto.nome, canto.aviso, Cor(canto.tipo)); }
            if (!chefeVisto && (hero - TrilhosVelhos.Chefe).sqrMagnitude < 20 * 20) { chefeVisto = true; Mostrar("Custódio dos Trilhos", "Guardava a linha · agora é do Breu", Vermelho, 3.2f); }
            if (armada && world.Hero.Health.IsAlive && (hero - TrilhosVelhos.Emboscada).sqrMagnitude < 5 * 5) StartCoroutine(Emboscar());
            if (!armada)
            {
                emboscada.RemoveAll(a => a == null || !a.IsAlive);
                if (emboscada.Count == 0) { rearmar -= Time.deltaTime; if (rearmar <= 0 && (hero - TrilhosVelhos.Emboscada).sqrMagnitude > 20 * 20) armada = true; }
            }
            Queimar(p);
            if (faixaTempo > 0)
            {
                faixaTempo -= Time.unscaledDeltaTime;
                faixa.alpha = Mathf.Clamp01(Mathf.Min((3.6f - faixaTempo) / .35f, faixaTempo / .7f));
            }
            else if (faixa.alpha > 0) faixa.alpha = 0;
        }

        static Color Cor(NascenteDoRio.Tipo t) =>
            t == NascenteDoRio.Tipo.Seguro ? Verde : t == NascenteDoRio.Tipo.Perigo ? Perigo : t == NascenteDoRio.Tipo.Chefe ? Vermelho : t == NascenteDoRio.Tipo.Segredo ? Antigo : t == NascenteDoRio.Tipo.Breu ? Breu : RusticHudTheme.Gold;

        // ---------- Name banner ----------
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
            var veil = new GameObject("VeuDoBreu", typeof(RectTransform)).GetComponent<RectTransform>();
            veil.SetParent(transform, false);
            var vc = veil.gameObject.AddComponent<Canvas>(); vc.renderMode = RenderMode.ScreenSpaceOverlay; vc.sortingOrder = 440;
            var full = new GameObject("Cor", typeof(RectTransform)).GetComponent<RectTransform>(); full.SetParent(veil, false);
            full.anchorMin = Vector2.zero; full.anchorMax = Vector2.one; full.offsetMin = full.offsetMax = Vector2.zero;
            veu = full.gameObject.AddComponent<Image>(); veu.color = new Color(.25f, .05f, .35f, 0); veu.raycastTarget = false;
        }

        void Mostrar(string nome, string aviso, Color cor, float segundos = 3.6f)
        {
            if (titulo == null) return;
            titulo.text = nome.ToUpperInvariant(); titulo.color = cor;
            subtitulo.text = aviso ?? ""; subtitulo.color = Color.Lerp(cor, Color.white, .55f);
            faixaTempo = Mathf.Max(segundos, 1.2f);
        }

        // ---------- Breu zones (same rule as the Nascente: a part of the maximum life every second) ----------
        void Queimar(Vector2 p)
        {
            TrilhosVelhos.Canto dentro = null;
            foreach (var c in TrilhosVelhos.Cantos) if (c.tipo == NascenteDoRio.Tipo.Breu && Vector2.Distance(p, c.centro) < c.raio) dentro = c;
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
            int damage = Mathf.Max(1, Mathf.CeilToInt(health.Maximum * NascenteDoRio.QueimaDoBreu * .5f));
            health.ApplyPeriodicDamage(damage, "BREU");
            EfeitosDeHabilidade.Explosao(world.Hero.transform.position + Vector3.up * .4f, .5f, Breu, 6);
        }

        void CriarZonaDoBreu(TrilhosVelhos.Canto zona)
        {
            var center = Chao(new Vector3(zona.centro.x, 0, zona.centro.y));
            var root = new GameObject("ZonaDoBreu_" + zona.nome).transform; root.position = center;
            var ps = root.gameObject.AddComponent<ParticleSystem>();
            var main = ps.main; main.startLifetime = 4f; main.startSpeed = .35f; main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.6f);
            main.startColor = new Color(.22f, .06f, .32f, .55f); main.maxParticles = 140; main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission; emission.rateOverTime = 26;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = zona.raio; shape.rotation = new Vector3(-90, 0, 0);
            var colorOver = ps.colorOverLifetime; colorOver.enabled = true;
            var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .3f), new GradientAlphaKey(0, 1) });
            colorOver.color = g;
            root.GetComponent<ParticleSystemRenderer>().sharedMaterial = EfeitosDeHabilidade.Brilho;
            var light = new GameObject("Luz").AddComponent<Light>(); light.transform.SetParent(root, false); light.transform.localPosition = Vector3.up * 2;
            light.type = LightType.Point; light.color = Breu; light.range = zona.raio * 1.4f; light.intensity = 2.2f; light.shadows = LightShadows.None;
            root.gameObject.AddComponent<Respirar>().luz = light;
            var cristal = new GameObject("CristalDoBreu"); cristal.transform.SetParent(root, false);
            var col = cristal.AddComponent<SphereCollider>(); col.radius = 1f; col.center = Vector3.up; WorldClickTarget.Atravessavel(col);
            var target = cristal.AddComponent<WorldClickTarget>(); target.Configure("Cristal do Breu", 2.6f);
            target.Interacting += actor => { if (world != null && actor == world.Hero.transform) Mostrar("Cristal do Breu", "Forte demais. Ainda não dá para tocar nisto.", Breu, 2.6f); };
            WorldClickTarget.Extras.Add(target); alvos.Add(target);
        }

        // ---------- Ambush in the narrow cut ----------
        IEnumerator Emboscar()
        {
            armada = false; rearmar = 120;
            Mostrar("Emboscada!", "Saíram de baixo dos trilhos", Vermelho, 2.4f);
            EfeitosDeHabilidade.Tremer(.35f);
            var pool = world.Enemies;
            var tipos = new List<EnemyDefinition>();
            foreach (var id in new[] { "rato-ferrugem", "rato-ferrugem", "sentinela-trilhos", "fagulha", "rato-ferrugem" })
                for (int i = 0; i < pool.Capacity; i++) if (pool.DefinitionAt(i) != null && pool.DefinitionAt(i).enemyId == id) { tipos.Add(pool.DefinitionAt(i)); break; }
            var hero = world.Hero.transform;
            var frente = hero.forward; frente.y = 0; if (frente.sqrMagnitude < .01f) frente = Vector3.forward; frente.Normalize();
            for (int i = 0; i < tipos.Count; i++)
            {
                float angle = -110 + 220f * i / Mathf.Max(1, tipos.Count - 1);
                var point = hero.position + Quaternion.Euler(0, angle, 0) * frente * 5.5f;
                EfeitosDeHabilidade.Explosao(point + Vector3.up * .3f, 1.1f, Ferrugem, 26);
                yield return new WaitForSeconds(.18f);
                var actor = pool.TrySpawn(tipos[i], point);
                if (actor != null) emboscada.Add(actor);
            }
        }

        // ---------- The Custódio (boss of the shunting yard) ----------
        void Surgiu(EnemyActor actor)
        {
            if (actor == null || actor.Profile == null || actor.Profile.kind != EnemyKind.Boss) return;
            actor.Health.ApplyAttributes(520, 0); actor.Health.Restore();
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
            EfeitosDeHabilidade.Explosao(where + Vector3.up, 2.4f, Ferrugem, 60);
            yield return new WaitForSeconds(.6f);
            if (Historia.Feito("m6_custodio")) yield break;
            Historia.Marcar("m6_custodio");
            world.GetComponent<RegionalInventory>()?.Service?.CollectRegionalGold(80);
            Mostrar("A linha está livre", "Volte ao Refúgio e conte à Nara", RusticHudTheme.Gold, 4.5f);
            yield return new WaitForSeconds(1.5f);
            // What the patrol left in the yard: they are alive, and went on to the station.
            AvisoNaTela.Mostrar("Riscado num vagão: \"Liga. Custódio tomado. Fomos para a Estação Antiga. Estamos vivos.\"", 6);
        }

        // ---------- Clickable things ----------
        WorldClickTarget Alvo(string nome, Vector3 at, string texto, Color cor)
        {
            var go = new GameObject(nome); go.transform.position = Chao(at);
            var col = go.AddComponent<SphereCollider>(); col.radius = .9f; col.center = Vector3.up * .8f; WorldClickTarget.Atravessavel(col);
            var luz = new GameObject("Luz").AddComponent<Light>(); luz.transform.SetParent(go.transform, false); luz.transform.localPosition = Vector3.up * 1.6f;
            luz.type = LightType.Point; luz.color = cor; luz.range = 4; luz.intensity = 1.6f; luz.shadows = LightShadows.None;
            go.AddComponent<Respirar>().luz = luz;
            var t = go.AddComponent<WorldClickTarget>(); t.Configure(texto, 2.6f);
            WorldClickTarget.Extras.Add(t); alvos.Add(t);
            return t;
        }

        // The stone map (Téo's side mission, but anyone who explores can find it): the lands of the other side.
        void CriarPedra()
        {
            if (Historia.Feito("m7_pedra")) return;
            var pedra = Alvo("PedraDoMapa", TrilhosVelhos.Pedra, "Olhar a pedra gravada", Antigo);
            pedra.Interacting += actor =>
            {
                if (world == null || actor != world.Hero.transform || Historia.Feito("m7_pedra")) return;
                Historia.Marcar("m7_pedra");
                EfeitosDeHabilidade.Explosao(pedra.transform.position + Vector3.up, 1.4f, Antigo, 40);
                Mostrar("A Pedra Gravada", "Um mapa. Rios, montanhas... e uma curva de rio que você conhece.", Antigo, 5f);
                AvisoNaTela.Mostrar("...É o caminho da minha vila. O mesmo rio. A mesma serra. Mas eu nunca estive aqui.", 6);
                WorldClickTarget.Extras.Remove(pedra); alvos.Remove(pedra);
                // The light goes out; the stone stays (it is part of the place).
                foreach (var l in pedra.GetComponentsInChildren<Light>()) Destroy(l.gameObject);
                Destroy(pedra);
            };
        }

        // Side mission "A Última Viagem": the line keeper's note and the three pages of his diary.
        void CriarDiario()
        {
            if (Historia.Feito("m8_feita")) return;
            var cobre = Saque.Secundaria;
            if (!Historia.Feito("m8_aceita"))
            {
                var bilhete = Alvo("BilheteDoGuarda", TrilhosVelhos.Bilhete, "Ler o bilhete na porta", cobre);
                bilhete.Interacting += actor =>
                {
                    if (world == null || actor != world.Hero.transform || Historia.Feito("m8_aceita")) return;
                    Historia.Marcar("m8_aceita");
                    Mostrar("A Última Viagem", "Nova missão · ache as 3 páginas do diário do guarda-linha", cobre, 3.6f);
                    AvisoNaTela.Mostrar("\"Fui embora com pressa. Perdi o diário pelo caminho, folha por folha. Quem achar, leia.\"", 6);
                    WorldClickTarget.Extras.Remove(bilhete); alvos.Remove(bilhete); Destroy(bilhete.gameObject);
                    CriarPaginas();
                };
            }
            else CriarPaginas();
        }

        void CriarPaginas()
        {
            for (int i = 0; i < TrilhosVelhos.Paginas.Length; i++)
            {
                if (Historia.Feito("m8_p" + i)) continue;
                int index = i;
                var alvo = Alvo("Pagina" + i, TrilhosVelhos.Paginas[i], "Pegar a página", Saque.Secundaria);
                Folha(alvo.transform);
                alvo.Interacting += actor =>
                {
                    if (world == null || actor != world.Hero.transform || Historia.Feito("m8_p" + index)) return;
                    Historia.Marcar("m8_p" + index);
                    int n = TrilhosVelhos.PaginasAchadas;
                    EfeitosDeHabilidade.Explosao(alvo.transform.position + Vector3.up, 1f, Saque.Secundaria, 24);
                    Mostrar("Página " + n + "/3", TrilhosVelhos.Diario[index], Saque.Secundaria, 5.5f);
                    WorldClickTarget.Extras.Remove(alvo); alvos.Remove(alvo); Destroy(alvo.gameObject);
                    if (n >= 3 && !Historia.Feito("m8_feita"))
                    {
                        Historia.Marcar("m8_feita");
                        world.GetComponent<RegionalInventory>()?.Service?.CollectRegionalGold(70);
                        Saque.SoltarRecurso(Recursos.Couro, 2, world.Hero.transform.position + Vector3.up);
                        AvisoNaTela.Mostrar("Missão concluída: A Última Viagem · +70 de ouro", 4);
                    }
                };
            }
        }

        // A page you can see from far: a sheet of paper floating and turning, with a thin column of light over it.
        static void Folha(Transform onde)
        {
            var papel = GameObject.CreatePrimitive(PrimitiveType.Cube); Destroy(papel.GetComponent<Collider>());
            papel.name = "Papel"; papel.transform.SetParent(onde, false); papel.transform.localPosition = Vector3.up * .9f; papel.transform.localScale = new Vector3(.42f, .02f, .56f);
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.SetColor("_BaseColor", new Color(.96f, .92f, .8f));
            m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", new Color(.5f, .45f, .3f)); papel.GetComponent<Renderer>().sharedMaterial = m;
            papel.AddComponent<Flutuar>();
            var coluna = GameObject.CreatePrimitive(PrimitiveType.Cylinder); Destroy(coluna.GetComponent<Collider>());
            coluna.name = "Coluna"; coluna.transform.SetParent(onde, false); coluna.transform.localPosition = Vector3.up * 3.5f; coluna.transform.localScale = new Vector3(.18f, 3.5f, .18f);
            var r = coluna.GetComponent<Renderer>(); r.sharedMaterial = EfeitosDeHabilidade.Brilho; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", new Color(1f, .85f, .45f, .35f)); r.SetPropertyBlock(block);
        }
        sealed class Flutuar : MonoBehaviour
        {
            float y0; void Start() => y0 = transform.localPosition.y;
            void Update() { transform.localPosition = new Vector3(0, y0 + Mathf.Sin(Time.time * 1.8f) * .15f, 0); transform.Rotate(8 * Time.deltaTime, 50 * Time.deltaTime, 0, Space.Self); }
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
