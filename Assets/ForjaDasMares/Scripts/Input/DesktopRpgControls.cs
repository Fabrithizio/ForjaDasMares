using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
namespace ForjaDasMares
{
    // Converts mouse intentions to the same normalized movement/aim used by keyboard and the network.
    [DefaultExecutionOrder(-175)]public sealed class DesktopRpgControls:MonoBehaviour
    {
        public const string ModeKey="DesktopRpgControlMode";
        public static bool MouseMode=>PlayerPrefs.GetInt(ModeKey,1)==1;
        public static string ModeName=>MouseMode?"MOUSE RPG":"WASD + MIRA";
        public static void ToggleMode(){PlayerPrefs.SetInt(ModeKey,MouseMode?0:1);PlayerPrefs.Save();}
        ArenaInput input;PlayerMotor motor;HeroCombat combat;Camera camera;Health health;
        InputAction stationary,toggle;NavMeshPath path;readonly Vector3[] corners=new Vector3[64];
        Health target;Vector3 destination,lastPlanned;int count,corner;float replan;bool navigating,pressed,previousMode;
        public bool EffectiveMouseMode=>!Application.isMobilePlatform&&MouseMode&&input!=null&&input.HasPointer;
        public bool StandStill=>stationary!=null&&stationary.enabled&&stationary.IsPressed();
        public bool CanAttack{get;private set;}=true;
        public Vector2 Movement{get;private set;}
        public string Notice{get;private set;}="";
        public void Configure(ArenaInput controls,PlayerMotor player,Camera view,HeroCombat attacks)
        {input=controls;if(motor!=player){Cancel();motor=player;health=player!=null?player.GetComponent<Health>():null;}camera=view;combat=attacks;}
        void Awake()
        {
            path=new NavMeshPath();
            stationary=new InputAction("StandStill",InputActionType.Button);stationary.AddBinding("<Keyboard>/leftShift");stationary.AddBinding("<Keyboard>/rightShift");
            toggle=new InputAction("ToggleMovement",InputActionType.Button,"<Keyboard>/f8");toggle.performed+=Toggle;previousMode=MouseMode;
        }
        void Toggle(InputAction.CallbackContext _){if(input!=null&&!input.CombatBlocked){ToggleMode();Cancel();}}
        void OnEnable(){stationary?.Enable();toggle?.Enable();}
        void OnDisable(){stationary?.Disable();toggle?.Disable();Cancel();}
        void OnDestroy(){stationary?.Dispose();if(toggle!=null){toggle.performed-=Toggle;toggle.Dispose();}}
        public void PressMouse(){pressed=true;}
        public void Cancel(){Notice="";navigating=false;target=null;count=corner=0;Movement=Vector2.zero;pressed=false;}
        public Vector3 AimDirection
        {
            get
            {
                if(motor==null)return Vector3.forward;
                if(EffectiveMouseMode&&target!=null&&target.IsAlive&&!StandStill)return Flat(target.transform.position-motor.transform.position);
                if(input!=null&&input.HasPointer&&TryPointer(out var point,out _))return Flat(point-motor.transform.position);
                Vector3 movement=motor.WorldDirection(input!=null?input.RawMove:Vector2.zero);return movement.sqrMagnitude>.001f?movement.normalized:motor.transform.forward;
            }
        }
        static Vector3 Flat(Vector3 value){value.y=0;return value.sqrMagnitude>.0001f?value.normalized:Vector3.zero;}
        public bool TryPointer(out Vector3 point,out Health enemy)
        {
            enemy=null;point=Vector3.zero;if(camera==null||input==null||motor==null)return false;
            Ray ray=camera.ScreenPointToRay(input.PointerPosition);
            if(Physics.Raycast(ray,out var hit,500,(1<<8)|(combat!=null?combat.GeometryMask|combat.TargetMask:(1<<9)|(1<<13)),QueryTriggerInteraction.Ignore))
            {point=hit.point;var hp=hit.collider.GetComponentInParent<Health>();if(hp!=null&&hp.Faction!=CombatFaction.Player&&hp.IsAlive){enemy=hp;point=hp.transform.position;}return true;}
            var plane=new Plane(Vector3.up,motor.transform.position);if(!plane.Raycast(ray,out float distance))return false;point=ray.GetPoint(distance);return true;
        }
        void Update()
        {
            CanAttack=true;Movement=input!=null?input.RawMove:Vector2.zero;
            if(input==null||motor==null||!input.isActiveAndEnabled||input.CombatBlocked){Cancel();return;}
            if(previousMode!=MouseMode){previousMode=MouseMode;bool click=pressed;Cancel();pressed=click;}
            if(health!=null&&!health.IsAlive){Cancel();CanAttack=false;return;}
            bool ui=input.PointerIsBlocked;
            if(!EffectiveMouseMode){if(StandStill)Movement=Vector2.zero;pressed=false;navigating=false;target=null;return;}
            CanAttack=false;
            if(ui){if(pressed)Cancel();pressed=false;Movement=input.RawMove;return;}
            bool pointer=TryPointer(out var point,out var hovered);
            if(StandStill){Cancel();Movement=Vector2.zero;CanAttack=true;return;}
            if(input.RawMove.sqrMagnitude>.001f){navigating=false;target=null;pressed=false;CanAttack=true;return;}
            if(pressed&&pointer){target=hovered;if(target==null){destination=point;navigating=true;replan=0;}}
            pressed=false;
            if(input.MouseAttackHeld&&hovered!=null){target=hovered;navigating=false;}
            else if(input.MouseAttackHeld&&target==null&&pointer){destination=point;navigating=true;}
            if(target!=null&&!target.IsAlive){target=null;navigating=false;}
            if(target!=null)
            {
                if(!input.MouseAttackHeld){target=null;navigating=false;Movement=Vector2.zero;return;}
                Vector3 delta=target.transform.position-motor.transform.position;delta.y=0;
                float range=Mathf.Max(.7f,combat!=null&&combat.BasicAttack!=null?combat.BasicAttack.range*.85f:2);
                bool visible=!Physics.Linecast(motor.transform.position+Vector3.up,target.transform.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore);
                if(delta.sqrMagnitude<=range*range&&visible){Movement=Vector2.zero;CanAttack=true;navigating=false;return;}
                destination=target.transform.position;navigating=true;
            }
            if(!navigating){Movement=Vector2.zero;return;}
            replan-=Time.unscaledDeltaTime;
            if(replan<=0&&(count==0||(lastPlanned-destination).sqrMagnitude>.2f))Plan(destination);
            if(count==0){Movement=Vector2.zero;return;}
            while(corner<count&&(corners[corner]-motor.transform.position).sqrMagnitude<.45f*.45f)corner++;
            if(corner>=count){navigating=false;Movement=Vector2.zero;return;}
            Vector3 direction=Flat(corners[corner]-motor.transform.position);Vector3 forward=motor.WorldDirection(Vector2.up),right=motor.WorldDirection(Vector2.right);
            Movement=Vector2.ClampMagnitude(new Vector2(Vector3.Dot(direction,right),Vector3.Dot(direction,forward)),1);
        }
        public bool Plan(Vector3 point)
        {
            destination=point;replan=.15f;lastPlanned=point;count=corner=0;
            var filter=new NavMeshQueryFilter{agentTypeID=0,areaMask=NavMesh.AllAreas};
            if(!NavMesh.SamplePosition(motor.transform.position,out var start,2,filter)||!NavMesh.SamplePosition(point,out var end,1.5f,filter)||!NavMesh.CalculatePath(start.position,end.position,filter,path)||path.status!=NavMeshPathStatus.PathComplete)
            {Notice="Caminho bloqueado";navigating=false;return false;}
            count=path.GetCornersNonAlloc(corners);corner=count>1?1:0;navigating=count>0;Notice="";return navigating;
        }
    }
}
