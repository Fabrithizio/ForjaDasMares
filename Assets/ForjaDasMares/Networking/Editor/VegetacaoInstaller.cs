using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ForjaDasMares.Networking.Editor
{
    // Vegetation from the Asset Store packs (Low Poly Wind, A piece of nature, Patchmesh flowers), turned into one
    // cartoon kit for the city: brighter colours, soft shading, our URP materials, the right size and simple colliders.
    // Kit prefabs: Art/Vegetacao/Arvores, Gramas, Flores. The city uses them in place of the old ball trees and bushes.
    public static class VegetacaoInstaller
    {
        // The packs as Unity downloaded them from the Asset Store (My Assets), imported straight from that folder.
        static readonly string[] Packages =
        {
            "Nicrom/Shaders/Low Poly Wind.unitypackage",
            "Evgenia/3D ModelsEnvironmentsFantasy/Lowpoly Piece of Nature.unitypackage",
            "Patchmesh/3D ModelsVegetationFlowers/Free Sample Stylized Hand-Painted Plant Flower Pack.unitypackage",
        };
        static string Cache => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "Unity", "Asset Store-5.x");
        static int next;
        const string Kit = "Assets/ForjaDasMares/Art/Vegetacao/";
        const string Wind = "Assets/Nicrom/Shaders/Wind/";
        const string Nature = "Assets/A_piece_of_nature/";
        const string Flowers = "Assets/Patchmesh/Free Sample Stylized Hand-Painted Plant & Flower Pack/Prefabs/";

        // source prefab, height in the game (m)
        static readonly (string path, float height)[] Trees =
        {
            (Wind + "Prefabs/LPW_Tree_A1_6.5m_01.prefab", 6.2f),
            (Wind + "Prefabs/LPW_Tree_B1_9m_01.prefab", 6.8f),
            (Wind + "Prefabs/LPW_Tree_B1_9m_02.prefab", 6.4f),
            (Wind + "Prefabs/LPW_Tree_C1_9m_01.prefab", 7.2f),
            (Wind + "Prefabs/LPW_Tree_C1_11m_01.prefab", 7.8f),
            (Nature + "Prefabs/Pine_tree.prefab", 6.6f),
        };
        static readonly (string path, float height)[] Grass =
        {
            (Wind + "Prefabs/LPW_Grass_A1_40cm_01.prefab", .55f),
            (Wind + "Prefabs/LPW_Grass_A1_50cm_01.prefab", .65f),
            (Wind + "Prefabs/LPW_Grass_B1_40cm_01.prefab", .5f),
            (Wind + "Prefabs/LPW_Flower_A1_H70cm_01.prefab", .8f),
        };
        static readonly (string path, float height)[] Blooms =
        {
            (Flowers + "Daisy 1 Pink.prefab", .7f),
            (Flowers + "Daisy 1 White.prefab", .7f),
            (Flowers + "Pansy Purple & Yellow 1.prefab", .55f),
            (Flowers + "Red Roses 2.prefab", .8f),
            (Flowers + "Sunflower 2.prefab", 1.1f),
        };

        [MenuItem("Forja/Cidade nova/0. Importar vegetação dos pacotes")]
        public static void ImportPackages()
        {
            next = 0;
            AssetDatabase.importPackageCompleted -= Imported; AssetDatabase.importPackageCompleted += Imported;
            AssetDatabase.importPackageFailed -= Failed; AssetDatabase.importPackageFailed += Failed;
            ImportNext();
        }

        static void ImportNext()
        {
            while (next < Packages.Length)
            {
                string path = Path.Combine(Cache, Packages[next++]);
                if (File.Exists(path)) { AssetDatabase.ImportPackage(path, false); return; }
                Debug.LogWarning("Vegetação: pacote não baixado: " + path);
            }
            AssetDatabase.importPackageCompleted -= Imported; AssetDatabase.importPackageFailed -= Failed;
            Prepare();
            Debug.Log("VEGETACAO_IMPORTADA · agora rode Forja/Cidade nova/4. Atualizar a cidade do jogo e jogar");
        }

        static void Imported(string name) => EditorApplication.delayCall += ImportNext;
        static void Failed(string name, string error) { Debug.LogError("Vegetação: falhou " + name + ": " + error); EditorApplication.delayCall += ImportNext; }

        public static bool Ready => Load("Arvores").Length > 0;

        public static GameObject[] Load(string group)
        {
            string folder = Kit + group;
            if (!AssetDatabase.IsValidFolder(folder)) return new GameObject[0];
            return AssetDatabase.FindAssets("t:Prefab", new[] { folder }).Select(g => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g))).Where(p => p != null).OrderBy(p => p.name).ToArray();
        }

        [MenuItem("Forja/Cidade nova/0b. Refazer kit de vegetação")]
        public static void Prepare()
        {
            if (!File.Exists(Trees[0].path) && !File.Exists(Blooms[0].path)) return;
            foreach (var f in new[] { "Arvores", "Gramas", "Flores", "Materiais", "Texturas" }) Folder(Kit + f);
            // Soft shading on the faceted wind-pack models (rounder, more cartoon).
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { Wind + "Assets/Models" }))
                if (AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) is ModelImporter model && model.importNormals != ModelImporterNormals.Calculate)
                { model.importNormals = ModelImporterNormals.Calculate; model.normalSmoothingAngle = 180; model.SaveAndReimport(); }
            var converted = new Dictionary<Material, Material>();
            int count = 0;
            for (int i = 0; i < Trees.Length; i++) count += Build(Trees[i].path, Kit + "Arvores/Arvore_" + i + ".prefab", Trees[i].height, true, converted);
            for (int i = 0; i < Grass.Length; i++) count += Build(Grass[i].path, Kit + "Gramas/Grama_" + i + ".prefab", Grass[i].height, false, converted);
            for (int i = 0; i < Blooms.Length; i++) count += Build(Blooms[i].path, Kit + "Flores/Flor_" + i + ".prefab", Blooms[i].height, false, converted);
            AssetDatabase.SaveAssets();
            Debug.Log("VEGETACAO_KIT " + count + " peças");
        }

        static int Build(string source, string target, float height, bool tree, Dictionary<Material, Material> converted)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(source);
            if (prefab == null) { Debug.LogWarning("Vegetação: não achei " + source); return 0; }
            var root = new GameObject(Path.GetFileNameWithoutExtension(target));
            try
            {
                var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
                PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                foreach (var c in model.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
                foreach (var m in model.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(m);
                foreach (var lod in model.GetComponentsInChildren<LODGroup>(true)) Object.DestroyImmediate(lod);
                foreach (var r in model.GetComponentsInChildren<Renderer>(true))
                {
                    var mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) if (mats[i] != null) mats[i] = Cartoon(mats[i], converted);
                    r.sharedMaterials = mats;
                    r.shadowCastingMode = tree ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                var renderers = model.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0) return 0;
                var b = renderers[0].bounds; foreach (var r in renderers) b.Encapsulate(r.bounds);
                if (b.size.y > .001f) model.transform.localScale *= height / b.size.y;
                b = renderers[0].bounds; foreach (var r in renderers) b.Encapsulate(r.bounds);
                model.transform.position += new Vector3(-b.center.x, -b.min.y, -b.center.z);
                if (tree)
                {
                    // Only the trunk blocks the way.
                    var trunk = root.AddComponent<CapsuleCollider>();
                    trunk.radius = .35f; trunk.height = 2.4f; trunk.center = Vector3.up * 1.2f;
                }
                foreach (var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.isStatic = true;
                PrefabUtility.SaveAsPrefabAsset(root, target);
                return 1;
            }
            finally { Object.DestroyImmediate(root); }
        }

        // Our URP material: the pack's own texture, brighter and more saturated, matte, leaves cut out by alpha.
        static Material Cartoon(Material source, Dictionary<Material, Material> converted)
        {
            if (converted.TryGetValue(source, out var done)) return done;
            var data = new SerializedObject(source);
            Texture texture = null; Color color = Color.white; float cutoff = -1;
            foreach (SerializedProperty entry in data.FindProperty("m_SavedProperties.m_TexEnvs"))
            {
                string name = entry.FindPropertyRelative("first").stringValue;
                var value = entry.FindPropertyRelative("second.m_Texture").objectReferenceValue as Texture;
                if (value != null && (name == "_BaseMap" || name == "_MainTex" && texture == null)) texture = value;
            }
            foreach (SerializedProperty entry in data.FindProperty("m_SavedProperties.m_Colors"))
            {
                string name = entry.FindPropertyRelative("first").stringValue;
                if (name == "_BaseColor" || name == "_Color") color = entry.FindPropertyRelative("second").colorValue;
            }
            foreach (SerializedProperty entry in data.FindProperty("m_SavedProperties.m_Floats"))
                if (entry.FindPropertyRelative("first").stringValue == "_Cutoff") cutoff = entry.FindPropertyRelative("second").floatValue;
            string shaderName = source.shader != null ? source.shader.name : "";
            bool leaves = shaderName.Contains("Transparent") || source.name.Contains("Grass") || source.name.Contains("Petal") || source.name.Contains("Leaf") || source.name.Contains("Daisy") || source.name.Contains("Pansy") || source.name.Contains("Sunflower") || source.name.Contains("Pothos") || source.name.Contains("Rose");

            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = source.name };
            if (texture != null) m.SetTexture("_BaseMap", Vivid(texture));
            Color.RGBToHSV(color, out float h, out float s, out float v);
            var tint = Color.HSVToRGB(h, Mathf.Clamp01(s * 1.25f), Mathf.Clamp01(v * 1.08f)); tint.a = 1;
            m.SetColor("_BaseColor", tint);
            m.SetFloat("_Smoothness", .05f); m.SetFloat("_Metallic", 0);
            m.SetFloat("_EnvironmentReflections", 0); m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            m.SetFloat("_SpecularHighlights", 0); m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            if (leaves && texture != null)
            {
                m.SetFloat("_AlphaClip", 1); m.SetFloat("_Cutoff", cutoff > 0 ? cutoff : .45f); m.EnableKeyword("_ALPHATEST_ON");
                m.SetFloat("_Cull", 0); m.doubleSidedGI = true;
            }
            m.enableInstancing = true;
            string path = Kit + "Materiais/" + Safe(source.name) + ".mat";
            var saved = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (saved == null) AssetDatabase.CreateAsset(m, path); else { EditorUtility.CopySerialized(m, saved); Object.DestroyImmediate(m); m = saved; }
            converted[source] = m;
            return m;
        }

        static readonly Dictionary<Texture, Texture2D> vivid = new Dictionary<Texture, Texture2D>();
        static Texture2D Vivid(Texture source)
        {
            if (vivid.TryGetValue(source, out var done) && done != null) return done;
            string path = Kit + "Texturas/" + Safe(source.name) + "_viva.png";
            int w = Mathf.Min(source.width, 1024), hgt = Mathf.Min(source.height, 1024);
            var rt = RenderTexture.GetTemporary(w, hgt, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            Graphics.Blit(source, rt);
            var previous = RenderTexture.active; RenderTexture.active = rt;
            var copy = new Texture2D(w, hgt, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, w, hgt), 0, 0); copy.Apply();
            RenderTexture.active = previous; RenderTexture.ReleaseTemporary(rt);
            var pixels = copy.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color.RGBToHSV(pixels[i], out float h, out float s, out float v);
                var c = Color.HSVToRGB(h, Mathf.Clamp01(s * 1.3f), Mathf.Clamp01(Mathf.Pow(v, .85f) * 1.05f)); c.a = pixels[i].a;
                pixels[i] = c;
            }
            copy.SetPixels(pixels); copy.Apply();
            File.WriteAllBytes(path, copy.EncodeToPNG());
            Object.DestroyImmediate(copy);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                importer.alphaIsTransparency = true; importer.mipmapEnabled = true; importer.maxTextureSize = 1024;
                importer.textureCompression = TextureImporterCompression.Compressed; importer.SaveAndReimport();
            }
            var result = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            vivid[source] = result;
            return result;
        }

        static string Safe(string name) { foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_'); return name.Replace('&', 'e'); }

        static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            Folder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
