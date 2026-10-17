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
        float replicaSpecial,replicaDodge,replicaCharge,replicaSuper,replicaRange;
        public float Charge=>Motor.enabled?Combat.ChargeProgress:replicaCharge;
        public float SuperProgress=>Motor.enabled?Combat.SuperProgress:replicaSuper;
        public float AimRange=>Motor.enabled?Combat.BasicAttack?.range??3:replicaRange;
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
            Roster.Changed+=RefreshFlow;RefreshFlow();
            SetAuthority(false); destination = transform.position; orientation = transform.rotation;
        }
        void RefreshFlow()=>Combat.ConfigureFlow(Roster.Current.onlineFlow);
        void OnDestroy(){if(Roster!=null)Roster.Changed-=RefreshFlow;}
        public void ApplyGear(ActorFrame frame, ProgressionCatalog catalog)
        {
            string signature = frame.hero + ":" + frame.mainItem + ":" + frame.secondaryItem;
            if (signature == gearSignature) return; gearSignature = signature;
            if (mainGear == null) { mainGear = CreateGear("ReplicaMain", new Vector3(.55f, 1, .3f)); secondaryGear = CreateGear("ReplicaSecondary", new Vector3(-.55f, 1, .3f)); }
            var main = catalog.Find(frame.mainItem); var secondary = catalog.Find(frame.secondaryItem);
            ShowGear(mainGear, main); ShowGear(secondaryGear, secondary);
            var model=Presentation.Visible.GetComponentInChildren<HeroModelView>(true);
            if(model!=null)
            {
                model.ShowDefaultWeapon(main==null);
                MountReplica(mainGear,model.PrimarySocket);MountReplica(secondaryGear,model.SecondarySocket);
            }
            foreach (Transform body in transform) if (body.name.StartsWith("HeroVisual_")) foreach (Transform part in body)
                if (part.name == "GreatSword" || part.name == "Bow" || part.name == "Staff" || part.name == "Focus" || part.name == "RuneBlade") part.gameObject.SetActive(main == null);
                else if (part.name == "Shield") part.gameObject.SetActive(secondary == null);
        }
        static void MountReplica(MeshRenderer gear,Transform socket)
        {if(gear==null||socket==null)return;gear.transform.SetParent(socket,false);gear.transform.localPosition=new Vector3(0,-.3f,0);gear.transform.localRotation=Quaternion.Euler(90,0,0);}
        MeshRenderer CreateGear(string label, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = label; go.transform.SetParent(Presentation.Visible, false); go.transform.localPosition = position;
            Destroy(go.GetComponent<Collider>()); var renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = Presentation.Visible.GetComponentInChildren<Renderer>().sharedMaterial; return renderer;
        }
        void ShowGear(MeshRenderer renderer, EquipmentDefinition item)
        {
            renderer.gameObject.SetActive(item != null); if (item == null) return;
            renderer.transform.SetParent(Presentation.Visible, false); renderer.transform.localScale = item.visualScale;
            gearColor ??= new MaterialPropertyBlock(); gearColor.SetColor("_BaseColor", item.color); renderer.SetPropertyBlock(gearColor);
        }
        public void SetAuthority(bool value)
        {
            Motor.enabled = Combat.enabled = value;
            GetComponent<CharacterController>().enabled = value;
            Presentation.ReplicaMode = !value;
        }
        public void Apply(ActorFrame frame)
        {
            if (Roster.Selected != frame.hero) Roster.Select(frame.hero);
            Health.SetReplicaState(frame.hp, frame.maximum);replicaCharge=Mathf.Clamp01(frame.charge);replicaSuper=Mathf.Clamp01(frame.super);replicaRange=Mathf.Clamp(frame.attackRange,.1f,20);replicaSpecial=Mathf.Max(0,frame.specialCooldown);replicaDodge=Mathf.Max(0,frame.dodgeCooldown);
            destination = frame.position; orientation = Quaternion.Euler(0, frame.yaw, 0);
            if (!hasFrame || (destination - transform.position).sqrMagnitude > 16) transform.SetPositionAndRotation(destination, orientation);
            hasFrame = true; Presentation.ApplyReplica((HeroState)frame.state, frame.animation,frame.variant,frame.actionVersion,frame.impact);
        }
        void LateUpdate()
        {
            if(mainGear!=null)mainGear.enabled=Health.IsAlive;if(secondaryGear!=null)secondaryGear.enabled=Health.IsAlive;
            if (Motor == null || Motor.enabled || !hasFrame) return;
            replicaSpecial=Mathf.Max(0,replicaSpecial-Time.unscaledDeltaTime);replicaDodge=Mathf.Max(0,replicaDodge-Time.unscaledDeltaTime);
            float blend = 1 - Mathf.Exp(-18 * Time.unscaledDeltaTime);
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, destination, blend), Quaternion.Slerp(transform.rotation, orientation, blend));
        }
    }
}
