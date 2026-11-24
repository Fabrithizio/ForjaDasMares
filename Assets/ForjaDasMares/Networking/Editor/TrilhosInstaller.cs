using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace ForjaDasMares.Networking.Editor
{
    // Builds the whole Trilhos Velhos in its scene (Scenes/Areas/Area_Trilhos), from the plan in TrilhosVelhos:
    // ground with mountains around, the old railway (sleepers and rusty rails) and the branch to the shunting yard,
    // overturned wagons, a dead locomotive, the line keeper's house and water tower, trees, rocks, the props of each
    // corner, the creatures of the line (new variants made here) with the Custódio in the yard, and the navigation.
    // Can be run again: everything it made is replaced.
    public static class TrilhosInstaller
    {
        const string ScenePath = "Assets/ForjaDasMares/Scenes/Areas/Area_Trilhos.unity";
        const string Data = "Assets/ForjaDasMares/Data/World/Areas/";
        const string Art = "Assets/ForjaDasMares/Art/";
        const float Half = 64, Outer = 96, Ground = 2, Height = 32;
        static Terrain terrain;

        [MenuItem("Forja/Mundo/Montar os Trilhos Velhos")]
        public static void Montar()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(ScenePath);
            // After opening the scene: opening it unloads assets loaded before, which would leave the list empty.
            var inimigos = Inimigos();
            foreach (var go in scene.GetRootGameObjects())
                if (go.name == "AreaEnvironment" || go.name == "AreaNavigation" || go.name == "TrilhosMontados") Object.DestroyImmediate(go);
            var root = new GameObject("TrilhosMontados").transform;
            var random = new System.Random(20261123);

            Terreno(root);
            Limites(root);
            Ferrovia(root);
            Vegetacao(root, random);
            Pedras(root, random);
            Cantos(root, random);
            var world = Object.FindFirstObjectByType<RegionalWorld>();
            Regiao(world, inimigos);
            Luz();
            bool ok = Navegacao();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("TRILHOS_MONTADOS caminhos_ok=" + ok);
            EditorUtility.DisplayDialog("Forja · Trilhos Velhos", ok ? "Os Trilhos Velhos estão montados. Dê Play e vá pela saída sudoeste da Nascente."
                : "Os Trilhos foram montados, mas algum caminho ficou bloqueado. Veja o aviso amarelo no Console e me mande.", "OK");
        }

        // ---------- Ground ----------
        static float S(float a, float b, float x) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(a, b, x));
        static float Ruido(Vector2 p, float scale, float seed) => Mathf.PerlinNoise((p.x + seed) * scale, (p.y + seed * 1.7f) * scale);

        static float Colinas(Vector2 p) => Ruido(p, .045f, 431) * 2.8f + Ruido(p, .12f, 77) * .6f;

        static float Altura(Vector2 p)
        {
            float road = TrilhosVelhos.AEstrada(p), linha = TrilhosVelhos.ALinha(p);
            // Rolling hills (a bit more than the Nascente: this is the dry side). Each corner is flat at the height of
            // its own middle, with a long gentle ramp around it (no pits: a corner below the hills could not be walked out of).
            float y = Colinas(p);
            foreach (var c in TrilhosVelhos.Cantos)
                y = Mathf.Lerp(Colinas(c.centro), y, S(c.raio * .55f, c.raio * 1.8f, Vector2.Distance(p, c.centro)));
            y *= S(4, 13, road);
            // The railway runs on a low embankment.
            y += (1 - S(1.6f, 3.4f, linha)) * .25f;
            // Cliffs at the edge and mountains beyond, open only where the line and the roads pass.
            float corridor = S(3.6f, 7.5f, road);
            float edge = Half - Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y));
            if (edge < 14) y += Mathf.Pow(1 - Mathf.Clamp(edge, 0, 14) / 14, 1.3f) * 15 * corridor;
            if (edge < 0) y += (Mathf.Clamp01(-edge / 14) * 6 + Ruido(p, .07f, 91) * 5 * Mathf.Clamp01(-edge / 6)) * corridor;
            // The narrow cut: rock walls on both sides of the line (where the ambush comes from).
            float corte = Vector2.Distance(p, new Vector2(TrilhosVelhos.Emboscada.x, TrilhosVelhos.Emboscada.z));
            if (corte < 14) y += S(4.5f, 7.5f, linha) * (1 - S(8, 14, corte)) * 4.5f;
            return y;
        }

        static (float breu, float rock, float dirt) Pintura(Vector2 p)
        {
            float linha = TrilhosVelhos.ALinha(p);
            float estrada = 1;
            foreach (var e in TrilhosVelhos.Estradas) estrada = Mathf.Min(estrada, S(1.6f, 3.2f, NascenteDoRio.Distancia(p, e)));
            float road = 1 - estrada;
            float lastro = 1 - S(1.8f, 2.8f, linha); // gravel under the sleepers
            float edge = Half - Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y));
            float rock = Mathf.Max(Mathf.Max(Mathf.Clamp01(1 - edge / 9), lastro), (1 - S(6, 10, Vector2.Distance(p, new Vector2(TrilhosVelhos.Emboscada.x, TrilhosVelhos.Emboscada.z)))) * S(2.5f, 4.5f, linha));
            float patches = S(.58f, .7f, Ruido(p, .1f, 613)) * .8f; // dry side: more bare earth
            float dirt = Mathf.Max(road, patches);
            float breu = 0;
            foreach (var c in TrilhosVelhos.Cantos)
            {
                float d = Vector2.Distance(p, c.centro);
                if (c.tipo == NascenteDoRio.Tipo.Breu) breu = Mathf.Max(breu, 1 - S(c.raio * .8f, c.raio * 1.15f, d));
                else if (c.tipo == NascenteDoRio.Tipo.Perigo || c.tipo == NascenteDoRio.Tipo.Chefe)
                {
                    float near = 1 - S(c.raio * .3f, c.raio * .95f, d);
                    breu = Mathf.Max(breu, near * S(.6f, .7f, Ruido(p, .2f, 31)) * .9f);
                    dirt = Mathf.Max(dirt, near * .6f);
                }
            }
            return (Mathf.Clamp01(breu * (1 - lastro)), Mathf.Clamp01(rock), dirt);
        }

        static Texture2D Textura(string name, System.Func<float, float, Color> pixel)
        {
            string path = Data + "Trilhos_" + name + ".png";
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
            string path = Data + "Trilhos_" + name + ".terrainlayer";
            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
            if (layer == null) { layer = new TerrainLayer(); AssetDatabase.CreateAsset(layer, path); }
            layer.diffuseTexture = tex; layer.tileSize = new Vector2(tile, tile); layer.smoothness = smooth; layer.metallic = 0;
            EditorUtility.SetDirty(layer); return layer;
        }

        static TerrainLayer[] Camadas()
        {
            // Dry grass, reddish earth, grey gravel (the line) and Breu.
            var grama = Textura("Grama", (u, v) =>
            {
                float n = Fbm(u, v, 5), fine = Rep(u, v, 48, 13);
                var c = Color.Lerp(new Color(.36f, .42f, .18f), new Color(.55f, .56f, .27f), n);
                if (fine > .8f) c = Color.Lerp(c, new Color(.66f, .62f, .34f), .5f);
                return c;
            });
            var terra = Textura("Terra", (u, v) =>
            {
                float n = Fbm(u, v, 3), fine = Rep(u, v, 64, 9);
                var c = Color.Lerp(new Color(.42f, .27f, .17f), new Color(.6f, .42f, .27f), n);
                if (fine > .86f) c = Color.Lerp(c, new Color(.7f, .6f, .48f), .6f);
                return c;
            });
            var cascalho = Textura("Cascalho", (u, v) =>
            {
                float n = Fbm(u, v, 19), stones = Rep(u, v, 40, 29);
                var c = Color.Lerp(new Color(.36f, .35f, .34f), new Color(.55f, .53f, .5f), n);
                if (stones > .7f) c = Color.Lerp(c, new Color(.68f, .66f, .62f), (stones - .7f) * 2.5f);
                if (stones < .18f) c *= .7f;
                return c;
            });
            var breu = Textura("Breu", (u, v) =>
            {
                float n = Fbm(u, v, 41), vein = Mathf.Abs(Rep(u, v, 5, 47) - .5f);
                var c = Color.Lerp(new Color(.07f, .03f, .1f), new Color(.16f, .07f, .21f), n);
                if (vein < .03f) c = Color.Lerp(c, new Color(.55f, .2f, .75f), 1 - vein / .03f);
                return c;
            });
            return new[] { Camada("Grama", grama, 7, 0), Camada("Terra", terra, 5, .05f), Camada("Cascalho", cascalho, 3, .1f), Camada("Breu", breu, 4, .6f) };
        }

        static void Terreno(Transform parent)
        {
            string path = Data + "trilhos_Terreno.asset";
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
            // The old sketch ground (if any) is removed with AreaEnvironment above; this one replaces it.
            var go = Terrain.CreateTerrainGameObject(data);
            go.name = "Terreno"; go.transform.SetParent(parent); go.transform.position = new Vector3(-Outer, -Ground, -Outer); go.layer = 8;
            terrain = go.GetComponent<Terrain>(); terrain.heightmapPixelError = 6; terrain.drawInstanced = true;
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Data/World/Terrain.mat");
            if (mat != null) terrain.materialTemplate = mat;
        }

        static float Chao(Vector2 p) => terrain.SampleHeight(new Vector3(p.x, 0, p.y)) + terrain.transform.position.y;

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

        // ---------- Materials and meshes ----------
        static Material Mat(string name, Color color, float smooth)
        {
            string path = Data + "Trilhos_" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
            m.color = color; m.SetFloat("_Smoothness", smooth);
            EditorUtility.SetDirty(m); return m;
        }
        static Material Madeira => AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Data/World/Wood.mat") ?? Mat("Madeira", new Color(.42f, .3f, .2f), .1f);
        static Material Ferrugem => Mat("Ferrugem", new Color(.45f, .27f, .17f), .3f);
        static Material Ferro => Mat("Ferro", new Color(.25f, .24f, .24f), .45f);
        static Material Vagao => Mat("Vagao", new Color(.38f, .2f, .14f), .2f);
        static Material Breu => Mat("Breu", new Color(.16f, .05f, .26f), .95f);

        static Mesh Salvar(Mesh mesh)
        {
            string path = Data + "Trilhos_" + mesh.name + ".asset";
            var prior = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (prior == null) { AssetDatabase.CreateAsset(mesh, path); return mesh; }
            EditorUtility.CopySerialized(mesh, prior); return prior;
        }

        static List<Vector2> Suave(Vector2[] pts)
        {
            var list = new List<Vector2>();
            for (int i = 0; i < pts.Length - 1; i++)
            {
                Vector2 p0 = pts[Mathf.Max(0, i - 1)], p1 = pts[i], p2 = pts[i + 1], p3 = pts[Mathf.Min(pts.Length - 1, i + 2)];
                int steps = Mathf.CeilToInt(Vector2.Distance(p1, p2) / .9f);
                for (int s = 0; s < steps; s++)
                {
                    float t = s / (float)steps, t2 = t * t, t3 = t2 * t;
                    list.Add(.5f * (2 * p1 + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + (-p0 + 3 * p1 - 3 * p2 + p3) * t3));
                }
            }
            list.Add(pts[pts.Length - 1]);
            return list;
        }

        // One mesh made of many boxes (sleepers or rails), so the whole line is a handful of objects.
        static GameObject Caixas(Transform parent, string name, List<Matrix4x4> boxes, Material mat)
        {
            var cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var combine = boxes.Select(m => new CombineInstance { mesh = cube, transform = m }).ToArray();
            var mesh = new Mesh { name = name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.CombineMeshes(combine, true, true);
            mesh.RecalculateBounds();
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = Salvar(mesh);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            go.isStatic = true;
            return go;
        }

        // ---------- The railway ----------
        static void Ferrovia(Transform parent)
        {
            var holder = new GameObject("Ferrovia").transform; holder.SetParent(parent, false);
            var dormentes = new List<Matrix4x4>(); var trilhos = new List<Matrix4x4>();
            var random = new System.Random(7);
            for (int l = 0; l < TrilhosVelhos.Trilhos.Length; l++)
            {
                var line = Suave(TrilhosVelhos.Trilhos[l]);
                for (int i = 0; i < line.Count; i++)
                {
                    var p = line[i];
                    if (Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y)) > Outer - 2) continue;
                    var dir = (line[Mathf.Min(line.Count - 1, i + 1)] - line[Mathf.Max(0, i - 1)]).normalized;
                    var rot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.y));
                    float y = Chao(p);
                    // Some sleepers are missing or crooked: the line is old.
                    if (random.NextDouble() > .08f)
                    {
                        var twist = Quaternion.Euler(0, (float)(random.NextDouble() * 10 - 5), 0);
                        dormentes.Add(Matrix4x4.TRS(new Vector3(p.x, y + .06f, p.y), rot * twist, new Vector3(2.3f, .12f, .32f)));
                    }
                    if (i == 0) continue;
                    var prev = line[i - 1]; float py = Chao(prev);
                    // A few rails are broken in the shunting yard branch.
                    if (l == 1 && random.NextDouble() < .12f) continue;
                    foreach (float side in new[] { -.72f, .72f })
                    {
                        var right = new Vector3(dir.y, 0, -dir.x) * side;
                        var a = new Vector3(prev.x, py + .19f, prev.y) + right; var b = new Vector3(p.x, y + .19f, p.y) + right;
                        var mid = (a + b) * .5f; var along = b - a;
                        if (along.sqrMagnitude < .0001f) continue;
                        trilhos.Add(Matrix4x4.TRS(mid, Quaternion.LookRotation(along), new Vector3(.09f, .12f, along.magnitude + .04f)));
                    }
                }
            }
            Caixas(holder, "Dormentes", dormentes, Madeira);
            Caixas(holder, "Trilhos", trilhos, Ferrugem);
        }

        // ---------- Things on the ground ----------
        static GameObject Load(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(Art + path);
        static GameObject Pedrinha => Load("ExpeditionSet/rock.fbx");

        static Bounds Caixa(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            var b = rs.Length > 0 ? rs[0].bounds : new Bounds(go.transform.position, Vector3.one);
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        static GameObject Por(Transform parent, GameObject prefab, Vector2 at, float altura, float giro, bool bloqueia, float afundar = 0)
        {
            if (prefab == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0, giro, 0)); go.transform.localScale = Vector3.one;
            var b = Caixa(go);
            if (altura > 0 && b.size.y > .001f) { go.transform.localScale = Vector3.one * (altura / b.size.y); b = Caixa(go); }
            go.transform.position += new Vector3(at.x - b.center.x, Chao(at) - b.min.y - afundar, at.y - b.center.z);
            foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.isStatic = true;
            if (bloqueia) Bloquear(go);
            return go;
        }

        static void Bloquear(GameObject go)
        {
            var b = Caixa(go);
            foreach (var c in go.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            var box = go.AddComponent<BoxCollider>();
            var s = go.transform.lossyScale;
            box.center = go.transform.InverseTransformPoint(b.center);
            box.size = new Vector3(b.size.x * .8f / s.x, b.size.y / s.y, b.size.z * .8f / s.z);
            go.layer = 9;
        }

        // A box made of a primitive, placed relative to a parent (local), with a material. No collider.
        static GameObject Peca(Transform parent, PrimitiveType type, Vector3 local, Vector3 scale, Vector3 euler, Material mat)
        {
            var go = GameObject.CreatePrimitive(type); go.name = type.ToString(); go.transform.SetParent(parent, false);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.localPosition = local; go.transform.localRotation = Quaternion.Euler(euler); go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat; go.isStatic = true;
            return go;
        }

        // An old wagon: body, roof planks and four wheels. Tilted = lying on its side.
        static GameObject VagaoVelho(Transform parent, Vector2 at, float giro, float tombo)
        {
            var go = new GameObject("Vagao"); go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(at.x, Chao(at) - (tombo > 45 ? .2f : 0), at.y);
            go.transform.rotation = Quaternion.Euler(0, giro, tombo);
            Peca(go.transform, PrimitiveType.Cube, new Vector3(0, 1.25f, 0), new Vector3(2.2f, 1.7f, 5.2f), Vector3.zero, Vagao);
            Peca(go.transform, PrimitiveType.Cube, new Vector3(0, 2.15f, 0), new Vector3(2.4f, .12f, 5.4f), Vector3.zero, Madeira);
            foreach (var z in new[] { -1.7f, 1.7f }) foreach (var x in new[] { -.95f, .95f })
                Peca(go.transform, PrimitiveType.Cylinder, new Vector3(x, .42f, z), new Vector3(.8f, .06f, .8f), new Vector3(0, 0, 90), Ferro);
            Bloquear(go);
            return go;
        }

        // The dead locomotive in the shunting yard.
        static void Locomotiva(Transform parent, Vector2 at, float giro)
        {
            var go = new GameObject("Locomotiva"); go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(at.x, Chao(at), at.y); go.transform.rotation = Quaternion.Euler(0, giro, 0);
            Peca(go.transform, PrimitiveType.Cylinder, new Vector3(0, 1.7f, .6f), new Vector3(1.8f, 2.6f, 1.8f), new Vector3(90, 0, 0), Ferro);
            Peca(go.transform, PrimitiveType.Cube, new Vector3(0, 1.9f, -2.6f), new Vector3(2.3f, 2.6f, 2.2f), Vector3.zero, Ferrugem);
            Peca(go.transform, PrimitiveType.Cube, new Vector3(0, 3.3f, -2.6f), new Vector3(2.6f, .15f, 2.6f), Vector3.zero, Ferro);
            Peca(go.transform, PrimitiveType.Cylinder, new Vector3(0, 3.4f, 2.4f), new Vector3(.55f, .8f, .55f), Vector3.zero, Ferro);
            Peca(go.transform, PrimitiveType.Cube, new Vector3(0, .5f, 0), new Vector3(2f, .5f, 7.2f), Vector3.zero, Ferro);
            foreach (var z in new[] { -2.4f, -.6f, 1.2f, 2.8f }) foreach (var x in new[] { -1.05f, 1.05f })
                Peca(go.transform, PrimitiveType.Cylinder, new Vector3(x, .65f, z), new Vector3(1.2f, .07f, 1.2f), new Vector3(0, 0, 90), Ferrugem);
            Bloquear(go);
        }

        // The water tower beside the keeper's house.
        static void CaixaDagua(Transform parent, Vector2 at)
        {
            var go = new GameObject("CaixaDagua"); go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(at.x, Chao(at), at.y);
            foreach (var x in new[] { -1.1f, 1.1f }) foreach (var z in new[] { -1.1f, 1.1f })
                Peca(go.transform, PrimitiveType.Cube, new Vector3(x, 2, z), new Vector3(.22f, 4, .22f), Vector3.zero, Madeira);
            Peca(go.transform, PrimitiveType.Cylinder, new Vector3(0, 5, 0), new Vector3(3, 1.1f, 3), Vector3.zero, Madeira);
            Peca(go.transform, PrimitiveType.Cylinder, new Vector3(0, 6.2f, 0), new Vector3(3.2f, .1f, 3.2f), Vector3.zero, Ferrugem);
            var box = go.AddComponent<BoxCollider>(); box.center = new Vector3(0, 2, 0); box.size = new Vector3(2.6f, 4, 2.6f); go.layer = 9;
        }

        // The keeper's house: low walls, an open door to the south, a fallen roof.
        static void Casa(Transform parent, Vector2 at)
        {
            var go = new GameObject("CasaDoGuardaLinha"); go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(at.x, Chao(at), at.y);
            var pedra = Mat("PedraDaCasa", new Color(.55f, .5f, .45f), .1f);
            foreach (var (pos, size) in new[] {
                (new Vector3(0, 1.2f, 2.6f), new Vector3(6, 2.4f, .4f)), (new Vector3(-2.8f, 1.2f, 0), new Vector3(.4f, 2.4f, 5.6f)), (new Vector3(2.8f, .9f, 0), new Vector3(.4f, 1.8f, 5.6f)),
                (new Vector3(-1.9f, 1.2f, -2.6f), new Vector3(2.2f, 2.4f, .4f)), (new Vector3(1.9f, .7f, -2.6f), new Vector3(2.2f, 1.4f, .4f)) })
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "Parede"; wall.transform.SetParent(go.transform, false);
                wall.transform.localPosition = pos; wall.transform.localScale = size; wall.layer = 9; wall.isStatic = true;
                wall.GetComponent<Renderer>().sharedMaterial = pedra;
            }
            Peca(go.transform, PrimitiveType.Cube, new Vector3(-.6f, 1.4f, .4f), new Vector3(4.4f, .15f, 4f), new Vector3(0, 0, 24), Madeira);
        }

        static void Placa(Transform parent, Vector2 at, float giro, string texto)
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube); post.name = "Placa"; post.transform.SetParent(parent, false);
            post.transform.position = new Vector3(at.x, Chao(at) + .9f, at.y); post.transform.localScale = new Vector3(.18f, 1.8f, .18f); post.layer = 9;
            post.GetComponent<Renderer>().sharedMaterial = Madeira;
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube); board.name = "Tabua"; board.transform.SetParent(parent, false);
            board.transform.position = post.transform.position + Vector3.up * .65f; board.transform.rotation = Quaternion.Euler(0, giro, 0);
            board.transform.localScale = new Vector3(1.9f, .55f, .08f); Object.DestroyImmediate(board.GetComponent<Collider>());
            board.GetComponent<Renderer>().sharedMaterial = Madeira;
            var label = new GameObject("Texto").AddComponent<TextMesh>(); label.transform.SetParent(parent, false);
            label.transform.position = board.transform.position + Quaternion.Euler(0, giro, 0) * new Vector3(0, 0, -.06f);
            label.transform.rotation = Quaternion.Euler(0, giro, 0);
            label.text = texto; label.fontSize = 40; label.characterSize = .042f; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
            label.color = new Color(1f, .93f, .76f);
        }

        static void Fogueira(Transform parent, Vector2 at)
        {
            var holder = new GameObject("Fogueira").transform; holder.SetParent(parent, false);
            holder.position = new Vector3(at.x, Chao(at), at.y);
            for (int i = 0; i < 4; i++)
                Peca(holder, PrimitiveType.Cylinder, new Vector3(0, .12f, 0), new Vector3(.13f, .45f, .13f), new Vector3(80, i * 45, 0), Madeira);
            var light = new GameObject("Brasa").AddComponent<Light>(); light.transform.SetParent(holder, false); light.transform.localPosition = Vector3.up * .5f;
            light.type = LightType.Point; light.color = new Color(1f, .62f, .3f); light.range = 7; light.intensity = 2.4f; light.shadows = LightShadows.None;
        }

        static Vector2 Rand(System.Random r, float range) => new Vector2((float)(r.NextDouble() * 2 - 1) * range, (float)(r.NextDouble() * 2 - 1) * range);

        static bool Livre(Vector2 p, float estrada)
        {
            if (Mathf.Abs(p.x) > Half - 1.5f || Mathf.Abs(p.y) > Half - 1.5f) return false;
            return TrilhosVelhos.AEstrada(p) >= estrada;
        }

        static void Vegetacao(Transform parent, System.Random random)
        {
            var arvores = VegetacaoInstaller.Load("Arvores"); var gramas = VegetacaoInstaller.Load("Gramas"); var flores = VegetacaoInstaller.Load("Flores");
            var holder = new GameObject("Vegetacao").transform; holder.SetParent(parent, false);
            int trees = 0, plants = 0;
            if (arvores.Length > 0)
                for (float x = -Half + 2; x < Half - 1; x += 4.8f)
                    for (float z = -Half + 2; z < Half - 1; z += 4.8f)
                    {
                        var p = new Vector2(x, z) + Rand(random, 1.8f);
                        if (!Livre(p, 5)) continue;
                        float chance = .2f;
                        float edge = Half - Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y));
                        if (edge < 14) chance = .6f;
                        bool skip = false;
                        foreach (var c in TrilhosVelhos.Cantos)
                        {
                            float d = Vector2.Distance(p, c.centro);
                            if (c.nome == "Bosque de Ferro") { if (d < c.raio) chance = d < 5 ? .1f : .75f; continue; }
                            if (c.tipo == NascenteDoRio.Tipo.Segredo) { if (d < 3.2f) skip = true; else if (d < 9 && p.y < c.centro.y + 2) chance = .95f; continue; }
                            if (c.tipo == NascenteDoRio.Tipo.Breu) { if (d < c.raio + 1.5f) skip = true; continue; }
                            if (d < c.raio * .75f) skip = true;
                        }
                        if (skip || random.NextDouble() > chance) continue;
                        var t = Por(holder, arvores[random.Next(arvores.Length)], p, 0, (float)random.NextDouble() * 360, false);
                        if (t == null) continue;
                        t.transform.localScale *= .8f + (float)random.NextDouble() * .4f;
                        t.layer = 9; trees++;
                    }
            for (float x = -Half + 1; x < Half - 1; x += 3f)
                for (float z = -Half + 1; z < Half - 1; z += 3f)
                {
                    var p = new Vector2(x, z) + Rand(random, 1.2f);
                    if (!Livre(p, 2.6f)) continue;
                    if (TrilhosVelhos.Cantos.Any(c => c.tipo == NascenteDoRio.Tipo.Breu && Vector2.Distance(p, c.centro) < c.raio)) continue;
                    bool calmo = TrilhosVelhos.Cantos.Any(c => c.tipo == NascenteDoRio.Tipo.Seguro && Vector2.Distance(p, c.centro) < c.raio);
                    double roll = random.NextDouble();
                    if (flores.Length > 0 && roll < (calmo ? .28f : .04f)) { Por(holder, flores[random.Next(flores.Length)], p, 0, (float)random.NextDouble() * 360, false); plants++; }
                    else if (gramas.Length > 0 && roll < .5f) { Por(holder, gramas[random.Next(gramas.Length)], p, 0, (float)random.NextDouble() * 360, false); plants++; }
                }
            // Weeds growing between the sleepers here and there.
            if (gramas.Length > 0)
                foreach (var line in TrilhosVelhos.Trilhos)
                    foreach (var p in Suave(line).Where((_, i) => i % 9 == 0))
                    {
                        if (Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y)) > Half - 2 || random.NextDouble() > .5f) continue;
                        var g = Por(holder, gramas[random.Next(gramas.Length)], p + Rand(random, .5f), 0, (float)random.NextDouble() * 360, false);
                        if (g != null) { g.transform.localScale *= .7f; plants++; }
                    }
            // Forest on the mountains around.
            if (arvores.Length > 0)
                for (float x = -Outer + 3; x < Outer - 2; x += 5.2f)
                    for (float z = -Outer + 3; z < Outer - 2; z += 5.2f)
                    {
                        var p = new Vector2(x, z) + Rand(random, 2);
                        float out_ = Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y));
                        if (out_ < Half + 1.5f || out_ > Outer - 4 || TrilhosVelhos.AEstrada(p) < 6) continue;
                        if (random.NextDouble() > .55f) continue;
                        var t = Por(holder, arvores[random.Next(arvores.Length)], p, 0, (float)random.NextDouble() * 360, false);
                        if (t != null) { t.transform.localScale *= .95f + (float)random.NextDouble() * .5f; trees++; }
                    }
            Debug.Log("TRILHOS_VEGETACAO arvores=" + trees + " plantas=" + plants);
        }

        static void Pedras(Transform parent, System.Random random)
        {
            var holder = new GameObject("Pedras").transform; holder.SetParent(parent, false);
            var rochas = Load("Cidade/Rochas/Rochas.prefab"); var rock = Pedrinha;
            var grande = rochas != null ? rochas : rock;
            // The narrow cut: rocks on top of both walls.
            var corte = new Vector2(TrilhosVelhos.Emboscada.x, TrilhosVelhos.Emboscada.z);
            for (int i = 0; i < 12; i++)
            {
                var p = corte + Rand(random, 9);
                if (TrilhosVelhos.ALinha(p) < 5.5f) continue;
                Por(holder, grande, p, 1.8f + (float)random.NextDouble() * 2.2f, (float)random.NextDouble() * 360, true, .4f);
            }
            // Scattered boulders across the dry hills.
            // (Small ones only: the big rock clusters are wide and closed whole paths.)
            for (int i = 0; i < 40; i++)
            {
                var p = Rand(random, Half - 4);
                if (!Livre(p, 5) || TrilhosVelhos.Cantos.Any(c => Vector2.Distance(p, c.centro) < c.raio + 4)) continue;
                bool big = random.NextDouble() < .3f;
                Por(holder, rock, p, big ? 1f + (float)random.NextDouble() * .5f : .45f + (float)random.NextDouble() * .4f, (float)random.NextDouble() * 360, big, .2f);
            }
        }

        // ---------- What each corner holds ----------
        static Vector2 Centro(string nome) => TrilhosVelhos.Cantos.First(c => c.nome == nome).centro;

        static void Cantos(Transform parent, System.Random random)
        {
            var holder = new GameObject("Cantos").transform; holder.SetParent(parent, false);
            var cogumelos = Load("ExpeditionSet/mushrooms.fbx"); var arbusto = Load("ExpeditionSet/bush.fbx");
            var barril = Load("ExpeditionSet/barrel.fbx"); var caixa = Load("ExpeditionSet/crate.fbx"); var selo = Load("ExpeditionSet/seal.fbx");
            var bandeira = Load("ExpeditionSet/banner.fbx"); var suprimento = Load("ExpeditionSet/supply.fbx");

            // Mirante do Desvio: where you arrive from the Nascente. A Liga post with a banner and a fire. Safe.
            var mirante = Centro("Mirante do Desvio");
            // Everything beside the road (it runs diagonally through the corner), so the arrival stays open.
            Por(holder, Load("Cidade/Tenda/Tenda.prefab"), mirante + new Vector2(-6, 3), 3f, 120, true);
            Por(holder, bandeira, mirante + new Vector2(3, -6), 2.4f, 200, true);
            Por(holder, suprimento, mirante + new Vector2(6, -3), 1, 30, true);
            Por(holder, Load("Cidade/Banco/Banco.prefab"), mirante + new Vector2(-6, -1), .9f, 45, true);
            Fogueira(holder, mirante + new Vector2(-4, 2));
            Placa(holder, mirante + new Vector2(2, -4), 225, "TRILHOS VELHOS\nEstação Antiga · noroeste");

            // Casa do Guarda-Linha: the ruined house, the water tower and a bench. Safe.
            var casa = Centro("Casa do Guarda-Linha");
            Casa(holder, casa + new Vector2(0, 1));
            CaixaDagua(holder, casa + new Vector2(-7.5f, 2));
            Por(holder, Load("Cidade/Banco/Banco.prefab"), casa + new Vector2(-4.5f, -4.5f), .9f, 70, true);
            Por(holder, Load("Cidade/Poco/Poco.prefab"), casa + new Vector2(5, -4), 2f, 10, true);

            // Vagões Tombados: three wagons on their side beside the line, crates spilled.
            var vagoes = Centro("Vagões Tombados");
            VagaoVelho(holder, vagoes + new Vector2(-5, 3), 40, 82);
            VagaoVelho(holder, vagoes + new Vector2(2, 7), 70, 75);
            VagaoVelho(holder, vagoes + new Vector2(5, -5), 15, 0);
            for (int i = 0; i < 7; i++)
            {
                var p = vagoes + Rand(random, 8);
                if (!Livre(p, 3)) continue;
                Por(holder, random.NextDouble() < .5f ? caixa : barril, p, .8f + (float)random.NextDouble() * .3f, (float)random.NextDouble() * 360, true);
            }

            // Bosque de Ferro: trees (vegetation pass) with old rail signals rusting among them.
            var bosque = Centro("Bosque de Ferro");
            for (int i = 0; i < 4; i++)
            {
                var p = bosque + Rand(random, 8);
                var sinal = new GameObject("SinalVelho"); sinal.transform.SetParent(holder, false);
                sinal.transform.position = new Vector3(p.x, Chao(p), p.y); sinal.transform.rotation = Quaternion.Euler((float)random.NextDouble() * 16 - 8, (float)random.NextDouble() * 360, 0);
                Peca(sinal.transform, PrimitiveType.Cylinder, new Vector3(0, 1.6f, 0), new Vector3(.16f, 1.6f, .16f), Vector3.zero, Ferro);
                Peca(sinal.transform, PrimitiveType.Cube, new Vector3(0, 3.1f, 0), new Vector3(.5f, .9f, .3f), Vector3.zero, Ferrugem);
            }
            for (int i = 0; i < 10; i++) { var p = bosque + Rand(random, 10); if (Livre(p, 4)) Por(holder, cogumelos, p, .6f, (float)random.NextDouble() * 360, false); }

            // Trincheira dos Ecos: a dark seal and broken crystals, mushrooms around.
            var trincheira = Centro("Trincheira dos Ecos");
            Por(holder, selo, trincheira, 1.3f, 0, true);
            for (int i = 0; i < 8; i++) { var p = trincheira + Rand(random, 8); if (Livre(p, 3)) Por(holder, cogumelos, p, .7f, (float)random.NextDouble() * 360, false); }

            // Pátio de Manobras: the dead locomotive, wagons, barrels; the ruin behind.
            var patio = Centro("Pátio de Manobras");
            Locomotiva(holder, patio + new Vector2(7, 6), 51); // on the branch line, where it stopped
            VagaoVelho(holder, patio + new Vector2(-8, 5), 100, 0);
            VagaoVelho(holder, patio + new Vector2(-7, -8), 160, 85);
            Por(holder, Load("Cidade/RuinaGigante/RuinaGigante.prefab"), patio + new Vector2(-6, -15), 6f, 200, true);
            foreach (var o in new[] { new Vector2(4, -6), new Vector2(9, -2), new Vector2(-2, 9) })
                Por(holder, random.NextDouble() < .5f ? caixa : barril, patio + o + Rand(random, .6f), .9f, (float)random.NextDouble() * 360, true);
            // The Liga patrol's last banner, near the wagon with the scratched message.
            Por(holder, bandeira, patio + new Vector2(-10, 1), 2.2f, 80, true);

            // Recanto da Pedra Antiga: bushes close it on three sides; the carved stone in the middle.
            var recanto = new Vector2(TrilhosVelhos.Pedra.x, TrilhosVelhos.Pedra.z);
            for (int i = 0; i < 11; i++)
            {
                float a = (200 + i * 15) * Mathf.Deg2Rad;
                var p = recanto + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (3.6f + (float)random.NextDouble() * .8f);
                Por(holder, arbusto, p, 1.4f + (float)random.NextDouble() * .5f, (float)random.NextDouble() * 360, true);
            }
            var pedra = Mat("PedraGravada", new Color(.62f, .66f, .7f), .25f);
            var laje = GameObject.CreatePrimitive(PrimitiveType.Cube); laje.name = "PedraGravada"; laje.transform.SetParent(holder, false);
            laje.transform.position = new Vector3(recanto.x, Chao(recanto) + .9f, recanto.y - .8f); laje.transform.rotation = Quaternion.Euler(-12, 0, 0);
            laje.transform.localScale = new Vector3(2.2f, 1.8f, .35f); laje.layer = 9; laje.GetComponent<Renderer>().sharedMaterial = pedra;
            // Lines carved on its face (a river and mountains): thin dark strips.
            var sulco = Mat("Sulco", new Color(.2f, .24f, .3f), .1f);
            for (int i = 0; i < 5; i++)
                Peca(laje.transform, PrimitiveType.Cube, new Vector3(-.3f + i * .14f, -.2f + (i % 2) * .25f, -.52f), new Vector3(.32f, .03f, .05f), new Vector3(0, 0, 30 - i * 18), sulco);

            // Breu zones: dark crystals and broken rocks.
            foreach (var zona in TrilhosVelhos.Cantos.Where(c => c.tipo == NascenteDoRio.Tipo.Breu))
            {
                var z = new GameObject("ZonaDoBreu_" + zona.nome).transform; z.SetParent(holder, false);
                for (int i = 0; i < 5; i++)
                {
                    float h = i == 0 ? 3.2f : 1.2f + (float)random.NextDouble() * 1.2f;
                    var at = zona.centro + (i == 0 ? Vector2.zero : Rand(random, 1.3f));
                    var shard = Peca(z, PrimitiveType.Cube, Vector3.zero, new Vector3(.55f, h, .55f) * (i == 0 ? 1.2f : 1),
                        new Vector3((float)random.NextDouble() * 24 - 12, (float)random.NextDouble() * 360, 45 + (float)random.NextDouble() * 20 - 10), Breu);
                    shard.name = "Cristal"; shard.transform.position = new Vector3(at.x, Chao(at) + h * .4f, at.y);
                }
                for (int i = 0; i < 6; i++)
                {
                    float ang = i * 60 * Mathf.Deg2Rad; var at = zona.centro + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * (zona.raio * .55f + (float)random.NextDouble() * 2);
                    Por(z, Pedrinha, at, .6f + (float)random.NextDouble() * .7f, (float)random.NextDouble() * 360, false, .1f);
                }
            }
        }

        // ---------- Creatures of the line: variants of the known ones ----------
        static EnemyDefinition Inimigo(string path) => AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/ForjaDasMares/Data/" + path + ".asset");

        static EnemyDefinition Variante(string from, string id, string nome, int vida, float velocidade, Color cor, bool miniboss)
        {
            const string folder = "Assets/ForjaDasMares/Data/Enemies/Trilhos";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/ForjaDasMares/Data/Enemies", "Trilhos");
            string path = folder + "/" + id + ".asset";
            var def = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (def == null)
            {
                if (!AssetDatabase.CopyAsset("Assets/ForjaDasMares/Data/Enemies/" + from + ".asset", path)) return null;
                def = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
                if (def == null) return null;
            }
            def.enemyId = id; def.displayName = nome; def.maximumHealth = vida; def.color = cor; def.miniboss = miniboss;
            var source = Inimigo("Enemies/" + from);
            if (source != null) def.speed = source.speed * velocidade;
            EditorUtility.SetDirty(def);
            return def;
        }

        sealed class Lista
        {
            public EnemyDefinition rato, sentinela, maquinista, raspador, fagulha, guarda, vigia, eco, fragmento, custodio;
            public IEnumerable<EnemyDefinition> Todos => new[] { rato, sentinela, maquinista, raspador, fagulha, guarda, vigia, eco, fragmento, custodio }.Where(e => e != null);
        }

        static Lista Inimigos()
        {
            var l = new Lista
            {
                rato = Variante("raspador", "rato-ferrugem", "Rato de Ferrugem", 30, 1.3f, new Color(.78f, .45f, .25f), false),
                sentinela = Variante("vigia-sombrio", "sentinela-trilhos", "Sentinela dos Trilhos", 55, 1f, new Color(.85f, .62f, .3f), false),
                maquinista = Variante("guarda-mare", "maquinista", "Maquinista Tomado", 240, .9f, new Color(.45f, .3f, .6f), true),
                raspador = Inimigo("Enemies/raspador"), fagulha = Inimigo("Enemies/fagulha"), guarda = Inimigo("Enemies/guarda-mare"),
                vigia = Inimigo("Enemies/vigia-sombrio"), eco = Inimigo("Enemies/eco-forja"), fragmento = Inimigo("Enemies/fragmento"),
                custodio = Inimigo("Enemies/custodio"),
            };
            // The Custódio used the big-agent navigation type, which these places do not bake: it never appeared.
            // Here it walks on the normal navigation like everyone else.
            if (l.custodio != null) { l.custodio.displayName = "Custódio dos Trilhos"; l.custodio.agentTypeId = 0; EditorUtility.SetDirty(l.custodio); }
            AssetDatabase.SaveAssets();
            return l;
        }

        static Vector3 P(float x, float z) => new Vector3(x, Chao(new Vector2(x, z)) + .05f, z);
        static Vector3 P(Vector2 c, float dx, float dz) => P(c.x + dx, c.y + dz);

        static void Regiao(RegionalWorld world, Lista e)
        {
            var region = AssetDatabase.LoadAssetAtPath<RegionDefinition>(Data + "trilhos.asset");
            if (world == null || region == null) { Debug.LogError("Trilhos: não achei o RegionalWorld ou a região (trilhos.asset)."); return; }
            region.displayName = "Trilhos Velhos";
            region.refuge = TrilhosVelhos.Chegada; region.route = new[] { region.refuge };
            region.activeEnemyLimit = 16;
            var vagoes = Centro("Vagões Tombados"); var bosque = Centro("Bosque de Ferro"); var trincheira = Centro("Trincheira dos Ecos"); var patio = Centro("Pátio de Manobras");
            var camps = new List<RegionCampDefinition>
            {
                new RegionCampDefinition { label = "Vagões Tombados", center = P(vagoes, 0, 0), respawnSeconds = 150,
                    spawns = new[] { P(vagoes, -3, -2), P(vagoes, 3, 1), P(vagoes, 0, 4), P(vagoes, -1, -5) }, enemies = new[] { e.rato, e.rato, e.rato, e.maquinista } },
                new RegionCampDefinition { label = "Bosque de Ferro", center = P(bosque, 0, 0), respawnSeconds = 120,
                    spawns = new[] { P(bosque, -4, 2), P(bosque, 4, -1), P(bosque, 0, -4), P(bosque, 2, 5) }, enemies = new[] { e.sentinela, e.sentinela, e.guarda, e.vigia } },
                new RegionCampDefinition { label = "Trincheira dos Ecos", center = P(trincheira, 0, 0), respawnSeconds = 140,
                    spawns = new[] { P(trincheira, 0, 2), P(trincheira, -4, -2), P(trincheira, 4, -2) }, enemies = new[] { e.eco, e.fagulha, e.fagulha } },
                new RegionCampDefinition { label = "Pátio de Manobras", center = P(TrilhosVelhos.Chefe.x, TrilhosVelhos.Chefe.z), respawnSeconds = 300,
                    spawns = new[] { P(TrilhosVelhos.Chefe.x, TrilhosVelhos.Chefe.z), P(patio, -5, 3), P(patio, 5, 3) }, enemies = new[] { e.custodio, e.sentinela, e.sentinela } },
                // A few rats along the line between the camps (the line is never fully safe).
                new RegionCampDefinition { label = "Linha", center = P(-20, 20), respawnSeconds = 90,
                    spawns = new[] { P(-22, 22), P(-18, 18) }, enemies = new[] { e.rato, e.raspador } },
            };
            foreach (var c in camps.Where(c => c.enemies.Any(x => x == null))) Debug.LogWarning("TRILHOS: acampamento sem criatura: " + c.label);
            camps.RemoveAll(c => c.enemies.Any(x => x == null));
            region.camps = camps.ToArray();
            region.bounds = new Bounds(Vector3.zero, new Vector3(Half * 2 + 4, 40, Half * 2 + 4));
            EditorUtility.SetDirty(region);
            var pool = world.Enemies;
            if (pool != null)
            {
                var so = new SerializedObject(pool); var list = so.FindProperty("definitions");
                var all = e.Todos.ToArray();
                list.arraySize = all.Length;
                for (int i = 0; i < all.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = all[i];
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            var hero = world.Hero; if (hero != null) hero.transform.position = region.refuge;
            EditorUtility.SetDirty(world);
        }

        static void Luz()
        {
            // Late afternoon, warm and dusty.
            var sun = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(x => x.type == LightType.Directional);
            if (sun != null) { sun.intensity = 1.25f; sun.color = new Color(1f, .84f, .66f); sun.transform.rotation = Quaternion.Euler(38, 60, 0); }
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 55; RenderSettings.fogEndDistance = 165;
            RenderSettings.fogColor = new Color(.62f, .55f, .45f); RenderSettings.ambientLight = new Color(.47f, .44f, .4f);
        }

        static bool Navegacao()
        {
            var go = new GameObject("AreaNavigation");
            var nav = go.AddComponent<NavMeshSurface>();
            nav.layerMask = (1 << 8) | (1 << 9); nav.useGeometry = NavMeshCollectGeometry.PhysicsColliders; nav.collectObjects = CollectObjects.All;
            nav.BuildNavMesh();
            string navPath = Data + "trilhos_Navegacao.asset";
            var prior = AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
            if (prior == null) AssetDatabase.CreateAsset(nav.navMeshData, navPath); else { EditorUtility.CopySerialized(nav.navMeshData, prior); nav.navMeshData = prior; }
            EditorUtility.SetDirty(nav);
            bool ok = true;
            if (!NavMesh.SamplePosition(TrilhosVelhos.Chegada, out var start, 4, NavMesh.AllAreas)) { Debug.LogWarning("TRILHOS: a chegada não tem chão andável."); return false; }
            var alvos = new List<(string, Vector3)>();
            foreach (var saida in AreasDoMundo.PorId("trilhos").saidas) alvos.Add(("saida " + saida.para, AreasDoMundo.Ponto(saida)));
            alvos.Add(("chefe", TrilhosVelhos.Chefe)); alvos.Add(("pedra", TrilhosVelhos.Pedra + new Vector3(0, 0, 2))); alvos.Add(("emboscada", TrilhosVelhos.Emboscada));
            alvos.Add(("bilhete", TrilhosVelhos.Bilhete));
            foreach (var pg in TrilhosVelhos.Paginas) alvos.Add(("pagina", pg));
            foreach (var c in TrilhosVelhos.Cantos) alvos.Add((c.nome, new Vector3(c.centro.x, 0, c.centro.y)));
            foreach (var (nome, alvo) in alvos)
            {
                var a = new Vector3(alvo.x, Chao(new Vector2(alvo.x, alvo.z)), alvo.z);
                var path = new NavMeshPath();
                if (!NavMesh.SamplePosition(a, out var end, 5, NavMesh.AllAreas) || !NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete)
                { Debug.LogWarning("TRILHOS: sem caminho até " + nome + " " + alvo); ok = false; }
            }
            return ok;
        }
    }
}
