using System.Collections.Generic;
using UnityEngine;
namespace ForjaDasMares
{
    public sealed class DungeonCorridor
    {
        public int a, b;
        public Renderer[] renderers;
        public void SetVisible(bool value) { foreach (var renderer in renderers) renderer.enabled = value; }
    }
    public sealed class DungeonRoom
    {
        public DungeonNode node;
        public GameObject root;
        public Renderer[] renderers;
        public Light[] lights;
        public EncounterDefinition encounter;
        public readonly List<DungeonDoor> doors = new List<DungeonDoor>(4);
        public readonly List<DungeonInteractable> interactions = new List<DungeonInteractable>(2);
        public Vector3[] spawnPoints;
        public bool entered, completed, objectiveComplete, encounterComplete, cooperativeObjective;
        public Vector3 Center => root.transform.position;
        public bool Contains(Vector3 point, float half = 4.8f) => Mathf.Abs(point.x - Center.x) <= half && Mathf.Abs(point.z - Center.z) <= half;
        public bool Locked => entered && !completed && (encounter != null || cooperativeObjective);
        public void SetVisible(bool value)
        {
            foreach (var renderer in renderers) renderer.enabled = value;
            foreach (var light in lights) light.enabled = value;
            foreach (var door in doors) door.SetVisible(value);
        }
    }
}
