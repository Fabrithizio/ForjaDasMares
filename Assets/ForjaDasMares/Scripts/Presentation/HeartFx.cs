using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ForjaDasMares
{
    // Heart weapon effects. On the flying weapon: a thick glowing ribbon, a bright core, hearts and sparkles
    // left in the air and a halo. On contact: a burst of hearts, a flash and a ring on the ground.
    // Everything is created once per pooled projectile and reused.
    public sealed class HeartFx : MonoBehaviour
    {
        Transform root;
        TrailRenderer ribbon, core;
        ParticleSystem hearts, sparkles, halo, petals;
        TrailRenderer spiralGold, spiralViolet;
        Vector3 lastPosition, travel = Vector3.forward;
        float spiralPhase;
        HeartFxStyle style;

        public void Play(HeartFxStyle value)
        {
            style = value;
            if (root == null) Build();
            root.gameObject.SetActive(true);

            ribbon.sharedMaterial = style.trail; core.sharedMaterial = style.trailCore != null ? style.trailCore : style.trail;
            ribbon.textureMode = LineTextureMode.Stretch; core.textureMode = LineTextureMode.Stretch;
            ribbon.time = style.trailTime; core.time = style.trailTime * .6f;
            ribbon.widthCurve = new AnimationCurve(new Keyframe(0, style.trailWidth), new Keyframe(.35f, style.trailWidth * .8f), new Keyframe(1, 0));
            core.widthCurve = new AnimationCurve(new Keyframe(0, style.trailWidth * .3f), new Keyframe(.6f, style.trailWidth * .18f), new Keyframe(1, 0));
            ribbon.colorGradient = MakeGradient(style.main, style.second, .95f, 0f);
            core.colorGradient = MakeGradient(style.core, Color.Lerp(style.core, style.main, .5f), 1f, 0f);
            ribbon.Clear(); core.Clear(); ribbon.emitting = core.emitting = style.trailWidth > 0;

            foreach (var system in new[] { hearts, sparkles, halo }) system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Setup(hearts, style.heart, style.heartsPerMetre, 0, new Vector2(.55f, .9f), style.heartSize, true);
            if (Mixed(style)) { var heartMain = hearts.main; heartMain.startColor = HeartColors(style); }
            Setup(sparkles, style.glow, style.sparklesPerMetre, 0, new Vector2(.3f, .5f), new Vector2(.05f, .13f), false);
            Setup(halo, style.glow, 0, style.haloSize > 0 ? 40 : 0, new Vector2(.1f, .14f), Vector2.one * style.haloSize, false);
            var haloMain = halo.main; haloMain.simulationSpace = ParticleSystemSimulationSpace.Local;
            var haloColor = halo.main; haloColor.startColor = new Color(style.main.r, style.main.g, style.main.b, .3f);
            var sparkleColor = sparkles.main; sparkleColor.startColor = new ParticleSystem.MinMaxGradient(style.sparkle, Color.Lerp(style.sparkle, Color.white, .5f));
            foreach (var system in new[] { hearts, sparkles, halo }) { system.Clear(); system.Play(); }

            // Legendary layers.
            bool epic = style.legendary;
            foreach (var spiral in new[] { spiralGold, spiralViolet })
            {
                spiral.sharedMaterial = style.trailCore != null ? style.trailCore : style.trail;
                spiral.textureMode = LineTextureMode.Stretch;
                spiral.time = style.trailTime * 1.25f;
                spiral.widthCurve = new AnimationCurve(new Keyframe(0, style.trailWidth * .2f), new Keyframe(1, 0));
                spiral.Clear(); spiral.emitting = epic;
            }
            spiralGold.colorGradient = MakeGradient(style.sparkle, style.core, 1f, 0f);
            spiralViolet.colorGradient = MakeGradient(Color.Lerp(style.second, Color.white, .25f), style.second, 1f, 0f);
            petals.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Setup(petals, style.petal != null ? style.petal : style.heart, epic ? 1.4f : 0, 0, new Vector2(.9f, 1.4f), new Vector2(.12f, .22f), false);
            var petalMain = petals.main; petalMain.gravityModifier = .18f; petalMain.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            var spin = petals.rotationOverLifetime; spin.enabled = epic; spin.z = new ParticleSystem.MinMaxCurve(-3f, 3f);
            if (epic) petals.Play();
            lastPosition = transform.position; spiralPhase = 0;

        }

        public void Stop()
        {
            if (root == null) return;
            ribbon.emitting = core.emitting = spiralGold.emitting = spiralViolet.emitting = false;
            foreach (var system in new[] { hearts, sparkles, halo, petals }) system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            root.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            if (root == null) return;
            ribbon.Clear(); core.Clear(); spiralGold.Clear(); spiralViolet.Clear();
            foreach (var system in new[] { hearts, sparkles, halo, petals }) system.Clear();
        }

        void Build()
        {
            // The projectile spins very fast, so the effects live on a child whose rotation is reset every frame.
            root = new GameObject("EfeitoCoracao").transform;
            root.SetParent(transform, false);
            ribbon = Trail("Faixa"); core = Trail("Nucleo");
            hearts = MakeSystem("Coracoes"); sparkles = MakeSystem("Brilhos"); halo = MakeSystem("Aura");
            spiralGold = Trail("EspiralOuro"); spiralViolet = Trail("EspiralVioleta"); petals = MakeSystem("Petalas");
        }

        void LateUpdate()
        {
            if (root == null || !root.gameObject.activeSelf) return;
            root.rotation = Quaternion.identity;
            var parent = transform.lossyScale;
            root.localScale = new Vector3(1 / Mathf.Max(.0001f, parent.x), 1 / Mathf.Max(.0001f, parent.y), 1 / Mathf.Max(.0001f, parent.z));
            if (style == null || !style.legendary) return;
            // Two ribbons spiralling around the flight path, like the drawing.
            Vector3 moved = transform.position - lastPosition; moved.y = 0;
            if (moved.sqrMagnitude > .00001f) travel = moved.normalized;
            lastPosition = transform.position;
            spiralPhase += Time.deltaTime * 13f;
            Vector3 side = Vector3.Cross(Vector3.up, travel);
            float radius = style.trailWidth * .6f;
            Vector3 a = (side * Mathf.Cos(spiralPhase) + Vector3.up * Mathf.Sin(spiralPhase) * .55f) * radius;
            spiralGold.transform.position = transform.position + a;
            spiralViolet.transform.position = transform.position - a;
        }

        TrailRenderer Trail(string name)
        {
            var go = new GameObject(name); go.transform.SetParent(root, false);
            var trail = go.AddComponent<TrailRenderer>();
            trail.minVertexDistance = .08f; trail.numCapVertices = 4; trail.numCornerVertices = 2;
            trail.alignment = LineAlignment.View; trail.textureMode = LineTextureMode.Stretch;
            trail.shadowCastingMode = ShadowCastingMode.Off; trail.receiveShadows = false; trail.autodestruct = false;
            return trail;
        }

        ParticleSystem MakeSystem(string name)
        {
            var go = new GameObject(name); go.transform.SetParent(root, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            renderer.sortingFudge = -10;
            return system;
        }

        void Setup(ParticleSystem system, Material material, float perMetre, float perSecond, Vector2 life, Vector2 size, bool rising)
        {
            system.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            var main = system.main;
            main.loop = true; main.playOnAwake = false; main.maxParticles = 120;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life.x, life.y);
            main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.startSpeed = rising ? new ParticleSystem.MinMaxCurve(.2f, .8f) : new ParticleSystem.MinMaxCurve(0, .3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(-.35f, .35f);
            main.startColor = new ParticleSystem.MinMaxGradient(style.main, Color.Lerp(style.main, style.second, .7f));
            main.gravityModifier = rising ? -.25f : 0;
            var emission = system.emission;
            emission.rateOverDistance = perMetre; emission.rateOverTime = perSecond;
            var shape = system.shape;
            shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = Mathf.Max(.05f, style.trailWidth * .3f);
            var fade = system.colorOverLifetime; fade.enabled = true;
            fade.color = MakeGradient(Color.white, Color.white, 1, 0);
            var grow = system.sizeOverLifetime; grow.enabled = true;
            grow.size = new ParticleSystem.MinMaxCurve(1, rising ? new AnimationCurve(new Keyframe(0, .4f), new Keyframe(.2f, 1.1f), new Keyframe(1, .8f)) : new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, .3f)));
        }

        static Gradient MakeGradient(Color start, Color end, float startAlpha, float endAlpha)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(start, 0), new GradientColorKey(end, 1) },
                new[] { new GradientAlphaKey(startAlpha, 0), new GradientAlphaKey(startAlpha * .7f, .4f), new GradientAlphaKey(endAlpha, 1) });
            return g;
        }

        // ---------- Burst on contact (shared pool) ----------
        sealed class Burst { public GameObject go; public ParticleSystem hearts, flash, ring, big, rays, rune; }
        static readonly List<Burst> bursts = new List<Burst>();
        static int nextBurst;
        static Transform burstRoot;

        public static void Impact(Vector3 position, HeartFxStyle style)
        {
            if (style == null) return;
            if (burstRoot == null)
            {
                bursts.Clear();
                burstRoot = new GameObject("EfeitosCoracaoImpacto").transform;
            }
            Burst burst;
            if (bursts.Count < 10)
            {
                burst = new Burst { go = new GameObject("Impacto") };
                burst.go.transform.SetParent(burstRoot, false);
                burst.hearts = Child(burst.go.transform, "Coracoes"); burst.flash = Child(burst.go.transform, "Clarao");
                burst.ring = Child(burst.go.transform, "Anel"); burst.big = Child(burst.go.transform, "CoracaoGrande");
                burst.rays = Child(burst.go.transform, "Raios"); burst.rune = Child(burst.go.transform, "Runa");
                bursts.Add(burst);
            }
            else burst = bursts[nextBurst++ % bursts.Count];
            burst.go.transform.position = position;
            float s = Mathf.Max(.3f, style.impactSize);

            // Hearts flying out and floating up.
            Configure(burst.hearts, style.heart, style, style.impactHearts, new Vector2(.6f, 1f), style.impactHeartSize * s, new Vector2(2f, 4.5f) * s, -.35f, false);
            if (Mixed(style)) { var mixedMain = burst.hearts.main; mixedMain.startColor = HeartColors(style); }
            var shape = burst.hearts.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = .3f;
            // A quick bright flash.
            Configure(burst.flash, style.glow, style, 2, new Vector2(.18f, .25f), new Vector2(1.6f, 2.4f) * s, Vector2.zero, 0, false);
            // A ring opening on the ground.
            Configure(burst.ring, style.ring, style, style.legendary ? 0 : 1, new Vector2(.4f, .4f), new Vector2(1f, 1f) * s, Vector2.zero, 0, true);
            var grow = burst.ring.sizeOverLifetime; grow.enabled = true; grow.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .4f), new Keyframe(1, 3.2f)));
            // The ring lies on the ground under the hit.
            float ground = style.legendary ? Ground(position) + .06f : Physics.Raycast(position + Vector3.up * .3f, Vector3.down, out var floor, 4f, (1 << 8) | 1, QueryTriggerInteraction.Ignore) ? floor.point.y + .06f : position.y - .85f;
            burst.ring.transform.position = new Vector3(position.x, ground, position.z);
            // Final version: a big heart rising from the target.
            Configure(burst.big, style.heart, style, style.bigHeart ? 1 : 0, new Vector2(.55f, .55f), Vector2.one * style.bigHeartSize * s, new Vector2(1.2f, 1.2f), 0, false);
            var big = burst.big.shape; big.enabled = true; big.shapeType = ParticleSystemShapeType.Cone; big.angle = 0; big.radius = .01f;
            burst.big.transform.rotation = Quaternion.LookRotation(Vector3.up);

            // Legendary: rays of golden light and a rune circle on the ground.
            bool epic = style.legendary;
            Configure(burst.rays, style.glow, style, epic ? 10 : 0, new Vector2(.2f, .3f), new Vector2(.14f, .24f), new Vector2(7f, 11f), 0, false);
            var raysRenderer = burst.rays.GetComponent<ParticleSystemRenderer>();
            raysRenderer.renderMode = ParticleSystemRenderMode.Stretch; raysRenderer.lengthScale = 2.5f; raysRenderer.velocityScale = .12f;
            var raysMain = burst.rays.main; raysMain.startColor = new ParticleSystem.MinMaxGradient(style.sparkle, Color.white);
            var raysShape = burst.rays.shape; raysShape.enabled = true; raysShape.shapeType = ParticleSystemShapeType.Sphere; raysShape.radius = .1f;
            var raysLimit = burst.rays.limitVelocityOverLifetime; raysLimit.enabled = false;
            // No magic circle under the enemy: it only opens at the hero's feet when throwing.
            Configure(burst.rune, style.rune, style, 0, new Vector2(.75f, .75f), Vector2.one * 2.4f, Vector2.zero, 0, true);
            RuneLook(burst.rune, style);
            burst.rune.transform.position = new Vector3(position.x, ground + .02f, position.z);
            if (epic && Mixed(style)) { var bigMain = burst.big.main; bigMain.startColor = HeartColors(style); }

            foreach (var system in new[] { burst.hearts, burst.flash, burst.ring, burst.big, burst.rays, burst.rune }) { system.Clear(); system.Play(); }
        }

        // Magic circle that spins open under the hero when the legendary weapon is thrown.
        static readonly List<ParticleSystem> runes = new List<ParticleSystem>();
        static int nextRune;
        static Transform runeRoot;
        // "onGround" = the position is already on the floor (the hero's feet).
        public static void Rune(Vector3 position, HeartFxStyle style, float size, bool onGround = false)
        {
            if (style == null || style.rune == null) return;
            if (runeRoot == null) { runes.Clear(); runeRoot = new GameObject("EfeitosCoracaoRunas").transform; }
            ParticleSystem rune;
            if (runes.Count < 4) { rune = Child(runeRoot, "Runa"); runes.Add(rune); }
            else rune = runes[nextRune++ % runes.Count];
            float ground = (onGround ? position.y : Ground(position)) + .05f;
            rune.transform.position = new Vector3(position.x, ground, position.z);
            Configure(rune, style.rune, style, 1, new Vector2(.8f, .8f), Vector2.one * size, Vector2.zero, 0, true);
            RuneLook(rune, style);
            rune.Clear(); rune.Play();
        }

        static void RuneLook(ParticleSystem rune, HeartFxStyle style)
        {
            var main = rune.main;
            main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            main.startColor = new ParticleSystem.MinMaxGradient(style.sparkle, Color.Lerp(style.sparkle, style.main, .5f));
            var turn = rune.rotationOverLifetime; turn.enabled = true; turn.z = 2.2f;
            var grow = rune.sizeOverLifetime; grow.enabled = true; grow.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .55f), new Keyframe(.25f, 1f), new Keyframe(1, 1.08f)));
            var fade = rune.colorOverLifetime; fade.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .15f), new GradientAlphaKey(.85f, .6f), new GradientAlphaKey(0, 1) });
            fade.color = g;
        }

        // Floor height under a point. The paved plaza has no collider (people walk on the NavMesh),
        // so the walkable mesh is checked first, then any solid collider.
        static float Ground(Vector3 position)
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(position, out var hit, 3f, UnityEngine.AI.NavMesh.AllAreas)) return hit.position.y;
            if (Physics.Raycast(position + Vector3.up * .3f, Vector3.down, out var floor, 4f, (1 << 8) | 1, QueryTriggerInteraction.Ignore)) return floor.point.y;
            return position.y - .85f;
        }

        public static bool Mixed(HeartFxStyle style) => style.heartColors != null && style.heartColors.Length > 0;

        // Each heart picks one of the colours (no blending between them).
        public static ParticleSystem.MinMaxGradient HeartColors(HeartFxStyle style)
        {
            var colors = style.heartColors;
            int n = Mathf.Min(colors.Length, 8);
            var keys = new GradientColorKey[n];
            for (int i = 0; i < n; i++) keys[i] = new GradientColorKey(colors[i], (i + 1f) / n);
            var g = new Gradient { mode = GradientMode.Fixed };
            g.SetKeys(keys, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
            return new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };
        }

        static ParticleSystem Child(Transform parent, string name)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            return system;
        }

        static void Configure(ParticleSystem system, Material material, HeartFxStyle style, int count, Vector2 life, Vector2 size, Vector2 speed, float gravity, bool flat)
        {
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = flat ? ParticleSystemRenderMode.HorizontalBillboard : ParticleSystemRenderMode.Billboard;
            var main = system.main;
            main.loop = false; main.playOnAwake = false; main.duration = 1f; main.maxParticles = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life.x, life.y);
            main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
            main.startRotation = new ParticleSystem.MinMaxCurve(-.4f, .4f);
            main.startColor = new ParticleSystem.MinMaxGradient(style.main, Color.Lerp(style.main, style.second, .6f));
            main.gravityModifier = gravity;
            var emission = system.emission;
            emission.rateOverTime = 0; emission.rateOverDistance = 0;
            emission.SetBursts(count > 0 ? new[] { new ParticleSystem.Burst(0, (short)count) } : new ParticleSystem.Burst[0]);
            var shape = system.shape; shape.enabled = false;
            var fade = system.colorOverLifetime; fade.enabled = true;
            fade.color = MakeGradient(Color.white, Color.white, 1, 0);
            var limit = system.limitVelocityOverLifetime; limit.enabled = speed.y > 0; limit.limit = .4f; limit.dampen = .12f;
        }
    }

    // Aura of the legendary weapon while it is in the hand: hearts drifting up, golden sparkles and a soft light.
    public sealed class HeldHeartAura : MonoBehaviour
    {
        Transform root;
        ParticleSystem hearts, sparkles, glow;
        Renderer weapon;
        bool on;

        public void Play(HeartFxStyle style)
        {
            weapon = GetComponent<Renderer>();
            if (root == null)
            {
                root = new GameObject("AuraLendaria").transform;
                root.SetParent(transform, false);
                hearts = Make("Coracoes"); sparkles = Make("Brilhos"); glow = Make("Luz");
            }
            root.gameObject.SetActive(true);
            Setup(hearts, style.heart, 1.4f, new Vector2(1.3f, 1.9f), new Vector2(.12f, .2f), new Vector2(.15f, .35f), -.12f, style.main, style.second, ParticleSystemSimulationSpace.World, .25f);
            if (HeartFx.Mixed(style)) { var heartMain = hearts.main; heartMain.startColor = HeartFx.HeartColors(style); }
            Setup(sparkles, style.glow, 9f, new Vector2(.5f, .9f), new Vector2(.04f, .09f), new Vector2(0f, .2f), -.05f, style.sparkle, Color.white, ParticleSystemSimulationSpace.World, .3f);
            Setup(glow, style.glow, 18f, new Vector2(.18f, .22f), new Vector2(.75f, .9f), Vector2.zero, 0, new Color(style.main.r, style.main.g, style.main.b, .2f), new Color(style.sparkle.r, style.sparkle.g, style.sparkle.b, .16f), ParticleSystemSimulationSpace.Local, .01f);
            on = true;
            foreach (var s in new[] { hearts, sparkles, glow }) { s.Clear(); s.Play(); }
        }

        public void Stop()
        {
            on = false;
            if (root == null) return;
            foreach (var s in new[] { hearts, sparkles, glow }) s.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            root.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (root == null || !on) return;
            root.rotation = Quaternion.identity;
            var parent = transform.lossyScale;
            root.localScale = new Vector3(1 / Mathf.Max(.0001f, parent.x), 1 / Mathf.Max(.0001f, parent.y), 1 / Mathf.Max(.0001f, parent.z));
            // While the weapon is flying the hand copy is hidden, so the aura pauses too.
            bool visible = weapon == null || weapon.enabled;
            foreach (var s in new[] { hearts, sparkles, glow }) { var e = s.emission; e.enabled = visible; }
        }

        ParticleSystem Make(string name)
        {
            var go = new GameObject(name); go.transform.SetParent(root, false);
            var s = go.AddComponent<ParticleSystem>();
            s.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            return s;
        }

        static void Setup(ParticleSystem s, Material material, float rate, Vector2 life, Vector2 size, Vector2 speed, float gravity, Color a, Color b, ParticleSystemSimulationSpace space, float radius)
        {
            s.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            s.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            var main = s.main;
            main.loop = true; main.playOnAwake = false; main.maxParticles = 60; main.simulationSpace = space;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life.x, life.y);
            main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
            main.startColor = new ParticleSystem.MinMaxGradient(a, b);
            main.gravityModifier = gravity;
            var emission = s.emission; emission.rateOverTime = rate; emission.rateOverDistance = 0;
            var shape = s.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = radius;
            var fade = s.colorOverLifetime; fade.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .2f), new GradientAlphaKey(0, 1) });
            fade.color = g;
        }

    }
}
