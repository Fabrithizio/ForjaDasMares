using System;
using System.IO;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    [DefaultExecutionOrder(245)]public sealed class RegionalInventory:MonoBehaviour
    {
        [SerializeField] ProgressionCatalog catalog;
        [SerializeField] string profileDirectory="CampaignProfiles-v1";
        RegionalWorld world;HeroEquipment equipment;
        public RegionalWorld World=>world;
        public ProgressionCatalog Catalog=>catalog;
        public ProfileService Service{get;private set;}
        public string SavePath{get;private set;}
        public int Revision{get;private set;}
        public string Notice{get;private set;}="";
#if UNITY_EDITOR
        public static string TestDirectory;
#endif
        static string testDirectory;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetStatics(){testDirectory=null;}
        public void Configure(ProgressionCatalog data,string folder){catalog=data;profileDirectory=folder;}
        public bool CanManage=>string.IsNullOrEmpty(BlockReason);
        public string BlockReason
        {
            get
            {
                if(Service==null||world==null||world.Hero==null)return "Carregando o perfil do personagem...";
                if(Service.Profile.expedition!=null)return "Finalize a expedição pendente no modo de desafios.";
                if(!world.Hero.Health.IsAlive)return "Reapareça antes de alterar o equipamento.";
                var motor=world.Hero.GetComponent<PlayerMotor>();
                if(world.Hero.GetComponent<HeroCombat>().IsBusy||motor.IsCharging||motor.IsDodging)return "Termine o ataque, a habilidade ou a esquiva antes de alterar o equipamento.";
                if(world.Enemies.ThreatNear(world.Hero.transform.position,12))return "Afaste-se dos inimigos para alterar o equipamento com segurança.";
                return "";
            }
        }
        void Start()
        {
            world=GetComponent<RegionalWorld>();if(world==null||catalog==null)throw new InvalidOperationException("Regional inventory requires world and catalog");
            string directory=Path.Combine(Application.persistentDataPath,profileDirectory);string identity=PlayerPrefs.GetString("CoopParticipantIdentity","");
            var args=Environment.GetCommandLineArgs();bool tests=Array.IndexOf(args,"-runTests")>=0;
            if(tests){testDirectory??=Path.Combine(Application.temporaryCachePath,"RegionalInventoryTests",Guid.NewGuid().ToString("N"));directory=testDirectory;identity="11111111111111111111111111111111";}
#if UNITY_EDITOR
            if(!string.IsNullOrEmpty(TestDirectory)){directory=TestDirectory;identity="11111111111111111111111111111111";}
#endif
            for(int i=0;i<args.Length-1;i++){if(args[i]=="-netSavePath")directory=Path.GetFullPath(args[++i]);else if(args[i]=="-netIdentity")identity=args[++i];}
            if(!Guid.TryParseExact(identity,"N",out _)){identity=Guid.NewGuid().ToString("N");if(!tests){PlayerPrefs.SetString("CoopParticipantIdentity",identity);PlayerPrefs.Save();}}
            SavePath=CoopRewardLedger.ParticipantPath(directory,identity);Service=new ProfileService(catalog,new ProfileStore(SavePath));Service.Changed+=Changed;Notice=Service.Message??"";
            equipment=world.Hero.GetComponent<HeroEquipment>();if(equipment==null)equipment=world.Hero.gameObject.AddComponent<HeroEquipment>();
            equipment.Configure(world.Hero,world.Hero.GetComponent<PlayerMotor>(),world.Hero.GetComponent<HeroCombat>(),Visual("RegionalEquippedMain"),Visual("RegionalEquippedSecondary"));
            equipment.enabled=true;equipment.Initialize(Service,catalog);Changed();
        }
        MeshRenderer Visual(string name)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(world.Hero.transform,false);
            go.transform.localPosition=new Vector3(name.EndsWith("Main")?.55f:-.55f,1,.3f);
            return go.GetComponent<MeshRenderer>();
        }
        void Changed(){Revision++;}
        public void SetNotice(string value){Notice=value;Revision++;}
        public void SelectHero(int index)
        {if(!CanManage){SetNotice(BlockReason);return;}world.Hero.GetComponent<ArenaInput>().Desktop?.Cancel();world.Hero.Select(Mathf.Clamp(index,0,world.Hero.Count-1));equipment.Refresh();Revision++;}
        public void Request(CoopInventoryAction action,string item,EquipmentSlot slot,bool confirmed,int talent)
        {
            if(!CanManage){SetNotice(BlockReason);return;}
            string hero=world.Hero.Current.heroId;
            switch(action)
            {
                case CoopInventoryAction.Equip:Service.Equip(hero,item,slot);break;
                case CoopInventoryAction.Unequip:Service.Unequip(hero,slot);break;
                case CoopInventoryAction.ClaimDelivery:Service.ClaimDelivery(item);break;
                case CoopInventoryAction.ClaimAllDeliveries:Service.ClaimAllDeliveries();break;
                case CoopInventoryAction.Discard:Service.Discard(item,confirmed);break;
                case CoopInventoryAction.SpendTalent:Service.SpendTalent(hero,(TalentId)talent);break;
                case CoopInventoryAction.ResetTalents:Service.ResetTalents(hero,confirmed);break;
                default:SetNotice("Ação de inventário inválida.");return;
            }
            SetNotice(Service.Message);
        }
        void OnDestroy(){if(Service!=null)Service.Changed-=Changed;}
    }
}


