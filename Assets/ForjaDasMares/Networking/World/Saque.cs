using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ForjaDasMares.Networking
{
    // Loot on the ground: everything that drops jumps out in a slow arc, lands, and stays softly lit until
    // the player clicks it (or presses F near it). The colour of the light says what it is:
    // mission item = pale, washed-out lilac (never gold), material = soft green, coins = warm yellow,
    // equipment = its rarity (common white, uncommon green, rare blue, epic violet, legendary GOLD).
    // Gold belongs to legendary gear only.
    public static class Saque
    {
        // Colours of the mission signs over people ("!" / "?"): gold = main story, copper = side mission.
        public static readonly Color Principal = new Color(1f, .8f, .35f), Secundaria = new Color(.95f, .52f, .28f);
        public static readonly Color ItemDeMissao = new Color(.78f, .74f, .88f);
        public static readonly Color Material = new Color(.6f, 1f, .85f), Ouro = new Color(1f, .86f, .3f);
        public static Color Raridade(ItemRarity r) =>
            r == ItemRarity.Common ? new Color(.95f, .95f, .92f) : r == ItemRarity.Uncommon ? new Color(.4f, 1f, .45f) :
            r == ItemRarity.Rare ? new Color(.35f, .6f, 1f) : r == ItemRarity.Epic ? new Color(.78f, .42f, 1f) : new Color(1f, .78f, .28f);
        // A resource that a mission in progress still asks for glows in the soft mission colour.
        public static Color CorDoRecurso(string id) => Missoes.QuemPrecisa(id) == null ? Material : ItemDeMissao;

        // Where it lands: a little away from where it came from, on walkable ground.
        public static Vector3 Pouso(Vector3 from)
        {
            for (int i = 0; i < 6; i++)
            {
                var dir = Random.insideUnitCircle.normalized * Random.Range(.7f, 1.6f);
                var p = from + new Vector3(dir.x, 0, dir.y);
                if (NavMesh.SamplePosition(p, out var hit, 1.2f, NavMesh.AllAreas)) return hit.position;
            }
            return NavMesh.SamplePosition(from, out var near, 2f, NavMesh.AllAreas) ? near.position : from;
        }

        public static void SoltarRecurso(string id, int amount, Vector3 from)
        {
            if (amount <= 0) return;
            var at = Pouso(from);
            var root = new GameObject("Saque_" + id); root.transform.position = at;
            var visual = new GameObject("Visual").transform; visual.SetParent(root.transform, false);
            Modelo(visual, id);
            var color = CorDoRecurso(id);
            var chao = root.AddComponent<SaqueNoChao>(); chao.Brilho(visual, color);
            Cair(visual, from + Vector3.up * .9f - at);
            var box = root.AddComponent<BoxCollider>(); box.center = Vector3.up * .3f; box.size = new Vector3(.8f, .6f, .8f); WorldClickTarget.Atravessavel(box);
            var target = root.AddComponent<WorldClickTarget>();
            string nome = Recursos.Nome(id);
            target.Configure("Pegar " + (amount > 1 ? amount + " " : "") + nome, 2.2f);
            target.HoverLabel = (amount > 1 ? amount + " " : "") + nome;
            target.Used.AddListener(() =>
            {
                if (!Recursos.Ganhar(id, amount, root.transform.position)) return;
                EfeitosDeHabilidade.Coluna(root.transform.position, .35f, color);
                Object.Destroy(root);
            });
            chao.Target = target; WorldClickTarget.Extras.Add(target);
        }

        // The arc: starts at offset (local), ends on the ground.
        public static void Cair(Transform visual, Vector3 offset)
        {
            if (visual == null) return;
            var q = visual.GetComponent<QuedaDeSaque>(); if (q == null) q = visual.gameObject.AddComponent<QuedaDeSaque>();
            q.Begin(offset);
        }

        // Simple shapes for each resource until there are models.
        static readonly Dictionary<Color, UnityEngine.Material> materials = new Dictionary<Color, UnityEngine.Material>();
        static UnityEngine.Material Mat(Color c)
        {
            if (materials.TryGetValue(c, out var m) && m != null) return m;
            m = new UnityEngine.Material(Shader.Find("Universal Render Pipeline/Lit")); m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", .25f);
            materials[c] = m; return m;
        }
        static void Peca(Transform parent, PrimitiveType type, Vector3 pos, Vector3 rot, Vector3 scale, Color c)
        {
            var go = GameObject.CreatePrimitive(type); Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localEulerAngles = rot; go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>(); r.sharedMaterial = Mat(c); r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        static void Modelo(Transform v, string id)
        {
            if (id == Recursos.Carne)
            {
                Peca(v, PrimitiveType.Sphere, new Vector3(0, .12f, 0), Vector3.zero, new Vector3(.34f, .2f, .26f), new Color(.62f, .2f, .18f));
                Peca(v, PrimitiveType.Cylinder, new Vector3(.2f, .12f, 0), new Vector3(0, 0, 90), new Vector3(.06f, .1f, .06f), new Color(.95f, .92f, .85f));
            }
            else if (id == Recursos.Couro)
            {
                Peca(v, PrimitiveType.Cube, new Vector3(0, .05f, 0), new Vector3(0, 20, 0), new Vector3(.44f, .04f, .32f), new Color(.58f, .4f, .24f));
                Peca(v, PrimitiveType.Cube, new Vector3(.04f, .1f, .02f), new Vector3(0, -15, 8), new Vector3(.34f, .04f, .24f), new Color(.5f, .34f, .2f));
            }
            else if (id == Recursos.Chifre)
            {
                Peca(v, PrimitiveType.Capsule, new Vector3(-.06f, .14f, 0), new Vector3(0, 0, 35), new Vector3(.07f, .2f, .07f), new Color(.92f, .87f, .72f));
                Peca(v, PrimitiveType.Capsule, new Vector3(.08f, .2f, 0), new Vector3(0, 0, -25), new Vector3(.05f, .12f, .05f), new Color(.92f, .87f, .72f));
            }
            else Peca(v, PrimitiveType.Sphere, Vector3.up * .12f, Vector3.zero, Vector3.one * .22f, Recursos.Cor(id));
        }
    }

    // The glow of something lying on the ground: a soft light that breathes, a small bright core and a few
    // rising sparks. Not too strong: it calls attention without blinding.
    public sealed class SaqueNoChao : MonoBehaviour
    {
        public WorldClickTarget Target;
        public float Vida = 180;
        Light glow; float phase, born;
        public void Brilho(Transform visual, Color c)
        {
            glow = new GameObject("Luz").AddComponent<Light>(); glow.transform.SetParent(visual, false); glow.transform.localPosition = Vector3.up * .45f;
            glow.type = LightType.Point; glow.color = c; glow.range = 1.8f; glow.intensity = .55f; glow.shadows = LightShadows.None;
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(core.GetComponent<Collider>());
            core.transform.SetParent(visual, false); core.transform.localPosition = Vector3.up * .32f; core.transform.localScale = Vector3.one * .07f;
            var r = core.GetComponent<Renderer>(); r.sharedMaterial = EfeitosDeHabilidade.Brilho;
            var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", c); r.SetPropertyBlock(block);
            EfeitosDeHabilidade.Aura(visual, c, true, .28f, 3);
            phase = Random.value * 6; born = Time.time;
        }
        void Update()
        {
            if (glow != null) glow.intensity = .45f + .2f * Mathf.Sin(Time.time * 2.2f + phase);
            if (Time.time - born > Vida) Destroy(gameObject);
        }
        void OnDestroy() { if (Target != null) WorldClickTarget.Extras.Remove(Target); }
    }

    // The jump: up and out, a slow hang at the top (like slow motion), a little bounce, then a gentle float.
    public sealed class QuedaDeSaque : MonoBehaviour
    {
        const float Duracao = 1.15f, Altura = 1.1f;
        Vector3 offset; float t; bool landed; Quaternion spin;
        public void Begin(Vector3 start) { offset = start; t = 0; landed = false; transform.localPosition = start; spin = Quaternion.Euler(Random.Range(200f, 360f), Random.Range(100f, 300f), 0); }
        void Update()
        {
            t += Time.deltaTime;
            if (!landed)
            {
                float s = Mathf.Clamp01(t / Duracao);
                // Fast at the ends, slow at the top.
                float u = s + .6f * Mathf.Sin(2 * Mathf.PI * s) / (2 * Mathf.PI);
                var p = Vector3.Lerp(offset, Vector3.zero, u); p.y += 4 * Altura * u * (1 - u);
                transform.localPosition = p;
                transform.localRotation = Quaternion.Slerp(Quaternion.identity, spin, 1 - u);
                if (s >= 1) { landed = true; t = 0; transform.localPosition = Vector3.zero; transform.localRotation = Quaternion.identity; }
                return;
            }
            // Small bounce, then a soft float and a slow turn.
            float bounce = t < .3f ? Mathf.Sin(t / .3f * Mathf.PI) * .12f : 0;
            float drift = t > .3f ? Mathf.Sin((t - .3f) * 2f) * .03f + .03f : 0;
            transform.localPosition = Vector3.up * (bounce + drift);
            transform.localRotation = Quaternion.Euler(0, (t - .3f) * 25f, 0);
        }
    }
}
