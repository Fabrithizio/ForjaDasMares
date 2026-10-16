using UnityEngine;
namespace ForjaDasMares
{
    // An optional authored model; heroes without one retain geometric presentation.
    public sealed class HeroModelView : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] Transform primarySocket, secondarySocket;
        [SerializeField] GameObject defaultWeapon;
        static readonly int Speed = Animator.StringToHash("Speed"), State = Animator.StringToHash("State"), Alive = Animator.StringToHash("Alive");
        static readonly int ActionTime = Animator.StringToHash("ActionTime");
        int lastState = -1, lastVersion = -1;
        static readonly int[] AttackStates = { Animator.StringToHash("Base Layer.Attack"), Animator.StringToHash("Base Layer.Attack2"), Animator.StringToHash("Base Layer.Attack3"), Animator.StringToHash("Base Layer.Charged"), Animator.StringToHash("Base Layer.Super") };
        static readonly int[] States = {
            Animator.StringToHash("Base Layer.Idle"), Animator.StringToHash("Base Layer.Walk"),
            Animator.StringToHash("Base Layer.Attack"), Animator.StringToHash("Base Layer.Skill"),
            Animator.StringToHash("Base Layer.Dodge"), Animator.StringToHash("Base Layer.Hit"),
            Animator.StringToHash("Base Layer.Dead"), Animator.StringToHash("Base Layer.Victory") };
        public Animator Animator => animator;
        public Transform PrimarySocket => primarySocket;
        public Transform SecondarySocket => secondarySocket;
        public GameObject DefaultWeapon => defaultWeapon;
        public void Configure(Animator player, Transform main, Transform offhand, GameObject weapon)
        { animator=player;primarySocket=main;secondarySocket=offhand;defaultWeapon=weapon; }
        public void ShowDefaultWeapon(bool visible) { if(defaultWeapon!=null)defaultWeapon.SetActive(visible); }
        void OnEnable() { lastState = lastVersion = -1; }
        public void Animate(HeroState state,float normalizedAttack,float movement,bool alive,int variant=0,int version=0,float impact=.25f)
        {
            if(animator==null||!animator.isActiveAndEnabled||animator.runtimeAnimatorController==null)return;
            animator.SetInteger(State,(int)state);animator.SetBool(Alive,alive);
            animator.SetFloat(Speed,state==HeroState.Walk?Mathf.Clamp(movement>0.1f?movement/5.8f:1,.4f,1.6f):1);
            bool action=state==HeroState.Attack||state==HeroState.Skill;
            int target=action&&(state==HeroState.Attack||variant==4)?AttackStates[Mathf.Clamp(variant,0,4)]:States[(int)state];
            // A single owner switches states. Motion-time sampling aligns the contact pose
            // with the actual hit; no Play() restarts or competing AnyState transitions.
            float contact=variant>=3||state==HeroState.Skill?.5f:.3f;
            float split=Mathf.Clamp(impact,.05f,.9f), time=Mathf.Clamp01(normalizedAttack);
            animator.SetFloat(ActionTime,action?(time<=split?time/split*contact:Mathf.Lerp(contact,1,(time-split)/(1-split))):time);
            if(lastState!=target || action&&lastVersion!=version)
            {
                if(lastState==-1)animator.Play(target,0,0);
                else animator.CrossFadeInFixedTime(target,state==HeroState.Hit?.035f:.065f,0,0);
                lastState=target;lastVersion=version;
            }
        }
    }
}
