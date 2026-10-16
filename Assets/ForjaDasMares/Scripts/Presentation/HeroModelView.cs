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
        public void Animate(HeroState state,float normalizedAttack,float movement,bool alive)
        {
            if(animator==null||!animator.isActiveAndEnabled||animator.runtimeAnimatorController==null)return;
            animator.SetInteger(State,(int)state);animator.SetBool(Alive,alive);
            animator.SetFloat(Speed,state==HeroState.Walk?Mathf.Clamp(movement>0.1f?movement/5.8f:1,.4f,1.6f):1);
            // Attack timing follows the existing combat clock; clips never grant damage.
            if(state==HeroState.Attack||state==HeroState.Skill)animator.Play(States[(int)state],0,Mathf.Clamp01(normalizedAttack));
        }
    }
}
