using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ForjaDasMares.Networking.Editor
{
    // Perdição de Netuno (legendary axe made by Fabricio in Meshy). This is its last form (V3); the two weaker
    // forms will come later, in V1 and V2. The installer turns the model into a weapon of the game:
    // mesh, material (only the blue parts glow), inventory icon and its own attacks:
    //   combo 1-2  Golpe Amplo            wide sweep of water (cone 200°)
    //   combo 3    Impacto do Maremoto    the axe falls on the ground: circle of water and crystals around
    //   charged    Onda Avançante         a wave runs forward along the ground (long line)
    //   super      Fúria do Oceano        a water whirlwind around the hero (big circle)
    // Menu: Forja/Arsenal/Perdição de Netuno.
    [InitializeOnLoad]
    public static class NetunoInstaller
    {
        // Runs by itself once the model is in the project and the weapon was not made yet (or after a change here).
        const int Version = 5; // 2: livelier colours · 3: the combo is one side slash, repeated · 4: closer reach · 5: combo of 3 sideways blows, passive wave, bull Super
        static NetunoInstaller() { EditorApplication.update += Tentar; }
        static void Tentar()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            EditorApplication.update -= Tentar;
            {
                if (File.Exists(Folder + "Modelo.fbx") && (!File.Exists(Folder + "Arma.asset") || EditorPrefs.GetInt("Forja.Netuno.Versao", 0) < Version))
                {
                    try { Apply(); }
                    catch (Exception e) { File.WriteAllText("Logs/netuno.txt", DateTime.Now + "\nERRO " + e); Debug.LogException(e); }
                }
            }
        }
        public const string Id = "netuno-v3";
        const string Folder = "Assets/ForjaDasMares/Art/Arsenal/Netuno/V3/";
        const string CatalogPath = "Assets/ForjaDasMares/Data/Campaign/Forge/Catalog.asset";

        [MenuItem("Forja/Arsenal/Perdição de Netuno")]
        public static void Apply()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ProgressionCatalog>(CatalogPath);
            if (catalog == null) { Debug.LogError("NETUNO: catálogo não encontrado"); return; }
            EquipmentDefinition baseAxe = null;
            foreach (var e in catalog.equipment) if (e != null && e.category == WeaponCategory.Axe && !e.starter && e.attacks != null && e.attacks.Length > 0) { baseAxe = e; break; }
            if (baseAxe == null) foreach (var e in catalog.equipment) if (e != null && e.category == WeaponCategory.Axe && e.attacks != null && e.attacks.Length > 0) { baseAxe = e; break; }
            if (baseAxe == null) { Debug.LogError("NETUNO: nenhum machado no catálogo para servir de base"); return; }

            string fbx = Folder + "Modelo.fbx", png = Folder + "Textura.png";
            if (!File.Exists(fbx) || !File.Exists(png)) { Debug.LogError("NETUNO: faltando " + fbx + " ou " + png); return; }
            if (AssetImporter.GetAtPath(png) is TextureImporter texture)
            {
                texture.textureType = TextureImporterType.Default; texture.maxTextureSize = 2048;
                texture.textureCompression = TextureImporterCompression.Compressed; texture.sRGBTexture = true; texture.mipmapEnabled = true;
                texture.isReadable = true;
                texture.SaveAndReimport();
            }
            if (AssetImporter.GetAtPath(fbx) is ModelImporter model)
            {
                model.materialImportMode = ModelImporterMaterialImportMode.None; model.animationType = ModelImporterAnimationType.None;
                model.importAnimation = false; model.importCameras = false; model.importLights = false; model.isReadable = true;
                model.SaveAndReimport();
            }

            var mesh = Save(BakeMesh(fbx, 1.3f, "PerdicaoDeNetuno"), Folder + "Malha.asset");
            // Same treatment as the heroes' textures (ExpedicionarioInstaller.Vivid): a bit more colour and depth,
            // so the painted texture does not look washed out under the game's light.
            var map = AssetDatabase.LoadAssetAtPath<Texture2D>(Vivid(png, Folder + "TexturaViva.png"));
            var glowMap = GlowMask(png, Folder + "Brilho.png");
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetTexture("_BaseMap", map); material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", .22f); material.SetFloat("_Metallic", 0);
            material.EnableKeyword("_EMISSION"); material.SetTexture("_EmissionMap", glowMap);
            material.SetColor("_EmissionColor", new Color(.35f, .7f, 1.1f));
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None; material.enableInstancing = true;
            material = Save(material, Folder + "Material.mat");
            var materials = new[] { material };

            var template = baseAxe.attacks[0];
            Color sea = new Color(.25f, .75f, 1f);
            var amplo1 = Attack(template, "GolpeAmplo1", sea, a => { a.shape = AttackShape.Arc; a.footprint = MeleeFootprint.Cone; a.arc = 200; a.range = 2.3f; a.damage = Mathf.RoundToInt(template.damage * 1.35f); a.knockback = 2.5f; a.maximumTargets = 0; });
            // Golpe 3 of the combo: a full spin around the hero that pushes everyone away.
            var giro = Attack(template, "GiroDasMares", sea, a => { a.shape = AttackShape.Arc; a.footprint = MeleeFootprint.Circle; a.arc = 360; a.range = 2.4f; a.damage = Mathf.RoundToInt(template.damage * 1.8f); a.knockback = 4.5f; a.windup = template.windup * 1.2f; a.recovery = template.recovery * 1.25f; a.maximumTargets = 0; });
            // Maré Crescente's wave (MareCrescente moves it forward and hits each enemy once).
            var onda = Attack(template, "OndaAvancante", sea, a => { a.shape = AttackShape.Arc; a.footprint = MeleeFootprint.Circle; a.arc = 360; a.width = 1.8f; a.range = 7.5f; a.damage = Mathf.RoundToInt(template.damage * 2.3f); a.knockback = 5; a.control = ControlEffect.Slow; a.slowMultiplier = .6f; a.controlDuration = 1.5f; a.maximumTargets = 0; });
            // Fúria do Touro Abissal: the bull's hit (it drags the enemies forward); the whirlpool's pulses come from it.
            var touro = Attack(template, "FuriaDoTouroAbissal", sea, a => { a.shape = AttackShape.Arc; a.footprint = MeleeFootprint.Circle; a.arc = 360; a.range = 10f; a.damage = Mathf.RoundToInt(template.damage * 3f); a.knockback = 7; a.windup = .45f; a.recovery = .9f; a.control = ControlEffect.Slow; a.slowMultiplier = .5f; a.controlDuration = 2f; a.maximumTargets = 0; });

            var icon = WriteIcon(mesh, png, Folder + "Icone.png");
            var item = Object.Instantiate(baseAxe);
            item.itemId = Id; item.name = "Arma";
            item.displayName = "Perdição de Netuno";
            item.description = "Machado lendário dos mares antigos: crânio de touro, âncoras e lâminas de água viva. Cada golpe arrasta o oceano junto.";
            item.visualMesh = mesh; item.visualMaterials = materials; item.authoredScale = Vector3.one;
            item.color = sea; item.icon = icon; item.starter = false;
            item.minimumDropRarity = ItemRarity.Legendary;
            item.attacks = new[] { amplo1, amplo1, giro }; // side slash, back the other way, full spin (Fabricio, 26/11)
            item.passiveAttack = onda; item.superAttack = touro;
            item.gripLength = .78f; item.gripHold = .3f;
            item.evolvesInto = null; item.evolveAtForge = 0;
            item = Save(item, Folder + "Arma.asset");
            if (catalog.Find(Id) == null)
            {
                var list = catalog.equipment; Array.Resize(ref list, list.Length + 1); list[list.Length - 1] = item; catalog.equipment = list;
            }
            EditorUtility.SetDirty(catalog);
            catalog.Validate();
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorPrefs.SetInt("Forja.Netuno.Versao", Version);
            string done = "NETUNO_PRONTO · base " + baseAxe.itemId + " · " + mesh.triangles.Length / 3 + " triângulos · dano base " + template.damage + " · tamanho " + mesh.bounds.size;
            Debug.Log(done);
            File.WriteAllText("Logs/netuno.txt", System.DateTime.Now + "\n" + done);
        }

        static AttackDefinition Attack(AttackDefinition template, string name, Color color, Action<AttackDefinition> setup)
        {
            var a = Object.Instantiate(template); a.name = name; a.color = color; a.heartFx = null; a.projectileVisual = null; a.projectileMaterials = null; a.returningProjectile = false;
            setup(a);
            return Save(a, Folder + name + ".asset");
        }

        static string Vivid(string source, string target)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false); texture.LoadImage(File.ReadAllBytes(source));
            var pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color.RGBToHSV(pixels[i], out float h, out float s, out float v);
                s = Mathf.Clamp01(s * 1.3f); v = Mathf.Clamp01(Mathf.Pow(v, 1.1f) * 1.02f);
                var c = Color.HSVToRGB(h, s, v); c.a = pixels[i].a; pixels[i] = c;
            }
            texture.SetPixels(pixels); texture.Apply();
            File.WriteAllBytes(target, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(target) is TextureImporter importer)
            { importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Compressed; importer.sRGBTexture = true; importer.mipmapEnabled = true; importer.SaveAndReimport(); }
            return target;
        }

        // Emission only where the model is glowing blue (blades, eyes, gems), not on the gold, bone or wood.
        static Texture2D GlowMask(string source, string path)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false); tex.LoadImage(File.ReadAllBytes(source));
            int w = tex.width, h = tex.height; var px = tex.GetPixels();
            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i]; float blue = c.b - Mathf.Max(c.r, c.g * .85f);
                float k = Mathf.Clamp01((blue - .08f) / .25f) * Mathf.Clamp01((c.b - .45f) / .3f);
                px[i] = new Color(c.r * k, c.g * k, c.b * k, 1);
            }
            var o = new Texture2D(w, h, TextureFormat.RGBA32, false); o.SetPixels(px); o.Apply();
            File.WriteAllBytes(path, o.EncodeToPNG());
            Object.DestroyImmediate(o); Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(path) is TextureImporter importer) { importer.maxTextureSize = 1024; importer.sRGBTexture = true; importer.textureCompression = TextureImporterCompression.Compressed; importer.SaveAndReimport(); }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // The model in one mesh, centred, thin side on Z, longest side = size (same layout as the other weapons).
        static Mesh BakeMesh(string fbx, float size, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            var go = (GameObject)Object.Instantiate(prefab);
            go.transform.SetPositionAndRotation(Vector3.zero, prefab.transform.rotation);
            var positions = new List<Vector3>(); var normals = new List<Vector3>(); var uvs = new List<Vector2>(); var triangles = new List<int>();
            foreach (var filter in go.GetComponentsInChildren<MeshFilter>())
            {
                var source = filter.sharedMesh; if (source == null) continue;
                var matrix = filter.transform.localToWorldMatrix; bool flip = matrix.determinant < 0; int offset = positions.Count;
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
                    for (int i = 0; i < t.Length; i += 3) { triangles.Add(offset + t[i]); triangles.Add(offset + (flip ? t[i + 2] : t[i + 1])); triangles.Add(offset + (flip ? t[i + 1] : t[i + 2])); }
                }
            }
            Object.DestroyImmediate(go);
            var bounds = new Bounds(positions[0], Vector3.zero); foreach (var p in positions) bounds.Encapsulate(p);
            int thin = bounds.size.x <= bounds.size.y && bounds.size.x <= bounds.size.z ? 0 : bounds.size.y <= bounds.size.z ? 1 : 2;
            Quaternion turn = thin == 2 ? Quaternion.identity : thin == 1 ? Quaternion.Euler(-90, 0, 0) : Quaternion.Euler(0, 90, 0);
            for (int i = 0; i < positions.Count; i++) { positions[i] = turn * (positions[i] - bounds.center); normals[i] = turn * normals[i]; }
            bounds = new Bounds(positions[0], Vector3.zero); foreach (var p in positions) bounds.Encapsulate(p);
            float scale = size / Mathf.Max(bounds.size.x, bounds.size.y, .0001f);
            for (int i = 0; i < positions.Count; i++) positions[i] = (positions[i] - bounds.center) * scale;
            var mesh = new Mesh { name = name, indexFormat = positions.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16 };
            mesh.SetVertices(positions); mesh.SetNormals(normals); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds(); mesh.RecalculateTangents();
            return mesh;
        }

        // Inventory icon: the painted model seen from the front, on a deep-sea backdrop with a cyan glow.
        static Sprite WriteIcon(Mesh mesh, string texturePath, string iconPath)
        {
            const int Big = 512, Size = 256;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false); tex.LoadImage(File.ReadAllBytes(texturePath));
            var color = new Color[Big * Big]; var depth = new float[Big * Big];
            for (int i = 0; i < depth.Length; i++) depth[i] = float.MaxValue;
            var vertices = mesh.vertices; var uv = mesh.uv; var tris = mesh.triangles; var b = mesh.bounds;
            float fit = Big * .86f / Mathf.Max(b.size.x, b.size.y);
            Vector2 Screen(Vector3 p) => new Vector2(Big * .5f + (p.x - b.center.x) * fit, Big * .5f + (p.y - b.center.y) * fit);
            var light = new Vector3(-.45f, .6f, -.66f).normalized;
            for (int t = 0; t < tris.Length; t += 3)
            {
                Vector3 p0 = vertices[tris[t]], p1 = vertices[tris[t + 1]], p2 = vertices[tris[t + 2]];
                Vector2 a = Screen(p0), c = Screen(p1), d = Screen(p2);
                float area = (c.x - a.x) * (d.y - a.y) - (c.y - a.y) * (d.x - a.x);
                if (Mathf.Abs(area) < 1e-6f) continue;
                Vector3 normal = Vector3.Cross(p1 - p0, p2 - p0).normalized; if (normal.z > 0) normal = -normal;
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
                        float z = w0 * p0.z + w1 * p1.z + w2 * p2.z; int index = y * Big + x;
                        if (z >= depth[index]) continue; depth[index] = z;
                        Vector2 st = w0 * uv[tris[t]] + w1 * uv[tris[t + 1]] + w2 * uv[tris[t + 2]];
                        var s = tex.GetPixelBilinear(st.x, st.y);
                        color[index] = new Color(s.r * shade, s.g * shade, s.b * shade, 1);
                    }
            }
            Object.DestroyImmediate(tex);
            var modelPx = new Color[Size * Size]; var alpha = new float[Size * Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Color sum = Color.clear;
                    for (int j = 0; j < 2; j++) for (int i = 0; i < 2; i++) { var s = color[(y * 2 + j) * Big + x * 2 + i]; sum += new Color(s.r * s.a, s.g * s.a, s.b * s.a, s.a); }
                    sum /= 4; modelPx[y * Size + x] = sum.a > 0 ? new Color(sum.r / sum.a, sum.g / sum.a, sum.b / sum.a, sum.a) : Color.clear; alpha[y * Size + x] = sum.a;
                }
            var glow = Blur(alpha, Size, 10); var outline = Blur(alpha, Size, 1);
            var result = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    int i = y * Size + x;
                    float dx = (x - Size * .5f) / (Size * .5f), dy = (y - Size * .5f) / (Size * .5f), r = Mathf.Sqrt(dx * dx + dy * dy);
                    Color pixel = new Color(.04f, .18f, .35f, .55f * Mathf.Clamp01((.95f - r) / .3f));
                    pixel = Over(pixel, new Color(.25f, .8f, 1f, Mathf.Clamp01(glow[i] * 2.2f) * .9f));
                    pixel = Over(pixel, new Color(.02f, .06f, .12f, Mathf.Clamp01(outline[i] * 1.5f) * .9f));
                    pixel = Over(pixel, modelPx[i]);
                    result[i] = pixel;
                }
            var icon = new Texture2D(Size, Size, TextureFormat.RGBA32, false); icon.SetPixels(result); icon.Apply();
            File.WriteAllBytes(iconPath, icon.EncodeToPNG()); Object.DestroyImmediate(icon);
            AssetDatabase.ImportAsset(iconPath, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(iconPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false; importer.maxTextureSize = 256; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        }

        static Color Over(Color under, Color over)
        {
            float a = over.a + under.a * (1 - over.a); if (a <= 0) return Color.clear;
            return new Color((over.r * over.a + under.r * under.a * (1 - over.a)) / a, (over.g * over.a + under.g * under.a * (1 - over.a)) / a, (over.b * over.a + under.b * under.a * (1 - over.a)) / a, a);
        }
        static float[] Blur(float[] a, int size, int radius)
        {
            var h = new float[a.Length]; var o = new float[a.Length];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) { float s = 0; int n = 0; for (int i = -radius; i <= radius; i++) { int xx = x + i; if (xx < 0 || xx >= size) continue; s += a[y * size + xx]; n++; } h[y * size + x] = s / n; }
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) { float s = 0; int n = 0; for (int j = -radius; j <= radius; j++) { int yy = y + j; if (yy < 0 || yy >= size) continue; s += h[yy * size + x]; n++; } o[y * size + x] = s / n; }
            return o;
        }

        static T Save<T>(T asset, string path) where T : Object
        {
            var saved = AssetDatabase.LoadAssetAtPath<T>(path);
            if (saved == null) { AssetDatabase.CreateAsset(asset, path); return asset; }
            if (asset is Mesh source && saved is Mesh target)
            {
                target.Clear(); target.indexFormat = source.indexFormat;
                target.SetVertices(source.vertices); target.SetNormals(source.normals); target.SetUVs(0, source.uv);
                target.SetTriangles(source.triangles, 0); target.RecalculateBounds(); target.RecalculateTangents();
                Object.DestroyImmediate(source); EditorUtility.SetDirty(target); return saved;
            }
            EditorUtility.CopySerialized(asset, saved); Object.DestroyImmediate(asset); EditorUtility.SetDirty(saved); return saved;
        }
    }
}
