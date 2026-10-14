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
        public StatSnapshot Stats { get; private set; }
        public EquipmentDefinition MainWeapon { get; private set; }
        public void Configure(HeroRoster roster, PlayerMotor player, HeroCombat attacks, MeshRenderer primary, MeshRenderer secondary)
        { heroes = roster; motor = player; combat = attacks; mainVisual = primary; secondaryVisual = secondary; }
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
            if (this == null || heroes == null || service == null) return;
            var profile = service.Profile; var loadout = profile.Loadout(heroes.Current.heroId);
            string state = heroes.Current.heroId + ":" + profile.experience + ":" + (loadout != null ? string.Join(",", loadout.items) : "");
            if (state == signature) return; signature = state;
            Stats = ProgressionStats.Calculate(heroes.Current, profile, catalog); heroes.Health.ApplyAttributes(Stats.maximumHealth, Stats.defense); heroes.Health.ConfigureDefense(this);
            motor.ApplyMovementAttributes(Stats.speed, Stats.cooldownMultiplier);
            var main = loadout != null ? profile.Item(loadout.items[0]) : null; var off = loadout != null ? profile.Item(loadout.items[1]) : null;
            MainWeapon = main != null ? catalog.Find(main.definitionId) : null; var secondary = off != null ? catalog.Find(off.definitionId) : null;
            var visual = heroes.GetComponent<HeroPresentation>().Visible;
            if (visual != null) { mainVisual.transform.SetParent(visual, false); secondaryVisual.transform.SetParent(visual, false); }
            combat.ApplyEquipment(MainWeapon != null ? MainWeapon.attacks : null, Stats, main != null ? catalog.Rarity(main.rarity).power : 1, MainWeapon != null && MainWeapon.twinStrike || secondary != null && secondary.category == WeaponCategory.Daggers, main != null && main.rarity == ItemRarity.Legendary ? catalog.legendaryEcho : null);
            Visual(mainVisual, MainWeapon); Visual(secondaryVisual, secondary);
            // Keep the original geometric hero body/poses; hide its old weapon only when an item replaces it.
            foreach (Transform root in heroes.transform) if (root.name.StartsWith("HeroVisual_")) foreach (Transform part in root)
                if (part.name == "GreatSword" || part.name == "Bow" || part.name == "Staff" || part.name == "Focus" || part.name == "RuneBlade") part.gameObject.SetActive(MainWeapon == null);
                else if (part.name == "Shield") part.gameObject.SetActive(secondary == null);
        }
        static void Visual(MeshRenderer renderer, EquipmentDefinition weapon)
        {
            if (renderer == null) return; renderer.gameObject.SetActive(weapon != null); if (weapon == null) return;
            renderer.transform.localScale = weapon.visualScale; var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", weapon.color); renderer.SetPropertyBlock(block);
        }
        public float DamageMultiplier(Vector3 direction, Health source, AttackDefinition attack)
        { return Stats.block > 0 && Vector3.Dot(-direction.normalized, transform.forward) > .5f ? 1 - Stats.block : 1; }
        void Update()
        {
            if (service == null || !heroes.Health.IsAlive || Stats.recovery <= 0) return;
            regeneration += Time.deltaTime * Stats.recovery; if (regeneration >= 1) { int points = Mathf.FloorToInt(regeneration); regeneration -= points; heroes.Health.Heal(points); }
        }
    }
}
