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
        ParticleSystem hearts, sparkles, halo;
        HeartFxStyle style;

        public void Play(HeartFxStyle value)
        {
            style = value;
            if (root == null) Build();
            root.gameObject.SetActive(true);

            ribbon.sharedMaterial = style.trail; core.sharedMaterial = style.trailCore != null ? style.trailCore : style.trail;
            ribbon.textureMode = LineTextureMode.Tile; core.textureMode = LineTextureMode.Stretch;
            ribbon.time = style.trailTime; core.time = style.trailTime * .6f;
            ribbon.widthCurve = new AnimationCurve(new Keyframe(0, style.trailWidth), new Keyframe(.35f, style.trailWidth * .8f), new Keyframe(1, 0));
            core.widthCurve = new AnimationCurve(new Keyframe(0, style.trailWidth * .38f), new Keyframe(1, 0));
            ribbon.colorGradient = MakeGradient(style.main, style.second, .95f, 0f);
            core.colorGradient = MakeGradient(Color.Lerp(style.main, Color.white, .7f), style.main, 1f, 0f);
            ribbon.Clear(); core.Clear(); ribbon.emitting = core.emitting = style.trailWidth > 0;

            foreach (var system in new[] { hearts, sparkles, halo }) system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Setup(hearts, style.heart, style.heartsPerMetre, 0, new Vector2(.55f, .9f), style.heartSize, true);
            Setup(sparkles, style.glow, style.sparklesPerMetre, 0, new Vector2(.3f, .5f), new Vector2(.05f, .13f), false);
            Setup(halo, style.glow, 0, style.haloSize > 0 ? 40 : 0, new Vector2(.1f, .14f), Vector2.one * style.haloSize, false);
            var haloMain = halo.main; haloMain.simulationSpace = ParticleSystemSimulationSpace.Local;
            var haloColor = halo.main; haloColor.startColor = new Color(style.main.r, style.main.g, style.main.b, .45f);
            foreach (var system in new[] { hearts, sparkles, halo }) { system.Clear(); system.Play(); }
        }

        public void Stop()
        {
            if (root == null) return;
            ribbon.emitting = core.emitting = false;
            foreach (var system in new[] { hearts, sparkles, halo }) system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            root.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            if (root == null) return;
            ribbon.Clear(); core.Clear();
            foreach (var system in new[] { hearts, sparkles, halo }) system.Clear();
        }

        void Build()
        {
            // The projectile spins very fast, so the effects live on a child whose rotation is reset every frame.
            root = new GameObject("EfeitoCoracao").transform;
            root.SetParent(transform, false);
            ribbon = Trail("Faixa"); core = Trail("Nucleo");
            hearts = MakeSystem("Coracoes"); sparkles = MakeSystem("Brilhos"); halo = MakeSystem("Aura");
        }

        void LateUpdate()
        {
            if (root == null || !root.gameObject.activeSelf) return;
            root.rotation = Quaternion.identity;
            var parent = transform.lossyScale;
            root.localScale = new Vector3(1 / Mathf.Max(.0001f, parent.x), 1 / Mathf.Max(.0001f, parent.y), 1 / Mathf.Max(.0001f, parent.z));
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
        sealed class Burst { public GameObject go; public ParticleSystem hearts, flash, ring, big; }
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
                bursts.Add(burst);
            }
            else burst = bursts[nextBurst++ % bursts.Count];
            burst.go.transform.position = position;
            float s = Mathf.Max(.3f, style.impactSize);

            // Hearts flying out and floating up.
            Configure(burst.hearts, style.heart, style, style.impactHearts, new Vector2(.6f, 1f), new Vector2(.22f, .45f) * s, new Vector2(2f, 4.5f) * s, -.35f, false);
            var shape = burst.hearts.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = .3f;
            // A quick bright flash.
            Configure(burst.flash, style.glow, style, 2, new Vector2(.18f, .25f), new Vector2(1.6f, 2.4f) * s, Vector2.zero, 0, false);
            // A ring opening on the ground.
            Configure(burst.ring, style.ring, style, 1, new Vector2(.4f, .4f), new Vector2(1f, 1f) * s, Vector2.zero, 0, true);
            var grow = burst.ring.sizeOverLifetime; grow.enabled = true; grow.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .4f), new Keyframe(1, 3.2f)));
            // The ring lies on the ground under the hit.
            float ground = Physics.Raycast(position + Vector3.up * .3f, Vector3.down, out var floor, 4f, (1 << 8) | 1, QueryTriggerInteraction.Ignore) ? floor.point.y + .06f : position.y - .85f;
            burst.ring.transform.position = new Vector3(position.x, ground, position.z);
            // Final version: a big heart rising from the target.
            Configure(burst.big, style.heart, style, style.bigHeart ? 1 : 0, new Vector2(.55f, .55f), new Vector2(1.1f, 1.1f) * s, new Vector2(1.2f, 1.2f), 0, false);
            var big = burst.big.shape; big.enabled = true; big.shapeType = ParticleSystemShapeType.Cone; big.angle = 0; big.radius = .01f;
            burst.big.transform.rotation = Quaternion.LookRotation(Vector3.up);

            foreach (var system in new[] { burst.hearts, burst.flash, burst.ring, burst.big }) { system.Clear(); system.Play(); }
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
}
