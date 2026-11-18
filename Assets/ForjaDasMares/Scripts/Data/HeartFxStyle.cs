using UnityEngine;

namespace ForjaDasMares
{
    // Look of the heart effects of one weapon version (trail, hearts in the air, glow and the burst on contact).
    [CreateAssetMenu(menuName = "Forja/Efeito de corações")]
    public sealed class HeartFxStyle : ScriptableObject
    {
        public Material heart, glow, trail, trailCore, ring;
        public Color main = new Color(1f, .4f, .75f, 1f);
        public Color second = new Color(.75f, .45f, 1f, 1f);
        // Bright line in the middle of the trail and the little sparkles (gold on the final version).
        public Color core = Color.white;
        public Color sparkle = new Color(1f, .85f, .4f, 1f);
        [Header("Voo")]
        [Min(0)] public float trailWidth = .6f;
        [Min(0)] public float trailTime = .3f;
        [Min(0)] public float heartsPerMetre = 3f;
        public Vector2 heartSize = new Vector2(.2f, .4f);
        [Min(0)] public float sparklesPerMetre = 4f;
        [Min(0)] public float haloSize = 1f;
        [Header("Contato")]
        [Min(0)] public int impactHearts = 10;
        [Min(0)] public float impactSize = 1f;
        public Vector2 impactHeartSize = new Vector2(.22f, .45f);
        [Min(0)] public float bigHeartSize = 1.1f;
        public bool bigHeart;
        [Header("Lendária (só a versão final)")]
        // Two spiral ribbons (gold and violet), falling petals, a rune circle on the ground when thrown,
        // rays of light on contact and an aura while the weapon is in the hand.
        public bool legendary;
        public Material petal, rune;
        // Optional mixed colours for the hearts (each heart picks one). Empty = main/second colours.
        public Color[] heartColors;
    }
}
