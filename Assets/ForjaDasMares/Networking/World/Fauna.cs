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
                barra = BarraDeVida.Criar(transform, health, especie == Especie.Javali ? 1.5f : 2.1f);
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
    public sealed class BarraDeVida : MonoBehaviour
    {
        Health health; Transform fill; Transform back; bool grande; float largura = 1, espessura = .1f;
        // Big bar with the name, always shown, for bosses and mini-bosses (placed just above the model).
        public static BarraDeVida Grande(Transform owner, Health hp, string nome)
        {
            float top = 2.4f;
            var rs = owner.GetComponentsInChildren<Renderer>();
            if (rs.Length > 0) { var b = rs[0].bounds; foreach (var r in rs) if (!(r is ParticleSystemRenderer) && !(r is TrailRenderer)) b.Encapsulate(r.bounds); top = Mathf.Clamp(b.max.y - owner.position.y, 1.4f, 7f); }
            var go = new GameObject("Vida"); go.transform.SetParent(owner, false);
            var s = owner.lossyScale.y; go.transform.localPosition = Vector3.up * (top + .45f) / Mathf.Max(.01f, s); go.transform.localScale = Vector3.one / Mathf.Max(.01f, s);
            var bar = go.AddComponent<BarraDeVida>(); bar.health = hp; bar.grande = true; bar.largura = 2.6f; bar.espessura = .22f;
            bar.back = Quad(go.transform, new Color(.06f, .02f, .04f, .95f), 2.66f, .3f, 0);
            bar.fill = Quad(go.transform, new Color(1f, .26f, .22f), 2.6f, .22f, -.001f);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var label = new GameObject("Nome").AddComponent<TextMesh>(); label.transform.SetParent(go.transform, false);
            label.transform.localPosition = new Vector3(0, .36f, -.002f);
            label.text = nome; label.font = font; label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            label.fontSize = 48; label.characterSize = .05f; label.anchor = TextAnchor.LowerCenter; label.alignment = TextAlignment.Center;
            label.color = new Color(1f, .82f, .72f);
            return bar;
        }
        public static BarraDeVida Criar(Transform owner, Health hp, float height)
        {
            var go = new GameObject("Vida"); go.transform.SetParent(owner, false); go.transform.localPosition = Vector3.up * height;
            var b = go.AddComponent<BarraDeVida>(); b.health = hp;
            b.back = Quad(go.transform, new Color(.08f, .02f, .1f, .9f), 1.02f, .14f, 0);
            b.fill = Quad(go.transform, new Color(.8f, .25f, 1f), 1f, .1f, -.001f);
            return b;
        }
        static Transform Quad(Transform parent, Color c, float w, float h, float z)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad); Destroy(q.GetComponent<Collider>());
            q.transform.SetParent(parent, false); q.transform.localPosition = new Vector3(0, 0, z); q.transform.localScale = new Vector3(w, h, 1);
            var r = q.GetComponent<Renderer>(); r.sharedMaterial = EfeitosDeHabilidade.Brilho; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", c); r.SetPropertyBlock(block);
            return q.transform;
        }
        void LateUpdate()
        {
            var cam = Camera.main; if (cam != null) transform.rotation = cam.transform.rotation;
            float f = health != null ? Mathf.Clamp01((float)health.Current / Mathf.Max(1, health.Maximum)) : 0;
            fill.localScale = new Vector3(largura * f, espessura, 1); fill.localPosition = new Vector3(-(1 - f) * largura * .5f, 0, -.001f);
            bool show = grande ? health != null && health.IsAlive : f < 1;
            back.gameObject.SetActive(show); fill.gameObject.SetActive(show);
            if (grande) foreach (Transform t in transform) if (t != back && t != fill) t.gameObject.SetActive(show);
        }
    }
}
