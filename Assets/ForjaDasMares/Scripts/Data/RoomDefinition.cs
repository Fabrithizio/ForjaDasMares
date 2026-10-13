using UnityEngine;
namespace ForjaDasMares
{
    public enum RoomCategory { Entrance, Combat, HardCombat, Treasure, Rest, Event, Trap, Boss }
    [CreateAssetMenu(menuName = "Forja/Room")]
    public sealed class RoomDefinition : ScriptableObject
    {
        public RoomCategory category;
        [Min(10)] public float size = 12;
        [Min(3)] public float doorWidth = 3;
        [Min(1)] public float wallHeight = 1.8f;
        public Vector3[] obstaclePositions;
    }
}
