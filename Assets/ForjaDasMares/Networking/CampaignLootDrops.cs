using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
namespace ForjaDasMares.Networking
{
    // Presentation only: the authority has already persisted the reward before this message.
    public sealed class CampaignLootDrops:MonoBehaviour
    {
        sealed class Token {public Transform view;public float time;public Vector3 from;}
        readonly Token[] pool=new Token[32];CoopDirector session;Material gold,loot;int sequence,lastReceived;bool initialized;CampaignCombatReadout readout;
        public void ConfigureReadout(CampaignCombatReadout value)=>readout=value;
        public int ActiveCount {get{int count=0;foreach(var token in pool)if(token!=null&&token.time>0)count++;return count;}}
        public void Bind(CoopDirector owner)
        {
            session=owner;sequence=lastReceived=0;Clear();if(session.Campaign!=null)Initialize();session.Manager.CustomMessagingManager.RegisterNamedMessageHandler("fdm.drop.v1",Receive);
        }
        void Initialize()
        {
            if(initialized)return;initialized=true;var shader=Shader.Find("Universal Render Pipeline/Unlit");gold=new Material(shader);gold.SetColor("_BaseColor",RusticHudTheme.Gold);loot=new Material(shader);loot.SetColor("_BaseColor",new Color(.3f,.8f,1));
            for(int i=0;i<pool.Length;i++){var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name="PooledGoldDrop_"+i;go.transform.SetParent(transform,false);Destroy(go.GetComponent<Collider>());go.transform.localScale=new Vector3(.3f,.05f,.3f);go.GetComponent<Renderer>().sharedMaterial=gold;go.SetActive(false);pool[i]=new Token{view=go.transform};}
        }
        public void Send(int slot,Vector3 point,int coins,int xp,int rarity)
        {
            if(session==null||!session.Manager.IsServer)return;int id=++sequence;
            if(slot==session.LocalSlot)Play(point,coins,xp,rarity);
            foreach(ulong client in session.Manager.ConnectedClientsIds)if(client!=session.Manager.LocalClientId&&session.TryGetSlot(client,out int owner)&&owner==slot)
            {
                using var writer=new FastBufferWriter(28,Allocator.Temp);writer.WriteValueSafe(id);writer.WriteValueSafe(point);writer.WriteValueSafe(coins);writer.WriteValueSafe(xp);writer.WriteValueSafe(rarity);
                session.Manager.CustomMessagingManager.SendNamedMessage("fdm.drop.v1",client,writer,NetworkDelivery.ReliableSequenced);
            }
        }
        void Receive(ulong sender,FastBufferReader reader)
        {
            if(session.Manager.IsServer||sender!=NetworkManager.ServerClientId||reader.Length-reader.Position!=28)return;
            reader.ReadValueSafe(out int id);reader.ReadValueSafe(out Vector3 point);reader.ReadValueSafe(out int coins);reader.ReadValueSafe(out int xp);reader.ReadValueSafe(out int rarity);
            if(id<=lastReceived||coins<0||coins>10000||xp<0||xp>10000||rarity< -1||rarity>4||!float.IsFinite(point.x)||!float.IsFinite(point.y)||!float.IsFinite(point.z))return;lastReceived=id;Play(point,coins,xp,rarity);
        }
        void Play(Vector3 point,int coins,int xp,int rarity)
        {
            if(session.Campaign==null)return;Initialize();int count=rarity>=0?3:2;
            foreach(var token in pool)if(token.time<=0&&count>0){token.from=point+new Vector3((count-2)*.3f,.25f,0);token.time=1.1f;token.view.position=token.from;token.view.GetComponent<Renderer>().sharedMaterial=count==1?loot:gold;token.view.gameObject.SetActive(true);count--;}
            readout?.ShowDrop(point,coins,xp,rarity);
        }
        void Update()
        {
            if(session==null)return;if(session.Phase!=CoopPhase.Playing){Clear();return;}var actor=session.Actor(session.LocalSlot);if(actor==null)return;
            foreach(var token in pool)if(token!=null&&token.time>0){token.time-=Time.deltaTime;if(token.time<=0){token.view.gameObject.SetActive(false);continue;}float t=1-token.time/1.1f;token.view.position=Vector3.Lerp(token.from,actor.transform.position+Vector3.up,Mathf.SmoothStep(0,1,t))+Vector3.up*Mathf.Sin(t*Mathf.PI)*.6f;token.view.Rotate(Vector3.up,180*Time.deltaTime,Space.World);}
        }
        void Clear(){foreach(var token in pool)if(token!=null){token.time=0;token.view.gameObject.SetActive(false);}}
        void OnDestroy(){foreach(var token in pool)if(token!=null&&token.view!=null)Destroy(token.view.gameObject);if(gold!=null)Destroy(gold);if(loot!=null)Destroy(loot);}
    }
}
