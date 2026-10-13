using UnityEngine;

namespace ForjaDasMares
{
    [CreateAssetMenu(menuName = "Forja das Mares/Melee Definition")]
    public sealed class MeleeDefinition : ScriptableObject
    {
        [Min(1)] public int damage = 25;
        [Min(0.1f)] public float range = 2.3f;
        [Range(1f, 180f)] public float arcDegrees = 110f;
        [Min(0f)] public float windup = 0.08f;
        [Min(0.01f)] public float recovery = 0.3f;
        void OnValidate() { damage = Mathf.Max(1, damage); range = Mathf.Max(0.1f, range); recovery = Mathf.Max(0.01f, recovery); }
    }
}
