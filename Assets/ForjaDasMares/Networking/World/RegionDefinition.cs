using System;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    [Serializable]public sealed class RegionCampDefinition
    {
        public string label;public Vector3 center;public Vector3[] spawns;public EnemyDefinition[] enemies;
        [Min(10)]public float respawnSeconds=45;
    }
    [CreateAssetMenu(menuName="Forja/Regional World")]
    public sealed class RegionDefinition:ScriptableObject
    {
        public string regionId="costa-ruinas",displayName="Costa das Ruínas";
        public Vector3 refuge=new Vector3(0,.1f,-50),portal=new Vector3(0,.1f,70);
        public Vector3[] route;public RegionCampDefinition[] camps;
        public RegionConnection[] connections=Array.Empty<RegionConnection>();
        public RegionLandmark[] landmarks=Array.Empty<RegionLandmark>();
        public Bounds bounds=new Bounds(new Vector3(0,0,8),new Vector3(160,40,160));
        [Range(4,20)]public int activeEnemyLimit=12;
    }
    [Serializable]public struct RegionConnection {public Vector3 from,to;public RegionConnection(Vector3 a,Vector3 b){from=a;to=b;}}
    [Serializable]public struct RegionLandmark {public string label;public Vector3 position;public Color color;}
}
