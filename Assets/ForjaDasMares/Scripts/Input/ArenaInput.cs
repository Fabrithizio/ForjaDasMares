using UnityEngine;
using UnityEngine.InputSystem;

namespace ForjaDasMares
{
    [DefaultExecutionOrder(-200)]
    public sealed class ArenaInput : MonoBehaviour, IHeroCommands, IContinuousCombatCommands, IWorldAimCommands
    {
        [SerializeField] InputActionAsset actions;
        InputActionAsset runtimeActions;
        InputActionMap gameplay;
        InputAction move, pointer;
        bool attackPending, dodgePending, resetPending, pointerMode;
        bool specialPending, nextPending, damagePending, victoryPending, touchHeld;
        int heroSlot = -1;
        Vector2 touchMove;
        bool blocked, deviceAttackHeld, uiAttackHeld, superPending;
        InputAction superAction;
        DesktopRpgControls desktop;
        public DesktopRpgControls Desktop=>desktop;
        public bool MouseAttackHeld=>deviceAttackHeld;
        public bool PointerIsBlocked=>HasPointer&&PointerBlocked!=null&&PointerBlocked(PointerPosition);
        bool PointerAttackAllowed=>!PointerIsBlocked&&(desktop==null||!desktop.EffectiveMouseMode||desktop.CanAttack);
        public bool AttackHeld => !blocked && (uiAttackHeld || deviceAttackHeld&&PointerAttackAllowed);
        public Vector3 AimDirection=>desktop!=null?desktop.AimDirection:transform.forward;
        public void ConfigureDesktop(PlayerMotor player,Camera view,HeroCombat attacks)
        {if(desktop==null)desktop=GetComponent<DesktopRpgControls>()??gameObject.AddComponent<DesktopRpgControls>();desktop.Configure(this,player,view,attacks);}
        public bool CombatBlocked => blocked;
        public void SetUiAttackHeld(bool held) { if(!blocked && held && !uiAttackHeld) attackPending=true; uiAttackHeld = !blocked && held; }
        public void RequestAttack() { if(!blocked) attackPending=true; }
        public void RequestSuper() { if(!blocked) superPending = true; }
        public bool TakeSuper() { bool value = !blocked && superPending; superPending = false; return value; }
        public System.Predicate<Vector2> PointerBlocked { get; set; }
        public void BlockCommands(bool value) { blocked = value; ClearRequests(); }
        public InputActionAsset Actions => actions;

        public Vector2 RawMove => blocked ? Vector2.zero : touchHeld ? touchMove : move != null && move.enabled ? Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f) : Vector2.zero;
        public Vector2 Move=>blocked?Vector2.zero:desktop!=null&&desktop.isActiveAndEnabled?desktop.Movement:RawMove;
        public void SetTouchMove(Vector2 value, bool held) { touchHeld = held; touchMove = Vector2.ClampMagnitude(value, 1f); if (held) pointerMode = false; }
        public Vector2 PointerPosition => pointer != null && pointer.enabled ? pointer.ReadValue<Vector2>() : Vector2.zero;
        public bool HasPointer => !blocked && pointerMode && pointer != null && pointer.enabled;
        public void Configure(InputActionAsset asset) => actions = asset;

        void Awake()
        {
            if (actions == null) { Debug.LogError("ArenaInput requires the ArenaControls asset.", this); enabled = false; return; }
            runtimeActions = Instantiate(actions);
            gameplay = runtimeActions.FindActionMap("Gameplay", true);
            move = gameplay.FindAction("Move", true);
            pointer = gameplay.FindAction("Point", true);
            pointer.performed += OnPoint;
            move.performed += OnMove;
            gameplay.FindAction("Attack", true).performed += OnAttack;
            gameplay.FindAction("Attack", true).canceled += _ => deviceAttackHeld = false;
            superAction = gameplay.FindAction("Super");
            if(superAction == null) { superAction = gameplay.AddAction("Super", InputActionType.Button); superAction.AddBinding("<Keyboard>/q"); superAction.AddBinding("<Gamepad>/rightShoulder"); }
            superAction.performed += _ => RequestSuper();
            gameplay.FindAction("Dodge", true).performed += OnDodge;
            gameplay.FindAction("Reset", true).performed += OnReset;
            var ability=gameplay.FindAction("Special");if(ability!=null&&!Application.isMobilePlatform)ability.AddBinding("<Mouse>/rightButton");
            BindOptional("Special", () => specialPending = true);
            BindOptional("NextHero", () => nextPending = true);
            BindOptional("TestDamage", () => damagePending = true);
            BindOptional("Victory", () => victoryPending = true);
            for (int i = 0; i < 4; i++) { int slot = i; BindOptional("Hero" + (i + 1), () => heroSlot = slot); }
        }
        void BindOptional(string name, System.Action callback)
        { InputAction action = gameplay.FindAction(name); if (action != null) action.performed += _ => callback(); }

        void OnEnable() => gameplay?.Enable();
        void OnDisable() { gameplay?.Disable(); ClearRequests(); }
        void OnDestroy() { if (runtimeActions != null) Destroy(runtimeActions); }
        void OnApplicationFocus(bool focus) { if (!focus) ClearRequests(); }
        void OnPoint(InputAction.CallbackContext _) => pointerMode = !Application.isMobilePlatform;
        void OnMove(InputAction.CallbackContext context) { if (context.control.device is Gamepad) pointerMode = false; }
        void OnAttack(InputAction.CallbackContext context)
        { if(context.control.device is Mouse){pointerMode=!Application.isMobilePlatform;desktop?.PressMouse();}if (context.control.device is Gamepad) pointerMode = false; attackPending = true; deviceAttackHeld = true; }
        void OnDodge(InputAction.CallbackContext _) => dodgePending = true;
        void OnReset(InputAction.CallbackContext _) => resetPending = true;
        void ClearRequests() { desktop?.Cancel();deviceAttackHeld = uiAttackHeld = superPending = false; attackPending = dodgePending = resetPending = specialPending = nextPending = damagePending = victoryPending = false; heroSlot = -1; SetTouchMove(Vector2.zero, false); }
        public bool TakeAttack() { bool value = !blocked && attackPending && (uiAttackHeld||PointerAttackAllowed); attackPending = false; return value; }
        public bool TakeDodge() { bool value = !blocked && dodgePending; dodgePending = false; return value; }
        public bool TakeReset() { bool value = !blocked && resetPending; resetPending = false; return value; }
        public bool TakeSpecial() { bool value = !blocked && specialPending; specialPending = false; return value; }
        public bool TakeNextHero() { bool value = !blocked && nextPending; nextPending = false; return value; }
        public int TakeHeroSlot() { int value = blocked ? -1 : heroSlot; heroSlot = -1; return value; }
        public bool TakeTestDamage() { bool value = !blocked && damagePending; damagePending = false; return value; }
        public bool TakeVictory() { bool value = !blocked && victoryPending; victoryPending = false; return value; }
    }
}
