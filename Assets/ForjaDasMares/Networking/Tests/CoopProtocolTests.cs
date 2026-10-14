using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace ForjaDasMares.Networking.Tests
{
    public sealed class CoopProtocolTests
    {
        [Test] public void JoinSchemaRejectsBadVersionIdentityAndHero()
        {
            var join = new JoinIntent { identity = "11111111111111111111111111111111", pin = "TEST42", hero = 0 };
            Assert.IsTrue(CoopLimits.ValidJoin(join, 4)); join.protocol = 99; Assert.IsFalse(CoopLimits.ValidJoin(join, 4));
            join.protocol = CoopLimits.Protocol; join.hero = 4; Assert.IsFalse(CoopLimits.ValidJoin(join, 4));
            join.hero = 0; join.identity = "arbitrary-identity"; Assert.IsFalse(CoopLimits.ValidJoin(join, 4));
        }
        [Test] public void SnapshotRoundtripPreservesCollectionsMasksAndSlots()
        {
            var frame = new WorldFrame { tick = 7, localSlot = 2, phase = (int)CoopPhase.Playing, world = new DungeonReplicaState { completed = 511, doors = 8, interactions = 15, room = 3 } };
            for (int i = 0; i < 4; i++) frame.players.Add(new ActorFrame { slot = i, hero = i, connected = true, hp = 50, maximum = 100, position = Vector3.right * i });
            frame.enemies.Add(new ActorFrame { slot = 5, hp = 3 }); frame.effects.Add(new EffectFrame { slot = 2, color = Color.red });
            var copy = JsonUtility.FromJson<WorldFrame>(JsonUtility.ToJson(frame));
            Assert.AreEqual(4, copy.players.Count); Assert.AreEqual(1, copy.enemies.Count); Assert.AreEqual(1, copy.effects.Count);
            Assert.AreEqual(511u, copy.world.completed); Assert.AreEqual(8u, copy.world.doors); Assert.AreEqual(Vector3.right * 2, copy.players[2].position);
        }
        [Test] public void NamedMessageHeaderDoesNotCountAsCommandPayload()
        {
            using var writer = new FastBufferWriter(32, Allocator.Temp);
            writer.WriteValueSafe(1ul); writer.WriteValueSafe(9u); writer.WriteValueSafe(0f); writer.WriteValueSafe(1f); writer.WriteValueSafe(0f); writer.WriteValueSafe(1f); writer.WriteValueSafe((byte)HeroButtons.Attack);
            using var reader = new FastBufferReader(writer, Allocator.Temp); reader.ReadValueSafe(out ulong header);
            Assert.AreEqual(CoopLimits.CommandBytes, reader.Length - reader.Position);
            Assert.AreNotEqual(CoopLimits.CommandBytes, reader.Length);
        }
    }
}
