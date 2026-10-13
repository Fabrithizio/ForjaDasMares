using UnityEngine;
namespace ForjaDasMares
{
    [CreateAssetMenu(menuName = "Forja/Biome")]
    public sealed class BiomeDefinition : ScriptableObject
    {
        public string biomeId, displayName;
        public Material floor, wall, accent, decoration;
        public Color ambient = Color.gray, light = Color.white, background = new Color(0.025f, 0.035f, 0.05f);
        public Mesh decorationMesh;
        public Vector3 decorationScale = Vector3.one;
        public EncounterDefinition[] encountersByDepth;
        public EncounterDefinition boss;
        public RoomRewardTable rewards;
    }
}
