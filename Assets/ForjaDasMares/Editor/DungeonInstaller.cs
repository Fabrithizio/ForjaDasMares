using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace ForjaDasMares.Editor
{
    public static class DungeonInstaller
    {
        public const string ScenePath = "Assets/ForjaDasMares/Scenes/DungeonTest.unity";
        const string Root = "Assets/ForjaDasMares/";
        [MenuItem("Forja das Mares/Install dungeon test (preserves arena)")]
        public static void Apply()
        {
            if (File.Exists(ScenePath)) { Debug.Log("DUNGEON_ALREADY_INSTALLED"); return; }
            var scene = EditorSceneManager.OpenScene(ArenaBuilder.ScenePath);
            var encounter = UnityEngine.Object.FindFirstObjectByType<EncounterController>(); if (encounter == null) throw new InvalidOperationException("Validated PvE arena is required.");
            Directory.CreateDirectory(Root + "Data/Dungeons"); AssetDatabase.Refresh();
            var meshes = new Mesh[3]; PrimitiveType[] types = { PrimitiveType.Cube, PrimitiveType.Cylinder, PrimitiveType.Sphere };
            for (int i = 0; i < types.Length; i++) { var primitive = GameObject.CreatePrimitive(types[i]); meshes[i] = primitive.GetComponent<MeshFilter>().sharedMesh; UnityEngine.Object.DestroyImmediate(primitive); }
            var biomes = Data(encounter, meshes); var rooms = new RoomDefinition[8];
            for (int i = 0; i < 8; i++) { int slot = i; rooms[i] = Asset<RoomDefinition>("Room_" + ((RoomCategory)i), r => { r.category = (RoomCategory)slot; r.obstaclePositions = slot == 1 || slot == 2 ? new[] { new Vector3(-2.8f, 0, -1.8f), new Vector3(2.8f, 0, 0) } : Array.Empty<Vector3>(); }); }
            var trap = Asset<AttackDefinition>("TrapHit", a => { a.damage = 16; a.range = 1.4f; a.windup = 1.1f; a.knockback = 0; a.arc = 360; a.color = Color.red; });
            GameObject.Find("01_Environment").SetActive(false);
            foreach (var target in UnityEngine.Object.FindObjectsByType<TrainingTargetMotion>(FindObjectsSortMode.None)) target.gameObject.SetActive(false);
            encounter.Heroes.Ally.gameObject.SetActive(false);
            foreach (var surface in UnityEngine.Object.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None)) surface.gameObject.SetActive(false);
            var root = new GameObject("DungeonSystems"); var generator = root.AddComponent<DungeonGenerator>();
            var normal = new GameObject("DungeonNavigation").AddComponent<NavMeshSurface>(); normal.transform.SetParent(root.transform); SetupNavigation(normal, 0);
            var boss = new GameObject("DungeonBossNavigation").AddComponent<NavMeshSurface>(); boss.transform.SetParent(root.transform); SetupNavigation(boss, biomes[0].boss.waves[0].entries[0].enemy.agentTypeId);
            generator.Configure(meshes[0], meshes[1], rooms, normal, boss, trap);
            string inputPath = Root + "Input/DungeonControls.inputactions";
            if (!File.Exists(inputPath))
            {
                var controls = ScriptableObject.CreateInstance<InputActionAsset>(); var map = controls.AddActionMap("Dungeon"); var action = map.AddAction("Interact", InputActionType.Button); action.AddBinding("<Keyboard>/f"); action.AddBinding("<Gamepad>/buttonNorth"); File.WriteAllText(inputPath, controls.ToJson()); UnityEngine.Object.DestroyImmediate(controls); AssetDatabase.ImportAsset(inputPath, ImportAssetOptions.ForceSynchronousImport);
            }
            var input = root.AddComponent<DungeonInteractionInput>(); input.Configure(AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath));
            var session = root.AddComponent<DungeonSession>(); session.Configure(generator, encounter, encounter.Heroes, UnityEngine.Object.FindFirstObjectByType<ArenaSession>(), UnityEngine.Object.FindFirstObjectByType<ArenaCamera>(), input, UnityEngine.Object.FindFirstObjectByType<TelegraphPool>(), biomes, UnityEngine.Object.FindFirstObjectByType<Light>());
            Hud(session, input);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ArenaBuilder.ScenePath, true), new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.bundleVersion = "0.4.0"; PlayerSettings.Android.bundleVersionCode = 4;
            AssetDatabase.SaveAssets(); Debug.Log("DUNGEON_INSTALL_OK");
        }
        static void SetupNavigation(NavMeshSurface surface, int type) { surface.agentTypeID = type; surface.collectObjects = CollectObjects.All; surface.layerMask = (1 << 8) | (1 << 9); surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders; }
        static T Asset<T>(string name, Action<T> configure) where T : ScriptableObject
        { string path = Root + "Data/Dungeons/" + name + ".asset"; var asset = AssetDatabase.LoadAssetAtPath<T>(path); if (asset != null) return asset; asset = ScriptableObject.CreateInstance<T>(); configure(asset); AssetDatabase.CreateAsset(asset, path); return asset; }
        static Material Material(string name, Color color)
        { string path = Root + "Materials/" + name + ".mat"; var value = AssetDatabase.LoadAssetAtPath<Material>(path); if (value != null) return value; value = new Material(Shader.Find("Universal Render Pipeline/Lit")); value.SetColor("_BaseColor", color); AssetDatabase.CreateAsset(value, path); return value; }
        static EncounterDefinition Fight(string id, string name, params SpawnEntry[] entries)
        { return Asset<EncounterDefinition>(id, e => { e.encounterId = id; e.displayName = name; e.maximumMobileEnemies = 8; e.maximumPcEnemies = 12; e.waves = new[] { new EncounterWave { displayName = name, delayBefore = 0.5f, entries = entries } }; }); }
        static SpawnEntry Entry(EnemyDefinition data, int count) => new SpawnEntry { enemy = data, count = count, delay = 0.4f };
        static BiomeDefinition[] Data(EncounterController encounter, Mesh[] meshes)
        {
            var enemies = new EnemyDefinition[5]; for (int i = 0; i < 5; i++) enemies[i] = encounter.Presets[i + 1].waves[0].entries[0].enemy;
            var reward = Asset<RoomRewardTable>("ChestRewards", r => { r.minimumCoins = 5; r.maximumCoins = 12; r.victoryCoins = 4; });
            string[] ids = { "catacumbas", "floresta-subterranea", "fortaleza-vulcanica" }, names = { "CATACUMBAS ANTIGAS", "FLORESTA SUBTERRANEA", "FORTALEZA VULCANICA" };
            Color[] floors = { new Color(0.23f, 0.25f, 0.32f), new Color(0.08f, 0.24f, 0.2f), new Color(0.23f, 0.13f, 0.12f) }, walls = { new Color(0.48f, 0.44f, 0.38f), new Color(0.16f, 0.34f, 0.27f), new Color(0.31f, 0.25f, 0.25f) }, accents = { new Color(0.95f, 0.72f, 0.25f), new Color(0.3f, 0.95f, 0.65f), new Color(1, 0.38f, 0.08f) };
            var result = new BiomeDefinition[3];
            for (int i = 0; i < 3; i++)
            {
                int slot = i; var floor = Material(ids[i] + "_Floor", floors[i]); var wall = Material(ids[i] + "_Wall", walls[i]); var accent = Material(ids[i] + "_Accent", accents[i]); var decoration = Material(ids[i] + "_Tiles", Color.Lerp(floors[i], walls[i], 0.2f));
                EncounterDefinition first, second, third;
                if (i == 0) { first = Fight(ids[i] + "_Early", "Saqueadores", Entry(enemies[0], 2)); second = Fight(ids[i] + "_Middle", "Sentinelas", Entry(enemies[1], 1), Entry(enemies[0], 1)); third = Fight(ids[i] + "_Hard", "Guarda sombria", Entry(enemies[1], 1), Entry(enemies[2], 1), Entry(enemies[0], 2)); }
                else if (i == 1) { first = Fight(ids[i] + "_Early", "Patrulha das raizes", Entry(enemies[0], 2)); second = Fight(ids[i] + "_Middle", "Eco entre folhas", Entry(enemies[4], 1), Entry(enemies[0], 1)); third = Fight(ids[i] + "_Hard", "Sombras e ecos", Entry(enemies[4], 1), Entry(enemies[2], 1), Entry(enemies[0], 2)); }
                else { first = Fight(ids[i] + "_Early", "Fagulhas", Entry(enemies[3], 1), Entry(enemies[0], 1)); second = Fight(ids[i] + "_Middle", "Guarda da forja", Entry(enemies[1], 1), Entry(enemies[3], 1)); third = Fight(ids[i] + "_Hard", "Linha incandescente", Entry(enemies[1], 1), Entry(enemies[2], 1), Entry(enemies[3], 2)); }
                result[i] = Asset<BiomeDefinition>(ids[i], b => { b.biomeId = ids[slot]; b.displayName = names[slot]; b.floor = floor; b.wall = wall; b.accent = accent; b.decoration = decoration; b.light = Color.Lerp(accents[slot], Color.white, 0.65f); b.ambient = Color.Lerp(floors[slot], Color.white, 0.35f); b.decorationMesh = meshes[slot == 0 ? 0 : slot == 1 ? 2 : 1]; b.decorationScale = slot == 0 ? new Vector3(0.5f, 1.5f, 0.5f) : slot == 1 ? new Vector3(1, 0.8f, 1) : new Vector3(0.6f, 1.1f, 0.6f); b.encountersByDepth = new[] { first, second, third }; b.boss = encounter.Presets[6]; b.rewards = reward; });
            }
            return result;
        }
        static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        { var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = size; return rect; }
        static Text Label(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        { var label = Rect(name, parent, anchor, position, size).gameObject.AddComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 18; label.color = Color.white; label.alignment = TextAnchor.MiddleCenter; label.raycastTarget = false; return label; }
        static void Button(Transform parent, string name, Vector2 position, string text, UnityEngine.Events.UnityAction action, Vector2? anchor = null)
        { var rect = Rect(name, parent, anchor ?? new Vector2(0, 1), position, new Vector2(190, 40)); var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(0.16f, 0.3f, 0.4f, 0.95f); var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; UnityEventTools.AddPersistentListener(button.onClick, action); Label(rect, "Label", Vector2.one * 0.5f, Vector2.zero, new Vector2(185, 36)).text = text; }
        static void Hud(DungeonSession session, DungeonInteractionInput input)
        {
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>(); var root = canvas.transform;
            canvas.GetComponent<HeroHud>().enabled = false; canvas.GetComponent<EncounterHud>().enabled = false;
            foreach (string name in new[] { "TargetHealthLabel", "TargetHealthBackground", "StartEncounter", "NextEncounter", "TestDamage", "Victory" }) root.Find(name).gameObject.SetActive(false);
            root.Find("ControlsHelp").GetComponent<Text>().text = "WASD / JOYSTICK | ATAQUE | HABILIDADE | ESQUIVA | F / INTERAGIR | R: reiniciar";
            Button(root, "Biome", new Vector2(15, -120), "TROCAR BIOMA", session.NextBiome); Button(root, "Seed", new Vector2(15, -166), "NOVA SEED", session.NextSeed); Button(root, "Fixed", new Vector2(15, -212), "FIXO / PROCEDURAL", session.ToggleFixed); Button(root, "Arena", new Vector2(15, -258), "VOLTAR A ARENA", session.BackToArena);
            Button(root, "Interact", new Vector2(-20, -260), "INTERAGIR / F", input.RequestInteraction, Vector2.one);
            var title = root.Find("Title").GetComponent<Text>(); title.rectTransform.anchorMin = title.rectTransform.anchorMax = title.rectTransform.pivot = new Vector2(0.5f, 1); title.rectTransform.anchoredPosition = new Vector2(0, -18); title.rectTransform.sizeDelta = new Vector2(800, 40);
            var prompt = Label(root, "InteractionPrompt", new Vector2(0.5f, 0), new Vector2(0, 66), new Vector2(750, 34));
            canvas.gameObject.AddComponent<DungeonHud>().Configure(session, title, root.Find("HeroStatus").GetComponent<Text>(), root.Find("EncounterStatus").GetComponent<Text>(), prompt, root.Find("BossBar/BossName").GetComponent<Text>(), root.Find("BossBar/Fill").GetComponent<Image>());
        }
        [MenuItem("Forja das Mares/Open dungeon test")]
        public static void Open() { if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return; EditorSceneManager.OpenScene(ScenePath); }
    }
}
