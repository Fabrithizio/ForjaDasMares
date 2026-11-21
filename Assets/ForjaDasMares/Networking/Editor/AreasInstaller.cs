using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace ForjaDasMares.Networking.Editor
{
    // Creates one scene per place of Chapter 1 (except the Refúgio, which already exists): a sketch to decorate later.
    // Each scene: ground 128 x 128 m with soft hills, cliffs around the edges, flat roads from the centre to every
    // passage (on the right side of the compass), the place name in the middle and its own navigation.
    // The gates themselves are built when the game runs (PortoesDoMundo), from the same table (AreasDoMundo).
    public static class AreasInstaller
    {
        const string Template = "Assets/ForjaDasMares/Scenes/CostaDasRuinas.unity";
        const string Folder = "Assets/ForjaDasMares/Scenes/Areas/";
        const string Data = "Assets/ForjaDasMares/Data/World/Areas/";
        const float Half = 64, Ground = 2, Height = 18;

        public static IEnumerable<string> ScenePaths =>
            AreasDoMundo.Todas.Where(a => a.id != "refugio").Select(a => Folder + a.cena + ".unity");

        [MenuItem("Forja/Mundo/Criar as áreas do Capítulo 1 (esboço)")]
        public static void CriarTodas()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Folder); Directory.CreateDirectory(Data); AssetDatabase.Refresh();
            int feitas = 0;
            foreach (var area in AreasDoMundo.Todas)
            {
                if (area.id == "refugio") continue;
                Criar(area); feitas++;
            }
            var scenes = EditorBuildSettings.scenes.ToList();
            foreach (var path in ScenePaths) if (!scenes.Any(s => s.path == path)) scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ForjaBuild.Cidade);
            Debug.Log("AREAS_PRONTAS " + feitas);
        }

        // The Nascente do Rio is south of the Refúgio on the world map: the river and its bridge move to the south of the
        // city, close to the south gate, winding instead of straight. The south exit is crossing that bridge.
        // Edits the city scene in place (nothing else changes). Can be run again.
        public const float RioZ = -47;
        // Winding, but straight across right below the gate (x = 0), so the bridge lines up with the gate.
        public static float RioEm(float x) => RioZ - 4f * (1 - Mathf.Cos(x * .035f)) + 3f * Mathf.Pow(Mathf.Sin(x * .011f), 2);
        [MenuItem("Forja/Mundo/Refúgio: rio e ponte ao sul")]
        public static void RioAoSul()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(ForjaBuild.Cidade);
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToList();
            Transform Find(string name) => all.FirstOrDefault(t => t != null && t.name == name); // some get destroyed below
            var rio = Find("Rio"); var a = Find("MargemA"); var b = Find("MargemB");
            if (rio == null) { Debug.LogError("Não achei o rio (objeto 'Rio') na cidade."); return; }
            Directory.CreateDirectory("Assets/ForjaDasMares/Data/World/Town/");
            // Just above the grass (the ground plane is at 0), so it shows.
            Faixa(rio, "RioCurvo", 0, 6.4f, .03f);
            if (a != null) Faixa(a, "MargemCurvaA", 3.9f, 1.6f, .025f);
            if (b != null) Faixa(b, "MargemCurvaB", -3.9f, 1.6f, .025f);
            // A darker middle line: the deep part of the water (cheap way to look deep).
            var fundo = Find("RioFundo");
            if (fundo == null) { fundo = new GameObject("RioFundo", typeof(MeshFilter), typeof(MeshRenderer)).transform; fundo.SetParent(rio.parent, false); }
            Faixa(fundo, "RioFundoCurvo", 0, 3.2f, .04f);
            var deep = AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Data/World/Town/AguaFunda.mat");
            if (deep == null) { deep = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(deep, "Assets/ForjaDasMares/Data/World/Town/AguaFunda.mat"); }
            deep.color = new Color(.10f, .33f, .50f); deep.SetFloat("_Smoothness", .92f); EditorUtility.SetDirty(deep);
            fundo.GetComponent<MeshRenderer>().sharedMaterial = deep;
            // Flowing water: a soft ripple texture that moves along the river (RioAnimado), on both strips.
            var ripples = Ondas();
            var water = AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Data/World/Town/AguaRio.mat");
            if (water == null) { water = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(water, "Assets/ForjaDasMares/Data/World/Town/AguaRio.mat"); }
            water.SetTexture("_BaseMap", ripples); water.color = new Color(.42f, .74f, .86f); water.SetFloat("_Smoothness", .9f); EditorUtility.SetDirty(water);
            deep.SetTexture("_BaseMap", ripples); EditorUtility.SetDirty(deep);
            rio.GetComponent<MeshRenderer>().sharedMaterial = water;
            foreach (var strip in new[] { rio, fundo })
            {
                var flow = strip.GetComponent<RioAnimado>(); if (flow == null) flow = strip.gameObject.AddComponent<RioAnimado>();
                flow.velocidade = strip == rio ? new Vector2(.10f, .01f) : new Vector2(.16f, -.01f);
            }
            fundo.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            // Bridge across the river, right below the south gate.
            float z0 = RioEm(0);
            var normal = Vector3.forward; // straight in line with the south gate
            var ponte = all.FirstOrDefault(t => t != null && t.name.StartsWith("Ponte") && t.parent != null && t.parent.name == "CidadeNova") ?? all.FirstOrDefault(t => t != null && t.name.StartsWith("Ponte"));
            if (ponte != null) { ponte.position = new Vector3(0, 0, z0); ponte.rotation = Quaternion.Euler(0, Mathf.Atan2(-normal.z, normal.x) * Mathf.Rad2Deg, 0); }
            // An invisible ramp that follows the deck of the bridge, so it can be walked over (the steps were too high).
            var oldRamp = Find("RampaDaPonte"); if (oldRamp != null) UnityEngine.Object.DestroyImmediate(oldRamp.gameObject);
            if (ponte != null)
            {
                Rampa(ponte, normal, z0, Find("Grama"));
                // The bridge's own colliders (steps, railings) stop blocking: the ramp is the floor now.
                foreach (var c in ponte.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            }
            // The old exit to the coast (its trigger box blocked the road to the bridge, and its sign).
            int oldExit = 0;
            foreach (var passage in UnityEngine.Object.FindObjectsByType<TownRoadPassage>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { var go = passage.gameObject; UnityEngine.Object.DestroyImmediate(passage); foreach (var c in go.GetComponents<Collider>()) UnityEngine.Object.DestroyImmediate(c); oldExit++; }
            foreach (var cue in UnityEngine.Object.FindObjectsByType<TownExitCue>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (cue != null) { UnityEngine.Object.DestroyImmediate(cue.gameObject); oldExit++; }
            foreach (var text in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (text != null && text.text != null && text.text.ToUpperInvariant().Contains("COSTA")) { UnityEngine.Object.DestroyImmediate(text.gameObject); oldExit++; }
            // The old exit marker in front of the south gate (yellow post and its arrow).
            foreach (var t in all.Where(t => t != null && (t.name == "Seta_saida_Costa" || (t.name == "Marco" && Mathf.Abs(t.position.x) < 4 && t.position.z < -26 && t.position.z > -44))).ToList())
            { UnityEngine.Object.DestroyImmediate(t.gameObject); oldExit++; }
            // The invisible walls that closed the south road and the west and east gates (the city had only one way out).
            foreach (var t in all.Where(t => t != null && t.name == "Bloqueio").ToList())
            {
                var p = t.position;
                bool south = Mathf.Abs(p.z + 42) < .6f && Mathf.Abs(p.x) < 1;
                bool sides = Mathf.Abs(Mathf.Abs(p.x) - 37.5f) < .6f && Mathf.Abs(p.z) < 1;
                if (south || sides) { UnityEngine.Object.DestroyImmediate(t.gameObject); oldExit++; }
            }
            // Plants standing in the river go away.
            int removed = 0;
            var nature = Find("Natureza");
            if (nature != null)
                foreach (var t in nature.GetComponentsInChildren<Transform>(true).Where(t => t != nature && t.parent != null && (t.parent == nature || t.parent.name == "GramaEspalhada")).ToList())
                    if (t != null && Mathf.Abs(t.position.z - RioEm(t.position.x)) < 6.5f) { UnityEngine.Object.DestroyImmediate(t.gameObject); removed++; }
            // The water cannot be walked across: invisible banks along the curve, except on the bridge.
            var old = Find("RioBloqueio"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var holder = new GameObject("RioBloqueio");
            for (float x = -300; x < 300; x += 4)
            {
                float mid = x + 2;
                if (Mathf.Abs(mid) < 5.5f) continue; // the bridge
                var p = new Vector3(mid, 1, RioEm(mid));
                var dir = new Vector3(4, 0, RioEm(x + 4) - RioEm(x));
                var block = GameObject.CreatePrimitive(PrimitiveType.Cube); block.name = "Agua"; block.transform.SetParent(holder.transform, false);
                block.transform.position = p; block.transform.rotation = Quaternion.LookRotation(Vector3.Cross(dir, Vector3.up)); block.transform.localScale = new Vector3(dir.magnitude + .6f, 2, 6); block.layer = 9;
                UnityEngine.Object.DestroyImmediate(block.GetComponent<MeshRenderer>());
                var volume = block.AddComponent<NavMeshModifierVolume>(); volume.size = new Vector3(1, 3, 1); volume.center = Vector3.zero; volume.area = 1;
            }
            // Navigation again, big enough to reach past the river and the side gates.
            var nav = UnityEngine.Object.FindFirstObjectByType<NavMeshSurface>();
            if (nav != null)
            {
                if (nav.collectObjects == CollectObjects.Volume) { nav.center = new Vector3(0, 0, -6); nav.size = new Vector3(140, 30, 140); }
                nav.RemoveData(); nav.navMeshData = null; nav.BuildNavMesh();
                const string path = "Assets/ForjaDasMares/Data/World/Town/NavigationNova.asset";
                var prior = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
                if (prior == null) AssetDatabase.CreateAsset(nav.navMeshData, path); else { EditorUtility.CopySerialized(nav.navMeshData, prior); nav.navMeshData = prior; }
                EditorUtility.SetDirty(nav);
            }
            bool ok = NavMesh.SamplePosition(new Vector3(0, 0, z0 - 6), out _, 2, NavMesh.AllAreas) && NavMesh.SamplePosition(new Vector3(-46, 0, 0), out _, 2, NavMesh.AllAreas) && NavMesh.SamplePosition(new Vector3(46, 0, 0), out _, 2, NavMesh.AllAreas);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("RIO_AO_SUL ponte=" + (ponte != null) + " saida_antiga_removida=" + oldExit + " plantas_removidas=" + removed + " navegacao=" + (nav != null ? nav.collectObjects.ToString() : "nenhuma") + " saidas_andaveis=" + ok);
        }

        static void Rampa(Transform ponte, Vector3 along, float z0, Transform ground)
        {
            foreach (var c in ponte.GetComponentsInChildren<Collider>(true)) c.enabled = true; // measured again on every run
            Physics.SyncTransforms();
            var renderers = ponte.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            var bounds = renderers[0].bounds; foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            float half = 0;
            for (int i = 0; i < 8; i++)
            {
                var c = new Vector3((i & 1) == 0 ? bounds.min.x : bounds.max.x, 0, (i & 2) == 0 ? bounds.min.z : bounds.max.z);
                half = Mathf.Max(half, Mathf.Abs(Vector3.Dot(c - new Vector3(0, 0, z0), along)));
            }
            half += 1.5f;
            // Height of the deck along the middle of the bridge (only the bridge's own colliders count).
            const float step = .5f; int n = Mathf.CeilToInt(half * 2 / step) + 1;
            var h = new float[n];
            for (int i = 0; i < n; i++)
            {
                var p = new Vector3(0, 0, z0) + along * (-half + i * step);
                h[i] = 0;
                foreach (var hit in Physics.RaycastAll(p + Vector3.up * 30, Vector3.down, 60, ~0, QueryTriggerInteraction.Ignore))
                    if (hit.collider.transform.IsChildOf(ponte)) h[i] = Mathf.Max(h[i], hit.point.y);
                if (h[i] == 0 && bounds.Contains(new Vector3(p.x, bounds.center.y, p.z))) h[i] = 0; // no collider: ground level
            }
            // Smooth it into a ramp no steeper than about 25 degrees.
            var s = new float[n];
            for (int i = 0; i < n; i++) { float sum = 0; int k = 0; for (int j = Mathf.Max(0, i - 3); j <= Mathf.Min(n - 1, i + 3); j++) { sum += h[j]; k++; } s[i] = sum / k; }
            s[0] = s[n - 1] = 0;
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 1; i < n; i++) s[i] = Mathf.Min(s[i], s[i - 1] + step * .45f);
                for (int i = n - 2; i >= 0; i--) s[i] = Mathf.Min(s[i], s[i + 1] + step * .45f);
            }
            var side = Vector3.Cross(Vector3.up, along).normalized;
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int i = 0; i < n; i++)
            {
                var p = new Vector3(0, s[i] + .02f, z0) + along * (-half + i * step);
                vertices.Add(p - side * 1.4f); vertices.Add(p + side * 1.4f);
                if (i > 0) { int v = i * 2; triangles.AddRange(new[] { v - 2, v, v - 1, v - 1, v, v + 1 }); triangles.AddRange(new[] { v - 2, v - 1, v, v - 1, v + 1, v }); }
            }
            var mesh = new Mesh { name = "RampaDaPonte" }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            string path = "Assets/ForjaDasMares/Data/World/Town/RampaDaPonte.asset";
            var prior = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (prior == null) AssetDatabase.CreateAsset(mesh, path); else { EditorUtility.CopySerialized(mesh, prior); mesh = prior; }
            var ramp = new GameObject("RampaDaPonte");
            ramp.layer = ground != null ? ground.gameObject.layer : 8;
            ramp.AddComponent<MeshCollider>().sharedMesh = mesh;
            Debug.Log("RAMPA_DA_PONTE comprimento=" + (half * 2).ToString("0.0") + " altura=" + s.Max().ToString("0.00"));
        }

        // Prints what stands on the road between the south gate and the bridge, and where the walkable ground stops.
        [MenuItem("Forja/Mundo/Diagnóstico da saída sul")]
        public static void Diagnostico()
        {
            Physics.SyncTransforms();
            var log = new System.Text.StringBuilder("DIAGNOSTICO_SUL\n");
            var nav = UnityEngine.Object.FindFirstObjectByType<NavMeshSurface>();
            if (nav != null) log.AppendLine("nav: collect=" + nav.collectObjects + " layers=" + nav.layerMask.value + " center=" + nav.center + " size=" + nav.size + " agent=" + nav.agentTypeID);
            var linha = new System.Text.StringBuilder("chao: ");
            for (float z = -30; z >= -62; z -= 2)
            {
                bool on = NavMesh.SamplePosition(new Vector3(0, 1.5f, z), out var hit, 2f, NavMesh.AllAreas) && Mathf.Abs(hit.position.x) < 1.5f && Mathf.Abs(hit.position.z - z) < 1.5f;
                linha.Append(z + (on ? "=" + hit.position.y.ToString("0.0") : "=X") + "  ");
            }
            log.AppendLine(linha.ToString());
            if (NavMesh.SamplePosition(new Vector3(0, 0, -30), out var a, 2, NavMesh.AllAreas) && NavMesh.SamplePosition(new Vector3(0, 0, -60), out var b, 3, NavMesh.AllAreas))
            {
                var path = new NavMeshPath(); NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path);
                log.AppendLine("caminho portao->depois do rio: " + path.status + " para em " + (path.corners.Length > 0 ? path.corners[path.corners.Length - 1].ToString() : "-"));
            }
            else log.AppendLine("sem chao no portao ou depois do rio");
            foreach (var c in Physics.OverlapBox(new Vector3(0, 1.5f, -44), new Vector3(4, 3, 10), Quaternion.identity, ~0, QueryTriggerInteraction.Collide))
                log.AppendLine("colisor: " + Caminho(c.transform) + " layer=" + LayerMask.LayerToName(c.gameObject.layer) + "(" + c.gameObject.layer + ") trigger=" + c.isTrigger + " ativo=" + c.enabled);
            Debug.Log(log.ToString());
        }
        static string Caminho(Transform t) { var s = t.name; while (t.parent != null) { t = t.parent; s = t.name + "/" + s; } return s; }

        // Light streaks on blue, tileable: the ripples of the river.
        static Texture2D Ondas()
        {
            const string path = "Assets/ForjaDasMares/Data/World/Town/AguaRio.png";
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    float w = Mathf.Sin(2 * Mathf.PI * (u * 3 + .35f * Mathf.Sin(2 * Mathf.PI * v * 2)));
                    float w2 = Mathf.Sin(2 * Mathf.PI * (u * 5 - v * 1 + .2f * Mathf.Sin(2 * Mathf.PI * (u + v) * 3)));
                    float light = Mathf.Clamp01(Mathf.Pow(Mathf.Max(0, w), 6) * .55f + Mathf.Pow(Mathf.Max(0, w2), 10) * .45f);
                    var c = Color.Lerp(new Color(.80f, .88f, .92f), Color.white, light);
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.wrapMode = TextureWrapMode.Repeat; importer.mipmapEnabled = true; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // A winding strip that follows the river line (offset to the side, width), saved as a mesh asset.
        static void Faixa(Transform target, string name, float offset, float width, float y)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>(); var uvs = new List<Vector2>();
            int i = 0;
            for (float x = -300; x <= 300; x += 2, i++)
            {
                float z = RioEm(x) + offset;
                vertices.Add(new Vector3(x, y, z - width * .5f)); vertices.Add(new Vector3(x, y, z + width * .5f));
                uvs.Add(new Vector2(x / 6f, 0)); uvs.Add(new Vector2(x / 6f, 1));
                if (i > 0) { int v = i * 2; triangles.AddRange(new[] { v - 2, v - 1, v, v - 1, v + 1, v }); }
            }
            var mesh = new Mesh { name = name }; mesh.SetVertices(vertices); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            string path = "Assets/ForjaDasMares/Data/World/Town/" + name + ".asset";
            var prior = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (prior == null) AssetDatabase.CreateAsset(mesh, path); else { EditorUtility.CopySerialized(mesh, prior); mesh = prior; }
            target.position = Vector3.zero; target.rotation = Quaternion.identity; target.localScale = Vector3.one;
            var filter = target.GetComponent<MeshFilter>(); if (filter != null) filter.sharedMesh = mesh;
        }

        static void Criar(AreasDoMundo.Area area)
        {
            var scene = EditorSceneManager.OpenScene(Template);
            foreach (var go in scene.GetRootGameObjects())
                if (go.name == "CoastEnvironment" || go.name == "RegionalNavigation") UnityEngine.Object.DestroyImmediate(go);
            var world = UnityEngine.Object.FindFirstObjectByType<RegionalWorld>();
            var hero = world.Hero; var pool = world.Enemies; var follow = UnityEngine.Object.FindFirstObjectByType<ArenaCamera>();
            string nome = AreasDoMundo.Nome(area.id);

            // Region data: no camps yet, the start in the middle.
            string regionPath = Data + area.id + ".asset";
            var region = AssetDatabase.LoadAssetAtPath<RegionDefinition>(regionPath);
            if (region == null) { region = ScriptableObject.CreateInstance<RegionDefinition>(); AssetDatabase.CreateAsset(region, regionPath); }
            region.regionId = area.id; region.displayName = nome;
            region.refuge = new Vector3(0, .1f, 0); region.portal = new Vector3(0, -500, 0);
            region.route = new[] { region.refuge }; region.camps = new RegionCampDefinition[0];
            region.connections = new RegionConnection[0]; region.landmarks = new RegionLandmark[0];
            region.bounds = new Bounds(Vector3.zero, new Vector3(Half * 2 + 4, 40, Half * 2 + 4));
            EditorUtility.SetDirty(region);
            world.Configure(region, hero, pool, follow, AssetDatabase.LoadAssetAtPath<SoundscapeLibrary>("Assets/ForjaDasMares/Resources/CampaignSoundscape.asset"));
            EditorUtility.SetDirty(world);
            hero.transform.position = region.refuge;

            var environment = new GameObject("AreaEnvironment");
            var roads = area.saidas.Select(s => AreasDoMundo.Direcao(s.lado) * 60f / Mathf.Max(Mathf.Abs(AreasDoMundo.Direcao(s.lado).x), Mathf.Abs(AreasDoMundo.Direcao(s.lado).z))).ToArray();
            Terreno(environment.transform, area.id, roads);

            // Invisible walls just outside the ground (the gates travel before anyone gets there).
            foreach (var (pos, size) in new[] {
                (new Vector3(0, 3, Half + 1), new Vector3(Half * 2 + 4, 10, 1)), (new Vector3(0, 3, -Half - 1), new Vector3(Half * 2 + 4, 10, 1)),
                (new Vector3(Half + 1, 3, 0), new Vector3(1, 10, Half * 2 + 4)), (new Vector3(-Half - 1, 3, 0), new Vector3(1, 10, Half * 2 + 4)) })
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "Limite"; wall.transform.SetParent(environment.transform, false);
                wall.transform.position = pos; wall.transform.localScale = size; wall.layer = 9;
                UnityEngine.Object.DestroyImmediate(wall.GetComponent<MeshRenderer>());
            }

            // Name of the place in the middle (temporary, until the place is decorated).
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube); post.name = "PlacaDoLugar"; post.transform.SetParent(environment.transform, false);
            post.transform.position = new Vector3(3, .9f, 3); post.transform.localScale = new Vector3(.2f, 1.8f, .2f); post.layer = 9;
            post.GetComponent<Renderer>().sharedMaterial = Material("Madeira", new Color(.32f, .22f, .13f));
            var label = new GameObject("NomeDoLugar").AddComponent<TextMesh>(); label.transform.SetParent(environment.transform, false);
            label.transform.position = new Vector3(3, 2.4f, 3); label.transform.rotation = Quaternion.Euler(40, 0, 0);
            label.text = nome.ToUpperInvariant() + "\n(esboço)"; label.fontSize = 44; label.characterSize = .09f; label.anchor = TextAnchor.LowerCenter; label.alignment = TextAlignment.Center;
            label.color = new Color(.95f, .88f, .66f);

            // Navigation.
            var nav = new GameObject("AreaNavigation").AddComponent<NavMeshSurface>();
            nav.layerMask = (1 << 8) | (1 << 9); nav.useGeometry = NavMeshCollectGeometry.PhysicsColliders; nav.collectObjects = CollectObjects.All;
            nav.BuildNavMesh();
            string navPath = Data + area.id + "_Navegacao.asset";
            var prior = AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
            if (prior == null) AssetDatabase.CreateAsset(nav.navMeshData, navPath); else { EditorUtility.CopySerialized(nav.navMeshData, prior); nav.navMeshData = prior; }
            if (!NavMesh.SamplePosition(region.refuge, out var start, 3, NavMesh.AllAreas)) throw new InvalidOperationException("Sem chão no centro de " + area.id);
            foreach (var road in roads)
            {
                var path = new NavMeshPath();
                if (!NavMesh.SamplePosition(road, out var end, 4, NavMesh.AllAreas) || !NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete)
                    throw new InvalidOperationException("Estrada sem saída em " + area.id + " para " + road);
            }

            // Light by kind of place.
            var sun = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(x => x.type == LightType.Directional);
            if (sun != null) { sun.intensity = 1.25f; sun.color = new Color(1, .93f, .8f); sun.transform.rotation = Quaternion.Euler(50, -30, 0); }
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 60; RenderSettings.fogEndDistance = 170;
            RenderSettings.fogColor = new Color(.42f, .58f, .55f); RenderSettings.ambientLight = new Color(.4f, .47f, .45f);

            EditorSceneManager.SaveScene(scene, Folder + area.cena + ".unity");
            Debug.Log("AREA_CRIADA " + area.id + " saidas=" + area.saidas.Length);
        }

        static float Estrada(Vector3 p, Vector3[] roads)
        {
            float best = p.magnitude - 9; // plaza in the middle
            foreach (var r in roads)
            {
                float t = Mathf.Clamp01(Vector3.Dot(p, r) / r.sqrMagnitude);
                best = Mathf.Min(best, Vector3.Distance(p, r * t));
            }
            return Mathf.Max(0, best);
        }

        static void Terreno(Transform parent, string id, Vector3[] roads)
        {
            string path = Data + id + "_Terreno.asset";
            var data = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
            if (data == null) { data = new TerrainData(); AssetDatabase.CreateAsset(data, path); }
            const int res = 129;
            data.heightmapResolution = res; data.alphamapResolution = 128; data.size = new Vector3(Half * 2, Height, Half * 2);
            data.terrainLayers = new[] { "Grass", "Soil", "Rock" }.Select(n => AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/ForjaDasMares/Data/World/" + n + ".terrainlayer")).Where(l => l != null).ToArray();
            float seed = Math.Abs(id.GetHashCode() % 1000);
            var heights = new float[res, res];
            for (int z = 0; z < res; z++)
                for (int x = 0; x < res; x++)
                {
                    var p = new Vector3(-Half + x * Half * 2 / (res - 1), 0, -Half + z * Half * 2 / (res - 1));
                    float road = Estrada(p, roads);
                    float open = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(4, 14, road));
                    float hills = Mathf.PerlinNoise((p.x + seed) * .045f, (p.z + seed * 2) * .045f) * 3.2f * open;
                    float edge = Half - Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.z));
                    float cliff = edge < 12 ? Mathf.Pow(1 - edge / 12, 2) * 13 * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(4, 9, road)) : 0;
                    heights[z, x] = (Ground + hills + cliff) / Height;
                }
            data.SetHeights(0, 0, heights);
            int layers = data.terrainLayers.Length;
            if (layers > 0)
            {
                var alpha = new float[128, 128, layers];
                for (int z = 0; z < 128; z++)
                    for (int x = 0; x < 128; x++)
                    {
                        var p = new Vector3(-Half + x * Half * 2 / 127f, 0, -Half + z * Half * 2 / 127f);
                        float road = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(2.5f, 5.5f, Estrada(p, roads)));
                        float edge = Half - Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.z));
                        float rock = layers > 2 ? Mathf.Clamp01(1 - edge / 9) * (1 - road) : 0;
                        alpha[z, x, 0] = Mathf.Max(0, 1 - road - rock);
                        if (layers > 1) alpha[z, x, 1] = road;
                        if (layers > 2) alpha[z, x, 2] = rock;
                    }
                data.SetAlphamaps(0, 0, alpha);
            }
            EditorUtility.SetDirty(data);
            var go = Terrain.CreateTerrainGameObject(data);
            go.name = "Terreno"; go.transform.SetParent(parent); go.transform.position = new Vector3(-Half, -Ground, -Half); go.layer = 8;
            var terrain = go.GetComponent<Terrain>(); terrain.heightmapPixelError = 8; terrain.drawInstanced = true;
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Data/World/Terrain.mat");
            if (mat != null) terrain.materialTemplate = mat;
        }

        static Material Material(string name, Color color)
        {
            string path = Data + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
            m.color = color; EditorUtility.SetDirty(m); return m;
        }
    }
}
