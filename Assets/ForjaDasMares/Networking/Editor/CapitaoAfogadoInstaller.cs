using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ForjaDasMares.Networking.Editor
{
    // Boss "Capitão Afogado": the ghost pirate made in Meshy (Mixamo skeleton, one FBX with all its animations).
    // Builds the enemy (model, glowing ghost material, animations, three attacks) and puts him at the end of the
    // Catacomb room: first the two guards, then he arrives.
    public static class CapitaoAfogadoInstaller
    {
        const string Root = "Assets/ForjaDasMares/Art/Inimigos/CapitaoAfogado/";
        public const string ModelPath = Root + "Modelo.fbx";
        const string Data = "Assets/ForjaDasMares/Data/Inimigos/";
        const string HeroClips = "Assets/ForjaDasMares/Art/Personagens/Animacoes/";
        const string CatacombRegion = "Assets/ForjaDasMares/Data/World/Catacombs/Catacombs.asset";
        const float Height = 2.5f;
        const string Marker = "forja-capitao-1";
        static readonly string[] Loops = { "Running", "Walking", "Walking_Woman", "Proud_Strut", "Flirty_Strut_inplace", "Limping_Walk_3", "Male_Head_Down_Charge", "Walk_Backward_While_Shooting_inplace" };

        [MenuItem("Forja/Instalar personagens, animações e chefe (tudo)", priority = 0)]
        public static void All()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ImportModel();
            ExpedicionarioInstaller.Install();
            Build();
        }

        // Everything above, then the phone APK (Builds/Atual/Android), without stopping to ask.
        [MenuItem("Forja/Instalar tudo e gerar APK do celular", priority = 1)]
        public static void AllAndApk()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ImportModel();
            ExpedicionarioInstaller.Install();
            Build();
            AssetDatabase.SaveAssets();
            CampaignInstaller.Android();
        }

        [MenuItem("Forja/Chefe/Instalar Capitão Afogado (na Catacumba)")]
        public static void Install()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        [MenuItem("Forja/Chefe/Testar Capitão Afogado (abre a Catacumba e joga)")]
        public static void Test()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(CatacombPreviewInstaller.ScenePath);
            EditorApplication.EnterPlaymode();
        }

        // Humanoid import, so the hero's Mixamo clips (idle, death) also work on him, and his walks on the hero.
        public static void ImportModel()
        {
            if (!File.Exists(ModelPath) || !(AssetImporter.GetAtPath(ModelPath) is ModelImporter importer)) return;
            if (importer.userData == Marker && importer.animationType == ModelImporterAnimationType.Human) return;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true; importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importCameras = importer.importLights = false; importer.isReadable = false;
            importer.SaveAndReimport();
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                clip.name = clip.takeName.Split('|').Last();
                clip.loopTime = Loops.Contains(clip.name); clip.loopPose = false;
                clip.lockRootRotation = false; clip.lockRootPositionXZ = false;
                clip.lockRootHeightY = true; clip.keepOriginalPositionY = true;
            }
            importer.clipAnimations = clips;
            importer.userData = Marker;
            importer.SaveAndReimport();
        }

        public static AnimationClip Take(string name) =>
            File.Exists(ModelPath) ? AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().FirstOrDefault(c => c.name == name) : null;

        static void Build()
        {
            if (!File.Exists(ModelPath) || !File.Exists(Root + "Textura.png")) { Debug.LogError("Capitão Afogado: faltando " + ModelPath + " ou Textura.png"); return; }
            if (!File.Exists(HeroClips + "Parado.fbx")) ExpedicionarioInstaller.Install();
            ImportModel();
            EnsureFolder(Data.TrimEnd('/'));

            var template = AssetDatabase.FindAssets("t:EnemyDefinition").Select(g => AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(d => d != null && d.kind == EnemyKind.Boss && !d.miniboss && d.boss != null && d.attack != null && d.prefab != null && d.prefab.GetComponent<EnemyPresentation>() != null && !AssetDatabase.GetAssetPath(d).StartsWith(Data))
                .FirstOrDefault();
            if (template == null) { Debug.LogError("Capitão Afogado: nenhum chefe antigo para servir de base (EnemyDefinition kind=Boss)."); return; }

            string baseName = template.name; // the asset reference dies when the Catacomb scene opens
            var material = GhostMaterial();
            var controller = Controller(out float[] lengths);
            var data = Definition(template);
            data.prefab = Prefab(template.prefab, material, controller, lengths);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            string where = PlaceInCatacomb(data);
            Debug.Log("CAPITAO_AFOGADO_PRONTO · base: " + baseName + " · " + where);
        }

        // ---------- Look ----------
        static Material GhostMaterial()
        {
            string png = Root + "Textura.png";
            ImportTexture(png, true);
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            source.LoadImage(File.ReadAllBytes(png));
            var pixels = source.GetPixels(); var glow = new Color[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color.RGBToHSV(pixels[i], out float h, out float s, out float v);
                // The sea-green ghost parts glow; everything else gets a little brighter and livelier.
                bool ghost = h > .40f && h < .56f && s > .30f && v > .40f;
                glow[i] = ghost ? Color.HSVToRGB(h, Mathf.Clamp01(s * 1.1f), Mathf.Clamp01(v * 1.1f)) : Color.black;
                var c = Color.HSVToRGB(h, Mathf.Clamp01(s * 1.18f), Mathf.Clamp01(Mathf.Pow(v, .8f) * 1.08f)); c.a = 1;
                pixels[i] = c;
            }
            Write(Root + "TexturaViva.png", source.width, source.height, pixels);
            Write(Root + "Brilho.png", source.width, source.height, glow);
            Object.DestroyImmediate(source);

            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "TexturaViva.png")); m.SetColor("_BaseColor", Color.white);
            m.SetFloat("_Smoothness", .3f); m.SetFloat("_Metallic", 0);
            m.EnableKeyword("_EMISSION"); m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            m.SetTexture("_EmissionMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Brilho.png"));
            m.SetColor("_EmissionColor", new Color(.55f, 1.05f, .95f));
            m.enableInstancing = true;
            return Save(m, Root + "Material.mat");
        }

        static void Write(string path, int width, int height, Color[] pixels)
        {
            var t = new Texture2D(width, height, TextureFormat.RGBA32, false);
            t.SetPixels(pixels); t.Apply();
            File.WriteAllBytes(path, t.EncodeToPNG());
            Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            ImportTexture(path, true);
        }

        static void ImportTexture(string path, bool color)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) return;
            importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Compressed;
            importer.sRGBTexture = color; importer.mipmapEnabled = true; importer.SaveAndReimport();
        }

        // ---------- Animations ----------
        // Same state names as the other rigged enemies (Idle, Walk, Run, Attack, Hit, Dead) plus Charge and Curse.
        static AnimatorController Controller(out float[] lengths)
        {
            AnimationClip Hero(string file) => ExpedicionarioInstaller.Clip(File.Exists(HeroClips + file + ".fbx") ? HeroClips + file + ".fbx" : null);
            var map = new (string state, AnimationClip clip)[]
            {
                ("Idle", Hero("Parado") ?? Take("Proud_Strut")),
                ("Walk", Take("Proud_Strut") ?? Take("Walking")),   // the pompous captain's walk
                ("Run", Take("Running")),
                ("Attack", Take("Attack")),
                ("Hit", Hero("Dano") ?? Take("Attack")),
                ("Dead", Hero("Morrer") ?? Take("Kneeling_Reload")),
                ("Charge", Take("Male_Head_Down_Charge") ?? Take("Running")),
                ("Curse", Take("Wave_for_Help_3") ?? Take("Attack")),   // arms up: calls the tide marks
            };
            string path = Root + "CapitaoAfogado.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path) ?? AnimatorController.CreateAnimatorControllerAtPath(path);
            var machine = controller.layers[0].stateMachine;
            foreach (var old in machine.states.ToArray()) machine.RemoveState(old.state);
            lengths = new float[map.Length];
            for (int i = 0; i < map.Length; i++)
            {
                var state = machine.AddState(map[i].state, new Vector3(250 * (i % 4), 90 * (i / 4), 0));
                state.motion = map[i].clip; state.writeDefaultValues = true;
                lengths[i] = map[i].clip != null ? map[i].clip.length : 0;
                if (i == 0) machine.defaultState = state;
            }
            EditorUtility.SetDirty(controller);
            return controller;
        }

        // ---------- Enemy data ----------
        static EnemyDefinition Definition(EnemyDefinition template)
        {
            var data = Save(Object.Instantiate(template), Data + "CapitaoAfogado.asset");
            data.enemyId = "capitao-afogado"; data.displayName = "Capitão Afogado"; data.kind = EnemyKind.Boss; data.miniboss = false;
            data.maximumHealth = 600; data.armor = .1f; data.poolCapacity = 1; data.spawnWarning = 1.4f;
            data.height = Height; data.radius = .5f; data.agentTypeId = 0; data.speed = 3.1f; data.acceleration = 20;
            data.detectionRange = 18; data.forgetRange = 30; data.knockbackResistance = .9f; data.staggerDuration = 0;
            data.color = Color.white;
            data.attack = Hit(template.attack, "Golpe", 10, 3.2f, 140);

            var pattern = Save(Object.Instantiate(template.boss), Data + "CapitaoAfogado_Padrao.asset");
            pattern.phaseTwoThreshold = .5f; pattern.phaseTwoTiming = .85f; pattern.phaseTwoDamage = 1.15f; pattern.phaseTwoRadius = 1.15f;
            var teal = new Color(.25f, 1f, .85f);
            pattern.attacks = new[]
            {
                new BossAttack { displayName = "Golpe do Capitão", kind = BossAttackKind.Sweep, hit = data.attack, warning = .9f, recovery = .8f, cooldown = 1.4f, range = 3.6f, radius = 3.4f, arc = 150, warningColor = new Color(1, .45f, .1f) },
                new BossAttack { displayName = "Investida Fantasma", kind = BossAttackKind.Charge, hit = Hit(template.attack, "Investida", 14, 1.4f, 180), warning = 1.0f, recovery = .9f, cooldown = 3.2f, range = 11, radius = 1.4f, length = 9, chargeSpeed = 11, warningColor = teal },
                new BossAttack { displayName = "Maldição das Marés", kind = BossAttackKind.GroundMarks, hit = Hit(template.attack, "Maldicao", 9, 1.7f, 360), warning = 1.2f, recovery = 1.0f, cooldown = 3.8f, range = 12, radius = 1.7f, warningColor = teal },
            };
            EditorUtility.SetDirty(pattern);
            data.boss = pattern;
            return data;
        }

        static AttackDefinition Hit(AttackDefinition template, string name, int damage, float range, float arc)
        {
            var hit = Save(Object.Instantiate(template), Data + "CapitaoAfogado_" + name + ".asset");
            hit.shape = AttackShape.Arc; hit.damage = damage; hit.range = range; hit.arc = arc; hit.knockback = 3; hit.color = new Color(.3f, 1, .85f);
            EditorUtility.SetDirty(hit);
            return hit;
        }

        // ---------- Prefab ----------
        static GameObject Prefab(GameObject templatePrefab, Material material, AnimatorController controller, float[] lengths)
        {
            var root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(templatePrefab));
            try
            {
                root.name = "CapitaoAfogado";
                root.transform.position = Vector3.zero; root.transform.rotation = Quaternion.identity;
                var view = root.GetComponent<EnemyPresentation>();
                var so = new SerializedObject(view);
                if (so.FindProperty("model").objectReferenceValue is Transform old && old != null) Object.DestroyImmediate(old.gameObject);
                var shield = so.FindProperty("shield").objectReferenceValue as GameObject;
                if (shield != null) Object.DestroyImmediate(shield);

                var visual = new GameObject("CapitaoVisual");
                visual.transform.SetParent(root.transform, false);
                var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath));
                model.transform.SetParent(visual.transform, false);
                model.name = "Modelo";
                var renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var r in renderers)
                {
                    r.sharedMaterials = Enumerable.Repeat(material, Mathf.Max(1, r.sharedMaterials.Length)).ToArray();
                    r.updateWhenOffscreen = false; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
                var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller; animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

                // Face +Z (the toes say where the front is), Height metres tall, feet on the floor.
                var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot); var toes = animator.GetBoneTransform(HumanBodyBones.LeftToes);
                if (foot != null && toes != null)
                {
                    Vector3 front = toes.position - foot.position; front.y = 0;
                    if (front.sqrMagnitude > .00001f) model.transform.rotation = Quaternion.FromToRotation(front.normalized, Vector3.forward) * model.transform.rotation;
                }
                var bounds = Bounds(model);
                if (bounds.size.y > .01f) model.transform.localScale *= Height / bounds.size.y;
                bounds = Bounds(model);
                model.transform.position += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);

                // Sea-green ghost light around him (lights up the dark catacomb as he walks).
                var light = new GameObject("BrilhoFantasma").AddComponent<Light>();
                light.transform.SetParent(visual.transform, false); light.transform.localPosition = new Vector3(0, Height * .6f, .4f);
                light.type = LightType.Point; light.range = 6; light.intensity = 1.6f; light.color = new Color(.3f, 1, .85f); light.shadows = LightShadows.None;

                var motion = visual.AddComponent<RiggedEnemyAnimation>();
                motion.Configure(animator);
                motion.ConfigureBoss(lengths, renderers.Cast<Renderer>().ToArray());
                view.Configure(root.GetComponent<EnemyActor>(), visual.transform, System.Array.Empty<Renderer>(), null);
                EnsureFolder(Root.TrimEnd('/'));
                return PrefabUtility.SaveAsPrefabAsset(root, Root + "CapitaoAfogado.prefab");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        // ---------- Catacomb ----------
        // Room 1 of the Catacomb: two guards, then the Captain (spawn at the far end of the room).
        static string PlaceInCatacomb(EnemyDefinition boss)
        {
            var region = AssetDatabase.LoadAssetAtPath<RegionDefinition>(CatacombRegion);
            if (region == null || region.camps == null || region.camps.Length == 0) return "Catacumba não encontrada (rode o instalador da Catacumba).";
            var camp = region.camps[0];
            var guards = camp.enemies.Where(e => e != null && e.kind != EnemyKind.Boss).Take(2).ToList();
            while (guards.Count < 2 && guards.Count > 0) guards.Add(guards[0]);
            if (guards.Count == 0) return "Catacumba sem guardas.";
            var spawns = camp.spawns.ToList();
            while (spawns.Count < 2) spawns.Add(camp.center);
            camp.enemies = new[] { guards[0], guards[1], boss };
            camp.spawns = new[] { spawns[0], spawns[1], camp.center + new Vector3(0, .06f, 4) };
            EditorUtility.SetDirty(region);

            var scene = EditorSceneManager.OpenScene(CatacombPreviewInstaller.ScenePath);
            var pool = Object.FindFirstObjectByType<EnemyPool>();
            if (pool == null) return "Catacumba sem EnemyPool.";
            var so = new SerializedObject(pool);
            var list = so.FindProperty("definitions");
            bool present = false;
            for (int i = 0; i < list.arraySize; i++) if (list.GetArrayElementAtIndex(i).objectReferenceValue == boss) present = true;
            if (!present) { list.arraySize++; list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = boss; }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            return "na Catacumba, sala 1, depois dos 2 guardas";
        }

        // ---------- Helpers ----------
        static Bounds Bounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static T Save<T>(T asset, string path) where T : Object
        {
            var saved = AssetDatabase.LoadAssetAtPath<T>(path);
            if (saved == null) { AssetDatabase.CreateAsset(asset, path); return asset; }
            EditorUtility.CopySerialized(asset, saved);
            Object.DestroyImmediate(asset);
            EditorUtility.SetDirty(saved);
            return saved;
        }
    }
}
