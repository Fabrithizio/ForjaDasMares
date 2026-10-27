using UnityEngine;
namespace ForjaDasMares
{
    // One animation owner: locomotion below the waist, masked actions above it.
    public sealed class HeroModelView : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] bool authoredAnimationsReady=true;
        public bool AuthoredAnimationsReady=>authoredAnimationsReady;
        [SerializeField] bool fitEquipmentToBody;
        public bool FitsEquipmentToBody=>fitEquipmentToBody;
        [SerializeField, Min(.1f)] float staffLength=1.45f, bookLength=.38f, shieldLength=.60f, focusLength=.30f, otherWeaponLength=.95f;
        public void ConfigureEquipmentFit(bool value)=>fitEquipmentToBody=value;
        public float EquipmentScale(EquipmentDefinition item)
        {
            if(!fitEquipmentToBody||item==null||item.visualMesh==null)return 1;
            Vector3 size=Vector3.Scale(item.visualMesh.bounds.size,item.authoredScale);
            float length=Mathf.Max(Mathf.Abs(size.x),Mathf.Abs(size.y),Mathf.Abs(size.z));
            float maximum=item.category==WeaponCategory.Staff?staffLength:item.category==WeaponCategory.Grimoire?bookLength:item.category==WeaponCategory.Shield?shieldLength:item.category==WeaponCategory.MagicWeapon?focusLength:otherWeaponLength;
            return length>.0001f?Mathf.Min(1,maximum/length):1;
        }
        [SerializeField] Transform primarySocket, secondarySocket;
        [SerializeField] GameObject defaultWeapon;
        [SerializeField, Min(.1f)] float walkStride=.9173f, runStride=2.4571f;
        static readonly int Speed=Animator.StringToHash("Speed"), State=Animator.StringToHash("State"), Alive=Animator.StringToHash("Alive");
        static readonly int ActionTime=Animator.StringToHash("ActionTime"), GaitTime=Animator.StringToHash("GaitTime");
        static readonly string[] Names={"Idle","Walk","Attack","Skill","Dodge","Hit","Dead","Victory"};
        static readonly string[] Attacks={"Attack","Attack2","Attack3","Charged","Super"};
        static readonly int[] BaseStates=Hashes("Base Layer.",Names),UpperStates=Hashes("Upper Body.",Names);
        static readonly int[] BaseAttacks=Hashes("Base Layer.",Attacks),UpperAttacks=Hashes("Upper Body.",Attacks);
        static readonly int Run=Animator.StringToHash("Base Layer.Run");
        static int[] Hashes(string prefix,string[] names)
        {var result=new int[names.Length];for(int i=0;i<names.Length;i++)result[i]=Animator.StringToHash(prefix+names[i]);return result;}
        static readonly string[][] WeaponMoves={new[]{"SwordSlash","SwordReturn","SwordThrust","HeavySword"},new[]{"AxeCleave","AxeBackhand","AxeSpin","HeavyAxe"},new[]{"HammerSweep","HammerBack","HammerSlam","HeavyHammer"},new[]{"StaffBolt","StaffReturn","StaffRelease","HeavyStaff"},new[]{"BookWord","BookPage","BookSeal","HeavyBook"},new[]{"OrbCast","OrbReturn","OrbBurst","HeavyOrb"}};
        static readonly int[][] WeaponBase={Hashes("Base Layer.",WeaponMoves[0]),Hashes("Base Layer.",WeaponMoves[1]),Hashes("Base Layer.",WeaponMoves[2]),Hashes("Base Layer.",WeaponMoves[3]),Hashes("Base Layer.",WeaponMoves[4]),Hashes("Base Layer.",WeaponMoves[5])};
        static readonly int[][] WeaponUpper={Hashes("Upper Body.",WeaponMoves[0]),Hashes("Upper Body.",WeaponMoves[1]),Hashes("Upper Body.",WeaponMoves[2]),Hashes("Upper Body.",WeaponMoves[3]),Hashes("Upper Body.",WeaponMoves[4]),Hashes("Upper Body.",WeaponMoves[5])};
        WeaponCategory weaponCategory;
        public void SetWeaponCategory(WeaponCategory value)=>weaponCategory=value;
        int AttackHash(int variant,bool upper,bool skill)
        {
            int group=weaponCategory==WeaponCategory.Axe?1:weaponCategory==WeaponCategory.Hammer?2:weaponCategory==WeaponCategory.Staff?3:weaponCategory==WeaponCategory.Grimoire?4:weaponCategory==WeaponCategory.MagicWeapon?5:0;
            // Caster specials have their own two-handed ritual, not the basic third shot.
            if(skill&&group>=3)return upper?UpperStates[3]:BaseStates[3];
            int index=skill?2:Mathf.Clamp(variant,0,3);int hash=(upper?WeaponUpper:WeaponBase)[group][index];
            return animator.HasState(upper?1:0,hash)?hash:skill?(upper?UpperStates[3]:BaseStates[3]):upper?UpperAttacks[Mathf.Clamp(variant,0,4)]:BaseAttacks[Mathf.Clamp(variant,0,4)];
        }
        int lastState=-1,lastUpper=-1,lastVersion=-1;
        bool running,wantsDefaultWeapon=true;
        float phase,upperWeight;
        public Animator Animator=>animator;
        public Transform PrimarySocket=>primarySocket;
        public Transform SecondarySocket=>secondarySocket;
        public GameObject DefaultWeapon=>defaultWeapon;
        public float GaitPhase=>phase;
        public float StanceFraction=>running?.28f:.60f;
        public float MovementSpeed {get;private set;}
        public bool LocomotionActive {get;private set;}
        public HeroState CurrentState {get;private set;}
        public void Configure(Animator player,Transform main,Transform offhand,GameObject weapon,bool animationsReady=true)
        {animator=player;primarySocket=main;secondarySocket=offhand;defaultWeapon=weapon;authoredAnimationsReady=animationsReady;}
        public void ShowDefaultWeapon(bool visible){wantsDefaultWeapon=visible;if(defaultWeapon!=null)defaultWeapon.SetActive(visible);}
        void OnEnable(){lastState=lastUpper=lastVersion=-1;phase=upperWeight=0;running=false;}
        float pauseUntil,heldActionTime;
        public void PauseImpact(float duration){pauseUntil=Mathf.Max(pauseUntil,Time.unscaledTime+Mathf.Clamp(duration,0,.06f));}
        public void Animate(HeroState state,float normalizedAttack,float movement,bool alive,int variant=0,int version=0,float impact=.25f)
        {
            if(Time.unscaledTime<pauseUntil)normalizedAttack=heldActionTime;else heldActionTime=normalizedAttack;
            if(defaultWeapon!=null)defaultWeapon.SetActive(wantsDefaultWeapon&&alive);
            // A newly integrated skin can expose its sockets/FX while its own clips
            // are being prepared. Never play incompatible clips on another rig.
            if(!authoredAnimationsReady)
            {
                CurrentState=state;MovementSpeed=Mathf.Max(0,movement);LocomotionActive=false;
                return;
            }
            if(animator==null||!animator.isActiveAndEnabled||animator.runtimeAnimatorController==null)return;
            CurrentState=state;MovementSpeed=Mathf.Max(0,movement);
            LocomotionActive=alive&&movement>.12f&&state!=HeroState.Dodge&&state!=HeroState.Dead;
            if(running?movement<2.6f:movement>3.2f)running=!running;
            if(LocomotionActive)phase=Mathf.Repeat(phase+movement*Time.deltaTime/(running?runStride:walkStride),1);
            animator.SetFloat(GaitTime,phase);animator.SetFloat(Speed,1);
            animator.SetInteger(State,(int)state);animator.SetBool(Alive,alive);
            bool action=state==HeroState.Attack||state==HeroState.Skill;
            bool attackVariant=action&&(state==HeroState.Attack||variant==4);
            int variantIndex=Mathf.Clamp(variant,0,4);
            float contact=variant>=3||state==HeroState.Skill?.5f:.3f;
            float split=Mathf.Clamp(impact,.05f,.9f),time=Mathf.Clamp01(normalizedAttack);
            animator.SetFloat(ActionTime,action?(time<=split?time/split*contact:Mathf.Lerp(contact,1,(time-split)/(1-split))):time);
            bool masked=LocomotionActive&&(action||state==HeroState.Hit||state==HeroState.Victory)&&animator.layerCount>1;
            int target=LocomotionActive?(running?Run:BaseStates[1]):action&&variant!=4?AttackHash(variantIndex,false,state==HeroState.Skill):attackVariant?BaseAttacks[variantIndex]:BaseStates[(int)state];
            if(!LocomotionActive&&state==HeroState.Walk)target=BaseStates[0];
            if(lastState!=target||!masked&&action&&lastVersion!=version)
            {
                if(lastState==-1)animator.Play(target,0,0);
                else animator.CrossFadeInFixedTime(target,state==HeroState.Hit?.04f:action?.08f:.12f,0,0);
                lastState=target;
            }
            if(animator.layerCount>1)
            {
                int upper=action&&variant!=4?AttackHash(variantIndex,true,state==HeroState.Skill):attackVariant?UpperAttacks[variantIndex]:UpperStates[(int)state];
                if(masked&&(upper!=lastUpper||action&&version!=lastVersion))
                {animator.CrossFadeInFixedTime(upper,.07f,1,0);lastUpper=upper;}
                upperWeight=Mathf.MoveTowards(upperWeight,masked?1:0,Time.deltaTime/0.10f);
                animator.SetLayerWeight(1,upperWeight);
            }
            lastVersion=version;
        }
    }
}
