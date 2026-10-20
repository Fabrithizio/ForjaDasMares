using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    public enum AdventureAction { ChooseBoon, UseFeature, Ping }
    // Clients send only an action and bounded indices. All proximity, identity and rewards are host decisions.
    public sealed class CampaignAdventureChannel:MonoBehaviour
    {
        CoopDirector session;readonly float[] requests=new float[4];float localRequest;
        public void Configure(CoopDirector owner){session=owner;}
        public void Bind(){System.Array.Clear(requests,0,4);session.Manager.CustomMessagingManager.RegisterNamedMessageHandler("fdm.adventure.v1",Receive);}
        public void Request(AdventureAction action,int id=0,int choice=0)
        {
            if(session==null||session.LocalSlot<0||session.Phase!=CoopPhase.Playing||Time.unscaledTime<localRequest)return;localRequest=Time.unscaledTime+.25f;
            if(session.Manager.IsServer){Handle(session.LocalSlot,action,id,choice);return;}
            using var writer=new FastBufferWriter(12,Allocator.Temp);writer.WriteValueSafe((int)action);writer.WriteValueSafe(id);writer.WriteValueSafe(choice);session.Manager.CustomMessagingManager.SendNamedMessage("fdm.adventure.v1",NetworkManager.ServerClientId,writer,NetworkDelivery.ReliableSequenced);
        }
        void Receive(ulong sender,FastBufferReader reader)
        {
            if(!session.Manager.IsServer||!session.TryGetSlot(sender,out int slot)||reader.Length-reader.Position!=12)return;
            reader.ReadValueSafe(out int action);reader.ReadValueSafe(out int id);reader.ReadValueSafe(out int choice);Handle(slot,(AdventureAction)action,id,choice);
        }
        public bool Handle(int slot,AdventureAction action,int id,int choice)
        {
            if(session==null||!session.Manager.IsServer||session.Phase!=CoopPhase.Playing||slot<0||slot>=4||!session.IsConnected(slot)||Time.unscaledTime<requests[slot]||!System.Enum.IsDefined(typeof(AdventureAction),action))return false;
            requests[slot]=Time.unscaledTime+.25f;var actor=session.Actor(slot);var features=session.Campaign?.Features;if(actor==null||!actor.Health.IsAlive||features==null)return false;
            return action==AdventureAction.ChooseBoon?features.ChooseBoon(actor.Health,slot,id,choice):action==AdventureAction.UseFeature?features.Use(actor.Health,id):features.Ping(slot,choice,actor.Health);
        }
    }
}
