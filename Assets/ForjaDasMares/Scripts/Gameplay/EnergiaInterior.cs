using UnityEngine;

namespace ForjaDasMares
{
    // The hero's inner energy (there is no mana): spent by the dodge (expensive on purpose) and by every ability
    // (each has its own cost). It comes back on its own after a short pause without spending.
    // Runes and items will be able to lower the costs later (CostMultiplier / DodgeMultiplier).
    public sealed class EnergiaInterior : MonoBehaviour
    {
        // Name shown on the screen (easy to change).
        public const string Nome = "FÔLEGO";
        [Min(1)] public float maximum = 100;
        [Min(0)] public float dodgeCost = 40;
        [Min(0)] public float regenPerSecond = 20;
        [Min(0)] public float regenDelay = 1f;
        public float CostMultiplier { get; set; } = 1;
        public float DodgeMultiplier { get; set; } = 1;

        float current = -1, pause;
        public float Current => current < 0 ? maximum : current;
        public float Fraction => Mathf.Clamp01(Current / Mathf.Max(1, maximum));
        // Time of the last attempt without enough energy (the bar blinks red).
        public float LastFail { get; private set; } = -10;

        public static EnergiaInterior Of(Component owner)
        {
            if (owner == null) return null;
            var found = owner.GetComponent<EnergiaInterior>();
            return found != null ? found : owner.gameObject.AddComponent<EnergiaInterior>();
        }

        public bool CanPay(float cost) => Current + .001f >= cost * Mathf.Max(0, CostMultiplier);
        public bool TrySpend(float cost) => Pay(cost * Mathf.Max(0, CostMultiplier));
        public bool TrySpendDodge() => Pay(dodgeCost * Mathf.Max(0, DodgeMultiplier));

        bool Pay(float amount)
        {
            if (amount <= 0) return true;
            if (Current + .001f < amount) { LastFail = Time.time; return false; }
            current = Current - amount; pause = regenDelay;
            return true;
        }

        public void Refill() { current = maximum; pause = 0; }

        void Update()
        {
            if (current < 0) current = maximum;
            if (pause > 0) { pause -= Time.deltaTime; return; }
            if (current < maximum) current = Mathf.Min(maximum, current + regenPerSecond * Time.deltaTime);
        }
    }
}
