using UnityEngine;
namespace ForjaDasMares
{
    [CreateAssetMenu(menuName="Forja/Combat feel")]
    public sealed class CombatFeelDefinition:ScriptableObject
    {
        public AudioClip impact,swing,heavy,step,reward,perfect;
        public Material trailMaterial;
        [Range(0,1)]public float volume=.32f;
        [Range(0,.08f)]public float impactPause=.035f;
        [Range(0,.1f)]public float cameraImpulse=.025f;
    }
}
