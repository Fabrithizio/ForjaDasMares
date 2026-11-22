using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // The start of the story (see claude/historia-biblia.md, "Começo do jogo — ROTEIRO NOVO"):
    // 1. Opening: slides over the world map art.
    // 2. Home village at dusk: walk freely, help your uncle, the dead station rings at night and pulls you away.
    // 3. Arrival inside the roots of the Nó do Refúgio: Iria and Nara find you. No orders.
    // 4. Nara talks when you look for her. 5. Téo lets you choose sword or staff.
    // 6. Touch the Nó: the root records you (you come back here when you fall).
    // Progress is saved per character, so each step happens once.
    public static class Historia
    {
        public const string Vila = "RefugioAntigo", Refugio = "RefugioUltimoSino";
        static readonly string[] Passos = { "prologo", "chegada", "nara", "teo", "no", "m1_aceita", "m1_feita", "m2_aceita", "m2_feita", "m3_aceita", "m3_feita", "m4_aceita", "m4_chegou", "m4_fonte", "m5_aceita", "m5_e0", "m5_e1", "m5_e2", "m5_feita",
            "hab_brotar", "hab_brasa", "hab_raizes", "hab_rajada", "hab_bolha", "hab_marca", "hab_gelo" };
        internal static string Who => string.IsNullOrEmpty(CampaignHome.CharacterName) ? "padrao" : CampaignHome.CharacterName;
        static string Key(string step) => "ForjaHistoria_" + Who + "_" + step;
        public static bool Feito(string step) => PlayerPrefs.GetInt(Key(step), 0) == 1;
        public static void Marcar(string step) { PlayerPrefs.SetInt(Key(step), 1); PlayerPrefs.Save(); }
        public static void Recomecar()
        {
            foreach (var s in Passos) PlayerPrefs.DeleteKey(Key(s));
            PlayerPrefs.DeleteKey("ForjaNo_" + Who);
            Missoes.Limpar();
            PlayerPrefs.Save();
        }
        // First scene when entering the game: the village until the prologue is done, then the Refúgio.
        public static string PrimeiraCena => Feito("prologo") ? Refugio : Vila;

        // ---- The Nó (root portal) that recorded you: where the web pulls you back when you fall ----
        public static string CenaDoNo => PlayerPrefs.GetString("ForjaNo_" + Who, "");
        public static bool RenascerNoNo;
        public static void GravarNo(string scene)
        {
            PlayerPrefs.SetString("ForjaNo_" + Who, scene);
            if (scene == Refugio) PlayerPrefs.SetInt(Key("no"), 1);
            PlayerPrefs.Save();
        }

        // What the "Acompanhamento" panel shows while the story is guiding the player (null = nothing from the story).
        public static string Objetivo
        {
            get
            {
                string scene = SceneManager.GetActiveScene().name;
                if (scene == Vila && !Feito("prologo")) return "Sua vila. Fim de tarde.";
                if (!Feito("prologo") || !Feito("chegada")) return null;
                if (scene == Refugio)
                {
                    if (!Feito("nara")) return "Você acordou num lugar desconhecido.";
                    if (!Feito("teo")) return "A Nara mandou você procurar o Téo, na forja.";
                    if (!Feito("no")) return "Antes de sair, toque a Raiz-Mãe do Refúgio (a árvore de raízes, ao lado das Expedições).";
                }
                if (!Feito("teo")) return null;
                // Chapter 1: the missions followed in the journal (up to 3), plus who has a request for you.
                return Missoes.Acompanhamento;
            }
        }
        // Skills open once the player has a weapon from Téo.
        public static bool TemArmaInicial => Feito("teo");
        public const int ErvasPedidas = 5, CarnesPedidas = 3, CourosPedidos = 3;
        // Iria taught the healing potion (mission 1).
        public static bool SabePocao => Feito("m1_feita");

        // Bell made of sound waves (no audio file needed). Cracked = buzzing, slightly out of tune.
        static AudioClip bell, crackedBell;
        public static AudioClip Sino(bool cracked)
        {
            if (cracked && crackedBell != null) return crackedBell;
            if (!cracked && bell != null) return bell;
            const int rate = 44100; float length = 3.2f; int n = (int)(rate * length);
            var data = new float[n]; float[] partials = { 1f, 2.01f, 2.42f, 3.0f, 4.18f, 5.4f }; float[] gains = { 1f, .55f, .4f, .3f, .18f, .1f };
            var random = new System.Random(7); float baseHz = cracked ? 196 : 233;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate, v = 0;
                for (int k = 0; k < partials.Length; k++) v += gains[k] * Mathf.Sin(2 * Mathf.PI * baseHz * partials[k] * t) * Mathf.Exp(-t * (1.1f + k * .55f));
                if (cracked) v += .35f * Mathf.Sin(2 * Mathf.PI * baseHz * 1.07f * t) * Mathf.Exp(-t * 1.6f) + (float)(random.NextDouble() - .5) * .05f * Mathf.Exp(-t * 3);
                data[i] = v * .22f * Mathf.Min(1, t * 200);
            }
            var clip = AudioClip.Create(cracked ? "SinoRachado" : "Sino", n, 1, rate, false); clip.SetData(data, 0);
            if (cracked) crackedBell = clip; else bell = clip;
            return clip;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded;
            Loaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        static void Loaded(Scene scene, LoadSceneMode mode)
        {
            bool vila = scene.name == Vila && !Feito("prologo");
            // In the Refúgio the story director stays on: start of the game and the Chapter 1 missions.
            bool chegada = scene.name == Refugio && Feito("prologo");
            // Mission 4: arriving at the Nascente do Rio.
            if (scene.name.Contains("Nascente")) Descobertas.Marcar(Descobertas.Nascente);
            if (scene.name.Contains("Nascente") && Feito("m4_aceita") && !Feito("m4_chegou"))
            {
                Marcar("m4_chegou");
                AvisoNaTela.Mostrar("...Esse rio. Parece o rio lá de casa.", 5);
            }
            if (!vila && !chegada) return;
            if (UnityEngine.Object.FindFirstObjectByType<CenaDaHistoria>() != null) return;
            new GameObject("Historia").AddComponent<CenaDaHistoria>().Begin(vila);
        }
    }

    public sealed class CenaDaHistoria : MonoBehaviour
    {
        RegionalWorld world;
        Canvas canvas; RectTransform root;
        CanvasGroup curtain; Image curtainImage;
        GameObject box; Text speaker, line, next; bool waitingClick; bool skipTyping;
        Text banner; CanvasGroup bannerGroup;
        LineRenderer marker; Transform markerTarget;
        AudioSource sound;
        readonly List<Canvas> hud = new List<Canvas>();
        readonly List<Renderer> hiddenPlates = new List<Renderer>();
        bool talking;
        string Me => string.IsNullOrEmpty(CampaignHome.CharacterName) ? "Você" : CampaignHome.CharacterName;

        public void Begin(bool vila) => StartCoroutine(vila ? Vila() : Refugio());

        void OnDestroy()
        {
            if (WorldClickTarget.Intercept == (Func<WorldClickTarget, Transform, bool>)Intercept) WorldClickTarget.Intercept = null;
        }

        // ---------- shared pieces ----------
        IEnumerator Setup()
        {
            for (int i = 0; i < 60 && (world = FindFirstObjectByType<RegionalWorld>()) == null; i++) yield return null;
            while (world != null && world.Hero == null) yield return null;
            yield return null; yield return null;
            var go = new GameObject("HistoriaUI", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            canvas = go.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 31000;
            var scaler = go.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            go.AddComponent<GraphicRaycaster>();
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null) new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            root = (RectTransform)go.transform;
            var c = Fill(root, "Cortina", Color.black); curtainImage = c.GetComponent<Image>(); curtain = c.gameObject.AddComponent<CanvasGroup>(); curtain.alpha = 1; curtain.blocksRaycasts = false;

            // Short notice (top centre), used only for moments like "later that night".
            var b = RusticHudTheme.Rect(root, "Objetivo", new Vector2(.5f, 1), new Vector2(0, -190), new Vector2(900, 64));
            RusticHudTheme.Panel(b, new Color(.03f, .06f, .07f, .88f), 14);
            banner = RusticHudTheme.Label(b, "Texto", new Vector2(.5f, .5f), Vector2.zero, new Vector2(860, 56), "", 24, TextAnchor.MiddleCenter); banner.color = RusticHudTheme.Gold;
            bannerGroup = b.gameObject.AddComponent<CanvasGroup>(); bannerGroup.alpha = 0; bannerGroup.blocksRaycasts = false;

            // Dialogue box (bottom).
            var d = RusticHudTheme.Rect(root, "Fala", new Vector2(.5f, 0), new Vector2(0, 150), new Vector2(1240, 230));
            RusticHudTheme.Panel(d, new Color(.03f, .055f, .07f, .95f), 18);
            // Phone: a tap anywhere on the screen advances the talk, not only on the box.
            var anywhere = RusticHudTheme.Rect(d, "ToqueNaTela", new Vector2(.5f, .5f), Vector2.zero, new Vector2(6000, 6000));
            anywhere.SetAsFirstSibling();
            var anywhereImage = anywhere.gameObject.AddComponent<Image>(); anywhereImage.color = new Color(0, 0, 0, 0);
            speaker = RusticHudTheme.Label(d, "Quem", new Vector2(0, 1), new Vector2(250, -34), new Vector2(460, 40), "", 28); speaker.color = RusticHudTheme.Gold;
            line = RusticHudTheme.Label(d, "Texto", new Vector2(.5f, .5f), new Vector2(0, -14), new Vector2(1160, 130), "", 26, TextAnchor.UpperLeft);
            line.horizontalOverflow = HorizontalWrapMode.Wrap; line.verticalOverflow = VerticalWrapMode.Overflow;
            next = RusticHudTheme.Label(d, "Seguir", new Vector2(1, 0), new Vector2(-150, 26), new Vector2(280, 30), "toque para continuar ▸", 18, TextAnchor.MiddleRight); next.color = RusticHudTheme.Muted;
            var click = d.gameObject.AddComponent<Button>(); click.transition = Selectable.Transition.None;
            click.targetGraphic = d.GetComponent<Graphic>(); // the panel already has its own graphic (RoundedHudGraphic)
            click.onClick.AddListener(Advance);
            box = d.gameObject; box.SetActive(false);

            sound = gameObject.AddComponent<AudioSource>(); sound.playOnAwake = false; sound.spatialBlend = 0;
            foreach (var h in FindObjectsByType<RegionalWorldHud>(FindObjectsSortMode.None)) hud.AddRange(h.GetComponentsInChildren<Canvas>(true));
        }

        void Advance() { if (waitingClick) waitingClick = false; else skipTyping = true; }
        bool Pressed => (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame));

        void Hud(bool visible) { foreach (var c in hud) if (c != null) c.enabled = visible; }
        void Control(bool on) { var input = world.Hero.GetComponent<ArenaInput>(); if (input != null) { if (!on) input.Desktop?.Cancel(); input.BlockCommands(!on); } if (!on) world.Hero.GetComponent<PlayerMotor>()?.StopWalking(); }

        IEnumerator Fade(float to, float seconds, Color? color = null)
        {
            if (color.HasValue) curtainImage.color = color.Value;
            float from = curtain.alpha;
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime) { curtain.alpha = Mathf.Lerp(from, to, t / seconds); yield return null; }
            curtain.alpha = to;
        }

        // Names floating over people are hidden while someone talks (they showed through the box).
        void Plates(bool visible)
        {
            if (!visible) { foreach (var r in hiddenPlates) if (r != null) r.enabled = true; hiddenPlates.Clear(); return; }
            foreach (var text in FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
            {
                var r = text.GetComponent<Renderer>();
                if (r != null && r.enabled) { r.enabled = false; hiddenPlates.Add(r); }
            }
        }

        IEnumerator Say(string who, string text)
        {
            if (!box.activeSelf) Plates(true);
            box.SetActive(true); box.transform.SetAsLastSibling(); speaker.text = who; line.text = ""; next.enabled = false;
            skipTyping = false;
            yield return null; // the key or tap that opened the talk must not skip the first line
            for (int i = 1; i <= text.Length; i++)
            {
                if (skipTyping || Pressed) { i = text.Length; skipTyping = false; }
                line.text = text.Substring(0, i);
                yield return new WaitForSecondsRealtime(.026f);
            }
            line.text = text; next.enabled = true; waitingClick = true;
            yield return null;
            while (waitingClick && !Pressed) yield return null;
            waitingClick = false;
            yield return null;
        }
        void CloseTalk() { box.SetActive(false); Plates(false); }

        IEnumerator Notice(string text, float seconds)
        {
            banner.text = text;
            for (float t = 0; t < .5f; t += Time.unscaledDeltaTime) { bannerGroup.alpha = t / .5f; yield return null; }
            bannerGroup.alpha = 1;
            yield return new WaitForSecondsRealtime(seconds);
            for (float t = 0; t < .5f; t += Time.unscaledDeltaTime) { bannerGroup.alpha = 1 - t / .5f; yield return null; }
            bannerGroup.alpha = 0;
        }

        // Glowing ring on the ground at a target, until the hero gets there. No text: the light is the only hint.
        IEnumerator GoTo(Transform target, float distance)
        {
            if (target == null) yield break;
            markerTarget = target;
            if (marker == null)
            {
                var go = new GameObject("MarcaDaHistoria"); marker = go.AddComponent<LineRenderer>();
                marker.loop = true; marker.positionCount = 40; marker.useWorldSpace = false; marker.widthMultiplier = .12f;
                var m = new Material(Shader.Find("Universal Render Pipeline/Unlit")); m.SetColor("_BaseColor", new Color(1f, .82f, .4f)); marker.sharedMaterial = m;
                marker.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            marker.gameObject.SetActive(true);
            while (true)
            {
                if (markerTarget == null) break;
                float pulse = 1.3f + Mathf.Sin(Time.time * 4) * .15f;
                for (int i = 0; i < 40; i++) { float a = i * Mathf.PI * 2 / 40; marker.SetPosition(i, new Vector3(Mathf.Cos(a) * pulse, .08f, Mathf.Sin(a) * pulse)); }
                marker.transform.position = markerTarget.position;
                Vector3 d = world.Hero.transform.position - markerTarget.position; d.y = 0;
                if (d.magnitude < distance) break;
                yield return null;
            }
            marker.gameObject.SetActive(false);
        }

        void Bell(float volume, bool cracked) => sound.PlayOneShot(Historia.Sino(cracked), volume);

        Transform PersonNamed(string part)
        {
            foreach (var text in FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
                if (text.text != null && text.text.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0)
                { var resident = text.GetComponentInParent<TownResident>(); return resident != null ? resident.transform : text.transform.parent; }
            return null;
        }
        static WorldClickTarget TargetOf(Transform person)
        {
            if (person == null) return null;
            var target = person.GetComponentInChildren<WorldClickTarget>();
            if (target == null) target = person.GetComponentInParent<WorldClickTarget>();
            return target;
        }

        static RectTransform Fill(Transform parent, string name, Color color)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            var image = r.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false; return r;
        }

        static Vector3 OnGround(Vector3 p)
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(p, out var hit, 4, UnityEngine.AI.NavMesh.AllAreas)) return hit.position;
            return p;
        }
        static void Place(Transform who, Vector3 at, Vector3 look)
        {
            if (who == null) return;
            var agent = who.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh) agent.Warp(at); else who.position = at;
            look.y = at.y; if ((look - at).sqrMagnitude > .01f) who.rotation = Quaternion.LookRotation(look - at);
        }

        // ---------- 1 + 2: opening and the home village ----------
        IEnumerator Vila()
        {
            yield return Setup();
            if (world == null) yield break;
            Hud(false); Control(false);
            yield return Abertura();

            // Late afternoon. The village sits on sleeping land: tired, greyish light.
            var sun = new List<Light>();
            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None)) if (light.type == LightType.Directional) sun.Add(light);
            foreach (var light in sun) { light.intensity *= .7f; light.color = new Color(1f, .72f, .5f); }
            RenderSettings.ambientLight = new Color(.42f, .36f, .33f);
            // Only your uncle is outside; the rest of the village is already closed.
            Transform uncle = PersonNamed("Bento");
            foreach (var resident in FindObjectsByType<TownResident>(FindObjectsSortMode.None)) if (resident.transform != uncle) resident.gameObject.SetActive(false);
            foreach (var target in FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None)) target.enabled = false;
            if (uncle != null)
            {
                // Keep only one nameplate (the model had more than one TextMesh: the name showed several times).
                bool first = true;
                foreach (var plate in uncle.GetComponentsInChildren<TextMesh>(true))
                {
                    if (first) { plate.text = "Seu tio"; first = false; }
                    else plate.gameObject.SetActive(false);
                }
                var walk = uncle.GetComponent<TownResident>(); if (walk != null) walk.enabled = false;
            }
            var station = GameObject.Find("ArrivalTransportPlate")?.transform ?? GameObject.Find("AncientMemorySite")?.transform;

            yield return Fade(0, 1.6f);
            Hud(true); Control(true);
            // No orders: you can walk around. Only a soft light on the ground shows where your uncle is.
            if (uncle != null) yield return GoTo(uncle, 2.4f);
            Hud(false); Control(false);
            if (uncle != null) { var face = world.Hero.transform.position - uncle.position; face.y = 0; if (face.sqrMagnitude > .01f) uncle.rotation = Quaternion.LookRotation(face); }
            yield return Say("Seu tio", "Chegou na hora. Segura esse lado aqui.");
            CloseTalk();
            yield return Fade(1, .5f);
            yield return new WaitForSecondsRealtime(.7f);
            yield return Fade(0, .5f);
            yield return Say("Seu tio", "A colheita desse ano mal encheu três caixotes. A terra anda cansada.");
            yield return Say(Me, "Ano que vem melhora.");
            yield return Say("Seu tio", "(sorri, meio triste) Seu avô dizia a mesma coisa.");
            CloseTalk();
            Bell(.25f, false);
            yield return new WaitForSecondsRealtime(1.6f);
            yield return Say("Seu tio", "...Ouviu isso?");
            yield return Say("Seu tio", "Deve ser o vento batendo na estação velha. Vai dormir, que amanhã tem feira.");
            CloseTalk();

            // Night falls.
            yield return Fade(1, 1.2f);
            if (uncle != null) uncle.gameObject.SetActive(false);
            foreach (var light in sun) { light.intensity *= .35f; light.color = new Color(.55f, .63f, .95f); }
            RenderSettings.ambientLight = new Color(.16f, .19f, .3f);
            RenderSettings.fogColor = new Color(.08f, .1f, .18f);
            StartCoroutine(Notice("Mais tarde, naquela noite...", 2.2f));
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Fade(0, 1.4f);
            yield return new WaitForSecondsRealtime(1.5f);
            Bell(.75f, false);
            // The old station glows in the distance. Nothing tells you to go: it is your choice.
            Vector3 at = station != null ? station.position : world.Hero.transform.position;
            var lamp = new GameObject("LuzDaEstacao").AddComponent<Light>(); lamp.type = LightType.Point; lamp.color = new Color(.5f, 1, .9f); lamp.range = 16; lamp.intensity = 2.5f; lamp.transform.position = at + Vector3.up * 2;
            Hud(true); Control(true);
            if (station != null) yield return GoTo(station, 3f);
            Hud(false); Control(false);

            // The dead station wakes up: the roots light up, the bell three times, and you are pulled away.
            var follow = FindFirstObjectByType<ArenaCamera>(); follow?.SetZoom(.6f);
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder); Destroy(pillar.GetComponent<Collider>());
            pillar.transform.position = at + Vector3.up * 6; pillar.transform.localScale = new Vector3(.2f, 6, .2f);
            var glow = new Material(Shader.Find("Universal Render Pipeline/Unlit")); glow.SetColor("_BaseColor", new Color(.55f, 1f, .9f)); pillar.GetComponent<Renderer>().sharedMaterial = glow;
            for (int ring = 0; ring < 3; ring++)
            {
                Bell(.7f + ring * .15f, false);
                for (float t = 0; t < 1.3f; t += Time.deltaTime)
                {
                    float k = (ring + t / 1.3f) / 3f;
                    pillar.transform.localScale = new Vector3(.2f + k * 2.6f, 6, .2f + k * 2.6f); lamp.intensity = 2.5f + k * 7;
                    yield return null;
                }
            }
            var body = world.Hero.GetComponentInChildren<Animator>();
            if (body != null && body.HasState(0, Animator.StringToHash("Pular"))) body.CrossFadeInFixedTime("Pular", .1f, 0);
            StartCoroutine(Fade(1, 1.6f, Color.white));
            for (float t = 0; t < 1.6f; t += Time.deltaTime) { pillar.transform.localScale += Vector3.one * Time.deltaTime * .6f; lamp.intensity += Time.deltaTime * 6; yield return null; }
            yield return new WaitForSecondsRealtime(.4f);
            var text = RusticHudTheme.Label(root, "Puxado", new Vector2(.5f, .5f), Vector2.zero, new Vector2(1400, 80), "O sino de uma estação morta tocou... e levou você.", 34, TextAnchor.MiddleCenter);
            text.color = new Color(.1f, .14f, .16f);
            yield return new WaitForSecondsRealtime(2.6f);
            Historia.Marcar("prologo");
            SceneManager.LoadSceneAsync(Historia.Refugio);
        }

        IEnumerator Abertura()
        {
            var art = Resources.Load<Texture2D>("Mapa/MapaMundi");
            var frame = Fill(root, "Abertura", Color.black);
            var pic = new GameObject("Arte", typeof(RectTransform)).GetComponent<RectTransform>(); pic.SetParent(frame, false);
            pic.anchorMin = pic.anchorMax = pic.pivot = new Vector2(.5f, .5f);
            var image = pic.gameObject.AddComponent<RawImage>(); image.texture = art; image.raycastTarget = false;
            Fill(frame, "Sombra", new Color(0, 0, 0, .45f));
            var caption = RusticHudTheme.Label(frame, "Texto", new Vector2(.5f, 0), new Vector2(0, 150), new Vector2(1400, 160), "", 34, TextAnchor.MiddleCenter);
            caption.horizontalOverflow = HorizontalWrapMode.Wrap; caption.verticalOverflow = VerticalWrapMode.Overflow;
            caption.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0, 0, .9f);
            var tap = frame.gameObject.AddComponent<Button>(); tap.transition = Selectable.Transition.None; tap.targetGraphic = frame.GetComponent<Image>(); frame.GetComponent<Image>().raycastTarget = true;
            tap.onClick.AddListener(() => skipSlide = true);
            RusticHudTheme.Button(frame, "Pular", new Vector2(1, 1), new Vector2(-150, -70), new Vector2(240, 76), "PULAR ▸", () => skipAll = true);
            frame.SetAsLastSibling();
            // Each slide: a part of the world map (centre in percent of the art, zoom) and its sentence.
            var slides = new (Vector2 focus, float zoom, string text)[]
            {
                (new Vector2(50, 50), 1.05f, "Debaixo do mundo corre uma teia viva.\nEla liga cada árvore, cada bicho, cada pedaço de terra."),
                (new Vector2(52, 38), 1.9f, "Há muito tempo, um povo aprendeu a viajar por dentro dela.\nConstruiu estações onde máquinas puxavam a força das raízes. Um sino avisava cada passagem."),
                (new Vector2(52, 8), 2.4f, "Um dia, todos os sinos se calaram.\nAs estações viraram ruínas, e ninguém lembra mais por quê."),
                (new Vector2(52, 44), 2.8f, "Todos... menos um.\nNum lugar distante, uma estação rachada ainda puxa gente de longe. Ela só traz. Nunca leva de volta."),
            };
            yield return Fade(0, .8f);
            foreach (var (focus, zoom, sentence) in slides)
            {
                if (skipAll) break;
                skipSlide = false; caption.text = sentence;
                float seconds = 7f;
                for (float t = 0; t < seconds && !skipSlide && !skipAll && !Pressed; t += Time.unscaledDeltaTime)
                {
                    float k = t / seconds, z = zoom * (1 + k * .08f);
                    var size = root.rect.size; float h = Mathf.Max(size.y, size.x * 9 / 16) * z;
                    pic.sizeDelta = new Vector2(h * 16 / 9, h);
                    pic.anchoredPosition = new Vector2((.5f - focus.x / 100) * pic.sizeDelta.x, (focus.y / 100 - .5f) * pic.sizeDelta.y);
                    caption.color = new Color(1, .96f, .86f, Mathf.Clamp01(t * 2) * Mathf.Clamp01((seconds - t) * 2));
                    yield return null;
                }
                yield return null;
            }
            yield return Fade(1, .7f, Color.black);
            Destroy(frame.gameObject);
        }
        bool skipAll, skipSlide;

        // ---------- 3 to 5: the Refúgio ----------
        Transform nara, iria, teo;
        IEnumerator Refugio()
        {
            yield return Setup();
            if (world == null) yield break;
            for (int i = 0; i < 10 && world.No == null; i++) yield return null;
            nara = PersonNamed("Nara"); iria = PersonNamed("Iria"); teo = PersonNamed("Téo"); if (teo == null) teo = PersonNamed("Teo");
            if (!Historia.Feito("chegada")) yield return Chegada();
            else yield return Fade(0, .6f);
            // From here on the player is free. Nara and Téo answer when the player looks for them.
            WorldClickTarget.Intercept = Intercept;
            Hud(true); Control(true);
            bras = PersonNamed("Brás"); if (bras == null) bras = PersonNamed("Bras");
            // "!" / "?" over the heads of whoever has a mission for you.
            MarcadorDeMissao.Colocar(nara, "Nara"); MarcadorDeMissao.Colocar(iria, "Iria"); MarcadorDeMissao.Colocar(teo, "Téo"); MarcadorDeMissao.Colocar(bras, "Brás");
            // Brás was only scenery: give him a way to talk (click or F).
            if (bras != null && TargetOf(bras) == null)
            {
                var talk = new GameObject("Conversar"); talk.transform.SetParent(bras, false);
                var box = talk.AddComponent<BoxCollider>(); box.center = Vector3.up; box.size = new Vector3(1, 2, 1);
                var t = talk.AddComponent<WorldClickTarget>(); t.Configure("Conversar · Brás · Feirante", 2.6f);
                WorldClickTarget.Extras.Add(t);
            }
            // Stays on for the Chapter 1 missions (the talks take over only when a mission needs them).
        }
        Transform bras;

        // Talking to Nara, Iria or Téo during the story opens the story talk instead of their usual menu.
        bool Intercept(WorldClickTarget target, Transform actor)
        {
            if (world == null || actor != world.Hero.transform) return false;
            if (talking) return true;
            bool naraDone = Historia.Feito("nara");
            if (target == TargetOf(nara) && !naraDone) { StartCoroutine(TalkNara()); return true; }
            if (target == TargetOf(iria) && !naraDone) { StartCoroutine(Short("Iria", "Já de pé? Ótimo! Anda, anda, a Nara quer falar com você. Ela tá ali, ó!")); return true; }
            if (target == TargetOf(teo) && !Historia.Feito("teo"))
            {
                if (naraDone) StartCoroutine(TalkTeo());
                else StartCoroutine(Short("Téo", "Hm. Chegado novo. Vai falar com a Nara primeiro. Depois volta aqui."));
                return true;
            }
            return Missao(target);
        }

        // ---------- Chapter 1: first missions (Iria → Brás → Téo → Nara → Nascente do Rio) ----------
        bool Missao(WorldClickTarget target)
        {
            if (target == TargetOf(bras) && !Historia.Feito("teo")) { StartCoroutine(Short("Brás", "Chegado novo? Brás, feirante. Vai falar com a Nara primeiro, depois passa aqui.")); return true; }
            if (!Historia.Feito("teo")) return false;
            // Something the player saw out there: whoever cares comments first (1 or 2 lines), then the usual talk.
            string quem = target == TargetOf(iria) ? "Iria" : target == TargetOf(bras) ? "Brás" : target == TargetOf(teo) ? "Téo" : target == TargetOf(nara) ? "Nara" : null;
            var comentario = quem != null ? Descobertas.Proximo(quem) : null;
            if (comentario != null) { StartCoroutine(Comentar(quem, comentario, target)); return true; }
            // The side missions can all be taken at once; Nara's (the main one) opens after the first hunt by the river.
            if (target == TargetOf(iria))
            {
                if (!Historia.Feito("m1_aceita")) { StartCoroutine(IriaPede()); return true; }
                if (!Historia.Feito("m1_feita"))
                {
                    if (Recursos.Quantos(Recursos.ErvaLuz) >= Historia.ErvasPedidas) StartCoroutine(IriaRecebe());
                    else StartCoroutine(Short("Iria", "Ainda não? Tudo bem, tudo bem! São aquelas plantinhas que brilham no campo, fora da muralha. Cinco! Preciso de cinco."));
                    return true;
                }
                StartCoroutine(IriaAlquimia()); return true;
            }
            if (target == TargetOf(bras))
            {
                if (!Historia.Feito("m2_aceita")) StartCoroutine(BrasPede());
                else if (!Historia.Feito("m2_feita"))
                {
                    if (Recursos.Quantos(Recursos.Carne) >= Historia.CarnesPedidas) StartCoroutine(BrasRecebe());
                    else StartCoroutine(Short("Brás", "Três peças de carne, amigo. Perto do rio. E olho aberto, que aqueles bichos não estão bons da cabeça."));
                }
                else StartCoroutine(Short("Brás", "Obrigado pela carne, amigo! A cidade agradece."));
                return true;
            }
            if (target == TargetOf(teo) && !Historia.Feito("m3_feita"))
            {
                if (!Historia.Feito("m3_aceita")) StartCoroutine(TeoPede());
                else if (Recursos.Quantos(Recursos.Couro) >= Historia.CourosPedidos) StartCoroutine(TeoFazManto());
                else StartCoroutine(Short("Téo", "Três couros. Dos bichos do rio."));
                return true;
            }
            if (target == TargetOf(nara))
            {
                if (!Historia.Feito("m2_feita")) StartCoroutine(Short("Nara", "Ainda se ajeitando? Ajude o pessoal da cidade. Quando eu precisar de você, eu chamo."));
                else if (!Historia.Feito("m4_aceita")) StartCoroutine(NaraNascente());
                else if (Historia.Feito("m4_fonte") && !Habilidades.Aprendeu("rajada")) StartCoroutine(NaraRajada());
                else if (Habilidades.Aprendeu("rajada")) StartCoroutine(Short("Nara", "O rio já clareou um pouco. Bom trabalho. Descansa."));
                else if (Historia.Feito("m4_chegou")) StartCoroutine(Short("Nara", "O rio ainda escurece. Tem alguma coisa lá no Poço da Nascente. Descubra o quê."));
                else StartCoroutine(Short("Nara", "A Nascente fica ao sul, rio acima. Só olhe. Volte inteiro."));
                return true;
            }
            return false;
        }

        IEnumerator Comentar(string quem, string[] falas, WorldClickTarget target)
        {
            yield return StartTalk(quem == "Téo" ? teo : quem == "Brás" ? bras : quem == "Nara" ? nara : iria);
            foreach (var fala in falas) yield return Say(quem, fala);
            End();
            if (!Missao(target)) target.Used.Invoke();
        }

        IEnumerator NaraRajada()
        {
            yield return StartTalk(nara);
            yield return Say("Nara", "Voltou inteiro da Nascente. Bom.");
            yield return Say("Nara", "Um capitão afogado? Lá na nascente? ...O Breu está levantando até os mortos do rio.");
            yield return Say("Nara", "Isso não é coisa pequena. Vou anotar no mapa. Com tinta forte.");
            yield return Say("Nara", "Quem anda por lá precisa sair rápido de onde não devia estar. Eu aprendi com o vento. Olha.");
            yield return Say("Nara", "(um sopro forte levanta a poeira em volta dela) Um passo, e o vento leva o resto.");
            Habilidades.Aprender("rajada");
            End();
        }

        IEnumerator TeoPede()
        {
            yield return StartTalk(teo);
            yield return Say("Téo", "Hm. Você de novo.");
            yield return Say("Téo", "Os bichos do rio, os tomados pelo Breu. O couro deles fica duro feito casca. Serve pra peça boa.");
            yield return Say("Téo", "Traz três couros. Eu faço uma coisa pra você vestir.");
            Historia.Marcar("m3_aceita");
            End();
        }

        IEnumerator IriaPede()
        {
            yield return StartTalk(iria);
            yield return Say("Iria", "Ei! Você! Tá ocupado? Não tá. Ótimo!");
            yield return Say("Iria", "Acabou a erva-luz. Toda. Quem vai sair machucado lá fora precisa de poção, e eu sem erva!");
            yield return Say(Me, "Erva-luz?");
            yield return Say("Iria", "Não conhece? De onde você veio não tem? Como vocês curam as coisas lá?! ...Depois você me conta.");
            yield return Say("Iria", "São plantinhas que brilham, no campo, logo fora da muralha. Traz cinco pra mim. Cinco!");
            Historia.Marcar("m1_aceita");
            End();
        }
        IEnumerator IriaRecebe()
        {
            yield return StartTalk(iria);
            var s = Recursos.Servico;
            if (s == null || !s.Trocar(new[] { (Recursos.ErvaLuz, Historia.ErvasPedidas) }, new[] { (Recursos.PocaoCura, 2) }, "Ervas entregues. +2 Poção de cura."))
            { yield return Say("Iria", "Hm? Cadê as ervas? Achei que tinha visto... deixa pra lá."); End(); yield break; }
            yield return Say("Iria", "Isso! Isso! Olha que bonitas. Toma, duas poções, pela ajuda. Fiz agora há pouco.");
            yield return Say("Iria", "Quer saber? Eu te ensino. Não é difícil: três ervas, água da fonte, e pronto. Vem, vem, olha aqui.");
            Historia.Marcar("m1_feita");
            CloseTalk();
            yield return Alquimia();
            yield return Say("Iria", "Viu? Agora você mesmo faz. Precisando, é só voltar aqui na fonte.");
            yield return Say("Iria", "Ah! E mais uma coisa. Quando apertar lá fora, faz assim: mão no chão, pensa em coisa viva...");
            yield return Say("Iria", "(uma luz verde brota entre os dedos dela) Pronto. Cura. Não muito, mas cura. Treina!");
            Habilidades.Aprender("brotar");
            End();
        }
        IEnumerator IriaAlquimia()
        {
            yield return StartTalk(iria);
            yield return Alquimia();
            End();
        }

        // Making potions at Iria's spring: 3 Erva-luz = 1 Poção de cura.
        IEnumerator Alquimia()
        {
            bool open = true;
            var panel = RusticHudTheme.Rect(root, "Alquimia", new Vector2(.5f, .5f), Vector2.zero, new Vector2(760, 380));
            RusticHudTheme.Panel(panel, new Color(.03f, .055f, .07f, .97f), 22);
            RusticHudTheme.Label(panel, "Titulo", new Vector2(.5f, 1), new Vector2(0, -44), new Vector2(700, 46), "FONTE DA IRIA · POÇÕES", 28, TextAnchor.MiddleCenter).color = RusticHudTheme.Gold;
            RusticHudTheme.Label(panel, "Receita", new Vector2(.5f, 1), new Vector2(0, -110), new Vector2(700, 40), "Poção de cura  =  3 Erva-luz", 24, TextAnchor.MiddleCenter);
            var info = RusticHudTheme.Label(panel, "Tem", new Vector2(.5f, 1), new Vector2(0, -158), new Vector2(700, 36), "", 19, TextAnchor.MiddleCenter);
            info.color = RusticHudTheme.Muted;
            void Fazer(int vezes)
            {
                var s = Recursos.Servico; if (s == null) return;
                int can = Mathf.Min(vezes, Recursos.Quantos(Recursos.ErvaLuz) / 3);
                if (can <= 0) return;
                if (s.Trocar(new[] { (Recursos.ErvaLuz, can * 3) }, new[] { (Recursos.PocaoCura, can) }, "+" + can + " Poção de cura."))
                    EfeitosDeHabilidade.Coluna(world.Hero.transform.position, .8f, new Color(1f, .4f, .45f));
            }
            var one = RusticHudTheme.Button(panel, "Fazer1", new Vector2(.5f, 0), new Vector2(-190, 110), new Vector2(330, 64), "FAZER 1", () => Fazer(1));
            var all = RusticHudTheme.Button(panel, "FazerTudo", new Vector2(.5f, 0), new Vector2(190, 110), new Vector2(330, 64), "FAZER TODAS", () => Fazer(99));
            RusticHudTheme.Button(panel, "Fechar", new Vector2(.5f, 0), new Vector2(0, 30), new Vector2(240, 54), "FECHAR", () => open = false);
            panel.SetAsLastSibling();
            while (open)
            {
                int ervas = Recursos.Quantos(Recursos.ErvaLuz);
                info.text = "Você tem: " + ervas + " Erva-luz · " + Recursos.Quantos(Recursos.PocaoCura) + " Poção de cura";
                one.interactable = all.interactable = ervas >= 3;
                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) open = false;
                yield return null;
            }
            Destroy(panel.gameObject);
        }

        IEnumerator BrasPede()
        {
            yield return StartTalk(bras);
            yield return Say("Brás", "Chegado novo? Dá pra ver pela cara. Brás, ao seu dispor. Compro, vendo, troco e, de graça, conto as novidades.");
            yield return Say("Brás", "E a novidade é ruim: os bichos lá perto do rio ficaram esquisitos. Olho roxo, brilhando. Atacam quem passa.");
            yield return Say("Brás", "Ninguém mais caça por lá, e a cidade tá sem carne. Você tem arma, não tem? Me traz três peças e eu pago bem.");
            yield return Say("Brás", "Saída sul, passando a ponte. Cuidado com o javali: ele avisa antes de vir. Quando ele parar e te encarar, sai da frente!");
            Historia.Marcar("m2_aceita");
            End();
        }
        IEnumerator BrasRecebe()
        {
            yield return StartTalk(bras);
            var s = Recursos.Servico;
            if (s == null || !s.GastarRecurso(Recursos.Carne, Historia.CarnesPedidas, "Carne entregue ao Brás."))
            { yield return Say("Brás", "Cadê a carne, amigo? Promessa não enche barriga."); End(); yield break; }
            s.CollectRegionalGold(40);
            yield return Say("Brás", "Olha só! Inteiro e com carne. Toma, quarenta de ouro, como combinado.");
            yield return Say("Brás", "E guarda o couro, viu? O Téo sabe o que fazer com couro de bicho tomado.");
            Historia.Marcar("m2_feita");
            End();
        }
        IEnumerator TeoFazManto()
        {
            yield return StartTalk(teo);
            var s = Recursos.Servico;
            if (s == null || !s.GastarRecurso(Recursos.Couro, Historia.CourosPedidos, "Couro entregue ao Téo."))
            { yield return Say("Téo", "Sem couro, sem peça."); End(); yield break; }
            yield return Say("Téo", "Hm. Couro de bicho tomado. O Breu deixa ele duro feito casca.");
            yield return Say("Téo", "(trabalha em silêncio por um tempo)");
            s.GrantTestEquipment("brasa-armor", ItemRarity.Common);
            var item = s.Profile.inventory.FindLast(x => x.definitionId == "brasa-armor");
            if (item != null) s.Equip(world.Hero.Current.heroId, item.instanceId, EquipmentSlot.Armor);
            yield return Say("Téo", "Pronto. Um manto. Não é bonito, mas segura um golpe.");
            yield return Say("Téo", "Viu como funciona? Material lá de fora vira peça aqui dentro. Quanto melhor o material, melhor a peça.");
            yield return Say("Téo", "E mais uma. Fica perto da forja. Sente o calor? Segura ele na arma. Assim.");
            Habilidades.Aprender("brasa");
            Historia.Marcar("m3_feita");
            End();
        }
        IEnumerator NaraNascente()
        {
            yield return StartTalk(nara);
            yield return Say("Nara", "Você foi até o rio. O que viu?");
            yield return Say(Me, "Bichos com olho roxo. E a água... meio escura.");
            yield return Say("Nara", "Breu. Descendo com a água. Se está no rio, vem de cima.");
            yield return Say("Nara", "A Nascente fica ao sul, rio acima. Meu mapa de lá é velho e tem buracos. Não gosto de buracos.");
            yield return Say("Nara", "Vá até a Nascente e descubra o que está escurecendo o rio. Só olhe. Volte inteiro.");
            Historia.Marcar("m4_aceita");
            End();
        }

        IEnumerator StartTalk(Transform person)
        {
            talking = true; Hud(false); Control(false);
            if (person != null) { var face = world.Hero.transform.position - person.position; face.y = 0; if (face.sqrMagnitude > .01f) person.rotation = Quaternion.LookRotation(face); }
            yield return null;
        }
        void End() { CloseTalk(); Hud(true); Control(true); talking = false; }

        IEnumerator Short(string who, string text)
        {
            yield return StartTalk(who == "Téo" ? teo : who == "Brás" ? bras : who == "Nara" ? nara : iria);
            yield return Say(who, text);
            End();
        }

        IEnumerator Chegada()
        {
            Hud(false); Control(false);
            curtainImage.color = Color.white; curtain.alpha = 1;
            // You wake up sitting among the roots of the Nó do Refúgio.
            var no = world.No;
            Vector3 center = no != null ? no.position : new Vector3(-13.5f, 0, 23);
            Vector3 hero = OnGround(center + new Vector3(.6f, 0, -.8f));
            world.Hero.GetComponent<PlayerMotor>()?.Teleport(hero, Quaternion.Euler(0, 160, 0));
            FindFirstObjectByType<ArenaCamera>()?.Snap();
            var walkers = new List<(Transform who, Vector3 pos, Quaternion rot)>();
            foreach (var who in new[] { nara, iria })
                if (who != null)
                {
                    walkers.Add((who, who.position, who.rotation));
                    var walk = who.GetComponent<TownResident>(); if (walk != null) walk.enabled = false;
                }
            Place(iria, OnGround(hero + new Vector3(1.3f, 0, -1f)), hero);
            Place(nara, OnGround(hero + new Vector3(4.2f, 0, -3.2f)), hero);
            var body = world.Hero.GetComponentInChildren<Animator>();
            if (body != null && body.HasState(0, Animator.StringToHash("Sentado"))) body.Play("Sentado", 0, 0);
            float volume = AudioListener.volume; AudioListener.volume = volume * .25f;
            Bell(.8f, true);
            // Blurry and muffled: the white fades slowly and the sound comes back little by little.
            for (float t = 0; t < 3.2f; t += Time.unscaledDeltaTime)
            {
                curtain.alpha = 1 - t / 3.2f; AudioListener.volume = Mathf.Lerp(volume * .25f, volume, t / 3.2f);
                yield return null;
            }
            curtain.alpha = 0; AudioListener.volume = volume;
            yield return Say("Iria", "Ei! Ei, acordou! Nara! NARA!");
            yield return Say(Me, "Onde...");
            yield return Say("Iria", "Calma, calma, não levanta rápido. Bebe isso. Tudo, bebe tudo.");
            if (nara != null) Place(nara, OnGround(hero + new Vector3(2.4f, 0, -2.2f)), hero);
            yield return Say("Nara", "(chegando, sem pressa) Outro?");
            yield return Say("Iria", "Outro! E tá inteiro, olha!");
            yield return Say("Nara", "Você está no Refúgio. Descansa. Depois a gente conversa.");
            yield return Say("Iria", "Quando conseguir ficar de pé, anda um pouco. Ajuda.");
            CloseTalk();
            if (body != null) body.CrossFadeInFixedTime("Idle", .4f, 0);
            yield return Fade(1, .6f, Color.black);
            // They go back to what they were doing.
            foreach (var (who, pos, rot) in walkers)
            {
                Place(who, pos, pos + rot * Vector3.forward);
                var walk = who.GetComponent<TownResident>(); if (walk != null) walk.enabled = true;
            }
            Historia.Marcar("chegada");
            yield return Fade(0, .6f);
        }

        IEnumerator TalkNara()
        {
            yield return StartTalk(nara);
            yield return Say(Me, "Que lugar é esse?");
            yield return Say("Nara", "Refúgio do Último Sino. Aquela Raiz-Mãe ali puxa gente de longe. Fazia anos que não trazia ninguém.");
            yield return Say(Me, "Eu preciso voltar pra casa.");
            yield return Say("Nara", "De onde você é?");
            yield return Say(Me, "(você conta da sua vila, do tio, da estação velha)");
            yield return Say("Nara", "...Não conheço. E olha que eu desenho mapas.");
            yield return Say("Nara", "A Raiz-Mãe daqui está rachada. Ela só leva pra outras Raízes-Mães vivas, e a da sua vila, pelo jeito, morreu. Pelo mesmo caminho ninguém volta.");
            yield return Say(Me, "Então como eu volto?");
            yield return Say("Nara", "Achando outro caminho. Lá fora tem estações apagadas, terras que ninguém pisa faz gerações.");
            yield return Say("Nara", "Mas lá fora, de mãos vazias, você não dura uma tarde. Passa no Téo, na forja. Diz que fui eu que mandei.");
            Historia.Marcar("nara");
            End();
        }

        IEnumerator TalkTeo()
        {
            yield return StartTalk(teo);
            yield return Say("Téo", "Hm. Chegado. Conheço essa cara.");
            yield return Say("Téo", "Caí naquelas mesmas raízes, faz vinte anos. Também queria voltar.");
            yield return Say("Téo", "(pausa) Às vezes ainda quero.");
            yield return Say("Téo", "Não tenho nada bonito pra te dar. Mas tenho duas peças que aguentam o tranco. Escolhe a que combina com você.");
            CloseTalk();
            int choice = -1;
            yield return ChooseWeapon(v => choice = v);
            bool sword = choice == 0;
            GiveWeapon(sword);
            yield return Say("Téo", sword ? "Espada. Bom. Fica perto, olha nos olhos do bicho." : "Cajado. Bom. Fica longe e não deixa eles chegarem.");
            yield return Say("Téo", "É simples, mas é honesta. Traz material lá de fora que eu faço quase qualquer coisa, e material bom vira peça única. Sem material, nem eu faço milagre.");
            yield return Say("Téo", "E antes de sair, toca a Raiz-Mãe. Ela grava você: se cair lá fora, ela te puxa de volta pra cá.");
            Historia.Marcar("teo");
            End();
        }

        // Two big buttons: close combat or distance. Every weapon in the game is one of these two.
        IEnumerator ChooseWeapon(Action<int> chosen)
        {
            int pick = -1;
            var panel = RusticHudTheme.Rect(root, "EscolhaDaArma", new Vector2(.5f, .5f), Vector2.zero, new Vector2(1000, 470));
            RusticHudTheme.Panel(panel, new Color(.03f, .055f, .07f, .96f), 22);
            var title = RusticHudTheme.Label(panel, "Titulo", new Vector2(.5f, 1), new Vector2(0, -50), new Vector2(900, 50), "Escolha sua primeira arma", 32, TextAnchor.MiddleCenter); title.color = RusticHudTheme.Gold;
            RusticHudTheme.Label(panel, "Aviso", new Vector2(.5f, 1), new Vector2(0, -100), new Vector2(900, 40), "Toda arma do jogo é de perto ou de longe. Esta é só o começo.", 20, TextAnchor.MiddleCenter).color = RusticHudTheme.Muted;
            RusticHudTheme.Button(panel, "Espada", new Vector2(.5f, .5f), new Vector2(-235, -40), new Vector2(420, 210), "ESPADA\n\ncorpo a corpo\nbate forte, fica perto", () => pick = 0);
            RusticHudTheme.Button(panel, "Cajado", new Vector2(.5f, .5f), new Vector2(235, -40), new Vector2(420, 210), "CAJADO\n\nà distância\nmagia de longe, mantém o perigo longe", () => pick = 1);
            panel.SetAsLastSibling();
            while (pick < 0) yield return null;
            Destroy(panel.gameObject);
            chosen(pick);
        }

        // The character's style follows the weapon: sword = close combat hero, staff = distance hero.
        void GiveWeapon(bool sword)
        {
            int look = sword ? 0 : 2;
            AccountService.SetLook(look);
            if (world.Hero.Count > look) world.Hero.Select(look);
            var service = world.GetComponent<RegionalInventory>()?.Service;
            if (service == null) return;
            string id = sword ? "cais-blade" : "sal-staff";
            service.GrantTestEquipment(id, ItemRarity.Common);
            var item = service.Profile.inventory.FindLast(x => x.definitionId == id);
            if (item != null) service.Equip(world.Hero.Current.heroId, item.instanceId, EquipmentSlot.MainHand);
        }
    }
}
