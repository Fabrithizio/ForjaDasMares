using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
namespace ForjaDasMares
{
    public sealed class DungeonGenerator : MonoBehaviour
    {
        [SerializeField] Mesh cubeMesh, cylinderMesh;
        [SerializeField] RoomDefinition[] roomDefinitions;
        [SerializeField] NavMeshSurface normalNavigation, bossNavigation;
        [SerializeField] AttackDefinition trapAttack;
        GameObject map;
        public DungeonLayout Layout { get; private set; }
        public DungeonRoom[] Rooms { get; private set; }
        public bool NavigationConnected { get; private set; }
        public readonly List<DungeonDoor> Doors = new List<DungeonDoor>(12);
        public readonly List<DungeonTrap> Traps = new List<DungeonTrap>(4);
        public readonly List<DungeonCorridor> Corridors = new List<DungeonCorridor>(12);
        public void Configure(Mesh cube, Mesh cylinder, RoomDefinition[] rooms, NavMeshSurface normal, NavMeshSurface boss, AttackDefinition trap)
        { cubeMesh = cube; cylinderMesh = cylinder; roomDefinitions = rooms; normalNavigation = normal; bossNavigation = boss; trapAttack = trap; }
        public void Generate(DungeonSession session, BiomeDefinition biome, int seed, int maximum, bool fixedMap, TelegraphPool warnings)
        {
            if (map != null) { map.SetActive(false); Destroy(map); }
            Doors.Clear(); Traps.Clear(); Corridors.Clear(); Layout = DungeonLayout.Generate(seed, maximum, fixedMap); Rooms = new DungeonRoom[Layout.rooms.Count];
            map = new GameObject("GeneratedDungeon_Seed_" + seed); map.transform.SetParent(transform);
            foreach (var node in Layout.rooms)
            {
                RoomDefinition data = roomDefinitions[(int)node.category];
                var root = new GameObject("Room_" + node.id + "_" + node.category); root.transform.SetParent(map.transform); root.transform.position = Layout.Center(node.id);
                var room = new DungeonRoom { node = node, root = root }; Rooms[node.id] = room;
                BuildRoom(room, data, biome);
                int difficulty = node.depth <= 1 ? 0 : node.depth <= 2 ? 1 : 2;
                if (node.category == RoomCategory.Combat || node.category == RoomCategory.HardCombat) room.encounter = biome.encountersByDepth[Mathf.Min(difficulty, biome.encountersByDepth.Length - 1)];
                else if (node.category == RoomCategory.Boss) room.encounter = biome.boss;
                room.objectiveComplete = node.category != RoomCategory.Event && node.category != RoomCategory.Trap;
                room.spawnPoints = new[] { root.transform.position + new Vector3(-3, 0.06f, 2), root.transform.position + new Vector3(3, 0.06f, 2), root.transform.position + new Vector3(0, 0.06f, 3) };
                if (node.category == RoomCategory.Treasure || node.category == RoomCategory.Event) Interaction(session, room, DungeonInteractionKind.Chest, new Vector3(-2, 0, -2), biome);
                if (node.category == RoomCategory.Event) Interaction(session, room, DungeonInteractionKind.Crystal, new Vector3(2, 0, -1), biome);
                if (node.category == RoomCategory.Rest) Interaction(session, room, DungeonInteractionKind.Rest, Vector3.zero, biome);
                if (node.category == RoomCategory.Boss) Interaction(session, room, DungeonInteractionKind.Exit, new Vector3(0, 0, 3), biome);
                if (node.category == RoomCategory.Trap)
                {
                    Interaction(session, room, DungeonInteractionKind.TrapLever, new Vector3(3, 0, -2), biome);
                    for (int i = 0; i < 3; i++)
                    {
                        var trap = Piece("TrapPlate_" + i, root.transform, new Vector3((i - 1) * 2.8f, 0.01f, 0), new Vector3(2, 0.02f, 2), biome.accent, false).AddComponent<DungeonTrap>();
                        trap.Configure(session, room, warnings, trapAttack); Traps.Add(trap);
                    }
                }
                room.renderers = root.GetComponentsInChildren<Renderer>(); room.lights = root.GetComponentsInChildren<Light>();
            }
            foreach (var node in Layout.rooms) foreach (int next in node.connections) if (next > node.id) Corridor(Rooms[node.id], Rooms[next], biome);
            Physics.SyncTransforms(); Bake(normalNavigation); Bake(bossNavigation);
            if (normalNavigation.navMeshData == null || bossNavigation.navMeshData == null) throw new InvalidOperationException("Dungeon navigation build failed.");
            NavigationConnected = ValidateNavigation(out string navigationError);
            if (!NavigationConnected) throw new InvalidOperationException(navigationError);
            Debug.Log("DUNGEON_GENERATED seed=" + seed + " rooms=" + Rooms.Length + " signature=" + Layout.Signature());
        }
        static void Bake(NavMeshSurface surface) { var previous = surface.navMeshData; surface.BuildNavMesh(); if (previous != null) UnityEngine.Object.Destroy(previous); }
        GameObject Piece(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collision, Mesh mesh = null, int layer = 9)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale; go.layer = layer;
            go.GetComponent<MeshFilter>().sharedMesh = mesh != null ? mesh : cubeMesh; go.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (collision) go.AddComponent<BoxCollider>(); return go;
        }
        void BuildRoom(DungeonRoom room, RoomDefinition data, BiomeDefinition biome)
        {
            float half = data.size * 0.5f;
            Piece("Floor", room.root.transform, new Vector3(0, -0.2f, 0), new Vector3(data.size + 0.2f, 0.4f, data.size + 0.2f), biome.floor, true, null, 8);
            for (int x = -4; x <= 4; x += 4) for (int z = -4; z <= 4; z += 4) Piece("Tile", room.root.transform, new Vector3(x, 0.004f, z), new Vector3(3.8f, 0.008f, 3.8f), biome.decoration, false, null, 8);
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            foreach (var direction in directions)
            {
                bool door = false; foreach (int next in room.node.connections) if (Layout.rooms[next].coordinate - room.node.coordinate == direction) door = true;
                Vector3 normal = new Vector3(direction.x, 0, direction.y), side = Vector3.Cross(Vector3.up, normal);
                if (door)
                {
                    float length = half - data.doorWidth * 0.5f;
                    for (int sign = -1; sign <= 1; sign += 2) Wall(room.root.transform, normal * half + side * (sign * (data.doorWidth * 0.5f + length * 0.5f)), normal, length, data.wallHeight, biome.wall);
                }
                else Wall(room.root.transform, normal * half, normal, data.size + 0.6f, data.wallHeight, biome.wall);
                var socket = new GameObject("Socket_" + direction.x + "_" + direction.y).transform; socket.SetParent(room.root.transform, false); socket.localPosition = normal * half; socket.localRotation = Quaternion.LookRotation(normal);
            }
            if (data.obstaclePositions != null) foreach (var position in data.obstaclePositions) Piece("Obstacle", room.root.transform, position + Vector3.up * 0.75f, new Vector3(1.2f, 1.5f, 1.2f), biome.wall, true);
            foreach (Vector3 corner in new[] { new Vector3(-4.7f, 0, -4.7f), new Vector3(4.7f, 0, 4.7f) })
            {
                Piece("BiomeDecoration_Placeholder", room.root.transform, corner + Vector3.up * biome.decorationScale.y, biome.decorationScale, biome.accent, false, biome.decorationMesh);
                var lamp = new GameObject("BiomeGlow", typeof(Light)); lamp.transform.SetParent(room.root.transform, false); lamp.transform.localPosition = corner + Vector3.up * 2.1f; var light = lamp.GetComponent<Light>(); light.type = LightType.Point; light.color = biome.light; light.range = 7; light.intensity = 2; light.shadows = LightShadows.None;
            }
        }
        void Wall(Transform parent, Vector3 position, Vector3 normal, float length, float height, Material material)
        {
            Vector3 scale = Mathf.Abs(normal.z) > 0.5f ? new Vector3(length, height, 0.35f) : new Vector3(0.35f, height, length);
            Piece("Wall", parent, position + Vector3.up * height * 0.5f, scale, material, true);
        }
        void Corridor(DungeonRoom a, DungeonRoom b, BiomeDefinition biome)
        {
            var root = new GameObject("Corridor_" + a.node.id + "_" + b.node.id).transform; root.SetParent(map.transform); root.position = (a.Center + b.Center) * 0.5f;
            Vector3 direction = (b.Center - a.Center).normalized; root.rotation = Quaternion.LookRotation(direction);
            Piece("Floor", root, new Vector3(0, -0.2f, 0), new Vector3(3.2f, 0.4f, 4.4f), biome.floor, true, null, 8);
            for (int sign = -1; sign <= 1; sign += 2) Piece("SideWall", root, new Vector3(sign * 1.65f, 0.9f, 0), new Vector3(0.3f, 1.8f, 4.2f), biome.wall, true);
            var door = Piece("EncounterDoor", root, new Vector3(0, 0.9f, 0), new Vector3(3, 1.8f, 0.35f), biome.accent, true).AddComponent<DungeonDoor>(); door.Configure(a.node.id, b.node.id); Doors.Add(door); a.doors.Add(door); b.doors.Add(door);
            var surfaces = new List<Renderer>(); foreach (var renderer in root.GetComponentsInChildren<Renderer>()) if (renderer.gameObject != door.gameObject) surfaces.Add(renderer);
            Corridors.Add(new DungeonCorridor { a = a.node.id, b = b.node.id, renderers = surfaces.ToArray() });
        }
        void Interaction(DungeonSession session, DungeonRoom room, DungeonInteractionKind kind, Vector3 position, BiomeDefinition biome)
        {
            var root = new GameObject(kind.ToString()).transform; root.SetParent(room.root.transform, false); root.localPosition = position;
            Piece("Base", root, Vector3.up * 0.3f, new Vector3(1, 0.6f, 0.8f), biome.wall, kind != DungeonInteractionKind.Exit && kind != DungeonInteractionKind.Rest);
            var part = Piece("MovingPart", root, Vector3.up * (kind == DungeonInteractionKind.Chest ? 0.65f : 1), kind == DungeonInteractionKind.Chest ? new Vector3(1.05f, 0.15f, 0.85f) : new Vector3(0.35f, 0.8f, 0.35f), biome.accent, false, kind == DungeonInteractionKind.Rest ? cylinderMesh : null).transform;
            var interaction = root.gameObject.AddComponent<DungeonInteractable>(); interaction.Configure(session, room, kind, part); room.interactions.Add(interaction);
        }
        public bool ValidateNavigation(out string error)
        {
            error = ""; foreach (var room in Rooms)
            {
                foreach (int type in new[] { normalNavigation.agentTypeID, bossNavigation.agentTypeID })
                {
                    var filter = new NavMeshQueryFilter { agentTypeID = type, areaMask = NavMesh.AllAreas };
                    var path = new NavMeshPath(); if (!NavMesh.CalculatePath(Rooms[0].Center, room.Center, filter, path) || path.status != NavMeshPathStatus.PathComplete) { error = "Navigation cannot reach room " + room.node.id + " for agent " + type; return false; }
                }
            }
            return true;
        }
    }
}
