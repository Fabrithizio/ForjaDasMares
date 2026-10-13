using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.UI;
namespace ForjaDasMares.Editor
{
    public static class PveInstaller
    {
        const string Root = "Assets/ForjaDasMares/";
        [MenuItem("Forja das Mares/Install PvE in existing arena")]
        public static void Apply()
        {
            if (!File.Exists(ArenaBuilder.ScenePath)) throw new InvalidOperationException("Existing arena is required.");
            var scene = EditorSceneManager.OpenScene(ArenaBuilder.ScenePath);
            if (UnityEngine.Object.FindFirstObjectByType<EncounterController>() != null) { Debug.Log("PVE_ALREADY_INSTALLED"); return; }
            foreach (string folder in new[] { "Data/Enemies", "Data/Encounters", "Prefabs/Enemies", "Navigation" }) Directory.CreateDirectory(Root + folder);
            AssetDatabase.Refresh();
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]); var layers = tags.FindProperty("layers"); layers.GetArrayElementAtIndex(12).stringValue = "TrainingAlly"; layers.GetArrayElementAtIndex(13).stringValue = "ArenaEnemy"; tags.ApplyModifiedProperties();
            int bossType = NavMesh.CreateSettings().agentTypeID;
            var navSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset")[0]);
            var entries = navSettings.FindProperty("m_Settings");
            for (int i = 0; i < entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("agentTypeID").intValue != bossType) continue;
                entry.FindPropertyRelative("agentRadius").floatValue = 0.8f; entry.FindPropertyRelative("agentHeight").floatValue = 2.7f;
            }
            navSettings.ApplyModifiedProperties();
            // Surface build settings use the persisted agent type. Default humanoid radius is sufficient for normal enemies.
            var data = CreateData(bossType);
            foreach (var enemy in data) if (enemy.prefab == null) { enemy.prefab = Prefab(enemy); EditorUtility.SetDirty(enemy); }
            foreach (var target in UnityEngine.Object.FindObjectsByType<TrainingTargetMotion>(FindObjectsSortMode.None))
            {
                var obstacle = target.gameObject.AddComponent<NavMeshObstacle>(); obstacle.shape = NavMeshObstacleShape.Capsule; obstacle.radius = 0.6f; obstacle.height = 1.9f; obstacle.center = Vector3.up * 0.95f; obstacle.carving = true;
            }
            Bake("ArenaNavigation", 0); Bake("ArenaBossNavigation", bossType);
            var signs = new GameObject("TelegraphPool").AddComponent<TelegraphPool>();
            var circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder); var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signs.Configure(circle.GetComponent<MeshFilter>().sharedMesh, line.GetComponent<MeshFilter>().sharedMesh, Material("Warning_Unlit", Color.white, true));
            UnityEngine.Object.DestroyImmediate(circle); UnityEngine.Object.DestroyImmediate(line);
            var heroes = UnityEngine.Object.FindFirstObjectByType<HeroRoster>(); var combat = heroes.GetComponent<HeroCombat>();
            var serialized = new SerializedObject(combat); serialized.FindProperty("targets").intValue = (1 << 10) | (1 << 13); serialized.ApplyModifiedProperties();
            var enemies = new GameObject("EnemyPool").AddComponent<EnemyPool>(); enemies.Configure(data, heroes, combat.Pool, signs, 1 << 9, (1 << 9) | (1 << 10) | (1 << 11) | (1 << 12) | (1 << 13), (1 << 11) | (1 << 12));
            var sockets = new Transform[3]; Vector3[] positions = { new Vector3(-7, 0.06f, -2), new Vector3(7, 0.06f, -2), new Vector3(0, 0.06f, 8) };
            for (int i = 0; i < sockets.Length; i++) { sockets[i] = new GameObject("EnemySpawn_" + i).transform; sockets[i].position = positions[i]; }
            var encounters = Encounters(data);
            var controller = new GameObject("EncounterController").AddComponent<EncounterController>(); controller.Configure(enemies, heroes, UnityEngine.Object.FindFirstObjectByType<ArenaSession>(), encounters, sockets);
            Hud(controller);
            PlayerSettings.bundleVersion = "0.3.0"; PlayerSettings.Android.bundleVersionCode = 3;
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); Debug.Log("PVE_INSTALL_OK");
        }
        static T Asset<T>(string folder, string name, Action<T> configure) where T : ScriptableObject
        { string path = Root + folder + "/" + name + ".asset"; var value = AssetDatabase.LoadAssetAtPath<T>(path); if (value != null) return value; value = ScriptableObject.CreateInstance<T>(); configure(value); AssetDatabase.CreateAsset(value, path); return value; }
        static AttackDefinition Attack(string name, int damage, float range, float warning, bool projectile = false)
        { return Asset<AttackDefinition>("Data/Enemies", name, a => { a.damage = damage; a.range = range; a.windup = warning; a.recovery = 0.55f; a.arc = 110; a.knockback = 1; a.shape = projectile ? AttackShape.Projectile : AttackShape.Arc; a.projectileSpeed = 8; a.projectileRadius = 0.16f; a.color = new Color(1, 0.25f, 0.2f); }); }
        static EnemyDefinition[] CreateData(int bossType)
        {
            string[] ids = { "raspador", "guarda-mare", "vigia-sombrio", "fagulha", "eco-forja", "fragmento", "custodio" };
            string[] names = { "Goblin Raspador", "Guardiao de Mare", "Vigia Sombrio", "Fagulha Instavel", "Eco da Forja", "Fragmento Invocado", "Custodio da Forja" };
            int[] hp = { 36, 95, 42, 28, 68, 18, 450 }, capacities = { 6, 3, 3, 3, 2, 6, 1 };
            float[] speed = { 4.4f, 2.1f, 3.3f, 3.9f, 2.8f, 4, 2.2f }, range = { 1.6f, 2, 11, 2.6f, 11, 1.4f, 4 };
            Color[] colors = { new Color(0.3f, 0.8f, 0.35f), new Color(0.4f, 0.55f, 0.65f), new Color(0.45f, 0.25f, 0.7f), new Color(1, 0.45f, 0.1f), new Color(0.8f, 0.25f, 0.75f), new Color(0.2f, 0.8f, 0.8f), new Color(0.8f, 0.4f, 0.18f) };
            var result = new EnemyDefinition[7];
            for (int i = 0; i < 7; i++)
            {
                int slot = i; var hit = Attack(ids[i] + "_Hit", i == 3 ? 24 : i == 1 ? 15 : i == 6 ? 22 : 10, range[i], i == 3 ? 1.1f : 0.65f, i == 2 || i == 4);
                result[i] = Asset<EnemyDefinition>("Data/Enemies", ids[i], e => { e.enemyId = ids[slot]; e.displayName = names[slot]; e.kind = (EnemyKind)slot; e.maximumHealth = hp[slot]; e.poolCapacity = capacities[slot]; e.speed = speed[slot]; e.attack = hit; e.color = colors[slot]; e.armor = slot == 1 ? 0.15f : slot == 6 ? 0.1f : 0; e.height = slot == 0 || slot == 5 ? 1.3f : slot == 6 ? 2.7f : 1.7f; e.radius = slot == 6 ? 0.7f : slot == 1 ? 0.4f : 0.32f; e.agentTypeId = slot == 6 ? bossType : 0; e.knockbackResistance = slot == 6 ? 1 : slot == 1 ? 0.6f : 0; e.staggerDuration = slot == 6 ? 0 : 0.12f; e.attackCooldown = slot == 2 ? 1.5f : 1.1f; e.retreatRange = slot == 4 ? 5 : 3.5f; e.preferredRange = slot == 4 ? 8 : 6; });
            }
            result[4].summonedEnemy = result[5]; EditorUtility.SetDirty(result[4]);
            var sweep = Attack("BossSweep", 22, 4, 0.9f); var marks = Attack("BossMarks", 18, 1.4f, 1.1f); var charge = Attack("BossCharge", 24, 1.2f, 1);
            result[6].boss = Asset<BossDefinition>("Data/Enemies", "CustodioPattern", b => b.attacks = new[] { new BossAttack { displayName = "Arco da Forja", kind = BossAttackKind.Sweep, hit = sweep, warning = 0.9f, radius = 3.5f, range = 4, arc = 120 }, new BossAttack { displayName = "Tres Selos", kind = BossAttackKind.GroundMarks, hit = marks, warning = 1.1f, radius = 1.3f, range = 16 }, new BossAttack { displayName = "Investida Incandescente", kind = BossAttackKind.Charge, hit = charge, warning = 1, radius = 0.9f, length = 7, range = 10, chargeSpeed = 8 } }); EditorUtility.SetDirty(result[6]);
            return result;
        }
        static Material Material(string name, Color color, bool unlit = false)
        { string path = Root + "Materials/" + name + ".mat"; var mat = AssetDatabase.LoadAssetAtPath<Material>(path); if (mat != null) return mat; mat = new Material(Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit")); mat.SetColor("_BaseColor", color); AssetDatabase.CreateAsset(mat, path); return mat; }
        static Renderer Shape(Transform parent, string name, PrimitiveType shape, Vector3 position, Vector3 scale, Material material)
        { var go = GameObject.CreatePrimitive(shape); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale; go.layer = 13; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); var renderer = go.GetComponent<Renderer>(); renderer.sharedMaterial = material; return renderer; }
        static GameObject Prefab(EnemyDefinition data)
        {
            var root = new GameObject(data.displayName); root.SetActive(false); root.layer = 13;
            var cc = root.AddComponent<CharacterController>(); cc.height = data.height; cc.radius = data.radius; cc.center = Vector3.up * data.height * 0.5f; cc.stepOffset = 0.25f; cc.enabled = false;
            root.AddComponent<NavMeshAgent>().enabled = false;
            var hp = root.AddComponent<Health>(); var motor = root.AddComponent<EnemyMotor>(); motor.Configure(hp);
            var attacks = root.AddComponent<EnemyCombat>(); var ai = root.AddComponent<EnemyBrain>(); var actor = root.AddComponent<EnemyActor>(); var view = root.AddComponent<EnemyPresentation>(); var guard = root.AddComponent<DirectionalGuard>();
            actor.Configure(hp, ai, attacks, motor, view, guard); hp.Configure(data.maximumHealth, CombatFaction.Enemy, data.armor); hp.ConfigureDefense(guard); guard.Configure(ai);
            var model = new GameObject("Original_GeometricPlaceholder").transform; model.SetParent(root.transform, false); var mat = Material("Enemy_" + data.enemyId, data.color); var steel = Material("Enemy_Steel", Color.gray); var parts = new List<Renderer>();
            parts.Add(Shape(model, "Body", data.kind == EnemyKind.Unstable ? PrimitiveType.Sphere : PrimitiveType.Capsule, Vector3.up * data.height * 0.5f, new Vector3(data.radius * 2, data.height * 0.5f, data.radius * 2), mat));
            parts.Add(Shape(model, "Eyes_Facing", PrimitiveType.Cube, new Vector3(0, data.height * 0.75f, data.radius), new Vector3(0.3f, 0.1f, 0.1f), steel));
            GameObject shield = null;
            if (data.kind == EnemyKind.Guardian) shield = Shape(model, "FrontalShield", PrimitiveType.Cube, new Vector3(0, 0.9f, 0.55f), new Vector3(1, 1.4f, 0.15f), steel).gameObject;
            if (data.kind == EnemyKind.Archer) Shape(model, "Bow", PrimitiveType.Cube, new Vector3(0.5f, 1, 0.2f), new Vector3(0.1f, 1, 0.15f), steel);
            if (data.kind == EnemyKind.Summoner || data.kind == EnemyKind.Boss) Shape(model, "Staff", PrimitiveType.Cylinder, new Vector3(data.radius + 0.15f, data.height * 0.5f, 0.2f), new Vector3(0.15f, data.height * 0.5f, 0.15f), steel);
            if (data.kind == EnemyKind.Unstable) Shape(model, "Fuse", PrimitiveType.Cylinder, new Vector3(0, data.height, 0), new Vector3(0.12f, 0.2f, 0.12f), steel);
            if (data.kind == EnemyKind.Raider) Shape(model, "Blade", PrimitiveType.Cube, new Vector3(0.45f, 0.7f, 0.3f), new Vector3(0.15f, 0.1f, 0.65f), steel);
            view.Configure(actor, model, parts.ToArray(), shield);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Root + "Prefabs/Enemies/" + data.enemyId + ".prefab"); UnityEngine.Object.DestroyImmediate(root); return prefab;
        }
        static void Bake(string name, int type)
        {
            var surface = new GameObject(name).AddComponent<NavMeshSurface>(); surface.agentTypeID = type; surface.collectObjects = CollectObjects.All; surface.layerMask = (1 << 8) | (1 << 9); surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders; surface.BuildNavMesh();
            if (surface.navMeshData == null) throw new InvalidOperationException("Navigation bake failed.");
            AssetDatabase.CreateAsset(surface.navMeshData, Root + "Navigation/" + name + ".asset"); EditorUtility.SetDirty(surface);
        }
        static EncounterWave Wave(string name, params SpawnEntry[] entries) => new EncounterWave { displayName = name, entries = entries, delayBefore = 0.6f };
        static SpawnEntry Entry(EnemyDefinition enemy, int count) => new SpawnEntry { enemy = enemy, count = count, delay = 0.4f };
        static EncounterDefinition[] Encounters(EnemyDefinition[] data)
        {
            var list = new List<EncounterDefinition>();
            list.Add(Asset<EncounterDefinition>("Data/Encounters", "MixedWaves", e => { e.encounterId = "mixed"; e.displayName = "Tres ondas PvE"; e.waves = new[] { Wave("Raspadores", Entry(data[0], 3)), Wave("Linha de defesa", Entry(data[1], 1), Entry(data[2], 1), Entry(data[3], 1)), Wave("Invocacao", Entry(data[4], 1), Entry(data[0], 2)) }; }));
            for (int i = 0; i < 5; i++) { int slot = i; list.Add(Asset<EncounterDefinition>("Data/Encounters", "Solo_" + data[i].enemyId, e => { e.encounterId = "solo-" + data[slot].enemyId; e.displayName = data[slot].displayName; e.waves = new[] { Wave(e.displayName, Entry(data[slot], 1)) }; })); }
            list.Add(Asset<EncounterDefinition>("Data/Encounters", "Boss", e => { e.encounterId = "boss"; e.displayName = data[6].displayName; var wave = Wave(e.displayName, Entry(data[6], 1)); wave.completion = WaveEndCondition.DefeatBoss; e.waves = new[] { wave }; })); return list.ToArray();
        }
        static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        { var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = size; return rect; }
        static Text Label(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        { var text = Rect(name, parent, anchor, position, size).gameObject.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 18; text.color = Color.white; text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false; return text; }
        static void Button(Transform parent, string name, Vector2 position, string text, UnityEngine.Events.UnityAction action)
        { var rect = Rect(name, parent, new Vector2(0, 1), position, new Vector2(190, 42)); var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(0.15f, 0.28f, 0.35f, 0.95f); var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; UnityEventTools.AddPersistentListener(button.onClick, action); Label(rect, "Label", Vector2.one * 0.5f, Vector2.zero, new Vector2(180, 40)).text = text; }
        static void Hud(EncounterController encounter)
        {
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>().transform;
            Button(canvas, "StartEncounter", new Vector2(15, -120), "INICIAR ENCONTRO", encounter.StartSelected); Button(canvas, "NextEncounter", new Vector2(15, -168), "PROXIMO ENCONTRO", encounter.NextPreset);
            var status = Label(canvas, "EncounterStatus", new Vector2(0.5f, 1), new Vector2(0, -160), new Vector2(650, 32));
            var bar = Rect("BossBar", canvas, new Vector2(0.5f, 1), new Vector2(0, -230), new Vector2(460, 18)); bar.gameObject.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            var fill = Rect("Fill", bar, Vector2.one * 0.5f, Vector2.zero, new Vector2(460, 18)).gameObject.AddComponent<Image>(); fill.color = new Color(0.9f, 0.15f, 0.1f); fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal;
            var bossText = Label(bar, "BossName", new Vector2(0.5f, 1), new Vector2(0, 8), new Vector2(600, 28)); canvas.gameObject.AddComponent<EncounterHud>().Configure(encounter, status, bossText, fill);
        }
        public static void RepairHudLayout()
        {
            var scene = EditorSceneManager.OpenScene(ArenaBuilder.ScenePath);
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>().transform;
            canvas.Find("EncounterStatus").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -160);
            canvas.Find("BossBar").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -230);
            EditorSceneManager.SaveScene(scene); Debug.Log("PVE_HUD_REPAIRED");
        }
    }
}
