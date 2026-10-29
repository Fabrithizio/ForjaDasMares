using UnityEngine;
namespace ForjaDasMares
{
    // Presentation only: real FBX clips, no animation events granting damage.
    public sealed class RiggedEnemyAnimation:MonoBehaviour
    {
        [SerializeField] Animator animator;
        EnemyActor actor;int state=-1;float flash;bool bound,replica;EnemyState replicaState;
        static readonly int[] States={Animator.StringToHash("Idle"),Animator.StringToHash("Walk"),Animator.StringToHash("Run"),Animator.StringToHash("Attack"),Animator.StringToHash("Hit"),Animator.StringToHash("Dead")};
        public int CurrentState=>state;
        public void Configure(Animator value){animator=value;}
        void Awake(){actor=GetComponentInParent<EnemyActor>();if(animator==null)animator=GetComponentInChildren<Animator>();}
        void OnEnable(){state=-1;flash=0;replica=false;if(actor!=null&&!bound){actor.Health.Damaged+=Hit;bound=true;}}
        void OnDisable(){if(bound&&actor!=null){actor.Health.Damaged-=Hit;bound=false;}}
        void Hit(){flash=.18f;}
        public void SetReplica(EnemyState value){replica=true;replicaState=value;}
        public void ResetAuthority(){replica=false;state=-1;flash=0;}
        void LateUpdate()
        {
            if(actor==null||animator==null)return;
            flash=Mathf.Max(0,flash-Time.deltaTime);bool alive=actor.Health.IsAlive;
            var brain=replica?replicaState:actor.Brain.State;
            float velocity=actor.Motor.Velocity.magnitude;
            int next=!alive?5:flash>0?4:brain==EnemyState.Attack?3:velocity>.15f?(velocity>2?2:1):0;
            if(next!=state){state=next;animator.CrossFadeInFixedTime(States[state],.07f,0);}
            animator.speed=state==1?Mathf.Clamp(velocity/1.7f,.65f,1.6f):state==2?Mathf.Clamp(velocity/3.3f,.7f,1.6f):state==3?1/Mathf.Max(.4f,actor.Profile.attack.windup+actor.Profile.attack.recovery):state==5?2:1;
        }
    }
}
