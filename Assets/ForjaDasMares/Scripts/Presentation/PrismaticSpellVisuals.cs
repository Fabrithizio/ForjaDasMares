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
        readonly float[] ages = new float[Capacity];
        readonly int[] kinds = new int[Capacity];
        readonly Vector3[] sizes = new Vector3[Capacity];
        Mesh shard, pulse, cage, fracture;
        Texture2D texture;
        Material surface;
        int next;

        public void Prepare(Material template)
        {
            if (surface != null) return;
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

        public void Burst(Vector3 position, float radius, int kind)
        {
            int slot = next;
            next = (next + 1) % Capacity;
            var body = bursts[slot];
            body.GetComponent<MeshFilter>().sharedMesh = kind == 0 ? fracture : kind == 1 ? pulse : cage;
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
                float duration = kind == 2 ? .85f : .48f;
                if (t >= duration) { body.gameObject.SetActive(false); continue; }
                float grow = Mathf.SmoothStep(.04f,1,Mathf.Clamp01(t / (kind == 1 ? .28f : .13f)));
                float sink = 1 - Mathf.SmoothStep(0,1,Mathf.InverseLerp(duration * .62f,duration,t));
                body.localScale = Vector3.Scale(sizes[i],new Vector3(grow,grow * sink,grow));
                if (kind == 0) body.Rotate(70 * Time.deltaTime, 100 * Time.deltaTime, 0);
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
                Vector3 axis = kind == 0 ? Vector3.forward : (Vector3.up + radial * (kind == 2 ? -.24f : .75f)).normalized;
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
            Destroy(surface); Destroy(texture);
        }
    }
}
