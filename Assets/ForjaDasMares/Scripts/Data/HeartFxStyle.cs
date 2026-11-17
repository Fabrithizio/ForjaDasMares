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
        public bool bigHeart;
    }
}
