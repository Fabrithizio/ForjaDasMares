using System;
using System.Collections.Generic;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    public enum CoopPhase { Offline, Connecting, Lobby, Loading, Playing, Defeat, Victory, Closed }
    [Serializable] public sealed class JoinIntent { public int protocol = CoopLimits.Protocol; public string identity, pin; public int hero; public bool campaign;public int campaignLevel=-1; }
    [Serializable] public sealed class ActorFrame
    {
        public int slot, hero, kind, hp, maximum, state; public bool connected;
        public Vector3 position; public float yaw, animation, specialCooldown, dodgeCooldown; public int variant,actionVersion;public float impact;
        public string mainItem, secondaryItem;
    }
    [Serializable] public sealed class EffectFrame
    {
        public int slot, kind, shape; public Vector3 position, direction, scale;
        public float radius, length, arc, remaining; public Color color;
    }
    [Serializable] public sealed class WorldFrame
    {
        public uint tick; public int phase, localSlot, seed, biome, room, encounter;
        public bool fixedMap, hasKey, finished; public string signature;
        public int campaignLevel=-1;public int expedition;public CampaignSnapshot campaign;
        public DungeonReplicaState world;
        public List<ActorFrame> players = new List<ActorFrame>(4);
        public List<ActorFrame> enemies = new List<ActorFrame>(24);
        public List<EffectFrame> effects = new List<EffectFrame>(96);
    }
    public static class CoopLimits
    {
        public const int Protocol = 3, Players = 4, CommandBytes = 21, MaximumSnapshotBytes = 49152;
        public static bool ValidJoin(JoinIntent intent, int heroCount) => intent != null && intent.protocol == Protocol
            && Guid.TryParseExact(intent.identity, "N", out _) && intent.hero >= 0 && intent.hero < heroCount
            && intent.pin != null && intent.pin.Length <= 12;
    }
}
