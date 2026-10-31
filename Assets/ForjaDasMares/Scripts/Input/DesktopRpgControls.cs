using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
namespace ForjaDasMares
{
    // Converts mouse intentions to the same normalized movement/aim used by keyboard and the network.
    [DefaultExecutionOrder(-175)]public sealed class DesktopRpgControls:MonoBehaviour
    {
        public const string ModeKey="DesktopRpgControlMode";
        public static bool MouseMode=>true;
        public static string ModeName=>"MOUSE RPG";
        ArenaInput input;PlayerMotor motor;HeroCombat combat;Camera camera;Health health;
        EnemyPool autoAimEnemies;Health autoAimTarget;float autoAimClock;readonly AimTargetSelector autoAim=new AimTargetSelector();
        public void ConfigureAutoAim(EnemyPool enemies){autoAimEnemies=enemies;autoAimTarget=null;autoAim.Reset();}
        InputAction stationary;NavMeshPath path;readonly Vector3[] corners=new Vector3[64];
        Vector3 queuedAim;bool queuedCast;
        WorldClickTarget interaction;
        public void StopForAbility(){if(Application.isMobilePlatform)return;Cancel();abilityLock=true;motor?.StopWalking();}
        Health target;Vector3 destination,lastPlanned;int count,corner;float replan;bool navigating,pressed,attackGesture,attackRecovery,abilityLock,interactionGesture;
        public bool EffectiveMouseMode=>!Application.isMobilePlatform&&MouseMode&&input!=null&&input.HasPointer;
        public bool StandStill=>stationary!=null&&stationary.enabled&&stationary.IsPressed();
        public bool CanAttack{get;private set;}=true;
        public Health SelectionTarget{get;private set;}
        public bool SelectionAttacking=>target!=null&&target==SelectionTarget&&input!=null&&input.MouseAttackHeld&&CanAttack;
        public Vector2 Movement{get;private set;}
        public string Notice{get;private set;}="";
        public void Configure(ArenaInput controls,PlayerMotor player,Camera view,HeroCombat attacks)
        {input=controls;if(motor!=player){Cancel();motor=player;health=player!=null?player.GetComponent<Health>():null;}camera=view;combat=attacks;if(GetComponent<TargetOutline>()==null)gameObject.AddComponent<TargetOutline>();}
        void Awake()
        {
            path=new NavMeshPath();
            stationary=new InputAction("StandStill",InputActionType.Button);stationary.AddBinding("<Keyboard>/leftShift");stationary.AddBinding("<Keyboard>/rightShift");
        }
        void OnEnable(){stationary?.Enable();}
        void OnDisable(){stationary?.Disable();Cancel();}
        void OnDestroy(){stationary?.Dispose();}
        public void PressMouse(){pressed=true;}
        public void Cancel(){Notice="";navigating=false;target=null;SelectionTarget=null;count=corner=0;Movement=Vector2.zero;pressed=false;attackGesture=attackRecovery=abilityLock=false;interaction=null;interactionGesture=false;queuedCast=false;}
        public Vector3 AimDirection
        {
            get
            {
                if(motor==null)return Vector3.forward;
                if(queuedCast&&combat!=null&&!combat.MovementWindowOpen)return queuedAim;
                if(navigating&&!StandStill&&Movement.sqrMagnitude>.001f)return motor.WorldDirection(Movement);
                if(Application.isMobilePlatform&&autoAimTarget!=null&&autoAimTarget.IsAlive)return Flat(autoAimTarget.transform.position-motor.transform.position);
                if(!Application.isMobilePlatform&&input!=null&&input.HasPointer&&target!=null&&target.IsAlive&&!StandStill)return Flat(target.transform.position-motor.transform.position);
                if(input!=null&&input.HasPointer&&TryPointer(out var point,out _))return Flat(point-motor.transform.position);
                Vector3 movement=motor.WorldDirection(input!=null?input.RawMove:Vector2.zero);return movement.sqrMagnitude>.001f?movement.normalized:motor.transform.forward;
            }
        }
        static Vector3 Flat(Vector3 value){value.y=0;return value.sqrMagnitude>.0001f?value.normalized:Vector3.zero;}
        public bool TryPointer(out Vector3 point,out Health enemy)
        {
            enemy=null;point=Vector3.zero;if(camera==null||input==null||motor==null)return false;
            Ray ray=camera.ScreenPointToRay(input.PointerPosition);
            if(Physics.Raycast(ray,out var hit,500,(1<<0)|(1<<8)|(combat!=null?combat.GeometryMask|combat.TargetMask:(1<<9)|(1<<13)),QueryTriggerInteraction.Ignore))
            {var hp=hit.collider.GetComponentInParent<Health>();if(hp!=null&&hp.Faction!=CombatFaction.Player&&hp.IsAlive){enemy=hp;point=hp.transform.position;return true;}}
            // Scenery is still solid for physics, but must not intercept a ground command.
            if(Physics.Raycast(ray,out var ground,500,1<<8,QueryTriggerInteraction.Ignore)){point=ground.point;return true;}
            var plane=new Plane(Vector3.up,motor.transform.position);if(!plane.Raycast(ray,out float distance))return false;point=ray.GetPoint(distance);return true;
        }
        void Update()
        {
            CanAttack=false;Movement=input!=null?input.RawMove:Vector2.zero;
            if(input==null||motor==null||!input.isActiveAndEnabled||input.CombatBlocked){Cancel();return;}
            if(health!=null&&!health.IsAlive){Cancel();CanAttack=false;return;}
            if(Application.isMobilePlatform||!input.HasPointer)
            {
                if(Application.isMobilePlatform&&autoAimEnemies!=null)
                {
                    autoAimClock-=Time.unscaledDeltaTime;if(autoAimClock<=0){autoAimClock=.12f;autoAim.BeginScan(motor.transform.position,Mathf.Min(12,combat!=null&&combat.BasicAttack!=null?combat.BasicAttack.range:3),combat!=null?combat.GeometryMask:1<<9);foreach(var enemy in autoAimEnemies.Actors)if(enemy.IsAlive)autoAim.Consider(enemy.Health);autoAimTarget=autoAim.EndScan();}
                }
                SelectionTarget=autoAimTarget!=null&&autoAimTarget.IsAlive?autoAimTarget:null;CanAttack=true;pressed=false;navigating=false;target=null;attackGesture=attackRecovery=false;return;
            }
            bool ui=input.PointerIsBlocked;
            if(ui){Cancel();Movement=input.RawMove;return;}
            bool pointer=TryPointer(out var point,out var hovered);
            SelectionTarget=hovered;
            WorldClickTarget clicked=null;
            if(pressed&&camera!=null&&Physics.Raycast(camera.ScreenPointToRay(input.PointerPosition),out var clickHit,500,(1<<0)|(1<<8)|(1<<9)|(combat!=null?combat.TargetMask:1<<13),QueryTriggerInteraction.Ignore))clicked=clickHit.collider.GetComponentInParent<WorldClickTarget>();
            bool groundPress=pressed&&pointer&&hovered==null&&!StandStill;
            if(groundPress){queuedCast=combat!=null&&!combat.MovementWindowOpen;queuedAim=target!=null?Flat(target.transform.position-motor.transform.position):motor.transform.forward;interactionGesture=clicked!=null;interaction=clicked;destination=clicked!=null?clicked.Approach:point;navigating=true;replan=0;count=corner=0;attackGesture=attackRecovery=false;target=null;combat?.CancelCharge();pressed=false;}
            if(interactionGesture&&!input.MouseAttackHeld)interactionGesture=false;
            if(interactionGesture&&interaction==null){Movement=Vector2.zero;motor.StopWalking();pressed=false;return;}
            if(interaction!=null){destination=interaction.Approach;if(!navigating&&(lastPlanned-destination).sqrMagnitude>.2f){navigating=true;replan=0;count=corner=0;}}
            if(interaction!=null&&interaction.InRange(motor.transform)){var use=interaction;interaction=null;navigating=false;Movement=Vector2.zero;motor.StopWalking();use.TryUse(motor.transform);return;}
            if((abilityLock||navigating)&&combat!=null&&!combat.MovementWindowOpen){Movement=Vector2.zero;motor.StopWalking();return;}
            abilityLock=false;
            if(StandStill){navigating=false;target=null;pressed=false;Movement=Vector2.zero;motor.StopWalking();CanAttack=true;attackGesture=input.MouseAttackHeld;attackRecovery|=input.MouseAttackHeld;Notice="";return;}
            bool attackPress=pressed&&hovered!=null;
            if((input.MouseAttackHeld||attackPress)&&hovered!=null){interaction=null;target=hovered;attackGesture=true;attackRecovery=true;}
            if(attackGesture&&(input.MouseAttackHeld||attackPress))
            {
                navigating=false;pressed=false;Movement=Vector2.zero;motor.StopWalking();target=hovered;
                if(target==null){Notice="Aponte para um alvo ou segure Shift para atirar livremente.";return;}
                Vector3 delta=target.transform.position-motor.transform.position;delta.y=0;
                float range=Mathf.Max(.7f,combat!=null&&combat.BasicAttack!=null?combat.BasicAttack.range*.95f:2);
                bool visible=!Physics.Linecast(motor.transform.position+Vector3.up,target.transform.position+Vector3.up,out var hit,combat!=null?combat.GeometryMask:1<<9,QueryTriggerInteraction.Ignore)||hit.collider.GetComponentInParent<Health>()==target;
                CanAttack=delta.sqrMagnitude<=range*range&&visible;
                Notice=CanAttack?"":!visible?"Alvo bloqueado por um obstáculo.":"Fora de alcance. Solte o ataque e aproxime-se.";
                return;
            }
            if(attackGesture){attackGesture=false;target=null;navigating=false;}
            if(attackRecovery&&combat!=null&&!combat.MovementWindowOpen){Movement=Vector2.zero;motor.StopWalking();return;}
            attackRecovery=false;target=null;Notice="";
            if(!EffectiveMouseMode){pressed=false;navigating=false;return;}
            if(input.RawMove.sqrMagnitude>.001f){navigating=false;pressed=false;return;}
            if(pressed&&pointer&&hovered==null){destination=point;navigating=true;replan=0;}
            pressed=false;
            if(input.MouseAttackHeld&&pointer&&hovered==null&&!groundPress&&interaction==null){destination=point;navigating=true;}
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
            if(!NavMesh.SamplePosition(motor.transform.position,out var start,2,filter)||!NavMesh.SamplePosition(point,out var end,4,filter)||Mathf.Abs(end.position.y-point.y)>2||!NavMesh.CalculatePath(start.position,end.position,filter,path)||path.status!=NavMeshPathStatus.PathComplete)
            {Notice="Caminho bloqueado";navigating=false;return false;}
            count=path.GetCornersNonAlloc(corners);corner=count>1?1:0;navigating=count>0;Notice="";return navigating;
        }
    }
}
