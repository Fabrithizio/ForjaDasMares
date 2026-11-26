using UnityEngine;

namespace ForjaDasMares
{
    // Perdição de Netuno (legendary axe, last form) — its look. How it plays is in MareCrescente (same numbers).
    // Decided with Fabricio (26/11), all sideways, nothing from above:
    //   combo   Corte das Marés      1: side slash · 2: back the other way · 3: a full spin around the hero
    //   passive Maré Crescente       drops orbit the blade (one per blow that lands); with 5, a wave runs forward
    //   super   Fúria do Touro Abissal  a bull of water charges forward, then a whirlpool pulls everyone in
    // While held: the blades shed small glowing drops. On contact: a splash of water and ice.
    public sealed class PerdicaoDeNetuno : EstiloDeArma
    {
        // Deep blue water with a bright cyan edge (Fabricio: "azul escuro com brilho", like the reference sheet).
        static readonly Color Espuma = new Color(.75f, .97f, 1f), Ciano = new Color(.2f, .7f, 1f), Mar = new Color(.02f, .2f, .62f), Fundo = new Color(.01f, .09f, .32f);

        // ---------------- held ----------------
        public override void Segurando(CortesDaEspada cortes, bool naMao, Vector3 ponta)
        {
            var marca = cortes.GetComponent<MarcaDaMare>();
            if (marca == null) marca = cortes.gameObject.AddComponent<MarcaDaMare>();
            marca.Atualizar(cortes, naMao, ponta);
        }

        // Drops of the Maré Crescente around the blade, the wave when it is released, and the drops that fall from
        // the blades. One per hero.
        public sealed class MarcaDaMare : MonoBehaviour
        {
            readonly MeshRenderer[] orbe = new MeshRenderer[MareCrescente.Max]; Transform raiz;
            ParticleSystem gotas, brilho; float relogio, giro, cheia; int ondas = -1, ultimoQuadro;
            public void Atualizar(CortesDaEspada cortes, bool naMao, Vector3 ponta)
            {
                ultimoQuadro = Time.frameCount;
                var mare = cortes.Heroi != null ? cortes.Heroi.Mecanica as MareCrescente : null;
                // A wave released by the passive (watched, so it shows exactly when the game sends it).
                if (mare != null)
                {
                    if (ondas >= 0 && mare.Ondas != ondas) Onda(mare.OndaInicio, mare.OndaDirecao, mare.OndaAlcance, Mathf.Max(.5f, transform.lossyScale.y));
                    ondas = mare.Ondas;
                }
                if (gotas == null) gotas = Vfx.Particulas(transform, "GotasDeNetuno", Vfx.AditivoPonto, -.05f, false, 60);
                if (brilho == null) brilho = Vfx.Particulas(transform, "BrilhoDaMare", Vfx.AditivoPonto, -.2f, false, 80);
                if (raiz == null)
                {
                    raiz = new GameObject("MareCrescente").transform; raiz.SetParent(transform, false);
                    var quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
                    for (int i = 0; i < orbe.Length; i++) orbe[i] = Vfx.Peca(raiz, "Gota" + i, quad, Vfx.AditivoPonto);
                }
                bool mostrar = naMao && mare != null;
                raiz.gameObject.SetActive(mostrar);
                if (naMao && (relogio -= Time.deltaTime) <= 0)
                {
                    relogio = .09f;
                    Vfx.Gota(gotas, ponta + Random.insideUnitSphere * .22f, Vector3.up * Random.Range(.1f, .35f) + Random.insideUnitSphere * .1f, Color.Lerp(Ciano, Espuma, Random.value * .5f), Random.Range(.04f, .09f), Random.Range(.6f, 1.1f));
                }
                if (!mostrar) return;
                // Five drops circling the axe head; the full ones glow, the empty ones are faint.
                int n = mare.Gotas; bool pronta = n >= MareCrescente.Max;
                cheia = Mathf.MoveTowards(cheia, pronta ? 1 : 0, Time.deltaTime * 4);
                giro += Time.deltaTime * (pronta ? 420 : 150);
                var cam = Camera.main; Quaternion olhar = cam != null ? cam.transform.rotation : Quaternion.identity;
                float s = Mathf.Max(.5f, transform.lossyScale.y), r = (.42f + .08f * cheia) * s;
                for (int i = 0; i < orbe.Length; i++)
                {
                    float a = (giro + i * 360f / orbe.Length) * Mathf.Deg2Rad;
                    var p = ponta + (Vector3.right * Mathf.Cos(a) + Vector3.forward * Mathf.Sin(a)) * r + Vector3.up * Mathf.Sin(a * 2 + giro * .02f) * .06f * s;
                    bool cheio = i < n;
                    float pulso = 1 + .25f * cheia * Mathf.Sin(Time.time * 14 + i);
                    orbe[i].transform.SetPositionAndRotation(p, olhar);
                    orbe[i].transform.localScale = Vector3.one * (cheio ? .2f * pulso : .09f) * s;
                    Vfx.Cor(orbe[i], cheio ? Color.Lerp(Ciano, Espuma, .35f * cheia) : new Color(Ciano.r, Ciano.g, Ciano.b, .18f));
                    if (cheio && pronta && Random.value < .35f) Vfx.Gota(brilho, p, Random.insideUnitSphere * .4f + Vector3.up * .3f, Espuma, Random.Range(.04f, .08f), Random.Range(.25f, .45f));
                }
            }
            void LateUpdate() { if (raiz != null && Time.frameCount - ultimoQuadro > 1) raiz.gameObject.SetActive(false); }
            void OnDestroy() { if (raiz != null) Destroy(raiz.gameObject); }
        }

        // ---------------- blows ----------------
        public override void Golpe(CortesDaEspada cortes, Golpe g)
        {
            switch (g.variante)
            {
                case 2: Giro(cortes, g); break;
                case 4: Touro(cortes, g); break;
                default: Amplo(cortes, g); break;
            }
        }

        // The swing seen from above, on the ground plane, turning the way the axe turned in the animation.
        static Golpe Plano(Golpe g, float span)
        {
            Vector3 forward = g.Frente, right = Vector3.Cross(Vector3.up, forward);
            bool toLeft = Vector3.Dot(g.Direcao(g.fim), right) < Vector3.Dot(g.Direcao(g.inicio), right);
            var h = g; h.centro = g.Chao + Vector3.up * .55f * g.tamanho; h.normal = Vector3.up; h.x = forward; h.y = right;
            h.inicio = toLeft ? span * .5f : -span * .5f; h.fim = -h.inicio;
            return h;
        }

        // Golpes 1 and 2 (Fabricio's drawing, 26/11): a big half circle of water around the hero, covering the front
        // and both sides — the real area of the blow (200°, 2.3 m). Deep indigo-blue inside, a wide bright cyan band
        // outside, thin white lines of foam. It sweeps the way the axe turned and bursts in a splash where it ends.
        static void Amplo(CortesDaEspada c, Golpe g)
        {
            float s = g.tamanho, r = 2.1f * s;
            var h = Plano(g, 210);
            var indigo = new Color(.13f, .19f, .7f);
            c.Crescente(h.centro, h.normal, h.x, h.y, h.inicio, h.fim, r * .78f, .7f * s, new Color(Ciano.r, Ciano.g, Ciano.b, .5f), Fundo, Color.Lerp(Ciano, Espuma, .4f), .45f, .8f);
            c.Crescente(h.centro, h.normal, h.x, h.y, h.inicio, h.fim, r, 1.25f * s, Ciano, indigo, Espuma, .55f, 1);
            c.Faiscas(h.centro, h.normal, h.x, h.y, h.inicio, h.fim, r, Ciano, 24);
            Spray(h, r, 48, 1);
            Respingo(h.centro + h.Direcao(h.fim) * r, 1.1f * s);
        }

        // Golpe 3: the axe goes all the way around — a full ring of water, a ring that runs out along the ground and
        // a heavier splash (it pushes everyone away).
        static void Giro(CortesDaEspada c, Golpe g)
        {
            float s = g.tamanho, r = 2.3f * s;
            var h = Plano(g, 360);
            var indigo = new Color(.13f, .19f, .7f);
            c.Crescente(h.centro, h.normal, h.x, h.y, h.inicio, h.fim, r * .75f, .8f * s, new Color(Ciano.r, Ciano.g, Ciano.b, .55f), Fundo, Color.Lerp(Ciano, Espuma, .4f), .5f, .8f);
            c.Crescente(h.centro, h.normal, h.x, h.y, h.inicio, h.fim, r, 1.35f * s, Ciano, indigo, Espuma, .6f, 1);
            c.Faiscas(h.centro, h.normal, h.x, h.y, h.inicio, h.fim, r, Ciano, 36);
            Spray(h, r, 70, 1.2f);
            AnelNoChao(g.Chao + Vector3.up * .06f, 3.2f * s, .7f);
            EfeitosDeHabilidade.Tremer(.05f);
        }

        // A ring of water running out along the ground.
        static void AnelNoChao(Vector3 at, float radius, float life)
        {
            var fx = Vfx.Animado.Criar("AnelDaMare", at, life, null);
            var ring = Vfx.Peca(fx.transform, "Anel", Vfx.Anel(.3f), Vfx.Aditivo);
            var body = Vfx.Peca(fx.transform, "AnelCorpo", Vfx.Anel(.5f), Vfx.Solido);
            fx.passo = t =>
            {
                float k = 1 - (1 - t) * (1 - t);
                ring.transform.localScale = body.transform.localScale = Vector3.one * radius * Mathf.Lerp(.3f, 1f, k);
                Vfx.Cor(ring, new Color(Ciano.r, Ciano.g, Ciano.b, 1 - t)); Vfx.Cor(body, new Color(Mar.r, Mar.g, Mar.b, .75f * (1 - t)));
            };
            fx.passo(0);
        }

        // A ring of water bursting at a point, turned to the camera, with drops (the splash at the end of a swing).
        static void Respingo(Vector3 at, float size)
        {
            var fx = Vfx.Animado.Criar("RespingoDoGolpe", at, 1f, null);
            var cam = Camera.main;
            if (cam != null) fx.transform.rotation = Quaternion.FromToRotation(Vector3.up, -cam.transform.forward);
            var ring = Vfx.Peca(fx.transform, "Anel", Vfx.Anel(.3f), Vfx.Aditivo);
            var body = Vfx.Peca(fx.transform, "AnelCorpo", Vfx.Anel(.45f), Vfx.Solido);
            var drops = Vfx.Particulas(fx.transform, "Gotas", Vfx.AditivoPonto, 1.2f, true, 40);
            for (int i = 0; i < 18; i++)
                Vfx.Gota(drops, at, Random.insideUnitSphere * 3.5f + Vector3.up * Random.Range(.5f, 2.5f), Color.Lerp(Ciano, Espuma, Random.value), Random.Range(.05f, .12f), Random.Range(.3f, .6f));
            fx.passo = t =>
            {
                float k = Mathf.Clamp01(t / .45f), grow = 1 - (1 - k) * (1 - k);
                ring.transform.localScale = body.transform.localScale = Vector3.one * size * Mathf.Lerp(.2f, 1f, grow);
                Vfx.Cor(ring, new Color(Espuma.r, Espuma.g, Espuma.b, 1 - k)); Vfx.Cor(body, new Color(Mar.r, Mar.g, Mar.b, .7f * (1 - k)));
            };
            fx.passo(0);
        }

        // Drops and foam flung out of the arc of a blow.
        static void Spray(Golpe g, float r, int amount, float force)
        {
            var fx = Vfx.Animado.Criar("SprayDeNetuno", g.centro, 1.4f, null);
            var drops = Vfx.Particulas(fx.transform, "Gotas", Vfx.AditivoPonto, 1.1f, true);
            var foam = Vfx.Particulas(fx.transform, "Espuma", Vfx.SolidoPonto, .2f, false);
            for (int i = 0; i < amount; i++)
            {
                float a = Mathf.Lerp(g.inicio + (g.fim - g.inicio) * .3f, g.fim + 10, Random.value);
                var dir = g.Direcao(a); var along = Vector3.Cross(g.normal, dir);
                Vfx.Gota(drops, g.centro + dir * r * Random.Range(.8f, 1.05f), (dir * Random.Range(1.5f, 4.5f) + along * Random.Range(1, 3.5f) + Vector3.up * Random.Range(.5f, 2.5f)) * force,
                    Color.Lerp(Ciano, Espuma, Random.value), Random.Range(.05f, .13f), Random.Range(.4f, .8f));
                if (i % 3 == 0) Vfx.Gota(foam, g.centro + dir * r * Random.Range(.85f, 1f), (dir * Random.Range(.3f, 1f) + Vector3.up * .3f) * force,
                    new Color(.85f, .97f, 1f, .7f), Random.Range(.25f, .5f), Random.Range(.4f, .7f));
            }
        }

        // Impacto do Maremoto: rings of water run out and crystals burst up (the end of the whirlpool).
        static void Impacto(Vector3 at, float radius, int crystals, float s)
        {
            EfeitosDeHabilidade.Explosao(at + Vector3.up * .3f, radius * .7f, Ciano, Mathf.RoundToInt(30 * s));
            EfeitosDeHabilidade.Tremer(.08f * s);
            var fx = Vfx.Animado.Criar("ImpactoDoMaremoto", at, 1.6f, null);
            var root = fx.transform;
            // Two rings of water: a fast bright one and a slower, wider foam one.
            var ring1 = Vfx.Peca(root, "Anel", Vfx.Anel(.35f), Vfx.Aditivo);
            var ring1b = Vfx.Peca(root, "AnelCorpo", Vfx.Anel(.5f), Vfx.Solido);
            var ring2 = Vfx.Peca(root, "AnelEspuma", Vfx.Anel(.18f), Vfx.Aditivo);
            // Crystals in two crowns around the hit.
            var shards = new Transform[crystals]; var glows = new MeshRenderer[crystals]; var tilt = new Quaternion[crystals]; var heights = new float[crystals];
            for (int i = 0; i < crystals; i++)
            {
                bool inner = i % 2 == 0;
                float a = (i + Random.Range(-.25f, .25f)) / crystals * 360f;
                Vector3 dir = Quaternion.Euler(0, a, 0) * Vector3.forward;
                var piece = Vfx.Peca(root, "Cristal", Vfx.Cristal, Vfx.Solido).transform;
                var glow = Vfx.Peca(piece, "Brilho", Vfx.Cristal, Vfx.Aditivo); glow.transform.localScale = Vector3.one * 1.12f;
                piece.localPosition = dir * radius * (inner ? Random.Range(.28f, .42f) : Random.Range(.55f, .72f));
                tilt[i] = Quaternion.AngleAxis(Random.Range(18, 38), Vector3.Cross(Vector3.up, dir)) * Quaternion.Euler(0, Random.Range(0, 360), 0);
                heights[i] = (inner ? Random.Range(1f, 1.5f) : Random.Range(.6f, 1f)) * s;
                piece.localRotation = tilt[i]; piece.localScale = Vector3.zero;
                shards[i] = piece; glows[i] = glow;
            }
            var drops = Vfx.Particulas(root, "Gotas", Vfx.AditivoPonto, 1.3f, true, 200);
            var rocks = Vfx.Particulas(root, "Pedras", Vfx.SolidoPonto, 2f, false, 60);
            for (int i = 0; i < 60 * s; i++)
            {
                var d = Random.insideUnitCircle.normalized; var flat = new Vector3(d.x, 0, d.y);
                Vfx.Gota(drops, at + flat * Random.Range(.2f, .8f), flat * Random.Range(1.5f, 5f) + Vector3.up * Random.Range(3f, 7f), Color.Lerp(Ciano, Espuma, Random.value), Random.Range(.06f, .15f), Random.Range(.5f, 1f));
                if (i % 3 == 0) Vfx.Gota(rocks, at + flat * Random.Range(.2f, 1f), flat * Random.Range(1f, 3.5f) + Vector3.up * Random.Range(2.5f, 5f), new Color(.22f, .24f, .3f, 1), Random.Range(.1f, .22f), Random.Range(.6f, 1.1f));
            }
            fx.passo = t =>
            {
                float k1 = Mathf.Clamp01(t / .3f), k2 = Mathf.Clamp01((t - .08f) / .45f);
                ring1.transform.localScale = Vector3.one * radius * Mathf.Lerp(.2f, 1f, 1 - (1 - k1) * (1 - k1));
                ring1b.transform.localScale = ring1.transform.localScale * .96f;
                Vfx.Cor(ring1, new Color(Ciano.r, Ciano.g, Ciano.b, 1 - k1)); Vfx.Cor(ring1b, new Color(Mar.r, Mar.g, Mar.b, .8f * (1 - k1)));
                ring2.transform.localScale = Vector3.one * radius * 1.15f * Mathf.Lerp(.1f, 1f, 1 - (1 - k2) * (1 - k2));
                Vfx.Cor(ring2, new Color(Espuma.r, Espuma.g, Espuma.b, .9f * (1 - k2) * (k2 > 0 ? 1 : 0)));
                for (int i = 0; i < shards.Length; i++)
                {
                    float delay = (i % 2 == 0 ? 0 : .05f) + i * .004f, u = t - delay;
                    float grow = Mathf.Clamp01(u / .1f), sink = Mathf.Clamp01((u - .55f) / .3f);
                    float h = heights[i] * (1 - (1 - grow) * (1 - grow) * (1 - grow)) * (1 - sink * sink);
                    shards[i].localScale = new Vector3(heights[i] * .9f, h, heights[i] * .9f);
                    Vfx.Cor(glows[i], new Color(.5f, .95f, 1f, .45f * (1 - sink)));
                }
            };
            fx.passo(0);
        }

        // The wave of the Maré Crescente: a curling wave runs forward along the ground (as far as the game's wave
        // goes, stopping at walls), throwing foam from its crest.
        static void Onda(Vector3 start, Vector3 forward, float distance, float s)
        {
            const float Width = 3.2f, Height = 1.7f, Life = 1.05f, Travel = MareCrescente.OndaTempo;
            var fx = Vfx.Animado.Criar("OndaAvancante", start, Life + .9f, null);
            fx.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            var mesh = WaveMesh(); fx.meus.Add(mesh);
            var body = Vfx.Peca(fx.transform, "Corpo", mesh, Vfx.Solido);
            var crest = Vfx.Peca(fx.transform, "Crista", mesh, Vfx.Aditivo);
            var foam = Vfx.Particulas(fx.transform, "Espuma", Vfx.SolidoPonto, .35f, false, 220);
            var drops = Vfx.Particulas(fx.transform, "Gotas", Vfx.AditivoPonto, 1.2f, true, 220);
            var trail = Vfx.Particulas(fx.transform, "Rastro", Vfx.AditivoPonto, 0, false, 120);
            EfeitosDeHabilidade.Tremer(.05f);
            fx.passo = t =>
            {
                float time = t * fx.vida; if (time > Life) { body.enabled = crest.enabled = false; return; }
                float go = Mathf.Clamp01(time / Travel), rise = Mathf.Clamp01(time / .14f), fall = Mathf.Clamp01((time - Travel) / (Life - Travel));
                float dist = distance * (1 - (1 - go) * (1 - go));
                Vector3 front = start + forward * dist;
                body.transform.position = crest.transform.position = front;
                float h = Height * s * rise * (1 - fall * .9f), w = Width * s * (1 + .15f * go);
                body.transform.localScale = new Vector3(w, h, h);
                crest.transform.localScale = new Vector3(w * .98f, h * 1.02f, h * 1.04f);
                Vfx.Cor(body, new Color(Mar.r, Mar.g, Mar.b, .9f * (1 - fall)));
                Vfx.Cor(crest, new Color(Ciano.r, Ciano.g, Ciano.b, 1 - fall));
                if (fall >= 1) return;
                var right = Vector3.Cross(Vector3.up, forward);
                for (int i = 0; i < 4; i++)
                {
                    Vector3 p = front + right * Random.Range(-.5f, .5f) * w + Vector3.up * h * Random.Range(.75f, 1.05f) + forward * h * .3f;
                    Vfx.Gota(foam, p, forward * Random.Range(1f, 3f) + Vector3.up * Random.Range(.5f, 2f), new Color(.88f, .98f, 1f, .75f), Random.Range(.2f, .45f), Random.Range(.35f, .6f));
                    Vfx.Gota(drops, p, forward * Random.Range(2f, 5f) + Vector3.up * Random.Range(1.5f, 4f) + right * Random.Range(-1.5f, 1.5f), Color.Lerp(Ciano, Espuma, Random.value), Random.Range(.05f, .12f), Random.Range(.4f, .7f));
                }
                Vfx.Gota(trail, front - forward * .4f * h + right * Random.Range(-.45f, .45f) * w + Vector3.up * .05f, Vector3.zero, new Color(.3f, .8f, 1f, .5f), Random.Range(.5f, .9f), .7f);
            };
            fx.passo(0);
        }

        // Wave sheet: x across (-.5..5), profile in y/z (1 = full height), curling forward at the crest.
        static Mesh WaveMesh()
        {
            const int C = 18;
            Vector2[] profile = { new Vector2(-.95f, 0), new Vector2(-.7f, .22f), new Vector2(-.42f, .52f), new Vector2(-.12f, .82f), new Vector2(.12f, 1f), new Vector2(.34f, 1.02f), new Vector2(.5f, .9f), new Vector2(.56f, .74f) };
            int R = profile.Length; var v = new Vector3[(C + 1) * R]; var uv = new Vector2[v.Length]; var col = new Color[v.Length]; var t = new int[C * (R - 1) * 6];
            for (int i = 0; i <= C; i++)
            {
                float u = i / (float)C, taper = Mathf.Pow(Mathf.Max(0, Mathf.Sin(Mathf.PI * u)), .55f); // sin(π) is a hair below 0: Pow of it is NaN
                for (int j = 0; j < R; j++)
                {
                    float k = j / (R - 1f);
                    v[i * R + j] = new Vector3(u - .5f, profile[j].y * taper, profile[j].x * (.6f + .4f * taper));
                    uv[i * R + j] = new Vector2(u, Mathf.Lerp(.05f, .95f, k));
                    col[i * R + j] = new Color(1, 1, 1, taper);
                }
            }
            int n = 0;
            for (int i = 0; i < C; i++) for (int j = 0; j < R - 1; j++)
                {
                    int a = i * R + j, b = (i + 1) * R + j;
                    t[n++] = a; t[n++] = a + 1; t[n++] = b; t[n++] = b; t[n++] = a + 1; t[n++] = b + 1;
                }
            var m = new Mesh { name = "Onda", vertices = v, uv = uv, colors = col, triangles = t }; m.RecalculateBounds();
            return m;
        }

        // The whirlpool where the bull stops: it rises, rocks and drops spin in it while it pulls the enemies in,
        // then it crashes down in an Impacto do Maremoto.
        static void Redemoinho(Vector3 at, float s)
        {
            const float Life = MareCrescente.RedemoinhoTempo, Radius = MareCrescente.RedemoinhoRaio;
            var fx = Vfx.Animado.Criar("RedemoinhoAbissal", at, Life + 1f, null);
            var outer = Funnel(2.6f, 4.6f, .9f); var inner = Funnel(1.6f, 3.4f, .5f); fx.meus.Add(outer); fx.meus.Add(inner);
            var o1 = Vfx.Peca(fx.transform, "Redemoinho", outer, Vfx.Solido); var o2 = Vfx.Peca(fx.transform, "RedemoinhoBrilho", outer, Vfx.Aditivo);
            var i1 = Vfx.Peca(fx.transform, "Miolo", inner, Vfx.Aditivo);
            var ground = Vfx.Peca(fx.transform, "Chao", Vfx.Anel(.6f), Vfx.Solido); ground.transform.localPosition = Vector3.up * .05f;
            var rocks = Vfx.Particulas(fx.transform, "Pedras", Vfx.SolidoPonto, 0, false, 80);
            var drops = Vfx.Particulas(fx.transform, "Gotas", Vfx.AditivoPonto, 0, true, 240);
            var orbit = rocks.velocityOverLifetime; orbit.enabled = true; orbit.space = ParticleSystemSimulationSpace.Local; orbit.orbitalY = new ParticleSystem.MinMaxCurve(4f); orbit.y = new ParticleSystem.MinMaxCurve(1.2f);
            var orbit2 = drops.velocityOverLifetime; orbit2.enabled = true; orbit2.space = ParticleSystemSimulationSpace.Local; orbit2.orbitalY = new ParticleSystem.MinMaxCurve(6f); orbit2.y = new ParticleSystem.MinMaxCurve(2.4f);
            var rm = rocks.main; rm.simulationSpace = ParticleSystemSimulationSpace.Local; var dm = drops.main; dm.simulationSpace = ParticleSystemSimulationSpace.Local;
            EfeitosDeHabilidade.Tremer(.06f);
            bool crashed = false; float spin = 0, last = 0;
            fx.passo = t =>
            {
                float time = t * fx.vida, dt = time - last; last = time;
                if (time > Life)
                {
                    o1.enabled = o2.enabled = i1.enabled = ground.enabled = false;
                    if (!crashed) { crashed = true; Impacto(at + Vector3.up * .05f, Radius, 22, 1.5f * s); }
                    return;
                }
                float rise = Mathf.Clamp01(time / .3f), end = Mathf.Clamp01((time - (Life - .3f)) / .3f);
                spin += dt * Mathf.Lerp(380, 900, rise);
                float h = (.2f + .8f * (1 - (1 - rise) * (1 - rise))) * (1 - end * .7f), w = (1 - end * .6f) * s;
                o1.transform.localRotation = o2.transform.localRotation = Quaternion.Euler(0, spin, 0);
                o1.transform.localScale = o2.transform.localScale = new Vector3(w, h * s, w);
                i1.transform.localRotation = Quaternion.Euler(0, -spin * 1.4f, 0); i1.transform.localScale = new Vector3(w, h * s, w);
                ground.transform.localScale = Vector3.one * Radius * (.4f + .6f * rise);
                Vfx.Cor(o1, new Color(Mar.r, Mar.g, Mar.b, .8f * (1 - end))); Vfx.Cor(o2, new Color(Ciano.r, Ciano.g, Ciano.b, .9f * (1 - end)));
                Vfx.Cor(i1, new Color(Espuma.r, Espuma.g, Espuma.b, .55f * (1 - end))); Vfx.Cor(ground, new Color(Fundo.r, Fundo.g, Fundo.b, .5f * rise * (1 - end)));
                for (int i = 0; i < 3; i++)
                {
                    float a = Random.value * Mathf.PI * 2, r = Random.Range(1f, 2.4f) * s;
                    var local = new Vector3(Mathf.Cos(a) * r, Random.Range(0f, .6f), Mathf.Sin(a) * r);
                    Vfx.Gota(drops, local, Vector3.zero, Color.Lerp(Ciano, Espuma, Random.value), Random.Range(.06f, .14f), Random.Range(.5f, .9f));
                    if (i == 0 && Random.value < .6f) Vfx.Gota(rocks, local * .9f, Vector3.zero, new Color(.2f, .22f, .28f, 1), Random.Range(.12f, .25f), Random.Range(.8f, 1.2f));
                }
            };
            fx.passo(0);
        }

        // Fúria do Touro Abissal: the axe opens a great slash and a bull of water charges out of it along the
        // ground (as far as the game's bull goes) with two glowing horns and burning eyes, a trail of foam behind;
        // where it stops, the whirlpool. Placeholder body until Fabricio's bull model: a wall of water with horns.
        static void Touro(CortesDaEspada c, Golpe g)
        {
            float s = g.tamanho;
            var h = Plano(g, 240);
            c.Crescente(h.centro, h.normal, h.x, h.y, h.inicio, h.fim, 2.4f * s, 1.6f * s, Ciano, new Color(.13f, .19f, .7f), Espuma, .6f, 1);
            Vector3 forward = g.Frente, start = g.Chao + forward;
            int walls = c.Heroi != null ? c.Heroi.WallMask : 0;
            float distance = MecanicasDeArma.Alcance(start, forward, MareCrescente.TouroDistancia, walls);
            const float Travel = MareCrescente.TouroTempo, Width = 3.4f, Height = 2.3f;
            var fx = Vfx.Animado.Criar("TouroAbissal", start, Travel + 1.2f, null);
            fx.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            var wave = WaveMesh(); var horn = HornMesh(); fx.meus.Add(wave); fx.meus.Add(horn);
            var bull = new GameObject("Touro").transform; bull.SetParent(fx.transform, false);
            var body = Vfx.Peca(bull, "Corpo", wave, Vfx.Solido); var crest = Vfx.Peca(bull, "Crista", wave, Vfx.Aditivo);
            body.transform.localScale = new Vector3(Width, Height, Height); crest.transform.localScale = new Vector3(Width * .98f, Height * 1.02f, Height * 1.04f);
            var horns = new MeshRenderer[4];
            for (int side = 0; side < 2; side++)
            {
                float m = side == 0 ? -1 : 1;
                var hs = Vfx.Peca(bull, "Chifre", horn, Vfx.Solido); var hg = Vfx.Peca(hs.transform, "ChifreBrilho", horn, Vfx.Aditivo);
                hs.transform.localPosition = new Vector3(m * .55f, Height * .95f, .35f);
                hs.transform.localRotation = Quaternion.Euler(-25, m * 20, 0); hs.transform.localScale = new Vector3(-m * 1.1f, 1.1f, 1.1f);
                hg.transform.localScale = Vector3.one * 1.12f;
                horns[side * 2] = hs; horns[side * 2 + 1] = hg;
            }
            var eyes = Vfx.Particulas(bull, "Olhos", Vfx.AditivoPonto, 0, false, 40);
            var foam = Vfx.Particulas(fx.transform, "Espuma", Vfx.SolidoPonto, .35f, false, 260);
            var drops = Vfx.Particulas(fx.transform, "Gotas", Vfx.AditivoPonto, 1.2f, true, 260);
            var trail = Vfx.Particulas(fx.transform, "Rastro", Vfx.AditivoPonto, 0, false, 160);
            EfeitosDeHabilidade.Tremer(.1f);
            bool pool = false;
            fx.passo = t =>
            {
                float time = t * fx.vida, go = Mathf.Clamp01(time / Travel), rise = Mathf.Clamp01(time / .12f), fade = Mathf.Clamp01((time - Travel) / .3f);
                float d = distance * (1 - (1 - go) * (1 - go));
                bull.position = start + forward * d;
                bull.localScale = Vector3.one * s * rise * (1 - fade * .8f);
                Vfx.Cor(body, new Color(Mar.r, Mar.g, Mar.b, .92f * (1 - fade))); Vfx.Cor(crest, new Color(Ciano.r, Ciano.g, Ciano.b, 1 - fade));
                for (int i = 0; i < horns.Length; i++) Vfx.Cor(horns[i], i % 2 == 0 ? new Color(Fundo.r, Fundo.g, Fundo.b, .95f * (1 - fade)) : new Color(Espuma.r, Espuma.g, Espuma.b, .7f * (1 - fade)));
                if (go >= 1 && !pool) { pool = true; Redemoinho(bull.position, s); EfeitosDeHabilidade.Tremer(.08f); }
                if (fade >= 1) return;
                var right = Vector3.Cross(Vector3.up, forward);
                // Burning eyes: two bright points under the horns, renewed every frame.
                for (int e = -1; e <= 1; e += 2)
                    Vfx.Gota(eyes, bull.position + right * e * .38f * s + Vector3.up * Height * .78f * s + forward * .55f * s, Vector3.zero, Espuma, .32f * s, .06f);
                for (int i = 0; i < 5; i++)
                {
                    Vector3 p = bull.position + right * Random.Range(-.5f, .5f) * Width * s + Vector3.up * Height * s * Random.Range(.6f, 1.05f) + forward * .4f * s;
                    Vfx.Gota(foam, p, forward * Random.Range(2f, 5f) + Vector3.up * Random.Range(.5f, 2f), new Color(.88f, .98f, 1f, .75f), Random.Range(.25f, .55f), Random.Range(.35f, .6f));
                    Vfx.Gota(drops, p, forward * Random.Range(3f, 7f) + Vector3.up * Random.Range(1.5f, 4f) + right * Random.Range(-2f, 2f), Color.Lerp(Ciano, Espuma, Random.value), Random.Range(.06f, .14f), Random.Range(.4f, .7f));
                }
                Vfx.Gota(trail, bull.position - forward * .5f * s + right * Random.Range(-.5f, .5f) * Width * s + Vector3.up * .05f, Vector3.zero, new Color(.3f, .8f, 1f, .5f), Random.Range(.7f, 1.2f), .9f);
            };
            fx.passo(0);
        }

        // A horn: a tapered tube curving up and out (base at the origin, tip about 1.1 up), deep at the root, bright at the tip.
        static Mesh HornMesh()
        {
            const int L = 12, S = 8; var v = new Vector3[(L + 1) * (S + 1)]; var uv = new Vector2[v.Length]; var col = new Color[v.Length]; var t = new int[L * S * 6];
            for (int i = 0; i <= L; i++)
            {
                float k = i / (float)L, a = k * 1.9f;
                Vector3 c = new Vector3(Mathf.Sin(a) * .55f - k * .15f, (1 - Mathf.Cos(a)) * .25f + k * .75f, Mathf.Sin(a * .5f) * .1f);
                Vector3 tangent = new Vector3(Mathf.Cos(a) * .55f * 1.9f - .15f, Mathf.Sin(a) * .25f * 1.9f + .75f, 0).normalized;
                Vector3 side = Vector3.Cross(tangent, Vector3.forward).normalized, up = Vector3.Cross(side, tangent);
                float r = Mathf.Lerp(.17f, .015f, Mathf.Pow(k, .8f));
                for (int j = 0; j <= S; j++)
                {
                    float b = j / (float)S * Mathf.PI * 2;
                    v[i * (S + 1) + j] = c + (side * Mathf.Cos(b) + up * Mathf.Sin(b)) * r;
                    uv[i * (S + 1) + j] = new Vector2(j / (float)S, Mathf.Lerp(.55f, .9f, k));
                    col[i * (S + 1) + j] = Color.Lerp(new Color(.3f, .6f, 1f, 1), Color.white, k);
                }
            }
            int n = 0;
            for (int i = 0; i < L; i++) for (int j = 0; j < S; j++)
                {
                    int a = i * (S + 1) + j, b = a + S + 1;
                    t[n++] = a; t[n++] = b; t[n++] = a + 1; t[n++] = a + 1; t[n++] = b; t[n++] = b + 1;
                }
            var m = new Mesh { name = "Chifre", vertices = v, uv = uv, colors = col, triangles = t }; m.RecalculateBounds();
            return m;
        }

        // A funnel opening upwards: bottom radius `low`, top radius `top`, height `height`; spiral bands of water.
        static Mesh Funnel(float top, float height, float low)
        {
            const int S = 36, H = 12; var v = new Vector3[(S + 1) * (H + 1)]; var uv = new Vector2[v.Length]; var col = new Color[v.Length]; var t = new int[S * H * 6];
            for (int j = 0; j <= H; j++)
            {
                float k = j / (float)H, r = Mathf.Lerp(low, top, k * k), y = k * height;
                float edge = Mathf.Clamp01(k / .15f) * Mathf.Clamp01((1 - k) / .25f);
                for (int i = 0; i <= S; i++)
                {
                    float a = i / (float)S * Mathf.PI * 2;
                    v[j * (S + 1) + i] = new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r);
                    uv[j * (S + 1) + i] = new Vector2(i / (float)S, .82f);
                    float band = Mathf.Pow(Mathf.Max(0, .5f + .5f * Mathf.Sin(a * 3 + k * 9)), 2.5f);
                    col[j * (S + 1) + i] = new Color(1, 1, 1, edge * (.2f + .8f * band));
                }
            }
            int n = 0;
            for (int j = 0; j < H; j++) for (int i = 0; i < S; i++)
                {
                    int a = j * (S + 1) + i, b = a + S + 1;
                    t[n++] = a; t[n++] = b; t[n++] = a + 1; t[n++] = a + 1; t[n++] = b; t[n++] = b + 1;
                }
            var m = new Mesh { name = "Redemoinho", vertices = v, uv = uv, colors = col, triangles = t }; m.RecalculateBounds();
            return m;
        }

        // ---------------- on the enemy ----------------
        public override void Acerto(Vector3 onde, Quaternion frente, bool forte)
        {
            EfeitosDeHabilidade.Extra("bolha", onde, frente, forte ? 1.3f : .9f);
            EfeitosDeHabilidade.Extra("gelo", onde, frente, forte ? 1.1f : .7f);
            var fx = Vfx.Animado.Criar("RespingoDeNetuno", onde, 1.1f, null);
            var drops = Vfx.Particulas(fx.transform, "Gotas", Vfx.AditivoPonto, 1.2f, true, 60);
            for (int i = 0; i < (forte ? 28 : 16); i++)
                Vfx.Gota(drops, onde, Random.insideUnitSphere * 3f + Vector3.up * Random.Range(1f, 3f), Color.Lerp(Ciano, Espuma, Random.value), Random.Range(.05f, .12f), Random.Range(.35f, .7f));
        }
    }
}
