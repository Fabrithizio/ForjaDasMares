using UnityEngine;
namespace ForjaDasMares
{
    // Presentation only: real FBX clips, no animation events granting damage.
    // Bosses can also have "Charge" (rush) and "Curse" (marks on the ground) states, and do not flinch on every hit:
    // they flash instead.
    public sealed class RiggedEnemyAnimation:MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] bool flinch=true;
        // Length in seconds of each state's clip (same order as States); 0 = unknown.
        [SerializeField] float[] lengths;
        [SerializeField] Renderer[] glow;
        EnemyActor actor;int state=-1;float flash,blink;bool bound,replica;EnemyState replicaState;
        MaterialPropertyBlock block;Color[] baseGlow;
        static readonly int[] States={Animator.StringToHash("Idle"),Animator.StringToHash("Walk"),Animator.StringToHash("Run"),Animator.StringToHash("Attack"),Animator.StringToHash("Hit"),Animator.StringToHash("Dead"),Animator.StringToHash("Charge"),Animator.StringToHash("Curse")};
        static readonly int Emission=Shader.PropertyToID("_EmissionColor");
        public int CurrentState=>state;
        public void Configure(Animator value){animator=value;}
        public void ConfigureBoss(float[] clipLengths,Renderer[] glowing){flinch=false;lengths=clipLengths;glow=glowing;}
        void Awake(){actor=GetComponentInParent<EnemyActor>();if(animator==null)animator=GetComponentInChildren<Animator>();}
        void OnEnable(){state=-1;flash=blink=0;replica=false;if(actor!=null&&!bound){actor.Health.Damaged+=Hit;bound=true;}}
        void OnDisable(){if(bound&&actor!=null){actor.Health.Damaged-=Hit;bound=false;}Glow(0);}
        void Hit(){if(flinch)flash=.18f;else blink=.12f;}
        public void SetReplica(EnemyState value){replica=true;replicaState=value;}
        public void ResetAuthority(){replica=false;state=-1;flash=blink=0;}
        float Length(int index)=>lengths!=null&&index<lengths.Length?lengths[index]:0;
        void LateUpdate()
        {
            if(actor==null||animator==null)return;
            flash=Mathf.Max(0,flash-Time.deltaTime);bool alive=actor.Health.IsAlive;
            var brain=replica?replicaState:actor.Brain.State;
            float velocity=actor.Motor.Velocity.magnitude;
            var move=replica?null:actor.Combat.CurrentBossMove;
            int attack=3;
            if(move!=null&&move.kind==BossAttackKind.Charge&&animator.HasState(0,States[6]))attack=6;
            else if(move!=null&&move.kind==BossAttackKind.GroundMarks&&animator.HasState(0,States[7]))attack=7;
            int next=!alive?5:flash>0?4:brain==EnemyState.Attack||move!=null?attack:velocity>.15f?(velocity>2?2:1):0;
            if(next!=state){state=next;animator.CrossFadeInFixedTime(States[state],state==5?.15f:.1f,0);}
            float length=Length(state);
            if(length>0)
            {
                // Bosses: each clip lasts exactly as long as the warning plus the recovery of the move.
                float duration=move!=null?move.warning*(actor.Brain.BossPhase==2&&actor.Profile.boss!=null?actor.Profile.boss.phaseTwoTiming:1)+move.recovery:0;
                animator.speed=state==1?Mathf.Clamp(velocity/1.4f,.7f,1.5f):state==2?Mathf.Clamp(velocity/3.6f,.7f,1.5f):state==6?1.3f:(state==3||state==7)&&duration>.2f?Mathf.Clamp(length/duration,.6f,2.5f):1;
            }
            else animator.speed=state==1?Mathf.Clamp(velocity/1.7f,.65f,1.6f):state==2?Mathf.Clamp(velocity/3.3f,.7f,1.6f):state==3?1/Mathf.Max(.4f,actor.Profile.attack.windup+actor.Profile.attack.recovery):state==5?2:1;
            if(blink>0){blink=Mathf.Max(0,blink-Time.deltaTime);Glow(blink>0?1:0);}
        }
        // Brief bright flash of the glowing parts when hit.
        void Glow(float amount)
        {
            if(glow==null||glow.Length==0)return;
            if(block==null)block=new MaterialPropertyBlock();
            if(baseGlow==null){baseGlow=new Color[glow.Length];for(int i=0;i<glow.Length;i++)baseGlow[i]=glow[i]!=null&&glow[i].sharedMaterial!=null&&glow[i].sharedMaterial.HasProperty(Emission)?glow[i].sharedMaterial.GetColor(Emission):Color.black;}
            for(int i=0;i<glow.Length;i++)
            {
                if(glow[i]==null)continue;
                glow[i].GetPropertyBlock(block);block.SetColor(Emission,Color.Lerp(baseGlow[i],new Color(1.6f,2.2f,2.2f),amount));glow[i].SetPropertyBlock(block);
            }
        }
    }
}
