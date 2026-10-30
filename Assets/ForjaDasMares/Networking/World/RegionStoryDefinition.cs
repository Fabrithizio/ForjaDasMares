using System;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    public enum RegionalInteraction { Cartographer, Artificer, Clue, Catacombs }
    [Serializable]public sealed class RegionalStoryPoint
    {
        public string label;
        public RegionalInteraction interaction;
        public Vector3 position;
        [Range(0,2)]public int clue;
        [TextArea]public string discovery;
    }
    [CreateAssetMenu(menuName="Forja/Region Story")]
    public sealed class RegionStoryDefinition:ScriptableObject
    {
        public string questId="sino-ato1-v1",title="O sino que não responde";
        [Range(1,3)]public int requiredClues=2;
        [Min(1)]public float interactionDistance=3;
        public RegionalStoryPoint[] points=Array.Empty<RegionalStoryPoint>();
    }
}
