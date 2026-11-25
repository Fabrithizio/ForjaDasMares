using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ForjaDasMares.Networking.Editor
{
    // Prepares the effects of the abilities: a soft additive glow material (kept in Resources, so the
    // shader goes into the PC and mobile builds) and the list of Cartoon FX prefabs used by each ability.
    // Runs by itself when something is missing; menu: Forja/Efeitos/Preparar efeitos das habilidades.
    [InitializeOnLoad]
    public static class EfeitosInstaller
    {
        const string Folder = "Assets/ForjaDasMares/Resources/Efeitos";
        const string Pack = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/";
        const int Version = 2;

        // ability key -> pack prefab (path inside CFXR Prefabs)
        static readonly (string key, string path)[] Map =
        {
            ("raizes", "Nature/CFXR3 Hit Leaves A (Lit)"),
            ("marca_perto", "Electric/CFXR3 Hit Electric C (Air)"),
            ("marca_longe", "Electric/CFXR3 Hit Electric C (Air)"),
            ("marca_aura", "Electric/CFXR Electrified 3"),
            ("marca_descarga", "Electric/CFXR2 Sparks Rain"),
            ("brasa_perto", "Sword Trails/Fire/CFXR4 Sword Hit FIRE (Cross)"),
            ("brasa_longe", "Fire/CFXR3 Hit Fire B (Air)"),
            ("brasa_aura", "Fire/CFXR Fire"),
            ("rajada", "Nature/CFXR4 Wind Trails"),
            ("bolha", "Liquids/CFXR Water Splash (Smaller)"),
            ("bolha_aura", "Magic Misc/CFXR4 Bouncing Glows Bubble (Blue Purple)"),
            ("brotar", "Nature/CFXR3 Shield Leaves A (Lit)"),
            ("brotar_aura", "Misc/CFXR2 Shiny Item (Loop)"),
            ("gelo", "Ice/CFXR3 Hit Ice B (Air)"),
            ("super", "Explosions/CFXR Explosion 1"),
            // Blade attacks (CortesDaEspada and the hit on the enemy).
            ("corte_cruz", "Sword Trails/Plain/CFXR4 Sword Hit PLAIN (Cross)"),
            ("corte_giro", "Sword Trails/Plain/CFXR4 Sword Trail PLAIN (360 Spiral)"),
            ("corte_giro_fino", "Sword Trails/Plain/CFXR4 Sword Trail PLAIN (360 Thin Spiral)"),
            ("acerto_brilho", "Light/CFXR3 Hit Light B (Air)"),
        };

        static EfeitosInstaller() { EditorApplication.delayCall += () => { if (Needed()) Install(); }; }
        static bool Needed() =>
            !File.Exists(Folder + "/BrilhoAditivo.mat") || !File.Exists(Folder + "/CatalogoDeEfeitos.asset") ||
            EditorPrefs.GetInt("Forja.Efeitos.Versao", 0) < Version;

        [MenuItem("Forja/Efeitos/Preparar efeitos das habilidades")]
        public static void Install()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Install; return; }
            if (!AssetDatabase.IsValidFolder("Assets/ForjaDasMares/Resources")) AssetDatabase.CreateFolder("Assets/ForjaDasMares", "Resources");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/ForjaDasMares/Resources", "Efeitos");
            var texture = SoftDot(Folder + "/PontoSuave.png");
            Glow(Folder + "/BrilhoAditivo.mat", texture);
            int found = Catalog(Folder + "/CatalogoDeEfeitos.asset");
            EditorPrefs.SetInt("Forja.Efeitos.Versao", Version);
            AssetDatabase.SaveAssets();
            Debug.Log("EFEITOS_PRONTOS · brilho ok · efeitos do pacote encontrados: " + found + " / " + Map.Length);
        }

        // A round soft dot (white, fading to the edge) for the particles.
        static Texture2D SoftDot(string path)
        {
            if (!File.Exists(path))
            {
                const int size = 128;
                var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float d = new Vector2(x - size / 2f + .5f, y - size / 2f + .5f).magnitude / (size / 2f);
                        float a = Mathf.Clamp01(1 - d); a = a * a * (3 - 2 * a);
                        t.SetPixel(x, y, new Color(1, 1, 1, a));
                    }
                File.WriteAllBytes(path, t.EncodeToPNG());
                Object.DestroyImmediate(t);
                AssetDatabase.ImportAsset(path);
                if (AssetImporter.GetAtPath(path) is TextureImporter importer) { importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.wrapMode = TextureWrapMode.Clamp; importer.SaveAndReimport(); }
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // URP particle material, transparent and additive (bright on any background).
        static void Glow(string path, Texture2D texture)
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
            m.shader = shader;
            m.SetTexture("_BaseMap", texture);
            m.SetColor("_BaseColor", Color.white);
            m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 2);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            m.SetFloat("_ZWrite", 0);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            EditorUtility.SetDirty(m);
        }

        static int Catalog(string path)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<CatalogoDeEfeitos>(path);
            if (catalog == null) { catalog = ScriptableObject.CreateInstance<CatalogoDeEfeitos>(); AssetDatabase.CreateAsset(catalog, path); }
            var items = new List<CatalogoDeEfeitos.Item>(); int found = 0;
            foreach (var (key, file) in Map)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Pack + file + ".prefab");
                if (prefab != null) found++; else Debug.LogWarning("Efeito não encontrado: " + file);
                items.Add(new CatalogoDeEfeitos.Item { chave = key, prefab = prefab });
            }
            catalog.itens = items.ToArray();
            EditorUtility.SetDirty(catalog);
            return found;
        }
    }
}
