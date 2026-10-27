using UnityEngine;
namespace ForjaDasMares.Networking
{
    public sealed class CoopActor : MonoBehaviour
    {
        public HeroRoster Roster { get; private set; }
        public PlayerMotor Motor { get; private set; }
        public HeroCombat Combat { get; private set; }
        public HeroPresentation Presentation { get; private set; }
        public ValidatedHeroCommands Commands { get; private set; }
        public Health Health => Roster.Health;
        bool replicaCounter;public bool CounterReady=>Motor.enabled?GetComponent<CombatBuildEffects>()?.CounterReady??false:replicaCounter;
        float replicaSpecial,replicaDodge,replicaCharge,replicaSuper,replicaRange,replicaFury;
        public float Charge=>Motor.enabled?Combat.ChargeProgress:replicaCharge;
        public float SuperProgress=>Motor.enabled?Combat.SuperProgress:replicaSuper;
        public float AimRange=>Motor.enabled?Combat.BasicAttack?.range??3:replicaRange;
        public float FuryRemaining=>Motor.enabled?Combat.FuryRemaining:replicaFury;
        public bool Enhanced=>Roster.Current.onlineFlow!=null;
        public float SpecialCooldown=>Motor.enabled?Combat.SpecialCooldownRemaining:replicaSpecial;
        public float DodgeCooldown=>Motor.enabled?Motor.DodgeCooldownRemaining:replicaDodge;
        Vector3 destination; Quaternion orientation; bool hasFrame;
        MeshRenderer mainGear, secondaryGear; string gearSignature;
        MaterialPropertyBlock gearColor;
        public void Configure(Transform basis)
        {
            Roster = GetComponent<HeroRoster>(); Motor = GetComponent<PlayerMotor>(); Combat = GetComponent<HeroCombat>(); Presentation = GetComponent<HeroPresentation>();
            Roster.enabled = false; GetComponent<PlayerAim>().enabled = false;
            Commands = GetComponent<ValidatedHeroCommands>(); if (Commands == null) Commands = gameObject.AddComponent<ValidatedHeroCommands>(); Commands.Clear(); Motor.Configure(Commands, basis);
            if(GetComponent<CombatBuildEffects>()==null)gameObject.AddComponent<CombatBuildEffects>();Roster.Changed+=RefreshFlow;RefreshFlow();
            SetAuthority(false); destination = transform.position; orientation = transform.rotation;
        }
        void RefreshFlow()=>Combat.ConfigureFlow(Roster.Current.onlineFlow);
        void OnDestroy(){if(Roster!=null)Roster.Changed-=RefreshFlow;}
        public void ApplyGear(ActorFrame frame, ProgressionCatalog catalog)
        {
            string signature = frame.hero + ":" + frame.mainItem + ":" + frame.secondaryItem;
            if (signature == gearSignature) return; gearSignature = signature;
            if (mainGear == null) { mainGear = CreateGear("ReplicaMain", new Vector3(.55f, 1, .3f)); secondaryGear = CreateGear("ReplicaSecondary", new Vector3(-.55f, 1, .3f)); }
            var main = catalog.Find(frame.mainItem);Presentation.SetWeapon(main); var secondary = catalog.Find(frame.secondaryItem);
            var model=Presentation.Visible.GetComponentInChildren<HeroModelView>(true);
            EquipmentVisualPlacement.Mount(mainGear.transform,model!=null?model.PrimarySocket:Presentation.Visible,model!=null,new Vector3(.55f,1,.3f),Quaternion.identity,main);
            EquipmentVisualPlacement.Mount(secondaryGear.transform,model!=null?model.SecondarySocket:Presentation.Visible,model!=null,new Vector3(-.55f,1,.3f),Quaternion.identity,secondary);
            ShowGear(mainGear,main);ShowGear(secondaryGear,secondary);
            if(model!=null)model.ShowDefaultWeapon(main==null);
            foreach (Transform body in transform) if (body.name.StartsWith("HeroVisual_"))
            {
                bool geometric=body.GetComponentInChildren<HeroModelView>(true)==null;
                foreach (Transform part in body)
                    if (part.name == "GreatSword" || part.name == "Bow" || part.name == "Staff" || part.name == "Focus" || part.name == "RuneBlade") part.gameObject.SetActive(geometric&&main == null);
                    else if (part.name == "Shield") part.gameObject.SetActive(geometric&&secondary == null);
            }
        }
        MeshRenderer CreateGear(string label, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = label; go.transform.SetParent(Presentation.Visible, false); go.transform.localPosition = position;
            Destroy(go.GetComponent<Collider>()); var renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = Presentation.Visible.GetComponentInChildren<Renderer>().sharedMaterial; return renderer;
        }
        void ShowGear(MeshRenderer renderer, EquipmentDefinition item)
        {
            renderer.gameObject.SetActive(item != null); if (item == null) return;
            var mesh=renderer.GetComponent<EquipmentMeshView>()??renderer.gameObject.AddComponent<EquipmentMeshView>();mesh.Apply(item);
        }
        public void ClearReplicaGear(){replicaFury=0;hasFrame=false;if(mainGear!=null)mainGear.gameObject.SetActive(false);if(secondaryGear!=null)secondaryGear.gameObject.SetActive(false);gearSignature=null;}
        public void SetAuthority(bool value,bool equipmentPresentation=false)
        {
            if(value||equipmentPresentation)ClearReplicaGear();
            var equipment=GetComponent<HeroEquipment>();if(equipment!=null)equipment.enabled=value||equipmentPresentation;
            Motor.enabled = Combat.enabled = value;
            GetComponent<CharacterController>().enabled = value;
            Presentation.ReplicaMode = !value;
        }
        public void Apply(ActorFrame frame)
        {
            if (Roster.Selected != frame.hero) Roster.Select(frame.hero);
            replicaCounter=frame.counter;Health.SetReplicaState(frame.hp, frame.maximum);GetComponent<CombatBuildEffects>()?.SetReplicaBoons(frame.boons);replicaFury=Mathf.Clamp(frame.fury,0,12);Presentation.ApplyReplicaFury(replicaFury);replicaCharge=Mathf.Clamp01(frame.charge);replicaSuper=Mathf.Clamp01(frame.super);replicaRange=Mathf.Clamp(frame.attackRange,.1f,20);replicaSpecial=Mathf.Max(0,frame.specialCooldown);replicaDodge=Mathf.Max(0,frame.dodgeCooldown);
            destination = frame.position; orientation = Quaternion.Euler(0, frame.yaw, 0);
            if (!hasFrame || (destination - transform.position).sqrMagnitude > 16) transform.SetPositionAndRotation(destination, orientation);
            hasFrame = true; Presentation.ApplyReplica((HeroState)frame.state, frame.animation,frame.variant,frame.actionVersion,frame.impact,frame.charge);
        }
        void LateUpdate()
        {
            if(mainGear!=null)mainGear.enabled=Health.IsAlive;if(secondaryGear!=null)secondaryGear.enabled=Health.IsAlive;
            if (Motor == null || Motor.enabled || !hasFrame) return;
            replicaFury=Mathf.Max(0,replicaFury-Time.unscaledDeltaTime);
            replicaSpecial=Mathf.Max(0,replicaSpecial-Time.unscaledDeltaTime);replicaDodge=Mathf.Max(0,replicaDodge-Time.unscaledDeltaTime);
            float blend = 1 - Mathf.Exp(-18 * Time.unscaledDeltaTime);
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, destination, blend), Quaternion.Slerp(transform.rotation, orientation, blend));
        }
    }
}
