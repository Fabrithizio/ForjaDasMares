using UnityEngine;
namespace ForjaDasMares
{
    [CreateAssetMenu(menuName = "Forja/Dungeon style")]
    public sealed class DungeonStyleDefinition : ScriptableObject
    {
        public string styleId;
        public Vector2Int[] mainRoute;
        public Mesh floor, bossFloor, corridorFloor, longWallX, longWallZ, shortWallX, shortWallZ, crate, lantern, gate, chestLid;
        public Material stoneTrim, wood, metal, glow;
        public Mesh Wall(bool alongZ, float length) => length > 8f ? (alongZ ? longWallZ : longWallX) : (alongZ ? shortWallZ : shortWallX);
    }
}
