using UnityEngine;

namespace ForjaDasMares
{
    [RequireComponent(typeof(Camera))]
    public sealed class ArenaCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(9f, 13f, -9f);
        [SerializeField] Vector3 lookOffset = new Vector3(0f, 0.9f, 0f);
        [SerializeField, Min(0.01f)] float followTime = 0.12f;
        [SerializeField, Min(0.1f)] float collisionRadius = 0.2f;
        [SerializeField, Min(0f)] float collisionPadding = 0.1f;
        [Header("Stable framing")]
        [SerializeField, Min(0f)] float obstructionReleaseDelay = 0.2f;
        [SerializeField, Min(0.05f)] float visibilityRadius = 0.3f;
        [SerializeField, Range(0.15f, 0.75f)] float obstructionOpacity = 0.35f;
        [SerializeField, Min(0.01f)] float zoomOutTime = 0.28f;
        [SerializeField] LayerMask blockers;
        [SerializeField, Range(30f, 80f)] float verticalFieldOfView = 48f;
        [SerializeField, Min(0.5f)] float referenceAspect = 1.777778f;
        Vector3 smoothedFocus, smoothVelocity;float impulse,impulseTime;
        public void Impact(float strength){if(PlayerPrefs.GetInt("CampaignCameraShake",0)==1){impulse=Mathf.Max(impulse,Mathf.Clamp(strength,0,.06f));impulseTime=.12f;}}
        [Header("Action RPG framing")]
        [SerializeField] Vector3 rpgOffset=new Vector3(0,11.5f,-8.5f);
        [SerializeField,Range(30,60)]float rpgFieldOfView=42;
        [SerializeField,Range(.4f,1)]float minimumZoom=.5f;
        [SerializeField,Range(1,2)]float maximumZoom=1.6f;
        [SerializeField, Range(1f, 1.5f)] float fullscreenRpgDistance = 1.2744f;
        bool rpgFraming;
        float zoom=1,desiredZoom=1;UnityEngine.InputSystem.InputAction wheel;
        public void UseRpgFraming(){rpgFraming=true;offset=rpgOffset;verticalFieldOfView=rpgFieldOfView;zoom=desiredZoom=1;minimumZoom=CloseZoom;maximumZoom=1.6f;}
        // Close-up: the camera can come right up to the hero, and the closer it gets the lower it looks,
        // so the face, the clothes and the weapon in the hand can be seen. Far away it stays high for combat.
        const float CloseZoom=.22f, FlattenFrom=.6f, CloseUpPitch=16f;
        public void SetZoom(float value){desiredZoom=Mathf.Clamp(value,minimumZoom,maximumZoom);}
        // Turning the view around the hero. Movement already follows the camera, so controls stay natural.
        float yaw,desiredYaw;
        public float Yaw=>yaw;
        public void ResetYaw(){desiredYaw=0;}
        readonly System.Collections.Generic.List<int> freeTouches=new System.Collections.Generic.List<int>();
        float pinchDistance;
        void ReadViewGestures()
        {
            float dt=Time.unscaledDeltaTime;
            bool turning=false;
            var events=UnityEngine.EventSystems.EventSystem.current;
            if(UnityEngine.InputSystem.Touchscreen.current is UnityEngine.InputSystem.Touchscreen screen)
            {
                // Only fingers that started on the world (not on the joystick or buttons) move the camera.
                foreach(var touch in screen.touches)
                {
                    int id=touch.touchId.ReadValue();
                    var phase=touch.phase.ReadValue();
                    if(phase==UnityEngine.InputSystem.TouchPhase.Began&&!freeTouches.Contains(id)&&(events==null||!events.IsPointerOverGameObject(id)))freeTouches.Add(id);
                    else if((phase==UnityEngine.InputSystem.TouchPhase.Ended||phase==UnityEngine.InputSystem.TouchPhase.Canceled||phase==UnityEngine.InputSystem.TouchPhase.None)&&freeTouches.Contains(id))freeTouches.Remove(id);
                }
                UnityEngine.InputSystem.Controls.TouchControl a=null,b=null;
                foreach(var touch in screen.touches){if(!freeTouches.Contains(touch.touchId.ReadValue())||!touch.press.isPressed)continue;if(a==null)a=touch;else if(b==null)b=touch;}
                if(a!=null&&b!=null)
                {
                    float distance=Vector2.Distance(a.position.ReadValue(),b.position.ReadValue());
                    if(pinchDistance>1&&distance>1)SetZoom(desiredZoom*pinchDistance/distance);
                    pinchDistance=distance;
                }
                else
                {
                    pinchDistance=0;
                    if(a!=null){desiredYaw+=a.delta.ReadValue().x/Mathf.Max(1,Screen.width)*240;turning=true;}
                }
            }
            if(UnityEngine.InputSystem.Mouse.current is UnityEngine.InputSystem.Mouse mouse&&mouse.middleButton.isPressed){desiredYaw+=mouse.delta.ReadValue().x*.35f;turning=true;}
            if(UnityEngine.InputSystem.Keyboard.current is UnityEngine.InputSystem.Keyboard keys)
            {
                if(keys.zKey.isPressed){desiredYaw-=90*dt;turning=true;}
                if(keys.xKey.isPressed){desiredYaw+=90*dt;turning=true;}
            }
            // A quick look around only: when the finger is lifted the view goes back to the standard angle,
            // the one names, signs and the whole layout are designed for.
            if(!turning)desiredYaw=0;
            yaw=Mathf.LerpAngle(yaw,desiredYaw,1-Mathf.Exp(-(turning?14:6)*dt));
        }
        Camera cameraComponent;
        bool boundFocus;
        float armDistance, armVelocity;
        readonly CameraOcclusion occlusion = new CameraOcclusion();
        Bounds focusBounds;
        public void SetFocusBounds(Bounds bounds, bool enabled) { focusBounds = bounds; boundFocus = enabled; }
        Vector3 Focus() => boundFocus ? focusBounds.ClosestPoint(target.position + lookOffset) : target.position + lookOffset;
        public Transform Target => target;
        public void Configure(Transform followTarget, LayerMask mask) { target = followTarget; blockers = mask; }
        void Awake()
        {
            cameraComponent = GetComponent<Camera>();
            wheel=new UnityEngine.InputSystem.InputAction("CameraZoom",UnityEngine.InputSystem.InputActionType.Value,"<Mouse>/scroll/y");wheel.Enable();
            if (target == null) { Debug.LogError("ArenaCamera requires a target.", this); enabled = false; }
        }
        void Start() => Snap();
        public void Snap()
        {
            if (target == null) return;
            if (cameraComponent == null) cameraComponent = GetComponent<Camera>();
            occlusion.Restore();
            smoothedFocus = Focus();
            smoothVelocity = Vector3.zero;
            ApplyPose(0f, true);
        }
        void LateUpdate()
        {
            if(!Application.isMobilePlatform&&wheel!=null&&(UnityEngine.EventSystems.EventSystem.current==null||!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())){float amount=wheel.ReadValue<float>();if(Mathf.Abs(amount)>.01f){float notches=Mathf.Abs(amount)>=30?amount/120:amount;SetZoom(desiredZoom*Mathf.Pow(.75f,Mathf.Clamp(notches,-3,3)));}}
            if(rpgFraming)ReadViewGestures();
            zoom=Mathf.Lerp(zoom,desiredZoom,1-Mathf.Exp(-14*Time.unscaledDeltaTime));
            smoothedFocus = Vector3.SmoothDamp(smoothedFocus, Focus(), ref smoothVelocity, followTime, Mathf.Infinity, Time.deltaTime);
            // This is a virtual tracking point, not the camera lens. Crossing a crate
            // must not reset the spring and repeatedly snap the view to the player.
            ApplyPose(Time.deltaTime, false);
        }
        void ApplyPose(float deltaTime, bool snap)
        {
            // Preserve horizontal framing on narrow screens instead of cropping the arena.
            float aspect = Mathf.Max(0.1f, cameraComponent.aspect);
            float tangent = Mathf.Tan(verticalFieldOfView * Mathf.Deg2Rad * 0.5f);
            cameraComponent.fieldOfView = Mathf.Clamp(2f * Mathf.Atan(tangent * Mathf.Max(1f, referenceAspect / aspect)) * Mathf.Rad2Deg, 30f, 95f);
            float armLength = Mathf.Max(0.1f, offset.magnitude*zoom*(rpgFraming&&!Application.isMobilePlatform&&Screen.fullScreen?fullscreenRpgDistance:1f));
            // A sight-line obstruction must not change the pitch or zoom. Only an
            // actual overlap at the lens may retract the arm for camera safety.
            Vector3 direction = offset.sqrMagnitude > 0.001f ? (Quaternion.Euler(0f, yaw, 0f) * offset).normalized : Vector3.up;
            if(rpgFraming&&zoom<FlattenFrom)
            {
                Vector3 level=new Vector3(direction.x,0,direction.z);
                if(level.sqrMagnitude>.0001f)
                {
                    float t=Mathf.SmoothStep(0,1,Mathf.InverseLerp(FlattenFrom,CloseZoom,zoom));
                    Vector3 low=level.normalized*Mathf.Cos(CloseUpPitch*Mathf.Deg2Rad)+Vector3.up*Mathf.Sin(CloseUpPitch*Mathf.Deg2Rad);
                    direction=Vector3.Slerp(direction,low,t).normalized;
                }
            }
            float safeDistance = armLength;
            if (Physics.CheckSphere(smoothedFocus + direction * armLength, collisionRadius, blockers, QueryTriggerInteraction.Ignore))
                safeDistance = ClearDistance(direction, armLength);
            if (snap || safeDistance < armDistance) { armDistance = safeDistance; armVelocity = 0f; }
            else if (deltaTime > 0f) armDistance = Mathf.Min(safeDistance, Mathf.SmoothDamp(armDistance, safeDistance, ref armVelocity, zoomOutTime, Mathf.Infinity, deltaTime));
            impulseTime=Mathf.Max(0,impulseTime-Time.unscaledDeltaTime);Vector3 shake=impulseTime>0?new Vector3(Mathf.Sin(Time.unscaledTime*90),0,Mathf.Cos(Time.unscaledTime*73))*impulse*(impulseTime/.12f):Vector3.zero;
            transform.position = smoothedFocus + direction * armDistance+shake;
            transform.rotation = Quaternion.LookRotation(-direction, Mathf.Abs(direction.y) > 0.999f ? Vector3.forward : Vector3.up);
            occlusion.Update(transform.position, target.position, blockers, visibilityRadius, obstructionReleaseDelay, Time.unscaledTime, obstructionOpacity);
        }
        void OnDisable() => occlusion.Restore();
        void OnDestroy(){occlusion.Restore();wheel?.Dispose();}
        float ClearDistance(Vector3 direction, float armLength)
        {
            if (Physics.SphereCast(smoothedFocus, collisionRadius, direction, out RaycastHit hit, armLength, blockers, QueryTriggerInteraction.Ignore))
                return Mathf.Max(0.01f, hit.distance - collisionPadding);
            return armLength;
        }
    }
}
