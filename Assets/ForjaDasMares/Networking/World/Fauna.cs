using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace ForjaDasMares.Networking
{
    // Wild animals (decided with Fabricio):
    // - around the Refúgio they are peaceful: they wander, graze and step away; nobody attacks anybody;
    // - near the river (Nascente do Rio) they were TAKEN by the Breu: glowing violet eyes, dark aura, aggressive.
    //   Those are hunted: leather, meat and antler.
    public static class Fauna
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded; Loaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); }
        static void Loaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == Historia.Refugio) new GameObject("FaunaDoRefugio").AddComponent<Soltar>().Configure(false, 3, 2);
            else if (scene.name.Contains("Nascente")) new GameObject("FaunaTomada").AddComponent<Soltar>().Configure(true, 2, 4);
        }

        sealed class Soltar : MonoBehaviour
        {
            bool tomados; int cervos, javalis;
            public void Configure(bool taken, int deer, int boars) { tomados = taken; cervos = deer; javalis = boars; }
            IEnumerator Start()
            {
                RegionalWorld world = null;
                for (int i = 0; i < 120 && (world = FindFirstObjectByType<RegionalWorld>()) == null; i++) yield return null;
                while (world != null && world.Hero == null) yield return null;
                yield return null;
                if (world == null) yield break;
                var random = new System.Random(tomados ? 11 : 5);
                for (int i = 0; i < cervos + javalis; i++)
                {
                    var kind = i < cervos ? AnimalSelvagem.Especie.Cervo : AnimalSelvagem.Especie.Javali;
                    if (!Ponto(world, random, i, out var home)) continue;
                    AnimalSelvagem.Criar(kind, tomados, home, world);
                }
            }
            // Peaceful ones on the fields around the town; taken ones spread over the area, away from the arrival.
            bool Ponto(RegionalWorld world, System.Random random, int index, out Vector3 point)
            {
                for (int tries = 0; tries < 40; tries++)
                {
                    Vector3 p;
                    if (!tomados)
                    {
                        // Outside the walls, on the fields around the town (not on the roads, not by the river in the south).
                        if (!Coleta.ForaDaCidade(random, out p)) continue;
                    }
                    else
                    {
                        // Nascente do Rio: they live in the Bosque Sussurrante (never in the safe corners).
                        var home = NascenteDoRio.Animais[index % NascenteDoRio.Animais.Length];
                        p = home + new Vector3((float)random.NextDouble() * 6 - 3, 0, (float)random.NextDouble() * 6 - 3);
                    }
                    if (NavMesh.SamplePosition(p, out var hit, 3, NavMesh.AllAreas)) { point = hit.position; return true; }
                }
                point = default; return false;
            }
        }
    }

    public sealed class AnimalSelvagem : MonoBehaviour
    {
        public enum Especie { Javali, Cervo }
        enum Estado { Vagando, Parado, Pastando, Fugindo, Alerta, Investida, Recuperando, Voltando, Morto }

        Especie especie; bool tomado; Vector3 home; float wander;
        public string Nome => especie == Especie.Javali ? "Javali" : "Cervo";
        NavMeshAgent agent; Animator anim; Health health; RegionalWorld world; Transform model;
        Estado estado; float until; Vector3 chargeDir; bool hitThisCharge; string playing;
        AttackDefinition golpe; GameObject breu; BarraDeVida barra;
        static readonly Color Roxo = new Color(.75f, .2f, 1f);

        public bool Tomado => tomado;
        public bool Vivo => health != null && health.IsAlive;
        public static AnimalSelvagem Criar(Especie kind, bool taken, Vector3 at, RegionalWorld world)
        {
            var prefab = Resources.Load<GameObject>("Animais/" + (kind == Especie.Javali ? "Javali" : "Cervo"));
            if (prefab == null) { Debug.LogWarning("Animal sem prefab: rode Forja/Animais/Preparar animais."); return null; }
            var go = Instantiate(prefab, at, Quaternion.Euler(0, Random.Range(0, 360f), 0));
            var a = go.AddComponent<AnimalSelvagem>();
            a.especie = kind; a.tomado = taken; a.home = at; a.world = world; a.wander = taken ? 16 : 9;
            return a;
        }

        void Start()
        {
            anim = GetComponentInChildren<Animator>(); model = anim != null ? anim.transform : transform;
            agent = gameObject.AddComponent<NavMeshAgent>();
            agent.speed = Walk; agent.angularSpeed = 300; agent.acceleration = 12; agent.radius = especie == Especie.Javali ? .45f : .4f; agent.height = 1.2f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            if (!tomado)
            {
                // Peaceful: nothing can hit it and it hits nothing.
                foreach (var c in GetComponentsInChildren<Collider>()) c.gameObject.layer = 2;
            }
            else
            {
                var combat = world.Hero.GetComponent<HeroCombat>();
                int mask = combat != null ? combat.TargetMask : 0, layer = 0;
                for (int i = 0; i < 32; i++) if ((mask & (1 << i)) != 0) { layer = i; break; }
                // A taller hit box, so shots at chest height do not fly over a low animal (a boar is knee-high).
                var body = GetComponent<CapsuleCollider>();
                var acerto = new GameObject("Acerto").AddComponent<BoxCollider>(); acerto.transform.SetParent(transform, false);
                float wide = body != null ? body.radius * 2 : .7f, longo = body != null ? Mathf.Max(body.height, wide) : 1.2f;
                acerto.center = new Vector3(0, .9f, body != null ? body.center.z : 0); acerto.size = new Vector3(wide, 1.8f, longo);
                foreach (var c in GetComponentsInChildren<Collider>()) c.gameObject.layer = layer;
                health = gameObject.AddComponent<Health>();
                health.Configure(especie == Especie.Javali ? 60 : 45, CombatFaction.Enemy);
                health.Impact += (dir, force) => { if (agent.enabled && agent.isOnNavMesh) agent.Move(new Vector3(dir.x, 0, dir.z).normalized * force * .25f); };
                golpe = ScriptableObject.CreateInstance<AttackDefinition>();
                golpe.damage = especie == Especie.Javali ? 12 : 9; golpe.knockback = 3; golpe.range = 1.4f; golpe.arc = 360; golpe.color = Roxo;
                Tomar();
                // Its life is shown at the top of the screen when focused (AlvoNoTopo), not over its head.
            }
            Mudar(Estado.Parado, Random.Range(1f, 3f));
        }
        void OnDestroy() { if (golpe != null) Destroy(golpe); }

        float Walk => especie == Especie.Javali ? 1.6f : 1.4f;
        float Run => especie == Especie.Javali ? 5.5f : 7f;

        // The Breu took it: violet eyes, dark-violet mist and a darker body.
        void Tomar()
        {
            breu = new GameObject("Breu"); breu.transform.SetParent(transform, false);
            EfeitosDeHabilidade.Aura(breu.transform, new Color(.45f, .1f, .7f), true, .6f, 12);
            var head = Find(model, "head");
            if (head != null)
                for (int s = -1; s <= 1; s += 2)
                {
                    var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(eye.GetComponent<Collider>());
                    eye.name = "Olho"; eye.transform.SetParent(head, true);
                    float size = especie == Especie.Javali ? .07f : .06f;
                    eye.transform.position = head.position + transform.forward * (especie == Especie.Javali ? .22f : .16f) + transform.right * s * .09f + Vector3.up * .05f;
                    eye.transform.localScale = Vector3.one * size / Mathf.Max(.0001f, head.lossyScale.x);
                    var r = eye.GetComponent<Renderer>(); r.sharedMaterial = EfeitosDeHabilidade.Brilho;
                    var b = new MaterialPropertyBlock(); b.SetColor("_BaseColor", Roxo * 2f); r.SetPropertyBlock(b);
                }
            foreach (var r in GetComponentsInChildren<SkinnedMeshRenderer>())
            { var b = new MaterialPropertyBlock(); r.GetPropertyBlock(b); b.SetColor("_BaseColor", new Color(.62f, .55f, .7f)); r.SetPropertyBlock(b); }
        }
        static Transform Find(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>()) if (t.name.EndsWith(name)) return t;
            return null;
        }

        void Mudar(Estado next, float seconds) { estado = next; until = Time.time + seconds; }
        void Tocar(string state, float fade = .2f)
        {
            if (anim == null || playing == state) return;
            if (!anim.HasState(0, Animator.StringToHash(state))) state = state == "Sprint" ? "Run" : state == "Pastar" ? "Idle" : state;
            if (!anim.HasState(0, Animator.StringToHash(state))) return;
            anim.CrossFadeInFixedTime(state, fade); playing = state;
        }

        void Update()
        {
            if (estado == Estado.Morto) return;
            if (health != null && !health.IsAlive) { StartCoroutine(Morrer()); return; }
            if (health != null && health.Carried)
            {
                // Taken by a blow (the Netuno's bull and whirlpool): follow it along the ground.
                if (agent.enabled && agent.isOnNavMesh) { Vector3 to = health.CarriedTo - transform.position; to.y = 0; agent.Move(to * Mathf.Min(1, 14 * Time.deltaTime)); }
                return;
            }
            var hero = world != null && world.Hero != null ? world.Hero.transform : null;
            Vector3 toHero = hero != null ? hero.position - transform.position : Vector3.one * 999; toHero.y = 0;
            float distance = toHero.magnitude;
            bool heroAlive = hero != null && world.Hero.Health.IsAlive;
            // The townsfolk talk about it later (Descobertas).
            if (tomado && !viuBreu && distance < 14) { viuBreu = true; Descobertas.Marcar(Descobertas.BreuVisto); }
            if (agent.enabled && agent.isOnNavMesh && estado != Estado.Investida) Tocar(agent.velocity.magnitude > 3.2f ? "Run" : agent.velocity.magnitude > .2f ? "Walk" : estado == Estado.Pastando ? "Pastar" : "Idle");

            if (!tomado)
            {
                // Peaceful: step away from a player that comes too close.
                if (distance < 3.5f && estado != Estado.Fugindo) { Fugir(toHero, 6, Walk * 1.8f); return; }
                Vagar();
                return;
            }
            // Taken by the Breu.
            if ((transform.position - home).sqrMagnitude > (wander + 14) * (wander + 14) && estado != Estado.Voltando && estado != Estado.Investida)
            { estado = Estado.Voltando; agent.speed = Run; agent.SetDestination(home); }
            switch (estado)
            {
                case Estado.Voltando:
                    if (health.Current < health.Maximum) health.Heal(1);
                    if ((transform.position - home).sqrMagnitude < 9) Mudar(Estado.Parado, 1);
                    return;
                case Estado.Alerta:
                    Encarar(toHero);
                    if (Time.time >= until) Investir(toHero);
                    return;
                case Estado.Investida:
                    Correr(hero, distance);
                    return;
                case Estado.Recuperando:
                    if (Time.time >= until) Mudar(Estado.Parado, .3f);
                    return;
                case Estado.Fugindo:
                    if (Time.time >= until) Mudar(Estado.Parado, 1);
                    return;
            }
            float aggro = especie == Especie.Javali ? 9 : 6;
            if (heroAlive && distance < aggro)
            {
                if (especie == Especie.Cervo && health.Current < health.Maximum * .35f) { Fugir(toHero, 10, Run); return; }
                if (distance < (especie == Especie.Javali ? 8 : 4.5f)) { Alertar(); return; }
                agent.speed = Run * .6f; agent.SetDestination(hero.position); return;
            }
            Vagar();
        }

        void Vagar()
        {
            agent.speed = Walk;
            if (Time.time < until) return;
            if (estado == Estado.Vagando && agent.isOnNavMesh && agent.remainingDistance > .4f) return;
            float roll = Random.value;
            if (roll < .35f) { Mudar(especie == Especie.Cervo && Random.value < .6f ? Estado.Pastando : Estado.Parado, Random.Range(2f, 5f)); if (agent.isOnNavMesh) agent.ResetPath(); return; }
            var p = home + Random.insideUnitSphere * wander; p.y = home.y;
            if (DentroDaCidade(p)) p = home;
            if (NavMesh.SamplePosition(p, out var hit, 3, NavMesh.AllAreas) && agent.isOnNavMesh) agent.SetDestination(hit.position);
            Mudar(Estado.Vagando, Random.Range(4f, 8f));
        }
        // The peaceful ones of the Refúgio stay on the fields: they never walk back in through the gates.
        bool DentroDaCidade(Vector3 p) => !tomado && new Vector2(p.x, p.z).magnitude < 40;
        void Fugir(Vector3 toHero, float far, float speed)
        {
            var away = -toHero.normalized * far; var p = transform.position + away;
            if (DentroDaCidade(p)) p = transform.position + new Vector3(transform.position.x, 0, transform.position.z).normalized * far;
            if (NavMesh.SamplePosition(p, out var hit, 4, NavMesh.AllAreas) && agent.isOnNavMesh) { agent.speed = speed; agent.SetDestination(hit.position); }
            Mudar(Estado.Fugindo, 2.5f);
        }
        void Encarar(Vector3 toHero)
        {
            if (toHero.sqrMagnitude > .01f) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toHero), 540 * Time.deltaTime);
        }
        // Warning before the charge: it stops, the eyes flare and violet sparks rise. That is the moment to dodge.
        void Alertar()
        {
            if (agent.isOnNavMesh) agent.ResetPath();
            Mudar(Estado.Alerta, especie == Especie.Javali ? .8f : .6f);
            Tocar("Idle", .1f);
            EfeitosDeHabilidade.Explosao(transform.position + Vector3.up * .2f, 1.1f, Roxo, 18);
        }
        void Investir(Vector3 toHero)
        {
            chargeDir = toHero.sqrMagnitude > .01f ? toHero.normalized : transform.forward;
            // The agent stays on (moved by hand), so there is no snap/teleport when the charge ends.
            hitThisCharge = false; if (agent.isOnNavMesh) agent.ResetPath(); agent.updateRotation = false;
            Mudar(Estado.Investida, especie == Especie.Javali ? .75f : .35f);
            Tocar("Sprint", .05f);
        }
        void Correr(Transform hero, float distance)
        {
            float speed = especie == Especie.Javali ? 11 : 9;
            if (agent.isOnNavMesh)
            {
                var before = transform.position;
                agent.Move(chargeDir * speed * Time.deltaTime);
                if ((transform.position - before).sqrMagnitude < .0001f) until = 0; // hit a wall: stop
            }
            else until = 0;
            transform.rotation = Quaternion.LookRotation(chargeDir);
            if (!hitThisCharge && hero != null && distance < 1.4f)
            {
                hitThisCharge = true;
                // Through ApplyHit: a dodge at the right moment is a PERFECT dodge (and fills the Super).
                world.Hero.Health.ApplyHit(golpe, chargeDir, health);
            }
            if (Time.time >= until)
            {
                agent.updateRotation = true;
                Mudar(Estado.Recuperando, especie == Especie.Javali ? 1.3f : .9f);
            }
        }

        static bool viuBreu;
        IEnumerator Morrer()
        {
            estado = Estado.Morto;
            if (tomado) Descobertas.Marcar(Descobertas.TomadoAbatido);
            if (agent.enabled) agent.enabled = false;
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
            if (breu != null) Destroy(breu);
            if (barra != null) Destroy(barra.gameObject);
            if (anim != null) anim.speed = 0;
            // What it leaves (on the ground).
            var at = transform.position;
            // Each piece jumps out and lands on the ground; click to pick it up.
            int carne = especie == Especie.Javali ? (Random.value < .5f ? 2 : 1) : 1;
            for (int i = 0; i < carne; i++) Saque.SoltarRecurso(Recursos.Carne, 1, at);
            Saque.SoltarRecurso(Recursos.Couro, 1, at);
            if (especie == Especie.Cervo && Random.value < .5f) Saque.SoltarRecurso(Recursos.Chifre, 1, at);
            EfeitosDeHabilidade.Explosao(at + Vector3.up * .5f, 1.4f, Roxo, 30);
            // No death animation yet: it falls on its side and sinks into the ground.
            var start = transform.rotation; var side = start * Quaternion.Euler(0, 0, 90);
            for (float t = 0; t < .5f; t += Time.deltaTime) { transform.rotation = Quaternion.Slerp(start, side, t / .5f); yield return null; }
            yield return new WaitForSeconds(3);
            for (float t = 0; t < 1.5f; t += Time.deltaTime) { transform.position += Vector3.down * Time.deltaTime * .5f; yield return null; }
            // A new one comes back later at the same place.
            var kind = especie; var spot = home; var w = world;
            w.StartCoroutine(Voltar(kind, spot, w));
            Destroy(gameObject);
        }
        static IEnumerator Voltar(Especie kind, Vector3 spot, RegionalWorld world)
        {
            yield return new WaitForSeconds(45);
            if (world != null) Criar(kind, true, spot, world);
        }
    }

    // Small life bar over a taken animal.
    // Life bars over creatures: a solid, readable bar (not a glow) in the game's rustic style. Gold frame, dark
    // bed, red life with a light top edge, notches every quarter (so you can tell how much is left at a glance), a
    // pale "recent damage" strip that shrinks after each hit, a white flash when hit, and a red pulse when it is
    // almost dead. Bosses also show their name and the numbers.
    public sealed class BarraDeVida : MonoBehaviour
    {
        Health health; Transform fill, brilho, rastro, back, moldura; TextMesh numeros; Renderer fillR;
        bool grande; float largura = 1, espessura = .1f, rastroF = 1, ultimoF = 1, espera, flash;
        static Material plano;
        static readonly Color Vermelho = new Color(.82f, .13f, .11f), Claro = new Color(1f, .5f, .4f), Rastro = new Color(1f, .86f, .62f);

        // Top of the model (renderers), from its pivot.
        public static float Altura(Transform owner, float padrao)
        {
            var rs = owner.GetComponentsInChildren<Renderer>(); bool first = true; var b = new Bounds();
            foreach (var r in rs) { if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer || r.GetComponentInParent<BarraDeVida>() != null) continue; if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds); }
            return first ? padrao : Mathf.Clamp(b.max.y - owner.position.y, 1.2f, 7f);
        }
        // Big bar with the name and numbers, always shown, for bosses and mini-bosses (just above the model).
        public static BarraDeVida Grande(Transform owner, Health hp, string nome)
        {
            float top = Altura(owner, 2.4f);
            var bar = Montar(owner, hp, top + .5f, 2.8f, .26f, true);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var label = Texto(bar.transform, font, nome, new Vector3(0, .34f, -.004f), 48, .05f, TextAnchor.LowerCenter, new Color(1f, .86f, .7f));
            bar.numeros = Texto(bar.transform, font, "", new Vector3(0, 0, -.006f), 40, .028f, TextAnchor.MiddleCenter, Color.white);
            return bar;
        }
        // Small bar (shown only after the first hit) for common creatures.
        public static BarraDeVida Criar(Transform owner, Health hp, float height) => Montar(owner, hp, height, 1.25f, .13f, false);

        static BarraDeVida Montar(Transform owner, Health hp, float height, float w, float h, bool big)
        {
            var go = new GameObject("Vida"); go.transform.SetParent(owner, false);
            var s = Mathf.Max(.01f, owner.lossyScale.y); go.transform.localPosition = Vector3.up * height / s; go.transform.localScale = Vector3.one / s;
            var bar = go.AddComponent<BarraDeVida>(); bar.health = hp; bar.grande = big; bar.largura = w; bar.espessura = h;
            float borda = big ? .06f : .04f;
            bar.moldura = Quad(go.transform, new Color(.78f, .6f, .3f), w + borda * 2, h + borda * 2, 0);
            bar.back = Quad(go.transform, new Color(.07f, .04f, .04f), w, h, -.001f);
            bar.rastro = Quad(go.transform, Rastro, w, h, -.002f);
            bar.fill = Quad(go.transform, Vermelho, w, h, -.003f); bar.fillR = bar.fill.GetComponent<Renderer>();
            bar.brilho = Quad(go.transform, Claro, w, h * .3f, -.0035f);
            // Notches every quarter (thin dark lines over the life).
            for (int i = 1; i < 4; i++) { var n = Quad(go.transform, new Color(.07f, .04f, .04f), big ? .03f : .022f, h, -.004f); n.localPosition = new Vector3(-w * .5f + w * i / 4f, 0, -.004f); }
            if (hp != null) bar.ultimoF = bar.rastroF = Fracao(hp);
            return bar;
        }
        static TextMesh Texto(Transform parent, Font font, string text, Vector3 at, int size, float character, TextAnchor anchor, Color c)
        {
            var label = new GameObject("Texto").AddComponent<TextMesh>(); label.transform.SetParent(parent, false); label.transform.localPosition = at;
            label.text = text; label.font = font; label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            label.fontSize = size; label.characterSize = character; label.anchor = anchor; label.alignment = TextAlignment.Center; label.color = c;
            return label;
        }
        static Transform Quad(Transform parent, Color c, float w, float h, float z)
        {
            if (plano == null) plano = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = "BarraDeVida" };
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad); Destroy(q.GetComponent<Collider>());
            q.transform.SetParent(parent, false); q.transform.localPosition = new Vector3(0, 0, z); q.transform.localScale = new Vector3(w, h, 1);
            var r = q.GetComponent<Renderer>(); r.sharedMaterial = plano; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            Cor(r, c);
            return q.transform;
        }
        static readonly MaterialPropertyBlock bloco = new MaterialPropertyBlock();
        static void Cor(Renderer r, Color c) { bloco.SetColor("_BaseColor", c); r.SetPropertyBlock(bloco); }
        static float Fracao(Health h) => Mathf.Clamp01((float)h.Current / Mathf.Max(1, h.Maximum));
        static void Encher(Transform t, float f, float w, float h, float y)
        {
            t.localScale = new Vector3(Mathf.Max(.0001f, w * f), h, 1);
            t.localPosition = new Vector3(-(1 - f) * w * .5f, y, t.localPosition.z);
        }
        void LateUpdate()
        {
            var cam = Camera.main; if (cam != null) transform.rotation = cam.transform.rotation;
            float f = health != null ? Fracao(health) : 0;
            // A hit: flash, and the pale strip waits a moment before shrinking to the new life.
            if (f < ultimoF - .0001f) { flash = 1; espera = .45f; }
            if (f > rastroF) rastroF = f;
            ultimoF = f;
            if (espera > 0) espera -= Time.deltaTime; else rastroF = Mathf.MoveTowards(rastroF, f, Time.deltaTime * .7f);
            flash = Mathf.MoveTowards(flash, 0, Time.deltaTime * 6);
            Encher(fill, f, largura, espessura, 0);
            Encher(rastro, rastroF, largura, espessura, 0);
            Encher(brilho, f, largura, espessura * .3f, espessura * .35f);
            var cor = Vermelho;
            if (f < .25f) cor = Color.Lerp(Vermelho, new Color(1f, .3f, .2f), .5f + Mathf.Sin(Time.time * 10) * .5f);
            Cor(fillR, Color.Lerp(cor, Color.white, flash * .8f));
            if (numeros != null && health != null) numeros.text = Mathf.Max(0, health.Current) + " / " + health.Maximum;
            bool show = grande ? health != null && health.IsAlive : f < 1 && f > 0;
            foreach (Transform t in transform) if (t.gameObject.activeSelf != show) t.gameObject.SetActive(show);
        }
    }
}
