using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ForjaDasMares.Editor
{
    public static class ArenaBuilder
    {
        public const string ScenePath = "Assets/ForjaDasMares/Scenes/ArenaTest.unity";
        const string Root = "Assets/ForjaDasMares/";
        const int FloorLayer = 8, SolidLayer = 9, TargetLayer = 10, PlayerLayer = 11;
        static int SolidMask => (1 << SolidLayer);

        [MenuItem("Forja das Mares/Create test arena (only if absent)")]
        public static void Build()
        {
            if (File.Exists(ScenePath))
            {
                Debug.Log("Arena already exists; preserved without regenerating.");
                return;
            }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Root + "Scenes");
            Directory.CreateDirectory(Root + "Materials");
            Directory.CreateDirectory(Root + "Data");
            Directory.CreateDirectory(Root + "Input");
            AssetDatabase.Refresh();
            ConfigureLayers();
            ConfigurePlayer();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material floor = MaterialAsset("Floor_Slate", new Color(0.13f, 0.23f, 0.29f));
            Material tile = MaterialAsset("Floor_Tiles", new Color(0.18f, 0.31f, 0.36f));
            Material wall = MaterialAsset("Walls_Stone", new Color(0.27f, 0.35f, 0.43f));
            Material copper = MaterialAsset("Obstacle_Copper", new Color(0.7f, 0.35f, 0.16f));
            Material blue = MaterialAsset("Hero_Teal", new Color(0.05f, 0.75f, 0.8f));
            Material cream = MaterialAsset("Hero_Cream", new Color(0.94f, 0.83f, 0.63f));
            Material gold = MaterialAsset("Weapon_Gold", new Color(1f, 0.69f, 0.18f));
            Material orange = MaterialAsset("Target_Orange", new Color(0.98f, 0.39f, 0.16f));

            Transform environment = new GameObject("01_Environment").transform;
            Primitive("Floor", PrimitiveType.Cube, environment, new Vector3(0f, -0.3f, 0f), new Vector3(26f, 0.6f, 26f), floor, FloorLayer);
            // Decorative tiles have no colliders and do not affect navigation.
            for (int x = -10; x <= 10; x += 4)
                for (int z = -10; z <= 10; z += 4)
                    Primitive($"Tile_{x}_{z}", PrimitiveType.Cube, environment, new Vector3(x, 0.006f, z), new Vector3(3.85f, 0.012f, 3.85f), tile, FloorLayer, false);
            Primitive("Wall_North", PrimitiveType.Cube, environment, new Vector3(0, 1.5f, 12.5f), new Vector3(26f, 3f, 0.8f), wall, SolidLayer);
            Primitive("Wall_South", PrimitiveType.Cube, environment, new Vector3(0, 1.5f, -12.5f), new Vector3(26f, 3f, 0.8f), wall, SolidLayer);
            Primitive("Wall_East", PrimitiveType.Cube, environment, new Vector3(12.5f, 1.5f, 0), new Vector3(0.8f, 3f, 24f), wall, SolidLayer);
            Primitive("Wall_West", PrimitiveType.Cube, environment, new Vector3(-12.5f, 1.5f, 0), new Vector3(0.8f, 3f, 24f), wall, SolidLayer);
            Primitive("Obstacle_Crate", PrimitiveType.Cube, environment, new Vector3(4, 0.9f, 0), new Vector3(2f, 1.8f, 2f), copper, SolidLayer);
            Primitive("Obstacle_LongWall", PrimitiveType.Cube, environment, new Vector3(-4, 1.25f, 3), new Vector3(1.4f, 2.5f, 5f), wall, SolidLayer);
            Primitive("Obstacle_Pillar", PrimitiveType.Cylinder, environment, new Vector3(4.5f, 1.5f, 6f), new Vector3(1.8f, 1.5f, 1.8f), copper, SolidLayer);

            Transform gameplay = new GameObject("02_Gameplay").transform;
            Transform spawn = new GameObject("SpawnPoint_Safe").transform;
            spawn.SetParent(gameplay); spawn.position = new Vector3(0f, 0.06f, -5f);
            Primitive("Spawn_Marker", PrimitiveType.Cylinder, environment, new Vector3(0f, 0.02f, -5f), new Vector3(1.5f, 0.02f, 1.5f), blue, FloorLayer, false);

            GameObject target = new GameObject("TrainingTarget_Placeholder");
            target.transform.SetParent(gameplay); target.transform.position = new Vector3(0f, 0.05f, 1.5f); target.layer = TargetLayer;
            CapsuleCollider targetCollider = target.AddComponent<CapsuleCollider>();
            targetCollider.height = 1.9f; targetCollider.radius = 0.55f; targetCollider.center = new Vector3(0f, 0.95f, 0f);
            Health health = target.AddComponent<Health>();
            GameObject targetBody = Primitive("Body_Geometric", PrimitiveType.Capsule, target.transform, new Vector3(0f, 0.95f, 0f), new Vector3(1.1f, 0.95f, 1.1f), orange, TargetLayer, false);
            Primitive("Target_Eye", PrimitiveType.Cube, target.transform, new Vector3(0f, 1.25f, -0.5f), new Vector3(0.7f, 0.15f, 0.12f), cream, TargetLayer, false);
            target.AddComponent<TargetFeedback>().Configure(health, targetBody.GetComponent<Renderer>());

            GameObject cameraObject = new GameObject("03_MainCamera"); cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            if (cameraObject.GetComponent<UniversalAdditionalCameraData>() == null) cameraObject.AddComponent<UniversalAdditionalCameraData>();
            camera.nearClipPlane = 0.1f; camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.035f, 0.055f, 0.09f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(-9f, -13f, 9f));

            GameObject hero = new GameObject("Player_Placeholder"); hero.transform.SetParent(gameplay); hero.layer = PlayerLayer;
            hero.transform.position = spawn.position;
            CharacterController controller = hero.AddComponent<CharacterController>();
            controller.height = 1.8f; controller.radius = 0.4f; controller.center = new Vector3(0f, 0.9f, 0f);
            controller.skinWidth = 0.04f; controller.stepOffset = 0.2f; controller.slopeLimit = 50f; controller.minMoveDistance = 0f;
            InputActionAsset input = ControlsAsset();
            ArenaInput reader = hero.AddComponent<ArenaInput>(); reader.Configure(input);
            PlayerMotor motor = hero.AddComponent<PlayerMotor>(); motor.Configure(reader, camera.transform);
            hero.AddComponent<PlayerAim>().Configure(motor, camera);
            Primitive("Body_Geometric", PrimitiveType.Capsule, hero.transform, new Vector3(0, 0.9f, 0), new Vector3(0.8f, 0.9f, 0.8f), blue, PlayerLayer, false);
            Primitive("Helmet_Geometric", PrimitiveType.Sphere, hero.transform, new Vector3(0, 1.55f, 0.07f), Vector3.one * 0.55f, cream, PlayerLayer, false);
            Primitive("Facing_Visor", PrimitiveType.Cube, hero.transform, new Vector3(0, 1.5f, 0.34f), new Vector3(0.44f, 0.12f, 0.12f), wall, PlayerLayer, false);
            Transform origin = new GameObject("StrikeOrigin").transform; origin.SetParent(hero.transform); origin.localPosition = Vector3.up;
            Transform weaponPivot = new GameObject("WeaponPivot_Geometric").transform; weaponPivot.SetParent(hero.transform); weaponPivot.localPosition = new Vector3(0.4f, 0.8f, 0.2f);
            Primitive("Sword_Geometric", PrimitiveType.Cube, weaponPivot, new Vector3(0, 0, 0.48f), new Vector3(0.14f, 0.14f, 1.2f), gold, PlayerLayer, false);
            Renderer slash = Primitive("AttackIndicator_Geometric", PrimitiveType.Cube, hero.transform, new Vector3(0, 0.14f, 1.25f), new Vector3(1.65f, 0.04f, 0.2f), gold, PlayerLayer, false).GetComponent<Renderer>();
            slash.enabled = false;
            MeleeDefinition melee = AssetDatabase.LoadAssetAtPath<MeleeDefinition>(Root + "Data/TrainingSword.asset");
            if (melee == null) { melee = ScriptableObject.CreateInstance<MeleeDefinition>(); AssetDatabase.CreateAsset(melee, Root + "Data/TrainingSword.asset"); }
            MeleeAttack combat = hero.AddComponent<MeleeAttack>(); combat.Configure(motor, melee, origin, 1 << TargetLayer, SolidMask);
            hero.AddComponent<CombatFeedback>().Configure(combat, weaponPivot, slash);
            ArenaCamera follow = cameraObject.AddComponent<ArenaCamera>(); follow.Configure(hero.transform, SolidMask);
            follow.Snap();
            ArenaSession session = new GameObject("ArenaSession").AddComponent<ArenaSession>();
            session.Configure(motor, spawn, combat, follow, health, SolidMask | (1 << TargetLayer));
            CreateLighting();
            CreateHud(session, health, input);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("ARENA_BUILD_OK: scene, Input System, references and build scene configured.");
        }

        static void ConfigureLayers()
        {
            SerializedObject tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tags.FindProperty("layers");
            string[] names = { "ArenaFloor", "ArenaSolid", "TrainingTarget", "Player" };
            for (int i = 0; i < names.Length; i++)
            {
                string existing = layers.GetArrayElementAtIndex(8 + i).stringValue;
                if (!string.IsNullOrEmpty(existing) && existing != names[i]) throw new InvalidOperationException("Layer slot already in use: " + existing);
                layers.GetArrayElementAtIndex(8 + i).stringValue = names[i];
            }
            tags.ApplyModifiedPropertiesWithoutUndo();
        }
        static void ConfigurePlayer()
        {
            RenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");
            if (pipeline == null) throw new InvalidOperationException("Official URP template PC pipeline asset is missing.");
            GraphicsSettings.defaultRenderPipeline = pipeline;
            PlayerSettings.companyName = "ForjaDasMaresPrototype";
            PlayerSettings.productName = "Forja das Mares - Arena";
            string code = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs/Microsoft VS Code/Code.exe");
            if (File.Exists(code)) EditorPrefs.SetString("kScriptsDefaultApp", code);
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false; PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true; PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.forjadasmares.prototype");
        }
        static Material MaterialAsset(string name, Color color)
        {
            string path = Root + "Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader not available. Resolve URP first.");
            material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", 0.15f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
        static GameObject Primitive(string name, PrimitiveType shape, Transform parent, Vector3 position, Vector3 scale, Material material, int layer, bool collision = true)
        {
            GameObject item = GameObject.CreatePrimitive(shape); item.name = name; item.layer = layer;
            item.transform.SetParent(parent, false); item.transform.localPosition = position; item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision) UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
            return item;
        }
        static InputActionAsset ControlsAsset()
        {
            string path = Root + "Input/ArenaControls.inputactions";
            InputActionAsset existing = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Gameplay"); asset.AddActionMap(map);
            InputAction move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");
            map.AddAction("Point", InputActionType.Value, "<Mouse>/position", expectedControlLayout: "Vector2");
            InputAction attack = map.AddAction("Attack", InputActionType.Button, "<Mouse>/leftButton"); attack.AddBinding("<Gamepad>/buttonSouth");
            InputAction dodge = map.AddAction("Dodge", InputActionType.Button, "<Keyboard>/space"); dodge.AddBinding("<Gamepad>/buttonEast");
            InputAction reset = map.AddAction("Reset", InputActionType.Button, "<Keyboard>/r"); reset.AddBinding("<Gamepad>/start");
            var ui = new InputActionMap("UI"); asset.AddActionMap(ui);
            InputAction point = ui.AddAction("Point", InputActionType.PassThrough, "<Mouse>/position", expectedControlLayout: "Vector2"); point.AddBinding("<Touchscreen>/touch*/position");
            InputAction click = ui.AddAction("Click", InputActionType.PassThrough, "<Mouse>/leftButton", expectedControlLayout: "Button"); click.AddBinding("<Touchscreen>/touch*/press");
            ui.AddAction("Scroll", InputActionType.PassThrough, "<Mouse>/scroll", expectedControlLayout: "Vector2");
            File.WriteAllText(path, asset.ToJson()); UnityEngine.Object.DestroyImmediate(asset);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        }
        static void CreateLighting()
        {
            Light light = new GameObject("04_SunLight").AddComponent<Light>(); light.type = LightType.Directional;
            if (light.GetComponent<UniversalAdditionalLightData>() == null) light.gameObject.AddComponent<UniversalAdditionalLightData>();
            light.intensity = 1.6f; light.shadows = LightShadows.Soft; light.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.42f, 0.5f, 0.58f);
            RenderSettings.fog = false;
        }
        static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var item = new GameObject(name, typeof(RectTransform)); var rect = item.GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = anchor; rect.pivot = anchor;
            rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        static Text TextElement(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, string content, int fontSize)
        {
            Text text = Rect(name, parent, anchor, position, size).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = fontSize;
            text.color = new Color(0.93f, 0.95f, 1f); text.text = content; text.raycastTarget = false;
            text.alignment = TextAnchor.MiddleCenter; text.horizontalOverflow = HorizontalWrapMode.Wrap; return text;
        }
        static void CreateHud(ArenaSession session, Health health, InputActionAsset input)
        {
            Canvas canvas = new GameObject("05_PrototypeHUD", typeof(RectTransform)).AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            TextElement("Title", canvas.transform, new Vector2(0f, 1f), new Vector2(20, -16), new Vector2(260, 36), "FORJA DAS MARÉS · PROTÓTIPO", 15);
            Text label = TextElement("TargetHealthLabel", canvas.transform, new Vector2(0.5f, 1f), new Vector2(0, -18), new Vector2(420, 32), "ALVO DE TREINO", 20);
            Image background = Rect("TargetHealthBackground", canvas.transform, new Vector2(0.5f, 1f), new Vector2(0, -58), new Vector2(320, 16)).gameObject.AddComponent<Image>();
            background.color = new Color(0.07f, 0.09f, 0.14f, 0.95f); background.raycastTarget = false;
            Image fill = Rect("TargetHealthFill", background.transform, Vector2.zero, Vector2.zero, new Vector2(320, 16)).gameObject.AddComponent<Image>();
            fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            fill.color = new Color(1f, 0.48f, 0.2f); fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.raycastTarget = false;
            Text instructions = TextElement("ControlsHelp", canvas.transform, new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(1040, 40), "", 19);
            var resetRect = Rect("ResetArenaButton", canvas.transform, Vector2.one, new Vector2(-20, -20), new Vector2(150, 42));
            Image resetImage = resetRect.gameObject.AddComponent<Image>(); resetImage.color = new Color(0.16f, 0.34f, 0.43f, 0.95f);
            Button resetButton = resetRect.gameObject.AddComponent<Button>(); resetButton.targetGraphic = resetImage;
            UnityEventTools.AddPersistentListener(resetButton.onClick, session.ResetArena);
            TextElement("ResetText", resetRect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140, 40), "REINICIAR", 19);
            GameObject touchPanel = new GameObject("TouchControls_Minimal", typeof(RectTransform)); touchPanel.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = touchPanel.GetComponent<RectTransform>(); panelRect.anchorMin = Vector2.zero; panelRect.anchorMax = Vector2.one; panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            RectTransform stickRect = Rect("TouchMoveStick", panelRect, Vector2.zero, new Vector2(80, 80), new Vector2(150, 150));
            stickRect.pivot = new Vector2(0.5f, 0.5f); stickRect.anchoredPosition = new Vector2(135, 135);
            Image stickImage = stickRect.gameObject.AddComponent<Image>(); stickImage.color = new Color(0.1f, 0.65f, 0.72f, 0.6f);
            OnScreenStick stick = stickRect.gameObject.AddComponent<OnScreenStick>(); stick.controlPath = "<Gamepad>/leftStick"; stick.movementRange = 65f;
            stick.useIsolatedInputActions = true;
            TextElement("MoveLabel", stickRect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140, 45), "MOVER", 19);
            TouchButton("TouchAttack", panelRect, new Vector2(-190, 90), "ATAQUE", "<Gamepad>/buttonSouth");
            TouchButton("TouchDodge", panelRect, new Vector2(-50, 90), "ESQUIVA", "<Gamepad>/buttonEast");
            touchPanel.SetActive(false);
            ArenaHud hud = canvas.gameObject.AddComponent<ArenaHud>(); hud.Configure(health, label, fill); hud.ConfigureTouch(touchPanel, instructions);
            GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            // Virtual gamepad attacks must not submit the previously selected Reset button.
            eventObject.GetComponent<EventSystem>().sendNavigationEvents = false;
            InputSystemUIInputModule module = eventObject.GetComponent<InputSystemUIInputModule>();
            module.actionsAsset = input;
            module.point = ActionReference(input, "UI/Point");
            module.leftClick = ActionReference(input, "UI/Click");
            module.scrollWheel = ActionReference(input, "UI/Scroll");
            module.move = module.submit = module.cancel = module.rightClick = module.middleClick = module.trackedDeviceOrientation = module.trackedDevicePosition = null;
        }
        static InputActionReference ActionReference(InputActionAsset input, string name)
        {
            InputAction action = input.FindAction(name, true);
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(input)))
                if (asset is InputActionReference reference && reference.action != null && reference.action.id == action.id) return reference;
            throw new InvalidOperationException("Persistent InputActionReference not imported: " + name);
        }
        static void TouchButton(string name, Transform parent, Vector2 position, string label, string control)
        {
            RectTransform rect = Rect(name, parent, new Vector2(1f, 0f), position, new Vector2(120, 100));
            Image image = rect.gameObject.AddComponent<Image>(); image.color = new Color(0.15f, 0.35f, 0.44f, 0.85f);
            OnScreenButton button = rect.gameObject.AddComponent<OnScreenButton>(); button.controlPath = control;
            TextElement("Label", rect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(115, 50), label, 19);
        }
    }
}
