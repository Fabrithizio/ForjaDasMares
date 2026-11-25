using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // The Raízes-Mães of the cities: every city has one. Touching one connects the player to it (each player has
    // their own connections) and opens the world map; on the map, a connected city can be chosen to travel to
    // (with a yes/no confirmation), and the player arrives in front of that city's Raiz-Mãe.
    public static class RaizesMaes
    {
        public sealed class Raiz { public string id, cena, nome; }
        public static readonly Raiz[] Todas =
        {
            new Raiz { id = "refugio", cena = Historia.Refugio, nome = "Raiz-Mãe do Refúgio" },
            new Raiz { id = "foz", cena = "Area_Foz", nome = "Raiz-Mãe da Foz" },
        };
        public static Raiz PorId(string id) { foreach (var r in Todas) if (r.id == id) return r; return null; }
        public static Raiz PorCena(string cena) { foreach (var r in Todas) if (r.cena == cena) return r; return null; }

        static string Key => "ForjaRaizes_" + Historia.Who;
        static HashSet<string> Lidas() => new HashSet<string>(PlayerPrefs.GetString(Key, "").Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
        public static bool Conectada(string id) => Lidas().Contains(id) || (id == "refugio" && Historia.CenaDoNo != "");
        public static void Conectar(string id)
        {
            var set = Lidas();
            if (!set.Add(id)) return;
            PlayerPrefs.SetString(Key, string.Join(",", set)); PlayerPrefs.Save();
        }

        // Where the player is arriving by a Raiz-Mãe (null = arriving some other way).
        public static string ChegandoEm;
        // The front of this scene's Raiz-Mãe (when the scene has one built while playing): where you arrive and rise again.
        public static Vector3? PontoAqui;

        // Touch: connects, records the player in the root (falling brings you back here) and opens the map.
        public static void Tocar(RegionalWorld world, string id, Vector3 at)
        {
            var raiz = PorId(id);
            if (world == null || raiz == null || MapaDoMundo.Aberto) return;
            bool nova = !Conectada(id);
            Conectar(id);
            DescobertasDoMapa.Descobrir(id);
            bool first = Historia.CenaDoNo != raiz.cena;
            Historia.GravarNo(raiz.cena);
            if (nova && id != "refugio")
            {
                EfeitosDeHabilidade.Explosao(at + Vector3.up * 2, 2.2f, new Color(.45f, 1f, .88f), 50);
                AvisoNaTela.Mostrar(raiz.nome + " ligada · agora dá para viajar até aqui pelas outras Raízes-Mães.", 4);
            }
            MapaDoMundo.Abrir(world, true);
        }

        public static void Viajar(RegionalWorld world, string id)
        {
            var raiz = PorId(id);
            if (world == null || raiz == null) return;
            ChegandoEm = id;
            AreasDoMundo.VindoDe = null;
            Partir(world, raiz.cena);
        }

        // Leaving through a root: the bell rings, the light closes over the screen, the new place loads behind it,
        // and the light opens again where you arrive (no loading hitch on screen).
        public static void Partir(RegionalWorld world, string cena)
        {
            if (world == null) return;
            world.Hero.GetComponent<ArenaInput>()?.BlockCommands(true);
            Sino();
            EfeitosDeHabilidade.Explosao(world.Hero.transform.position + Vector3.up, 1.8f, new Color(.45f, 1f, .88f), 40);
            VeuDaRaiz.Fechar(() => world.Viajar(cena));
        }

        // The bell of the roots, heard the same wherever the camera is (it is the travel sound: it calls, the root carries).
        // It survives the change of scene, so it keeps ringing while you travel.
        static AudioSource sino;
        public static void Sino()
        {
            if (sino == null)
            {
                var go = new GameObject("SinoDaRaiz"); Object.DontDestroyOnLoad(go);
                sino = go.AddComponent<AudioSource>(); sino.playOnAwake = false; sino.spatialBlend = 0; sino.clip = Historia.Sino(true);
            }
            float efeitos = Volumes.De(Volumes.Efeitos); sino.Stop(); if (efeitos <= 0) return;
            sino.volume = .85f * efeitos; sino.Play();
        }

        // Arriving through a root: a ring of sap opens on the ground, motes rise, and the light over the screen opens.
        public static void Chegada(Vector3 at)
        {
            var cor = new Color(.45f, 1f, .88f);
            EfeitosDeHabilidade.Explosao(at + Vector3.up * .2f, 2.4f, cor, 46);
            EfeitosDeHabilidade.Explosao(at + Vector3.up * 1.2f, 1.1f, Color.Lerp(cor, Color.white, .5f), 24);
            var luz = new GameObject("LuzDaChegada").AddComponent<Light>(); luz.transform.position = at + Vector3.up * 1.5f;
            luz.type = LightType.Point; luz.color = cor; luz.range = 8; luz.intensity = 5; luz.shadows = LightShadows.None;
            luz.gameObject.AddComponent<ApagarLuz>();
            VeuDaRaiz.Abrir();
        }
        sealed class ApagarLuz : MonoBehaviour
        {
            Light luz; float t;
            void Awake() => luz = GetComponent<Light>();
            void Update() { t += Time.deltaTime; luz.intensity = Mathf.Lerp(5, 0, t / 1.4f); if (t >= 1.4f) Destroy(gameObject); }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded; Loaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); }
        static void Loaded(Scene scene, LoadSceneMode mode)
        {
            PontoAqui = null;
            // The Refúgio has its own (the building in the scene, RegionalWorld). The others are built here.
            var raiz = PorCena(scene.name);
            if (raiz != null && raiz.id != "refugio" && Object.FindFirstObjectByType<RaizMaeConstruida>() == null)
                new GameObject("RaizMae_" + raiz.id).AddComponent<RaizMaeConstruida>().id = raiz.id;
        }
    }

    // A Raiz-Mãe built while playing (cities whose scene has none yet, like the Vila da Foz): a stone core with a
    // glowing crystal, embraced by a twisted trunk and roots, with a crown of leaves. Clicking it (or F near it) uses it.
    public sealed class RaizMaeConstruida : MonoBehaviour
    {
        public string id;
        static readonly Vector3 Onde = new Vector3(0, 0, -14); // south of the middle, away from the roads
        static readonly Color Seiva = new Color(.45f, 1f, .88f);
        RegionalWorld world; Light brilho; Transform cristal;

        IEnumerator Start()
        {
            for (int i = 0; i < 300 && (world = FindFirstObjectByType<RegionalWorld>()) == null; i++) yield return null;
            while (world != null && world.Hero == null) yield return null;
            if (world == null) yield break;
            var raiz = RaizesMaes.PorId(id);
            transform.position = Chao(Onde);
            // It faces south, toward the camera: arriving in front of it, the tree is behind the hero, never covering the view.
            transform.rotation = Quaternion.Euler(0, 180, 0);
            var frente = Chao(transform.position + transform.forward * 4.2f);
            if (UnityEngine.AI.NavMesh.SamplePosition(frente, out var nav, 1.5f, UnityEngine.AI.NavMesh.AllAreas)) frente = nav.position;
            else
            {
                // No ground there: keep the old side (north).
                transform.rotation = Quaternion.identity;
                frente = Chao(transform.position + Vector3.forward * 4.2f);
                if (UnityEngine.AI.NavMesh.SamplePosition(frente, out nav, 4, UnityEngine.AI.NavMesh.AllAreas)) frente = nav.position;
            }
            RaizesMaes.PontoAqui = frente;
            var target = Construir(raiz.nome, frente);
            target.Interacting += actor => { if (world != null && actor == world.Hero.transform && world.Hero.Health.IsAlive) RaizesMaes.Tocar(world, id, transform.position); };
            yield return null;
            // Arriving by a Raiz-Mãe, or pulled back after falling: in front of it, facing the town.
            bool chegando = RaizesMaes.ChegandoEm == id, renascendo = Historia.RenascerNoNo && Historia.CenaDoNo == raiz.cena;
            if (chegando || renascendo)
            {
                RaizesMaes.ChegandoEm = null; Historia.RenascerNoNo = false;
                world.ChegarEm(frente, transform.rotation);
                // The bell calls, the root carries: it rings every time someone arrives through it.
                RaizesMaes.Chegada(frente);
                if (renascendo) TextoFlutuante.Mostrar(frente + Vector3.up * 2.2f, "A " + raiz.nome + " puxou você de volta.", Seiva);
            }
        }

        void OnDestroy() { RaizesMaes.PontoAqui = null; }

        void Update()
        {
            if (brilho != null) brilho.intensity = 2.6f + Mathf.Sin(Time.time * 1.6f) * .9f;
            if (cristal != null) { cristal.Rotate(0, 30 * Time.deltaTime, 0, Space.World); cristal.localPosition = new Vector3(0, 2.1f + Mathf.Sin(Time.time * 1.3f) * .12f, 1.75f); }
        }

        static Vector3 Chao(Vector3 p)
        {
            if (Physics.Raycast(new Vector3(p.x, 60, p.z), Vector3.down, out var hit, 120, 1 << 8, QueryTriggerInteraction.Ignore)) return hit.point;
            return p;
        }

        static Material Mat(Color c, float smooth, Color? glow = null)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", smooth);
            if (glow.HasValue) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", glow.Value); }
            return m;
        }

        GameObject Peca(PrimitiveType type, Vector3 local, Vector3 scale, Vector3 euler, Material mat)
        {
            var go = GameObject.CreatePrimitive(type); go.transform.SetParent(transform, false);
            Destroy(go.GetComponent<Collider>());
            go.transform.localPosition = local; go.transform.localRotation = Quaternion.Euler(euler); go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        WorldClickTarget Construir(string nome, Vector3 frente)
        {
            var casca = Mat(new Color(.42f, .3f, .2f), .15f);
            var pedra = Mat(new Color(.55f, .55f, .52f), .2f);
            var folha = Mat(new Color(.3f, .55f, .28f), .1f, new Color(.05f, .16f, .08f));
            var seiva = Mat(Seiva, .9f, Seiva * 1.8f);
            // Stone core (the old portal machine) with the crystal in front.
            Peca(PrimitiveType.Cube, new Vector3(0, 1.6f, 0), new Vector3(3.2f, 3.2f, 3.2f), Vector3.zero, pedra);
            Peca(PrimitiveType.Cylinder, new Vector3(0, 2.1f, 1.62f), new Vector3(1.7f, .05f, 1.7f), new Vector3(90, 0, 0), Mat(new Color(.35f, .35f, .33f), .3f));
            cristal = Peca(PrimitiveType.Cube, new Vector3(0, 2.1f, 1.75f), new Vector3(.55f, .55f, .55f), new Vector3(45, 0, 45), seiva).transform;
            // The trunk: three twisted strands rising over the core.
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120;
                var dir = Quaternion.Euler(0, a, 0) * Vector3.forward;
                Peca(PrimitiveType.Cylinder, dir * 1.3f + Vector3.up * 3.2f, new Vector3(1.1f, 3.4f, 1.1f), new Vector3(-12, a + 25, 10), casca);
                Peca(PrimitiveType.Cylinder, dir * .6f + Vector3.up * 7.2f, new Vector3(.9f, 1.8f, .9f), new Vector3(14, a - 30, -12), casca);
            }
            // Roots embracing the core and spreading on the ground.
            for (int i = 0; i < 9; i++)
            {
                float a = i * 40 + 10;
                var dir = Quaternion.Euler(0, a, 0) * Vector3.forward;
                if (Vector3.Angle(dir, Vector3.forward) < 30) continue; // the front stays open (the crystal)
                Peca(PrimitiveType.Capsule, dir * 2.8f + Vector3.up * .55f, new Vector3(.6f, 2.1f, .6f), Quaternion.LookRotation(dir).eulerAngles + new Vector3(70, 0, 0), casca);
            }
            // Crown of leaves.
            Peca(PrimitiveType.Sphere, new Vector3(0, 9.6f, 0), new Vector3(7.5f, 4.2f, 7.5f), Vector3.zero, folha);
            Peca(PrimitiveType.Sphere, new Vector3(2.2f, 8.6f, 1), new Vector3(4.5f, 3, 4.5f), Vector3.zero, folha);
            Peca(PrimitiveType.Sphere, new Vector3(-2.3f, 8.8f, -.8f), new Vector3(4.8f, 3.2f, 4.8f), Vector3.zero, folha);

            brilho = new GameObject("Brilho").AddComponent<Light>(); brilho.transform.SetParent(transform, false);
            brilho.transform.localPosition = new Vector3(0, 2.4f, 2.6f);
            brilho.type = LightType.Point; brilho.color = Seiva; brilho.range = 11; brilho.shadows = LightShadows.None;

            // Motes of sap rising slowly around it.
            var ps = gameObject.AddComponent<ParticleSystem>();
            var main = ps.main; main.startLifetime = 4f; main.startSpeed = .4f; main.startSize = new ParticleSystem.MinMaxCurve(.08f, .18f);
            main.startColor = new Color(Seiva.r, Seiva.g, Seiva.b, .8f); main.maxParticles = 80; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -.02f;
            var emission = ps.emission; emission.rateOverTime = 12;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 4; shape.rotation = new Vector3(-90, 0, 0);
            GetComponent<ParticleSystemRenderer>().sharedMaterial = EfeitosDeHabilidade.Brilho;

            // Solid body (walls for walking) that is also what the click hits.
            gameObject.layer = 9;
            var corpo = gameObject.AddComponent<CapsuleCollider>(); corpo.center = new Vector3(0, 4.5f, 0); corpo.radius = 2.1f; corpo.height = 9;
            var copa = new GameObject("Copa"); copa.transform.SetParent(transform, false); copa.layer = 9;
            var copaCol = copa.AddComponent<SphereCollider>(); copaCol.center = new Vector3(0, 9.4f, 0); copaCol.radius = 3.6f;

            // The sign above, like the Refúgio's.
            var placa = new GameObject("Nome").AddComponent<TextMesh>(); placa.transform.SetParent(transform, false);
            placa.transform.localPosition = new Vector3(0, 4.3f, 2.2f);
            placa.text = nome.ToUpperInvariant(); placa.fontSize = 48; placa.characterSize = .07f; placa.anchor = TextAnchor.MiddleCenter;
            placa.color = new Color(1f, .95f, .82f);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null) { placa.font = font; placa.GetComponent<MeshRenderer>().sharedMaterial = font.material; }
            placa.gameObject.AddComponent<OlharCamera>();

            // The use point in front (walkable ground); the body and the crown stand for it when clicked.
            var uso = new GameObject("Uso"); uso.transform.position = frente; uso.transform.rotation = Quaternion.LookRotation(transform.position - frente);
            uso.transform.SetParent(transform, true);
            var target = uso.AddComponent<WorldClickTarget>(); target.Configure(nome, 4.5f);
            WorldClickTarget.Alias[corpo] = target; WorldClickTarget.Alias[copaCol] = target;
            WorldClickTarget.Extras.Add(target);
            // Only the tree itself is clickable (not the ground around it); the mouse over it lights it up.
            var pecas = new List<Renderer>();
            foreach (var r in GetComponentsInChildren<Renderer>()) if (!(r is ParticleSystemRenderer) && r.GetComponent<TextMesh>() == null) pecas.Add(r);
            target.Realce = pecas.ToArray();
            BrilhoAoApontar.Criar(target, transform.position + Vector3.up * 2.4f);
            return target;
        }

        sealed class OlharCamera : MonoBehaviour
        {
            void LateUpdate() { var c = Camera.main; if (c != null) transform.rotation = Quaternion.LookRotation(transform.position - c.transform.position); }
        }
    }

    // The light of the roots over the whole screen: it closes when you leave, hides the loading, and opens when you
    // arrive. It also blocks clicks while closed.
    public sealed class VeuDaRaiz : MonoBehaviour
    {
        static VeuDaRaiz atual;
        Image tela; bool abrir, carregou;
        public static void Fechar(System.Action ir)
        {
            if (atual != null) return;
            var go = new GameObject("VeuDaRaiz"); DontDestroyOnLoad(go);
            atual = go.AddComponent<VeuDaRaiz>(); atual.Montar(); atual.StartCoroutine(atual.Rodar(ir));
        }
        public static void Abrir() { if (atual != null) atual.abrir = true; }
        void Montar()
        {
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 32000;
            gameObject.AddComponent<GraphicRaycaster>();
            var img = new GameObject("Luz", typeof(RectTransform)); img.transform.SetParent(transform, false);
            var rt = (RectTransform)img.transform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            tela = img.AddComponent<Image>(); tela.color = new Color(.8f, 1f, .94f, 0); tela.raycastTarget = true;
        }
        void Carregou(Scene s, LoadSceneMode m) => carregou = true;
        IEnumerator Rodar(System.Action ir)
        {
            SceneManager.sceneLoaded += Carregou;
            // Closing: the light grows over everything.
            for (float t = 0; t < .6f; t += Time.unscaledDeltaTime) { tela.color = new Color(.8f, 1f, .94f, Mathf.SmoothStep(0, 1, t / .6f)); yield return null; }
            tela.color = new Color(.8f, 1f, .94f, 1);
            yield return null;
            ir?.Invoke();
            // Loading behind the light (or, if the travel did not happen, open again soon).
            for (float t = 0; !carregou && t < 8f; t += Time.unscaledDeltaTime) yield return null;
            // The new place: wait for the arrival (in front of the root) or a short time.
            for (float t = 0; !abrir && t < 2.5f; t += Time.unscaledDeltaTime) yield return null;
            yield return new WaitForSecondsRealtime(.2f);
            SceneManager.sceneLoaded -= Carregou;
            for (float t = 0; t < .9f; t += Time.unscaledDeltaTime) { tela.color = new Color(.8f, 1f, .94f, 1 - Mathf.SmoothStep(0, 1, t / .9f)); yield return null; }
            atual = null; Destroy(gameObject);
        }
        void OnDestroy() { SceneManager.sceneLoaded -= Carregou; if (atual == this) atual = null; }
    }

    // Lights a Raiz-Mãe up while the mouse is over it: a glow and motes of sap, so you know it can be clicked.
    public sealed class BrilhoAoApontar : MonoBehaviour
    {
        WorldClickTarget alvo; Light luz; ParticleSystem motes; DesktopRpgControls controls; float forca;
        public static void Criar(WorldClickTarget alvo, Vector3 onde)
        {
            var go = new GameObject("BrilhoAoApontar"); go.transform.position = onde; go.transform.SetParent(alvo.transform, true);
            var b = go.AddComponent<BrilhoAoApontar>(); b.alvo = alvo;
            b.luz = go.AddComponent<Light>(); b.luz.type = LightType.Point; b.luz.color = new Color(1f, .88f, .55f); b.luz.range = 9; b.luz.intensity = 0; b.luz.shadows = LightShadows.None;
            b.motes = go.AddComponent<ParticleSystem>();
            var main = b.motes.main; main.startLifetime = 1.4f; main.startSpeed = .9f; main.startSize = new ParticleSystem.MinMaxCurve(.07f, .16f);
            main.startColor = new Color(1f, .9f, .6f, .9f); main.maxParticles = 60; main.simulationSpace = ParticleSystemSimulationSpace.World; main.gravityModifier = -.05f;
            var em = b.motes.emission; em.rateOverTime = 0;
            var shape = b.motes.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 2.4f;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = EfeitosDeHabilidade.Brilho;
        }
        void Update()
        {
            if (controls == null) controls = FindFirstObjectByType<DesktopRpgControls>();
            bool aponta = controls != null && controls.HoveredInteraction == alvo;
            forca = Mathf.MoveTowards(forca, aponta ? 1 : 0, Time.unscaledDeltaTime * 5);
            luz.intensity = forca * (2.4f + Mathf.Sin(Time.time * 6) * .5f);
            var em = motes.emission; em.rateOverTime = forca * 28;
        }
    }
}
