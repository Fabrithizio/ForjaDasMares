using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace ForjaDasMares.Networking.Editor
{
    // Builds the whole Nascente do Rio in its scene (Scenes/Areas/Area_Nascente), from the plan in NascenteDoRio:
    // ground with the river valley and the spring, the water, trees, grass, rocks, the small camps and props of each
    // corner, the creature camps (and the drowned captain at the spring) and the navigation. Can be run again:
    // everything it made is replaced.
    public static class NascenteInstaller
    {
        const string ScenePath = "Assets/ForjaDasMares/Scenes/Areas/Area_Nascente.unity";
        const string Data = "Assets/ForjaDasMares/Data/World/Areas/";
        const string Art = "Assets/ForjaDasMares/Art/";
        const float Half = 64, Ground = 2, Height = 32;
        static Terrain terrain;

        [MenuItem("Forja/Mundo/Montar a Nascente do Rio")]
        public static void Montar()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(ScenePath);
            foreach (var go in scene.GetRootGameObjects())
                if (go.name == "AreaEnvironment" || go.name == "AreaNavigation" || go.name == "NascenteMontada") Object.DestroyImmediate(go);
            var root = new GameObject("NascenteMontada").transform;
            var random = new System.Random(20261122);

            Terreno(root);
            Limites(root);
            Agua(root);
            Vegetacao(root, random);
            Pedras(root, random);
            Cantos(root, random);
            var world = Object.FindFirstObjectByType<RegionalWorld>();
            Regiao(world);
            Luz();
            bool ok = Navegacao();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("NASCENTE_MONTADA caminhos_ok=" + ok);
            EditorUtility.DisplayDialog("Forja · Nascente do Rio", ok ? "A Nascente do Rio está montada. Dê Play e vá pela saída sul do Refúgio."
                : "A Nascente foi montada, mas algum caminho ficou bloqueado. Veja o aviso amarelo no Console e me mande.", "OK");
        }

        // ---------- Ground ----------
        // The ground is bigger than the place (192 m): around the playable square (128 m) rise mountains and
        // forest, and the roads and the river go on through them, so there is never a dark gap at the edge.
        const float Outer = 96;
        static float S(float a, float b, float x) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(a, b, x));
        static float Ruido(Vector2 p, float scale, float seed) => Mathf.PerlinNoise((p.x + seed) * scale, (p.y + seed * 1.7f) * scale);

        static float Altura(Vector2 p)
        {
            float road = NascenteDoRio.AEstrada(p), rio = NascenteDoRio.AoRio(p), poco = Vector2.Distance(p, NascenteDoRio.Poco);
            float open = S(4, 12, road) * S(5, 13, rio) * S(NascenteDoRio.RaioDoPoco + 2, NascenteDoRio.RaioDoPoco + 10, poco);
            // Corners stay mostly flat (room to fight or to rest).
            foreach (var c in NascenteDoRio.Cantos) open *= S(c.raio * .55f, c.raio * 1.15f, Vector2.Distance(p, c.centro));
            float y = Ruido(p, .05f, 311) * 2.6f * open;
            // The rocks the spring comes out of.
            float m = Vector2.Distance(p, NascenteDoRio.Cachoeira);
            y += Mathf.Pow(Mathf.Clamp01(1 - m / 7.5f), 1.4f) * 5.2f;
            // Cliffs at the edge of the place and mountains beyond it, open where the roads and the river pass.
            // Near the exits the mountains hug the road: only the road goes on, with the arrow in the gap.
            float corridor = S(3.6f, 7.5f, road) * S(5, 11, rio);
            float edge = Half - Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y));
            if (edge < 14) y += Mathf.Pow(1 - Mathf.Clamp(edge, 0, 14) / 14, 1.3f) * 14 * corridor;
            if (edge < 0) y += (Mathf.Clamp01(-edge / 14) * 6 + Ruido(p, .07f, 57) * 5 * Mathf.Clamp01(-edge / 6)) * corridor;
            // River bed and the pool of the spring (the ford is shallow, it can be walked).
            float bank = Mathf.Min(rio - NascenteDoRio.LarguraDoRio * .5f, poco - NascenteDoRio.RaioDoPoco);
            if (bank < 1.8f)
            {
                bool vau = Vector2.Distance(p, NascenteDoRio.Vau) < NascenteDoRio.RaioDoVau + 1.5f;
                float fundo = vau ? -.42f : poco < NascenteDoRio.RaioDoPoco + 1 ? -1.6f : -1.15f;
                y = Mathf.Lerp(fundo, y, S(-1.6f, 1.8f, bank));
            }
            return y;
        }

        // How much of each ground goes at a point: Breu, rock, earth (grass is the rest).
        static (float breu, float rock, float dirt) Pintura(Vector2 p)
        {
            float road = 1 - S(1.8f, 3.4f, NascenteDoRio.AEstrada(p));
            float bank = Mathf.Min(NascenteDoRio.AoRio(p) - NascenteDoRio.LarguraDoRio * .5f, Vector2.Distance(p, NascenteDoRio.Poco) - NascenteDoRio.RaioDoPoco);
            float mud = 1 - S(0, 2.2f, bank);
            float edge = Half - Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y));
            float rock = Mathf.Max(Mathf.Clamp01(1 - edge / 9), 1 - S(2, 6.5f, Vector2.Distance(p, NascenteDoRio.Cachoeira))) * (1 - road);
            // Patches of soft earth across the grass (it should not look like one flat carpet).
            float patches = S(.62f, .74f, Ruido(p, .11f, 913)) * .7f;
            float dirt = Mathf.Max(road, Mathf.Max(mud * .85f, patches));
            float breu = 0;
            foreach (var c in NascenteDoRio.Cantos)
            {
                float d = Vector2.Distance(p, c.centro);
                if (c.tipo == NascenteDoRio.Tipo.Breu) breu = Mathf.Max(breu, 1 - S(c.raio * .8f, c.raio * 1.15f, d));
                else if (c.tipo == NascenteDoRio.Tipo.Perigo || c.tipo == NascenteDoRio.Tipo.Chefe)
                {
                    // Stains of Breu where the creatures gather; trampled earth around them.
                    float near = 1 - S(c.raio * .3f, c.raio * .95f, d);
                    breu = Mathf.Max(breu, near * S(.6f, .7f, Ruido(p, .2f, 71)) * .9f);
                    dirt = Mathf.Max(dirt, near * .5f);
                }
            }
            // The Breu at the spring stains its shore.
            breu = Mathf.Max(breu, (1 - S(0, 3.5f, Vector2.Distance(p, NascenteDoRio.Poco) - NascenteDoRio.RaioDoPoco)) * S(.45f, .6f, Ruido(p, .25f, 5)) * .8f);
            return (Mathf.Clamp01(breu * (1 - road)), rock, dirt);
        }

        // Tileable textures made here: earth, rock and Breu (the grass is the city's own).
        static Texture2D Textura(string name, System.Func<float, float, Color> pixel)
        {
            string path = Data + "Nascente_" + name + ".png";
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) tex.SetPixel(x, y, pixel(x / (float)size, y / (float)size));
            tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.wrapMode = TextureWrapMode.Repeat; importer.mipmapEnabled = true; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        // Value noise that repeats at the borders of the texture (u, v from 0 to 1).
        static float Rep(float u, float v, int cells, int seed)
        {
            float x = u * cells, y = v * cells; int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0; fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
            float H(int i, int j) { i = ((i % cells) + cells) % cells; j = ((j % cells) + cells) % cells; uint h = (uint)(i * 374761393 + j * 668265263 + seed * 982451653); h = (h ^ (h >> 13)) * 1274126177u; return (h & 0xffff) / 65535f; }
            return Mathf.Lerp(Mathf.Lerp(H(x0, y0), H(x0 + 1, y0), fx), Mathf.Lerp(H(x0, y0 + 1), H(x0 + 1, y0 + 1), fx), fy);
        }
        static float Fbm(float u, float v, int seed) => Rep(u, v, 4, seed) * .5f + Rep(u, v, 8, seed + 1) * .3f + Rep(u, v, 24, seed + 2) * .2f;

        static TerrainLayer Camada(string name, Texture2D tex, float tile, float smooth)
        {
            string path = Data + "Nascente_" + name + ".terrainlayer";
            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
            if (layer == null) { layer = new TerrainLayer(); AssetDatabase.CreateAsset(layer, path); }
            layer.diffuseTexture = tex; layer.tileSize = new Vector2(tile, tile); layer.smoothness = smooth; layer.metallic = 0;
            EditorUtility.SetDirty(layer); return layer;
        }

        static TerrainLayer[] Camadas()
        {
            var grama = AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "Cidade/_Chao/GramaTextura.png");
            if (grama == null) grama = Textura("Grama", (u, v) => Color.Lerp(new Color(.24f, .42f, .16f), new Color(.38f, .58f, .22f), Fbm(u, v, 7)));
            var terra = Textura("Terra", (u, v) =>
            {
                float n = Fbm(u, v, 3), fine = Rep(u, v, 64, 9);
                var c = Color.Lerp(new Color(.36f, .27f, .17f), new Color(.52f, .40f, .26f), n);
                if (fine > .86f) c = Color.Lerp(c, new Color(.66f, .58f, .46f), .6f); // little pebbles
                return c;
            });
            var pedra = Textura("Pedra", (u, v) =>
            {
                float n = Fbm(u, v, 17), crack = Mathf.Abs(Rep(u, v, 6, 23) - .5f);
                var c = Color.Lerp(new Color(.40f, .40f, .39f), new Color(.60f, .59f, .55f), n);
                if (crack < .025f) c *= .62f;
                return c;
            });
            var breu = Textura("Breu", (u, v) =>
            {
                float n = Fbm(u, v, 41), vein = Mathf.Abs(Rep(u, v, 5, 47) - .5f);
                var c = Color.Lerp(new Color(.07f, .03f, .1f), new Color(.16f, .07f, .21f), n);
                if (vein < .03f) c = Color.Lerp(c, new Color(.55f, .2f, .75f), 1 - vein / .03f); // glowing veins
                return c;
            });
            return new[] { Camada("Grama", grama, 7, 0), Camada("Terra", terra, 5, .05f), Camada("Pedra", pedra, 6, .15f), Camada("Breu", breu, 4, .6f) };
        }

        static void Terreno(Transform parent)
        {
            string path = Data + "nascente_Terreno.asset";
            var data = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
            if (data == null) { data = new TerrainData(); AssetDatabase.CreateAsset(data, path); }
            const int res = 257; // must be 2^n + 1
            data.heightmapResolution = res; data.alphamapResolution = 256; data.size = new Vector3(Outer * 2, Height, Outer * 2);
            data.terrainLayers = Camadas();
            var heights = new float[res, res];
            for (int z = 0; z < res; z++)
                for (int x = 0; x < res; x++)
                {
                    var p = new Vector2(-Outer + x * Outer * 2 / (res - 1), -Outer + z * Outer * 2 / (res - 1));
                    heights[z, x] = Mathf.Clamp01((Ground + Altura(p)) / Height);
                }
            data.SetHeights(0, 0, heights);
            const int a = 256;
            var alpha = new float[a, a, 4];
            for (int z = 0; z < a; z++)
                for (int x = 0; x < a; x++)
                {
                    var p = new Vector2(-Outer + x * Outer * 2 / (a - 1f), -Outer + z * Outer * 2 / (a - 1f));
                    var (breu, rock, dirt) = Pintura(p);
                    rock *= 1 - breu; dirt = Mathf.Min(dirt, 1 - breu - rock);
                    alpha[z, x, 3] = breu; alpha[z, x, 2] = rock; alpha[z, x, 1] = Mathf.Max(0, dirt);
                    alpha[z, x, 0] = Mathf.Max(0, 1 - breu - rock - Mathf.Max(0, dirt));
                }
            data.SetAlphamaps(0, 0, alpha);
            EditorUtility.SetDirty(data);
            var go = Terrain.CreateTerrainGameObject(data);
            go.name = "Terreno"; go.transform.SetParent(parent); go.transform.position = new Vector3(-Outer, -Ground, -Outer); go.layer = 8;
            terrain = go.GetComponent<Terrain>(); terrain.heightmapPixelError = 6; terrain.drawInstanced = true;
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Data/World/Terrain.mat");
            if (mat != null) terrain.materialTemplate = mat;
        }

        static float Chao(Vector2 p) => terrain.SampleHeight(new Vector3(p.x, 0, p.y)) + terrain.transform.position.y;

        // Invisible walls a few steps past the exit arrows: the arrow is the end of the way.
        const float Parede = Half - 5;
        static void Limites(Transform parent)
        {
            var holder = new GameObject("Limites").transform; holder.SetParent(parent, false);
            foreach (var (pos, size) in new[] {
                (new Vector3(0, 6, Parede), new Vector3(Parede * 2 + 2, 24, 1)), (new Vector3(0, 6, -Parede), new Vector3(Parede * 2 + 2, 24, 1)),
                (new Vector3(Parede, 6, 0), new Vector3(1, 24, Parede * 2 + 2)), (new Vector3(-Parede, 6, 0), new Vector3(1, 24, Parede * 2 + 2)) })
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "Limite"; wall.transform.SetParent(holder, false);
                wall.transform.position = pos; wall.transform.localScale = size; wall.layer = 9;
                Object.DestroyImmediate(wall.GetComponent<MeshRenderer>());
            }
        }

        // ---------- Water ----------
        static List<Vector2> RioSuave()
        {
            var pts = NascenteDoRio.Rio; var list = new List<Vector2>();
            for (int i = 0; i < pts.Length - 1; i++)
            {
                Vector2 p0 = pts[Mathf.Max(0, i - 1)], p1 = pts[i], p2 = pts[i + 1], p3 = pts[Mathf.Min(pts.Length - 1, i + 2)];
                int steps = Mathf.CeilToInt(Vector2.Distance(p1, p2));
                for (int s = 0; s < steps; s++)
                {
                    float t = s / (float)steps, t2 = t * t, t3 = t2 * t;
                    list.Add(.5f * (2 * p1 + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + (-p0 + 3 * p1 - 3 * p2 + p3) * t3));
                }
            }
            list.Add(pts[pts.Length - 1]);
            return list;
        }

        static Material Mat(string name, Color color, float smooth, Texture texture)
        {
            string path = Data + "Nascente_" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
            m.color = color; m.SetFloat("_Smoothness", smooth);
            if (texture != null) m.SetTexture("_BaseMap", texture);
            EditorUtility.SetDirty(m); return m;
        }

        static Mesh Salvar(Mesh mesh)
        {
            string path = Data + "Nascente_" + mesh.name + ".asset";
            var prior = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (prior == null) { AssetDatabase.CreateAsset(mesh, path); return mesh; }
            EditorUtility.CopySerialized(mesh, prior); return prior;
        }

        static GameObject Malha(Transform parent, string name, Mesh mesh, Material mat, Vector2 flow)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = Salvar(mesh);
            var r = go.GetComponent<MeshRenderer>(); r.sharedMaterial = mat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            // The water flows by itself when the game runs (RioAnimado finds it by its name).
            return go;
        }

        static void Agua(Transform parent)
        {
            var holder = new GameObject("Agua").transform; holder.SetParent(parent, false);
            var ripples = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ForjaDasMares/Data/World/Town/AguaRio.png");
            // The river gets darker towards the spring: the Breu is in the water.
            var rio = Mat("Rio", new Color(.36f, .6f, .74f), .9f, ripples);
            var escura = Mat("AguaDoBreu", new Color(.34f, .22f, .5f), .93f, ripples);
            var queda = Mat("Cachoeira", new Color(.62f, .78f, .9f), .85f, ripples);
            const float y = -.32f;

            var line = RioSuave();
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>(); var colors = new List<Color>();
            float along = 0; float half = NascenteDoRio.LarguraDoRio * .5f + .5f;
            for (int i = 0; i < line.Count; i++)
            {
                var dir = (line[Mathf.Min(line.Count - 1, i + 1)] - line[Mathf.Max(0, i - 1)]).normalized;
                var side = new Vector2(-dir.y, dir.x);
                if (i > 0) along += Vector2.Distance(line[i], line[i - 1]);
                var a = line[i] - side * half; var b = line[i] + side * half;
                v.Add(new Vector3(a.x, y, a.y)); v.Add(new Vector3(b.x, y, b.y));
                uv.Add(new Vector2(along / 6f, 0)); uv.Add(new Vector2(along / 6f, 1));
                if (i > 0) { int k = i * 2; tri.AddRange(new[] { k - 2, k - 1, k, k - 1, k + 1, k }); } // facing up
            }
            var strip = new Mesh { name = "Rio" }; strip.SetVertices(v); strip.SetUVs(0, uv); strip.SetTriangles(tri, 0); strip.RecalculateNormals(); strip.RecalculateBounds();
            Malha(holder, "Rio", strip, rio, new Vector2(-.12f, .01f));

            // Pool of the spring (dark, the Breu is here).
            var disc = new Mesh { name = "Poco" }; var dv = new List<Vector3> { new Vector3(NascenteDoRio.Poco.x, y + .01f, NascenteDoRio.Poco.y) }; var duv = new List<Vector2> { NascenteDoRio.Poco / 6f }; var dt = new List<int>();
            const int n = 40; float r = NascenteDoRio.RaioDoPoco + .9f;
            for (int i = 0; i <= n; i++)
            {
                float ang = i * Mathf.PI * 2 / n; var p = NascenteDoRio.Poco + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r;
                dv.Add(new Vector3(p.x, y + .01f, p.y)); duv.Add(p / 6f);
                if (i > 0) dt.AddRange(new[] { 0, i + 1, i });
            }
            disc.SetVertices(dv); disc.SetUVs(0, duv); disc.SetTriangles(dt, 0); disc.RecalculateNormals(); disc.RecalculateBounds();
            Malha(holder, "PocoDaNascente", disc, escura, new Vector2(.02f, .03f));

            // The small waterfall that comes out of the rocks, east of the pool.
            float top = Chao(NascenteDoRio.Cachoeira) - .3f;
            var fx = NascenteDoRio.Cachoeira.x - 3.2f; var fz = NascenteDoRio.Cachoeira.y;
            var fall = new Mesh { name = "Cachoeira" };
            fall.SetVertices(new List<Vector3> { new Vector3(fx, y, fz - 1.4f), new Vector3(fx, y, fz + 1.4f), new Vector3(fx + .8f, top, fz - 1.1f), new Vector3(fx + .8f, top, fz + 1.1f) });
            fall.SetUVs(0, new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 2), new Vector2(1, 2) });
            fall.SetTriangles(new[] { 0, 2, 1, 1, 2, 3, 0, 1, 2, 1, 3, 2 }, 0); fall.RecalculateNormals(); fall.RecalculateBounds();
            Malha(holder, "Cachoeira", fall, queda, new Vector2(0, .9f));

            // The water cannot be walked into (except the ford): marks on the navigation.
            var bloqueio = new GameObject("AguaBloqueio").transform; bloqueio.SetParent(holder, false);
            for (int i = 0; i < line.Count; i += 2)
            {
                if (Vector2.Distance(line[i], NascenteDoRio.Vau) < NascenteDoRio.RaioDoVau + .3f) continue;
                if (Vector2.Distance(line[i], NascenteDoRio.Poco) < NascenteDoRio.RaioDoPoco - 1) continue;
                var dir = (line[Mathf.Min(line.Count - 1, i + 1)] - line[Mathf.Max(0, i - 1)]).normalized;
                var go = new GameObject("Agua"); go.transform.SetParent(bloqueio, false);
                go.transform.position = new Vector3(line[i].x, 0, line[i].y); go.transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.y));
                var vol = go.AddComponent<NavMeshModifierVolume>(); vol.size = new Vector3(NascenteDoRio.LarguraDoRio - .6f, 5, 2.6f); vol.center = Vector3.zero; vol.area = 1;
            }
            foreach (float ang in new[] { 0f, 45f })
            {
                var go = new GameObject("PocoBloqueio"); go.transform.SetParent(bloqueio, false);
                go.transform.position = new Vector3(NascenteDoRio.Poco.x, 0, NascenteDoRio.Poco.y); go.transform.rotation = Quaternion.Euler(0, ang, 0);
                float side = NascenteDoRio.RaioDoPoco * 2 * .84f;
                var vol = go.AddComponent<NavMeshModifierVolume>(); vol.size = new Vector3(side, 5, side); vol.center = Vector3.zero; vol.area = 1;
            }
        }

        // ---------- Things on the ground ----------
        static GameObject Load(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(Art + path);

        static Bounds Caixa(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            var b = rs.Length > 0 ? rs[0].bounds : new Bounds(go.transform.position, Vector3.one);
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        // Puts a model on the ground at (x, z), turned and scaled to a height. Blocking ones get a box on layer 9.
        static GameObject Por(Transform parent, GameObject prefab, Vector2 at, float altura, float giro, bool bloqueia, float afundar = 0)
        {
            if (prefab == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0, giro, 0)); go.transform.localScale = Vector3.one;
            var b = Caixa(go);
            if (altura > 0 && b.size.y > .001f) { go.transform.localScale = Vector3.one * (altura / b.size.y); b = Caixa(go); }
            go.transform.position += new Vector3(at.x - b.center.x, Chao(at) - b.min.y - afundar, at.y - b.center.z);
            foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.isStatic = true;
            if (bloqueia)
            {
                b = Caixa(go);
                foreach (var c in go.GetComponentsInChildren<Collider>(true)) c.enabled = false;
                var box = go.AddComponent<BoxCollider>();
                var s = go.transform.lossyScale;
                box.center = go.transform.InverseTransformPoint(b.center);
                // Slightly smaller than the drawing (round rocks), so the way around them stays open.
                box.size = new Vector3(b.size.x * .8f / s.x, b.size.y / s.y, b.size.z * .8f / s.z);
                go.layer = 9;
            }
            return go;
        }

        static Vector2 Rand(System.Random r, float range) => new Vector2((float)(r.NextDouble() * 2 - 1) * range, (float)(r.NextDouble() * 2 - 1) * range);

        static bool Livre(Vector2 p, float estrada, float agua)
        {
            if (Mathf.Abs(p.x) > Half - 1.5f || Mathf.Abs(p.y) > Half - 1.5f) return false;
            if (NascenteDoRio.AEstrada(p) < estrada) return false;
            if (NascenteDoRio.AoRio(p) < NascenteDoRio.LarguraDoRio * .5f + agua) return false;
            if (Vector2.Distance(p, NascenteDoRio.Poco) < NascenteDoRio.RaioDoPoco + agua) return false;
            return true;
        }

        static void Vegetacao(Transform parent, System.Random random)
        {
            var arvores = VegetacaoInstaller.Load("Arvores"); var gramas = VegetacaoInstaller.Load("Gramas"); var flores = VegetacaoInstaller.Load("Flores");
            var holder = new GameObject("Vegetacao").transform; holder.SetParent(parent, false);
            int trees = 0, plants = 0;
            if (arvores.Length > 0)
                for (float x = -Half + 2; x < Half - 1; x += 4.6f)
                    for (float z = -Half + 2; z < Half - 1; z += 4.6f)
                    {
                        var p = new Vector2(x, z) + Rand(random, 1.8f);
                        if (!Livre(p, 4.8f, 3.2f)) continue;
                        float chance = .26f;
                        float edge = Half - Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y));
                        if (edge < 14) chance = .62f;
                        bool skip = false;
                        foreach (var c in NascenteDoRio.Cantos)
                        {
                            float d = Vector2.Distance(p, c.centro);
                            if (c.nome == "Bosque Sussurrante") { if (d < c.raio) chance = d < 5 ? .15f : .8f; continue; }
                            if (c.tipo == NascenteDoRio.Tipo.Segredo) { if (d < 3.2f) skip = true; else if (d < 8.5f && p.y < c.centro.y + 2) chance = .95f; continue; }
                            if (c.tipo == NascenteDoRio.Tipo.Breu) { if (d < c.raio + 1.5f) skip = true; continue; }
                            if (d < c.raio * .75f) skip = true;
                        }
                        if (Vector2.Distance(p, NascenteDoRio.Cachoeira) < 6) skip = true;
                        if (skip || random.NextDouble() > chance) continue;
                        var t = Por(holder, arvores[random.Next(arvores.Length)], p, 0, (float)random.NextDouble() * 360, false);
                        if (t == null) continue;
                        t.transform.localScale *= .85f + (float)random.NextDouble() * .4f;
                        t.layer = 9; trees++; // the trunk (its capsule) blocks the way
                    }
            for (float x = -Half + 1; x < Half - 1; x += 2.8f)
                for (float z = -Half + 1; z < Half - 1; z += 2.8f)
                {
                    var p = new Vector2(x, z) + Rand(random, 1.1f);
                    if (!Livre(p, 2.6f, .8f)) continue;
                    if (NascenteDoRio.Cantos.Any(c => c.tipo == NascenteDoRio.Tipo.Breu && Vector2.Distance(p, c.centro) < c.raio)) continue;
                    bool calmo = NascenteDoRio.Cantos.Any(c => c.tipo == NascenteDoRio.Tipo.Seguro && Vector2.Distance(p, c.centro) < c.raio);
                    double roll = random.NextDouble();
                    if (flores.Length > 0 && roll < (calmo ? .32f : .07f)) { Por(holder, flores[random.Next(flores.Length)], p, 0, (float)random.NextDouble() * 360, false); plants++; }
                    else if (gramas.Length > 0 && roll < .55f) { Por(holder, gramas[random.Next(gramas.Length)], p, 0, (float)random.NextDouble() * 360, false); plants++; }
                }
            // Reeds along the river banks.
            if (gramas.Length > 0)
                foreach (var p in RioSuave().Where((_, i) => i % 3 == 0))
                    for (int s = -1; s <= 1; s += 2)
                    {
                        var q = p + Rand(random, 1.2f) + new Vector2(s * (NascenteDoRio.LarguraDoRio * .5f + 1.2f), 0);
                        if (Livre(q, 2.4f, .5f)) { var g = Por(holder, gramas[random.Next(gramas.Length)], q, 0, (float)random.NextDouble() * 360, false); if (g != null) g.transform.localScale *= 1.5f; plants++; }
                    }
            // Forest on the mountains around the place (beyond the edge), so the view never ends in a gap.
            if (arvores.Length > 0)
                for (float x = -Outer + 3; x < Outer - 2; x += 5.2f)
                    for (float z = -Outer + 3; z < Outer - 2; z += 5.2f)
                    {
                        var p = new Vector2(x, z) + Rand(random, 2);
                        float out_ = Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y));
                        if (out_ < Half + 1.5f || out_ > Outer - 4 || NascenteDoRio.AEstrada(p) < 6 || NascenteDoRio.AoRio(p) < 7) continue;
                        if (random.NextDouble() > .6f) continue;
                        var t = Por(holder, arvores[random.Next(arvores.Length)], p, 0, (float)random.NextDouble() * 360, false);
                        if (t != null) { t.transform.localScale *= .95f + (float)random.NextDouble() * .5f; trees++; }
                    }
            Debug.Log("NASCENTE_VEGETACAO arvores=" + trees + " plantas=" + plants);
        }

        static void Pedras(Transform parent, System.Random random)
        {
            var holder = new GameObject("Pedras").transform; holder.SetParent(parent, false);
            var rochas = Load("Cidade/Rochas/Rochas.prefab"); var rock = Load("ExpeditionSet/rock.fbx");
            var grande = rochas != null ? rochas : rock;
            // The rocks the spring comes out of, around the waterfall.
            for (int i = 0; i < 9; i++)
            {
                float a = i * 40 * Mathf.Deg2Rad; float d = 2.6f + (float)random.NextDouble() * 2.4f;
                var p = NascenteDoRio.Cachoeira + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * d;
                if (p.x < NascenteDoRio.Cachoeira.x - 2.2f && Mathf.Abs(p.y - NascenteDoRio.Cachoeira.y) < 1.8f) continue; // the waterfall itself
                Por(holder, grande, p, 2.4f + (float)random.NextDouble() * 2.6f, (float)random.NextDouble() * 360, true, .5f);
            }
            // Pedras Partidas: big broken rocks with room to fight between them.
            var pedras = NascenteDoRio.Cantos.First(c => c.nome == "Pedras Partidas").centro;
            for (int i = 0; i < 11; i++)
            {
                float a = (i * 33 + (float)random.NextDouble() * 15) * Mathf.Deg2Rad; float d = 7 + (float)random.NextDouble() * 6;
                var p = pedras + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * d;
                if (!Livre(p, 4.5f, 2)) continue;
                Por(holder, grande, p, 1.6f + (float)random.NextDouble() * 2.4f, (float)random.NextDouble() * 360, true, .3f);
            }
            // Small rocks on the river banks.
            var line = RioSuave();
            for (int i = 4; i < line.Count; i += 7)
            {
                var dir = (line[Mathf.Min(line.Count - 1, i + 1)] - line[i - 1]).normalized; var side = new Vector2(-dir.y, dir.x) * (i % 14 == 4 ? 1 : -1);
                var p = line[i] + side * (NascenteDoRio.LarguraDoRio * .5f + .7f);
                if (Vector2.Distance(p, NascenteDoRio.Vau) < NascenteDoRio.RaioDoVau + 1 || !Livre(p, 3, -.2f)) continue;
                Por(holder, rock, p, .5f + (float)random.NextDouble() * .6f, (float)random.NextDouble() * 360, false, .15f);
            }
            // Stepping stones across the ford.
            var vdir = (NascenteDoRio.Rio[4] - NascenteDoRio.Rio[2]).normalized; var across = new Vector2(-vdir.y, vdir.x);
            for (int i = -2; i <= 2; i++)
            {
                var p = NascenteDoRio.Vau + across * (i * 1.3f) + vdir * ((i % 2) * .8f);
                var s = Por(holder, rock, p, .35f, (float)random.NextDouble() * 360, false, .05f);
                if (s != null) s.transform.localScale = Vector3.Scale(s.transform.localScale, new Vector3(1.6f, 1, 1.6f));
            }
        }

        static void Placa(Transform parent, Vector2 at, float giro, string texto)
        {
            var wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Data/World/Wood.mat");
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube); post.name = "Placa"; post.transform.SetParent(parent, false);
            post.transform.position = new Vector3(at.x, Chao(at) + .9f, at.y); post.transform.localScale = new Vector3(.18f, 1.8f, .18f); post.layer = 9;
            if (wood != null) post.GetComponent<Renderer>().sharedMaterial = wood;
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube); board.name = "Tabua"; board.transform.SetParent(parent, false);
            board.transform.position = post.transform.position + Vector3.up * .65f; board.transform.rotation = Quaternion.Euler(0, giro, 0);
            board.transform.localScale = new Vector3(1.7f, .55f, .08f); Object.DestroyImmediate(board.GetComponent<Collider>());
            if (wood != null) board.GetComponent<Renderer>().sharedMaterial = wood;
            var label = new GameObject("Texto").AddComponent<TextMesh>(); label.transform.SetParent(parent, false);
            label.transform.position = board.transform.position + Quaternion.Euler(0, giro, 0) * new Vector3(0, 0, -.06f);
            label.transform.rotation = Quaternion.Euler(0, giro, 0);
            label.text = texto; label.fontSize = 40; label.characterSize = .045f; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
            label.color = new Color(1f, .93f, .76f);
        }

        static void Fogueira(Transform parent, Vector2 at)
        {
            var wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Data/World/Wood.mat");
            var holder = new GameObject("Fogueira").transform; holder.SetParent(parent, false);
            holder.position = new Vector3(at.x, Chao(at), at.y);
            for (int i = 0; i < 4; i++)
            {
                var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder); log.name = "Lenha"; log.transform.SetParent(holder, false);
                Object.DestroyImmediate(log.GetComponent<Collider>());
                log.transform.localPosition = new Vector3(0, .12f, 0); log.transform.localRotation = Quaternion.Euler(80, i * 45, 0); log.transform.localScale = new Vector3(.13f, .45f, .13f);
                if (wood != null) log.GetComponent<Renderer>().sharedMaterial = wood;
            }
            var light = new GameObject("Brasa").AddComponent<Light>(); light.transform.SetParent(holder, false); light.transform.localPosition = Vector3.up * .5f;
            light.type = LightType.Point; light.color = new Color(1f, .62f, .3f); light.range = 7; light.intensity = 2.4f; light.shadows = LightShadows.None;
        }

        // ---------- What each corner holds ----------
        static void Cantos(Transform parent, System.Random random)
        {
            var holder = new GameObject("Cantos").transform; holder.SetParent(parent, false);
            var breu = Mat("Breu", new Color(.16f, .05f, .26f), .95f, null);
            var cogumelos = Load("ExpeditionSet/mushrooms.fbx"); var arbusto = Load("ExpeditionSet/bush.fbx");
            var barril = Load("ExpeditionSet/barrel.fbx"); var caixa = Load("ExpeditionSet/crate.fbx"); var selo = Load("ExpeditionSet/seal.fbx");
            var bandeira = Load("ExpeditionSet/banner.fbx"); var suprimento = Load("ExpeditionSet/supply.fbx");

            // Vau do Norte: where you arrive. A Liga camp left behind (Nara's map is old): tent, crates, a fire. Safe.
            var vau = new GameObject("VauDoNorte").transform; vau.SetParent(holder, false);
            Por(vau, Load("Cidade/Tenda/Tenda.prefab"), new Vector2(-9, 52), 3.2f, 120, true);
            Por(vau, Load("Cidade/Caixotes/Caixotes.prefab"), new Vector2(-10.5f, 46.5f), 1.3f, 30, true);
            Por(vau, suprimento, new Vector2(-6.5f, 55.5f), 1, 200, true);
            Por(vau, Load("Cidade/Banco/Banco.prefab"), new Vector2(-6.2f, 44.5f), .9f, 75, true);
            Fogueira(vau, new Vector2(-5, 48));
            Placa(vau, new Vector2(3.5f, 41), 0, "NASCENTE DO RIO\nPoço da Nascente ao sul");

            // Bosque Sussurrante: the forest (trees from the vegetation pass) with mushrooms; the taken animals live here.
            var bosque = NascenteDoRio.Cantos.First(c => c.nome == "Bosque Sussurrante").centro;
            for (int i = 0; i < 14; i++) { var p = bosque + Rand(random, 17); if (Livre(p, 4, 2)) Por(holder, cogumelos, p, .6f + (float)random.NextDouble() * .5f, (float)random.NextDouble() * 360, false); }

            // Clareira Quieta: an old well and benches. Safe.
            var clareira = NascenteDoRio.Cantos.First(c => c.nome == "Clareira Quieta").centro;
            Por(holder, Load("Cidade/Poco/Poco.prefab"), clareira, 2.2f, 20, true);
            Por(holder, Load("Cidade/Banco/Banco.prefab"), clareira + new Vector2(3.6f, 2.8f), .9f, 230, true);
            Por(holder, Load("Cidade/Banco/Banco.prefab"), clareira + new Vector2(-3.4f, -3), .9f, 50, true);

            // Pedras Partidas: a broken banner and trampled Breu.
            var pedras = NascenteDoRio.Cantos.First(c => c.nome == "Pedras Partidas").centro;
            // The three banners of the lost Liga patrol (side mission) and the note at the camp.
            foreach (var e in NascenteDoRio.Estandartes) Por(holder, bandeira, new Vector2(e.x, e.z), 2.2f, (e.x * 13) % 360, true);

            // Toca do Breu: a dark seal in the ground, mushrooms and puddles of Breu.
            var toca = NascenteDoRio.Cantos.First(c => c.nome == "Toca do Breu").centro;
            Por(holder, selo, toca, 1.3f, 0, true);
            for (int i = 0; i < 8; i++) { var p = toca + Rand(random, 9); if (Livre(p, 4, 2)) Por(holder, cogumelos, p, .7f, (float)random.NextDouble() * 360, false); }

            // Trilha dos Juncos: tall reeds on both sides of the path (the ambush).
            var juncos = NascenteDoRio.Cantos.First(c => c.nome == "Trilha dos Juncos").centro;
            var gramas = VegetacaoInstaller.Load("Gramas");
            if (gramas.Length > 0)
                for (int i = 0; i < 40; i++)
                {
                    var p = juncos + Rand(random, 8);
                    if (NascenteDoRio.AEstrada(p) < 2.4f || !Livre(p, 2.4f, 1)) continue;
                    var g = Por(holder, gramas[random.Next(gramas.Length)], p, 0, (float)random.NextDouble() * 360, false);
                    if (g != null) g.transform.localScale *= 2.2f;
                }

            // Poço da Nascente: cargo of a drowned ship scattered around, an old ruin behind, Breu on the shore.
            var poco = NascenteDoRio.Cantos.First(c => c.nome == "Poço da Nascente").centro;
            Por(holder, Load("Cidade/RuinaGigante/RuinaGigante.prefab"), new Vector2(NascenteDoRio.Chefe.x, NascenteDoRio.Chefe.z - 11), 6.5f, 180, true);
            foreach (var (o, model, h) in new[] { (new Vector2(-8, -4), barril, .9f), (new Vector2(9, -6), caixa, .8f), (new Vector2(-10, -9), caixa, .8f), (new Vector2(7, -10), barril, .9f), (new Vector2(-4, -10), barril, .9f) })
                Por(holder, model, new Vector2(NascenteDoRio.Chefe.x, NascenteDoRio.Chefe.z) + o + Rand(random, .6f), h, (float)random.NextDouble() * 360, true);

            // Recanto Escondido: bushes close it on three sides; the way in is from the north.
            var recanto = new Vector2(NascenteDoRio.Segredo.x, NascenteDoRio.Segredo.z);
            for (int i = 0; i < 11; i++)
            {
                float a = (200 + i * 15) * Mathf.Deg2Rad; // from west-southwest round to east: open to the north
                var p = recanto + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (3.6f + (float)random.NextDouble() * .8f);
                Por(holder, arbusto, p, 1.4f + (float)random.NextDouble() * .5f, (float)random.NextDouble() * 360, true);
            }
            Por(holder, Pedrinha, recanto + new Vector2(1.6f, -1.2f), .9f, 30, false);
            Por(holder, Pedrinha, recanto + new Vector2(-1.5f, -1.4f), .7f, 140, false);

            // Breu zones: a cluster of dark crystals in the middle (for much later) and broken rocks around.
            foreach (var zona in NascenteDoRio.Cantos.Where(c => c.tipo == NascenteDoRio.Tipo.Breu))
            {
                var z = new GameObject("ZonaDoBreu_" + zona.nome).transform; z.SetParent(holder, false);
                for (int i = 0; i < 5; i++)
                {
                    var shard = GameObject.CreatePrimitive(PrimitiveType.Cube); shard.name = "Cristal"; shard.transform.SetParent(z, false);
                    Object.DestroyImmediate(shard.GetComponent<Collider>());
                    float h = i == 0 ? 3.2f : 1.2f + (float)random.NextDouble() * 1.2f;
                    var at = zona.centro + (i == 0 ? Vector2.zero : Rand(random, 1.3f));
                    shard.transform.position = new Vector3(at.x, Chao(at) + h * .4f, at.y);
                    shard.transform.rotation = Quaternion.Euler((float)random.NextDouble() * 24 - 12, (float)random.NextDouble() * 360, 45 + (float)random.NextDouble() * 20 - 10);
                    shard.transform.localScale = new Vector3(.55f, h, .55f) * (i == 0 ? 1.2f : 1);
                    shard.GetComponent<Renderer>().sharedMaterial = breu;
                }
                for (int i = 0; i < 6; i++)
                {
                    float ang = i * 60 * Mathf.Deg2Rad; var at = zona.centro + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * (zona.raio * .55f + (float)random.NextDouble() * 2);
                    Por(z, Pedrinha, at, .6f + (float)random.NextDouble() * .7f, (float)random.NextDouble() * 360, false, .1f);
                }
            }
        }
        static GameObject Pedrinha => Load("ExpeditionSet/rock.fbx");

        // ---------- Creatures (camps of the region) ----------
        static EnemyDefinition Inimigo(string path) => AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/ForjaDasMares/Data/" + path + ".asset");

        static Vector3 P(float x, float z) => new Vector3(x, Chao(new Vector2(x, z)) + .05f, z);

        static void Regiao(RegionalWorld world)
        {
            var region = AssetDatabase.LoadAssetAtPath<RegionDefinition>(Data + "nascente.asset");
            if (world == null || region == null) { Debug.LogError("Nascente: não achei o RegionalWorld ou a região (nascente.asset)."); return; }
            var raspador = Inimigo("Enemies/raspador"); var fagulha = Inimigo("Enemies/fagulha"); var guarda = Inimigo("Enemies/guarda-mare");
            var vigia = Inimigo("Enemies/vigia-sombrio"); var capitao = Inimigo("Inimigos/CapitaoAfogado");
            region.displayName = "Nascente do Rio";
            region.refuge = NascenteDoRio.Chegada; region.route = new[] { region.refuge };
            region.activeEnemyLimit = 14;
            var camps = new List<RegionCampDefinition>
            {
                new RegionCampDefinition { label = "Pedras Partidas", center = P(36, 26), respawnSeconds = 120,
                    spawns = new[] { P(33, 24), P(39, 23), P(36, 30), P(41, 28) }, enemies = new[] { raspador, raspador, raspador, vigia } },
                new RegionCampDefinition { label = "Toca do Breu", center = P(-48, -10), respawnSeconds = 120,
                    spawns = new[] { P(-50, -7), P(-45, -12), P(-51, -13), P(-45, -6) }, enemies = new[] { fagulha, fagulha, guarda, raspador } },
                new RegionCampDefinition { label = "Poço da Nascente", center = NascenteDoRio.Chefe, respawnSeconds = 300,
                    spawns = new[] { P(NascenteDoRio.Chefe.x, NascenteDoRio.Chefe.z), P(20, -42), P(32, -42) }, enemies = new[] { capitao, vigia, vigia } },
            };
            camps.RemoveAll(c => c.enemies.Any(e => e == null));
            region.camps = camps.ToArray();
            region.bounds = new Bounds(Vector3.zero, new Vector3(Half * 2 + 4, 40, Half * 2 + 4));
            EditorUtility.SetDirty(region);
            // The creatures the place can show at once (the pool makes them when the scene opens).
            var pool = world.Enemies;
            if (pool != null)
            {
                var so = new SerializedObject(pool); var list = so.FindProperty("definitions");
                var all = new[] { raspador, fagulha, guarda, vigia, capitao }.Where(e => e != null).ToArray();
                list.arraySize = all.Length;
                for (int i = 0; i < all.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = all[i];
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            var hero = world.Hero; if (hero != null) hero.transform.position = region.refuge;
            EditorUtility.SetDirty(world);
        }

        static void Luz()
        {
            var sun = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(x => x.type == LightType.Directional);
            if (sun != null) { sun.intensity = 1.2f; sun.color = new Color(1f, .91f, .78f); sun.transform.rotation = Quaternion.Euler(48, -35, 0); }
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 55; RenderSettings.fogEndDistance = 160;
            RenderSettings.fogColor = new Color(.42f, .55f, .56f); RenderSettings.ambientLight = new Color(.42f, .47f, .48f);
        }

        static bool Navegacao()
        {
            var go = new GameObject("AreaNavigation");
            var nav = go.AddComponent<NavMeshSurface>();
            nav.layerMask = (1 << 8) | (1 << 9); nav.useGeometry = NavMeshCollectGeometry.PhysicsColliders; nav.collectObjects = CollectObjects.All;
            nav.BuildNavMesh();
            string navPath = Data + "nascente_Navegacao.asset";
            var prior = AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
            if (prior == null) AssetDatabase.CreateAsset(nav.navMeshData, navPath); else { EditorUtility.CopySerialized(nav.navMeshData, prior); nav.navMeshData = prior; }
            EditorUtility.SetDirty(nav);
            // Every exit, corner and camp can be reached from the arrival.
            bool ok = true;
            if (!NavMesh.SamplePosition(NascenteDoRio.Chegada, out var start, 4, NavMesh.AllAreas)) { Debug.LogWarning("NASCENTE: a chegada não tem chão andável."); return false; }
            var alvos = new List<(string, Vector3)>();
            foreach (var saida in AreasDoMundo.PorId("nascente").saidas) alvos.Add(("saida " + saida.para, AreasDoMundo.Ponto(saida)));
            alvos.AddRange(new List<(string, Vector3)> {
                ("chefe", NascenteDoRio.Chefe), ("segredo", NascenteDoRio.Segredo + new Vector3(0, 0, 1.5f)), ("emboscada", NascenteDoRio.Emboscada) });
            foreach (var c in NascenteDoRio.Cantos) alvos.Add((c.nome, new Vector3(c.centro.x, 0, c.centro.y)));
            foreach (var (nome, alvo) in alvos)
            {
                var a = new Vector3(alvo.x, Chao(new Vector2(alvo.x, alvo.z)), alvo.z);
                var path = new NavMeshPath();
                if (!NavMesh.SamplePosition(a, out var end, 5, NavMesh.AllAreas) || !NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete)
                { Debug.LogWarning("NASCENTE: sem caminho até " + nome + " " + alvo); ok = false; }
            }
            return ok;
        }
    }
}
