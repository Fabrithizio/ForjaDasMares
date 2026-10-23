using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    // Offline organisation of server-confirmed snapshots. Never imports client items into a host.
    public sealed class CampaignProfileHub:MonoBehaviour
    {
        [Serializable] sealed class Plan {public string scope;public bool remote;public int deliveries;public List<CoopInventoryIntent> pending=new List<CoopInventoryIntent>();}
        CoopDirector director;CoopProgression authority;CoopProfileChannel channel;ProfileService cached,prepared;Plan plan;int revision;float retry;int attempts;
        string Root=>authority.HomeCacheRoot;string PlanPath=>Path.Combine(Root,"context.json");string ProfilePath=>Path.Combine(Root,"profile-v2.json");
        public void Configure(CoopDirector owner,CoopProgression progression,CoopProfileChannel wire){director=owner;authority=progression;channel=wire;}
        void Load()
        {
            if(plan!=null)return;plan=new Plan();
            try{if(File.Exists(PlanPath))plan=JsonUtility.FromJson<Plan>(File.ReadAllText(PlanPath))??new Plan();plan.pending??=new List<CoopInventoryIntent>();plan.pending.RemoveAll(x=>x==null||x.hero==null||authority.Catalog.Hero(x.hero)==null||!Enum.IsDefined(typeof(EquipmentSlot),x.slot)||x.action!=CoopInventoryAction.Equip&&x.action!=CoopInventoryAction.Unequip||x.item!=null&&x.item.Length>64);if(plan.pending.Count>20)plan.pending.RemoveRange(20,plan.pending.Count-20);
                if(!Guid.TryParseExact(plan.scope,"N",out _)){plan.scope=null;plan.remote=false;plan.pending.Clear();}
                else if(File.Exists(ProfilePath)){var store=new ProfileStore(ProfilePath);if(store.Load(authority.Catalog,out _)!=null){cached=new ProfileService(authority.Catalog,store);cached.Changed+=Changed;}}
            }catch(Exception){plan=new Plan();cached=null;}
            if(cached==null)plan.remote=false;
        }
        public bool UsingRemote {get{Load();return plan.remote&&cached!=null;}}
        public bool HasRemote {get{Load();return cached!=null;}}
        public int DeliveryCount=>UsingRemote?Mathf.Max(plan.deliveries,cached.Profile.deliveries.Count):authority.HomeProfile.Profile.deliveries.Count;
        public int Revision=>revision+authority.HomeRevision;
        public ProfileService Service
        {
            get
            {
                Load();var service=UsingRemote?cached:authority.HomeProfile;
                if(channel.HomeMode&&(prepared!=service||director.Actor(0).GetComponent<HeroEquipment>()==null||!director.Actor(0).GetComponent<HeroEquipment>().enabled))
                {authority.PrepareHome(service);prepared=service;}
                return service;
            }
        }
        public string Description=>UsingRemote?"PERFIL DA ÚLTIMA SALA · valida ao voltar para ela":HasRemote?"PERFIL DESTE APARELHO · trocar para o da última sala":"PERFIL DESTE APARELHO";
        public void Toggle(){Load();if(cached==null)return;plan.remote=!plan.remote;prepared=null;revision++;Persist();}
        void Changed()=>revision++;
        void Persist(){try{Directory.CreateDirectory(Root);string temporary=PlanPath+".tmp";File.WriteAllText(temporary,JsonUtility.ToJson(plan));if(File.Exists(PlanPath))File.Replace(temporary,PlanPath,PlanPath+".bak");else File.Move(temporary,PlanPath);}catch(Exception){channel.SetNotice("Não foi possível guardar a organização do perfil.");}}
        public void OwnSession(){Load();plan.remote=false;prepared=null;revision++;Persist();}
        public void Accept(CoopProfileFrame packet)
        {
            if(!Guid.TryParseExact(packet.scope,"N",out _)||packet.scope==director.ParticipantIdentity)return;Load();
            try
            {
                if(plan.scope!=packet.scope)plan.pending.Clear();
                int before=plan.pending.Count;
                foreach(var pending in plan.pending.ToArray())
                {
                    var set=packet.profile.Loadout(pending.hero);
                    bool applied=set!=null&&(pending.action==CoopInventoryAction.Unequip?string.IsNullOrEmpty(set.items[(int)pending.slot]):set.items[(int)pending.slot]==pending.item);
                    if(applied||pending.action==CoopInventoryAction.Equip&&packet.profile.Item(pending.item)==null)plan.pending.Remove(pending);
                }
                if(before!=plan.pending.Count)attempts=0;plan.deliveries=packet.deliveryCount;
                var store=new ProfileStore(ProfilePath);store.Save(packet.profile,authority.Catalog);if(cached!=null)cached.Changed-=Changed;
                cached=new ProfileService(authority.Catalog,store);cached.Changed+=Changed;plan.scope=packet.scope;plan.remote=true;prepared=null;revision++;Persist();
            }
            catch(Exception){channel.SetNotice("O perfil da sala está disponível, mas sua cópia local não pôde ser guardada.");}
        }
        public void Request(CoopInventoryAction action,string item,EquipmentSlot slot,bool confirmed,int talent=0)
        {
            var service=Service;string hero=director.Actor(0).Roster.Current.heroId;
            if(UsingRemote&&action!=CoopInventoryAction.Equip&&action!=CoopInventoryAction.Unequip){channel.SetNotice("Receba ou descarte itens na sala de origem. Aqui você pode organizar o equipamento.");return;}
            bool success=action==CoopInventoryAction.Equip?service.Equip(hero,item,slot):action==CoopInventoryAction.Unequip?service.Unequip(hero,slot):action==CoopInventoryAction.ClaimDelivery?service.ClaimDelivery(item):action==CoopInventoryAction.Discard?service.Discard(item,confirmed):action==CoopInventoryAction.SpendTalent?service.SpendTalent(hero,(TalentId)talent):service.ResetTalents(hero,confirmed);
            channel.SetNotice(service.Message);
            if(success&&UsingRemote)
            {
                plan.pending.RemoveAll(x=>x.hero==hero&&x.slot==slot);if(plan.pending.Count>=20)plan.pending.RemoveAt(0);
                plan.pending.Add(new CoopInventoryIntent{action=action,item=item,slot=slot,hero=hero,confirmed=confirmed});Persist();channel.SetNotice("Organização salva nesta cópia. Será validada ao voltar à sala de origem.");
            }
        }
        void Update()
        {
            if(plan==null||plan.pending.Count==0||director.Manager.IsServer||director.Phase!=CoopPhase.Lobby||channel.CurrentScope!=plan.scope)return;
            retry-=Time.unscaledDeltaTime;if(retry>0)return;retry=1;
            if(attempts++>=3){plan.pending.RemoveAt(0);attempts=0;Persist();channel.SetNotice("Uma alteração não pôde ser aplicada à sala. Confira o equipamento.");return;}
            channel.SendIntent(plan.pending[0]);
        }
        void OnDestroy(){if(cached!=null)cached.Changed-=Changed;}
    }
}
