using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    public enum CoopInventoryAction { Equip, Unequip, ClaimDelivery, Discard }
    [Serializable] public sealed class CoopInventoryIntent { public CoopInventoryAction action; public string item; public EquipmentSlot slot; public bool confirmed; }
    [Serializable] public sealed class CoopProfileFrame
    { public int revision, deliveryCount; public bool hasRun, hasResult; public string message; public PlayerProfile profile; }
    public sealed class CoopProfileChannel : MonoBehaviour
    {
        CoopDirector director; CoopProgression authority;
        readonly int[] sent = new int[4];
        readonly int[] wireSequence = new int[4];
        readonly float[] lastRequest = new float[4];
        readonly float[] lastResync=new float[4];float initialRetry;
        CoopProfileFrame local;
        public PlayerProfile LocalProfile => director.Manager.IsServer && director.LocalSlot >= 0 ? authority.Profile(director.LocalSlot)?.Profile : local?.profile;
        public string Message { get; private set; }
        public int DeliveryCount => director.Manager.IsServer && director.LocalSlot >= 0 ? authority.Profile(director.LocalSlot)?.Profile.deliveries.Count ?? 0 : local?.deliveryCount ?? 0;
        public int Revision => director.Manager.IsServer && director.LocalSlot >= 0 ? authority.Revision(director.LocalSlot) : local?.revision ?? 0;
        public ProgressionCatalog Catalog => authority.Catalog;
        public void Configure(CoopDirector session, CoopProgression progression) { director = session; authority = progression; }
        public void Bind()
        {
            Array.Clear(sent, 0, sent.Length); Array.Clear(wireSequence, 0, wireSequence.Length); local = null;
            initialRetry=0;Array.Clear(lastResync,0,lastResync.Length);
            director.Manager.CustomMessagingManager.RegisterNamedMessageHandler("fdm.profile.v1", Receive);
            director.Manager.CustomMessagingManager.RegisterNamedMessageHandler("fdm.inventory.v1", ReceiveIntent);
            director.Manager.CustomMessagingManager.RegisterNamedMessageHandler("fdm.profile.request.v1",ReceiveInitialRequest);
        }
        void Update()
        {
            if(director==null||director.Manager.IsServer||!director.Manager.IsConnectedClient||director.LocalSlot<0||local!=null)return;
            initialRetry-=Time.unscaledDeltaTime;if(initialRetry>0)return;initialRetry=1.5f;
            using var writer=new FastBufferWriter(4,Allocator.Temp);writer.WriteValueSafe(CoopLimits.Protocol);
            director.Manager.CustomMessagingManager.SendNamedMessage("fdm.profile.request.v1",NetworkManager.ServerClientId,writer,NetworkDelivery.ReliableSequenced);
        }
        void ReceiveInitialRequest(ulong sender,FastBufferReader reader)
        {
            if(!director.Manager.IsServer||!director.TryGetSlot(sender,out int slot)||reader.Length-reader.Position!=4||Time.unscaledTime-lastResync[slot]<1)return;
            reader.ReadValueSafe(out int protocol);if(protocol!=CoopLimits.Protocol)return;
            lastResync[slot]=Time.unscaledTime;Resend(slot);
        }
        public void Resend(int slot) => sent[slot] = 0;
        public void Sync(ulong client, int slot)
        {
            var source = authority.Profile(slot); if (source == null || sent[slot] == authority.Revision(slot)) return;
            if (client == director.Manager.LocalClientId) { sent[slot] = authority.Revision(slot); return; }
            var value = source.Profile;
            // History and the full delivery stock are not resent in every packet. Inventory/run/result are bounded by the catalog.
            var visible = new PlayerProfile { campaignCleared=value.campaignCleared, experience = value.experience, coins = value.coins, inventory = value.inventory, loadouts = value.loadouts, options = value.options, expedition = value.expedition==null?null:new ExpeditionData{runId=value.expedition.runId,heroId=value.expedition.heroId,location=value.expedition.location,experience=value.expedition.experience,coins=value.expedition.coins,items=value.expedition.items}, lastResult = value.lastResult, deliveries = value.deliveries.GetRange(0, Math.Min(8, value.deliveries.Count)), completedRuns = new List<string>() };
            var packet = new CoopProfileFrame { revision = ++wireSequence[slot], deliveryCount = value.deliveries.Count, hasRun = value.expedition != null, hasResult = value.lastResult != null, profile = visible, message = authority.Error ?? source.Message };
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(packet));
            if (bytes.Length > CoopLimits.MaximumSnapshotBytes) { Message = "Perfil excedeu o limite de replicação."; return; }
            using var writer = new FastBufferWriter(bytes.Length + 4, Allocator.Temp); writer.WriteValueSafe(bytes.Length); writer.WriteBytesSafe(bytes);
            director.Manager.CustomMessagingManager.SendNamedMessage("fdm.profile.v1", client, writer, NetworkDelivery.ReliableFragmentedSequenced); sent[slot] = authority.Revision(slot);
        }
        void Receive(ulong sender, FastBufferReader reader)
        {
            if (director.Manager.IsServer || sender != NetworkManager.ServerClientId || reader.Length - reader.Position < 4) return;
            reader.ReadValueSafe(out int size); if (size <= 0 || size > CoopLimits.MaximumSnapshotBytes || size != reader.Length - reader.Position) return;
            var bytes = new byte[size]; reader.ReadBytesSafe(ref bytes, size); CoopProfileFrame packet;
            try { packet = JsonUtility.FromJson<CoopProfileFrame>(Encoding.UTF8.GetString(bytes)); } catch (Exception) { return; }
            if (packet?.profile == null || packet.revision < 1 || local != null && packet.revision <= local.revision) return;
            if (!packet.hasRun) packet.profile.expedition = null; if (!packet.hasResult) packet.profile.lastResult = null;
            local = packet; Message = packet.message;
        }
        public void Request(CoopInventoryAction action, string item, EquipmentSlot slot, bool confirmed = false)
        {
            if (director.LocalSlot < 0 || !director.Manager.IsConnectedClient) return;
            var intent = new CoopInventoryIntent { action = action, item = item, slot = slot, confirmed = confirmed };
            if (director.Manager.IsServer) { Apply(director.LocalSlot, intent); return; }
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(intent)); using var writer = new FastBufferWriter(bytes.Length + 4, Allocator.Temp);
            writer.WriteValueSafe(bytes.Length); writer.WriteBytesSafe(bytes); director.Manager.CustomMessagingManager.SendNamedMessage("fdm.inventory.v1", NetworkManager.ServerClientId, writer);
        }
        void ReceiveIntent(ulong sender, FastBufferReader reader)
        {
            if (!director.Manager.IsServer || !director.TryGetSlot(sender, out int slot) || reader.Length - reader.Position < 4) return;
            if (Time.unscaledTime - lastRequest[slot] < .1f) return; lastRequest[slot] = Time.unscaledTime;
            reader.ReadValueSafe(out int size); if (size <= 0 || size > 384 || size != reader.Length - reader.Position) return;
            byte[] bytes = new byte[size]; reader.ReadBytesSafe(ref bytes, size); CoopInventoryIntent intent;
            try { intent = JsonUtility.FromJson<CoopInventoryIntent>(Encoding.UTF8.GetString(bytes)); } catch (Exception) { return; }
            Apply(slot, intent);
        }
        void Apply(int slot, CoopInventoryIntent intent)
        {
            if (intent == null || !Enum.IsDefined(typeof(CoopInventoryAction), intent.action) || !Enum.IsDefined(typeof(EquipmentSlot), intent.slot) || intent.item != null && intent.item.Length > 64 || !authority.CanManage(slot)) return;
            var profile = authority.Profile(slot); if (profile == null) return; string hero = director.Actor(slot).Roster.Current.heroId;
            bool success = intent.action == CoopInventoryAction.Equip ? profile.Equip(hero, intent.item, intent.slot)
                : intent.action == CoopInventoryAction.Unequip ? profile.Unequip(hero, intent.slot)
                : intent.action == CoopInventoryAction.ClaimDelivery ? profile.ClaimDelivery(intent.item) : profile.Discard(intent.item, intent.confirmed);
            Message = profile.Message; if (!success) sent[slot] = 0;
        }
    }
}
