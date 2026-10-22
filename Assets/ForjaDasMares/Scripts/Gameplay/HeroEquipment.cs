using UnityEngine;
namespace ForjaDasMares
{
    public sealed class HeroEquipment : MonoBehaviour, IHitDefense
    {
        [SerializeField] HeroRoster heroes;
        [SerializeField] PlayerMotor motor;
        [SerializeField] HeroCombat combat;
        [SerializeField] MeshRenderer mainVisual, secondaryVisual;
        ProfileService service;
        ProgressionCatalog catalog;
        string signature;
        float regeneration;
        bool campaignSolo;
        public bool CampaignSolo => campaignSolo;
        public void SetCampaignSolo(bool value)
        {
            if(campaignSolo==value)return;
            int capacity=heroes.Health.Maximum, hp=heroes.Health.Current;
            campaignSolo=value;signature=null;Refresh();
            // Preserve the health ratio, rounding down: joining/leaving cannot heal the player.
            heroes.Health.SetReplicaState(Mathf.FloorToInt((float)hp*heroes.Health.Maximum/Mathf.Max(1,capacity)),heroes.Health.Maximum);
        }
        Vector3 mainRestPosition, offRestPosition;
        Quaternion mainRestRotation, offRestRotation;
        void Awake() => CaptureRestPose();
        void OnEnable(){signature=null;Refresh();}
        void OnDisable(){if(mainVisual!=null)mainVisual.gameObject.SetActive(false);if(secondaryVisual!=null)secondaryVisual.gameObject.SetActive(false); }
        void CaptureRestPose()
        {
            if(mainVisual!=null){mainRestPosition=mainVisual.transform.localPosition;mainRestRotation=mainVisual.transform.localRotation;}
            if(secondaryVisual!=null){offRestPosition=secondaryVisual.transform.localPosition;offRestRotation=secondaryVisual.transform.localRotation;}
        }
        public StatSnapshot Stats { get; private set; }
        public EquipmentDefinition MainWeapon { get; private set; }
        public void Configure(HeroRoster roster, PlayerMotor player, HeroCombat attacks, MeshRenderer primary, MeshRenderer secondary)
        { heroes = roster; motor = player; combat = attacks; mainVisual = primary; secondaryVisual = secondary; CaptureRestPose(); }
        public void Initialize(ProfileService profile, ProgressionCatalog data)
        {
            heroes.Changed -= HeroChanged;
            if (service != null) service.Changed -= Refresh;
            service = profile; catalog = data; regeneration = 0;
            heroes.Changed += HeroChanged; service.Changed += Refresh; HeroChanged();
        }
        void HeroChanged() { signature = null; Refresh(); }
        void OnDestroy() { if (heroes != null) heroes.Changed -= HeroChanged; if (service != null) service.Changed -= Refresh; }
        public void Refresh()
        {
            if (this == null || !enabled || heroes == null || service == null) return;
            var profile = service.Profile; var loadout = profile.Loadout(heroes.Current.heroId);
            string state = heroes.Current.heroId + ":" + HeroTalents.Signature(profile,heroes.Current.heroId) + ":" + ProgressionStats.Level(profile.experience,catalog.maximumLevel) + ":" + (loadout != null ? string.Join(",", loadout.items) : "");
            if (state == signature) return; signature = state;
            var calculated=ProgressionStats.Calculate(heroes.Current, profile, catalog);
            if(campaignSolo)calculated=ProgressionStats.WithSoloBonus(calculated);
            Stats=calculated;heroes.Health.ApplyAttributes(Stats.maximumHealth, Stats.defense); heroes.Health.ConfigureDefense(this);
            motor.ApplyMovementAttributes(Stats.speed, Stats.cooldownMultiplier);
            var main = loadout != null ? profile.Item(loadout.items[0]) : null; var off = loadout != null ? profile.Item(loadout.items[1]) : null;
            MainWeapon = main != null ? catalog.Find(main.definitionId) : null;heroes.GetComponent<HeroPresentation>().SetWeapon(MainWeapon); var secondary = off != null ? catalog.Find(off.definitionId) : null;
            var visual = heroes.GetComponent<HeroPresentation>().Visible;
            if (visual != null)
            {
                var model = visual.GetComponentInChildren<HeroModelView>(true);
                Mount(mainVisual,model!=null?model.PrimarySocket:visual,model!=null,mainRestPosition,mainRestRotation,MainWeapon);
                Mount(secondaryVisual,model!=null?model.SecondarySocket:visual,model!=null,offRestPosition,offRestRotation,secondary);
            }
            combat.ApplyEquipment(MainWeapon != null ? MainWeapon.attacks : null, Stats, main != null ? catalog.Rarity(main.rarity).power : 1, MainWeapon != null && MainWeapon.twinStrike || secondary != null && secondary.category == WeaponCategory.Daggers, main != null && main.rarity == ItemRarity.Legendary ? catalog.legendaryEcho : null,MainWeapon!=null?MainWeapon.ability:null);
            var effects=GetComponent<CombatBuildEffects>()??gameObject.AddComponent<CombatBuildEffects>();BuildSynergy synergy=BuildSynergy.None;if(loadout!=null)foreach(var id in loadout.items){var owned=profile.Item(id);var item=owned!=null?catalog.Find(owned.definitionId):null;if(item!=null)synergy|=item.synergy;}effects.Configure(MainWeapon!=null?MainWeapon.perk:WeaponPerk.None,synergy);
            Visual(mainVisual, MainWeapon); Visual(secondaryVisual, secondary);
            // Keep the original geometric hero body/poses; hide its old weapon only when an item replaces it.
            foreach (Transform root in heroes.transform) if (root.name.StartsWith("HeroVisual_"))
            {
                var model=root.GetComponentInChildren<HeroModelView>(true);
                if(model!=null){model.ShowDefaultWeapon(MainWeapon==null);continue;}
                foreach (Transform part in root)
                if (part.name == "GreatSword" || part.name == "Bow" || part.name == "Staff" || part.name == "Focus" || part.name == "RuneBlade") part.gameObject.SetActive(MainWeapon == null);
                else if (part.name == "Shield") part.gameObject.SetActive(secondary == null);
            }
        }
        static void Mount(MeshRenderer visual,Transform parent,bool socket,Vector3 restPosition,Quaternion restRotation,EquipmentDefinition item)
        {
            if(visual!=null)EquipmentVisualPlacement.Mount(visual.transform,parent,socket,restPosition,restRotation,item);
        }
        static void Visual(MeshRenderer renderer, EquipmentDefinition weapon)
        {
            if (renderer == null) return; renderer.gameObject.SetActive(weapon != null); if (weapon == null) return;
            var mesh=renderer.GetComponent<EquipmentMeshView>()??renderer.gameObject.AddComponent<EquipmentMeshView>();mesh.Apply(weapon);
        }
        public float DamageMultiplier(Vector3 direction, Health source, AttackDefinition attack)
        { return Stats.block > 0 && Vector3.Dot(-direction.normalized, transform.forward) > .5f ? 1 - Stats.block : 1; }
        void LateUpdate(){if(heroes==null)return;if(mainVisual!=null)mainVisual.enabled=heroes.Health.IsAlive;if(secondaryVisual!=null)secondaryVisual.enabled=heroes.Health.IsAlive;}
        void Update()
        {
            if (service == null || !heroes.Health.IsAlive || Stats.recovery <= 0) return;
            regeneration += Time.deltaTime * Stats.recovery; if (regeneration >= 1) { int points = Mathf.FloorToInt(regeneration); regeneration -= points; heroes.Health.Heal(points); }
        }
    }
}
