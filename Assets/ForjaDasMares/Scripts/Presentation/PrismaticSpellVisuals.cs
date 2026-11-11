using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ForjaDasMares
{
    // The spell meshes are presentation only. They never add colliders or deal damage.
    public sealed class PrismaticSpellVisuals : MonoBehaviour
    {
        const int Capacity = 8;
        readonly Transform[] bursts = new Transform[Capacity];
        readonly float[] ages = new float[Capacity], lifetimes = new float[Capacity];
        readonly int[] kinds = new int[Capacity];
        readonly Vector3[] sizes = new Vector3[Capacity];
        Mesh shard, pulse, cage, fracture, volcano, wind, barrier;
        Texture2D texture;
        Material surface;
        MaterialPropertyBlock tint;
        int next;

        public void Prepare(Material template)
        {
            if (surface != null) return;
            tint = new MaterialPropertyBlock();
            texture = new Texture2D(64, 64, TextureFormat.RGBA32, true) { name = "Prismatic crystal veins", wrapMode = TextureWrapMode.Repeat };
            var pixels = new Color32[64 * 64];
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float vein = Mathf.Abs(Mathf.Sin((x + 8 * Mathf.Sin(y * .16f)) * .28f));
                float grain = Mathf.PerlinNoise(x * .24f, y * .11f);
                float value = .38f + .28f * grain + .34f * Mathf.Pow(1 - vein, 6);
                pixels[y * 64 + x] = Color.Lerp(new Color(.06f,.22f,.4f), new Color(.79f,.98f,1), value);
            }
            texture.SetPixels32(pixels);
            texture.Apply(true, true);
            surface = new Material(template) { name = "Veined prism surface" };
            surface.SetTexture("_BaseMap", texture);
            surface.SetColor("_BaseColor", new Color(.75f,.9f,1));
            surface.SetFloat("_Smoothness", .68f);
            surface.EnableKeyword("_EMISSION");
            surface.SetColor("_EmissionColor", new Color(.10f,.25f,.38f));
            shard = Build(0, 1);
            pulse = Build(1, 16);
            cage = Build(2, 12);
            fracture = Build(3, 7);
            volcano = Build(4, Application.isMobilePlatform ? 24 : 40);
            wind = Build(5, Application.isMobilePlatform ? 28 : 48);
            barrier = Build(6, Application.isMobilePlatform ? 24 : 40);
            for (int i = 0; i < Capacity; i++)
            {
                var go = new GameObject("Prismatic spell visual", typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(transform, false);
                var renderer = go.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = surface;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                bursts[i] = go.transform;
                go.SetActive(false);
            }
        }

        public void ShapeLance(MeshFilter filter, MeshRenderer renderer)
        {
            filter.sharedMesh = shard;
            renderer.sharedMaterial = surface;
        }

        public void Burst(Vector3 position, float radius, int kind, float lifetime = 0)
        {
            int slot = next;
            next = (next + 1) % Capacity;
            var body = bursts[slot];
            body.GetComponent<MeshFilter>().sharedMesh = kind switch
            {
                0 => fracture, 1 => pulse, 3 => volcano, 4 => wind, 5 => barrier, _ => cage
            };
            Color color = kind == 3 ? new Color(1,.37f,.05f) : kind == 4 ? new Color(.32f,1,.46f) :
                kind == 5 ? new Color(.30f,.76f,1) : new Color(.75f,.9f,1);
            tint.SetColor("_BaseColor", color);
            tint.SetColor("_EmissionColor", color * .6f);
            body.GetComponent<MeshRenderer>().SetPropertyBlock(tint);
            lifetimes[slot] = lifetime > 0 ? lifetime : kind == 2 ? .85f : .48f;
            ages[slot] = 0;
            kinds[slot] = kind;
            sizes[slot] = kind == 0 ? new Vector3(radius,radius,radius) : new Vector3(radius, kind == 1 ? .7f : 1.55f,radius);
            body.SetPositionAndRotation(position, Quaternion.identity);
            body.localScale = Vector3.one * .03f;
            body.gameObject.SetActive(true);
        }

        void Update()
        {
            for (int i = 0; i < Capacity; i++)
            {
                var body = bursts[i];
                if (body == null || !body.gameObject.activeSelf) continue;
                int kind = kinds[i];
                float t = ages[i] += Time.deltaTime;
                float duration = lifetimes[i];
                if (t >= duration) { body.gameObject.SetActive(false); continue; }
                float grow = Mathf.SmoothStep(.04f,1,Mathf.Clamp01(t / (kind == 1 ? .28f : .13f)));
                float sink = 1 - Mathf.SmoothStep(0,1,Mathf.InverseLerp(duration * .62f,duration,t));
                body.localScale = Vector3.Scale(sizes[i],new Vector3(grow,grow * sink,grow));
                if (kind == 0) body.Rotate(70 * Time.deltaTime, 100 * Time.deltaTime, 0);
                if (kind == 4 || kind == 5) body.Rotate(0, (kind == 4 ? 250 : 48) * Time.deltaTime, 0, Space.World);
            }
        }

        static Mesh Build(int kind, int count)
        {
            var vertices = new List<Vector3>(count * 36);
            var uv = new List<Vector2>(count * 36);
            var triangles = new List<int>(count * 36);
            void Face(Vector3 a, Vector3 b, Vector3 c, int side)
            {
                int start = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c);
                float shift = side * .14f;
                uv.Add(new Vector2(shift,0)); uv.Add(new Vector2(shift+.25f,.38f)); uv.Add(new Vector2(shift+.12f,1));
                triangles.Add(start); triangles.Add(start+1); triangles.Add(start+2);
            }
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2 / count;
                var radial = new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                var center = kind == 0 ? Vector3.zero : radial * (kind == 2 ? .72f : kind == 1 ? .78f : .28f);
                float width = kind == 0 ? .55f : kind == 2 ? .08f : .13f;
                float height = kind == 0 ? 2.8f : kind == 2 ? .64f + .12f * (i % 4) : .25f + .13f * (i % 3);
                if (kind >= 4)
                {
                    float band = (i % 5) / 4f;
                    center = kind == 4 ? radial * Mathf.Lerp(.24f,.94f,band) :
                        kind == 5 ? radial * Mathf.Lerp(.45f,.88f,band) + Vector3.up * (.22f + .75f * band) :
                        radial * Mathf.Lerp(.86f,.27f,band) + Vector3.up * (.08f + .88f * band);
                    width = kind == 4 ? .045f : .036f;
                    height = kind == 4 ? .30f + band * .32f : .29f;
                }
                Vector3 axis = kind == 0 ? Vector3.forward : (Vector3.up + radial * (kind == 2 ? -.24f : .75f)).normalized;
                if (kind == 5 || kind == 6)
                    axis = (Vector3.Cross(Vector3.up,radial) + Vector3.up * .23f).normalized;
                Vector3 basis = Vector3.Cross(axis,kind == 0 ? Vector3.up : radial).normalized;
                Vector3 other = Vector3.Cross(axis,basis);
                for (int side = 0; side < 6; side++)
                {
                    float a = side * Mathf.PI / 3, b = (side+1) * Mathf.PI / 3;
                    Vector3 p = center + (basis*Mathf.Cos(a)+other*Mathf.Sin(a))*width;
                    Vector3 q = center + (basis*Mathf.Cos(b)+other*Mathf.Sin(b))*width;
                    Face(p,q,center+axis*height,side);
                    Face(q,p,center-axis*(kind == 0 ? .9f : height*.18f),side+1);
                }
            }
            if (kind == 2 || kind == 6)
            {
                int segments = Application.isMobilePlatform ? 12 : 20;
                int rows = 4;
                Vector3 Point(int row, int column)
                {
                    float pitch = row * Mathf.PI / (2 * rows);
                    float yaw = (column + (row % 2) * .5f) * 2 * Mathf.PI / segments;
                    return new Vector3(Mathf.Cos(yaw) * Mathf.Cos(pitch), 1.35f * Mathf.Sin(pitch),
                        Mathf.Sin(yaw) * Mathf.Cos(pitch));
                }
                void Edge(Vector3 a, Vector3 b)
                {
                    Vector3 width = Vector3.Cross((a + b).normalized, b - a).normalized * .008f;
                    Face(a - width, b - width, b + width, 1);
                    Face(a - width, b + width, a + width, 1);
                    Face(a - width, b + width, b - width, 1);
                    Face(a - width, a + width, b + width, 1);
                }
                for (int row = 0; row < rows; row++)
                for (int column = 0; column < segments; column++)
                {
                    Vector3 a = Point(row,column);
                    Edge(a,Point(row,column+1));
                    Edge(a,Point(row+1,column));
                    if (row < rows - 1) Edge(a,Point(row+1,column+(row % 2 == 0 ? -1 : 1)));
                }
            }
            var mesh = new Mesh { name = "Prismatic facets " + kind };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0,uv);
            mesh.SetTriangles(triangles,0);
            mesh.RecalculateNormals();
            return mesh;
        }

        void OnDestroy()
        {
            Destroy(shard); Destroy(pulse); Destroy(cage); Destroy(fracture);
            Destroy(volcano); Destroy(wind); Destroy(barrier);
            Destroy(surface); Destroy(texture);
        }
    }
}
