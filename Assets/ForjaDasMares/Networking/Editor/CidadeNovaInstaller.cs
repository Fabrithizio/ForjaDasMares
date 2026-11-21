using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace ForjaDasMares.Networking.Editor
{
    // New first city built from the Meshy models in Art/Cidade.
    // Step 1 prepares every model (import settings, material, prefab with the base on the ground and real size).
    // Step 2 builds a SEPARATE preview scene (CidadeNova) laid out like the approved map. The game scene is not touched.
    public static class CidadeNovaInstaller
    {
        const string Art = "Assets/ForjaDasMares/Art/Cidade/";
        public const string PreviewScene = "Assets/ForjaDasMares/Scenes/CidadeNova.unity";

        // Height in meters of each model in the game.
        static readonly Dictionary<string, float> Heights = new Dictionary<string, float>
        {
            { "Monumento", 7f }, { "PortaoSul", 9f }, { "Muralha", 3f }, { "TorreMuralha", 6.5f },
            { "CasaA", 7f }, { "CasaB", 5f }, { "CasaC", 7.5f }, { "ForjaTeo", 6f }, { "SalaoInstancias", 9f },
            { "Estalagem", 8f }, { "Carroca", 3f }, { "EstacaoAntiga", 8f }, { "BarracaFrutas", 3.2f },
            { "BarracaTecidos", 3.2f }, { "FonteCura", 2.4f }, { "Poco", 2.6f }, { "MesaMapas", 1.3f },
            { "Ponte", 5f }, { "RuinaGigante", 45f }, { "Tenda", 4.5f },
            { "Banco", 1.1f }, { "Caixotes", 1.7f }, { "MesaBancos", 1.15f }, { "Poste", 3.6f }, { "QuadroAvisos", 2.6f }, { "Rochas", 1.5f },
            { "NPCTeo", 1.85f }, { "NPCOficial", 1.75f }, { "NPCIria", 1.7f }, { "NPCMercador", 1.75f },
        };
        static readonly HashSet<string> Big = new HashSet<string> { "Monumento", "PortaoSul", "ForjaTeo", "SalaoInstancias", "Estalagem", "EstacaoAntiga", "CasaA" };
        static readonly HashSet<string> NoCollision = new HashSet<string> { "RuinaGigante", "NPCTeo", "NPCOficial", "NPCIria", "NPCMercador" };

        // ---------- Step 1 ----------

        [MenuItem("Forja/Cidade nova/1. Preparar modelos")]
        public static void Prepare()
        {
            // Vegetation kit from the Asset Store packs (made once; menu "0b" remakes it).
            if (!VegetacaoInstaller.Ready) VegetacaoInstaller.Prepare();
            int count = 0;
            foreach (var slug in Heights.Keys)
            {
                string folder = Art + slug + "/";
                string model = File.Exists(folder + slug + ".fbx") ? folder + slug + ".fbx" : folder + slug + ".obj";
                string texturePath = folder + slug + ".png";
                if (!File.Exists(model) || !File.Exists(texturePath)) { Debug.LogWarning("Faltando: " + slug); continue; }

                if (AssetImporter.GetAtPath(texturePath) is TextureImporter texture)
                {
                    texture.maxTextureSize = Big.Contains(slug) ? 2048 : 1024;
                    texture.textureCompression = TextureImporterCompression.Compressed;
                    texture.sRGBTexture = true; texture.mipmapEnabled = true;
                    texture.SaveAndReimport();
                }
                if (AssetImporter.GetAtPath(model) is ModelImporter importer)
                {
                    importer.materialImportMode = ModelImporterMaterialImportMode.None;
                    importer.importAnimation = false; importer.importCameras = false; importer.importLights = false;
                    // Characters keep their skeleton so their bones can move; scenery has none.
                    importer.animationType = slug.StartsWith("NPC") ? ModelImporterAnimationType.Generic : ModelImporterAnimationType.None;
                    importer.meshCompression = ModelImporterMeshCompression.Medium;
                    importer.isReadable = false;
                    importer.SaveAndReimport();
                }

                var material = AssetDatabase.LoadAssetAtPath<Material>(folder + slug + ".mat");
                if (material == null)
                {
                    material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    AssetDatabase.CreateAsset(material, folder + slug + ".mat");
                }
                material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
                material.SetColor("_BaseColor", Color.white);
                material.SetFloat("_Smoothness", .12f);
                material.SetFloat("_Metallic", 0f);
                material.enableInstancing = true;
                EditorUtility.SetDirty(material);

                var source = AssetDatabase.LoadAssetAtPath<GameObject>(model);
                var root = new GameObject(slug);
                // "Ajuste" carries our size/position so the model keeps the scale and rotation it was imported with.
                var body = new GameObject("Ajuste");
                body.transform.SetParent(root.transform, false);
                var model3d = (GameObject)PrefabUtility.InstantiatePrefab(source);
                model3d.name = "Modelo";
                model3d.transform.SetParent(body.transform, false);
                foreach (var r in body.GetComponentsInChildren<Renderer>())
                {
                    r.sharedMaterials = Enumerable.Repeat(material, Mathf.Max(1, r.sharedMaterials.Length)).ToArray();
                    r.shadowCastingMode = ShadowCastingMode.On;
                }
                // The wall must run along X so it can be chained.
                var b = Bounds(body);
                if (slug == "Muralha" && b.size.z > b.size.x) { body.transform.localRotation = Quaternion.Euler(0, 90, 0); b = Bounds(body); }
                float scale = Heights[slug] / Mathf.Max(.01f, b.size.y);
                body.transform.localScale = Vector3.one * scale;
                b = Bounds(body);
                body.transform.position -= new Vector3(b.center.x, b.min.y, b.center.z);
                b = Bounds(body);
                if (slug == "PortaoSul")
                {
                    // Two side blocks so the player can walk through the arch.
                    bool alongX = b.size.x >= b.size.z;
                    float wide = alongX ? b.size.x : b.size.z;
                    foreach (float side in new[] { -1f, 1f })
                    {
                        var block = root.AddComponent<BoxCollider>();
                        var offset = (alongX ? Vector3.right : Vector3.forward) * side * wide * .36f;
                        block.center = b.center + offset;
                        block.size = alongX ? new Vector3(wide * .28f, b.size.y, b.size.z) : new Vector3(b.size.x, b.size.y, wide * .28f);
                    }
                }
                else if (!NoCollision.Contains(slug))
                {
                    var box = root.AddComponent<BoxCollider>();
                    box.center = b.center;
                    box.size = new Vector3(b.size.x * .9f, b.size.y, b.size.z * .9f);
                }
                foreach (var t in root.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = true;
                PrefabUtility.SaveAsPrefabAsset(root, folder + slug + ".prefab");
                Object.DestroyImmediate(root);
                count++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("CIDADE_NOVA_MODELOS_PRONTOS " + count);
        }

        static Bounds Bounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        static GameObject Prefab(string slug) => AssetDatabase.LoadAssetAtPath<GameObject>(Art + slug + "/" + slug + ".prefab");

        // ---------- Step 2 ----------

        [MenuItem("Forja/Cidade nova/2. Montar prévia da cidade")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Prepare();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Lighting();
            BuildCity();

            var camera = new GameObject("CameraPrevia").AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 48, -66);
            camera.transform.LookAt(new Vector3(0, 0, -5));
            camera.fieldOfView = 45; camera.nearClipPlane = 1f; camera.farClipPlane = 600;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            camera.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            camera.gameObject.tag = "MainCamera";

            // Test walker: WASD to walk, Shift to run, mouse wheel to zoom.
            var walker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            walker.name = "Passeio";
            Object.DestroyImmediate(walker.GetComponent<Collider>());
            walker.transform.position = new Vector3(0, 1.1f, -31);
            walker.transform.localScale = new Vector3(.8f, .9f, .8f);
            walker.GetComponent<Renderer>().sharedMaterial = Flat("Passeio", new Color(.55f, .35f, .85f));
            var controller = walker.AddComponent<CharacterController>();
            controller.height = 2f; controller.radius = .5f; controller.center = Vector3.zero;
            walker.AddComponent<CidadePasseio>().view = camera;

            EditorSceneManager.SaveScene(scene, PreviewScene);
            SceneView.lastActiveSceneView?.LookAt(Vector3.zero, Quaternion.Euler(40, 0, 0), 70);
            Debug.Log("CIDADE_NOVA_PREVIA_PRONTA");
        }

        // The whole city (buildings, walls, nature, details, ground, river). Used by the preview and by the game scene.
        static Transform BuildCity()
        {
            var world = new GameObject("CidadeNova").transform;

            placed.Clear();
            const float R = 36; // wall radius: compact, so the city feels full
            Place(world, "Monumento", new Vector3(0, 0, 0), 180);
            Place(world, "FonteCura", new Vector3(9, 0, -7), 0, true);

            // North: instance hall, ancient station beside it, giant ruin far away.
            Place(world, "SalaoInstancias", new Vector3(0, 0, 26), 0, true);
            Place(world, "EstacaoAntiga", new Vector3(-17, 0, 28), 0, true);
            Place(world, "CasaC", new Vector3(16, 0, 28), 0, true);
            Place(world, "RuinaGigante", new Vector3(10, 0, 150), 180);

            // East: market and Teo's forge.
            Place(world, "ForjaTeo", new Vector3(24, 0, 7), 0, true);
            Place(world, "BarracaFrutas", new Vector3(14, 0, 13), 0, true);
            Place(world, "BarracaTecidos", new Vector3(15, 0, -11), 0, true);
            Place(world, "CasaA", new Vector3(28, 0, -8), 0, true);
            Place(world, "CasaB", new Vector3(26, 0, 19), 0, true);
            Place(world, "CasaC", new Vector3(25, 0, -19), 0, true);

            // West: neighbourhood and the inn.
            Place(world, "Estalagem", new Vector3(-25, 0, 6), 0, true);
            Place(world, "Poco", new Vector3(-15, 0, -9), 0);
            Place(world, "CasaA", new Vector3(-28, 0, 17), 0, true);
            Place(world, "CasaB", new Vector3(-28, 0, -8), 0, true);
            Place(world, "CasaC", new Vector3(-22, 0, -20), 0, true);
            Place(world, "CasaB", new Vector3(15, 0, -26), 0, true);

            // South: League post right inside the gate (where the game starts).
            Place(world, "Tenda", new Vector3(-9, 0, -26), 0, true);
            Place(world, "Carroca", new Vector3(7, 0, -30), 90);
            Place(world, "MesaMapas", new Vector3(6, 0, -24.5f), 0, true);

            Walls(world, R);
            Nature(world, R);
            Details(world);
            Ground(world);

            // River and bridge to the east.
            River(world);
            Place(world, "Ponte", new Vector3(58, 0, 0), 0);
            return world;
        }

        // ---------- Step 4: the new city inside the real game ----------

        // The new city is the main town scene. The first town is kept as RefugioAntigo (source of the game systems, reusable for another city).
        public const string OldScene = "Assets/ForjaDasMares/Scenes/RefugioAntigo.unity";
        const string TestScene = "Assets/ForjaDasMares/Scenes/RefugioNovo.unity";
        const string TownData = "Assets/ForjaDasMares/Data/World/Town/";

        // Copies the current town scene (hero, HUD, inventory, NPCs and services stay exactly as they are),
        // removes only the old scenery and puts the new city in its place. The old scene is not changed.
        [MenuItem("Forja/Cidade nova/4. Atualizar a cidade do jogo e jogar")]
        public static void BuildGame()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Prepare();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            string GameScene = ForjaBuild.Cidade;
            if (!File.Exists(OldScene))
            {
                string moved = AssetDatabase.MoveAsset(GameScene, OldScene);
                if (!string.IsNullOrEmpty(moved)) { Debug.LogError("Não consegui guardar a cidade antiga: " + moved); return; }
            }
            AssetDatabase.DeleteAsset(TestScene);
            File.Copy(OldScene, GameScene, true);
            AssetDatabase.ImportAsset(GameScene, ImportAssetOptions.ForceUpdate);
            // Build list: the main town path must point to the new scene.
            var buildScenes = EditorBuildSettings.scenes.Where(x => x.path != OldScene && x.path != TestScene && x.path != GameScene).ToList();
            int at = System.Array.FindIndex(EditorBuildSettings.scenes, x => x.path == OldScene || x.path == GameScene);
            buildScenes.Insert(Mathf.Clamp(at < 0 ? buildScenes.Count : at, 0, buildScenes.Count), new EditorBuildSettingsScene(GameScene, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
            var scene = EditorSceneManager.OpenScene(GameScene);
            var world = Object.FindFirstObjectByType<RegionalWorld>();
            var hub = Object.FindFirstObjectByType<TownHub>();
            if (world == null || hub == null) { Debug.LogError("A cena da cidade não tem RegionalWorld/TownHub."); return; }

            // 1. Keep every service and NPC; drop the old scenery.
            var services = new GameObject("Servicos").transform;
            var env = GameObject.Find("TownEnvironment");
            if (env != null)
            {
                var keep = new HashSet<GameObject>();
                foreach (var c in env.GetComponentsInChildren<WorldClickTarget>(true)) keep.Add(c.gameObject);
                foreach (var c in env.GetComponentsInChildren<TownResident>(true)) keep.Add(c.gameObject);
                foreach (var c in env.GetComponentsInChildren<TownRoadPassage>(true)) keep.Add(c.gameObject);
                foreach (var c in env.GetComponentsInChildren<Health>(true)) keep.Add(c.gameObject);
                foreach (var go in keep.ToArray())
                {
                    bool nested = false;
                    for (var t = go.transform.parent; t != null; t = t.parent) if (keep.Contains(t.gameObject)) { nested = true; break; }
                    if (nested) continue;
                    var instance = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                    if (instance != null) PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    go.transform.SetParent(services, true);
                }
                Object.DestroyImmediate(env);
            }
            foreach (var feedback in services.GetComponentsInChildren<WorldInteractionFeedback>(true)) Object.DestroyImmediate(feedback);
            foreach (var cue in services.GetComponentsInChildren<TownExitCue>(true)) Object.DestroyImmediate(cue);

            // 2. The new city.
            var city = BuildCity();
            foreach (var c in city.GetComponentsInChildren<Collider>(true)) c.gameObject.layer = 9;
            var grass = city.Find("Chao/Grama"); if (grass != null) grass.gameObject.layer = 8;
            // The east and west gates lead to maps that do not exist yet: closed for now.
            foreach (float x in new[] { -37.5f, 37.5f }) Blocker(city, new Vector3(x, 2, 0), new Vector3(1.5f, 4, 9));
            Blocker(city, new Vector3(0, 2, -42), new Vector3(12, 4, 1.5f));
            Blocker(city, new Vector3(-7, 2, -39.5f), new Vector3(1.5f, 4, 6));
            Blocker(city, new Vector3(7, 2, -39.5f), new Vector3(1.5f, 4, 6));

            // 3. Everyone to their new place.
            var spawn = new Vector3(0, .16f, -28);
            Vector3 nara = new Vector3(-5, 0, -23), daro = new Vector3(19, 0, 3.8f);
            int roamer = 0;
            var log = new System.Text.StringBuilder();
            foreach (Transform child in services)
            {
                var target = child.GetComponent<WorldClickTarget>();
                string label = (target != null ? target.Label : "") + " " + child.name;
                Vector3? to = null; string sign = null;
                if (label.Contains("Nara")) to = nara;
                else if (label.Contains("Daro")) to = daro;
                else if (label.Contains("Iria")) to = new Vector3(11.5f, 0, -5);
                else if (label.Contains("Luma")) to = new Vector3(-9, 0, -16);
                else if (label.Contains("Téo")) to = new Vector3(19.5f, 0, 9);
                else if (label.Contains("fonte")) to = new Vector3(9, 0, -7);
                else if (label.Contains("Expedições")) { to = new Vector3(4.5f, 0, 19.5f); sign = "EXPEDIÇÕES"; }
                else if (label.Contains("santuário")) { to = new Vector3(-13.5f, 0, 23); sign = "ESTAÇÃO ANTIGA"; }
                else if (child.GetComponent<TownRoadPassage>() != null) to = new Vector3(0, 0, -38.5f);
                else if (label.Contains("Costa")) { to = new Vector3(0, 0, -34.5f); sign = "SAÍDA · COSTA"; }
                else if (label.Contains("TrainingDummy")) { to = new Vector3(-10.5f, 0, -12.5f); sign = "TREINO"; }
                else if (child.GetComponent<TownResident>() != null)
                {
                    float a = roamer++ * 2.1f + .6f;
                    to = new Vector3(Mathf.Sin(a) * 12, 0, Mathf.Cos(a) * 12);
                    var so = new SerializedObject(child.GetComponent<TownResident>());
                    var route = so.FindProperty("waypoints");
                    if (route != null && route.arraySize >= 2)
                    {
                        for (int i = 0; i < route.arraySize; i++)
                        {
                            float b = a + (i + 1) * 1.7f;
                            route.GetArrayElementAtIndex(i).vector3Value = new Vector3(Mathf.Sin(b) * 20.5f, 0, Mathf.Cos(b) * 20.5f);
                        }
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
                if (to == null) { log.AppendLine("Sem lugar novo (ficou onde estava): " + child.name); continue; }
                var p = to.Value; p.y = child.position.y;
                child.position = p;
                var outward = new Vector3(p.x, 0, p.z);
                bool resident = child.GetComponent<TownResident>() != null;
                if (child.GetComponent<TownRoadPassage>() == null && outward.sqrMagnitude > 1) child.rotation = Quaternion.LookRotation(resident ? -outward.normalized : outward.normalized);
                if (resident && target != null)
                {
                    // Residents look at the plaza and the player walks up to their front.
                    var targetData = new SerializedObject(target);
                    var offset = targetData.FindProperty("approachOffset");
                    if (offset != null) { offset.vector3Value = Vector3.forward * 1.6f; targetData.ApplyModifiedPropertiesWithoutUndo(); }
                    string model = label.Contains("Téo") ? "NPCTeo" : label.Contains("Nara") ? "NPCOficial" : label.Contains("Iria") ? "NPCIria" : null;
                    var modelPrefab = model != null ? Prefab(model) : null;
                    if (modelPrefab != null)
                    {
                        foreach (var r in child.GetComponentsInChildren<MeshRenderer>(true)) if (r.GetComponent<TextMesh>() == null) r.enabled = false;
                        var body = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, child);
                        body.transform.localPosition = new Vector3(0, -child.position.y, 0); body.transform.localRotation = Quaternion.identity;
                        foreach (var t in body.GetComponentsInChildren<Transform>(true)) t.gameObject.isStatic = false;
                        body.AddComponent<NpcIdle>().Configure(model == "NPCTeo" ? NpcIdle.Style.Ferreiro : model == "NPCIria" ? NpcIdle.Style.Curandeira : NpcIdle.Style.Oficial);
                    }
                }
                if (sign != null)
                {
                    // The old model of this service is gone: hide what is left of it and show a simple sign.
                    if (child.GetComponent<Health>() == null) foreach (var r in child.GetComponentsInChildren<MeshRenderer>(true)) r.enabled = false;
                    Sign(city, sign, new Vector3(p.x, 3.2f, p.z));
                }
                else if (child.GetComponent<TownResident>() == null && child.GetComponent<TownRoadPassage>() == null)
                    foreach (var r in child.GetComponentsInChildren<MeshRenderer>(true)) r.enabled = false;
            }

            // The market merchant stands by the fruit stall (scenery for now).
            var merchantSpot = new Vector3(11.5f, 0, 11.5f);
            var merchant = Place(city, "NPCMercador", merchantSpot, 0, true);
            if (merchant != null)
            {
                foreach (var t in merchant.GetComponentsInChildren<Transform>(true)) t.gameObject.isStatic = false;
                merchant.AddComponent<NpcIdle>().Configure(NpcIdle.Style.Mercador);
                var nameText = new GameObject("Nome").AddComponent<TextMesh>();
                nameText.transform.SetParent(merchant.transform, false);
                nameText.transform.localPosition = Vector3.up * 2.2f;
                nameText.text = "Brás · Feirante"; nameText.fontSize = 48; nameText.characterSize = .035f; nameText.anchor = TextAnchor.MiddleCenter;
                merchant.AddComponent<TownResident>().Configure(null, null, null, null, null, nameText.transform);
            }

            // 4. Region and story data for this scene only (the old city keeps its own).
            var region = Object.Instantiate(world.Definition);
            AssetDatabase.DeleteAsset(TownData + "RegionNova.asset");
            AssetDatabase.CreateAsset(region, TownData + "RegionNova.asset");
            region.refuge = spawn; region.portal = new Vector3(0, .08f, -35);
            region.bounds = new Bounds(Vector3.zero, new Vector3(84, 30, 84));
            region.route = new[] { spawn, new Vector3(0, 0, -35) };
            if (region.landmarks != null)
                for (int i = 0; i < region.landmarks.Length; i++)
                {
                    var mark = region.landmarks[i];
                    if (mark.label == "Nara") mark.position = nara; else if (mark.label == "Daro") mark.position = daro; else if (mark.label == "Treino") mark.position = new Vector3(-10.5f, 0, -12.5f);
                    region.landmarks[i] = mark;
                }
            EditorUtility.SetDirty(region);
            world.Configure(region, world.Hero, world.Enemies, Object.FindFirstObjectByType<ArenaCamera>(), AssetDatabase.LoadAssetAtPath<SoundscapeLibrary>("Assets/ForjaDasMares/Resources/CampaignSoundscape.asset"));
            var journey = world.GetComponent<RegionalJourney>();
            if (journey != null && journey.Definition != null)
            {
                var story = Object.Instantiate(journey.Definition);
                AssetDatabase.DeleteAsset(TownData + "StoryNova.asset");
                AssetDatabase.CreateAsset(story, TownData + "StoryNova.asset");
                if (story.points.Length > 1) { story.points[0].position = new Vector3(nara.x, .08f, nara.z); story.points[1].position = new Vector3(daro.x, .08f, daro.z); }
                EditorUtility.SetDirty(story);
                journey.Configure(world, story);
                EditorUtility.SetDirty(journey);
            }
            hub.Configure(world, new[] { spawn, spawn + new Vector3(-1.4f, 0, 0), spawn + new Vector3(1.4f, 0, 0), spawn + new Vector3(0, 0, 1.4f) });
            var hubData = new SerializedObject(hub);
            var keepPlace = hubData.FindProperty("fixedExpeditionPlace");
            if (keepPlace != null) { keepPlace.boolValue = true; hubData.ApplyModifiedPropertiesWithoutUndo(); }
            EditorUtility.SetDirty(hub); EditorUtility.SetDirty(world);
            var heroTransform = world.Hero != null ? world.Hero.transform : null;
            if (heroTransform != null) heroTransform.position = spawn;

            // 5. Warm light, and walking paths rebuilt for the new layout.
            var sun = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(l => l.type == LightType.Directional);
            if (sun != null) { sun.color = new Color(1f, .88f, .72f); sun.intensity = 1.45f; sun.transform.rotation = Quaternion.Euler(42, -35, 0); }
            RenderSettings.fogColor = new Color(.90f, .82f, .70f); RenderSettings.fogStartDistance = 90; RenderSettings.fogEndDistance = 320;
            Physics.SyncTransforms();
            var nav = Object.FindFirstObjectByType<Unity.AI.Navigation.NavMeshSurface>();
            if (nav != null)
            {
                nav.RemoveData(); nav.navMeshData = null;
                nav.BuildNavMesh();
                AssetDatabase.DeleteAsset(TownData + "NavigationNova.asset");
                AssetDatabase.CreateAsset(nav.navMeshData, TownData + "NavigationNova.asset");
                EditorUtility.SetDirty(nav);
                foreach (var t in Object.FindObjectsByType<WorldClickTarget>(FindObjectsSortMode.None))
                    if (!UnityEngine.AI.NavMesh.SamplePosition(t.Approach, out _, 2.5f, UnityEngine.AI.NavMesh.AllAreas)) log.AppendLine("Sem caminho até: " + t.Label);
            }
            else log.AppendLine("Não achei o NavMeshSurface da cena.");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("CIDADE_NOVA_NO_JOGO_PRONTA\n" + log);
            EditorApplication.isPlaying = true;
        }

        static void Blocker(Transform parent, Vector3 position, Vector3 size)
        {
            var go = new GameObject("Bloqueio");
            go.transform.SetParent(parent, false); go.transform.position = position; go.layer = 9;
            go.AddComponent<BoxCollider>().size = size;
        }

        static void Sign(Transform parent, string text, Vector3 position)
        {
            var t = new GameObject("Placa " + text).AddComponent<TextMesh>();
            t.transform.SetParent(parent, false); t.transform.position = position; t.transform.rotation = Quaternion.Euler(50, 0, 0);
            t.text = text; t.characterSize = .05f; t.fontSize = 90; t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center;
            t.color = new Color(1f, .86f, .5f);
            var post = Soft(parent, PrimitiveType.Cylinder, "Marco", new Vector3(position.x, .6f, position.z - .9f), new Vector3(.35f, .6f, .35f), Flat("LuzLanterna", new Color(1f, .86f, .55f)));
            post.layer = 0;
        }

        // Flat ring mesh for the round street (one piece, so nothing overlaps and flickers).
        static Mesh Annulus(float inner, float outer, int segments)
        {
            EnsureFolder();
            var vertices = new Vector3[(segments + 1) * 2]; var uv = new Vector2[vertices.Length]; var normals = new Vector3[vertices.Length];
            var triangles = new int[segments * 6];
            for (int i = 0; i <= segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2;
                var dir = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a));
                vertices[i * 2] = dir * inner; vertices[i * 2 + 1] = dir * outer;
                float along = a * (inner + outer) * .5f / 5f;
                uv[i * 2] = new Vector2(0, along); uv[i * 2 + 1] = new Vector2((outer - inner) / 5f, along);
                normals[i * 2] = normals[i * 2 + 1] = Vector3.up;
            }
            for (int i = 0; i < segments; i++)
            {
                int v = i * 2, t = i * 6;
                triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
                triangles[t + 3] = v + 1; triangles[t + 4] = v + 3; triangles[t + 5] = v + 2;
            }
            var mesh = new Mesh { name = "Anel" };
            mesh.vertices = vertices; mesh.uv = uv; mesh.normals = normals; mesh.triangles = triangles;
            // Make sure the top faces up.
            if (Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]).y < 0) { System.Array.Reverse(triangles); mesh.triangles = triangles; }
            mesh.RecalculateBounds();
            string path = Art + "_Chao/Anel.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        [MenuItem("Forja/Cidade nova/3. Passear na cidade (Play)")]
        public static void Walk()
        {
            if (!File.Exists(PreviewScene)) { Build(); }
            else { if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return; EditorSceneManager.OpenScene(PreviewScene); }
            EditorApplication.isPlaying = true;
        }

        static GameObject Place(Transform parent, string slug, Vector3 position, float yaw, bool faceCentre = false)
        {
            var prefab = Prefab(slug);
            if (prefab == null) { Debug.LogWarning("Prefab faltando: " + slug); return null; }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = position;
            if (faceCentre)
            {
                var dir = -new Vector3(position.x, 0, position.z);
                if (dir.sqrMagnitude > .01f) yaw = Quaternion.LookRotation(dir).eulerAngles.y;
            }
            go.transform.rotation = Quaternion.Euler(0, yaw, 0);
            placed.Add(position);
            return go;
        }

        static void Walls(Transform parent, float radius)
        {
            var wall = Prefab("Muralha");
            var tower = Prefab("TorreMuralha");
            if (wall == null) return;
            var holder = new GameObject("Muralha").transform; holder.SetParent(parent, false);
            var probe = (GameObject)PrefabUtility.InstantiatePrefab(wall);
            float length = Mathf.Max(1f, Bounds(probe).size.x);
            Object.DestroyImmediate(probe);
            float gateWidth = 11;
            var gatePrefab = Prefab("PortaoSul");
            if (gatePrefab != null)
            {
                var gateProbe = (GameObject)PrefabUtility.InstantiatePrefab(gatePrefab);
                var gb = Bounds(gateProbe); gateWidth = Mathf.Max(gb.size.x, gb.size.z);
                Object.DestroyImmediate(gateProbe);
            }
            // Three gates: east (to the bridge), south (where the player arrives) and west (to the fields).
            // Angle convention: position = (sin a, 0, cos a) * radius.
            float[] gates = { Mathf.PI * .5f, Mathf.PI, Mathf.PI * 1.5f };
            float half = Mathf.Asin(Mathf.Clamp01((gateWidth * .5f - .9f) / radius));
            for (int g = 0; g < gates.Length; g++)
            {
                var gp = new Vector3(Mathf.Sin(gates[g]) * radius, 0, Mathf.Cos(gates[g]) * radius);
                var gate = Place(parent, "PortaoSul", gp, 0);
                if (gate != null) gate.transform.rotation = Quaternion.LookRotation(gp.normalized);

                // Wall from this gate to the next one, with the pieces stretched a little so both ends touch the gates.
                float from = gates[g] + half;
                float to = (g + 1 < gates.Length ? gates[g + 1] : gates[0] + Mathf.PI * 2) - half;
                float arc = (to - from) * radius;
                int count = Mathf.Max(1, Mathf.CeilToInt(arc / length));
                float stretch = arc / count / length * 1.04f;
                for (int i = 0; i < count; i++)
                {
                    float a = from + (i + .5f) / count * (to - from);
                    var p = new Vector3(Mathf.Sin(a) * radius, 0, Mathf.Cos(a) * radius);
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(wall, holder);
                    go.transform.position = p;
                    go.transform.rotation = Quaternion.LookRotation(-p.normalized);
                    go.transform.localScale = new Vector3(stretch, 1, 1);
                }
                if (tower == null) continue;
                int towers = Mathf.Max(1, Mathf.RoundToInt(arc / 28f));
                for (int i = 1; i <= towers; i++)
                {
                    float a = from + i / (towers + 1f) * (to - from);
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(tower, holder);
                    go.transform.position = new Vector3(Mathf.Sin(a) * radius, 0, Mathf.Cos(a) * radius);
                }
            }
        }

        // Stone paths: a ring street in front of the buildings and a short path to each door.
        static void Paths(Transform parent, Texture2D stone)
        {
            var holder = new GameObject("Caminhos").transform; holder.SetParent(parent, false);
            const float ring = 20.5f;
            var ringObject = new GameObject("Anel");
            ringObject.transform.SetParent(holder, false); ringObject.transform.position = new Vector3(0, .03f, 0);
            ringObject.AddComponent<MeshFilter>().sharedMesh = Annulus(ring - 1.5f, ring + 1.5f, 72);
            ringObject.AddComponent<MeshRenderer>().sharedMaterial = Textured("Anel", stone, Vector2.one, new Color(.93f, .88f, .82f));
            ringObject.isStatic = true;
            int index = 0;
            foreach (var b in placed)
            {
                float r = new Vector2(b.x, b.z).magnitude;
                if (r < ring + 2.5f || r > 34) continue;
                var dir = new Vector3(b.x, 0, b.z).normalized;
                float len = r - 2.5f - ring;
                var mid = dir * (ring + len * .5f); mid.y = -.005f;
                var go = Soft(holder, PrimitiveType.Cube, "Entrada", mid, new Vector3(2.4f, .05f, len + 1f), Textured("Entrada" + index++, stone, new Vector2(.5f, (len + 1f) / 5f), new Color(.90f, .84f, .76f)));
                go.transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        static readonly List<Vector3> placed = new List<Vector3>();
        static bool Free(Vector3 p, float distance)
        {
            foreach (var b in placed) if ((b - p).sqrMagnitude < distance * distance) return false;
            return true;
        }
        static bool OnRoad(Vector3 p) => Mathf.Abs(p.x) < 4.5f || Mathf.Abs(p.z) < 4.5f || Mathf.Abs(new Vector2(p.x, p.z).magnitude - 20.5f) < 2.6f || OnEntrance(p);

        // The little stone paths from the ring street to each building (same rule Paths() uses to draw them).
        static bool OnEntrance(Vector3 p)
        {
            const float ring = 20.5f;
            foreach (var b in placed)
            {
                float r = new Vector2(b.x, b.z).magnitude;
                if (r < ring + 2.5f || r > 34) continue;
                var dir = new Vector3(b.x, 0, b.z).normalized;
                float along = Vector3.Dot(new Vector3(p.x, 0, p.z), dir);
                if (along < ring - 1 || along > r) continue;
                float side = (new Vector3(p.x, 0, p.z) - dir * along).magnitude;
                if (side < 2.4f) return true;
            }
            return false;
        }

        // ---------- Ground, water and light ----------

        static Material Flat(string name, Color color, float smooth = .1f)
        {
            string path = Art + "_Chao/" + name + ".mat";
            EnsureFolder();
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", color); m.SetFloat("_Smoothness", smooth);
            EditorUtility.SetDirty(m);
            return m;
        }

        static GameObject Shape(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, float yaw = 0)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name; go.transform.SetParent(parent, false);
            go.transform.position = position; go.transform.localScale = scale; go.transform.rotation = Quaternion.Euler(0, yaw, 0);
            go.GetComponent<Renderer>().sharedMaterial = material;
            go.isStatic = true;
            return go;
        }

        static void Ground(Transform parent)
        {
            var ground = new GameObject("Chao").transform; ground.SetParent(parent, false);
            var grassTex = MakeTexture("GramaTextura", false);
            var stoneTex = MakeTexture("PedraTextura", true);
            Shape(ground, PrimitiveType.Plane, "Grama", Vector3.zero, new Vector3(60, 1, 60), Textured("Grama", grassTex, new Vector2(120, 120), Color.white));
            Disc(ground, "PracaBorda", new Vector3(0, -.015f, 0), 32.4f, 32.4f, stoneTex, new Color(.72f, .64f, .54f));
            Disc(ground, "Praca", new Vector3(0, -.005f, 0), 30, 30, stoneTex, Color.white);
            Disc(ground, "PatioSalao", new Vector3(0, -.02f, 22), 18, 12, stoneTex, Color.white);
            Disc(ground, "PatioLiga", new Vector3(0, -.02f, -27), 22, 12, stoneTex, new Color(.92f, .86f, .78f));
            Road(ground, "RuaSul", new Vector3(0, .015f, -25.5f), 6, 22, stoneTex);
            Road(ground, "RuaNorte", new Vector3(0, .015f, 17), 5, 8, stoneTex);
            Road(ground, "RuaLeste", new Vector3(38.5f, .015f, 0), 47, 5, stoneTex);
            Road(ground, "RuaOeste", new Vector3(-24.5f, .015f, 0), 21, 5, stoneTex);
            var dirt = Flat("Terra", new Color(.66f, .54f, .38f));
            Soft(ground, PrimitiveType.Cube, "EstradaSul", new Vector3(0, -.015f, -86), new Vector3(7, .05f, 100), dirt);
            Soft(ground, PrimitiveType.Cube, "EstradaOeste", new Vector3(-76, -.015f, 0), new Vector3(80, .05f, 6), dirt);
            Soft(ground, PrimitiveType.Cube, "EstradaLeste", new Vector3(92, -.015f, 0), new Vector3(60, .05f, 6), dirt);
            Paths(ground, stoneTex);
            // Soft green hills on the horizon so the world does not end in a flat line.
            var hill = Flat("Colina", new Color(.36f, .56f, .30f));
            var random = new System.Random(5);
            for (int i = 0; i < 14; i++)
            {
                float a = i / 14f * Mathf.PI * 2 + (float)random.NextDouble() * .3f;
                float d = 150 + (float)random.NextDouble() * 60;
                float w = 90 + (float)random.NextDouble() * 70;
                if (Mathf.Abs(Mathf.Sin(a)) < .25f && Mathf.Cos(a) > 0) continue; // keep the giant ruin visible to the north
                Soft(ground, PrimitiveType.Sphere, "Colina", new Vector3(Mathf.Sin(a) * d, -8, Mathf.Cos(a) * d), new Vector3(w, 30 + (float)random.NextDouble() * 40, w), hill);
            }
        }

        static void Disc(Transform parent, string name, Vector3 position, float width, float depth, Texture2D texture, Color tint) =>
            Soft(parent, PrimitiveType.Cylinder, name, position, new Vector3(width, .07f, depth), Textured(name, texture, new Vector2(width / 5f, depth / 5f), tint));

        static void Road(Transform parent, string name, Vector3 position, float width, float depth, Texture2D texture) =>
            Soft(parent, PrimitiveType.Cube, name, position, new Vector3(width, .05f, depth), Textured(name, texture, new Vector2(width / 5f, depth / 5f), new Color(.95f, .9f, .84f)));

        static Material Textured(string name, Texture2D texture, Vector2 tiling, Color tint)
        {
            var m = Flat("Tex_" + name, tint);
            m.SetTexture("_BaseMap", texture);
            m.SetTextureScale("_BaseMap", tiling);
            EditorUtility.SetDirty(m);
            return m;
        }

        // Tileable value noise.
        static float Hash(int x, int y, int seed) { unchecked { int h = x * 374761393 + y * 668265263 + seed * 1442695041; h = (h ^ (h >> 13)) * 1274126177; return ((h ^ (h >> 16)) & 0xFFFF) / 65535f; } }
        static float Noise(float x, float y, int period, int seed)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0; fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
            int xa = ((x0 % period) + period) % period, xb = (xa + 1) % period, ya = ((y0 % period) + period) % period, yb = (ya + 1) % period;
            return Mathf.Lerp(Mathf.Lerp(Hash(xa, ya, seed), Hash(xb, ya, seed), fx), Mathf.Lerp(Hash(xa, yb, seed), Hash(xb, yb, seed), fx), fy);
        }

        // Painted-looking tileable textures made here, so no download is needed: rounded cobblestones and soft grass.
        static Texture2D MakeTexture(string name, bool stones)
        {
            EnsureFolder();
            string path = Art + "_Chao/" + name + ".png";
            const int size = 512, cells = 8;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    Color c;
                    if (stones)
                    {
                        float gx = u * cells, gy = v * cells; int cx = Mathf.FloorToInt(gx), cy = Mathf.FloorToInt(gy);
                        float best = 9, second = 9; int bx = 0, by = 0;
                        for (int j = -1; j <= 1; j++)
                            for (int i = -1; i <= 1; i++)
                            {
                                int nx = cx + i, ny = cy + j; int wx = ((nx % cells) + cells) % cells, wy = ((ny % cells) + cells) % cells;
                                float px = nx + .2f + Hash(wx, wy, 1) * .6f, py = ny + .2f + Hash(wx, wy, 2) * .6f;
                                float d = (px - gx) * (px - gx) + (py - gy) * (py - gy);
                                if (d < best) { second = best; best = d; bx = wx; by = wy; } else if (d < second) second = d;
                            }
                        float edge = Mathf.Sqrt(second) - Mathf.Sqrt(best);
                        float tone = .78f + Hash(bx, by, 3) * .22f;
                        var stone = Color.Lerp(new Color(.80f, .70f, .56f), new Color(.88f, .80f, .68f), Hash(bx, by, 4)) * tone;
                        float round = Mathf.SmoothStep(0, 1, Mathf.Clamp01(edge / .22f));
                        c = Color.Lerp(new Color(.50f, .43f, .34f), stone, round) * (.94f + Noise(u * 64, v * 64, 64, 9) * .08f);
                    }
                    else
                    {
                        float n = Noise(u * 8, v * 8, 8, 5) * .6f + Noise(u * 32, v * 32, 32, 6) * .3f + Noise(u * 128, v * 128, 128, 7) * .1f;
                        c = Color.Lerp(new Color(.36f, .58f, .26f), new Color(.52f, .72f, .34f), n);
                    }
                    c.a = 1; pixels[y * size + x] = c;
                }
            var texture = new Texture2D(size, size, TextureFormat.RGB24, false);
            texture.SetPixels(pixels); texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                importer.wrapMode = TextureWrapMode.Repeat; importer.maxTextureSize = 512; importer.mipmapEnabled = true;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // Small things that give life: lamp posts, flowers, barrels and crates.
        static void Details(Transform parent)
        {
            var holder = new GameObject("Detalhes").transform; holder.SetParent(parent, false);
            for (int i = 0; i < 8; i++)
            {
                float a = (i + .5f) / 8f * Mathf.PI * 2;
                var lampSpot = new Vector3(Mathf.Sin(a) * 16.6f, 0, Mathf.Cos(a) * 16.6f);
                Lamp(holder, lampSpot, -lampSpot);
            }
            foreach (float z in new[] { -24f, -31.5f }) { Lamp(holder, new Vector3(-3.7f, 0, z), Vector3.right); Lamp(holder, new Vector3(3.7f, 0, z), Vector3.left); }
            foreach (float x in new[] { 31f, -31f }) { Lamp(holder, new Vector3(x, 0, 3.4f), Vector3.back); Lamp(holder, new Vector3(x, 0, -3.4f), Vector3.forward); }

            var random = new System.Random(31);
            Color[] petals = { new Color(.95f, .45f, .6f), new Color(.98f, .85f, .35f), new Color(.75f, .55f, .95f), new Color(.98f, .98f, .95f) };
            var kitFlowers = VegetacaoInstaller.Load("Flores"); var kitGrass = VegetacaoInstaller.Load("Gramas");
            for (int i = 0; i < 18; i++)
            {
                float a = (float)random.NextDouble() * Mathf.PI * 2, d = 17 + (float)random.NextDouble() * 15;
                var p = new Vector3(Mathf.Sin(a) * d, 0, Mathf.Cos(a) * d);
                if (OnRoad(p) || !Free(p, 5f)) continue;
                if (kitFlowers.Length > 0)
                {
                    // Flower bed: hand-painted flowers with some grass between them.
                    for (int k = 0; k < 6; k++)
                    {
                        var spot = p + new Vector3((float)random.NextDouble() * 1.6f - .8f, 0, (float)random.NextDouble() * 1.6f - .8f);
                        Kit(holder, k < 4 || kitGrass.Length == 0 ? kitFlowers : kitGrass, spot, 1.3f + (float)random.NextDouble() * .4f, random);
                    }
                    continue;
                }
                var petal = Flat("Flor" + (i % petals.Length), petals[i % petals.Length]);
                for (int k = 0; k < 3; k++)
                    Soft(holder, PrimitiveType.Sphere, "Flor", p + new Vector3((float)random.NextDouble() - .5f, .18f, (float)random.NextDouble() - .5f), Vector3.one * .38f, petal);
                Soft(holder, PrimitiveType.Sphere, "Folhas", p + Vector3.up * .05f, new Vector3(1.3f, .3f, 1.3f), Flat("FolhaEscura", new Color(.28f, .52f, .27f)));
            }

            var wood = Flat("Madeira", new Color(.55f, .38f, .22f)); var woodDark = Flat("MadeiraEscura", new Color(.42f, .29f, .18f));
            Vector3[] piles = { new Vector3(19, 0, 11), new Vector3(20, 0, 2), new Vector3(11, 0, -13), new Vector3(10, 0, -28), new Vector3(-12, 0, -22), new Vector3(-20, 0, 9) };
            foreach (var p in piles)
            {
                if (Prefab("Caixotes") != null) { Place(holder, "Caixotes", p + new Vector3(.5f, 0, -.2f), (float)random.NextDouble() * 360); continue; }
                Shape(holder, PrimitiveType.Cylinder, "Barril", p + new Vector3(0, .55f, 0), new Vector3(.8f, .55f, .8f), wood);
                Shape(holder, PrimitiveType.Cube, "Caixote", p + new Vector3(1f, .4f, .3f), Vector3.one * .8f, woodDark, 20);
                Shape(holder, PrimitiveType.Cube, "Caixote", p + new Vector3(.4f, .3f, -.9f), Vector3.one * .6f, wood, 50);
            }

            // Benches around the plaza, facing the monument.
            for (int i = 0; i < 4; i++)
            {
                float a = (i + .5f) / 4f * Mathf.PI * 2;
                var p = new Vector3(Mathf.Sin(a) * 13.2f, 0, Mathf.Cos(a) * 13.2f);
                if (Free(p, 3f)) Place(holder, "Banco", p, 0, true);
            }
            // Tables outside the inn and a notice board by the expedition hall.
            int tables = 0;
            foreach (var p in new[] { new Vector3(-17.5f, 0, 11f), new Vector3(-17.5f, 0, 1.5f), new Vector3(-25f, 0, 14f), new Vector3(-25f, 0, -2f) })
                if (tables < 2 && !OnRoad(p) && Free(p, 3f)) { Place(holder, "MesaBancos", p, 90); tables++; }
            foreach (var p in new[] { new Vector3(8.5f, 0, 18f), new Vector3(-8.5f, 0, 18f) })
                if (!OnRoad(p) && Free(p, 2.5f)) { Place(holder, "QuadroAvisos", p, 0, true); break; }
        }

        // "toward" = where the lantern should hang: over the road or the plaza.
        static void Lamp(Transform parent, Vector3 p, Vector3 toward)
        {
            if (Prefab("Poste") != null)
            {
                // The lantern arm of the model points along its local +X, so that axis turns to "toward".
                toward.y = 0; toward.Normalize();
                float yaw = Quaternion.LookRotation(Vector3.Cross(toward, Vector3.up)).eulerAngles.y;
                Place(parent, "Poste", p, yaw);
                return;
            }
            var iron = Flat("Ferro", new Color(.18f, .17f, .18f));
            var glow = Flat("LuzLanterna", new Color(1f, .86f, .55f));
            glow.EnableKeyword("_EMISSION");
            glow.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            glow.SetColor("_EmissionColor", new Color(1f, .75f, .35f) * 2.5f);
            Shape(parent, PrimitiveType.Cylinder, "Poste", p + Vector3.up * 1.5f, new Vector3(.14f, 1.5f, .14f), iron);
            Soft(parent, PrimitiveType.Cylinder, "PosteBase", p + Vector3.up * .15f, new Vector3(.4f, .15f, .4f), iron);
            Soft(parent, PrimitiveType.Sphere, "Lanterna", p + Vector3.up * 3.15f, Vector3.one * .5f, glow);
        }

        // Round puffy trees, bushes and rocks: a few inside, a thick belt outside so the world seems to go on.
        static void Nature(Transform parent, float wallRadius)
        {
            var holder = new GameObject("Natureza").transform; holder.SetParent(parent, false);
            var random = new System.Random(77);
            // Trees inside: near the wall, only where there is no building, road or gate.
            int planted = 0;
            for (int i = 0; i < 24; i++)
            {
                float a = (i + .5f) / 24f * Mathf.PI * 2;
                var p = new Vector3(Mathf.Sin(a) * 31.5f, 0, Mathf.Cos(a) * 31.5f);
                if (Mathf.Abs(p.x) < 7 || Mathf.Abs(p.z) < 7 || !Free(p, 8.5f) || OnEntrance(p)) continue;
                Tree(holder, p, 1f, planted++ % 3 == 0, random, true);
            }
            foreach (var p in new[] { new Vector3(-12, 0, 13), new Vector3(11.5f, 0, -15.5f) })
                if (Free(p, 4.5f)) Tree(holder, p, .9f, true, random, true);
            for (int i = 0; i < 70; i++)
            {
                float a = (float)random.NextDouble() * Mathf.PI * 2;
                float d = wallRadius + 6 + (float)random.NextDouble() * 34;
                var p = new Vector3(Mathf.Sin(a) * d, 0, Mathf.Cos(a) * d);
                if (p.z < 0 && Mathf.Abs(p.x) < 8) continue;          // south road
                if (Mathf.Abs(p.x - 58) < 8) continue;                // river
                if (Mathf.Abs(p.z) < 7) continue;                     // east and west roads
                Tree(holder, p, 1.1f + (float)random.NextDouble() * .5f, random.NextDouble() < .2, random, false);
                if (random.NextDouble() < .5) Bush(holder, p + new Vector3((float)random.NextDouble() * 5 - 2.5f, 0, (float)random.NextDouble() * 5 - 2.5f), random);
                if (random.NextDouble() < .25) Rock(holder, p + new Vector3((float)random.NextDouble() * 6 - 3, 0, (float)random.NextDouble() * 6 - 3), random);
            }
            for (int i = 0; i < 14; i++)
            {
                float a = i / 14f * Mathf.PI * 2 + .2f;
                var p = new Vector3(Mathf.Sin(a) * 17f, 0, Mathf.Cos(a) * 17f);
                if (Mathf.Abs(p.x) < 4.5f || Mathf.Abs(p.z) < 4.5f || !Free(p, 3.5f)) continue;
                Bush(holder, p, random);
            }
            if (!VegetacaoInstaller.Ready) return;

            // More trees inside the walls, on the grass between the houses.
            int extra = 0; var treeSpots = new List<Vector3>();
            for (int i = 0; i < 400 && extra < 22; i++)
            {
                float a = (float)random.NextDouble() * Mathf.PI * 2, d = 18 + (float)random.NextDouble() * 15;
                var p = new Vector3(Mathf.Sin(a) * d, 0, Mathf.Cos(a) * d);
                if (OnRoad(p) || !Free(p, 7.5f) || Mathf.Abs(d - 20.5f) < 4 || treeSpots.Exists(t => (t - p).sqrMagnitude < 25)) continue;
                Tree(holder, p, .8f + (float)random.NextDouble() * .3f, false, random, true);
                treeSpots.Add(p); extra++;
            }
            // A denser forest outside.
            for (int i = 0; i < 90; i++)
            {
                float a = (float)random.NextDouble() * Mathf.PI * 2, d = wallRadius + 5 + (float)random.NextDouble() * 38;
                var p = new Vector3(Mathf.Sin(a) * d, 0, Mathf.Cos(a) * d);
                if ((p.z < 0 && Mathf.Abs(p.x) < 8) || Mathf.Abs(p.x - 58) < 8 || Mathf.Abs(p.z) < 7 || OnRoad(p)) continue;
                Tree(holder, p, 1f + (float)random.NextDouble() * .5f, false, random, false);
            }
            // Grass tufts and flowers all over the lawns, so the grass is not a flat green sheet.
            var tufts = VegetacaoInstaller.Load("Gramas"); var blooms = VegetacaoInstaller.Load("Flores");
            var lawn = new GameObject("GramaEspalhada").transform; lawn.SetParent(holder, false);
            int inside = 0, outside = 0;
            for (int i = 0; i < 4000 && (inside < 260 || outside < 320); i++)
            {
                bool inner = inside < 260;
                float a = (float)random.NextDouble() * Mathf.PI * 2;
                float d = inner ? 16 + (float)random.NextDouble() * 18 : wallRadius + 3 + (float)random.NextDouble() * 40;
                var p = new Vector3(Mathf.Sin(a) * d, 0, Mathf.Cos(a) * d);
                if (OnRoad(p) || !Free(p, inner ? 5.5f : 2f) || Mathf.Abs(d - wallRadius) < 2.5f || Mathf.Abs(p.x - 58) < 5) continue;
                bool flower = blooms.Length > 0 && random.NextDouble() < .14;
                Kit(lawn, flower ? blooms : tufts, p, flower ? 1.2f + (float)random.NextDouble() * .3f : .9f + (float)random.NextDouble() * .7f, random);
                if (inner) inside++; else outside++;
            }
        }

        static GameObject Soft(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = Shape(parent, type, name, position, scale, material);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static GameObject Kit(Transform parent, GameObject[] options, Vector3 p, float scale, System.Random random)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(options[random.Next(options.Length)], parent);
            go.transform.position = p;
            go.transform.rotation = Quaternion.Euler(0, (float)random.NextDouble() * 360, 0);
            go.transform.localScale = Vector3.one * scale;
            return go;
        }

        static void Tree(Transform parent, Vector3 p, float size, bool pink, System.Random random, bool solid)
        {
            var kitTrees = VegetacaoInstaller.Load("Arvores");
            if (kitTrees.Length > 0)
            {
                // Half pointy pines, half round trees (Arvore_3 and Arvore_4 have round crowns).
                var round = System.Array.FindAll(kitTrees, t => t.name == "Arvore_3" || t.name == "Arvore_4");
                var pointy = System.Array.FindAll(kitTrees, t => t.name != "Arvore_3" && t.name != "Arvore_4");
                var pool = round.Length > 0 && pointy.Length > 0 ? (random.NextDouble() < .5 ? round : pointy) : kitTrees;
                var planted = Kit(parent, pool, p, size * (.85f + (float)random.NextDouble() * .3f), random);
                if (!solid) foreach (var c in planted.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
                return;
            }
            var bark = Flat("Tronco", new Color(.45f, .31f, .20f));
            var leaves = pink
                ? new[] { Flat("FlorRosa", new Color(.96f, .66f, .80f)), Flat("FlorLilas", new Color(.84f, .64f, .95f)) }
                : new[] { Flat("FolhaClara", new Color(.52f, .76f, .36f)), Flat("FolhaMedia", new Color(.38f, .64f, .30f)), Flat("FolhaEscura", new Color(.28f, .52f, .27f)) };
            var tree = new GameObject(pink ? "ArvoreRosa" : "Arvore").transform; tree.SetParent(parent, false); tree.position = p;
            var trunk = Shape(tree, PrimitiveType.Cylinder, "Tronco", p + Vector3.up * 1.6f * size, new Vector3(.55f, 1.6f, .55f) * size, bark);
            if (!solid) Object.DestroyImmediate(trunk.GetComponent<Collider>());
            Soft(tree, PrimitiveType.Sphere, "Copa", p + new Vector3(0, 4.3f, 0) * size, new Vector3(3.6f, 3.1f, 3.6f) * size, leaves[random.Next(leaves.Length)]);
            for (int i = 0; i < 2; i++)
            {
                float a = (float)random.NextDouble() * Mathf.PI * 2;
                var off = new Vector3(Mathf.Cos(a) * 1.3f, 3.5f + (float)random.NextDouble() * .8f, Mathf.Sin(a) * 1.3f) * size;
                float s = (2.2f + (float)random.NextDouble() * .6f) * size;
                Soft(tree, PrimitiveType.Sphere, "Copa", p + off, new Vector3(s, s * .85f, s), leaves[random.Next(leaves.Length)]);
            }
        }

        static void Bush(Transform parent, Vector3 p, System.Random random)
        {
            var grass = VegetacaoInstaller.Load("Gramas"); var blooms = VegetacaoInstaller.Load("Flores");
            if (grass.Length > 0)
            {
                // A little clump of grass with a flower or two.
                int n = 3 + random.Next(3);
                for (int k = 0; k < n; k++)
                {
                    var spot = p + new Vector3((float)random.NextDouble() * 1.8f - .9f, 0, (float)random.NextDouble() * 1.8f - .9f);
                    Kit(parent, blooms.Length > 0 && random.NextDouble() < .35 ? blooms : grass, spot, 1.2f + (float)random.NextDouble() * .5f, random);
                }
                return;
            }
            var leaf = random.NextDouble() < .5 ? Flat("FolhaMedia", new Color(.38f, .64f, .30f)) : Flat("FolhaClara", new Color(.52f, .76f, .36f));
            float s = 1.1f + (float)random.NextDouble() * .7f;
            Soft(parent, PrimitiveType.Sphere, "Arbusto", p + Vector3.up * s * .3f, new Vector3(s * 1.2f, s * .8f, s), leaf);
        }

        static void Rock(Transform parent, Vector3 p, System.Random random)
        {
            var rocks = Prefab("Rochas");
            if (rocks != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(rocks, parent);
                go.transform.position = p;
                go.transform.rotation = Quaternion.Euler(0, (float)random.NextDouble() * 360, 0);
                go.transform.localScale = Vector3.one * (.7f + (float)random.NextDouble() * .7f);
                return;
            }
            float s = .9f + (float)random.NextDouble() * 1.2f;
            Soft(parent, PrimitiveType.Sphere, "Rocha", p + Vector3.up * s * .2f, new Vector3(s * 1.4f, s * .8f, s), Flat("Rocha", new Color(.70f, .66f, .58f)));
        }

        static void River(Transform parent)
        {
            var water = Flat("Agua", new Color(.30f, .68f, .80f), .9f);
            Soft(parent, PrimitiveType.Cube, "Rio", new Vector3(58, -.02f, 0), new Vector3(6, .1f, 400), water);
            var bank = Flat("Margem", new Color(.70f, .62f, .46f));
            Soft(parent, PrimitiveType.Cube, "MargemA", new Vector3(54.4f, -.03f, 0), new Vector3(1.4f, .1f, 400), bank);
            Soft(parent, PrimitiveType.Cube, "MargemB", new Vector3(61.6f, -.03f, 0), new Vector3(1.4f, .1f, 400), bank);
        }

        static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Art + "_Chao")) AssetDatabase.CreateFolder(Art.TrimEnd('/'), "_Chao");
        }

        static void Lighting()
        {
            var sun = new GameObject("Sol").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, .86f, .68f); sun.intensity = 1.5f;
            sun.shadows = LightShadows.Soft; sun.shadowStrength = .75f;
            sun.transform.rotation = Quaternion.Euler(40, -35, 0);
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.66f, .74f, .90f);
            RenderSettings.ambientEquatorColor = new Color(.74f, .66f, .54f);
            RenderSettings.ambientGroundColor = new Color(.40f, .34f, .25f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(.90f, .82f, .70f);
            RenderSettings.fogStartDistance = 90; RenderSettings.fogEndDistance = 320;

            EnsureFolder();
            string path = Art + "_Chao/CorDaCidade.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
                var tone = profile.Add<Tonemapping>(true); tone.mode.Override(TonemappingMode.Neutral);
                var bloom = profile.Add<Bloom>(true); bloom.threshold.Override(1f); bloom.intensity.Override(.4f); bloom.scatter.Override(.7f);
                var grade = profile.Add<ColorAdjustments>(true); grade.postExposure.Override(.15f); grade.contrast.Override(10); grade.saturation.Override(15);
                var vignette = profile.Add<Vignette>(true); vignette.intensity.Override(.18f);
                foreach (var c in profile.components) { c.name = c.GetType().Name; AssetDatabase.AddObjectToAsset(c, profile); }
                AssetDatabase.SaveAssets();
            }
            var volume = new GameObject("Cor").AddComponent<Volume>();
            volume.isGlobal = true; volume.sharedProfile = profile;
        }
    }
}
