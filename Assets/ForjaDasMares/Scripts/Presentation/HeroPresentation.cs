using UnityEngine;
namespace ForjaDasMares
{
    public enum HeroState { Idle, Walk, Attack, Skill, Dodge, Hit, Dead, Victory }
    [DefaultExecutionOrder(50)]
    public sealed class HeroPresentation : MonoBehaviour
    {
        [SerializeField] Transform[] models;
        [SerializeField] PlayerMotor motor;
        [SerializeField] HeroCombat combat;
        [SerializeField] Health health;
        [SerializeField] Animator animator;
        public bool HasCastingView{get;private set;}
        Transform visible;
        HeroModelView visibleView;
        float hitTime, victoryTime;
        Vector3 previousPosition;
        bool hasPosition;
        public EquipmentDefinition EquippedWeapon {get;private set;}
        public float ActionProgress=>ReplicaMode?replicaAnimation:State==HeroState.Dodge?(motor.IsDodging?motor.DodgeProgress:1):State==HeroState.Hit?Mathf.Clamp01(1-hitTime/.2f):combat.NormalizedTime;
        void Start(){var feedback=GetComponent<ActorImpactFeedback>()??gameObject.AddComponent<ActorImpactFeedback>();feedback.Configure(health,GetComponent<HeroRoster>().Current.feedback,this);var reach=GetComponent<HeroAttackArea>()??gameObject.AddComponent<HeroAttackArea>();reach.Configure(this,combat,GetComponent<HeroRoster>());}
        public int ActionVariant=>ReplicaMode?replicaVariant:combat.AnimationVariant;
        public int ActionVersion=>ReplicaMode?replicaVersion:combat.ActionVersion;
        WeaponCategory DefaultWeaponCategory=>HasCastingView?WeaponCategory.Staff:WeaponCategory.Sword;
        public void SetWeapon(EquipmentDefinition value){EquippedWeapon=value;if(visibleView!=null)visibleView.SetWeaponCategory(value!=null?value.category:DefaultWeaponCategory);}
        public HeroState State { get; private set; }
        public Transform Visible => visible;
        public bool ReplicaMode { get; set; }
        public float ChargeProgress=>ReplicaMode?replicaCharge:combat.ChargeProgress;
        float replicaCharge;
        float replicaAnimation,replicaImpact=.25f;int replicaVariant,replicaVersion;
        public void ApplyReplica(HeroState value, float time,int variant=0,int version=0,float impact=.25f,float charge=0) { replicaCharge=Mathf.Clamp01(charge);State = value; replicaAnimation = Mathf.Clamp01(time);replicaVariant=variant;replicaVersion=version;replicaImpact=impact; }
        public void Configure(Transform[] roots, PlayerMotor movement, HeroCombat attacks, Health hp)
        { models = roots; motor = movement; combat = attacks; health = hp; }
        void OnEnable() { if (health != null) health.Damaged += Hit; }
        void OnDisable() { if (health != null) health.Damaged -= Hit; }
        void Hit() { hitTime = 0.2f; victoryTime = 0; }
        public void Show(int slot)
        {
            var feedback=GetComponent<ActorImpactFeedback>();if(feedback!=null){feedback.ResetVisual();feedback.Configure(health,GetComponent<HeroRoster>().Current.feedback,this);}
            for (int i = 0; i < models.Length; i++) models[i].gameObject.SetActive(i == slot);
            visible = models[Mathf.Clamp(slot, 0, models.Length - 1)];HasCastingView=visible.GetComponentInChildren<PrismaCastingView>(true)!=null; visibleView = visible.GetComponentInChildren<HeroModelView>(true); ResetPose();hasPosition=false;
            if(visibleView != null)
            {
                foreach(var view in visible.GetComponentsInChildren<HeroModelView>(true))if(view!=visibleView)view.gameObject.SetActive(false);
                // Legacy referenced parts stay serialized but must never draw over the skin.
                foreach(string name in new[]{"Body_Placeholder","Head_Placeholder","FacingMarker","GreatSword","Staff","Focus","Bow","RuneBlade","Shield"})
                { var part=visible.Find(name);if(part!=null)part.gameObject.SetActive(false); }
            }
        }
        public void ShowVictory() { if (health.IsAlive) victoryTime = 2; }
        public void ResetPose() { hitTime = victoryTime = 0; if (visible != null) { visible.localPosition = Vector3.zero; visible.localRotation = Quaternion.identity; } }
        // Authored Animator parameters must be set before Unity evaluates animation.
        // Existing geometric/replica presentation retains its late update timing.
        void Update(){if(visibleView!=null)Present();}
        void LateUpdate(){if(visibleView==null)Present();}
        void Present()
        {
            if (visible == null) return;
            Vector3 delta=transform.position-previousPosition;delta.y=0;
            float actualSpeed=hasPosition&&Time.deltaTime>0&&delta.sqrMagnitude<4?delta.magnitude/Time.deltaTime:0;
            previousPosition=transform.position;hasPosition=true;
            hitTime = Mathf.Max(0, hitTime - Time.deltaTime); victoryTime = Mathf.Max(0, victoryTime - Time.deltaTime);
            if (!ReplicaMode) State = !health.IsAlive ? HeroState.Dead : hitTime > 0 ? HeroState.Hit : motor.IsDodging ? HeroState.Dodge : combat.IsUsingSpecial ? HeroState.Skill : combat.IsAttacking ? HeroState.Attack : victoryTime > 0 ? HeroState.Victory : motor.PlanarVelocity.sqrMagnitude > 0.1f ? HeroState.Walk : HeroState.Idle;
            if (visibleView != null)
            {
                visibleView.SetWeaponCategory(EquippedWeapon!=null?EquippedWeapon.category:DefaultWeaponCategory);
                visible.localPosition = Vector3.zero; visible.localRotation = Quaternion.identity;
                float time=ActionProgress;
                visibleView.Animate(State, time, actualSpeed, health.IsAlive, ReplicaMode?replicaVariant:combat.AnimationVariant, ReplicaMode?replicaVersion:combat.ActionVersion, ReplicaMode?replicaImpact:combat.ImpactTime);
                return;
            }
            float bob = State == HeroState.Walk ? Mathf.Sin(Time.time * 16) * 0.05f : State == HeroState.Victory ? Mathf.Abs(Mathf.Sin(Time.time * 8)) * 0.25f : 0;
            float lean = State == HeroState.Dodge ? 35 : State == HeroState.Hit ? -18 : State == HeroState.Dead ? 85 : 0;
            float swing = State == HeroState.Attack || State == HeroState.Skill ? Mathf.Sin((ReplicaMode ? replicaAnimation : combat.NormalizedTime) * Mathf.PI * 2) * 45 : 0;
            visible.localPosition = Vector3.up * bob; visible.localRotation = Quaternion.Euler(lean, swing, 0);
            if (animator != null && animator.runtimeAnimatorController != null)
            { animator.SetFloat("Speed", motor.PlanarVelocity.magnitude); animator.SetInteger("State", (int)State); animator.SetBool("Alive", health.IsAlive); }
        }
    }
}
