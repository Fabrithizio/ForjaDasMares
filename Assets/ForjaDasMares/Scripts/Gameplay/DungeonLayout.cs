using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace ForjaDasMares
{
    public sealed class DungeonNode
    {
        public int id, depth;
        public Vector2Int coordinate;
        public RoomCategory category;
        public readonly List<int> connections = new List<int>(4);
    }
    public sealed class DungeonLayout
    {
        public readonly List<DungeonNode> rooms = new List<DungeonNode>(12);
        public const float Spacing = 16;
        public Vector3 Center(int id) => new Vector3(rooms[id].coordinate.x * Spacing, 0, rooms[id].coordinate.y * Spacing);
        static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        static readonly RoomCategory[] Main = { RoomCategory.Entrance, RoomCategory.Combat, RoomCategory.Combat, RoomCategory.Event, RoomCategory.HardCombat, RoomCategory.Trap, RoomCategory.Boss };
        public static DungeonLayout Generate(int seed, int maximumRooms, bool fixedMap)
        {
            maximumRooms = Mathf.Clamp(maximumRooms, 7, 12); var random = new System.Random(seed); var layout = new DungeonLayout();
            var positions = new List<Vector2Int> { Vector2Int.zero, Vector2Int.up };
            if (fixedMap) positions = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 2), new Vector2Int(1, 2), new Vector2Int(0, 2) };
            else if (!Extend(positions, random)) throw new InvalidOperationException("Unable to generate connected main route.");
            for (int i = 0; i < Main.Length; i++) { layout.Add(positions[i], Main[i], i); if (i > 0) layout.Connect(i - 1, i); }
            for (int extra = 7; extra < maximumRooms; extra++)
            {
                bool added = false;
                for (int attempt = 0; attempt < 40 && !added; attempt++)
                {
                    int parent = attempt < 4 ? 1 + (extra - 7) % 2 : random.Next(1, Math.Min(layout.rooms.Count, 5));
                    Vector2Int position = layout.rooms[parent].coordinate + Directions[(attempt + random.Next(4)) % 4];
                    if (layout.Occupied(position)) continue;
                    layout.Add(position, extra == 8 ? RoomCategory.Rest : RoomCategory.Treasure, layout.rooms[parent].depth + 1); layout.Connect(parent, extra); added = true;
                }
                if (!added) break;
            }
            if (!layout.Validate(maximumRooms, out string error)) throw new InvalidOperationException(error); return layout;
        }
        static bool Extend(List<Vector2Int> positions, System.Random random)
        {
            if (positions.Count == 7) return true;
            int first = random.Next(4);
            for (int i = 0; i < 4; i++)
            {
                Vector2Int next = positions[positions.Count - 1] + Directions[(first + i) % 4]; if (positions.Contains(next)) continue;
                positions.Add(next); if (Extend(positions, random)) return true; positions.RemoveAt(positions.Count - 1);
            }
            return false;
        }
        bool Occupied(Vector2Int position) { foreach (var room in rooms) if (room.coordinate == position) return true; return false; }
        void Add(Vector2Int position, RoomCategory category, int depth) => rooms.Add(new DungeonNode { id = rooms.Count, coordinate = position, category = category, depth = depth });
        void Connect(int a, int b) { rooms[a].connections.Add(b); rooms[b].connections.Add(a); }
        public bool Validate(int maximum, out string error)
        {
            error = "";
            if (rooms.Count < 7 || rooms.Count > maximum || rooms[0].category != RoomCategory.Entrance || rooms[6].category != RoomCategory.Boss) { error = "Invalid route length/entry/boss."; return false; }
            var unique = new HashSet<Vector2Int>(); var visited = new HashSet<int>(); var queue = new Queue<int>(); queue.Enqueue(0);
            foreach (var room in rooms)
            {
                if (!unique.Add(room.coordinate)) { error = "Overlapping rooms."; return false; }
                foreach (int next in room.connections)
                    if (next < 0 || next >= rooms.Count || !rooms[next].connections.Contains(room.id) || Mathf.Abs(rooms[next].coordinate.x - room.coordinate.x) + Mathf.Abs(rooms[next].coordinate.y - room.coordinate.y) != 1) { error = "Broken reciprocal door/corridor."; return false; }
            }
            while (queue.Count > 0) { int id = queue.Dequeue(); if (!visited.Add(id)) continue; foreach (int next in rooms[id].connections) queue.Enqueue(next); }
            if (visited.Count != rooms.Count) { error = "Unreachable rooms."; return false; } return true;
        }
        public string Signature()
        { var text = new StringBuilder(); foreach (var room in rooms) { text.Append(room.id).Append(':').Append(room.coordinate.x).Append(',').Append(room.coordinate.y).Append('/').Append((int)room.category).Append('['); foreach (int next in room.connections) text.Append(next).Append(','); text.Append(']'); } return text.ToString(); }
    }
}
