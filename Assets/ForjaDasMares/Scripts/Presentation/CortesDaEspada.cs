using UnityEngine;
using UnityEngine.Rendering;

namespace ForjaDasMares
{
    // Anime-style slashes of the blade weapons (sword, axe, hammer, daggers), in the spirit of the reference picture
    // Fabricio sent: a bright crescent with a white core and a coloured glow, drawn on the plane of the real swing.
    // The arc is not a fixed drawing: it is read from where the blade tip actually travelled in the animation
    // right before the blow, so each move (horizontal, diagonal, upward...) gets its own slash.
    //   combo (1, 2, 3)  -> crescent in the colour of the weapon's rarity
    //   strong attack    -> larger crescent, doubled
    //   super            -> big crescent + spiral of light + burst
    // The weapon's power (rarity + forge level) makes it all grow (decided with Fabricio): a plain +0 sword gives a
    // simple slash; an upgraded or epic weapon gives bigger, longer, layered slashes with many more sparks.
    // Everything is pooled (4 slashes, one spark system): nothing is created per blow, good for weak phones.
    [DefaultExecutionOrder(250)]
    public sealed class CortesDaEspada : MonoBehaviour
    {
        const int History = 20, Segments = 30, Pool = 4;
        static readonly Color Azul = new Color(.22f, .58f, 1f), Ouro = new Color(1f, .72f, .25f), Brasa = new Color(1f, .42f, .12f);

        HeroPresentation hero; ExpedicionarioCorpo body; HeroEquipment gear;
        readonly Vector3[] tips = new Vector3[History]; readonly float[] when = new float[History]; int count;
        int version = -1; bool fired;
        readonly Corte[] slashes = new Corte[Pool]; int next;
        ParticleSystem sparks;

        public void Configure(HeroPresentation view) { hero = view; }
        // When the last slash appeared (used by the Editor test "Forja/Teste/Golpe e pausa" to freeze on it).
        public static float UltimoCorte { get; private set; } = -10;

        void LateUpdate()
        {
            if (hero == null) return;
            if (body == null) body = GetComponent<ExpedicionarioCorpo>();
            var state = hero.State; int variant = hero.ActionVariant;
            bool blow = state == HeroState.Attack || state == HeroState.Skill && variant == 4;
            bool blade = body != null && body.ArmaNaMao && Laminas(hero.EquippedWeapon);
            if (!blow || !blade) { count = 0; if (!blow) version = -1; Animate(); return; }
            if (hero.ActionVersion != version) { version = hero.ActionVersion; count = 0; fired = false; }
            // Remember where the tip went (newest first).
            for (int i = History - 1; i > 0; i--) { tips[i] = tips[i - 1]; when[i] = when[i - 1]; }
            tips[0] = body.PontaDaArma; when[0] = Time.time; count = Mathf.Min(count + 1, History);
            // The slash appears with the blow (the moment the hit lands in the game).
            if (!fired && count >= 3 && hero.ActionProgress >= hero.ActionImpact - .02f) { fired = true; Fire(variant); }
            Animate();
        }

        // 0 = common +0 ... 1 = legendary +12. Rarity weighs a bit more than the forge.
        public float Poder
        {
            get
            {
                if (gear == null) { gear = GetComponent<HeroEquipment>(); if (gear == null) gear = GetComponentInParent<HeroEquipment>(); }
                if (gear == null) return 0;
                return Mathf.Clamp01((int)gear.MainRarity / 4f * .55f + gear.MainForge / (float)ForgeRules.MaxLevel * .45f);
            }
        }
        static Color CorDaRaridade(ItemRarity r) =>
            r == ItemRarity.Uncommon ? new Color(.3f, 1f, .7f) :
            r == ItemRarity.Rare ? new Color(.15f, .45f, 1f) :
            r == ItemRarity.Epic ? new Color(.7f, .3f, 1f) :
            r == ItemRarity.Legendary ? new Color(1f, .62f, .15f) : Azul;

        static bool Laminas(EquipmentDefinition weapon)
        {
            var c = weapon != null ? weapon.category : WeaponCategory.Sword;
            return c == WeaponCategory.Sword || c == WeaponCategory.Axe || c == WeaponCategory.Hammer || c == WeaponCategory.Daggers;
        }

        void Fire(int variant)
        {
            float size = Mathf.Max(.5f, hero.Visible != null ? hero.Visible.lossyScale.y : 1);
            Vector3 center = transform.position + Vector3.up * 1.05f * size;
            // The swing: from the tip ~0.14 s ago to the tip now.
            int old = 1; while (old < count - 1 && Time.time - when[old] < .14f) old++;
            Vector3 ra = tips[old] - center, rb = tips[0] - center;
            Vector3 normal = Vector3.Cross(ra, rb);
            if (normal.sqrMagnitude < 1e-4f) normal = Vector3.Cross(rb, tips[0] - tips[Mathf.Min(1, count - 1)]);
            if (normal.sqrMagnitude < 1e-6f) normal = Vector3.up;
            normal.Normalize();
            // The game camera looks from above: a slash on a vertical plane would be seen edge-on, as a thin line.
            // Tilt the plane toward the camera (keeping the direction the blade turned), like anime games do.
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 facing = -cam.transform.forward; if (Vector3.Dot(normal, facing) < 0) facing = -facing;
                normal = Vector3.Slerp(normal, facing, .6f).normalized;
            }
            Vector3 x = Vector3.ProjectOnPlane(ra, normal); if (x.sqrMagnitude < 1e-6f) x = Vector3.ProjectOnPlane(transform.right, normal);
            x.Normalize(); Vector3 y = Vector3.Cross(normal, x);
            Vector3 flat = Vector3.ProjectOnPlane(rb, normal);
            float reach = Mathf.Atan2(Vector3.Dot(flat, y), Vector3.Dot(flat, x)) * Mathf.Rad2Deg; // how far the blade travelled (degrees)
            if (reach < 0) reach += 360;
            float end = reach + 45, span = Mathf.Clamp(end + 25, 130, 210), start = end - span;
            float radius = Mathf.Clamp(flat.magnitude, .7f * size, 1.4f * size) + .3f * size;

            UltimoCorte = Time.time;
            bool heavy = variant == 3, super = variant == 4;
            float power = Poder;
            var rarity = gear != null ? gear.MainRarity : ItemRarity.Common;
            var cat = hero.EquippedWeapon != null ? hero.EquippedWeapon.category : WeaponCategory.Sword;
            // Common and uncommon weapons: strong blows in gold. Rare and above keep their own colour, brighter.
            Color own = rarity == ItemRarity.Common ? (cat == WeaponCategory.Axe ? Brasa : cat == WeaponCategory.Hammer ? Ouro : Azul) : CorDaRaridade(rarity);
            Color tint = (super || heavy) && rarity <= ItemRarity.Uncommon ? Ouro : own;
            float scale = (super ? 1.55f : heavy ? 1.3f : 1) * (1 + .6f * power);   // the area grows with the weapon
            float width = .9f * (1 + .45f * power), life = .1f * power;
            Spawn(center, normal, x, y, start, end, radius * scale, width * size * scale, tint, (super ? .45f : heavy ? .38f : .3f) + life);
            // Doubled crescent: strong blows always; every blow from an upgraded weapon.
            if (heavy || super || power >= .3f) Spawn(center, normal, x, y, start - 25, end - 10, radius * scale * .74f, width * .72f * size * scale, Color.Lerp(tint, Color.white, .3f), (super ? .42f : .34f) + life);
            // An echo outside the first one: epic-level weapons.
            if (power >= .6f) Spawn(center, normal, x, y, start + 15, end + 20, radius * scale * 1.22f, width * .45f * size * scale, Color.Lerp(tint, Color.white, .15f), .36f + life);
            Sparks(center, normal, x, y, start, end, radius * scale, tint, Mathf.RoundToInt((super ? 26 : heavy ? 18 : 10) * (1 + 2f * power)));
            if (super)
            {
                EfeitosDeHabilidade.Extra("corte_giro", transform.position + Vector3.up * .9f * size, transform.rotation, 1.3f * size * (1 + .5f * power));
                EfeitosDeHabilidade.Explosao(transform.position + transform.forward * 1.2f * size, 2.2f * size * (1 + .5f * power), tint, Mathf.RoundToInt(40 * (1 + power)));
            }
            else if (heavy) EfeitosDeHabilidade.Extra("corte_giro_fino", transform.position + Vector3.up * .9f * size, transform.rotation, 1.05f * size * (1 + .5f * power));
            // Legendary-level weapons: every combo blow also leaves a thin spiral of light.
            else if (power >= .85f) EfeitosDeHabilidade.Extra("corte_giro_fino", transform.position + Vector3.up * .9f * size, transform.rotation, .8f * size);
        }

        void Spawn(Vector3 c, Vector3 n, Vector3 x, Vector3 y, float a0, float a1, float r, float w, Color tint, float life)
        {
            var s = slashes[next]; if (s == null) slashes[next] = s = new Corte(transform);
            next = (next + 1) % Pool;
            s.Begin(c, n, x, y, a0, a1, r, w, tint, life);
        }

        void Animate() { foreach (var s in slashes) s?.Tick(); }

        void Sparks(Vector3 c, Vector3 n, Vector3 x, Vector3 y, float a0, float a1, float r, Color tint, int amount)
        {
            if (sparks == null)
            {
                var go = new GameObject("FaiscasDoCorte"); go.transform.SetParent(transform, false);
                sparks = go.AddComponent<ParticleSystem>(); sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = sparks.main; main.loop = false; main.playOnAwake = false; main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = 120; main.gravityModifier = .35f;
                var emission = sparks.emission; emission.enabled = false;
                var col = sparks.colorOverLifetime; col.enabled = true;
                var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) }); col.color = g;
                var sz = sparks.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 0));
                var rend = go.GetComponent<ParticleSystemRenderer>(); rend.sharedMaterial = EfeitosDeHabilidade.Brilho; rend.renderMode = ParticleSystemRenderMode.Stretch; rend.velocityScale = .06f; rend.lengthScale = 1.5f;
                rend.shadowCastingMode = ShadowCastingMode.Off; rend.receiveShadows = false;
                sparks.Play();
            }
            for (int i = 0; i < amount; i++)
            {
                float a = Mathf.Lerp(a0 + (a1 - a0) * .35f, a1, Random.value) * Mathf.Deg2Rad;
                Vector3 dir = x * Mathf.Cos(a) + y * Mathf.Sin(a);
                Vector3 along = Vector3.Cross(n, dir);
                sparks.Emit(new ParticleSystem.EmitParams
                {
                    position = c + dir * r * Random.Range(.85f, 1.05f),
                    velocity = (dir * Random.Range(1.5f, 4f) + along * Random.Range(1f, 3f) + Vector3.up * Random.Range(.2f, 1.5f)),
                    startColor = Color.Lerp(tint, Color.white, Random.Range(.2f, .7f)),
                    startSize = Random.Range(.05f, .12f),
                    startLifetime = Random.Range(.25f, .5f),
                }, 1);
            }
        }

        void OnDisable() { foreach (var s in slashes) s?.Hide(); count = 0; }
        void OnDestroy() { foreach (var s in slashes) s?.Dispose(); }

        // One crescent: three strips on the same arc. A solid coloured body (normal blending, so it reads even on a
        // bright stone floor), a glow on top of it (additive) and a thin white core near the outer edge.
        sealed class Corte
        {
            static Texture2D texture; static Material material, solid;
            readonly GameObject root; readonly Mesh body, glow, core; readonly MeshRenderer bodyView, glowView, coreView;
            readonly Vector3[] bv = new Vector3[(Segments + 1) * 2], gv = new Vector3[(Segments + 1) * 2], cv = new Vector3[(Segments + 1) * 2];
            readonly Color[] bc = new Color[(Segments + 1) * 2], gc = new Color[(Segments + 1) * 2], cc = new Color[(Segments + 1) * 2];
            Vector3 c, n, x, y; float a0, a1, r, w, life, born = -10; Color tint;

            public Corte(Transform owner)
            {
                root = new GameObject("Corte"); root.transform.SetParent(owner, false);
                body = Strip("CorteCorpo", out bodyView, Solid); glow = Strip("CorteBrilho", out glowView, Material); core = Strip("CorteNucleo", out coreView, Material);
                glowView.sortingOrder = 1; coreView.sortingOrder = 2;
                Hide();
            }
            Mesh Strip(string name, out MeshRenderer view, Material look)
            {
                var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(root.transform, false);
                var mesh = new Mesh { name = name }; mesh.MarkDynamic();
                var uv = new Vector2[(Segments + 1) * 2]; var tri = new int[Segments * 6];
                for (int i = 0; i <= Segments; i++)
                {
                    uv[i * 2] = new Vector2(i / (float)Segments, 0); uv[i * 2 + 1] = new Vector2(i / (float)Segments, 1);
                    if (i == Segments) continue;
                    int t = i * 6, v = i * 2; tri[t] = v; tri[t + 1] = v + 1; tri[t + 2] = v + 2; tri[t + 3] = v + 1; tri[t + 4] = v + 3; tri[t + 5] = v + 2;
                }
                mesh.vertices = new Vector3[(Segments + 1) * 2]; mesh.uv = uv; mesh.triangles = tri;
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                view = go.GetComponent<MeshRenderer>(); view.sharedMaterial = look; view.shadowCastingMode = ShadowCastingMode.Off; view.receiveShadows = false;
                view.lightProbeUsage = LightProbeUsage.Off; view.reflectionProbeUsage = ReflectionProbeUsage.Off;
                return mesh;
            }

            // Across the strip (v): glow rising to a peak near the outer edge, then a sharp cut; thin streaks along it.
            static Material Material
            {
                get
                {
                    if (material != null) return material;
                    const int W = 8, H = 64;
                    texture = new Texture2D(W, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "CorteTextura" };
                    var rnd = new System.Random(7);
                    for (int j = 0; j < H; j++)
                    {
                        float v = j / (H - 1f);
                        float a = v < .82f ? Mathf.Pow(v / .82f, 1.4f) : Mathf.Pow(1 - (v - .82f) / .18f, .6f);
                        a *= .8f + .2f * (float)rnd.NextDouble();
                        for (int i = 0; i < W; i++) texture.SetPixel(i, j, new Color(1, 1, 1, Mathf.Clamp01(a)));
                    }
                    texture.Apply(false, true);
                    material = new Material(EfeitosDeHabilidade.Brilho) { name = "CorteDaEspada" };
                    material.SetTexture("_BaseMap", texture); material.mainTexture = texture;
                    material.SetFloat("_Cull", 0); // a flat strip: seen from both sides (the swing plane can face away)
                    return material;
                }
            }
            static Material Solid
            {
                get
                {
                    if (solid != null) return solid;
                    solid = new Material(Material) { name = "CorteDaEspadaCorpo" };
                    solid.SetFloat("_Blend", 0);
                    solid.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); solid.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    solid.renderQueue = Material.renderQueue - 1;
                    return solid;
                }
            }

            public void Begin(Vector3 center, Vector3 normal, Vector3 ax, Vector3 ay, float from, float to, float radius, float width, Color color, float seconds)
            {
                c = center; n = normal; x = ax; y = ay; a0 = from; a1 = to; r = radius; w = width; tint = color; life = seconds; born = Time.time;
                root.SetActive(true); Tick();
            }
            public void Hide() { born = -10; if (root != null) root.SetActive(false); }

            public void Tick()
            {
                if (!root.activeSelf) return;
                float t = (Time.time - born) / life;
                if (t >= 1) { Hide(); return; }
                float head = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / .3f));      // the slash draws itself quickly...
                float tail = Mathf.Clamp01((t - .2f) / .8f);                       // ...then erases from its start
                float grow = 1 + .12f * Mathf.Sqrt(t), thin = 1 - .45f * t, fade = 1 - t * t;
                for (int i = 0; i <= Segments; i++)
                {
                    float u = i / (float)Segments;
                    float a = Mathf.Lerp(a0, a1, u) * Mathf.Deg2Rad;
                    Vector3 dir = x * Mathf.Cos(a) + y * Mathf.Sin(a);
                    // Crescent: pointed ends, thickest a little after the middle (toward the leading edge).
                    float shape = Mathf.Sin(Mathf.PI * Mathf.Pow(u, 1.6f));
                    float outer = r * grow, wide = w * shape * thin;
                    bv[i * 2] = root.transform.InverseTransformPoint(c + dir * (outer - wide)); bv[i * 2 + 1] = root.transform.InverseTransformPoint(c + dir * outer);
                    float glowW = wide * .7f;
                    gv[i * 2] = root.transform.InverseTransformPoint(c + dir * (outer - glowW)); gv[i * 2 + 1] = root.transform.InverseTransformPoint(c + dir * outer);
                    float coreW = wide * .24f, coreOut = outer - wide * .04f;
                    cv[i * 2] = root.transform.InverseTransformPoint(c + dir * (coreOut - coreW)); cv[i * 2 + 1] = root.transform.InverseTransformPoint(c + dir * coreOut);
                    float seen = u > head ? 0 : Mathf.Clamp01((u - tail) / .25f) * (.55f + .45f * u);
                    float edge = Mathf.Clamp01((head - u) / .08f + .35f);                // the leading edge glows brighter
                    float k = seen * fade * edge;
                    bc[i * 2] = bc[i * 2 + 1] = new Color(tint.r * .7f, tint.g * .75f, tint.b * .9f, Mathf.Clamp01(k * 1.1f));
                    gc[i * 2] = gc[i * 2 + 1] = new Color(tint.r, tint.g, tint.b, k * .7f);
                    cc[i * 2] = cc[i * 2 + 1] = new Color(1, 1, 1, k);
                }
                body.vertices = bv; body.colors = bc; body.RecalculateBounds();
                glow.vertices = gv; glow.colors = gc; glow.RecalculateBounds();
                core.vertices = cv; core.colors = cc; core.RecalculateBounds();
            }

            public void Dispose() { if (body != null) Object.Destroy(body); if (glow != null) Object.Destroy(glow); if (core != null) Object.Destroy(core); }
        }
    }
}
