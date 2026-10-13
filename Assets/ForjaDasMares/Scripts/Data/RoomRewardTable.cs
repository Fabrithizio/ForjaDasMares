using UnityEngine;
namespace ForjaDasMares
{
    [CreateAssetMenu(menuName = "Forja/Reward Table")]
    public sealed class RoomRewardTable : ScriptableObject
    {
        [Min(0)] public int minimumCoins = 5, maximumCoins = 12;
        [Min(0)] public int victoryCoins = 4;
        public bool eventGrantsOptionalKey = true;
    }
}
