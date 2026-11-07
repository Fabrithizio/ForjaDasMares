using UnityEngine;
using Unity.Netcode;
namespace ForjaDasMares.Networking
{
    // Presentation adapters: the online path still delegates to its existing validated channel.
    public sealed class InventoryHeroView
    {
        readonly CoopActor online;readonly HeroRoster regional;
        public InventoryHeroView(CoopActor actor){online=actor;}
        public InventoryHeroView(HeroRoster hero){regional=hero;}
        public bool Matches(CoopActor actor)=>online==actor;
        public HeroRoster Roster=>online!=null?online.Roster:regional;
        public Health Health=>Roster.Health;
        public HeroPresentation Presentation=>Roster.GetComponent<HeroPresentation>();
        public Transform transform=>Roster.transform;
        public float FuryRemaining=>online!=null?online.FuryRemaining:Roster.GetComponent<HeroCombat>().FuryRemaining;
        public T GetComponent<T>() where T:Component=>Roster.GetComponent<T>();
    }
    public sealed class InventorySessionView
    {
        readonly CoopDirector online;readonly RegionalInventory regional;readonly bool regionalMode;
        readonly InventoryHeroView[] actors=new InventoryHeroView[4];
        public InventoryProfileView Profiles{get;}
        public InventorySessionView(CoopDirector owner){online=owner;Profiles=new InventoryProfileView(owner.GetComponent<CoopProfileChannel>());}
        public InventorySessionView(RegionalInventory owner){regional=owner;regionalMode=true;actors[0]=new InventoryHeroView(owner.World.Hero);Profiles=new InventoryProfileView(owner);}
        public bool IsRegional=>regionalMode;
        public bool FullLayout=>IsRegional||online.Campaign!=null;
        public int LocalSlot=>IsRegional?0:online.LocalSlot;
        public int SelectedHero=>IsRegional?regional.World.Hero.Selected:online.SelectedHero;
        public int HeroCount=>IsRegional?regional.World.Hero.Count:online.HeroCount;
        public CoopPhase Phase=>IsRegional?CoopPhase.Playing:online.Phase;
        public NetworkManager Manager=>online!=null?online.Manager:null;
        public DungeonSession Dungeon=>online!=null?online.Dungeon:null;
        public EncounterState EncounterStatus=>online!=null?online.EncounterStatus:EncounterState.Ready;
        public InventoryHeroView Actor(int slot)
        {
            if(IsRegional)return slot==0?actors[0]:null;
            var actor=online.Actor(slot);if(actor==null)return null;
            if(actors[slot]==null||!actors[slot].Matches(actor))actors[slot]=new InventoryHeroView(actor);return actors[slot];
        }
        public void SelectHero(int hero){if(IsRegional)regional.SelectHero(hero);else online.SelectHero(hero);}
        public void SetInputBlocked(bool value)
        {
            if(!IsRegional){if(online!=null)online.SetInputBlocked(value);return;}
            if(regional==null||regional.World==null||regional.World.Hero==null)return;
            var input=regional.World.Hero.GetComponent<ArenaInput>();input.Desktop?.Cancel();input.BlockCommands(value);
            regional.World.Hero.GetComponent<PlayerMotor>().StopWalking();
        }
        public T GetComponent<T>() where T:Component=>IsRegional?regional.GetComponent<T>():online.GetComponent<T>();
    }
    public sealed class InventoryHubView
    {
        readonly CoopProfileChannel online;readonly RegionalInventory regional;
        public InventoryHubView(CoopProfileChannel wire){online=wire;}
        public InventoryHubView(RegionalInventory local){regional=local;}
        public bool UsingRemote=>regional==null&&online.Hub.UsingRemote;
        public bool HasRemote=>regional==null&&online.Hub.HasRemote;
        public string Description=>regional!=null?"PERFIL DESTE APARELHO · COMPARTILHADO COM OS DESAFIOS":online.Hub.Description;
        public ProfileService Service=>regional!=null?regional.Service:online.Hub.Service;
        public void Toggle(){if(regional==null)online.Hub.Toggle();}
    }
    public sealed class InventoryProfileView
    {
        readonly CoopProfileChannel online;readonly RegionalInventory regional;
        public InventoryHubView Hub{get;}
        public InventoryProfileView(CoopProfileChannel wire){online=wire;Hub=new InventoryHubView(wire);}
        public InventoryProfileView(RegionalInventory local){regional=local;Hub=new InventoryHubView(local);}
        public bool HomeMode=>regional!=null||online.HomeMode;
        public PlayerProfile LocalProfile=>regional!=null?regional.Service.Profile:online.LocalProfile;
        public ProgressionCatalog Catalog=>regional!=null?regional.Catalog:online.Catalog;
        public int DeliveryCount=>regional!=null?LocalProfile.deliveries.Count:online.DeliveryCount;
        public int Revision=>regional!=null?regional.Revision:online.Revision;
        public string Message=>regional!=null?regional.Notice:online.Message;
        public bool CanManageLocal=>regional!=null?regional.CanManage:online.CanManageLocal;
        public string LocalManagementBlockReason=>regional!=null?regional.BlockReason:online.LocalManagementBlockReason;
        public void SetNotice(string value){if(regional!=null)regional.SetNotice(value);else online.SetNotice(value);}
        public void Request(CoopInventoryAction action,string item,EquipmentSlot slot,bool confirmed=false,int talent=0)
        {if(regional!=null)regional.Request(action,item,slot,confirmed,talent);else online.Request(action,item,slot,confirmed,talent);}
    }
}

