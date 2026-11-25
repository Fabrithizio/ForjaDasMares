using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ForjaDasMares.Networking.Editor
{
    // The two starting characters (Expedicionário / Expedicionária) made in Meshy with the Mixamo skeleton.
    // Imports them as Humanoid (so every Mixamo animation works on both and on future skins), fixes hair that
    // was skinned to the arms, builds one Animator for both and saves ready prefabs for the menu and the game.
    // Animations: Mixamo FBX files with Portuguese names in Art/Personagens/Animacoes (table "Map" below).
    public static class ExpedicionarioInstaller
    {
        const string Root = "Assets/ForjaDasMares/Art/Personagens/";
        const string Animations = Root + "Animacoes/";
        const string ControllerPath = Root + "Expedicionario.controller";
        const string ResourcesFolder = "Assets/ForjaDasMares/Resources/Personagens/";
        public static readonly string[] Genders = { "Masculino", "Feminino" };
        static readonly float[] Heights = { 1.75f, 1.65f };

        [MenuItem("Forja/Personagem/Instalar personagens iniciais")]
        public static void Install()
        {
            EnsureFolder(Root.TrimEnd('/')); EnsureFolder(Animations.TrimEnd('/'));
            EnsureFolder("Assets/ForjaDasMares/Resources"); EnsureFolder(ResourcesFolder.TrimEnd('/'));
            RemoveOldClips();
            // 1. Bodies (Humanoid), so the animations can be measured on them.
            for (int g = 0; g < Genders.Length; g++)
            {
                string folder = Root + Genders[g] + "/";
                string fbx = folder + "Modelo.fbx", png = folder + "Textura.png";
                if (!File.Exists(fbx) || !File.Exists(png)) { Debug.LogError("Personagem: faltando " + fbx + " ou " + png); continue; }
                if (AssetImporter.GetAtPath(png) is TextureImporter texture)
                {
                    texture.maxTextureSize = 2048; texture.textureCompression = TextureImporterCompression.Compressed;
                    texture.sRGBTexture = true; texture.mipmapEnabled = true; texture.SaveAndReimport();
                }
                if (AssetImporter.GetAtPath(fbx) is ModelImporter model)
                {
                    model.animationType = ModelImporterAnimationType.Human;
                    model.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    model.importAnimation = false; model.materialImportMode = ModelImporterMaterialImportMode.None;
                    model.importCameras = model.importLights = false; model.isReadable = true;
                    model.SaveAndReimport();
                }
            }
            // 2. Animations, cut around the moment of each throw, and the walks from the pirate captain's pack.
            var report = new System.Text.StringBuilder("PERSONAGENS_PRONTOS ·");
            var files = ImportAnimations();
            ImportarCapitao();
            // Walk: our own Mixamo walk if there is one (Andar.fbx or any "Walking"), otherwise the normal run, slowed.
            // Run: the fast run, which matches the hero's real speed (the normal run made the legs spin too fast).
            string ownWalk = files.Keys.FirstOrDefault(k => k == "Andar") ?? files.Keys.FirstOrDefault(k => { var l = k.ToLowerInvariant(); return l.Contains("walk") && !l.Contains("pole") && !l.Contains("turn") && !l.Contains("dying") && !l.Contains("back"); });
            AnimationClip walk = ownWalk != null ? Clip(files[ownWalk]) : Clip(files.TryGetValue("Correr", out var jog) ? jog : null);
            AnimationClip femaleWalk = files.TryGetValue("Andar_Feminino", out var fw) ? Clip(fw) : walk;
            var maleSource = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Masculino/Modelo.fbx");
            if (maleSource != null)
            {
                var rig = Object.Instantiate(maleSource);
                try { TrimActions(files, rig, report); }
                finally { Object.DestroyImmediate(rig); }
            }
            var controller = BuildController(files, walk);
            var female = FemaleController(controller, walk, femaleWalk);
            report.Append(" | clipes: ").Append(files.Count).Append(ownWalk != null ? " | andar: " + ownWalk : " | andar: Correr devagar (baixe Walking no Mixamo)").Append(" | correr: ").Append(RunFile(files));
            // 3. Ready characters.
            for (int g = 0; g < Genders.Length; g++)
            {
                string folder = Root + Genders[g] + "/";
                string fbx = folder + "Modelo.fbx", png = folder + "Textura.png";
                if (!File.Exists(fbx) || !File.Exists(png)) continue;
                // Livelier colours to match the bright, warm city (the Meshy texture came out dark and dull).
                string vivid = Vivid(png, folder + "TexturaViva.png");
                var material = Save(Material(AssetDatabase.LoadAssetAtPath<Texture2D>(vivid)), folder + "Material.mat");
                // Stride of this body with its own walk and run, so the feet keep pace with the ground.
                float andar = .95f, correr = 2.45f;
                var source = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
                var rig = Object.Instantiate(source);
                try
                {
                    var b = Bounds(rig);
                    float scale = b.size.y > .01f ? Heights[g] / b.size.y : 1;
                    var runClip = Clip(files.TryGetValue(RunFile(files), out var r) ? r : null);
                    correr = Mathf.Clamp(Stride(rig, runClip, scale, correr), 1.2f, 5f);
                    var walkClip = g == 1 ? femaleWalk : walk;
                    andar = walkClip != null ? Mathf.Clamp(Stride(rig, walkClip, scale, andar), .6f, 2.4f) : correr * .55f;
                }
                finally { Object.DestroyImmediate(rig); }
                int fixedVertices;
                var prefab = BuildPrefab(fbx, folder, material, g == 1 ? female : controller, Heights[g], Genders[g], andar, correr, out fixedVertices);
                report.Append(" | ").Append(Genders[g]).Append(prefab != null ? " ok" : " ERRO").Append(" passada ").Append(andar.ToString("0.00")).Append("/").Append(correr.ToString("0.00")).Append(" (cabelo corrigido: ").Append(fixedVertices).Append(" pontos)");
            }
            InstallWeaponLocomotion();
            AssetDatabase.SaveAssets();
            Debug.Log(report.ToString());
        }

        // Standing, walking and running with the weapon in the hand (Mixamo packs "Sword and Shield" and "Pro Magic").
        // Files: Animacoes/Espada/Espada_Parado|Andar|Correr.fbx and Animacoes/Cajado/Cajado_Parado|Andar|Correr.fbx.
        // Result: Resources/Personagens/LocomocaoPorArma.asset, used by the hero's body while the weapon is equipped.
        // The walks of the pirate captain's pack (Meshy, Mixamo skeleton) are used by the hero: import it as Humanoid.
        const string CapitaoModelo = "Assets/ForjaDasMares/Art/Inimigos/CapitaoAfogado/Modelo.fbx", CapitaoMarca = "forja-capitao-1";
        static readonly string[] CapitaoLoops = { "Running", "Walking", "Walking_Woman", "Proud_Strut", "Flirty_Strut_inplace", "Limping_Walk_3", "Male_Head_Down_Charge", "Walk_Backward_While_Shooting_inplace" };
        static void ImportarCapitao()
        {
            if (!File.Exists(CapitaoModelo) || !(AssetImporter.GetAtPath(CapitaoModelo) is ModelImporter importer)) return;
            if (importer.userData == CapitaoMarca && importer.animationType == ModelImporterAnimationType.Human) return;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true; importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importCameras = importer.importLights = false; importer.isReadable = false;
            importer.SaveAndReimport();
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                clip.name = clip.takeName.Split('|').Last();
                clip.loopTime = CapitaoLoops.Contains(clip.name); clip.loopPose = false;
                clip.lockRootRotation = false; clip.lockRootPositionXZ = false;
                clip.lockRootHeightY = true; clip.keepOriginalPositionY = true;
            }
            importer.clipAnimations = clips;
            importer.userData = CapitaoMarca;
            importer.SaveAndReimport();
        }

        [MenuItem("Forja/Personagem/Animações por arma (Espada e Cajado)")]
        public static void InstallWeaponLocomotion()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null) { Debug.LogError("Rode antes: Forja/Personagem/Instalar personagens iniciais"); return; }
            AnimationClip Base(string state) => controller.layers[0].stateMachine.states.Select(s => s.state).FirstOrDefault(s => s.name == state)?.motion as AnimationClip;
            EnsureFolder("Assets/ForjaDasMares/Resources"); EnsureFolder(ResourcesFolder.TrimEnd('/'));
            string path = ResourcesFolder + "LocomocaoPorArma.asset";
            var set = AssetDatabase.LoadAssetAtPath<LocomocaoPorArma>(path);
            if (set == null) { set = ScriptableObject.CreateInstance<LocomocaoPorArma>(); AssetDatabase.CreateAsset(set, path); }
            set.baseParado = Base("Idle"); set.baseParadoOlhando = Base("Idle2"); set.baseAndar = Base("Walk"); set.baseCorrer = Base("Run");
            set.baseAtaque1 = Base("Attack"); set.baseAtaque2 = Base("Attack2"); set.baseAtaque3 = Base("Attack3");
            set.baseForte = Base("Charged"); set.baseSuper = Base("Super"); set.baseHabilidade = Base("Skill"); set.baseDano = Base("Hit");
            var female = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(Root + "Feminino/Animador.overrideController");
            set.baseAndarFeminino = female != null && set.baseAndar != null ? female[set.baseAndar] : null;
            var report = new System.Text.StringBuilder("ANIMACOES_POR_ARMA ·");
            set.espada = WeaponMoves("Espada", report);
            set.cajado = WeaponMoves("Cajado", report);
            FightMoves(set, report);
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            Debug.Log(report.ToString());
        }

        // How each weapon fights (chosen from the pictures of "Ver golpes novos da espada", 24/11):
        // sword = Mixamo "Pro Sword And Shield": quick slash, backhand, overhead finish; jump smash when charged;
        // spinning blow for the Super; sword raised then brought down for abilities; its own hit reaction.
        // staff = the "Pro Magic" casts: three one-hand casts in the combo, the two-hand blast when charged.
        // Each clip is cut around its fastest moment, so the hit lands when the game hits (like the base moves).
        static readonly (string arquivo, float contato)[] GolpesDaEspada =
        {
            ("Espada_Golpe_1", .3f), ("Espada_Golpe_3", .3f), ("Espada_Ataque_4", .3f), ("Espada_Ataque_1", .5f), ("Espada_Golpe_4", .5f), ("Espada_Magia_1", .5f), ("Espada_Dano", 0),
        };
        static void FightMoves(LocomocaoPorArma set, System.Text.StringBuilder report)
        {
            var maleSource = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Masculino/Modelo.fbx");
            if (maleSource == null) return;
            var rig = Object.Instantiate(maleSource);
            try
            {
                AnimationClip Golpe(string folder, string file, float contact)
                {
                    string path = folder + file + ".fbx";
                    if (!File.Exists(path) || !(AssetImporter.GetAtPath(path) is ModelImporter importer)) { report.Append(" faltando ").Append(file); return null; }
                    importer.animationType = ModelImporterAnimationType.Human;
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    importer.importAnimation = true; importer.materialImportMode = ModelImporterMaterialImportMode.None;
                    importer.importCameras = importer.importLights = false;
                    importer.SaveAndReimport();
                    SetClip(importer, file, false, -1, -1);
                    var clip = Clip(path);
                    if (clip == null || contact <= 0) return clip;
                    float peak = Peak(Sample(rig, clip));
                    if (peak < 0) return clip;
                    float before = contact < .4f ? .45f : .75f, after = contact < .4f ? 1.05f : .75f;
                    float first = Mathf.Max(0, peak - before), last = Mathf.Min(clip.length, peak + after);
                    SetClip(importer, file, false, first, last);
                    report.Append(" ").Append(file).Append(" ").Append(first.ToString("0.00")).Append("-").Append(last.ToString("0.00")).Append("s");
                    return Clip(path);
                }
                if (set.espada != null)
                {
                    string golpes = Animations + "Espada/Golpes/";
                    var g = GolpesDaEspada.Select(x => Golpe(golpes, x.arquivo, x.contato)).ToArray();
                    set.espada.ataque1 = g[0]; set.espada.ataque2 = g[1]; set.espada.ataque3 = g[2];
                    set.espada.forte = g[3]; set.espada.super = g[4]; set.espada.habilidade = g[5]; set.espada.dano = g[6];
                }
                if (set.cajado != null)
                {
                    // The base combo starts with a throw (the boomerang's move): the staff casts instead.
                    set.cajado.ataque1 = set.baseAtaque2; set.cajado.ataque2 = set.baseAtaque3;
                    set.cajado.ataque3 = Golpe(Animations, "Ataque_Magico_3", .3f); // the 3rd blow hits at 30% too (HeroModelView)
                }
            }
            finally { Object.DestroyImmediate(rig); }
        }

        static LocomocaoPorArma.Jeito WeaponMoves(string nome, System.Text.StringBuilder report)
        {
            string folder = Animations + nome + "/";
            var moves = new LocomocaoPorArma.Jeito { nome = nome };
            AnimationClip Import(string kind)
            {
                string file = folder + nome + "_" + kind + ".fbx";
                if (!File.Exists(file) || !(AssetImporter.GetAtPath(file) is ModelImporter importer)) { report.Append(" faltando ").Append(file); return null; }
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true; importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importCameras = importer.importLights = false;
                importer.SaveAndReimport();
                SetClip(importer, nome + "_" + kind, true, -1, -1);
                return Clip(file);
            }
            moves.parado = Import("Parado"); moves.andar = Import("Andar"); moves.correr = Import("Correr");
            for (int g = 0; g < Genders.Length; g++)
            {
                var source = AssetDatabase.LoadAssetAtPath<GameObject>(Root + Genders[g] + "/Modelo.fbx");
                if (source == null) continue;
                var rig = Object.Instantiate(source);
                try
                {
                    var b = Bounds(rig);
                    float scale = b.size.y > .01f ? Heights[g] / b.size.y : 1;
                    moves.passadaAndar[g] = moves.andar != null ? Mathf.Clamp(Stride(rig, moves.andar, scale, .95f), .6f, 2.4f) : .95f;
                    moves.passadaCorrer[g] = moves.correr != null ? Mathf.Clamp(Stride(rig, moves.correr, scale, 2.45f), 1.2f, 5f) : 2.45f;
                }
                finally { Object.DestroyImmediate(rig); }
            }
            report.Append(" | ").Append(nome).Append(moves.parado != null && moves.andar != null && moves.correr != null ? " ok" : " INCOMPLETO")
                .Append(" passada ").Append(moves.passadaAndar[0].ToString("0.00")).Append("/").Append(moves.passadaCorrer[0].ToString("0.00"));
            return moves;
        }

        static string Vivid(string source, string target)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(source));
            var pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color.RGBToHSV(pixels[i], out float h, out float sat, out float val);
                sat = Mathf.Clamp01(sat * 1.28f);
                val = Mathf.Clamp01(Mathf.Pow(val, .82f) * 1.06f);
                var c = Color.HSVToRGB(h, sat, val); c.a = pixels[i].a;
                pixels[i] = c;
            }
            texture.SetPixels(pixels); texture.Apply();
            File.WriteAllBytes(target, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(target) is TextureImporter importer)
            {
                importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Compressed;
                importer.sRGBTexture = true; importer.mipmapEnabled = true; importer.SaveAndReimport();
            }
            return target;
        }

        static Material Material(Texture2D map)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetTexture("_BaseMap", map); m.SetColor("_BaseColor", Color.white);
            m.SetFloat("_Smoothness", .25f); m.SetFloat("_Metallic", 0); m.enableInstancing = true;
            return m;
        }

        // ---------- Animations ----------
        // Each file in Art/Personagens/Animacoes has a Portuguese name; the table says which game state uses it.
        static readonly string[] LoopFiles = { "Parado", "Parado_Olhando", "Correr", "Correr_Rapido", "Andar", "Andar_Feminino", "Vitoria", "Acenar", "Sentado" };
        // state, file, contact point (fraction of the action where the hit/throw happens; 0 = whole clip)
        static readonly (string state, string file, float contact)[] Map =
        {
            ("Idle", "Parado", 0), ("Idle2", "Parado_Olhando", 0), ("Walk", "*andar", 0), ("Run", "*correr", 0),
            ("Attack", "Arremessar", .3f), ("Attack2", "Ataque_Magico_1", .3f), ("Attack3", "Ataque_Magico_2", .3f),
            ("Charged", "Ataque_Forte", .5f), ("Super", "Ataque_Area", .5f), ("Skill", "Magia", .5f),
            ("Dodge", "Rolar", 0), ("Hit", "Dano", 0), ("Dead", "Morrer", 0), ("Victory", "Vitoria", 0),
            // Extras for missions and emotes (not used by combat yet).
            ("Acenar", "Acenar", 0), ("Pular", "Pular", 0), ("Sentado", "Sentado", 0), ("Coletar", "Coletar", 0), ("Conjurar", "Conjurar", 0),
        };

        static void RemoveOldClips()
        {
            foreach (var old in new[] { "Running_Reload.fbx", "Idle_Turn_Right.fbx" })
                if (File.Exists(Animations + old)) AssetDatabase.DeleteAsset(Animations + old);
        }

        // Imports every FBX of the folder as Humanoid (works for any Mixamo/Meshy skeleton), in place.
        static Dictionary<string, string> ImportAnimations()
        {
            var files = new Dictionary<string, string>();
            foreach (var path in Directory.GetFiles(Animations, "*.fbx").Select(p => p.Replace('\\', '/')).OrderBy(p => p))
            {
                if (!(AssetImporter.GetAtPath(path) is ModelImporter importer)) continue;
                string name = Path.GetFileNameWithoutExtension(path);
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true; importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importCameras = importer.importLights = false;
                importer.SaveAndReimport();
                var lower = name.ToLowerInvariant();
                bool loop = LoopFiles.Contains(name) || lower.Contains("idle") || lower.Contains("walk") || lower.Contains("run");
                SetClip(importer, name, loop, -1, -1);
                files[name] = path;
            }
            return files;
        }

        // One clip per file, named like the file. first/last in seconds (-1 = whole take).
        public static void SetClip(ModelImporter importer, string name, bool loop, float first, float last)
        {
            var settings = importer.defaultClipAnimations;
            if (settings.Length == 0) return;
            var clip = settings[0];
            clip.name = name; clip.loopTime = loop; clip.loopPose = false;
            // In place: the clip's own travel and turning go to the root and are thrown away (root motion off).
            // Facing as authored (Mixamo faces forward): rotation baked into the pose, based on the original.
            // Without this the body leans its hips sideways and looks like it runs crabwise.
            clip.lockRootRotation = true; clip.keepOriginalOrientation = true; clip.rotationOffset = 0;
            clip.lockRootPositionXZ = false;
            clip.lockRootHeightY = true; clip.keepOriginalPositionY = true;
            float rate = 30;
            var probe = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__"));
            if (probe != null && probe.frameRate > 1) rate = probe.frameRate;
            float start = clip.firstFrame;
            if (last > 0) clip.lastFrame = Mathf.Min(clip.lastFrame, start + last * rate);
            if (first >= 0) clip.firstFrame = Mathf.Min(clip.lastFrame - 1, start + first * rate);
            importer.clipAnimations = new[] { clip };
            importer.SaveAndReimport();
        }

        public static AnimationClip Clip(string path) =>
            path == null ? null : AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__"));

        // Samples a clip on the real body and returns, per frame: hands (relative to hips), feet gap and hip height.
        struct Amostra { public float time; public Vector3 left, right; public float feetGap, hipHeight; }
        static List<Amostra> Sample(GameObject rig, AnimationClip clip)
        {
            var list = new List<Amostra>();
            var animator = rig.GetComponentInChildren<Animator>();
            if (animator == null || clip == null || !animator.isHuman) return list;
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            var lh = animator.GetBoneTransform(HumanBodyBones.LeftHand); var rh = animator.GetBoneTransform(HumanBodyBones.RightHand);
            var lf = animator.GetBoneTransform(HumanBodyBones.LeftFoot); var rf = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            if (hips == null || lh == null || rh == null || lf == null || rf == null) return list;
            bool started = !AnimationMode.InAnimationMode();
            if (started) AnimationMode.StartAnimationMode();
            try
            {
                int frames = Mathf.Max(2, Mathf.CeilToInt(clip.length * 30));
                for (int i = 0; i <= frames; i++)
                {
                    float t = clip.length * i / frames;
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(animator.gameObject, clip, t);
                    AnimationMode.EndSampling();
                    Vector3 gap = lf.position - rf.position; gap.y = 0;
                    list.Add(new Amostra { time = t, left = lh.position - hips.position, right = rh.position - hips.position, feetGap = gap.magnitude, hipHeight = hips.position.y - Mathf.Min(lf.position.y, rf.position.y) });
                }
            }
            finally { if (started) AnimationMode.StopAnimationMode(); }
            return list;
        }

        // Moment of the throw/cast: when a hand moves fastest.
        static float Peak(List<Amostra> poses)
        {
            float best = 0, at = -1;
            for (int i = 1; i < poses.Count; i++)
            {
                float dt = Mathf.Max(.001f, poses[i].time - poses[i - 1].time);
                float speed = Mathf.Max((poses[i].left - poses[i - 1].left).magnitude, (poses[i].right - poses[i - 1].right).magnitude) / dt;
                if (speed > best) { best = speed; at = poses[i].time; }
            }
            return at;
        }

        // Cuts long Mixamo clips around the moment that matters, so the throw lands when the game hits.
        static void TrimActions(Dictionary<string, string> files, GameObject rig, System.Text.StringBuilder report)
        {
            foreach (var (state, file, contact) in Map)
            {
                if (!files.TryGetValue(file, out var path)) continue;
                var importer = (ModelImporter)AssetImporter.GetAtPath(path);
                bool loop = LoopFiles.Contains(file);
                SetClip(importer, file, loop, -1, -1); // measure on the whole take
                var clip = Clip(path);
                if (clip == null) continue;
                if (contact > 0)
                {
                    float peak = Peak(Sample(rig, clip));
                    if (peak < 0) continue;
                    float before = contact < .4f ? .45f : .75f, after = contact < .4f ? 1.05f : .75f;
                    float first = Mathf.Max(0, peak - before), last = Mathf.Min(clip.length, peak + after);
                    SetClip(importer, file, false, first, last);
                    report.Append(" ").Append(file).Append(" ").Append(first.ToString("0.00")).Append("-").Append(last.ToString("0.00")).Append("s");
                }
                else if (state == "Dodge")
                {
                    // "Sprinting Forward Roll" sprints first: keep only the roll (while the hips are low).
                    var poses = Sample(rig, clip);
                    if (poses.Count < 3) continue;
                    float top = poses.Max(p => p.hipHeight), from = -1, to = -1;
                    foreach (var p in poses) if (p.hipHeight < top * .72f) { if (from < 0) from = p.time; to = p.time; }
                    if (from >= 0)
                    {
                        float first = Mathf.Max(0, from - .12f), last = Mathf.Min(clip.length, to + .22f);
                        SetClip(importer, file, false, first, last);
                        report.Append(" Rolar ").Append(first.ToString("0.00")).Append("-").Append(last.ToString("0.00")).Append("s");
                    }
                }
                else if (state == "Dead")
                {
                    // "Walking To Dying" walks first: start just before the body begins to fall.
                    var poses = Sample(rig, clip);
                    if (poses.Count < 3) continue;
                    float standing = poses[0].hipHeight, start = 0;
                    foreach (var p in poses) if (p.hipHeight < standing * .85f) { start = Mathf.Max(0, p.time - .55f); break; }
                    if (start > 0) { SetClip(importer, file, false, start, -1); report.Append(" Morrer desde ").Append(start.ToString("0.00")).Append("s"); }
                }
            }
        }

        // Metres travelled per loop: two steps, each as long as the widest gap between the feet.
        static string RunFile(Dictionary<string, string> files) => files.ContainsKey("Correr_Rapido") ? "Correr_Rapido" : "Correr";

        static float Stride(GameObject rig, AnimationClip clip, float scale, float fallback)
        {
            // Best: the distance the original Mixamo motion travels in one loop (its root motion).
            var animator = rig.GetComponentInChildren<Animator>();
            if (clip != null && animator != null && animator.isHuman)
            {
                float travel = new Vector3(clip.averageSpeed.x, 0, clip.averageSpeed.z).magnitude * clip.length * animator.humanScale * scale;
                if (travel > .3f) return travel;
            }
            var poses = Sample(rig, clip);
            if (poses.Count == 0) return fallback;
            float widest = poses.Max(p => p.feetGap), narrowest = poses.Min(p => p.feetGap);
            // The feet must really swing; if the body did not move, the sample failed: keep the default.
            return widest - narrowest > .05f ? widest * 2 * scale : fallback;
        }

        static AnimatorController BuildController(Dictionary<string, string> files, AnimationClip walk)
        {
            // The asset is kept (only emptied) so prefabs that point to it stay connected.
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) ?? AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            while (controller.layers.Length > 1) controller.RemoveLayer(1);
            foreach (var (name, type) in new[] { ("State", AnimatorControllerParameterType.Int), ("Speed", AnimatorControllerParameterType.Float), ("Alive", AnimatorControllerParameterType.Bool), ("ActionTime", AnimatorControllerParameterType.Float), ("GaitTime", AnimatorControllerParameterType.Float) })
                if (!controller.parameters.Any(p => p.name == name)) controller.AddParameter(name, type);
            var machine = controller.layers[0].stateMachine;
            foreach (var old in machine.states.ToArray()) machine.RemoveState(old.state);
            foreach (var old in machine.anyStateTransitions.ToArray()) machine.RemoveAnyStateTransition(old);
            var idleClip = Clip(files.TryGetValue("Parado", out var p0) ? p0 : null);
            var states = new Dictionary<string, AnimatorState>();
            for (int i = 0; i < Map.Length; i++)
            {
                var (name, file, _) = Map[i];
                var clip = file == "*andar" ? walk : Clip(files.TryGetValue(file == "*correr" ? RunFile(files) : file, out var path) ? path : null);
                if (clip == null && name == "Walk") clip = Clip(files.TryGetValue("Correr", out var run) ? run : null);
                if (clip == null) clip = idleClip;
                var state = machine.AddState(name, new Vector3(250 * (i % 4), 90 * (i / 4), 0));
                state.motion = clip; state.writeDefaultValues = true;
                bool action = name.StartsWith("Attack") || name == "Skill" || name == "Charged" || name == "Super" || name == "Dodge" || name == "Hit";
                if (action) { state.timeParameterActive = true; state.timeParameter = "ActionTime"; }
                if (name == "Walk" || name == "Run") { state.timeParameterActive = true; state.timeParameter = "GaitTime"; }
                if (name == "Idle") machine.defaultState = state;
                states[name] = state;
            }
            // Standing still: calm breathing, and now and then the long idle where the hero looks around.
            if (states.TryGetValue("Idle", out var idle) && states.TryGetValue("Idle2", out var idle2) && idle2.motion != idle.motion)
            {
                var go = idle.AddTransition(idle2); go.hasExitTime = true; go.exitTime = 2f; go.hasFixedDuration = true; go.duration = .5f;
                var back = idle2.AddTransition(idle); back.hasExitTime = true; back.exitTime = .96f; back.hasFixedDuration = true; back.duration = .5f;
            }
            EditorUtility.SetDirty(controller);
            return controller;
        }

        // Female body: same controller, her own walk.
        static RuntimeAnimatorController FemaleController(AnimatorController controller, AnimationClip walk, AnimationClip femaleWalk)
        {
            string path = Root + "Feminino/Animador.overrideController";
            if (walk == null || femaleWalk == null || walk == femaleWalk) { AssetDatabase.DeleteAsset(path); return controller; }
            var over = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
            if (over == null) { over = new AnimatorOverrideController(controller); AssetDatabase.CreateAsset(over, path); }
            over.runtimeAnimatorController = controller;
            var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            over.GetOverrides(pairs);
            for (int i = 0; i < pairs.Count; i++) pairs[i] = new KeyValuePair<AnimationClip, AnimationClip>(pairs[i].Key, pairs[i].Key == walk ? femaleWalk : null);
            over.ApplyOverrides(pairs);
            EditorUtility.SetDirty(over);
            return over;
        }

        // ---------- Prefab ----------
        static GameObject BuildPrefab(string fbx, string folder, Material material, RuntimeAnimatorController controller, float height, string gender, float andar, float correr, out int fixedVertices)
        {
            fixedVertices = 0;
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            var root = new GameObject("Expedicionario_" + gender);
            try
            {
                var model = (GameObject)PrefabUtility.InstantiatePrefab(source, root.transform);
                model.name = "Modelo";
                foreach (var r in model.GetComponentsInChildren<Renderer>(true)) { r.sharedMaterials = Enumerable.Repeat(material, Mathf.Max(1, r.sharedMaterials.Length)).ToArray(); r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; }

                var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller; animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

                // Face +Z (the toes say where the front is) and stand on the floor at the right height.
                var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot); var toes = animator.GetBoneTransform(HumanBodyBones.LeftToes);
                if (foot != null && toes != null)
                {
                    Vector3 front = toes.position - foot.position; front.y = 0;
                    if (front.sqrMagnitude > .00001f) model.transform.rotation = Quaternion.FromToRotation(front.normalized, Vector3.forward) * model.transform.rotation;
                }
                var bounds = Bounds(model);
                if (bounds.size.y > .01f) model.transform.localScale *= height / bounds.size.y;
                bounds = Bounds(model);
                model.transform.position += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);

                foreach (var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (gender != "Feminino") break;
                    var mesh = FixHair(skin, animator, out int count);
                    if (count > 0) { skin.sharedMesh = Save(mesh, folder + "Malha_" + skin.name + ".asset"); fixedVertices += count; }
                    else Object.DestroyImmediate(mesh);
                    skin.updateWhenOffscreen = false;
                }

                // Cartoon proportions: bigger head, hands and feet (bone scale, animations only rotate bones).
                foreach (var (bone, size) in new[] { (HumanBodyBones.Head, 1.15f), (HumanBodyBones.LeftHand, 1.12f), (HumanBodyBones.RightHand, 1.12f), (HumanBodyBones.LeftFoot, 1.1f), (HumanBodyBones.RightFoot, 1.1f) })
                {
                    var t = animator.GetBoneTransform(bone);
                    if (t != null) t.localScale = Vector3.one * size;
                }
                var passada = root.AddComponent<PassadaDoCorpo>();
                passada.andar = andar; passada.correr = correr;
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, ResourcesFolder + gender + ".prefab");
                return prefab;
            }
            finally { Object.DestroyImmediate(root); }
        }

        static Bounds Bounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        // Hair (or anything) that hangs far from the arms but follows the arm bones gets moved to the head/neck.
        // That is the ponytail that went with the raised hand.
        static Mesh FixHair(SkinnedMeshRenderer skin, Animator animator, out int count)
        {
            count = 0;
            var mesh = Object.Instantiate(skin.sharedMesh);
            mesh.name = skin.sharedMesh.name;
            var bones = skin.bones;
            Transform Bone(HumanBodyBones b) => animator.GetBoneTransform(b);
            int Index(Transform t) => t == null ? -1 : Array.IndexOf(bones, t);
            int head = Index(Bone(HumanBodyBones.Head)), neck = Index(Bone(HumanBodyBones.Neck));
            int chest = Index(Bone(HumanBodyBones.UpperChest)); if (chest < 0) chest = Index(Bone(HumanBodyBones.Chest));
            if (head < 0) return mesh;
            if (neck < 0) neck = head; if (chest < 0) chest = neck;

            var arm = new HashSet<int>();
            var segments = new List<(Vector3, Vector3)>();
            foreach (var side in new[] { (HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand), (HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand) })
            {
                var chain = new[] { Bone(side.Item1), Bone(side.Item2), Bone(side.Item3), Bone(side.Item4) };
                for (int i = 0; i + 1 < chain.Length; i++) if (chain[i] != null && chain[i + 1] != null) segments.Add((chain[i].position, chain[i + 1].position));
                var hand = chain[3];
                if (hand != null && chain[2] != null) segments.Add((hand.position, hand.position + (hand.position - chain[2].position) * .6f));
            }
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null) continue;
                string n = bones[i].name;
                if (n.Contains("Shoulder") || n.Contains("Arm") || n.Contains("Hand")) arm.Add(i);
            }
            float far = .16f * Mathf.Max(.01f, Bounds(animator.gameObject).size.y / 1.7f);
            float neckY = bones[neck].position.y, chestY = bones[chest].position.y;

            // World position of each vertex in the rest pose, skinned by hand (bones x bind poses).
            var vertices = mesh.vertices; var binds = mesh.bindposes;
            var matrices = new Matrix4x4[bones.Length];
            for (int i = 0; i < bones.Length; i++) matrices[i] = bones[i] != null && i < binds.Length ? bones[i].localToWorldMatrix * binds[i] : skin.transform.localToWorldMatrix;
            var weights = mesh.boneWeights;
            Vector3 World(int v)
            {
                var w = weights[v];
                return matrices[w.boneIndex0].MultiplyPoint3x4(vertices[v]) * w.weight0 + matrices[w.boneIndex1].MultiplyPoint3x4(vertices[v]) * w.weight1
                     + matrices[w.boneIndex2].MultiplyPoint3x4(vertices[v]) * w.weight2 + matrices[w.boneIndex3].MultiplyPoint3x4(vertices[v]) * w.weight3;
            }
            // 1) Vertices that follow an arm but are not on the arm (candidates).
            // 2) Seeds: the candidates up at head height (the top of the ponytail) or very far from the arm.
            // 3) Grow from the seeds through connected candidates: the whole ponytail, never the sleeve.
            int total = Mathf.Min(weights.Length, vertices.Length);
            float scaleBody = Mathf.Max(.01f, Bounds(animator.gameObject).size.y / 1.7f);
            float near = .06f * scaleBody;
            var leftArm = Bone(HumanBodyBones.LeftUpperArm); var rightArm = Bone(HumanBodyBones.RightUpperArm);
            float shoulderY = leftArm != null && rightArm != null ? (leftArm.position.y + rightArm.position.y) * .5f : neckY - .1f * scaleBody;
            var points = new Vector3[total]; var candidate = new bool[total]; var take = new bool[total];
            var queue = new Queue<int>();
            for (int v = 0; v < total; v++)
            {
                var w = weights[v];
                float onArm = (arm.Contains(w.boneIndex0) ? w.weight0 : 0) + (arm.Contains(w.boneIndex1) ? w.weight1 : 0) + (arm.Contains(w.boneIndex2) ? w.weight2 : 0) + (arm.Contains(w.boneIndex3) ? w.weight3 : 0);
                points[v] = World(v);
                if (onArm < .25f) continue;
                float nearest = float.MaxValue;
                foreach (var (a, b) in segments) nearest = Mathf.Min(nearest, Distance(points[v], a, b));
                if (nearest < near) continue;
                candidate[v] = true;
                if (points[v].y > shoulderY + .02f * scaleBody || nearest > far) { take[v] = true; queue.Enqueue(v); }
            }
            // Neighbours: triangle edges plus vertices at the same spot (texture seams split them).
            var links = new List<int>[total];
            void Link(int a, int b) { if (a >= total || b >= total) return; (links[a] ??= new List<int>()).Add(b); (links[b] ??= new List<int>()).Add(a); }
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                var tris = mesh.GetTriangles(sub);
                for (int i = 0; i + 2 < tris.Length; i += 3) { Link(tris[i], tris[i + 1]); Link(tris[i + 1], tris[i + 2]); Link(tris[i + 2], tris[i]); }
            }
            var spots = new Dictionary<Vector3Int, List<int>>();
            for (int v = 0; v < total; v++)
            {
                if (!candidate[v]) continue;
                var key = Vector3Int.RoundToInt(points[v] * 1000);
                if (!spots.TryGetValue(key, out var list)) spots[key] = list = new List<int>();
                foreach (int other in list) Link(v, other);
                list.Add(v);
            }
            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                if (links[v] == null) continue;
                foreach (int n in links[v]) if (candidate[n] && !take[n]) { take[n] = true; queue.Enqueue(n); }
            }
            for (int v = 0; v < total; v++)
            {
                if (!take[v]) continue;
                float t = Mathf.Clamp01((neckY - points[v].y) / Mathf.Max(.05f, neckY - chestY + .15f));
                weights[v] = new BoneWeight { boneIndex0 = head, weight0 = 1 - t, boneIndex1 = chest, weight1 = t };
                count++;
            }
            mesh.boneWeights = weights;
            return mesh;
        }

        static float Distance(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a; float t = d.sqrMagnitude > 0 ? Mathf.Clamp01(Vector3.Dot(p - a, d) / d.sqrMagnitude) : 0;
            return Vector3.Distance(p, a + d * t);
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
