using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ForjaDasMares.Networking.Editor
{
    // Wild animals made by Fabricio (Meshy, low poly): imports the models, builds the animator
    // (Idle, Walk, Run, Sprint, Pastar) and the prefab in Resources/Animais/<Nome>.prefab.
    // Runs by itself when a prefab is missing; menu: Forja/Animais/Preparar animais.
    [InitializeOnLoad]
    public static class AnimaisInstaller
    {
        const string Art = "Assets/ForjaDasMares/Art/Animais/";
        const string Out = "Assets/ForjaDasMares/Resources/Animais/";
        // name, size of the animal (metres: length for the boar, height for the deer), clips by state
        static readonly (string nome, float tamanho, bool altura, (string estado, string clip)[] clips)[] Lista =
        {
            ("Javali", 1.35f, false, new[] { ("Idle", "idle"), ("Walk", "walk"), ("Run", "run"), ("Sprint", "sprint") }),
            ("Cervo", 1.75f, true, new[] { ("Idle", "deer_idle"), ("Pastar", "idle01"), ("Walk", "walk"), ("Run", "run") }),
        };

        static AnimaisInstaller() { EditorApplication.delayCall += () => { if (Lista.Any(a => File.Exists(Art + a.nome + "/" + a.nome + ".fbx") && !File.Exists(Out + a.nome + ".prefab"))) Install(); }; }

        [MenuItem("Forja/Animais/Preparar animais")]
        public static void Install()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Install; return; }
            if (!AssetDatabase.IsValidFolder("Assets/ForjaDasMares/Resources")) AssetDatabase.CreateFolder("Assets/ForjaDasMares", "Resources");
            if (!AssetDatabase.IsValidFolder(Out.TrimEnd('/'))) AssetDatabase.CreateFolder("Assets/ForjaDasMares/Resources", "Animais");
            var report = new System.Text.StringBuilder("ANIMAIS_PRONTOS ·");
            foreach (var a in Lista) report.Append(' ').Append(Build(a.nome, a.tamanho, a.altura, a.clips));
            AssetDatabase.SaveAssets();
            Debug.Log(report.ToString());
        }

        static string Build(string nome, float size, bool byHeight, (string estado, string clip)[] wanted)
        {
            string fbx = Art + nome + "/" + nome + ".fbx", png = Art + nome + "/" + nome + ".png";
            if (!(AssetImporter.GetAtPath(fbx) is ModelImporter importer)) return nome + ": modelo não encontrado";
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true; importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importCameras = importer.importLights = false;
            importer.SaveAndReimport();
            var clips = importer.defaultClipAnimations;
            foreach (var c in clips) { c.loopTime = true; c.lockRootPositionXZ = true; c.keepOriginalPositionXZ = false; }
            importer.clipAnimations = clips; importer.SaveAndReimport();
            var all = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToArray();

            // Material with the painted texture.
            string matPath = Art + nome + "/" + nome + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, matPath); }
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(png)); mat.SetFloat("_Smoothness", .1f); EditorUtility.SetDirty(mat);

            // Animator: one state per movement; the game picks the state by name.
            string ctrlPath = Art + nome + "/" + nome + ".controller";
            AssetDatabase.DeleteAsset(ctrlPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            var machine = controller.layers[0].stateMachine;
            var found = new System.Text.StringBuilder();
            foreach (var (estado, key) in wanted)
            {
                var clip = all.FirstOrDefault(c => c.name.ToLowerInvariant().EndsWith(key)) ?? all.FirstOrDefault(c => c.name.ToLowerInvariant().Contains(key));
                if (clip == null) { found.Append(" sem ").Append(estado); continue; }
                var state = machine.AddState(estado); state.motion = clip;
                if (estado == "Idle") machine.defaultState = state;
            }

            // Prefab: the model, scaled to a real size, with a collider.
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            var root = new GameObject(nome);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(source); model.transform.SetParent(root.transform, false);
            foreach (var r in model.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
            var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller; animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            var bounds = Bounds(model);
            float measure = byHeight ? bounds.size.y : Mathf.Max(bounds.size.x, bounds.size.z);
            float scale = measure > .001f ? size / measure : 1;
            model.transform.localScale = Vector3.one * scale;
            bounds = Bounds(model);
            model.transform.localPosition = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
            bounds = Bounds(model);
            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.direction = 2; capsule.center = bounds.center - root.transform.position;
            capsule.radius = Mathf.Min(bounds.extents.x, bounds.extents.y) * .9f; capsule.height = Mathf.Max(bounds.size.z, capsule.radius * 2);
            PrefabUtility.SaveAsPrefabAsset(root, Out + nome + ".prefab");
            Object.DestroyImmediate(root);
            return nome + ": ok (" + all.Length + " animações" + found + ", escala " + scale.ToString("0.###") + ")";
        }

        static Bounds Bounds(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b;
        }
    }
}
