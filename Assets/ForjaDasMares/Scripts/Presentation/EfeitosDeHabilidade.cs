using UnityEngine;
using UnityEngine.Rendering;

namespace ForjaDasMares
{
    // Visual effects of the abilities, made in code (bright and colourful, decided with Fabricio).
    // Sparks, a ring on the ground, a flash of light, rising particles and looping auras on whoever has a status.
    // When the Cartoon FX pack is imported, its effects are added on top of these (see Extra).
    public static class EfeitosDeHabilidade
    {
        static Material glow;
        public static Material Brilho
        {
            get
            {
                if (glow != null) return glow;
                glow = Resources.Load<Material>("Efeitos/BrilhoAditivo");
                if (glow == null) glow = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                return glow;
            }
        }

        public static Color Cor(Elemento e) =>
            e == Elemento.Fogo ? new Color(1f, .45f, .12f) :
            e == Elemento.Gelo ? new Color(.55f, .9f, 1f) :
            e == Elemento.Raio ? new Color(1f, .92f, .25f) :
            e == Elemento.Vento ? new Color(.8f, 1f, .9f) :
            e == Elemento.Agua ? new Color(.2f, .55f, 1f) :
            e == Elemento.Seiva ? new Color(.45f, 1f, .35f) : Color.white;

        // Big hit: sparks flying out, a ring growing on the ground and a flash of light.
        public static void Explosao(Vector3 p, float radius, Color c, int sparks = 45)
        {
            var root = new GameObject("Efeito");
            root.transform.position = p;
            Faiscas(root.transform, c, sparks, radius);
            Anel(root.transform, c, radius);
            Clarao(root.transform, c, radius);
            Tremer(.035f);
            Object.Destroy(root, 2.5f);
        }

        // Rising light (heal, shield, power-up) around a body.
        public static void Coluna(Vector3 p, float radius, Color c)
        {
            var root = new GameObject("Efeito");
            root.transform.position = p;
            var s = Sistema(root.transform, "Subindo");
            var main = s.main; main.duration = .8f; main.loop = false; main.startLifetime = new ParticleSystem.MinMaxCurve(.6f, 1.1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.2f); main.startSize = new ParticleSystem.MinMaxCurve(.12f, .3f);
            main.startColor = new ParticleSystem.MinMaxGradient(c, Color.Lerp(c, Color.white, .6f)); main.simulationSpace = ParticleSystemSimulationSpace.World;
            var shape = s.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = radius; shape.rotation = new Vector3(90, 0, 0);
            var emission = s.emission; emission.rateOverTime = 0; emission.SetBursts(new[] { new ParticleSystem.Burst(0, 40), new ParticleSystem.Burst(.25f, 25) });
            var vel = s.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World; vel.y = new ParticleSystem.MinMaxCurve(1.5f, 2.5f); vel.x = new ParticleSystem.MinMaxCurve(0, 0); vel.z = new ParticleSystem.MinMaxCurve(0, 0);
            Fade(s); Shrink(s);
            s.Play();
            Anel(root.transform, c, radius * 1.2f);
            Clarao(root.transform, c, radius * .8f);
            Object.Destroy(root, 2.5f);
        }

        // A streak of light between two points (dash, thrown seed, spark).
        public static void Rastro(Vector3 from, Vector3 to, Color c)
        {
            var root = new GameObject("Rastro");
            root.transform.position = from;
            var line = root.AddComponent<LineRenderer>();
            line.sharedMaterial = Brilho; line.positionCount = 2; line.SetPosition(0, from); line.SetPosition(1, to);
            line.widthMultiplier = .35f; line.startColor = c; line.endColor = Color.Lerp(c, Color.white, .5f);
            line.shadowCastingMode = ShadowCastingMode.Off; line.receiveShadows = false;
            root.AddComponent<SumirLinha>();
            var s = Sistema(root.transform, "Faiscas");
            var main = s.main; main.duration = .3f; main.loop = false; main.startLifetime = new ParticleSystem.MinMaxCurve(.3f, .6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.5f, 2f); main.startSize = new ParticleSystem.MinMaxCurve(.08f, .2f); main.startColor = c; main.simulationSpace = ParticleSystemSimulationSpace.World;
            float distance = Mathf.Max(.1f, Vector3.Distance(from, to));
            s.transform.position = (from + to) * .5f; s.transform.rotation = Quaternion.LookRotation((to - from) / distance);
            var shape = s.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(.3f, .3f, distance);
            var emission = s.emission; emission.rateOverTime = 0; emission.SetBursts(new[] { new ParticleSystem.Burst(0, 30) });
            Fade(s); Shrink(s); s.Play();
            Object.Destroy(root, 1.2f);
        }

        // Looping effect on a body while a status lasts (burning, marked, rooted, shield...).
        public static ParticleSystem Aura(Transform body, Color c, bool rising, float radius = .5f, float rate = 16)
        {
            var s = Sistema(body, "Aura");
            s.transform.localPosition = Vector3.up * (rising ? .3f : 1.1f);
            var main = s.main; main.loop = true; main.duration = 1; main.startLifetime = new ParticleSystem.MinMaxCurve(.5f, .9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.1f, .5f); main.startSize = new ParticleSystem.MinMaxCurve(.1f, .24f);
            main.startColor = new ParticleSystem.MinMaxGradient(c, Color.Lerp(c, Color.white, .5f)); main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = 60;
            var shape = s.shape; shape.shapeType = rising ? ParticleSystemShapeType.Circle : ParticleSystemShapeType.Sphere; shape.radius = radius; if (rising) shape.rotation = new Vector3(90, 0, 0);
            var emission = s.emission; emission.rateOverTime = rate;
            if (rising) { var vel = s.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World; vel.x = new ParticleSystem.MinMaxCurve(0, 0); vel.z = new ParticleSystem.MinMaxCurve(0, 0); vel.y = new ParticleSystem.MinMaxCurve(.8f, 1.6f); }
            Fade(s); Shrink(s);
            s.Play();
            return s;
        }

        // ---------- Pieces ----------
        static void Faiscas(Transform root, Color c, int count, float radius)
        {
            var s = Sistema(root, "Faiscas");
            var main = s.main; main.duration = .5f; main.loop = false; main.startLifetime = new ParticleSystem.MinMaxCurve(.35f, .75f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(radius * 1.6f, radius * 3.6f); main.startSize = new ParticleSystem.MinMaxCurve(.12f, .32f);
            main.startColor = new ParticleSystem.MinMaxGradient(c, Color.Lerp(c, Color.white, .7f)); main.gravityModifier = .35f; main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = 200;
            var shape = s.shape; shape.shapeType = ParticleSystemShapeType.Hemisphere; shape.radius = .3f; shape.rotation = new Vector3(-90, 0, 0);
            var emission = s.emission; emission.rateOverTime = 0; emission.SetBursts(new[] { new ParticleSystem.Burst(0, (short)count) });
            Fade(s); Shrink(s);
            s.transform.localPosition = Vector3.up * .4f;
            s.Play();
        }
        static void Anel(Transform root, Color c, float radius)
        {
            var s = Sistema(root, "Anel");
            s.transform.localPosition = Vector3.up * .08f;
            var main = s.main; main.duration = .5f; main.loop = false; main.startLifetime = .45f; main.startSpeed = 0; main.startSize = radius * 2.4f;
            main.startColor = new Color(c.r, c.g, c.b, .9f); main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = s.emission; emission.rateOverTime = 0; emission.SetBursts(new[] { new ParticleSystem.Burst(0, 1) });
            var shape = s.shape; shape.enabled = false;
            var size = s.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, .2f, 1, 1));
            Fade(s);
            s.GetComponent<ParticleSystemRenderer>().renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            s.Play();
        }
        static void Clarao(Transform root, Color c, float radius)
        {
            var go = new GameObject("Clarao"); go.transform.SetParent(root, false); go.transform.localPosition = Vector3.up;
            var light = go.AddComponent<Light>(); light.type = LightType.Point; light.color = c; light.range = Mathf.Max(3, radius * 3); light.intensity = 5;
            go.AddComponent<ApagarLuz>();
        }
        static ParticleSystem Sistema(Transform parent, string name)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var s = go.AddComponent<ParticleSystem>(); s.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = Brilho; r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            return s;
        }
        static void Fade(ParticleSystem s)
        {
            var col = s.colorOverLifetime; col.enabled = true;
            var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(.8f, .6f), new GradientAlphaKey(0, 1) });
            col.color = g;
        }
        static void Shrink(ParticleSystem s)
        {
            var size = s.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 0));
        }

        // A light camera shake on strong moments (only if the player turned shaking on).
        public static void Tremer(float strength) => Object.FindFirstObjectByType<ArenaCamera>()?.Impact(strength);

        // Cartoon FX on top (the pack's prefabs, listed in Resources/Efeitos/CatalogoDeEfeitos by EfeitosInstaller).
        static CatalogoDeEfeitos catalogo; static bool procurou;
        static GameObject Prefab(string key)
        {
            if (!procurou) { procurou = true; catalogo = Resources.Load<CatalogoDeEfeitos>("Efeitos/CatalogoDeEfeitos"); }
            return catalogo != null ? catalogo.Find(key) : null;
        }
        public static void Extra(string key, Vector3 p, Color c) => Extra(key, p, Quaternion.identity, 1);
        public static void Extra(string key, Vector3 p, Quaternion rotation, float scale)
        {
            var prefab = Prefab(key);
            if (prefab == null) return;
            var go = Object.Instantiate(prefab, p, rotation);
            go.transform.localScale = Vector3.one * scale;
            Object.Destroy(go, 4);
        }
        // A looping pack effect that follows a body (removed by whoever created it).
        public static GameObject ExtraLoop(string key, Transform body, Vector3 offset, float scale = 1)
        {
            var prefab = Prefab(key);
            if (prefab == null) return null;
            var go = Object.Instantiate(prefab, body);
            go.transform.localPosition = offset; go.transform.localRotation = Quaternion.identity; go.transform.localScale = Vector3.one * scale;
            return go;
        }

        sealed class ApagarLuz : MonoBehaviour
        {
            Light l; float start;
            void Awake() { l = GetComponent<Light>(); start = l.intensity; }
            void Update() { l.intensity = Mathf.MoveTowards(l.intensity, 0, start * Time.deltaTime / .35f); if (l.intensity <= 0) enabled = false; }
        }
        sealed class SumirLinha : MonoBehaviour
        {
            LineRenderer line; float t;
            void Awake() => line = GetComponent<LineRenderer>();
            void Update() { t += Time.deltaTime / .35f; line.widthMultiplier = Mathf.Lerp(.35f, 0, t); if (t >= 1) enabled = false; }
        }
    }
}
