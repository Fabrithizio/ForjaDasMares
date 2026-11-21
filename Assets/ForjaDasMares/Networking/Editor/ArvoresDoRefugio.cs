using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace ForjaDasMares.Networking.Editor
{
    // Refúgio: trees only on the grass. Moves the trees that stand on stone (squares, streets, paths) or in front of
    // the Raiz-Mãe station to the nearest free grass, and plants more trees between the houses so the city feels alive.
    public static class ArvoresDoRefugio
    {
        const string ScenePath = "Assets/ForjaDasMares/Scenes/RefugioUltimoSino.unity";
        const float TownRadius = 31f, StoneMargin = 1.8f, TreeGap = 4.5f, BuildingGap = 1.6f;
        const int NewTrees = 22;

        struct Stone { public Transform t; public Vector3[] v; public int[] tri; }
        static List<Stone> stones; static List<Bounds> blocks; static List<Vector3> trees;

        [MenuItem("Forja/Mundo/Refúgio: árvores na grama (e mais árvores)")]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                scene = EditorSceneManager.OpenScene(ScenePath);
            }
            var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            // Trees of the kit inside the walls (the forest outside stays as it is).
            var inside = all.Where(t => t.name.StartsWith("Arvore") && PrefabUtility.IsOutermostPrefabInstanceRoot(t.gameObject) && Flat(t.position).magnitude < TownRadius + 2).ToList();
            CollectStones(all);
            CollectBlocks(all, inside);
            trees = inside.Select(t => t.position).ToList();

            // In front of the Raiz-Mãe: the station's own footprint, a bit bigger.
            var station = all.FirstOrDefault(t => t.name == "EstacaoAntiga");
            Bounds stationArea = station != null ? Footprint(station) : new Bounds(new Vector3(-13.5f, 0, 23), Vector3.zero);
            stationArea.Expand(new Vector3(9, 0, 9));

            int moved = 0, removed = 0;
            for (int i = 0; i < inside.Count; i++)
            {
                var tree = inside[i]; var p = tree.position;
                bool badStone = OnStone(p, StoneMargin * .6f);
                bool badStation = stationArea.Contains(new Vector3(p.x, stationArea.center.y, p.z));
                if (!badStone && !badStation) continue;
                trees.RemoveAt(trees.IndexOf(p));
                if (Find(p, 3, 16, stationArea, out var spot))
                {
                    Undo.RecordObject(tree, "Árvore na grama");
                    tree.position = new Vector3(spot.x, p.y, spot.z);
                    trees.Add(tree.position); moved++;
                }
                else { Undo.DestroyObjectImmediate(tree.gameObject); removed++; }
            }

            // More trees between the houses.
            int added = 0;
            var models = inside.Where(t => t != null).Select(t => PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject)).Where(g => g != null).Distinct().ToArray();
            var scales = inside.Where(t => t != null).Select(t => t.localScale.x).DefaultIfEmpty(1).ToArray();
            var parent = inside.FirstOrDefault(t => t != null)?.parent;
            var random = new System.Random(2026);
            for (int i = 0; i < 3000 && added < NewTrees && models.Length > 0; i++)
            {
                float a = (float)random.NextDouble() * Mathf.PI * 2, d = 9 + (float)random.NextDouble() * (TownRadius - 9);
                var p = new Vector3(Mathf.Sin(a) * d, 0, Mathf.Cos(a) * d);
                if (!Good(p, stationArea)) continue;
                var go = (GameObject)PrefabUtility.InstantiatePrefab(models[random.Next(models.Length)], parent);
                go.transform.position = p;
                go.transform.rotation = Quaternion.Euler(0, (float)random.NextDouble() * 360, 0);
                go.transform.localScale = Vector3.one * scales[random.Next(scales.Length)] * (.9f + (float)random.NextDouble() * .2f);
                Undo.RegisterCreatedObjectUndo(go, "Árvore nova");
                trees.Add(p); added++;
            }

            // The trees block the way: navigation again.
            var nav = Object.FindFirstObjectByType<NavMeshSurface>();
            if (nav != null)
            {
                nav.RemoveData(); nav.navMeshData = null; nav.BuildNavMesh();
                const string path = "Assets/ForjaDasMares/Data/World/Town/NavigationNova.asset";
                var prior = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
                if (prior == null) AssetDatabase.CreateAsset(nav.navMeshData, path); else { EditorUtility.CopySerialized(nav.navMeshData, prior); nav.navMeshData = prior; }
                EditorUtility.SetDirty(nav);
            }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("ARVORES_DO_REFUGIO movidas=" + moved + " removidas=" + removed + " novas=" + added + " pedras=" + stones.Count + " obstaculos=" + blocks.Count + " estacao=" + (station != null));
        }

        static Vector2 Flat(Vector3 p) => new Vector2(p.x, p.z);

        // Flat stone pieces of the ground (squares, streets, ring, little paths, bridge).
        static readonly string[] StoneNames = { "Praca", "Patio", "Rua", "Anel", "Entrada", "Caminho", "Ponte", "Rampa", "Calcada", "Margem" };
        static void CollectStones(Transform[] all)
        {
            stones = new List<Stone>();
            foreach (var t in all)
            {
                var filter = t.GetComponent<MeshFilter>(); var r = t.GetComponent<MeshRenderer>();
                if (filter == null || r == null || filter.sharedMesh == null) continue;
                bool named = StoneNames.Any(n => t.name.StartsWith(n));
                var tex = r.sharedMaterial != null && r.sharedMaterial.HasProperty("_BaseMap") ? r.sharedMaterial.GetTexture("_BaseMap") : null;
                bool stony = tex != null && tex.name.Contains("Pedra");
                if (!named && !stony) continue;
                if (r.bounds.size.y > .8f || r.bounds.max.y > 1f) continue; // only flat ground, not walls
                stones.Add(new Stone { t = t, v = filter.sharedMesh.vertices, tri = filter.sharedMesh.triangles });
            }
        }

        // Houses, stalls, NPCs, fountain, wall...: their footprint on the ground.
        static void CollectBlocks(Transform[] all, List<Transform> insideTrees)
        {
            blocks = new List<Bounds>();
            foreach (var t in all)
            {
                bool root = PrefabUtility.IsOutermostPrefabInstanceRoot(t.gameObject);
                if (!root && PrefabUtility.IsPartOfPrefabInstance(t.gameObject)) continue; // inside a prefab: the root counts
                if (t.name.StartsWith("Arvore") || t.name.StartsWith("Grama") || t.name.StartsWith("Flor") || t.name == "Colina") continue;
                Bounds f;
                if (root) f = Footprint(t);
                else { var r = t.GetComponent<Renderer>(); if (r == null) continue; f = r.bounds; }
                if (f.size.y < 1f || f.size.x > 50 || f.size.z > 50) continue;
                blocks.Add(f);
            }
        }

        static Bounds Footprint(Transform t)
        {
            var rs = t.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(t.position, Vector3.zero);
            var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        static bool Good(Vector3 p, Bounds station)
        {
            if (Flat(p).magnitude > TownRadius) return false;
            if (station.Contains(new Vector3(p.x, station.center.y, p.z))) return false;
            foreach (var t in trees) if ((Flat(t) - Flat(p)).sqrMagnitude < TreeGap * TreeGap) return false;
            foreach (var b in blocks)
            {
                if (p.x > b.min.x - BuildingGap && p.x < b.max.x + BuildingGap && p.z > b.min.z - BuildingGap && p.z < b.max.z + BuildingGap) return false;
            }
            return !OnStone(p, StoneMargin);
        }

        // Nearest good grass around a point.
        static bool Find(Vector3 from, float min, float max, Bounds station, out Vector3 spot)
        {
            for (float d = min; d <= max; d += 1f)
                for (int k = 0; k < 24; k++)
                {
                    float a = k / 24f * Mathf.PI * 2;
                    var p = new Vector3(from.x + Mathf.Sin(a) * d, 0, from.z + Mathf.Cos(a) * d);
                    if (Good(p, station)) { spot = p; return true; }
                }
            spot = from; return false;
        }

        // On stone: the point or any point around it (margin) falls on a stone piece seen from above.
        static bool OnStone(Vector3 p, float margin)
        {
            if (Hit(p)) return true;
            for (int k = 0; k < 8; k++)
            {
                float a = k / 8f * Mathf.PI * 2;
                if (Hit(p + new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * margin)) return true;
            }
            return false;
        }

        static bool Hit(Vector3 p)
        {
            foreach (var s in stones)
            {
                if (s.t == null) continue;
                var local = s.t.InverseTransformPoint(new Vector3(p.x, s.t.position.y, p.z));
                var q = new Vector2(local.x, local.z);
                for (int i = 0; i + 2 < s.tri.Length; i += 3)
                {
                    Vector3 a = s.v[s.tri[i]], b = s.v[s.tri[i + 1]], c = s.v[s.tri[i + 2]];
                    if (Inside(q, new Vector2(a.x, a.z), new Vector2(b.x, b.z), new Vector2(c.x, c.z))) return true;
                }
            }
            return false;
        }

        static bool Inside(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Cross(p, a, b), d2 = Cross(p, b, c), d3 = Cross(p, c, a);
            float area = Mathf.Abs(Cross(a, b, c));
            if (area < 1e-6f) return false;
            bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }
        static float Cross(Vector2 p, Vector2 a, Vector2 b) => (a.x - p.x) * (b.y - p.y) - (b.x - p.x) * (a.y - p.y);
    }
}
