using UnityEngine;

namespace ForjaDasMares
{
    // Living pose for rigged NPC models that came without idle clips. Everything is done on the bones by code:
    // breathing, weight shift, looking around and a small job for each character.
    public sealed class NpcIdle : MonoBehaviour
    {
        public enum Style { Calmo, Ferreiro, Curandeira, Mercador, Oficial }
        [SerializeField] Style style = Style.Calmo;

        Transform hips, spine, head, leftArm, leftFore, leftHand, rightArm, rightFore, rightHand;
        Quaternion hipsBase, spineBase, headBase, leftArmBase, rightArmBase, leftForeBase, rightForeBase;
        float time, lookTimer, lookYaw, lookTarget;
        Transform glow;
        Material glowMaterial;
        bool ready;

        public void Configure(Style value) => style = value;

        void Start()
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                string n = t.name;
                int colon = n.LastIndexOf(':');
                if (colon >= 0) n = n.Substring(colon + 1);
                switch (n)
                {
                    case "Hips": hips = t; break;
                    case "Spine": spine = t; break;
                    case "Head": head = t; break;
                    case "LeftArm": leftArm = t; break;
                    case "LeftForeArm": leftFore = t; break;
                    case "LeftHand": leftHand = t; break;
                    case "RightArm": rightArm = t; break;
                    case "RightForeArm": rightFore = t; break;
                    case "RightHand": rightHand = t; break;
                }
            }
            LowerArm(leftArm, leftFore, leftHand);
            LowerArm(rightArm, rightFore, rightHand);
            if (hips != null) hipsBase = hips.localRotation;
            if (spine != null) spineBase = spine.localRotation;
            if (head != null) headBase = head.localRotation;
            if (leftArm != null) leftArmBase = leftArm.localRotation;
            if (rightArm != null) rightArmBase = rightArm.localRotation;
            if (leftFore != null) leftForeBase = leftFore.localRotation;
            if (rightFore != null) rightForeBase = rightFore.localRotation;
            time = Random.value * 10;
            lookTimer = Random.Range(1f, 3f);
            ready = spine != null && leftArm != null && rightArm != null;
            if (!ready) { Debug.LogWarning("NpcIdle: ossos não encontrados em " + name); return; }
            if (style == Style.Ferreiro) BuildForge();
            if (style == Style.Curandeira) BuildGlow();
        }

        // Points the upper arm down along the body and bends the elbow a little, whatever the bone axes are.
        void LowerArm(Transform arm, Transform fore, Transform hand)
        {
            if (arm == null || fore == null) return;
            Vector3 side = arm.position - transform.position; side.y = 0;
            side = side.sqrMagnitude > .0001f ? side.normalized : transform.right;
            Vector3 down = (Vector3.down + side * .22f + transform.forward * .06f).normalized;
            arm.rotation = Quaternion.FromToRotation(fore.position - arm.position, down) * arm.rotation;
            if (hand == null) return;
            Vector3 bent = (down + transform.forward * .32f).normalized;
            fore.rotation = Quaternion.FromToRotation(hand.position - fore.position, bent) * fore.rotation;
        }

        static Material Paint(Color color, float emission = 0)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var m = new Material(shader != null ? shader : Shader.Find("Standard"));
            m.SetColor("_BaseColor", color); m.color = color;
            m.SetFloat("_Smoothness", .2f);
            if (emission > 0) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", color * emission); }
            return m;
        }

        static Transform Piece(Transform parent, PrimitiveType type, Vector3 worldScale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = material;
            go.transform.SetParent(parent, true);
            var lossy = parent != null ? parent.lossyScale : Vector3.one;
            go.transform.localScale = new Vector3(worldScale.x / Mathf.Max(.0001f, lossy.x), worldScale.y / Mathf.Max(.0001f, lossy.y), worldScale.z / Mathf.Max(.0001f, lossy.z));
            return go.transform;
        }

        // A hammer in the right hand and an anvil in front of him.
        void BuildForge()
        {
            var iron = Paint(new Color(.25f, .24f, .26f));
            var wood = Paint(new Color(.5f, .33f, .18f));
            if (rightHand != null)
            {
                var hammer = new GameObject("Martelo").transform;
                hammer.SetParent(rightHand, false);
                Vector3 front = transform.forward;
                var handle = Piece(hammer, PrimitiveType.Cylinder, new Vector3(.05f, .2f, .05f), wood);
                handle.position = rightHand.position + front * .14f;
                handle.rotation = Quaternion.FromToRotation(Vector3.up, front);
                var head = Piece(hammer, PrimitiveType.Cube, new Vector3(.12f, .26f, .12f), iron);
                head.position = rightHand.position + front * .32f;
                head.rotation = Quaternion.LookRotation(front);
            }
            var holder = transform.parent != null && transform.parent.parent != null ? transform.parent.parent : transform.parent;
            Vector3 spot = transform.position + transform.forward * .8f;
            var anvil = new GameObject("Bigorna").transform;
            anvil.SetParent(holder, true);
            anvil.position = spot;
            anvil.rotation = Quaternion.LookRotation(transform.right);
            var foot = Piece(anvil, PrimitiveType.Cylinder, new Vector3(.4f, .25f, .4f), wood);
            foot.position = spot + Vector3.up * .25f;
            var top = Piece(anvil, PrimitiveType.Cube, new Vector3(.32f, .2f, .62f), iron);
            top.position = spot + Vector3.up * .6f; top.rotation = anvil.rotation;
        }

        // A soft green light that grows between her hands while she channels.
        void BuildGlow()
        {
            glowMaterial = Paint(new Color(.45f, 1f, .7f), 2.5f);
            glow = Piece(null, PrimitiveType.Sphere, Vector3.one * .18f, glowMaterial);
            glow.name = "LuzDeCura";
            glow.SetParent(transform, true);
        }

        void Arm(Transform arm, Transform fore, Quaternion armBase, Quaternion foreBase, float forward, float outward, float elbow, float sideSign)
        {
            if (arm == null) return;
            Vector3 right = transform.right, front = transform.forward;
            arm.localRotation = armBase;
            arm.rotation = Quaternion.AngleAxis(-forward, right) * Quaternion.AngleAxis(sideSign * outward, front) * arm.rotation;
            if (fore == null) return;
            fore.localRotation = foreBase;
            fore.rotation = Quaternion.AngleAxis(-elbow, right) * fore.rotation;
        }

        void LateUpdate()
        {
            if (!ready) return;
            float dt = Time.deltaTime;
            time += dt;
            Vector3 right = transform.right, front = transform.forward, up = Vector3.up;
            float breath = Mathf.Sin(time * 1.7f);
            float sway = Mathf.Sin(time * .55f);

            float lean = breath * 1.8f, twist = 0, hipRoll = sway * 2.5f;
            float lF = 4 + breath * 3, lO = 3, lE = 8, rF = 4 - breath * 3, rO = 3, rE = 8;

            switch (style)
            {
                case Style.Ferreiro:
                {
                    // Raise the hammer slowly, strike fast, rest a moment.
                    float cycle = (time % 2.2f) / 2.2f, lift;
                    if (cycle < .55f) lift = Mathf.SmoothStep(0, 1, cycle / .55f);
                    else if (cycle < .64f) lift = 1 - (cycle - .55f) / .09f;
                    else lift = 0;
                    rF = 15 + lift * 95; rE = 25 + lift * 55; rO = 6;
                    lF = 25; lE = 40; lO = 8;
                    lean = 6 + (1 - lift) * 8 - lift * 4;
                    hipRoll = sway * 1.2f;
                    break;
                }
                case Style.Curandeira:
                {
                    // Hands come together in front of the chest and a healing light grows.
                    float channel = Mathf.SmoothStep(0, 1, Mathf.PingPong(time / 2.6f, 1));
                    lF = rF = 18 + channel * 42;
                    lE = rE = 30 + channel * 55;
                    lO = rO = -channel * 10;
                    lean = 2 + channel * 3 + breath;
                    if (glow != null && leftHand != null && rightHand != null)
                    {
                        glow.position = (leftHand.position + rightHand.position) * .5f + front * .12f;
                        float size = .05f + channel * .17f * (1 + Mathf.Sin(time * 6) * .12f);
                        var lossy = transform.lossyScale;
                        glow.localScale = new Vector3(size / lossy.x, size / lossy.y, size / lossy.z);
                    }
                    break;
                }
                case Style.Mercador:
                {
                    // Turns to each side and opens an arm, showing the goods.
                    float side = Mathf.Sin(time * .7f);
                    twist = side * 18;
                    float show = Mathf.Max(0, Mathf.Sin(time * 1.4f));
                    if (side > 0) { rF = 25 + show * 35; rO = 15 + show * 35; rE = 20; }
                    else { lF = 25 + show * 35; lO = 15 + show * 35; lE = 20; }
                    lean = 3 + breath * 2;
                    break;
                }
                case Style.Oficial:
                {
                    // Hands behind the back, looking over the road; now and then points ahead.
                    float point = Mathf.Clamp01((Mathf.Sin(time * .45f) - .55f) * 3);
                    lF = -18; lE = 50; lO = 6;
                    rF = Mathf.Lerp(-18, 80, point); rE = Mathf.Lerp(50, 5, point); rO = 6;
                    lean = -2 + breath * 1.5f;
                    break;
                }
            }

            if (hips != null)
            {
                hips.localRotation = hipsBase;
                hips.rotation = Quaternion.AngleAxis(hipRoll, front) * hips.rotation;
            }
            spine.localRotation = spineBase;
            spine.rotation = Quaternion.AngleAxis(twist, up) * Quaternion.AngleAxis(lean, right) * Quaternion.AngleAxis(-hipRoll * .8f, front) * spine.rotation;
            Arm(leftArm, leftFore, leftArmBase, leftForeBase, lF, lO, lE, -1);
            Arm(rightArm, rightFore, rightArmBase, rightForeBase, rF, rO, rE, 1);

            if (head != null)
            {
                lookTimer -= dt;
                if (lookTimer <= 0) { lookTarget = Random.Range(-35f, 35f); lookTimer = Random.Range(2f, 5f); }
                if (style == Style.Ferreiro) lookTarget = 0;
                lookYaw = Mathf.Lerp(lookYaw, lookTarget, dt * 2.5f);
                float nod = style == Style.Ferreiro ? 12 : breath * 2;
                head.localRotation = headBase;
                head.rotation = Quaternion.AngleAxis(lookYaw - twist * .5f, up) * Quaternion.AngleAxis(nod, right) * head.rotation;
            }
        }

        void OnDestroy() { if (glowMaterial != null) Destroy(glowMaterial); }
    }
}
