using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
namespace ForjaDasMares.Editor
{
    public static class HeroInstaller
    {
        const string Root = "Assets/ForjaDasMares/";
        [MenuItem("Forja das Mares/Upgrade existing arena with heroes")]
        public static void Apply()
        {
            if (!File.Exists(ArenaBuilder.ScenePath)) throw new InvalidOperationException("Existing arena is required.");
            var scene = EditorSceneManager.OpenScene(ArenaBuilder.ScenePath);
            if (UnityEngine.Object.FindFirstObjectByType<HeroRoster>() != null) { Debug.Log("HERO_INSTALL_ALREADY_DONE"); return; }
            Directory.CreateDirectory(Root + "Data/Heroes"); AssetDatabase.Refresh();
            var heroes = CreateData();
            var motor = UnityEngine.Object.FindFirstObjectByType<PlayerMotor>();
            var session = UnityEngine.Object.FindFirstObjectByType<ArenaSession>();
            var input = motor.GetComponent<ArenaInput>();
            UpgradeInput(input);
            var player = motor.gameObject;
            player.GetComponent<MeleeAttack>().enabled = false;
            player.GetComponent<CombatFeedback>().enabled = false;
            foreach (Transform child in player.transform) if (child.name != "StrikeOrigin") child.gameObject.SetActive(false);
            var hp = player.AddComponent<Health>();
            var pool = new GameObject("CombatEffectPool").AddComponent<CombatEffectPool>(); pool.Configure(Material("Combat_White", Color.white));
            ConfigurePoolMeshes(pool);
            var combat = player.AddComponent<HeroCombat>();
            combat.Configure(motor, hp, player.transform.Find("StrikeOrigin"), pool, 1 << 10, 1 << 9, (1 << 11) | (1 << 12));
            var presentation = player.AddComponent<HeroPresentation>();
            var visuals = new Transform[heroes.Length];
            for (int i = 0; i < heroes.Length; i++) visuals[i] = Visual(player.transform, heroes[i], i);
            presentation.Configure(visuals, motor, combat, hp);
            var ally = Target("TrainingAlly_ShieldTest", new Vector3(-3.5f, 0.05f, -3.5f), player.transform.parent, Color.cyan, 12, 150, CombatFaction.Player);
            var main = session.TrainingTarget;
            var targets = new TrainingTargetMotion[3];
            targets[0] = main.gameObject.AddComponent<TrainingTargetMotion>(); targets[0].Configure(1 << 9);
            for (int i = 1; i < 3; i++)
            {
                var target = Target("TrainingTarget_Group_" + i, new Vector3(i == 1 ? -1.5f : 1.5f, 0.05f, 2.7f), player.transform.parent, new Color(1, 0.45f, 0.15f), 10, 140, CombatFaction.Training);
                targets[i] = target.gameObject.AddComponent<TrainingTargetMotion>(); targets[i].Configure(1 << 9);
            }
            var roster = player.AddComponent<HeroRoster>(); roster.Configure(heroes, motor, combat, hp, ally, presentation);
            session.ConfigureHeroes(roster, targets, ally);
            ConfigureHud(input, roster, combat);
            PlayerSettings.bundleVersion = "0.2.0"; PlayerSettings.Android.bundleVersionCode = 2;
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); Debug.Log("HERO_INSTALL_OK");
        }
        static void ConfigurePoolMeshes(CombatEffectPool pool)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pool.ConfigureMeshes(sphere.GetComponent<MeshFilter>().sharedMesh, cylinder.GetComponent<MeshFilter>().sharedMesh);
            UnityEngine.Object.DestroyImmediate(sphere); UnityEngine.Object.DestroyImmediate(cylinder);
            EditorUtility.SetDirty(pool);
        }
        public static void RepairPoolMeshes()
        {
            var scene = EditorSceneManager.OpenScene(ArenaBuilder.ScenePath);
            ConfigurePoolMeshes(UnityEngine.Object.FindFirstObjectByType<CombatEffectPool>());
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); Debug.Log("HERO_POOL_MESHES_OK");
        }
        static T Asset<T>(string name, Action<T> configure) where T : ScriptableObject
        {
            string path = Root + "Data/Heroes/" + name + ".asset";
            T existing = AssetDatabase.LoadAssetAtPath<T>(path); if (existing != null) return existing;
            T data = ScriptableObject.CreateInstance<T>(); configure(data); AssetDatabase.CreateAsset(data, path); return data;
        }
        static AttackDefinition Attack(string name, AttackShape shape, int damage, float range, float recovery, Color color, float knock = 0, ControlEffect control = ControlEffect.None, float splash = 0)
        {
            return Asset<AttackDefinition>(name, a => { a.shape = shape; a.damage = damage; a.range = range; a.recovery = recovery; a.windup = 0.1f; a.arc = 120; a.color = color; a.knockback = knock; a.control = control; a.controlDuration = control == ControlEffect.Root ? 1.5f : 2; a.splashRadius = splash; a.projectileSpeed = 17; });
        }
        static HeroDefinition[] CreateData()
        {
            var result = new HeroDefinition[4];
            string[] ids = { "warrior", "ranger", "arcanist", "rune-knight" };
            string[] names = { "GUERREIRO - Brasa", "PATRULHEIRO - Vento", "ARCANISTA - Prisma", "CAVALEIRO RUNICO - Baluarte" };
            string[] strengths = { "Golpes em arco e impacto em grupo", "Alcance, mobilidade e armadilha", "Tres elementos e controle de grupos", "Resistencia, protecao e investida" };
            string[] weaknesses = { "Precisa aproximar-se", "Pouca resistencia", "Vida baixa", "Movimento lento e alcance curto" };
            Color[] colors = { new Color(1, 0.45f, 0.16f), new Color(0.2f, 0.85f, 0.4f), new Color(0.65f, 0.35f, 1), new Color(0.3f, 0.65f, 1) };
            int[] hp = { 140, 105, 80, 210 }; float[] speed = { 5.8f, 6.8f, 5.5f, 4.7f };
            for (int i = 0; i < 4; i++)
            {
                int slot = i; Color color = colors[i];
                AttackDefinition[] attacks;
                if (i == 0) attacks = new[] { Attack("Sword_1", AttackShape.Arc, 26, 2.8f, 0.3f, color, 2), Attack("Sword_2", AttackShape.Arc, 28, 2.8f, 0.32f, color, 2), Attack("Sword_3", AttackShape.Arc, 36, 3, 0.45f, color, 4) };
                else if (i == 1) attacks = new[] { Attack("Arrow", AttackShape.Projectile, 20, 13, 0.25f, color) };
                else if (i == 2) attacks = new[] { Attack("Ice", AttackShape.Projectile, 16, 11, 0.35f, Color.cyan, 0, ControlEffect.Slow), Attack("Fire", AttackShape.Projectile, 20, 11, 0.35f, new Color(1, 0.3f, 0), 0, ControlEffect.None, 2.8f), Attack("Lightning", AttackShape.Projectile, 18, 11, 0.35f, Color.yellow, 3) };
                else attacks = new[] { Attack("RuneStrike", AttackShape.Arc, 22, 2.4f, 0.45f, color, 3) };
                var hit = Attack(ids[i] + "_SpecialHit", AttackShape.Arc, i == 0 ? 45 : i == 1 ? 28 : i == 2 ? 32 : 24, 4, 0.35f, color, i == 0 ? 6 : i == 3 ? 8 : 0, i == 1 || i == 2 ? ControlEffect.Root : ControlEffect.None);
                var ability = Asset<AbilityDefinition>(ids[i] + "_Ability", a => { a.displayName = new[] { "Impacto de Brasa", "Laco de Raizes", "Pulso Glacial", "Guarda e Investida" }[slot]; a.effect = (AbilityEffect)slot; a.hit = hit; a.radius = slot == 0 ? 3.4f : slot == 1 ? 1.8f : 4; a.cooldown = slot == 2 ? 7 : 5; });
                result[i] = Asset<HeroDefinition>(ids[i], h => { h.heroId = ids[slot]; h.displayName = names[slot]; h.role = names[slot].Split('-')[0]; h.strengths = strengths[slot]; h.weaknesses = weaknesses[slot]; h.maximumHealth = hp[slot]; h.speed = speed[slot]; h.armor = slot == 3 ? 0.18f : slot == 0 ? 0.05f : 0; h.attacks = attacks; h.special = ability; h.visualSlot = slot; h.color = color; h.dodge.speed = slot == 1 ? 15 : 12; h.dodge.cooldown = slot == 1 ? 0.6f : 0.8f; });
            }
            return result;
        }
        static void UpgradeInput(ArenaInput reader)
        {
            string path = Root + "Input/ArenaControls.inputactions";
            var data = InputActionAsset.FromJson(File.ReadAllText(path)); var map = data.FindActionMap("Gameplay", true);
            Add(map, "Special", "<Keyboard>/e", "<Mouse>/rightButton", "<Gamepad>/buttonWest");
            Add(map, "NextHero", "<Keyboard>/tab", "<Gamepad>/rightShoulder"); Add(map, "TestDamage", "<Keyboard>/h"); Add(map, "Victory", "<Keyboard>/v");
            for (int i = 1; i <= 4; i++) Add(map, "Hero" + i, "<Keyboard>/digit" + i);
            File.WriteAllText(path, data.ToJson()); UnityEngine.Object.DestroyImmediate(data); AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            reader.Configure(AssetDatabase.LoadAssetAtPath<InputActionAsset>(path));
        }
        static void Add(InputActionMap map, string name, params string[] bindings)
        { if (map.FindAction(name) != null) return; var action = map.AddAction(name, InputActionType.Button); foreach (string binding in bindings) action.AddBinding(binding); }
        static Material Material(string name, Color color)
        {
            string path = Root + "Materials/" + name + ".mat"; var mat = AssetDatabase.LoadAssetAtPath<Material>(path); if (mat != null) return mat;
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.SetColor("_BaseColor", color); AssetDatabase.CreateAsset(mat, path); return mat;
        }
        static Transform Primitive(string name, Transform parent, PrimitiveType shape, Vector3 pos, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(shape); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go.transform;
        }
        static Transform Visual(Transform parent, HeroDefinition hero, int slot)
        {
            Transform root = new GameObject("HeroVisual_" + hero.heroId).transform; root.SetParent(parent, false);
            var mat = Material("Hero_" + hero.heroId, hero.color); var steel = Material("Hero_Steel", new Color(0.8f, 0.85f, 0.9f));
            Primitive("Body_Placeholder", root, slot == 2 ? PrimitiveType.Cylinder : PrimitiveType.Capsule, new Vector3(0, 0.85f, 0), new Vector3(slot == 3 ? 0.95f : 0.65f, 0.8f, 0.65f), mat);
            Primitive("Head_Placeholder", root, PrimitiveType.Sphere, new Vector3(0, 1.6f, 0), Vector3.one * 0.5f, steel);
            Primitive("FacingMarker", root, PrimitiveType.Cube, new Vector3(0, 1.55f, 0.28f), new Vector3(0.35f, 0.12f, 0.1f), mat);
            if (slot == 0) Primitive("GreatSword", root, PrimitiveType.Cube, new Vector3(0.55f, 1, 0.45f), new Vector3(0.28f, 0.12f, 1.5f), steel);
            if (slot == 1)
            { Primitive("Bow", root, PrimitiveType.Cube, new Vector3(0.55f, 1, 0.3f), new Vector3(0.12f, 1.1f, 0.15f), mat); Primitive("Quiver", root, PrimitiveType.Cylinder, new Vector3(0, 1, -0.4f), new Vector3(0.35f, 0.6f, 0.35f), steel); }
            if (slot == 2)
            { Primitive("Staff", root, PrimitiveType.Cylinder, new Vector3(0.55f, 1, 0.25f), new Vector3(0.12f, 1, 0.12f), steel); Primitive("Focus", root, PrimitiveType.Sphere, new Vector3(0.55f, 2, 0.25f), Vector3.one * 0.35f, mat); }
            if (slot == 3)
            { Primitive("Shield", root, PrimitiveType.Cube, new Vector3(-0.6f, 1, 0.25f), new Vector3(0.15f, 1.1f, 0.8f), steel); Primitive("RuneBlade", root, PrimitiveType.Cube, new Vector3(0.55f, 1, 0.35f), new Vector3(0.18f, 0.15f, 0.9f), mat); }
            root.gameObject.SetActive(slot == 0); return root;
        }
        static Health Target(string name, Vector3 position, Transform parent, Color color, int layer, int maximum, CombatFaction faction)
        {
            var go = new GameObject(name); go.layer = layer; go.transform.SetParent(parent); go.transform.position = position;
            var collider = go.AddComponent<CapsuleCollider>(); collider.height = 1.9f; collider.radius = 0.55f; collider.center = Vector3.up * 0.95f;
            var hp = go.AddComponent<Health>(); hp.Configure(maximum, faction);
            var body = Primitive("Body_Placeholder", go.transform, PrimitiveType.Capsule, Vector3.up * 0.95f, new Vector3(1.1f, 0.95f, 1.1f), Material(layer == 12 ? "Ally_Cyan" : "Target_Orange", color));
            go.AddComponent<TargetFeedback>().Configure(hp, body.GetComponent<Renderer>()); return hp;
        }
        static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        static Text Text(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, string text)
        {
            var label = Rect(name, parent, anchor, position, size).gameObject.AddComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 18; label.color = Color.white; label.alignment = TextAnchor.MiddleCenter; label.raycastTarget = false; label.text = text; return label;
        }
        static void Button(Transform parent, string name, Vector2 pos, string text, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(name, parent, Vector2.one, pos, new Vector2(150, 40)); var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(0.13f, 0.3f, 0.4f, 0.95f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; UnityEventTools.AddPersistentListener(button.onClick, action);
            Text(rect, "Label", Vector2.one * 0.5f, Vector2.zero, new Vector2(150, 40), text);
        }
        static void ConfigureHud(ArenaInput input, HeroRoster roster, HeroCombat combat)
        {
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            var hud = canvas.gameObject.AddComponent<HeroHud>();
            hud.Configure(roster, combat, Text(canvas.transform, "HeroStatus", new Vector2(0.5f, 1), new Vector2(0, -85), new Vector2(800, 65), ""));
            Button(canvas.transform, "NextHero", new Vector2(-20, -70), "HERÓI / TAB", roster.Next);
            Button(canvas.transform, "TestDamage", new Vector2(-20, -120), "DANO / H", roster.TestDamage);
            Button(canvas.transform, "Victory", new Vector2(-20, -170), "VITÓRIA / V", roster.Victory);
            var panel = canvas.transform.Find("TouchControls_Minimal"); var stick = panel.Find("TouchMoveStick").GetComponent<RectTransform>();
            UnityEngine.Object.DestroyImmediate(stick.GetComponent<OnScreenStick>());
            var thumb = Rect("StickThumb", stick, Vector2.one * 0.5f, Vector2.zero, Vector2.one * 55); thumb.gameObject.AddComponent<Image>().color = new Color(0.8f, 1, 1, 0.9f);
            stick.gameObject.AddComponent<TouchMoveStick>().Configure(input, thumb);
            var skill = Rect("TouchSpecial", panel, new Vector2(1, 0), new Vector2(-120, 205), new Vector2(150, 70)); skill.gameObject.AddComponent<Image>().color = new Color(0.4f, 0.2f, 0.65f, 0.9f);
            skill.gameObject.AddComponent<OnScreenButton>().controlPath = "<Gamepad>/buttonWest";
            Text(skill, "Label", Vector2.one * 0.5f, Vector2.zero, new Vector2(145, 65), "HABILIDADE");
        }
    }
}
