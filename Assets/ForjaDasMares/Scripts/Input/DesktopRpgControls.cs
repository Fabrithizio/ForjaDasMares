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
        public void StopForAbility(){if(UnityEngine.Device.Application.isMobilePlatform)return;Cancel();abilityLock=true;motor?.StopWalking();}
        Health target;Vector3 destination,lastPlanned;int count,corner;float replan;bool navigating,pressed,attackGesture,attackRecovery,abilityLock,interactionGesture;
        public bool EffectiveMouseMode=>!UnityEngine.Device.Application.isMobilePlatform&&MouseMode&&input!=null&&input.HasPointer;
        public bool StandStill=>stationary!=null&&stationary.enabled&&stationary.IsPressed();
        public bool CanAttack{get;private set;}=true;
        public WorldClickTarget HoveredInteraction { get; private set; }
        public Health SelectionTarget{get;private set;}
        public bool SelectionAttacking=>target!=null&&target==SelectionTarget&&input!=null&&input.MouseAttackHeld&&CanAttack;
        public Vector2 Movement{get;private set;}
        public string Notice{get;private set;}="";
        public void Configure(ArenaInput controls,PlayerMotor player,Camera view,HeroCombat attacks)
        {input=controls;if(motor!=player){Cancel();motor=player;health=player!=null?player.GetComponent<Health>():null;}camera=view;combat=attacks;if(GetComponent<TargetOutline>()==null)gameObject.AddComponent<TargetOutline>();}
        void Awake()
        {
            path=new NavMeshPath();
            stationary=Teclas.Criar("parado");
        }
        void OnEnable(){stationary?.Enable();}
        void OnDisable(){stationary?.Disable();Cancel();}
        void OnDestroy(){stationary?.Dispose();}
        public void PressMouse(){pressed=true;}
        // Walking by itself (picked on the world map): to this target, then use it. Any move, click or hit stops it.
        public WorldClickTarget Auto{get;private set;}
        public bool AutoParou{get;set;}
        public void IrAte(WorldClickTarget alvo){Auto=alvo;AutoParou=false;interaction=null;navigating=false;count=corner=0;replan=0;}
        public void PararAuto(){if(Auto==null&&!autoPonto)return;Auto=null;autoPonto=false;AutoParou=true;navigating=false;Movement=Vector2.zero;motor?.StopWalking();}
        // Walking by itself to a spot (picked on the big minimap). False when there is no way there.
        bool autoPonto;Vector3 pontoAuto;
        public bool IndoParaPonto=>autoPonto;
        public bool IrPara(Vector3 ponto){Auto=null;interaction=null;target=null;if(!Plan(ponto))return false;autoPonto=true;pontoAuto=ponto;AutoParou=false;pressed=false;vidaNoCaminho=health!=null?health.Current:-1;return true;}
        int vidaNoCaminho=-1;
        public void Cancel(){Notice="";navigating=false;target=null;SelectionTarget=null;HoveredInteraction=null;count=corner=0;Movement=Vector2.zero;pressed=false;attackGesture=attackRecovery=abilityLock=false;interaction=null;interactionGesture=false;queuedCast=false;atacandoSozinho=false;travado=null;Mirando=null;MirandoAgora=false;if(input!=null)input.AutoAttack=false;}
        // While an ability starts (PC): aim where the mouse is, not where the hero was walking.
        public bool AimAtPointer{get;set;}
        public Vector3 AimDirection
        {
            get
            {
                if(motor==null)return Vector3.forward;
                if(AimAtPointer&&!UnityEngine.Device.Application.isMobilePlatform&&input!=null&&input.HasPointer&&TryPointer(out var aimPoint,out var aimEnemy))
                {var aim=Flat((aimEnemy!=null?aimEnemy.transform.position:aimPoint)-motor.transform.position);if(aim.sqrMagnitude>.001f)return aim;}
                if(queuedCast&&combat!=null&&!combat.MovementWindowOpen)return queuedAim;
                if(navigating&&!StandStill&&Movement.sqrMagnitude>.001f)return motor.WorldDirection(Movement);
                if(UnityEngine.Device.Application.isMobilePlatform&&autoAimTarget!=null&&autoAimTarget.IsAlive)return Flat(autoAimTarget.transform.position-motor.transform.position);
                if(!UnityEngine.Device.Application.isMobilePlatform&&input!=null&&input.HasPointer&&target!=null&&target.IsAlive&&!StandStill)return Flat(target.transform.position-motor.transform.position);
                if(input!=null&&input.HasPointer&&TryPointer(out var point,out _))return Flat(point-motor.transform.position);
                Vector3 movement=motor.WorldDirection(input!=null?input.RawMove:Vector2.zero);return movement.sqrMagnitude>.001f?movement.normalized:motor.transform.forward;
            }
        }
        static Vector3 Flat(Vector3 value){value.y=0;return value.sqrMagnitude>.0001f?value.normalized:Vector3.zero;}
        // The first thing under the cursor, skipping scenery that stands between the camera and the hero (a tree
        // the camera looks through is see-through: clicking "on" it means the ground behind it).
        static readonly RaycastHit[] clickHits=new RaycastHit[16];
        bool ClickRay(Ray ray,int mask,out RaycastHit best)
        {
            best=default;int count=Physics.RaycastNonAlloc(ray,clickHits,500,mask,QueryTriggerInteraction.Ignore);
            System.Array.Sort(clickHits,0,count,HitOrder);
            Vector3 eye=ray.origin,hero=motor!=null?motor.transform.position+Vector3.up*1.1f:eye;Vector3 toHero=hero-eye;float heroDistance=toHero.magnitude;
            for(int i=0;i<count;i++)
            {
                var hit=clickHits[i];
                bool scenery=(hit.collider.gameObject.layer==9||hit.collider.gameObject.layer==0)&&hit.collider.GetComponentInParent<Health>()==null;
                if(scenery&&motor!=null&&hit.distance<heroDistance-1f&&hit.collider.Raycast(new Ray(eye,toHero/heroDistance),out _,heroDistance))continue;
                best=hit;return true;
            }
            return false;
        }
        sealed class ByDistance:System.Collections.Generic.IComparer<RaycastHit>{public int Compare(RaycastHit a,RaycastHit b)=>a.distance.CompareTo(b.distance);}
        static readonly ByDistance HitOrder=new ByDistance();
        public bool TryPointer(out Vector3 point,out Health enemy)
        {
            enemy=null;point=Vector3.zero;if(camera==null||input==null||motor==null)return false;
            // Pointer outside the game view (Device Simulator, window losing focus): no aim, no error.
            var screen=input.PointerPosition;if(!float.IsFinite(screen.x)||!float.IsFinite(screen.y))return false;
            Ray ray=camera.ScreenPointToRay(screen);
            if(Physics.Raycast(ray,out var hit,500,(1<<0)|(1<<8)|(combat!=null?combat.GeometryMask|combat.TargetMask:(1<<9)|(1<<13)),QueryTriggerInteraction.Ignore))
            {var hp=hit.collider.GetComponentInParent<Health>();if(hp!=null&&hp.Faction!=CombatFaction.Player&&hp.IsAlive){enemy=hp;point=hp.transform.position;return true;}}
            // Scenery is still solid for physics, but must not intercept a ground command.
            if(Physics.Raycast(ray,out var ground,500,1<<8,QueryTriggerInteraction.Ignore))
            {
                point=ground.point;
                // Easy to pick: a creature counts as pointed at when the cursor is close to it on the screen (not only exactly on it).
                var near=Magnet(ground.point,screen);if(near!=null){enemy=near;point=near.transform.position;}
                return true;
            }
            var plane=new Plane(Vector3.up,motor.transform.position);if(!plane.Raycast(ray,out float distance))return false;point=ray.GetPoint(distance);return true;
        }
        static readonly Collider[] magnetHits=new Collider[24];
        Health Magnet(Vector3 ground,Vector2 screen)
        {
            int mask=combat!=null?combat.TargetMask:1<<13;
            int n=Physics.OverlapSphereNonAlloc(ground,3f,magnetHits,mask,QueryTriggerInteraction.Ignore);
            Health best=null;float bestDistance=Screen.height*.065f;
            for(int i=0;i<n;i++)
            {
                var hp=magnetHits[i].GetComponentInParent<Health>();
                if(hp==null||!hp.IsAlive||hp.Faction==CombatFaction.Player||hp==health)continue;
                var s=camera.WorldToScreenPoint(hp.transform.position+Vector3.up*.7f);if(s.z<=0)continue;
                float d=Vector2.Distance(new Vector2(s.x,s.y),screen);if(d<bestDistance){bestDistance=d;best=hp;}
            }
            System.Array.Clear(magnetHits,0,n);
            return best;
        }
        bool OnScreen(Health hp)
        {
            if(camera==null||hp==null)return false;
            var v=camera.WorldToViewportPoint(hp.transform.position+Vector3.up*.7f);
            return v.z>0&&v.x>-.02f&&v.x<1.02f&&v.y>-.02f&&v.y<1.02f;
        }
        bool holdTracked,holdFromWorld;

        // ---------- Phone attack ----------
        // Tap ATAQUE: the hero turns to the nearest creature on the screen, locks it and keeps attacking by himself
        // (walking up to it if needed) until it falls or the joystick moves. When it falls, the next one nearby.
        // Hold ATAQUE: a small target appears; dragging from the button works like a joystick to put it on any
        // creature on the screen; letting go locks that one.
        Health travado;bool atacandoSozinho;
        public Health Travado=>atacandoSozinho?travado:null;
        public Health Mirando{get;private set;}
        public Vector3 PontoDaMira{get;private set;}
        public bool MirandoAgora{get;private set;}
        float AlcanceDoAtaque=>Mathf.Max(.7f,combat!=null&&combat.BasicAttack!=null?combat.BasicAttack.range*.95f:2);
        public void ToqueAtaque()
        {
            var nearest=MaisPerto(motor.transform.position,22,null);
            if(nearest!=null){travado=nearest;atacandoSozinho=true;}
            else input.RequestAttack(); // nothing around: one blow forward
        }
        public void MirarComArrasto(Vector2 stick)
        {
            MirandoAgora=true;
            var hero=motor.transform.position;
            PontoDaMira=hero+motor.WorldDirection(Vector2.ClampMagnitude(stick,1))*(Vector2.ClampMagnitude(stick,1).magnitude*16f);
            Mirando=MaisPerto(hero,24,PontoDaMira);
        }
        public void SoltarArrasto()
        {
            if(Mirando!=null){travado=Mirando;atacandoSozinho=true;}
            Mirando=null;MirandoAgora=false;
        }
        static readonly Collider[] nearHits=new Collider[48];
        Health MaisPerto(Vector3 hero,float radius,Vector3? toward)
        {
            int mask=combat!=null?combat.TargetMask:1<<13,walls=combat!=null?combat.GeometryMask:1<<9;
            int n=Physics.OverlapSphereNonAlloc(hero,radius,nearHits,mask,QueryTriggerInteraction.Ignore);
            Health best=null;float bestScore=float.MaxValue;var from=toward??hero;
            for(int i=0;i<n;i++)
            {
                var hp=nearHits[i].GetComponentInParent<Health>();
                if(hp==null||hp==best||!hp.IsAlive||hp.Faction!=CombatFaction.Enemy||!OnScreen(hp))continue;
                if(Physics.Linecast(hero+Vector3.up,hp.transform.position+Vector3.up,out var wall,walls,QueryTriggerInteraction.Ignore)&&wall.collider.GetComponentInParent<Health>()!=hp)continue;
                var d=hp.transform.position-from;d.y=0;float score=d.sqrMagnitude;
                if(score<bestScore){bestScore=score;best=hp;}
            }
            System.Array.Clear(nearHits,0,n);
            return best;
        }
        // Returns true when the hero has to walk to the locked creature (the path code below does it).
        bool Celular()
        {
            pressed=false;target=null;attackGesture=attackRecovery=false;CanAttack=true;input.AutoAttack=false;
            if(atacandoSozinho&&input.RawMove.sqrMagnitude>.06f){atacandoSozinho=false;travado=null;}
            if(atacandoSozinho&&(travado==null||!travado.IsAlive||!OnScreen(travado)))
            {travado=MaisPerto(motor.transform.position,10,null);if(travado==null)atacandoSozinho=false;}
            if(atacandoSozinho)
            {
                autoAimTarget=travado;SelectionTarget=Mirando??travado;
                Vector3 delta=travado.transform.position-motor.transform.position;delta.y=0;
                float range=AlcanceDoAtaque;
                bool visible=!Physics.Linecast(motor.transform.position+Vector3.up,travado.transform.position+Vector3.up,out var hit,combat!=null?combat.GeometryMask:1<<9,QueryTriggerInteraction.Ignore)||hit.collider.GetComponentInParent<Health>()==travado;
                if(delta.sqrMagnitude<=range*range&&visible){navigating=false;Movement=Vector2.zero;motor.StopWalking();input.AutoAttack=true;return false;}
                destination=travado.transform.position;
                if(!navigating||(lastPlanned-destination).sqrMagnitude>1f){navigating=true;replan=0;count=corner=0;}
                return true;
            }
            navigating=false;
            // Free: abilities still aim at the nearest creature within reach.
            autoAimClock-=Time.unscaledDeltaTime;
            if(autoAimClock<=0){autoAimClock=.15f;autoAimTarget=MaisPerto(motor.transform.position,Mathf.Min(12,AlcanceDoAtaque+2),null);}
            SelectionTarget=Mirando??(autoAimTarget!=null&&autoAimTarget.IsAlive?autoAimTarget:null);
            return false;
        }
        void Update()
        {
            HoveredInteraction = null;
            CanAttack=false;Movement=input!=null?input.RawMove:Vector2.zero;
            if(input==null||motor==null||!input.isActiveAndEnabled||input.CombatBlocked){Cancel();return;}
            if(health!=null&&!health.IsAlive){Cancel();CanAttack=false;return;}
            if(autoPonto)
            {
                // It only stops by: clicking on the world to walk (or moving the stick), using an ability, or a hit that
                // takes more than 2% of the life. Closing the map, clicking buttons, scratches: it keeps going.
                bool cliqueNoMundo=pressed&&!input.PointerIsBlocked;
                if(pressed&&!cliqueNoMundo)pressed=false;
                if(health!=null){int agora=health.Current;if(vidaNoCaminho>=0&&vidaNoCaminho-agora>health.Maximum*.02f){PararAuto();return;}vidaNoCaminho=agora;}
                if(input.RawMove.sqrMagnitude>.01f||cliqueNoMundo){PararAuto();return;}
                var falta=pontoAuto-motor.transform.position;falta.y=0;if(falta.sqrMagnitude<1.2f*1.2f){autoPonto=false;navigating=false;Movement=Vector2.zero;motor.StopWalking();return;}
                destination=pontoAuto;
                if(!navigating&&!Plan(destination)){PararAuto();return;}
                goto Seguir;
            }
            if(Auto!=null)
            {
                if(!Auto.isActiveAndEnabled||input.RawMove.sqrMagnitude>.01f||pressed||input.MouseAttackHeld){pressed=false;PararAuto();return;}
                if(Auto.InRange(motor.transform)){var use=Auto;Auto=null;navigating=false;Movement=Vector2.zero;motor.StopWalking();use.TryUse(motor.transform);return;}
                destination=Auto.Approach;
                if((count==0||!navigating)&&!Plan(destination)){PararAuto();return;}
                goto Seguir;
            }
            if(UnityEngine.Device.Application.isMobilePlatform){if(Celular())goto Seguir;return;}
            if(!input.HasPointer){SelectionTarget=null;CanAttack=true;pressed=false;navigating=false;target=null;attackGesture=attackRecovery=false;return;}
            // A hold that started on the world keeps walking even when the cursor slides over the HUD.
            bool overUi=input.PointerIsBlocked;
            if(input.MouseAttackHeld){if(!holdTracked){holdTracked=true;holdFromWorld=!overUi;}}else holdTracked=holdFromWorld=false;
            bool ui=overUi&&!holdFromWorld;
            if(ui){Cancel();Movement=input.RawMove;return;}
            var pointerAt=input.PointerPosition;if(!float.IsFinite(pointerAt.x)||!float.IsFinite(pointerAt.y)){Cancel();return;}
            bool pointer=TryPointer(out var point,out var hovered);
            SelectionTarget=hovered;
            if (camera != null && ClickRay(camera.ScreenPointToRay(input.PointerPosition),(1<<0)|(1<<8)|(1<<9)|(combat!=null?combat.TargetMask:1<<13),out var clickHit))
            {
                var candidate = clickHit.collider.GetComponentInParent<WorldClickTarget>();
                if (candidate == null) WorldClickTarget.Alias.TryGetValue(clickHit.collider, out candidate);
                if (candidate == null) candidate = WorldClickTarget.PertoDoClique(clickHit.point);
                if (candidate != null && candidate.isActiveAndEnabled) HoveredInteraction = candidate;
            }
            WorldClickTarget clicked = pressed ? HoveredInteraction : null;
            if (clicked != null) Debug.Log("CLIQUE_ALVO " + clicked.Label);
            if (HoveredInteraction != null) hovered = SelectionTarget = null;
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
                pressed=false;
                // Locked: once the button was pressed on a creature, keep attacking it while the button is held
                // and it is alive and on the screen, even if the mouse moves away. Pointing at another one switches.
                if(hovered!=null)target=hovered;else if(target!=null&&(!target.IsAlive||!OnScreen(target)))target=null;
                if(target==null){navigating=false;Movement=Vector2.zero;motor.StopWalking();Notice="Aponte para um alvo ou segure Shift para atirar livremente.";return;}
                SelectionTarget=target;
                Vector3 delta=target.transform.position-motor.transform.position;delta.y=0;
                float range=Mathf.Max(.7f,combat!=null&&combat.BasicAttack!=null?combat.BasicAttack.range*.95f:2);
                bool visible=!Physics.Linecast(motor.transform.position+Vector3.up,target.transform.position+Vector3.up,out var hit,combat!=null?combat.GeometryMask:1<<9,QueryTriggerInteraction.Ignore)||hit.collider.GetComponentInParent<Health>()==target;
                CanAttack=delta.sqrMagnitude<=range*range&&visible;
                Notice="";
                if(CanAttack){navigating=false;Movement=Vector2.zero;motor.StopWalking();return;}
                // Too far or behind something: walk towards it, then the attacks start by themselves.
                destination=target.transform.position;
                if(!navigating||(lastPlanned-destination).sqrMagnitude>1f){navigating=true;replan=0;count=corner=0;}
                goto Seguir;
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
            Seguir:
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
            {Notice="Caminho bloqueado";navigating=false;if(interaction!=null)Debug.Log("CAMINHO_BLOQUEADO ate "+interaction.Label+" "+point);return false;}
            count=path.GetCornersNonAlloc(corners);corner=count>1?1:0;navigating=count>0;Notice="";return navigating;
        }
    }
}
