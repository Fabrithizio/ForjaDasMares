using System;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    [Serializable]public sealed class SoundscapeCue
    {
        public string displayName;
        public AudioClip music,ambience;
        [Range(0,1)]public float musicGain=.26f,ambienceGain=.16f;
    }
    [CreateAssetMenu(menuName="Forja/Soundscape Library")]
    public sealed class SoundscapeLibrary:ScriptableObject
    {
        public SoundscapeCue menu;
        public SoundscapeCue[] biomes;
        public AudioClip bossMusic,start,victory,defeat;
        [Range(.2f,5)]public float crossfade=2.5f;
    }
}
