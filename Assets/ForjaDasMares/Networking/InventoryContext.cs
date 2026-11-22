using UnityEngine;
namespace ForjaDasMares.Networking
{
    // What the old inventory screens (CoopInventoryHud, CampaignCharacterSheet) read about the hero and the profile.
    // Only the regional game (this device) is left; the old online expeditions were removed.
    public enum CoopPhase { Offline, Connecting, Lobby, Loading, Playing, Defeat, Victory, Closed }
    public enum CoopInventoryAction { Equip, Unequip, ClaimDelivery, Discard, SpendTalent, ResetTalents, ClaimAllDeliveries }
    public sealed class InventoryHeroView
    {
        readonly HeroRoster regional;
        public InventoryHeroView(HeroRoster hero){regional=hero;}
        public HeroRoster Roster=>regional;
        public Health Health=>Roster.Health;
        public HeroPresentation Presentation=>Roster.GetComponent<HeroPresentation>();
        public Transform transform=>Roster.transform;
        public float FuryRemaining=>Roster.GetComponent<HeroCombat>().FuryRemaining;
        public T GetComponent<T>() where T:Component=>Roster.GetComponent<T>();
    }
    public sealed class InventorySessionView
    {
        readonly RegionalInventory regional;
        readonly InventoryHeroView hero;
        public InventoryProfileView Profiles{get;}
        public InventorySessionView(RegionalInventory owner){regional=owner;hero=new InventoryHeroView(owner.World.Hero);Profiles=new InventoryProfileView(owner);}
        public bool IsRegional=>true;
        public bool FullLayout=>true;
        public int LocalSlot=>0;
        public int SelectedHero=>regional.World.Hero.Selected;
        public int HeroCount=>regional.World.Hero.Count;
        public CoopPhase Phase=>CoopPhase.Playing;
        public InventoryHeroView Actor(int slot)=>slot==0?hero:null;
        public void SelectHero(int index){}
        public void SetInputBlocked(bool value)
        {
            if(regional==null||regional.World==null||regional.World.Hero==null)return;
            var input=regional.World.Hero.GetComponent<ArenaInput>();input.Desktop?.Cancel();input.BlockCommands(value);
            regional.World.Hero.GetComponent<PlayerMotor>().StopWalking();
        }
        public T GetComponent<T>() where T:Component=>regional.GetComponent<T>();
    }
    public sealed class InventoryHubView
    {
        readonly RegionalInventory regional;
        public InventoryHubView(RegionalInventory local){regional=local;}
        public bool UsingRemote=>false;
        public bool HasRemote=>false;
        public string Description=>"PERFIL DESTE APARELHO";
        public ProfileService Service=>regional.Service;
        public void Toggle(){}
    }
    public sealed class InventoryProfileView
    {
        readonly RegionalInventory regional;
        public InventoryHubView Hub{get;}
        public InventoryProfileView(RegionalInventory local){regional=local;Hub=new InventoryHubView(local);}
        public bool HomeMode=>true;
        public PlayerProfile LocalProfile=>regional.Service.Profile;
        public ProgressionCatalog Catalog=>regional.Catalog;
        public int DeliveryCount=>LocalProfile.deliveries.Count;
        public int Revision=>regional.Revision;
        public string Message=>regional.Notice;
        public bool CanManageLocal=>regional.CanManage;
        public string LocalManagementBlockReason=>regional.BlockReason;
        public void SetNotice(string value)=>regional.SetNotice(value);
        public void Request(CoopInventoryAction action,string item,EquipmentSlot slot,bool confirmed=false,int talent=0)=>regional.Request(action,item,slot,confirmed,talent);
    }
}
