using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ForjaDasMares
{
    // A blow as read from the animation (CortesDaEspada): the plane of the swing, the arc the blade covered,
    // the reach and which move it was (0-2 combo, 3 charged, 4 super).
    public struct Golpe
    {
        public Vector3 centro, normal, x, y; public float inicio, fim, raio, tamanho; public int variante; public Transform heroi;
        public Vector3 Direcao(float graus) { float a = graus * Mathf.Deg2Rad; return x * Mathf.Cos(a) + y * Mathf.Sin(a); }
        public Vector3 Frente => Vector3.ProjectOnPlane(heroi.forward, Vector3.up).normalized;
        public Vector3 Chao => heroi.position;
    }

    // Special weapons have their own look, made one by one (decided with Fabricio: each weapon is unique,
    // no general rule). A weapon without a style uses the plain slash of CortesDaEspada.
    public abstract class EstiloDeArma
    {
        public abstract void Golpe(CortesDaEspada cortes, Golpe g);
        public abstract void Acerto(Vector3 onde, Quaternion frente, bool forte);
        public virtual void Segurando(CortesDaEspada cortes, bool naMao, Vector3 ponta) { }
    }

    public static class EstilosDeArma
    {
        static readonly Dictionary<string, EstiloDeArma> porId = new Dictionary<string, EstiloDeArma>();
        static EquipmentDefinition ultimo; static EstiloDeArma ultimoEstilo;
        public static EstiloDeArma Para(EquipmentDefinition arma)
        {
            if (arma == null || string.IsNullOrEmpty(arma.itemId)) return null;
            if (arma == ultimo) return ultimoEstilo;
            ultimo = arma;
            if (!porId.TryGetValue(arma.itemId, out ultimoEstilo))
            {
                ultimoEstilo = arma.itemId.StartsWith("netuno") ? new PerdicaoDeNetuno() : null;
                porId[arma.itemId] = ultimoEstilo;
            }
            return ultimoEstilo;
        }
    }

    // Shared pieces for hand-made effects: materials, textures, simple meshes and a little animator.
    public static class Vfx
    {
        static Texture2D faixa; static Material aditivo, solido, aditivoPonto, solidoPonto;
        // Across v: rises to a bright band near the edge, then a soft cut (same profile as the slashes).
        public static Texture2D Faixa
        {
            get
            {
                if (faixa != null) return faixa;
                const int H = 64; faixa = new Texture2D(4, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "VfxFaixa" };
                for (int j = 0; j < H; j++)
                {
                    float v = j / (H - 1f); float a = v < .82f ? Mathf.Pow(v / .82f, 1.4f) : Mathf.Pow(1 - (v - .82f) / .18f, .6f);
                    for (int i = 0; i < 4; i++) faixa.SetPixel(i, j, new Color(1, 1, 1, a));
                }
                faixa.Apply(false, true); return faixa;
            }
        }
        static Material Make(Texture tex, bool additive)
        {
            var m = new Material(EfeitosDeHabilidade.Brilho) { name = additive ? "VfxAditivo" : "VfxSolido" };
            m.SetTexture("_BaseMap", tex); m.mainTexture = tex; m.SetFloat("_Cull", 0);
            if (!additive) { m.SetFloat("_Blend", 0); m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); m.renderQueue = 3000; }
            else m.renderQueue = 3001;
            return m;
        }
        public static Material Aditivo => aditivo != null ? aditivo : aditivo = Make(Faixa, true);
        public static Material Solido => solido != null ? solido : solido = Make(Faixa, false);
        static Texture Ponto => EfeitosDeHabilidade.Brilho.GetTexture("_BaseMap");
        public static Material AditivoPonto => aditivoPonto != null ? aditivoPonto : aditivoPonto = Make(Ponto, true);
        public static Material SolidoPonto => solidoPonto != null ? solidoPonto : solidoPonto = Make(Ponto, false);

        public static MeshRenderer Peca(Transform parent, string name, Mesh mesh, Material look)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.GetComponent<MeshRenderer>(); r.sharedMaterial = look; r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off; r.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return r;
        }
        static MaterialPropertyBlock block;
        public static void Cor(Renderer r, Color c) { block ??= new MaterialPropertyBlock(); block.SetColor("_BaseColor", c); r.SetPropertyBlock(block); }

        // Flat ring on the ground (radius 1, width as a fraction), v across the width for the band texture.
        static readonly Dictionary<int, Mesh> rings = new Dictionary<int, Mesh>();
        public static Mesh Anel(float largura)
        {
            int key = Mathf.RoundToInt(largura * 100);
            if (rings.TryGetValue(key, out var m) && m != null) return m;
            const int S = 48; var v = new Vector3[(S + 1) * 2]; var uv = new Vector2[v.Length]; var t = new int[S * 6];
            for (int i = 0; i <= S; i++)
            {
                float a = i / (float)S * Mathf.PI * 2; var d = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                v[i * 2] = d * (1 - largura); v[i * 2 + 1] = d; uv[i * 2] = new Vector2(i / (float)S, 0); uv[i * 2 + 1] = new Vector2(i / (float)S, 1);
                if (i == S) continue; int k = i * 6, b = i * 2; t[k] = b; t[k + 1] = b + 1; t[k + 2] = b + 2; t[k + 3] = b + 1; t[k + 4] = b + 3; t[k + 5] = b + 2;
            }
            m = new Mesh { name = "VfxAnel", vertices = v, uv = uv, triangles = t }; m.RecalculateBounds();
            rings[key] = m; return m;
        }

        // A crystal: long six-sided point, deep blue at the root, white at the tip.
        static Mesh cristal;
        public static Mesh Cristal
        {
            get
            {
                if (cristal != null) return cristal;
                var v = new List<Vector3> { new Vector3(0, -.1f, 0) }; var c = new List<Color> { new Color(.05f, .3f, .75f, .95f) };
                for (int i = 0; i < 6; i++) { float a = i * Mathf.PI / 3; v.Add(new Vector3(Mathf.Cos(a) * .16f, .28f, Mathf.Sin(a) * .16f)); c.Add(new Color(.35f, .85f, 1f, .95f)); }
                v.Add(new Vector3(0, 1.25f, 0)); c.Add(new Color(.92f, 1f, 1f, 1f));
                var t = new List<int>();
                for (int i = 0; i < 6; i++) { int a = 1 + i, b = 1 + (i + 1) % 6; t.AddRange(new[] { 0, b, a, a, b, 7 }); }
                var uv = new Vector2[v.Count]; for (int i = 0; i < uv.Length; i++) uv[i] = new Vector2(.5f, .82f);
                cristal = new Mesh { name = "VfxCristal" }; cristal.SetVertices(v); cristal.SetColors(c); cristal.uv = uv; cristal.SetTriangles(t, 0); cristal.RecalculateBounds();
                return cristal;
            }
        }

        // A short-lived effect: calls back every frame with t = 0..1, then removes itself (and the meshes it made).
        public sealed class Animado : MonoBehaviour
        {
            public float vida = 1, nascido; public System.Action<float> passo; public readonly List<Object> meus = new List<Object>();
            public static Animado Criar(string name, Vector3 onde, float vida, System.Action<float> passo)
            {
                var go = new GameObject(name); go.transform.position = onde;
                var a = go.AddComponent<Animado>(); a.vida = vida; a.nascido = Time.time; a.passo = passo; return a;
            }
            void Update()
            {
                float t = (Time.time - nascido) / Mathf.Max(.01f, vida);
                if (t >= 1) { Destroy(gameObject); return; }
                passo?.Invoke(t);
            }
            void OnDestroy() { foreach (var o in meus) if (o != null) Destroy(o); }
        }

        // A particle system ready for manual bursts (Emit), world space.
        public static ParticleSystem Particulas(Transform parent, string name, Material look, float gravity, bool stretch, int max = 160)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var s = go.AddComponent<ParticleSystem>(); s.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = s.main; main.loop = false; main.playOnAwake = false; main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = max; main.gravityModifier = gravity;
            var emission = s.emission; emission.enabled = false;
            var col = s.colorOverLifetime; col.enabled = true;
            var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(.8f, .6f), new GradientAlphaKey(0, 1) }); col.color = g;
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = look; r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            if (stretch) { r.renderMode = ParticleSystemRenderMode.Stretch; r.velocityScale = .05f; r.lengthScale = 1.6f; }
            s.Play(); return s;
        }
        public static void Gota(ParticleSystem s, Vector3 p, Vector3 v, Color c, float size, float life)
            => s.Emit(new ParticleSystem.EmitParams { position = p, velocity = v, startColor = c, startSize = size, startLifetime = life }, 1);
    }
}
