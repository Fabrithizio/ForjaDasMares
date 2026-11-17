using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ForjaDasMares.Networking.Editor
{
    // Bumerangue do Coração: the three Meshy models (V1 wood, V2 silver, V3 crystal wings) become
    // three weapons with their own mesh, materials, flying model and inventory icon.
    public static class CoracaoInstaller
    {
        const string Root = "Assets/ForjaDasMares/Art/Arsenal/Coracao/";
        const string RoseFolder = "Assets/ForjaDasMares/Art/Arsenal/RoseBoomerang/";
        const string CatalogPath = "Assets/ForjaDasMares/Data/Campaign/Forge/Catalog.asset";
        public static readonly string[] Ids = { "coracao-v1", "coracao-v2", "coracao-v3" };

        sealed class Version
        {
            public string folder, id, name, description;
            public float size, held, emission, scale;
            public int damage, heavyDamage, textureSize;
            public Color backdrop, glow;
            public int glowRadius;
            public bool sparkles;
            // Heart effects
            public Color fxMain, fxSecond;
            public float trailWidth, trailTime, heartsPerMetre, sparklesPerMetre, haloSize, impactSize;
            public Vector2 heartSize;
            public int impactHearts;
            public bool bigHeart;
        }

        static readonly Version[] Versions =
        {
            new Version { folder = "V1", id = Ids[0], name = "Bumerangue do Coração · Broto", size = 1.15f, held = .5f, emission = 0f, damage = 13, heavyDamage = 18, textureSize = 1024,
                description = "Madeira clara, bordas de aço e um pequeno coração rosa. A primeira forma da relíquia: vai até o inimigo e volta à mão.",
                backdrop = new Color(1f, .78f, .48f, .22f), glow = new Color(1f, .62f, .78f, .25f), glowRadius = 5,
                fxMain = new Color(1f, .5f, .74f), fxSecond = new Color(1f, .78f, .6f), trailWidth = .4f, trailTime = .22f, heartsPerMetre = 1.4f, heartSize = new Vector2(.14f, .24f), sparklesPerMetre = 0, haloSize = 0, impactHearts = 6, impactSize = .65f },
            new Version { folder = "V2", id = Ids[1], name = "Bumerangue do Coração · Desperto", size = 1.3f, held = .56f, emission = .2f, damage = 17, heavyDamage = 24, textureSize = 1024,
                description = "As lâminas viraram duas luas de prata e o coração despertou. O arremesso deixa um rastro de corações.",
                backdrop = new Color(.82f, .88f, 1f, .24f), glow = new Color(1f, .42f, .74f, .6f), glowRadius = 8,
                fxMain = new Color(1f, .32f, .7f), fxSecond = new Color(.82f, .58f, 1f), trailWidth = .7f, trailTime = .32f, heartsPerMetre = 2.8f, heartSize = new Vector2(.2f, .36f), sparklesPerMetre = 3, haloSize = .8f, impactHearts = 11, impactSize = .9f },
            new Version { folder = "V3", id = Ids[2], name = "Bumerangue do Coração · Asa do Coração", size = 1.45f, held = .62f, emission = .5f, damage = 22, heavyDamage = 31, textureSize = 2048,
                description = "A forma final: luas douradas, asas de cristal e o coração em chamas rosas. Cada arremesso floresce em corações.",
                backdrop = new Color(1f, .5f, .86f, .3f), glow = new Color(1f, .45f, .86f, .95f), glowRadius = 12, sparkles = true,
                fxMain = new Color(1f, .28f, .72f), fxSecond = new Color(.72f, .42f, 1f), trailWidth = 1.05f, trailTime = .45f, heartsPerMetre = 5f, heartSize = new Vector2(.26f, .52f), sparklesPerMetre = 7, haloSize = 1.4f, impactHearts = 18, impactSize = 1.25f, bigHeart = true },
        };

        [MenuItem("Forja/Arsenal/Bumerangue do Coração (3 versões)")]
        public static void Apply()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ProgressionCatalog>(CatalogPath);
            var baseItem = AssetDatabase.LoadAssetAtPath<EquipmentDefinition>(RoseFolder + "AsaDoRosal.asset");
            var baseThrow = AssetDatabase.LoadAssetAtPath<AttackDefinition>(RoseFolder + "ReturningThrow.asset");
            var baseBurst = AssetDatabase.LoadAssetAtPath<AttackDefinition>(RoseFolder + "RosalBurst.asset");
            var baseAbility = AssetDatabase.LoadAssetAtPath<AbilityDefinition>(RoseFolder + "RosalAbility.asset");
            if (catalog == null || baseItem == null || baseThrow == null || baseBurst == null || baseAbility == null)
            { Debug.LogError("Coração: faltam os arquivos da Asa do Rosal ou o catálogo. Rode antes Forja/Arsenal/Gerar Asa do Rosal."); return; }

            var report = new System.Text.StringBuilder("CORACAO_PRONTO");
            var shared = SharedFx();
            foreach (var v in Versions)
            {
                string folder = Root + v.folder + "/";
                string fbx = folder + "Modelo.fbx", png = folder + "Textura.png";
                if (!File.Exists(fbx) || !File.Exists(png)) { Debug.LogError("Coração: faltando " + fbx + " ou " + png); continue; }

                if (AssetImporter.GetAtPath(png) is TextureImporter texture)
                {
                    texture.textureType = TextureImporterType.Default;
                    texture.maxTextureSize = v.textureSize;
                    texture.textureCompression = TextureImporterCompression.Compressed;
                    texture.sRGBTexture = true; texture.mipmapEnabled = true;
                    texture.SaveAndReimport();
                }
                if (AssetImporter.GetAtPath(fbx) is ModelImporter model)
                {
                    model.materialImportMode = ModelImporterMaterialImportMode.None;
                    model.animationType = ModelImporterAnimationType.None;
                    model.importAnimation = false; model.importCameras = false; model.importLights = false;
                    model.isReadable = true;
                    model.SaveAndReimport();
                }

                var mesh = Save(BakeMesh(fbx, v.size, v.name), folder + "Malha.asset");
                var map = AssetDatabase.LoadAssetAtPath<Texture2D>(png);
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetTexture("_BaseMap", map);
                material.SetColor("_BaseColor", Color.white);
                material.SetFloat("_Smoothness", .35f);
                material.SetFloat("_Metallic", 0f);
                if (v.emission > 0)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetTexture("_EmissionMap", map);
                    material.SetColor("_EmissionColor", Color.white * v.emission);
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                }
                material.enableInstancing = true;
                material = Save(material, folder + "Material.mat");
                var materials = new[] { material };

                var hit = Object.Instantiate(baseThrow);
                hit.name = "Arremesso";
                hit.damage = v.damage;
                hit.projectileVisual = mesh; hit.projectileMaterials = materials; hit.projectileVisualScale = 1f;
                hit.color = v.glow.a > .5f ? new Color(v.glow.r, v.glow.g, v.glow.b, 1) : new Color(1f, .55f, .75f, 1);
                hit.heartFx = Style(v, folder, shared);
                hit = Save(hit, folder + "Arremesso.asset");
                var heavy = Object.Instantiate(baseBurst);
                heavy.name = "ArremessoForte";
                heavy.damage = v.heavyDamage;
                heavy.projectileVisual = mesh; heavy.projectileMaterials = materials; heavy.projectileVisualScale = 1.1f;
                heavy.color = hit.color;
                heavy.heartFx = hit.heartFx;
                heavy = Save(heavy, folder + "ArremessoForte.asset");
                var ability = Object.Instantiate(baseAbility);
                ability.hit = heavy;
                ability = Save(ability, folder + "Habilidade.asset");

                var icon = WriteIcon(mesh, png, folder + "Icone.png", v);

                var item = Object.Instantiate(baseItem);
                item.itemId = v.id;
                item.displayName = v.name;
                item.description = v.description;
                item.visualMesh = mesh;
                item.visualMaterials = materials;
                item.authoredScale = Vector3.one * .65f;
                item.heldLength = v.held;
                item.color = hit.color;
                item.icon = icon;
                item.attacks = new[] { hit };
                item.ability = ability;
                item = Save(item, folder + "Arma.asset");

                if (catalog.Find(v.id) == null)
                {
                    var equipment = catalog.equipment;
                    Array.Resize(ref equipment, equipment.Length + 1);
                    equipment[equipment.Length - 1] = item;
                    catalog.equipment = equipment;
                }
                EditorUtility.SetDirty(catalog);
                report.Append(" | ").Append(v.folder).Append(' ').Append(mesh.triangles.Length / 3).Append(" triângulos");
            }
            // Evolution in the Forge: V1 becomes V2 at +8, V2 becomes V3 at +12 (the forge level is kept).
            var items = new EquipmentDefinition[Versions.Length];
            for (int i = 0; i < Versions.Length; i++) items[i] = AssetDatabase.LoadAssetAtPath<EquipmentDefinition>(Root + Versions[i].folder + "/Arma.asset");
            int[] evolveAt = { 8, 12 };
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;
                items[i].evolvesInto = i + 1 < items.Length ? items[i + 1] : null;
                items[i].evolveAtForge = i < evolveAt.Length ? evolveAt[i] : 0;
                EditorUtility.SetDirty(items[i]);
            }
            catalog.Validate();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(report.ToString());
        }

        // Copies the model into one mesh lying flat in X/Y (thin side on Z), centred, with its longest side = size.
        // This is the same layout the flying boomerang and the hand socket already expect.
        static Mesh BakeMesh(string fbx, float size, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            var go = (GameObject)Object.Instantiate(prefab);
            go.transform.SetPositionAndRotation(Vector3.zero, prefab.transform.rotation);
            var positions = new List<Vector3>(); var normals = new List<Vector3>(); var uvs = new List<Vector2>(); var triangles = new List<int>();
            foreach (var filter in go.GetComponentsInChildren<MeshFilter>())
            {
                var source = filter.sharedMesh; if (source == null) continue;
                var matrix = filter.transform.localToWorldMatrix;
                bool flip = matrix.determinant < 0;
                int offset = positions.Count;
                var v = source.vertices; var n = source.normals; var uv = source.uv;
                for (int i = 0; i < v.Length; i++)
                {
                    positions.Add(matrix.MultiplyPoint3x4(v[i]));
                    normals.Add(n.Length == v.Length ? matrix.MultiplyVector(n[i]).normalized : Vector3.forward);
                    uvs.Add(uv.Length == v.Length ? uv[i] : Vector2.zero);
                }
                for (int s = 0; s < source.subMeshCount; s++)
                {
                    var t = source.GetTriangles(s);
                    for (int i = 0; i < t.Length; i += 3)
                    {
                        triangles.Add(offset + t[i]);
                        triangles.Add(offset + (flip ? t[i + 2] : t[i + 1]));
                        triangles.Add(offset + (flip ? t[i + 1] : t[i + 2]));
                    }
                }
            }
            Object.DestroyImmediate(go);

            var bounds = new Bounds(positions[0], Vector3.zero);
            foreach (var p in positions) bounds.Encapsulate(p);
            int thin = bounds.size.x <= bounds.size.y && bounds.size.x <= bounds.size.z ? 0 : bounds.size.y <= bounds.size.z ? 1 : 2;
            // Proper rotations only (no mirroring): the thin axis goes to Z.
            Quaternion turn = thin == 2 ? Quaternion.identity : thin == 1 ? Quaternion.Euler(-90, 0, 0) : Quaternion.Euler(0, 90, 0);
            for (int i = 0; i < positions.Count; i++) { positions[i] = turn * (positions[i] - bounds.center); normals[i] = turn * normals[i]; }
            bounds = new Bounds(positions[0], Vector3.zero);
            foreach (var p in positions) bounds.Encapsulate(p);
            float scale = size / Mathf.Max(bounds.size.x, bounds.size.y, .0001f);
            for (int i = 0; i < positions.Count; i++) positions[i] = (positions[i] - bounds.center) * scale;

            var mesh = new Mesh { name = name, indexFormat = positions.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16 };
            mesh.SetVertices(positions); mesh.SetNormals(normals); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds(); mesh.RecalculateTangents();
            return mesh;
        }

        // ---------- Inventory icon: the painted model drawn straight from its texture, plus outline, glow and backdrop ----------
        static Sprite WriteIcon(Mesh mesh, string texturePath, string iconPath, Version v)
        {
            const int Big = 512, Size = 256;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(texturePath));
            var color = new Color[Big * Big]; var depth = new float[Big * Big];
            for (int i = 0; i < depth.Length; i++) depth[i] = float.MaxValue;

            var vertices = mesh.vertices; var uv = mesh.uv; var tris = mesh.triangles;
            var b = mesh.bounds;
            float fit = Big * (v.sparkles ? .8f : .84f) / Mathf.Max(b.size.x, b.size.y);
            Vector2 Screen(Vector3 p) => new Vector2(Big * .5f + (p.x - b.center.x) * fit, Big * .5f + (p.y - b.center.y) * fit);
            var light = new Vector3(-.45f, .6f, -.66f).normalized;
            for (int t = 0; t < tris.Length; t += 3)
            {
                Vector3 p0 = vertices[tris[t]], p1 = vertices[tris[t + 1]], p2 = vertices[tris[t + 2]];
                Vector2 a = Screen(p0), c = Screen(p1), d = Screen(p2);
                float area = (c.x - a.x) * (d.y - a.y) - (c.y - a.y) * (d.x - a.x);
                if (Mathf.Abs(area) < 1e-6f) continue;
                Vector3 normal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                if (normal.z > 0) normal = -normal; // the side facing the camera
                float shade = Mathf.Clamp(.84f + .3f * Vector3.Dot(normal, light), .7f, 1.12f);
                int minX = Mathf.Max(0, (int)Mathf.Floor(Mathf.Min(a.x, c.x, d.x))), maxX = Mathf.Min(Big - 1, (int)Mathf.Ceil(Mathf.Max(a.x, c.x, d.x)));
                int minY = Mathf.Max(0, (int)Mathf.Floor(Mathf.Min(a.y, c.y, d.y))), maxY = Mathf.Min(Big - 1, (int)Mathf.Ceil(Mathf.Max(a.y, c.y, d.y)));
                for (int y = minY; y <= maxY; y++)
                    for (int x = minX; x <= maxX; x++)
                    {
                        float px = x + .5f, py = y + .5f;
                        float w0 = ((c.x - px) * (d.y - py) - (c.y - py) * (d.x - px)) / area;
                        float w1 = ((d.x - px) * (a.y - py) - (d.y - py) * (a.x - px)) / area;
                        float w2 = 1 - w0 - w1;
                        if (w0 < 0 || w1 < 0 || w2 < 0) continue;
                        float z = w0 * p0.z + w1 * p1.z + w2 * p2.z;
                        int index = y * Big + x;
                        if (z >= depth[index]) continue;
                        depth[index] = z;
                        Vector2 st = w0 * uv[tris[t]] + w1 * uv[tris[t + 1]] + w2 * uv[tris[t + 2]];
                        var sample = tex.GetPixelBilinear(st.x, st.y);
                        color[index] = new Color(sample.r * shade, sample.g * shade, sample.b * shade, 1);
                    }
            }
            Object.DestroyImmediate(tex);

            // 2x2 down-sample = smooth edges.
            var model = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Color sum = Color.clear;
                    for (int j = 0; j < 2; j++) for (int i = 0; i < 2; i++) { var s = color[(y * 2 + j) * Big + x * 2 + i]; sum += new Color(s.r * s.a, s.g * s.a, s.b * s.a, s.a); }
                    sum /= 4;
                    model[y * Size + x] = sum.a > 0 ? new Color(sum.r / sum.a, sum.g / sum.a, sum.b / sum.a, sum.a) : Color.clear;
                }

            var alpha = new float[Size * Size];
            for (int i = 0; i < alpha.Length; i++) alpha[i] = model[i].a;
            var outline = Blur(Dilate(alpha, Size, 2), Size, 1);
            var glow = Blur(Dilate(alpha, Size, 2), Size, v.glowRadius);

            var result = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    int i = y * Size + x;
                    // Soft round backdrop.
                    float dx = (x - Size * .5f) / (Size * .5f), dy = (y - Size * .5f) / (Size * .5f);
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    Color pixel = v.backdrop; pixel.a *= Mathf.Clamp01((.92f - r) / .3f);
                    pixel = Over(pixel, new Color(v.glow.r, v.glow.g, v.glow.b, Mathf.Clamp01(glow[i] * 1.6f) * v.glow.a));
                    pixel = Over(pixel, new Color(.16f, .05f, .12f, Mathf.Clamp01(outline[i] * 1.4f) * .92f));
                    pixel = Over(pixel, model[i]);
                    result[i] = pixel;
                }
            if (v.sparkles)
            {
                Heart(result, Size, new Vector2(.16f, .80f), .07f, new Color(1f, .5f, .82f, .95f));
                Heart(result, Size, new Vector2(.86f, .18f), .055f, new Color(1f, .62f, .88f, .9f));
                Heart(result, Size, new Vector2(.84f, .86f), .04f, new Color(1f, .78f, .93f, .85f));
                Star(result, Size, new Vector2(.12f, .22f), .055f, new Color(1f, .95f, .85f, .95f));
                Star(result, Size, new Vector2(.9f, .55f), .04f, new Color(1f, .9f, .97f, .9f));
            }

            var icon = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            icon.SetPixels(result); icon.Apply();
            File.WriteAllBytes(iconPath, icon.EncodeToPNG());
            Object.DestroyImmediate(icon);
            AssetDatabase.ImportAsset(iconPath, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(iconPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 256;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        }

        static Color Over(Color under, Color over)
        {
            float a = over.a + under.a * (1 - over.a);
            if (a <= 0) return Color.clear;
            return new Color((over.r * over.a + under.r * under.a * (1 - over.a)) / a, (over.g * over.a + under.g * under.a * (1 - over.a)) / a, (over.b * over.a + under.b * under.a * (1 - over.a)) / a, a);
        }

        static float[] Dilate(float[] a, int size, int radius)
        {
            var o = new float[a.Length];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float m = 0;
                    for (int j = -radius; j <= radius && m < 1; j++)
                        for (int i = -radius; i <= radius; i++)
                        {
                            if (i * i + j * j > radius * radius) continue;
                            int xx = x + i, yy = y + j;
                            if (xx < 0 || yy < 0 || xx >= size || yy >= size) continue;
                            m = Mathf.Max(m, a[yy * size + xx]);
                        }
                    o[y * size + x] = m;
                }
            return o;
        }

        static float[] Blur(float[] a, int size, int radius)
        {
            if (radius <= 0) return a;
            var h = new float[a.Length]; var o = new float[a.Length];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float s = 0; int n = 0;
                    for (int i = -radius; i <= radius; i++) { int xx = x + i; if (xx < 0 || xx >= size) continue; s += a[y * size + xx]; n++; }
                    h[y * size + x] = s / n;
                }
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float s = 0; int n = 0;
                    for (int j = -radius; j <= radius; j++) { int yy = y + j; if (yy < 0 || yy >= size) continue; s += h[yy * size + x]; n++; }
                    o[y * size + x] = s / n;
                }
            return o;
        }

        static void Heart(Color[] pixels, int size, Vector2 centre, float radius, Color tint)
        {
            int r = Mathf.CeilToInt(radius * size * 1.3f);
            int cx = (int)(centre.x * size), cy = (int)(centre.y * size);
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                {
                    int px = cx + x, py = cy + y;
                    if (px < 0 || py < 0 || px >= size || py >= size) continue;
                    float u = x / (radius * size), w = y / (radius * size) + .15f;
                    float f = Mathf.Pow(u * u + w * w - 1, 3) - u * u * w * w * w;
                    float edge = Mathf.Clamp01(-f * 6);
                    if (edge <= 0) continue;
                    var c = tint; c.a *= edge;
                    pixels[py * size + px] = Over(pixels[py * size + px], c);
                }
        }

        static void Star(Color[] pixels, int size, Vector2 centre, float radius, Color tint)
        {
            int r = Mathf.CeilToInt(radius * size);
            int cx = (int)(centre.x * size), cy = (int)(centre.y * size);
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                {
                    int px = cx + x, py = cy + y;
                    if (px < 0 || py < 0 || px >= size || py >= size) continue;
                    float u = Mathf.Abs(x) / (float)r, w = Mathf.Abs(y) / (float)r;
                    float d = Mathf.Sqrt(u) + Mathf.Sqrt(w);
                    float edge = Mathf.Clamp01((1 - d) * 4);
                    if (edge <= 0) continue;
                    var c = tint; c.a *= edge;
                    pixels[py * size + px] = Over(pixels[py * size + px], c);
                }
        }

        // ---------- Heart effects: textures and additive materials made once, one style per version ----------
        sealed class Shared { public Material heart, glow, ring, core; public Texture2D trail; }

        static Shared SharedFx()
        {
            string fx = Root + "Efeitos/";
            if (!AssetDatabase.IsValidFolder(Root + "Efeitos")) AssetDatabase.CreateFolder(Root.TrimEnd('/'), "Efeitos");
            var heart = Texture(fx + "Coracao.png", 128, (u, v) =>
            {
                float x = (u - .5f) * 2.6f, y = (v - .47f) * 2.6f + .1f;
                float f = Mathf.Pow(x * x + y * y - 1, 3) - x * x * y * y * y;
                float inside = Mathf.Clamp01(-f * 8);
                float halo = Mathf.Exp(-Mathf.Max(0, f) * 3.5f) * .45f;
                float core = Mathf.Clamp01(1 - Mathf.Sqrt(x * x + (y - .2f) * (y - .2f)) * .9f);
                return new Color(1, 1, 1, Mathf.Clamp01(Mathf.Max(inside * (.75f + .25f * core), halo)));
            }, false);
            var glow = Texture(fx + "Brilho.png", 64, (u, v) =>
            {
                float d = Mathf.Clamp01(Vector2.Distance(new Vector2(u, v), Vector2.one * .5f) * 2);
                return new Color(1, 1, 1, Mathf.Pow(1 - d, 2.2f));
            }, false);
            var ring = Texture(fx + "Anel.png", 128, (u, v) =>
            {
                float d = Vector2.Distance(new Vector2(u, v), Vector2.one * .5f) * 2;
                return new Color(1, 1, 1, Mathf.Clamp01(Mathf.Exp(-Mathf.Pow((d - .78f) / .09f, 2)) + Mathf.Clamp01(1 - d) * .18f));
            }, false);
            // Ribbon tile: a soft band with a heart drawn in the middle (repeats along the trail).
            var trail = Texture(fx + "Faixa.png", 128, (u, v) =>
            {
                float band = Mathf.Exp(-Mathf.Pow((v - .5f) / .3f, 2)) * .55f;
                float x = (v - .5f) * 3.4f, y = (u - .5f) * 3.4f + .1f; // heart pointing along the trail
                float f = Mathf.Pow(x * x + y * y - 1, 3) - x * x * y * y * y;
                float rim = Mathf.Exp(-Mathf.Abs(f) * 14f);
                float fill = Mathf.Clamp01(-f * 6) * .55f;
                return new Color(1, 1, 1, Mathf.Clamp01(band + Mathf.Max(rim, fill)));
            }, true);
            var core = Texture(fx + "Nucleo.png", 64, (u, v) => new Color(1, 1, 1, Mathf.Exp(-Mathf.Pow((v - .5f) / .2f, 2))), false);
            return new Shared
            {
                heart = Additive(fx + "Coracao.mat", heart, Vector2.one),
                glow = Additive(fx + "Brilho.mat", glow, Vector2.one),
                ring = Additive(fx + "Anel.mat", ring, Vector2.one),
                core = Additive(fx + "Nucleo.mat", core, Vector2.one),
                trail = trail
            };
        }

        static HeartFxStyle Style(Version v, string folder, Shared shared)
        {
            var style = ScriptableObject.CreateInstance<HeartFxStyle>();
            style.heart = shared.heart; style.glow = shared.glow; style.ring = shared.ring; style.trailCore = shared.core;
            // One heart per tile: the tile is as long as the ribbon is wide, so hearts keep their shape.
            style.trail = Additive(folder + "Rastro.mat", shared.trail, new Vector2(1f / Mathf.Max(.2f, v.trailWidth), 1));
            style.main = v.fxMain; style.second = v.fxSecond;
            style.trailWidth = v.trailWidth; style.trailTime = v.trailTime;
            style.heartsPerMetre = v.heartsPerMetre; style.heartSize = v.heartSize;
            style.sparklesPerMetre = v.sparklesPerMetre; style.haloSize = v.haloSize;
            style.impactHearts = v.impactHearts; style.impactSize = v.impactSize; style.bigHeart = v.bigHeart;
            return Save(style, folder + "Efeito.asset");
        }

        static Texture2D Texture(string path, int size, Func<float, float, Color> paint, bool repeatU)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    pixels[y * size + x] = paint((x + .5f) / size, (y + .5f) / size);
            texture.SetPixels(pixels); texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true; importer.sRGBTexture = true; importer.mipmapEnabled = true;
                importer.wrapModeU = repeatU ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                importer.wrapModeV = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // Additive transparent particle material (glows over everything, never darkens).
        static Material Additive(string path, Texture2D texture, Vector2 tiling)
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            var m = new Material(shader);
            m.SetTexture("_BaseMap", texture);
            m.SetTextureScale("_BaseMap", tiling);
            m.SetColor("_BaseColor", Color.white);
            m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 2);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            m.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            m.SetFloat("_ZWrite", 0);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return Save(m, path);
        }

        static T Save<T>(T asset, string path) where T : Object
        {
            var saved = AssetDatabase.LoadAssetAtPath<T>(path);
            if (saved == null) { AssetDatabase.CreateAsset(asset, path); return asset; }
            if (asset is Mesh source && saved is Mesh target)
            {
                target.Clear();
                target.indexFormat = source.indexFormat;
                target.SetVertices(source.vertices); target.SetNormals(source.normals); target.SetUVs(0, source.uv);
                target.SetTriangles(source.triangles, 0);
                target.RecalculateBounds(); target.RecalculateTangents();
                Object.DestroyImmediate(source);
                EditorUtility.SetDirty(target);
                return saved;
            }
            EditorUtility.CopySerialized(asset, saved);
            Object.DestroyImmediate(asset);
            EditorUtility.SetDirty(saved);
            return saved;
        }
    }
}
