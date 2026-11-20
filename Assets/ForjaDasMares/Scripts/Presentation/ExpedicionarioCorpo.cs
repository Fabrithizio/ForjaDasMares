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
        Transform movedOrigin; Vector3 originRest; bool originMoved;
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
            bool inHand = AlwaysInHand(category) || Time.time < drawnUntil;
            Vector3? tip = null;
            if (inHand) tip = PoseHandWeapon(category); else PoseBackWeapon();
            // Shots of a staff (or bow, focus) leave from the tip of the weapon in the hand, not from the chest.
            bool shoots = category == WeaponCategory.Staff || category == WeaponCategory.MagicWeapon || category == WeaponCategory.Bow;
            var origin = combat != null ? combat.StrikePoint : null;
            if (origin != movedOrigin) { RestoreOrigin(); movedOrigin = origin; if (origin != null) originRest = origin.localPosition; }
            if (origin != null && shoots && tip.HasValue) { origin.position = tip.Value; originMoved = true; }
            else RestoreOrigin();
        }
        void RestoreOrigin() { if (originMoved && movedOrigin != null) movedOrigin.localPosition = originRest; originMoved = false; }

        // In the fist: the weapon crosses the palm along the thumb, the thin end (handle) in the hand.
        Vector3? PoseHandWeapon(WeaponCategory category)
        {
            if (backSocket == null || bodyAnimator == null || body == null) return null;
            var hand = bodyAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            var middle = bodyAnimator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            var thumb = bodyAnimator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
            if (hand == null || middle == null) { PoseBackWeapon(); return null; }
            // Sizes follow the character's real height (head to feet), so the staff reaches about the shoulder.
            var head = bodyAnimator.GetBoneTransform(HumanBodyBones.Head); var foot = bodyAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
            float height = head != null && foot != null ? Mathf.Max(.5f, head.position.y - foot.position.y + .2f) : 1.6f * Mathf.Max(.3f, body.transform.lossyScale.y);
            Vector3 grip = Vector3.Lerp(hand.position, middle.position, .55f);
            Vector3 fingers = (middle.position - hand.position).normalized;
            Vector3 up = thumb != null ? thumb.position - hand.position : body.transform.up;
            Vector3 blade = up - Vector3.Dot(up, fingers) * fingers;
            if (blade.sqrMagnitude < 1e-6f) blade = body.transform.up;
            blade.Normalize();
            // A staff stands almost upright in the fist; blades follow the thumb.
            if (category == WeaponCategory.Staff) blade = Vector3.Slerp(blade, body.transform.up, .8f).normalized;
            float length = (category == WeaponCategory.Staff ? .85f : category == WeaponCategory.Sword ? .55f : category == WeaponCategory.Axe ? .48f : category == WeaponCategory.Hammer ? .52f : .38f) * height;
            Vector3? tip = null;
            float hold = category == WeaponCategory.Staff ? .38f : category == WeaponCategory.Sword || category == WeaponCategory.Axe || category == WeaponCategory.Hammer ? .1f : .3f;
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
                var model = Quaternion.LookRotation(Axis(thin), Axis(along) * sign);
                var world = Quaternion.LookRotation(fingers, blade);
                item.rotation = world * Quaternion.Inverse(model);
                float factor = length / Mathf.Max(.0001f, extent[along]);
                var parent = backSocket.lossyScale;
                item.localScale = new Vector3(factor / Mathf.Max(.0001f, parent.x), factor / Mathf.Max(.0001f, parent.y), factor / Mathf.Max(.0001f, parent.z));
                Vector3 handle = bounds.center + Axis(along) * (sign * (-extent[along] * .5f + hold * extent[along]));
                item.position += grip - item.TransformPoint(handle);
                if (tip == null) tip = item.TransformPoint(bounds.center + Axis(along) * (sign * extent[along] * .5f));
            }
            return tip;
        }

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
            bodyAnimator = animator; bodyView = view;
            view.ConfigureEquipmentFit(true);
            var passada = body.GetComponent<PassadaDoCorpo>();
            if (passada != null) view.ConfigureStride(passada.andar, passada.correr);
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
