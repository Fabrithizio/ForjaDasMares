using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Unity.AI.Navigation;

namespace ForjaDasMares.Networking.Editor
{
    // Targeted scene migration. Never calls the base installers or replaces story/save data.
    public static class TownArtInstaller
    {
        const string Art = "Assets/ForjaDasMares/Art/World/SettlementKit/";
        const string Data = "Assets/ForjaDasMares/Data/World/Town/";
        static readonly string[] Names = {
            "Casa_do_artesao", "Casa_do_herbalista", "Parede_enxaimel", "Porta_de_madeira", "Janela_trabalhada",
            "Arbusto_folhoso", "Arbusto_florido", "Arvore_copa_aberta", "Arvore_luminosa", "Samambaia",
            "Tufo_de_grama", "Flores_violetas", "Cogumelos_do_breu", "Banco_de_madeira", "Cerca_de_madeira",
            "Lanterna_de_rua", "Poco_de_pedra", "Fonte_de_cristal", "Placa_de_caminho", "Floreira_folhosa",
            "Console_antigo", "Marco_antigo", "Carrinho_de_feira", "Banca_de_mercado", "Lenha_empilhada",
            "Cesto_trancado", "Altar_antigo", "Toco_cortado", "Ponte_de_madeira", "Escada_de_pedra",
            "Trepadeira_em_trelica", "Toldo_de_fachada", "Cristais_arcanos", "Estandarte_de_tecido", "Estante_de_potes", "Marco_de_transporte"
        };

        [MenuItem("Forja/World/Integrar kit da cidade (preserva missao)")]
        public static void Apply()
        {
            PrepareAssets();
            UpgradeTown();
            UpgradeFirstTrail();
            PlayerSettings.bundleVersion = "0.27.0";
            AssetDatabase.SaveAssets();
            Debug.Log("SETTLEMENT_READY prefabs=36 scenes=2 storyPreserved=true");
        }

        static void PrepareAssets()
        {
            Directory.CreateDirectory(Art);
            // Only generated kit files are copied, never the user's arbitrary Desktop content.
            string source = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Materiais do jogo/05 Kit original Blender/Modelos"));
            foreach (var name in Names) foreach (var level in new[] { "LOD0", "LOD1" })
            {
                string file = name + "_" + level + ".fbx";
                if (File.Exists(Path.Combine(source, file))) File.Copy(Path.Combine(source, file), Art + file, true);
                else if (!File.Exists(Art + file)) throw new FileNotFoundException("Missing kit model", file);
            }
            if (File.Exists(Path.Combine(source, "Paleta_original.png"))) File.Copy(Path.Combine(source, "Paleta_original.png"), Art + "Paleta_original.png", true);
            else if (!File.Exists(Art + "Paleta_original.png")) throw new FileNotFoundException("Missing settlement palette");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var textureImporter = (TextureImporter)AssetImporter.GetAtPath(Art + "Paleta_original.png");
            textureImporter.mipmapEnabled = false; textureImporter.filterMode = FilterMode.Point;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.wrapMode = TextureWrapMode.Clamp; textureImporter.isReadable = false;
            textureImporter.SaveAndReimport();
            string materialPath = Art + "SettlementPalette.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, materialPath); }
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "Paleta_original.png"));
            material.SetColor("_BaseColor", Color.white); material.SetFloat("_Smoothness", .12f);
            material.enableInstancing = true; material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black); EditorUtility.SetDirty(material);
            foreach (var name in Names)
            {
                var root = new GameObject(name); root.layer = 9;
                var renderers = new Renderer[2];
                for (int i = 0; i < 2; i++)
                {
                    string path = Art + name + "_LOD" + i + ".fbx";
                    var importer = (ModelImporter)AssetImporter.GetAtPath(path);
                    importer.materialImportMode = ModelImporterMaterialImportMode.None;
                    importer.importAnimation = false; importer.importCameras = false; importer.importLights = false;
                    importer.isReadable = false; importer.generateSecondaryUV = true;
                    importer.SaveAndReimport();
                    var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
                    instance.name = "LOD" + i; instance.transform.SetParent(root.transform, false);
                    var meshes = instance.GetComponentsInChildren<MeshRenderer>();
                    if (meshes.Length != 1) throw new InvalidOperationException("Expected one renderer in " + path);
                    renderers[i] = meshes[0]; meshes[0].sharedMaterial = material;
                    foreach (var child in instance.GetComponentsInChildren<Transform>()) child.gameObject.layer = 9;
                }
                var group = root.AddComponent<LODGroup>();
                group.SetLODs(new[] { new LOD(.26f, new[] { renderers[0] }), new LOD(.035f, new[] { renderers[1] }) });
                group.fadeMode = LODFadeMode.None; group.RecalculateBounds();
                Bounds b = ActOneInstaller.RendererBounds(root.transform, renderers[0]);
                if (b.size.y < .05f || b.size.y > 15 || b.size.x > 20) throw new InvalidOperationException("Unexpected imported scale: " + name + " " + b);
                PrefabUtility.SaveAsPrefabAsset(root, Art + name + ".prefab");
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        static GameObject Place(Transform parent, string name, Vector3 position, float scale = 1, float yaw = 0, bool collision = false)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Art + name + ".prefab");
            if (prefab == null) throw new InvalidOperationException("Missing prepared prefab " + name);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(parent, false); instance.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            instance.transform.localScale = Vector3.one * scale;
            if (collision)
            {
                var renderer = instance.GetComponent<LODGroup>().GetLODs()[0].renderers[0];
                Bounds bounds = ActOneInstaller.RendererBounds(instance.transform, renderer);
                var box = instance.AddComponent<BoxCollider>(); box.center = bounds.center; box.size = bounds.size;
                if (name.StartsWith("Arvore")) { box.center = new Vector3(0, 1, 0); box.size = new Vector3(.75f, 2, .75f); }
                if (name.StartsWith("Casa")) { box.center = new Vector3(0, 1.65f, 0); box.size = new Vector3(5.9f, 3.3f, 4.8f); }
            }
            return instance;
        }

        static void RemovePreviousArt(Transform root, string name)
        { var prior = root.Find(name); if (prior != null) UnityEngine.Object.DestroyImmediate(prior.gameObject); }

        static void UpgradeTown()
        {
            var scene = EditorSceneManager.OpenScene(TownInstaller.ScenePath);
            var root = GameObject.Find("TownEnvironment").transform;
            RemovePreviousArt(root, "TownArt027");
            string[] blockouts = { "Oficina do Daro", "Arquivo da Nara", "Estalagem do Sino", "Door", "Roof", "Window", "BenchSeat", "BenchLeg", "WorkshopBench", "MarketCounter", "MarketCanopy", "Planter", "GeometricShrub", "SupplyCrate" };
            foreach (Transform child in root.Cast<Transform>().ToArray())
                if (blockouts.Contains(child.name) && child.GetComponent<TextMesh>() == null) UnityEngine.Object.DestroyImmediate(child.gameObject);
            foreach (var wall in root.GetComponentsInChildren<MeshRenderer>().Where(r => r.name == "BoundaryWall")) wall.enabled = false;
            var art = new GameObject("TownArt027").transform; art.SetParent(root, false);
            // Imported FBX façades face +Z; turn them toward the central approach (-Z).
            Place(art, "Casa_do_artesao", new Vector3(24, 0, 11), 1.35f, 180, true);
            Place(art, "Casa_do_herbalista", new Vector3(-24, 0, 11), 1.35f, 180, true);
            Place(art, "Casa_do_herbalista", new Vector3(0, 0, 27), 1.55f, 180, true);
            Place(art, "Toldo_de_fachada", new Vector3(24, 0, 7), 1.4f);
            Place(art, "Estante_de_potes", new Vector3(19, 0, 8), 1, 25, true);
            Place(art, "Lenha_empilhada", new Vector3(29, 0, 8), 1.2f, 90, true);
            Place(art, "Banca_de_mercado", new Vector3(-19, 0, -18), 1.4f, 0, true);
            Place(art, "Carrinho_de_feira", new Vector3(-24, 0, -16), 1, 35, true);
            Place(art, "Cesto_trancado", new Vector3(-17, 0, -18), 1.4f, 0, true);
            for (int i = 0; i < 4; i++) Place(art, "Banco_de_madeira", new Vector3(-18 + i * 12, 0, 21), 1.2f, 0, true);
            foreach (int side in new[] { -1, 1 })
            {
                for (int i = 0; i < 3; i++) Place(art, "Floreira_folhosa", new Vector3(side * 29, 0, -2 + i * 5), 1.5f, i * 45, true);
                for (int i = 0; i < 5; i++)
                {
                    Vector3 p = new Vector3(side * (32 + i % 2), 0, -26 + i * 12);
                    Place(art, i % 2 == 0 ? "Arvore_copa_aberta" : "Arvore_luminosa", p, 1.25f, i * 70, true);
                    Place(art, "Arbusto_florido", p + Vector3.forward * 3, 1.5f, i * 57);
                }
                for (int i = 0; i < 4; i++)
                    Place(art, "Lanterna_de_rua", new Vector3(side * 8, 0, -24 + i * 13), 1.3f, side < 0 ? 0 : 180, true);
            }
            for (int i = 0; i < 14; i++)
            {
                float x = -31 + i * 4.8f;
                Place(art, "Arbusto_folhoso", new Vector3(x, 0, 33), 1.25f, i * 43);
                if (Mathf.Abs(x) > 5) Place(art, "Cerca_de_madeira", new Vector3(x, 0, -34), 1.7f, 0, true);
            }
            for (int i = 0; i < 12; i++)
                Place(art, i % 3 == 0 ? "Samambaia" : "Flores_violetas", new Vector3(-15 + i % 4 * 2, 0, 16 + i / 4 * 1.7f), 1.2f, i * 31);
            var fountain = GameObject.Find("RestFountain");
            foreach (var renderer in fountain.GetComponents<MeshRenderer>()) UnityEngine.Object.DestroyImmediate(renderer);
            foreach (var filter in fountain.GetComponents<MeshFilter>()) UnityEngine.Object.DestroyImmediate(filter);
            foreach (var collider in fountain.GetComponents<Collider>()) UnityEngine.Object.DestroyImmediate(collider);
            RemovePreviousArt(fountain.transform, "Fonte_de_cristal");
            fountain.transform.SetPositionAndRotation(new Vector3(-11, 0, 10), Quaternion.identity); fountain.transform.localScale = Vector3.one;
            var bowl = Place(fountain.transform, "Fonte_de_cristal", fountain.transform.position);
            var solid = fountain.AddComponent<BoxCollider>(); solid.center = Vector3.up * .3f; solid.size = new Vector3(2.4f, .6f, 2.4f);
            var feedback = fountain.GetComponent<WorldInteractionFeedback>() ?? fountain.AddComponent<WorldInteractionFeedback>();
            feedback.Configure(fountain.GetComponent<WorldClickTarget>(), bowl.transform, bowl.GetComponentsInChildren<Renderer>());
            Place(art, "Poco_de_pedra", new Vector3(-19, 0, 26), 1.2f, 20, true);
            Place(art, "Placa_de_caminho", new Vector3(5, 0, -25), 1.2f, 180, true);
            // Real decorative lamps, but no per-lamp lights/scripts: mobile-friendly shared material.
            UpdateNavigation(Data + "Navigation.asset", true);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("TOWN_ART_UPGRADED instances=" + art.GetComponentsInChildren<LODGroup>().Length);
        }

        static void UpgradeFirstTrail()
        {
            var scene = EditorSceneManager.OpenScene(ActOneInstaller.ScenePath);
            var parent = GameObject.Find("ActOneEnvironment").transform;
            RemovePreviousArt(parent, "FirstTrailArt027");
            var art = new GameObject("FirstTrailArt027").transform; art.SetParent(parent, false);
            // Dress two existing exits, not a corridor or new room. Keep combat/spawn positions intact.
            var sites = new[] { new Vector3(-22, 0, -16), new Vector3(-34, 0, -25), new Vector3(-47, 0, -24), new Vector3(25, 0, 10), new Vector3(35, 0, 22), new Vector3(42, 0, 15) };
            for (int i = 0; i < sites.Length; i++)
            {
                Vector3 p = Ground(sites[i]);
                Place(art, "Arvore_copa_aberta", p, 1.2f, i * 43, true);
                Place(art, "Arbusto_folhoso", Ground(p + Vector3.right * 2), 1.6f, i * 57);
                Place(art, "Samambaia", Ground(p + Vector3.back * 2), 1.2f, i * 23);
            }
            Place(art, "Placa_de_caminho", Ground(new Vector3(-15, 0, -13)), 1.2f, 70, true);
            Place(art, "Marco_antigo", Ground(new Vector3(24, 0, 4)), 1.3f, -20, true);
            UpdateNavigation("Assets/ForjaDasMares/Data/World/Act1/Navigation.asset", false);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("FIRST_TRAIL_ART_UPGRADED loopsPreserved=true");
        }
        static Vector3 Ground(Vector3 p)
        {
            foreach (var terrain in Terrain.activeTerrains)
            {
                Vector3 start = terrain.transform.position; Vector3 size = terrain.terrainData.size;
                if (p.x >= start.x && p.x <= start.x + size.x && p.z >= start.z && p.z <= start.z + size.z)
                { p.y = terrain.SampleHeight(p) + start.y; break; }
            }
            return p;
        }
        public static void FixFacadeOrientation()
        {
            var scene = EditorSceneManager.OpenScene(TownInstaller.ScenePath);
            var art = GameObject.Find("TownArt027");
            if (art == null) throw new InvalidOperationException("Town art must be installed first");
            var houses = art.GetComponentsInChildren<LODGroup>().Where(g => g.name.StartsWith("Casa_")).ToArray();
            if (houses.Length != 3) throw new InvalidOperationException("Expected three existing houses");
            foreach (var house in houses) house.transform.rotation = Quaternion.Euler(0, 180, 0);
            // Box collision is centred in X/Z and invariant under this 180-degree turn.
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("TOWN_FACADES_FIXED count=3");
        }
        static void UpdateNavigation(string path, bool town)
        {
            Physics.SyncTransforms();
            var surface = UnityEngine.Object.FindFirstObjectByType<NavMeshSurface>();
            surface.BuildNavMesh();
            var prior = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
            if (prior == null) throw new InvalidOperationException("Navigation asset missing: " + path);
            var world = UnityEngine.Object.FindFirstObjectByType<RegionalWorld>();
            if (!NavMesh.SamplePosition(world.Definition.refuge, out var start, 2, NavMesh.AllAreas)) throw new InvalidOperationException("Refuge not walkable");
            var points = town ? UnityEngine.Object.FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None).Select(t => t.Approach)
                : world.Definition.route.Concat(world.Definition.camps.SelectMany(c => c.spawns)).Concat(world.Journey != null ? world.Journey.Definition.points.Select(p => p.position) : world.GetComponent<RegionalJourney>().Definition.points.Select(p => p.position));
            foreach (var p in points)
            {
                var route = new NavMeshPath();
                if (!NavMesh.SamplePosition(p, out var end, 2.5f, NavMesh.AllAreas) || !NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, route) || route.status != NavMeshPathStatus.PathComplete)
                    throw new InvalidOperationException("Art obstructed destination: " + p);
            }
            // Never overwrite the stable navigation asset until connectivity has passed.
            EditorUtility.CopySerialized(surface.navMeshData, prior); surface.RemoveData(); surface.navMeshData = prior; surface.AddData();
        }
    }
}
