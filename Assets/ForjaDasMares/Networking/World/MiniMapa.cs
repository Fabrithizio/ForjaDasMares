using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // Round minimap (top-right), used in every region.
    // - The ground is drawn from the region when it loads: walkable streets, houses and walls, trees.
    // - What you have not walked near yet stays dark, and stays revealed next time (saved per character and region).
    // - Places you have found (forge, fountain, shop, station, exits, people...) get an icon.
    // - Turns with the camera, so "up" on the minimap is where the camera looks. The arrow is you.
    // - Tap/click or N: bigger view. + / -: zoom. Works the same on PC and phone.
    public sealed class MiniMapa : MonoBehaviour
    {
        const float Small = 165, Big = 400, Border = 7, RevealRadius = 15;
        const int FogSize = 128;

        RegionalWorld world;
        RectTransform frame, spin, ground, arrow, north;
        RawImage groundImage, fogImage;
        Texture2D fog; Color32[] fogPixels; byte[] explored;
        Vector2 center; float side;
        float radius = 32, smallRadius = 32, bigRadius = 70; bool expanded;
        // Big map: drag to look around (PC and phone) without walking there. Metres away from the hero.
        Vector2 pan;
        string key; float revealTimer, saveTimer, poiTimer; bool fogDirty, saveDirty; int lastTargets;
        readonly List<Poi> pois = new List<Poi>();
        RectTransform markers;

        sealed class Poi { public Transform target; public Vector3 fixedPosition; public RectTransform icon; public Text label, mark; public string who; public Graphic symbol; public RoundedHudGraphic badge; public Color color; }

        // Moving people and enemies: small live dots (light = townsfolk, red = enemies).
        readonly List<Transform> walkers = new List<Transform>();
        readonly List<EnemyActor> foes = new List<EnemyActor>();
        readonly List<RectTransform> dots = new List<RectTransform>();
        float walkerTimer;

        void FindWalkers()
        {
            walkers.Clear(); foes.Clear();
            foreach (var r in FindObjectsByType<TownResident>(FindObjectsSortMode.None)) walkers.Add(r.transform);
            foreach (var e in FindObjectsByType<EnemyActor>(FindObjectsSortMode.None)) foes.Add(e);
            animals.Clear(); foreach (var a in FindObjectsByType<AnimalSelvagem>(FindObjectsSortMode.None)) animals.Add(a);
        }
        readonly List<AnimalSelvagem> animals = new List<AnimalSelvagem>();

        RectTransform Dot(int index)
        {
            while (dots.Count <= index)
            {
                var d = RusticHudTheme.Rect(markers, "Ponto", new Vector2(.5f, .5f), Vector2.zero, new Vector2(7, 7));
                var g = d.gameObject.AddComponent<RoundedHudGraphic>(); g.radius = 3.5f; g.raycastTarget = false; g.borderColor = new Color(.05f, .05f, .05f);
                d.SetAsFirstSibling(); // under the icons
                dots.Add(d);
            }
            return dots[index];
        }

        void DrawWalkers(float ppm)
        {
            walkerTimer -= Time.unscaledDeltaTime;
            if (walkerTimer <= 0) { walkerTimer = 2; FindWalkers(); }
            int used = 0;
            foreach (var w in walkers)
            {
                if (w == null || !w.gameObject.activeInHierarchy || !Explored(w.position)) continue;
                var d = Dot(used++); d.gameObject.SetActive(true);
                d.anchoredPosition = (new Vector2(w.position.x, w.position.z) - center) * ppm; d.sizeDelta = Vector2.one * 7;
                var g = d.GetComponent<RoundedHudGraphic>(); g.radius = 3.5f; g.color = new Color(.95f, .9f, .78f); g.borderColor = new Color(.05f, .05f, .05f);
            }
            foreach (var e in foes)
            {
                if (e == null || !e.gameObject.activeInHierarchy || e.Health == null || !e.Health.IsAlive) continue;
                var p = e.transform.position; if ((new Vector2(p.x, p.z) - new Vector2(world.Hero.transform.position.x, world.Hero.transform.position.z)).magnitude > radius * 1.5f) continue;
                var d = Dot(used++); d.gameObject.SetActive(true);
                // Normal: small red. Strong (mini-boss): bigger orange with gold ring. Boss: biggest purple with gold ring.
                bool boss = e.Profile != null && e.Profile.kind == EnemyKind.Boss && !e.Profile.miniboss, strong = e.Profile != null && (e.Profile.miniboss || e.Profile.kind == EnemyKind.Boss);
                d.anchoredPosition = (new Vector2(p.x, p.z) - center) * ppm; d.sizeDelta = Vector2.one * (boss ? 15 : strong ? 11 : 8);
                var g = d.GetComponent<RoundedHudGraphic>(); g.radius = d.sizeDelta.x * .5f;
                g.color = boss ? new Color(.72f, .35f, 1f) : strong ? new Color(1f, .58f, .15f) : new Color(1f, .3f, .22f);
                g.borderColor = strong ? RusticHudTheme.Gold : new Color(.05f, .05f, .05f);
            }
            // Animals: red when taken by the Breu (they attack), green when peaceful.
            foreach (var a in animals)
            {
                if (a == null || !a.Vivo) continue;
                var p = a.transform.position; if ((new Vector2(p.x, p.z) - new Vector2(world.Hero.transform.position.x, world.Hero.transform.position.z)).magnitude > radius * 1.5f) continue;
                var d = Dot(used++); d.gameObject.SetActive(true);
                d.anchoredPosition = (new Vector2(p.x, p.z) - center) * ppm; d.sizeDelta = Vector2.one * 8;
                var g = d.GetComponent<RoundedHudGraphic>(); g.radius = 4;
                g.color = a.Tomado ? new Color(1f, .3f, .22f) : new Color(.45f, .95f, .45f); g.borderColor = new Color(.05f, .05f, .05f);
            }
            for (int i = used; i < dots.Count; i++) if (dots[i].gameObject.activeSelf) dots[i].gameObject.SetActive(false);
        }

        public static MiniMapa Criar(RectTransform root, RegionalWorld world, float top)
        {
            var rect = RusticHudTheme.Rect(root, "MiniMapa", new Vector2(1, 1), new Vector2(-20, top), new Vector2(Small, Small));
            rect.pivot = new Vector2(1, 1);
            var map = rect.gameObject.AddComponent<MiniMapa>();
            map.world = world; map.frame = rect;
            map.Build();
            return map;
        }

        void Build()
        {
            // Round window.
            var mask = RusticHudTheme.Rect(frame, "Janela", new Vector2(.5f, .5f), Vector2.zero, Vector2.one * (Small - Border * 2));
            var maskImage = mask.gameObject.AddComponent<Image>(); maskImage.sprite = CircleSprite(); maskImage.color = new Color(.05f, .09f, .1f, 1);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            mask.gameObject.AddComponent<Clique>().map = this;
            spin = RusticHudTheme.Rect(mask, "Giro", new Vector2(.5f, .5f), Vector2.zero, Vector2.one);
            ground = RusticHudTheme.Rect(spin, "Chao", new Vector2(.5f, .5f), Vector2.zero, Vector2.one * 100);
            groundImage = ground.gameObject.AddComponent<RawImage>(); groundImage.raycastTarget = false; groundImage.color = new Color(.12f, .2f, .22f);
            var fogRect = RusticHudTheme.Rect(ground, "Nevoa", new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
            fogRect.anchorMin = Vector2.zero; fogRect.anchorMax = Vector2.one; fogRect.offsetMin = fogRect.offsetMax = Vector2.zero;
            fogImage = fogRect.gameObject.AddComponent<RawImage>(); fogImage.raycastTarget = false;
            markers = RusticHudTheme.Rect(ground, "Lugares", new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);

            // You: an arrow in the middle.
            arrow = RusticHudTheme.Rect(mask, "Voce", new Vector2(.5f, .5f), Vector2.zero, new Vector2(18, 21));
            var arrowImage = arrow.gameObject.AddComponent<RawImage>(); arrowImage.texture = ArrowTexture(); arrowImage.raycastTarget = false;

            // Copper ring and the north mark that slides around it.
            var ring = RusticHudTheme.Rect(frame, "Moldura", new Vector2(.5f, .5f), Vector2.zero, Vector2.one * Small);
            ring.anchorMin = Vector2.zero; ring.anchorMax = Vector2.one; ring.offsetMin = ring.offsetMax = Vector2.zero;
            var ringImage = ring.gameObject.AddComponent<RawImage>(); ringImage.texture = RingTexture(); ringImage.raycastTarget = false;
            north = RusticHudTheme.Rect(frame, "Norte", new Vector2(.5f, .5f), Vector2.zero, new Vector2(20, 20));
            var nBadge = north.gameObject.AddComponent<RoundedHudGraphic>(); nBadge.radius = 10; nBadge.color = new Color(.1f, .12f, .14f); nBadge.borderColor = RusticHudTheme.Gold; nBadge.raycastTarget = false;
            RusticHudTheme.Label(north, "N", new Vector2(.5f, .5f), Vector2.zero, new Vector2(20, 20), "N", 12, TextAnchor.MiddleCenter).color = RusticHudTheme.Gold;

            // Zoom buttons under the ring (big enough for a finger).
            RusticHudTheme.Button(frame, "Menos", new Vector2(0, 0), new Vector2(4, -4), new Vector2(34, 34), "−", () => Zoom(1.25f));
            RusticHudTheme.Button(frame, "Mais", new Vector2(1, 0), new Vector2(-4, -4), new Vector2(34, 34), "+", () => Zoom(.8f));
            // The world map, between − and + (small and out of the way; K on the PC).
            var mapa = RusticHudTheme.IconButton(frame, "MapaMundo", new Vector2(.5f, 0), new Vector2(0, -6), HudSymbol.Compass, () => { if (!MapaDoMundo.Aberto) MapaDoMundo.Abrir(world, false); });
            ((RectTransform)mapa.transform).sizeDelta = new Vector2(38, 38);
            mapa.GetComponent<RoundedHudGraphic>().radius = 19;
            // Big map: a click (or tap) on it walks there; this X (or N) closes it.
            fechar = RusticHudTheme.Button(frame, "Fechar", new Vector2(1, 1), new Vector2(-2, -2), new Vector2(38, 38), "×", () => { if (expanded) Toggle(); }).gameObject;
            fechar.SetActive(false);
            // Where you are walking to: a small gold mark on the map.
            alvoMarca = RusticHudTheme.Rect(spin, "Destino", new Vector2(.5f, .5f), Vector2.zero, new Vector2(14, 14));
            var marca = alvoMarca.gameObject.AddComponent<RoundedHudGraphic>(); marca.radius = 7; marca.color = new Color(1f, .82f, .38f); marca.borderColor = new Color(.15f, .1f, .05f); marca.raycastTarget = false;
            alvoMarca.gameObject.SetActive(false);

            var bounds = world.Definition.bounds;
            center = new Vector2(bounds.center.x, bounds.center.z);
            side = Mathf.Max(bounds.size.x, bounds.size.z);
            key = "ForjaMini_" + (string.IsNullOrEmpty(CampaignHome.CharacterName) ? "padrao" : CampaignHome.CharacterName) + "_" + world.Definition.regionId;
            LoadExplored();
            StartCoroutine(TakePhoto());
        }

        // Phone: the big map opens in the middle of the screen, above the buttons, and goes back to the corner.
        Vector2 cornerPosition; bool cornerKnown;
        public void Toggle()
        {
            expanded = !expanded;
            bool phone = UnityEngine.Device.Application.isMobilePlatform;
            float big = phone ? 480 : Big;
            if (phone)
            {
                if (!cornerKnown) { cornerPosition = frame.anchoredPosition; cornerKnown = true; }
                var anchor = expanded ? new Vector2(.5f, .5f) : new Vector2(1, 1);
                frame.anchorMin = frame.anchorMax = frame.pivot = anchor;
                frame.anchoredPosition = expanded ? Vector2.zero : cornerPosition;
                if (expanded) frame.SetAsLastSibling();
            }
            frame.sizeDelta = Vector2.one * (expanded ? big : Small);
            ((RectTransform)frame.Find("Janela")).sizeDelta = Vector2.one * ((expanded ? big : Small) - Border * 2);
            if (expanded) { smallRadius = radius; radius = bigRadius; } else { bigRadius = radius; radius = smallRadius; }
            pan = Vector2.zero;
            if (fechar != null) fechar.SetActive(expanded);
        }
        GameObject fechar; RectTransform alvoMarca;

        // A click on the big map: the hero walks to that spot (on the mount if riding). Moving or clicking stops it.
        void IrPara(PointerEventData data)
        {
            if (world == null || world.Hero == null || lastPpm <= 0) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(frame, data.position, data.pressEventCamera, out var local)) return;
            local -= frame.rect.center; // from the middle of the map (the frame's pivot sits in its corner)
            var metres = (Vector2)(Quaternion.Euler(0, 0, -lastYaw) * (Vector3)local) / lastPpm;
            var hero = world.Hero.transform.position;
            var flat = new Vector2(hero.x, hero.z) + pan + metres;
            var point = new Vector3(flat.x, hero.y, flat.y);
            if (Physics.Raycast(point + Vector3.up * 200, Vector3.down, out var hit, 400, 1 << 8, QueryTriggerInteraction.Ignore)) point = hit.point;
            // Clicked on a house or a wall: go to the nearest walkable spot.
            if (UnityEngine.AI.NavMesh.SamplePosition(point, out var chao, 8f, UnityEngine.AI.NavMesh.AllAreas)) point = chao.position;
            var controles = world.Hero.GetComponent<ArenaInput>()?.Desktop;
            if (controles == null) return;
            if (!controles.IrPara(point)) { EfeitosDeHabilidade.Explosao(world.Hero.transform.position + Vector3.up * 2, .4f, new Color(1f, .4f, .3f), 6); return; }
            destino = point;
        }
        Vector3? destino;

        void Zoom(float factor) => radius = Mathf.Clamp(radius * factor, 12, expanded ? 140 : 80);

        // ---------- Drawn map of the region ----------
        // Not a photo: the map is drawn from the region itself. Where you can walk (the navigation mesh) is light
        // stone, buildings and walls are brown blocks, trees are green dots, everything else is dark. Done once.
        const int DrawSize = 512;
        Texture2D drawn; Color32[] canvas;

        IEnumerator TakePhoto()
        {
            yield return new WaitForSeconds(.8f); // let the city, trees and people finish appearing
            drawn = new Texture2D(DrawSize, DrawSize, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "MiniMapaDesenho" };
            canvas = new Color32[DrawSize * DrawSize];
            var background = new Color32(18, 40, 44, 255);
            for (int i = 0; i < canvas.Length; i++) canvas[i] = background;

            // Walkable ground, with a darker rim so streets read as streets.
            var mesh = UnityEngine.AI.NavMesh.CalculateTriangulation();
            var rim = new Color32(92, 82, 62, 255); var floor = new Color32(206, 188, 150, 255);
            for (int pass = 0; pass < 2; pass++)
                for (int i = 0; i + 2 < mesh.indices.Length; i += 3)
                {
                    Vector2 a = Pixel(mesh.vertices[mesh.indices[i]]), b = Pixel(mesh.vertices[mesh.indices[i + 1]]), c = Pixel(mesh.vertices[mesh.indices[i + 2]]);
                    if (pass == 0) { var m = (a + b + c) / 3; a = Grow(a, m); b = Grow(b, m); c = Grow(c, m); }
                    Triangle(a, b, c, pass == 0 ? rim : floor);
                }
            yield return null;

            // Solid things: houses, walls, stalls, rocks (their real footprint), trees as round dots.
            var heroRoot = world.Hero.transform;
            var roof = new Color32(132, 84, 54, 255); var edge = new Color32(54, 32, 20, 255); var tree = new Color32(52, 118, 66, 255); var treeEdge = new Color32(24, 60, 34, 255);
            foreach (var col in FindObjectsByType<Collider>(FindObjectsSortMode.None))
            {
                if (col == null || !col.enabled || col.isTrigger || col.gameObject.layer == 5) continue;
                if (col.transform.IsChildOf(heroRoot) || col.GetComponentInParent<EnemyActor>() != null || col.GetComponentInParent<TownResident>() != null) continue;
                var bounds = col.bounds;
                if (bounds.size.x > side * .45f || bounds.size.z > side * .45f) continue; // the ground itself
                if (bounds.size.y < .25f) continue; // flat decals and floors
                if (col is CapsuleCollider && bounds.size.x < 3.5f)
                {
                    Vector2 c = Pixel(bounds.center); float r = Mathf.Max(1.6f, bounds.extents.x / side * DrawSize * 1.6f);
                    Disc(c, r + 1, treeEdge); Disc(c, r, tree);
                    continue;
                }
                Vector2[] corners;
                if (col is BoxCollider box)
                {
                    var t = box.transform; Vector3 h = box.size * .5f, o = box.center;
                    corners = new[] { Pixel(t.TransformPoint(o + new Vector3(-h.x, 0, -h.z))), Pixel(t.TransformPoint(o + new Vector3(h.x, 0, -h.z))), Pixel(t.TransformPoint(o + new Vector3(h.x, 0, h.z))), Pixel(t.TransformPoint(o + new Vector3(-h.x, 0, h.z))) };
                }
                else corners = new[] { Pixel(new Vector3(bounds.min.x, 0, bounds.min.z)), Pixel(new Vector3(bounds.max.x, 0, bounds.min.z)), Pixel(new Vector3(bounds.max.x, 0, bounds.max.z)), Pixel(new Vector3(bounds.min.x, 0, bounds.max.z)) };
                var mid = (corners[0] + corners[1] + corners[2] + corners[3]) / 4;
                var outer = new Vector2[4]; for (int k = 0; k < 4; k++) outer[k] = Grow(corners[k], mid, 1.6f);
                Triangle(outer[0], outer[1], outer[2], edge); Triangle(outer[0], outer[2], outer[3], edge);
                Triangle(corners[0], corners[1], corners[2], roof); Triangle(corners[0], corners[2], corners[3], roof);
            }
            drawn.SetPixels32(canvas); drawn.Apply(false);
            canvas = null;
            groundImage.texture = drawn; groundImage.color = Color.white;
            Debug.Log("MINIMAPA desenhado · " + world.Definition.regionId + " · chão " + mesh.indices.Length / 3 + " triângulos");
            CollectPlaces();
        }

        Vector2 Pixel(Vector3 point) => new Vector2(((point.x - center.x) / side + .5f) * DrawSize, ((point.z - center.y) / side + .5f) * DrawSize);
        static Vector2 Grow(Vector2 p, Vector2 from, float amount = 1.4f) { var d = p - from; return d.sqrMagnitude < .0001f ? p : p + d.normalized * amount; }

        void Triangle(Vector2 a, Vector2 b, Vector2 c, Color32 color)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x)))), maxX = Mathf.Min(DrawSize - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
            int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y)))), maxY = Mathf.Min(DrawSize - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));
            float area = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
            if (Mathf.Abs(area) < .0001f) return;
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    float px = x + .5f, py = y + .5f;
                    float w0 = (b.x - px) * (c.y - py) - (b.y - py) * (c.x - px), w1 = (c.x - px) * (a.y - py) - (c.y - py) * (a.x - px), w2 = (a.x - px) * (b.y - py) - (a.y - py) * (b.x - px);
                    if (area > 0 ? (w0 >= 0 && w1 >= 0 && w2 >= 0) : (w0 <= 0 && w1 <= 0 && w2 <= 0)) canvas[y * DrawSize + x] = color;
                }
        }

        void Disc(Vector2 c, float r, Color32 color)
        {
            for (int y = Mathf.Max(0, (int)(c.y - r)); y <= Mathf.Min(DrawSize - 1, (int)(c.y + r)); y++)
                for (int x = Mathf.Max(0, (int)(c.x - r)); x <= Mathf.Min(DrawSize - 1, (int)(c.x + r)); x++)
                    if ((x + .5f - c.x) * (x + .5f - c.x) + (y + .5f - c.y) * (y + .5f - c.y) <= r * r) canvas[y * DrawSize + x] = color;
        }

        // ---------- Discovered ground ----------
        void LoadExplored()
        {
            explored = new byte[FogSize * FogSize];
            var saved = PlayerPrefs.GetString(key, "");
            if (!string.IsNullOrEmpty(saved))
            {
                try { var bytes = System.Convert.FromBase64String(saved); if (bytes.Length == explored.Length) explored = bytes; }
                catch (System.FormatException) { }
            }
            fog = new Texture2D(FogSize, FogSize, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            fogPixels = new Color32[explored.Length];
            fogImage.texture = fog;
            fogDirty = true;
        }

        bool Explored(Vector3 position)
        {
            int x = Mathf.FloorToInt(((position.x - center.x) / side + .5f) * FogSize), y = Mathf.FloorToInt(((position.z - center.y) / side + .5f) * FogSize);
            return x >= 0 && y >= 0 && x < FogSize && y < FogSize && explored[y * FogSize + x] != 0;
        }

        void Reveal(Vector3 position)
        {
            float cell = side / FogSize; int reach = Mathf.CeilToInt(RevealRadius / cell);
            int cx = Mathf.FloorToInt(((position.x - center.x) / side + .5f) * FogSize), cy = Mathf.FloorToInt(((position.z - center.y) / side + .5f) * FogSize);
            for (int y = cy - reach; y <= cy + reach; y++)
            for (int x = cx - reach; x <= cx + reach; x++)
            {
                if (x < 0 || y < 0 || x >= FogSize || y >= FogSize) continue;
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) > reach * reach) continue;
                int i = y * FogSize + x;
                if (explored[i] == 0) { explored[i] = 1; fogDirty = saveDirty = true; }
            }
        }

        void PaintFog()
        {
            for (int i = 0; i < explored.Length; i++) fogPixels[i] = explored[i] != 0 ? new Color32(0, 0, 0, 0) : new Color32(6, 12, 16, 235);
            fog.SetPixels32(fogPixels); fog.Apply(false);
            fogDirty = false;
        }

        // ---------- Places with icons ----------
        void CollectPlaces()
        {
            foreach (var poi in pois) if (poi.icon != null) Destroy(poi.icon.gameObject);
            pois.Clear();
            foreach (var target in FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None))
            {
                string label = target.Label ?? "";
                if (label.ToLowerInvariant().Contains("ouro")) continue; // gold piles come and go
                if (label.StartsWith("Pegar") || label.StartsWith("Colher")) continue; // loot and herbs too
                string who = null;
                if (label.StartsWith("Conversar"))
                {
                    // People: use the name on their head ("Téo · Ferreiro"); only the important ones get an icon.
                    var plate = Plate(target.transform);
                    if (plate == null && target.transform.parent != null) plate = Plate(target.transform.parent);
                    if (plate == null) continue;
                    var lines = System.Text.RegularExpressions.Regex.Replace(plate.text, "<.*?>", "").Replace('·', '\n').Split('\n');
                    who = lines[0].Trim(); string role = lines.Length > 1 ? lines[1].Trim() : "";
                    if (Style(who + " " + role).Item1 == HudSymbol.Hero) continue;
                    label = role.Length > 0 ? who + " · " + role : who;
                }
                Add(target.transform, Vector3.zero, label, who);
            }
            // Exits of the place: "Saída para Nascente do Rio" (closed ones too).
            bool exits = false;
            foreach (var (point, text) in AreasDoMundo.Placas(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)) { Add(null, point, text); exits = true; }
            if (!exits) Add(null, world.Definition.portal, "Saída da região");
            for (int i = 0; i < world.Definition.camps.Length; i++) Add(null, world.Definition.camps[i].center, "Acampamento inimigo");
        }

        // The name plate of a person (not the "!" / "?" sign over the head).
        static TextMesh Plate(Transform t)
        {
            foreach (var m in t.GetComponentsInChildren<TextMesh>(true)) if (m.GetComponentInParent<MarcadorDeMissao>() == null) return m;
            return null;
        }
        void Add(Transform target, Vector3 position, string label, string who = null)
        {
            var (symbol, color, bell) = Style(label);
            var icon = RusticHudTheme.Rect(markers, label, new Vector2(.5f, .5f), Vector2.zero, new Vector2(21, 21));
            var badge = icon.gameObject.AddComponent<RoundedHudGraphic>(); badge.radius = 10.5f; badge.color = new Color(.08f, .1f, .12f, .92f); badge.borderColor = color; badge.raycastTarget = false;
            if (bell) { var b = RusticHudTheme.Rect(icon, "Sino", new Vector2(.5f, .5f), Vector2.zero, new Vector2(20, 20)).gameObject.AddComponent<RawImage>(); b.texture = Resources.Load<Texture2D>("Mapa/IconeUltimoSino"); b.raycastTarget = false; }
            Graphic symbolArt = null;
            if (!bell) { var i = RusticHudTheme.Icon(icon, symbol, new Vector2(.5f, .5f), Vector2.zero, 14); i.color = color; symbolArt = i; }
            var name = RusticHudTheme.Label(icon, "Nome", new Vector2(.5f, 0), new Vector2(0, -10), new Vector2(180, 18), Short(label), 12, TextAnchor.MiddleCenter);
            name.horizontalOverflow = HorizontalWrapMode.Overflow;
            name.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0, 0, .95f);
            // A person with a new mission: the icon turns into "!" until the mission is accepted, then goes back.
            Text mark = null;
            if (who != null)
            {
                mark = RusticHudTheme.Label(icon, "Missao", new Vector2(.5f, .5f), new Vector2(0, 1), new Vector2(21, 21), "", 17, TextAnchor.MiddleCenter);
                mark.fontStyle = FontStyle.Bold; mark.raycastTarget = false;
                mark.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0, 0, .95f);
            }
            pois.Add(new Poi { target = target, fixedPosition = position, icon = icon, label = name, mark = mark, who = who, symbol = symbolArt, badge = badge, color = color });
        }

        static string Short(string label) { int cut = label.IndexOf('·'); return (cut > 0 ? label.Substring(0, cut) : label).Trim(); }

        static (HudSymbol, Color, bool) Style(string label)
        {
            var l = label.ToLowerInvariant();
            if (l.Contains("estação") || l.Contains("sino")) return (HudSymbol.Compass, RusticHudTheme.Gold, true);
            if (l.Contains("forja") || l.Contains("téo") || l.Contains("teo") || l.Contains("ferreir") || l.Contains("arma")) return (HudSymbol.Sword, new Color(1, .62f, .32f), false);
            if (l.Contains("fonte") || l.Contains("iria") || l.Contains("cura") || l.Contains("descans") || l.Contains("curandeir")) return (HudSymbol.Heart, new Color(.45f, .9f, 1f), false);
            if (l.Contains("loja") || l.Contains("mercad") || l.Contains("brás") || l.Contains("bras") || l.Contains("feira") || l.Contains("banca") || l.Contains("compr")) return (HudSymbol.Bag, new Color(.95f, .85f, .45f), false);
            if (l.Contains("expedi") || l.Contains("desafio") || l.Contains("catacumba") || l.Contains("altar") || l.Contains("instân")) return (HudSymbol.Rune, new Color(.75f, .55f, 1f), false);
            if (l.StartsWith("fechado")) return (HudSymbol.Target, new Color(1, .45f, .4f), false);
            if (l.Contains("saída") || l.Contains("estrada") || l.Contains("portão") || l.Contains("seguir") || l.Contains("costa")) return (HudSymbol.Compass, new Color(.6f, 1f, .7f), false);
            if (l.Contains("acampamento")) return (HudSymbol.Target, new Color(1, .35f, .25f), false);
            if (l.Contains("nara") || l.Contains("liga") || l.Contains("oficial") || l.Contains("missão") || l.Contains("cartógraf")) return (HudSymbol.Scroll, RusticHudTheme.Gold, false);
            return (HudSymbol.Hero, new Color(.85f, .9f, .88f), false);
        }

        // ---------- Every frame ----------
        void LateUpdate()
        {
            if (world == null || world.Hero == null) return;
            if (Teclas.Apertou("minimapa")) Toggle();
            var hero = world.Hero.transform;
            revealTimer -= Time.unscaledDeltaTime;
            if (revealTimer <= 0) { revealTimer = .2f; Reveal(hero.position); }
            if (fogDirty) PaintFog();
            saveTimer -= Time.unscaledDeltaTime;
            if (saveDirty && saveTimer <= 0) Save();
            poiTimer -= Time.unscaledDeltaTime;
            if (poiTimer <= 0 && drawn != null)
            {
                poiTimer = 4;
                // Again when people or places appear later (Brás gets his talk after the scene starts).
                int count = FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None).Length;
                if (pois.Count == 0 || count != lastTargets) { lastTargets = count; CollectPlaces(); }
            }

            float window = frame.sizeDelta.x - Border * 2;
            float ppm = window * .5f / radius;
            ground.sizeDelta = Vector2.one * side * ppm;
            if (!expanded) pan = Vector2.zero;
            ground.anchoredPosition = (center - new Vector2(hero.position.x, hero.position.z) - pan) * ppm;
            var cam = Camera.main;
            float yaw = cam != null ? cam.transform.eulerAngles.y : 0;
            spin.localRotation = Quaternion.Euler(0, 0, yaw);
            arrow.localRotation = Quaternion.Euler(0, 0, yaw - hero.eulerAngles.y);
            // You stay where you are on the map while it is dragged away (clamped to the window).
            var you = (Vector2)(Quaternion.Euler(0, 0, yaw) * (Vector3)(-pan * ppm));
            arrow.anchoredPosition = Vector2.ClampMagnitude(you, window * .5f - 8);
            lastYaw = yaw; lastPpm = ppm;
            if (expanded && Mouse.current != null && hoveringMap)
            { float wheel = Mouse.current.scroll.ReadValue().y; if (Mathf.Abs(wheel) > .01f) Zoom(wheel > 0 ? .9f : 1.1f); }
            float ring = frame.sizeDelta.x * .5f - Border * .5f;
            north.anchoredPosition = new Vector2(-Mathf.Sin(yaw * Mathf.Deg2Rad), Mathf.Cos(yaw * Mathf.Deg2Rad)) * ring;

            // The walk's destination on the map (while walking).
            var walking = world.Hero.GetComponent<ArenaInput>()?.Desktop;
            if (destino.HasValue && (walking == null || !walking.IndoParaPonto)) destino = null;
            alvoMarca.gameObject.SetActive(destino.HasValue);
            if (destino.HasValue) alvoMarca.anchoredPosition = (new Vector2(destino.Value.x, destino.Value.z) - new Vector2(hero.position.x, hero.position.z) - pan) * ppm;
            DrawWalkers(ppm);
            var upright = Quaternion.Euler(0, 0, -yaw);
            foreach (var poi in pois)
            {
                if (poi.icon == null) continue;
                Vector3 p = poi.target != null ? poi.target.position : poi.fixedPosition;
                bool show = (poi.target == null || poi.target.gameObject.activeInHierarchy) && Explored(p);
                if (poi.icon.gameObject.activeSelf != show) poi.icon.gameObject.SetActive(show);
                if (!show) continue;
                poi.icon.anchoredPosition = (new Vector2(p.x, p.z) - center) * ppm;
                poi.icon.localRotation = upright;
                poi.label.enabled = false; // decided below, so names never sit on top of each other
                if (poi.mark != null && Time.frameCount % 15 == 0)
                {
                    var (s, c) = Missoes.Marcador(poi.who);
                    bool offer = s == "!";
                    poi.mark.text = offer ? "!" : ""; poi.mark.color = c;
                    if (poi.symbol != null) poi.symbol.enabled = !offer;
                    poi.badge.borderColor = offer ? c : poi.color;
                }
            }
            if (expanded) PlaceLabels(yaw);
        }

        // Names only where they fit: people and exits first, then the rest; a name that would cover another is hidden
        // (zoom in or drag to read it).
        readonly List<Rect> placed = new List<Rect>();
        void PlaceLabels(float yaw)
        {
            placed.Clear();
            var rot = Quaternion.Euler(0, 0, yaw);
            float half = (frame.sizeDelta.x - Border * 2) * .5f;
            for (int pass = 0; pass < 3; pass++)
                foreach (var poi in pois)
                {
                    if (poi.icon == null || !poi.icon.gameObject.activeSelf) continue;
                    int rank = poi.who != null || poi.label.text.StartsWith("Saída") ? 0 : poi.label.text.StartsWith("Acampamento") ? 2 : 1;
                    if (rank != pass) continue;
                    Vector2 at = (Vector2)(rot * (Vector3)(poi.icon.anchoredPosition + ground.anchoredPosition));
                    if (Mathf.Abs(at.x) > half || Mathf.Abs(at.y) > half) continue;
                    float w = Mathf.Min(180, poi.label.preferredWidth + 6);
                    var box = new Rect(at.x - w * .5f, at.y - 20, w, 16);
                    bool free = true;
                    foreach (var r in placed) if (r.Overlaps(box)) { free = false; break; }
                    if (!free) continue;
                    placed.Add(box); poi.label.enabled = true;
                }
        }

        float lastYaw, lastPpm = 1; bool hoveringMap, dragged;
        void Arrastar(Vector2 screenDelta)
        {
            if (!expanded || lastPpm <= 0) return;
            float scale = frame.lossyScale.x > 0 ? frame.lossyScale.x : 1;
            var local = screenDelta / scale;
            var metres = (Vector2)(Quaternion.Euler(0, 0, -lastYaw) * (Vector3)local);
            pan -= metres / lastPpm;
            pan = Vector2.ClampMagnitude(pan, side * .6f);
        }

        void Save()
        {
            PlayerPrefs.SetString(key, System.Convert.ToBase64String(explored)); PlayerPrefs.Save();
            saveDirty = false; saveTimer = 5;
        }

        void OnDestroy()
        {
            if (saveDirty) Save();
            if (drawn != null) Destroy(drawn);
        }

        // Tap/click: open the small map; on the big one, walk there (X closes). Drag (big map): look around. Pointer over it: the wheel zooms.
        sealed class Clique : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
        {
            public MiniMapa map;
            public void OnPointerClick(PointerEventData data) { if (map.dragged) { map.dragged = false; return; } if (map.expanded) map.IrPara(data); else map.Toggle(); }
            public void OnBeginDrag(PointerEventData data) { map.dragged = map.expanded; }
            public void OnDrag(PointerEventData data) => map.Arrastar(data.delta);
            public void OnEndDrag(PointerEventData data) { }
            public void OnPointerEnter(PointerEventData data) => map.hoveringMap = true;
            public void OnPointerExit(PointerEventData data) => map.hoveringMap = false;
        }

        // ---------- Small drawn textures ----------
        static Sprite circle;
        static Sprite CircleSprite()
        {
            if (circle != null) return circle;
            const int S = 256; var t = new Texture2D(S, S, TextureFormat.RGBA32, false); var px = new Color32[S * S]; float r = S * .5f - 1;
            for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
            { float d = Vector2.Distance(new Vector2(x + .5f, y + .5f), Vector2.one * S * .5f); px[y * S + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(r - d) * 255)); }
            t.SetPixels32(px); t.Apply();
            return circle = Sprite.Create(t, new Rect(0, 0, S, S), Vector2.one * .5f);
        }

        static Texture2D ring;
        static Texture2D RingTexture()
        {
            if (ring != null) return ring;
            const int S = 512; var t = new Texture2D(S, S, TextureFormat.RGBA32, false); var px = new Color32[S * S];
            float outer = S * .5f - 2, width = S * .5f * (Border * 2.2f / Small) + 4;
            Color copper = new Color(.62f, .45f, .24f), light = new Color(.93f, .76f, .45f), dark = new Color(.16f, .1f, .05f);
            for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
            {
                float d = Vector2.Distance(new Vector2(x + .5f, y + .5f), Vector2.one * S * .5f);
                float band = (outer - d) / width; // 0 at the outside, 1 at the inside of the ring
                Color c = Color.clear;
                if (band >= 0 && band <= 1)
                {
                    float shine = 1 - Mathf.Abs(band - .45f) * 2.2f;
                    c = Color.Lerp(copper, light, Mathf.Clamp01(shine)); c.a = 1;
                    if (band < .12f || band > .88f) c = Color.Lerp(c, dark, .75f);
                    c.a = Mathf.Clamp01(Mathf.Min(band, 1 - band) * width * 1.5f);
                }
                else if (band > 1 && band < 1.6f) c = new Color(0, 0, 0, (1.6f - band) / .6f * .45f); // soft inner shadow
                px[y * S + x] = c;
            }
            t.SetPixels32(px); t.Apply();
            return ring = t;
        }

        static Texture2D arrowTexture;
        static Texture2D ArrowTexture()
        {
            if (arrowTexture != null) return arrowTexture;
            const int W = 88, H = 104; var t = new Texture2D(W, H, TextureFormat.RGBA32, false); var px = new Color32[W * H];
            Vector2 tip = new Vector2(W * .5f, H - 6), left = new Vector2(6, 6), right = new Vector2(W - 6, 6), notch = new Vector2(W * .5f, H * .3f);
            for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
            {
                var p = new Vector2(x + .5f, y + .5f);
                bool inside = InTriangle(p, tip, left, notch) || InTriangle(p, tip, notch, right);
                bool outline = !inside && (Near(p, tip, left) || Near(p, tip, right) || Near(p, left, notch) || Near(p, notch, right));
                px[y * W + x] = inside ? new Color32(120, 255, 225, 255) : outline ? new Color32(8, 20, 22, 255) : new Color32(0, 0, 0, 0);
            }
            t.SetPixels32(px); t.Apply();
            return arrowTexture = t;
        }
        static bool InTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y), d2 = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y), d3 = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0; return !(neg && pos);
        }
        static bool Near(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude); return Vector2.Distance(p, a + ab * t) < 6;
        }
    }
}
