using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForjaDasMares
{
    // The hero's body is the starting character chosen at creation (Expedicionário / Expedicionária).
    // It sits inside the hero's current visual, takes over the animation and the hand sockets, and the old
    // test bodies stay hidden. Weapons, effects and combat keep working through HeroModelView.
    public sealed class ExpedicionarioCorpo : MonoBehaviour
    {
        HeroPresentation hero;
        Transform swappedFor;
        GameObject body;
        readonly List<Renderer> hidden = new List<Renderer>();
        int refreshIn;
        Transform backSocket, chestBone;
        Animator bodyAnimator; HeroModelView bodyView; HeroCombat combat;
        int lastAction; float drawnUntil;
        Transform muzzle;
        // Standing, walking and running change with the weapon (LocomocaoPorArma).
        RuntimeAnimatorController baseController; LocomocaoPorArma.Jeito currentMoves; bool movesReady; int bodyGender;
        PassadaDoCorpo passada;
        readonly Dictionary<LocomocaoPorArma.Jeito, RuntimeAnimatorController> movesCache = new Dictionary<LocomocaoPorArma.Jeito, RuntimeAnimatorController>();
        // Goes up every time a hero changes body, so portraits and previews redraw.
        public static int Version { get; private set; }

        // Adds the body to every hero in every scene, also heroes that appear later (network).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindFirstObjectByType<Watcher>() != null) return;
            var go = new GameObject("CorpoDosHerois");
            DontDestroyOnLoad(go);
            go.AddComponent<Watcher>();
        }

        sealed class Watcher : MonoBehaviour
        {
            float next;
            void Update()
            {
                if (Time.unscaledTime < next) return;
                next = Time.unscaledTime + .5f;
                foreach (var each in FindObjectsByType<HeroPresentation>(FindObjectsSortMode.None))
                    if (each.GetComponent<ExpedicionarioCorpo>() == null) each.gameObject.AddComponent<ExpedicionarioCorpo>();
            }
        }

        void Awake() => hero = GetComponent<HeroPresentation>();

        void LateUpdate()
        {
            if (hero == null) return;
            var visible = hero.Visible;
            if (visible == null) return;
            if (visible != swappedFor) Swap(visible);
            if (refreshIn > 0 && --refreshIn == 0) RefreshEquipment();
            // Other systems may switch the old body back on: keep it hidden.
            foreach (var r in hidden) if (r != null && !r.forceRenderingOff) r.forceRenderingOff = true;
            PoseWeapon();
        }

        // Each kind of weapon is carried its own way (decided with Fabricio):
        // sword, axe, hammer and staff stay in the right hand; the others (boomerang, bow, daggers...) rest on the back
        // and go to the hand while attacking, then back after a few seconds without fighting.
        const float DrawnSeconds = 3f;
        static bool AlwaysInHand(WeaponCategory c) => c == WeaponCategory.Sword || c == WeaponCategory.Axe || c == WeaponCategory.Hammer || c == WeaponCategory.Staff;
        void PoseWeapon()
        {
            if (bodyView == null) { PoseBackWeapon(); return; }
            if (combat == null) combat = GetComponentInParent<HeroCombat>();
            if (combat != null && (combat.IsBusy || combat.ActionVersion != lastAction)) { lastAction = combat.ActionVersion; drawnUntil = Time.time + DrawnSeconds; }
            var category = bodyView.CurrentWeaponCategory;
            UpdateMoves(category);
            bool inHand = AlwaysInHand(category) || Time.time < drawnUntil;
            Vector3? tip = null;
            if (inHand) tip = PoseHandWeapon(category); else PoseBackWeapon();
            // Shots of a staff (or bow, focus) leave from the tip of the weapon in the hand, not from the chest.
            bool shoots = category == WeaponCategory.Staff || category == WeaponCategory.MagicWeapon || category == WeaponCategory.Bow;
            // The aim point stays at the chest; only the visible start of the shot moves to the tip (HeroCombat.Muzzle).
            if (combat == null) return;
            if (shoots && tip.HasValue)
            {
                if (muzzle == null) { muzzle = new GameObject("PontaDaArma").transform; muzzle.SetParent(body.transform, false); }
                muzzle.position = tip.Value;
                combat.Muzzle = muzzle;
            }
            else if (combat.Muzzle == muzzle) combat.Muzzle = null;
        }

        void UpdateMoves(WeaponCategory category)
        {
            if (bodyAnimator == null || baseController == null) return;
            var set = LocomocaoPorArma.Load();
            var moves = set != null ? set.Para(category) : null;
            if (movesReady && moves == currentMoves) return;
            movesReady = true; currentMoves = moves;
            if (moves == null)
            {
                bodyAnimator.runtimeAnimatorController = baseController;
                if (passada != null) bodyView.ConfigureStride(passada.andar, passada.correr);
                return;
            }
            if (!movesCache.TryGetValue(moves, out var controller)) movesCache[moves] = controller = set.Montar(baseController, moves);
            bodyAnimator.runtimeAnimatorController = controller;
            int g = Mathf.Clamp(bodyGender, 0, 1);
            bodyView.ConfigureStride(moves.passadaAndar.Length > g ? moves.passadaAndar[g] : 1, moves.passadaCorrer.Length > g ? moves.passadaCorrer[g] : 2.4f);
        }

        // In the fist. Each weapon has a grip (position, turn, size and handle point) stored in the hand bone's own frame,
        // so the weapon is really attached to the hand and follows every turn of the wrist.
        // The grip is tuned in Play with the "Ajustar pegada" tool (F8, Editor only) and saved in Resources/Pegadas.json,
        // which goes into the PC and mobile builds.
        float bodyHeight;
        Pegada editing; string editingKey;
        Vector3? PoseHandWeapon(WeaponCategory category)
        {
            if (backSocket == null || bodyAnimator == null || body == null) return null;
            var hand = bodyAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            if (hand == null) { PoseBackWeapon(); return null; }
            float height = Height();
            Vector3? tip = null;
            for (int i = 0; i < backSocket.childCount; i++)
            {
                var item = backSocket.GetChild(i);
                var filter = item.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;
                var mesh = filter.sharedMesh; var bounds = mesh.bounds; Vector3 extent = bounds.size;
                int along = extent.x >= extent.y && extent.x >= extent.z ? 0 : extent.y >= extent.z ? 1 : 2;
                int thin = extent.x <= extent.y && extent.x <= extent.z ? 0 : extent.y <= extent.z ? 1 : 2;
                if (thin == along) thin = (along + 1) % 3;
                float sign = HandleAtMin(mesh, along) ? 1 : -1;
                string key = category + "/" + mesh.name;
                var grip = Grip(key, category, hand, height);
                if (tip == null) { editing = grip; editingKey = key; }
                var model = Quaternion.LookRotation(Axis(thin), Axis(along) * sign);
                item.rotation = hand.rotation * Quaternion.Euler(grip.rot) * Quaternion.Inverse(model);
                float factor = grip.length * height / Mathf.Max(.0001f, extent[along]);
                var parent = backSocket.lossyScale;
                item.localScale = new Vector3(factor / Mathf.Max(.0001f, parent.x), factor / Mathf.Max(.0001f, parent.y), factor / Mathf.Max(.0001f, parent.z));
                Vector3 handle = bounds.center + Axis(along) * (sign * (-extent[along] * .5f + grip.hold * extent[along]));
                Vector3 point = hand.position + hand.rotation * (grip.pos * height);
                item.position += point - item.TransformPoint(handle);
                if (tip == null) tip = item.TransformPoint(bounds.center + Axis(along) * (sign * extent[along] * .5f));
            }
            return tip;
        }

        // Character height (head to feet), measured once per body so crouching or jumping doesn't resize the weapon.
        float Height()
        {
            if (bodyHeight > 0) return bodyHeight;
            var head = bodyAnimator.GetBoneTransform(HumanBodyBones.Head); var foot = bodyAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
            bodyHeight = head != null && foot != null ? Mathf.Max(.5f, head.position.y - foot.position.y + .2f) : 1.6f * Mathf.Max(.3f, body.transform.lossyScale.y);
            return bodyHeight;
        }

        // The saved grip of this weapon, or a first guess: handle in the palm, the weapon crossing the fist along the thumb.
        Pegada Grip(string key, WeaponCategory category, Transform hand, float height)
        {
            if (Pegadas.TryGetValue(key, out var known)) return known;
            var made = Default(key, category, hand, height);
            Pegadas[key] = made;
            return made;
        }
        Pegada Default(string key, WeaponCategory category, Transform hand, float height)
        {
            var middle = bodyAnimator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            var thumb = bodyAnimator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
            var back = Quaternion.Inverse(hand.rotation);
            Vector3 fingers = middle != null ? back * (middle.position - hand.position).normalized : Vector3.up;
            Vector3 up = thumb != null ? back * (thumb.position - hand.position) : back * body.transform.up;
            Vector3 blade = up - Vector3.Dot(up, fingers) * fingers;
            if (blade.sqrMagnitude < 1e-6f) blade = Vector3.Cross(fingers, Vector3.right);
            Vector3 grip = middle != null ? back * (Vector3.Lerp(hand.position, middle.position, .55f) - hand.position) : Vector3.zero;
            return new Pegada
            {
                key = key, saved = false,
                pos = grip / height,
                rot = Quaternion.LookRotation(fingers, blade.normalized).eulerAngles,
                length = category == WeaponCategory.Staff ? .85f : category == WeaponCategory.Sword ? .55f : category == WeaponCategory.Axe ? .48f : category == WeaponCategory.Hammer ? .52f : .38f,
                hold = category == WeaponCategory.Staff ? .38f : category == WeaponCategory.Sword || category == WeaponCategory.Axe || category == WeaponCategory.Hammer ? .1f : .3f,
            };
        }

        [System.Serializable] public sealed class Pegada { public string key; public Vector3 pos, rot; public float length, hold; [System.NonSerialized] public bool saved = true; }
        [System.Serializable] sealed class ListaDePegadas { public List<Pegada> itens = new List<Pegada>(); }
        static Dictionary<string, Pegada> pegadas;
        static Dictionary<string, Pegada> Pegadas { get { if (pegadas == null) LoadGrips(); return pegadas; } }
        static void LoadGrips()
        {
            pegadas = new Dictionary<string, Pegada>();
            string text = null;
#if UNITY_EDITOR
            if (System.IO.File.Exists(GripFile)) text = System.IO.File.ReadAllText(GripFile);
#endif
            if (text == null) { var asset = Resources.Load<TextAsset>("Pegadas"); if (asset != null) text = asset.text; }
            if (string.IsNullOrEmpty(text)) return;
            var list = JsonUtility.FromJson<ListaDePegadas>(text);
            if (list?.itens != null) foreach (var p in list.itens) if (!string.IsNullOrEmpty(p.key)) { p.saved = true; pegadas[p.key] = p; }
        }
#if UNITY_EDITOR
        static string GripFile => Application.dataPath + "/ForjaDasMares/Resources/Pegadas.json";
        // Writes only the grips that were saved with the button (the guesses stay out of the file).
        static void SaveGrip(Pegada grip)
        {
            grip.saved = true;
            var list = new ListaDePegadas();
            string old = System.IO.File.Exists(GripFile) ? System.IO.File.ReadAllText(GripFile) : null;
            var before = string.IsNullOrEmpty(old) ? null : JsonUtility.FromJson<ListaDePegadas>(old);
            if (before?.itens != null) foreach (var p in before.itens) if (p.key != grip.key) list.itens.Add(p);
            list.itens.Add(grip);
            System.IO.File.WriteAllText(GripFile, JsonUtility.ToJson(list, true));
            UnityEditor.AssetDatabase.ImportAsset("Assets/ForjaDasMares/Resources/Pegadas.json");
        }
        static Pegada SavedCopy(string key)
        {
            if (!System.IO.File.Exists(GripFile)) return null;
            var list = JsonUtility.FromJson<ListaDePegadas>(System.IO.File.ReadAllText(GripFile));
            if (list?.itens != null) foreach (var p in list.itens) if (p.key == key) return p;
            return null;
        }

        // "Ajustar pegada": F8 in Play (Editor only). Sliders move and turn the weapon in the hand; "Salvar pegada" keeps it.
        static ExpedicionarioCorpo tuning; static bool tuningOpen; static bool paused;
        void OnGUI()
        {
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F8 && !hero.ReplicaMode && body != null)
            {
                tuningOpen = !tuningOpen; tuning = this; e.Use();
                if (!tuningOpen) Pause(false);
            }
            if (!tuningOpen || tuning != this) return;
            float k = Mathf.Max(1, Screen.height / 900f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(k, k, 1));
            GUILayout.BeginArea(new Rect(12, 80, 340, 560), GUI.skin.box);
            GUILayout.Label("<b>AJUSTAR PEGADA</b>  (F8 fecha)", new GUIStyle(GUI.skin.label) { richText = true });
            var grip = editing;
            if (grip == null || !AlwaysInHand(bodyView.CurrentWeaponCategory)) { GUILayout.Label("Equipe uma arma de mão (espada, machado, martelo ou cajado)."); GUILayout.EndArea(); return; }
            GUILayout.Label(editingKey + (grip.saved ? "" : "   (não salvo)"));
            float h = Height();
            grip.pos.x = Slider("Mover X (cm)", grip.pos.x * h * 100, -20, 20) / (h * 100);
            grip.pos.y = Slider("Mover Y (cm)", grip.pos.y * h * 100, -20, 20) / (h * 100);
            grip.pos.z = Slider("Mover Z (cm)", grip.pos.z * h * 100, -20, 20) / (h * 100);
            grip.rot.x = Slider("Girar X", Wrap(grip.rot.x), -180, 180);
            grip.rot.y = Slider("Girar Y", Wrap(grip.rot.y), -180, 180);
            grip.rot.z = Slider("Girar Z", Wrap(grip.rot.z), -180, 180);
            grip.length = Slider("Tamanho (cm)", grip.length * h * 100, 15, 200) / (h * 100);
            grip.hold = Slider("Onde a mão segura (%)", grip.hold * 100, 0, 100) / 100;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+90 X")) grip.rot.x += 90; if (GUILayout.Button("+90 Y")) grip.rot.y += 90; if (GUILayout.Button("+90 Z")) grip.rot.z += 90;
            GUILayout.EndHorizontal();
            if (GUILayout.Button(paused ? "Continuar animação" : "Pausar animação")) Pause(!paused);
            GUILayout.Space(6);
            if (GUILayout.Button("SALVAR PEGADA", GUILayout.Height(34))) SaveGrip(grip);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Voltar ao salvo")) { var copy = SavedCopy(editingKey); if (copy != null) { copy.saved = true; Pegadas[editingKey] = copy; editing = copy; } }
            if (GUILayout.Button("Padrão")) { var hand = bodyAnimator.GetBoneTransform(HumanBodyBones.RightHand); if (hand != null) { var d = Default(editingKey, bodyView.CurrentWeaponCategory, hand, h); Pegadas[editingKey] = d; editing = d; } }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        void Pause(bool value) { paused = value; if (bodyAnimator != null) bodyAnimator.speed = value ? 0 : 1; }
        static float Wrap(float a) { a %= 360; return a > 180 ? a - 360 : a < -180 ? a + 360 : a; }
        static float Slider(string label, float value, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(150));
            value = GUILayout.HorizontalSlider(value, min, max, GUILayout.Width(120));
            GUILayout.Label(value.ToString("0.0"), GUILayout.Width(44));
            GUILayout.EndHorizontal();
            return value;
        }
#endif

        // Which end of the long axis is the handle: the thinner one (a staff head or a blade is wider than the grip).
        static readonly Dictionary<Mesh, bool> handleAtMin = new Dictionary<Mesh, bool>();
        static bool HandleAtMin(Mesh mesh, int along)
        {
            if (handleAtMin.TryGetValue(mesh, out bool known)) return known;
            bool result = true;
            if (mesh.isReadable)
            {
                var vertices = mesh.vertices; var b = mesh.bounds; float min = b.min[along], span = Mathf.Max(.0001f, b.size[along]);
                Vector2 center = Flat(b.center, along); float low = 0, high = 0; int nLow = 0, nHigh = 0;
                foreach (var v in vertices)
                {
                    float k = (v[along] - min) / span; float r = (Flat(v, along) - center).magnitude;
                    if (k < .15f) { low += r; nLow++; } else if (k > .85f) { high += r; nHigh++; }
                }
                if (nLow > 0 && nHigh > 0) result = low / nLow <= high / nHigh;
            }
            handleAtMin[mesh] = result;
            return result;
        }
        static Vector2 Flat(Vector3 v, int along) => along == 0 ? new Vector2(v.y, v.z) : along == 1 ? new Vector2(v.x, v.z) : new Vector2(v.x, v.y);

        // Every frame, after the animation: the main weapon lies flat on the back, tilted like a sword carried
        // crosswise, about 58 cm long and just behind the jacket. Works for any weapon model, whatever its own axes.
        const float BackLength = .58f, BackDepth = .10f, BackTilt = 35f;
        void PoseBackWeapon()
        {
            if (backSocket == null || chestBone == null || body == null) return;
            var root = body.transform;
            float size = Mathf.Max(.3f, root.lossyScale.y);
            Vector3 front = root.forward, up = root.up;
            for (int i = 0; i < backSocket.childCount; i++)
            {
                var item = backSocket.GetChild(i);
                var filter = item.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;
                var bounds = filter.sharedMesh.bounds; Vector3 extent = bounds.size;
                int along = extent.x >= extent.y && extent.x >= extent.z ? 0 : extent.y >= extent.z ? 1 : 2;
                int thin = extent.x <= extent.y && extent.x <= extent.z ? 0 : extent.y <= extent.z ? 1 : 2;
                if (thin == along) thin = (along + 1) % 3;
                // Model space: thin axis -> out of the back, long axis -> the diagonal.
                var model = Quaternion.LookRotation(Axis(thin), Axis(along));
                var world = Quaternion.LookRotation(front, Quaternion.AngleAxis(BackTilt, front) * up);
                item.rotation = world * Quaternion.Inverse(model);
                float factor = BackLength * size / Mathf.Max(.0001f, extent[along]);
                var parent = backSocket.lossyScale;
                item.localScale = new Vector3(factor / Mathf.Max(.0001f, parent.x), factor / Mathf.Max(.0001f, parent.y), factor / Mathf.Max(.0001f, parent.z));
                Vector3 target = chestBone.position - front * (BackDepth * size + extent[thin] * factor * .5f) + up * .03f * size;
                item.position += target - item.TransformPoint(bounds.center);
            }
        }

        static Vector3 Axis(int index) => index == 0 ? Vector3.right : index == 1 ? Vector3.up : Vector3.forward;

        void Swap(Transform visible)
        {
            var view = visible.GetComponentInChildren<HeroModelView>(true);
            if (!hero.ReplicaMode && AccountService.Character == null) AccountService.SignIn();
            int gender = hero.ReplicaMode ? 0 : Mathf.Clamp(AccountService.Character != null ? AccountService.Character.gender : 0, 0, 1);
            var prefab = Resources.Load<GameObject>("Personagens/" + (gender == 1 ? "Feminino" : "Masculino"));
            if (view == null || prefab == null) { swappedFor = visible; return; }

            if (body != null) Destroy(body);
            foreach (var r in hidden) if (r != null) r.forceRenderingOff = false;
            hidden.Clear();

            // Hide the old body (not the equipped weapons, which carry an EquipmentMeshView).
            foreach (var r in visible.GetComponentsInChildren<Renderer>(true))
                if (r.GetComponentInParent<EquipmentMeshView>() == null && r is not ParticleSystemRenderer && r is not TrailRenderer) { r.forceRenderingOff = true; hidden.Add(r); }
            if (view.Animator != null) view.Animator.enabled = false;

            body = Instantiate(prefab, visible);
            body.name = "Expedicionario";
            body.transform.localPosition = Vector3.zero; body.transform.localRotation = Quaternion.identity;
            foreach (var t in body.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = visible.gameObject.layer;
            var animator = body.GetComponentInChildren<Animator>(true);
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            // Main weapon rests on the back (diagonal, flat against it); the second item stays in the left hand.
            Transform back = Back(animator, body.transform), left = Socket(animator, HumanBodyBones.LeftHand, "MaoEsquerda");
            backSocket = back;
            chestBone = animator.GetBoneTransform(HumanBodyBones.UpperChest) ?? animator.GetBoneTransform(HumanBodyBones.Chest) ?? animator.GetBoneTransform(HumanBodyBones.Spine);
            view.Configure(animator, back, left, null, true);
            bodyHeight = 0;
            bodyAnimator = animator; bodyView = view;
            view.ConfigureEquipmentFit(true);
            passada = body.GetComponent<PassadaDoCorpo>();
            if (passada != null) view.ConfigureStride(passada.andar, passada.correr);
            baseController = animator.runtimeAnimatorController; bodyGender = gender; movesReady = false; currentMoves = null; movesCache.Clear();
            swappedFor = visible;

            // Weapons move to the new body (now and again two frames later, after the equipment has loaded).
            RefreshEquipment(); refreshIn = 3;
            Version++;
        }

        void RefreshEquipment()
        {
            var equipment = GetComponent<HeroEquipment>() ?? GetComponentInParent<HeroEquipment>();
            if (equipment != null && equipment.enabled) { equipment.enabled = false; equipment.enabled = true; }
        }

        // Point between the shoulder blades. The mount turns the weapon by 90 degrees on X, so the socket is turned back
        // by that amount: the weapon ends up flat on the back, tilted like a sword carried crosswise.
        static Transform Back(Animator animator, Transform body)
        {
            var chest = animator.GetBoneTransform(HumanBodyBones.UpperChest) ?? animator.GetBoneTransform(HumanBodyBones.Chest) ?? animator.GetBoneTransform(HumanBodyBones.Spine);
            if (chest == null) return animator.transform;
            float size = Mathf.Max(.5f, body.lossyScale.y);
            var socket = new GameObject("Costas").transform;
            socket.SetParent(chest, false);
            socket.position = chest.position - body.forward * .2f * size + body.up * .02f * size;
            socket.rotation = body.rotation * Quaternion.Euler(0, 0, 35) * Quaternion.Euler(-90, 0, 0);
            var scale = chest.lossyScale;
            socket.localScale = new Vector3(1 / Mathf.Max(.0001f, scale.x), 1 / Mathf.Max(.0001f, scale.y), 1 / Mathf.Max(.0001f, scale.z));
            return socket;
        }

        // Grip point in the palm, pointing along the fingers (adjusted per weapon by EquipmentVisualPlacement).
        static Transform Socket(Animator animator, HumanBodyBones bone, string name)
        {
            var hand = animator.GetBoneTransform(bone);
            if (hand == null) return animator.transform;
            var socket = new GameObject(name).transform;
            socket.SetParent(hand, false);
            var middle = animator.GetBoneTransform(bone == HumanBodyBones.RightHand ? HumanBodyBones.RightMiddleProximal : HumanBodyBones.LeftMiddleProximal);
            if (middle != null) socket.position = Vector3.Lerp(hand.position, middle.position, .6f);
            var scale = hand.lossyScale;
            socket.localScale = new Vector3(1 / Mathf.Max(.0001f, scale.x), 1 / Mathf.Max(.0001f, scale.y), 1 / Mathf.Max(.0001f, scale.z));
            return socket;
        }
    }
}
