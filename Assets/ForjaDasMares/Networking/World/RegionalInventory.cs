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
        AbilityDefinition[] starterSkills;
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
            if(!tests&&CampaignHome.LocalAccountFolder!=null){directory=CampaignHome.LocalAccountFolder;identity=CampaignHome.LocalAccountId;}
            for(int i=0;i<args.Length-1;i++){if(args[i]=="-netSavePath")directory=Path.GetFullPath(args[++i]);else if(args[i]=="-netIdentity")identity=args[++i];}
            if(!Guid.TryParseExact(identity,"N",out _)){identity=Guid.NewGuid().ToString("N");if(!tests){PlayerPrefs.SetString("CoopParticipantIdentity",identity);PlayerPrefs.Save();}}
            SavePath=CoopRewardLedger.ParticipantPath(directory,identity);Service=new ProfileService(catalog,new ProfileStore(SavePath),false);Service.Changed+=Changed;Notice=Service.Message??"";
            equipment=world.Hero.GetComponent<HeroEquipment>();if(equipment==null)equipment=world.Hero.gameObject.AddComponent<HeroEquipment>();
            equipment.Configure(world.Hero,world.Hero.GetComponent<PlayerMotor>(),world.Hero.GetComponent<HeroCombat>(),Visual("RegionalEquippedMain"),Visual("RegionalEquippedSecondary"));
            world.Hero.GetComponent<HeroCombat>().ConfigureRegionalRules(()=>equipment.MainWeapon!=null,()=>world.Journey!=null&&world.Journey.ClueCount>0);
            var combat = world.Hero.GetComponent<HeroCombat>();
            starterSkills = BuildStarterSkills(world.Hero.Current.heroId);
            int carriedSkills = 0;
            if (world.Journey == null && world.Definition.regionId == "catacumbas-previa")
            {
                string path = RegionalJourney.JournalPath();
                carriedSkills = RegionalJourney.SkillCount(RegionalJourney.ReadJournal(path,"sino-ato1-v1")
                    ?? RegionalJourney.ReadJournal(path+".bak","sino-ato1-v1"));
            }
            combat.ConfigureRegionalSkills(starterSkills, () => world.Journey?.UnlockedSkills ?? carriedSkills);
            equipment.RequireEquippedVisual=true;
            equipment.enabled=true;equipment.Initialize(Service,catalog);Changed();
        }
        static AbilityDefinition[] BuildStarterSkills(string hero)
        {
            bool ranged = hero == "arcanist" || hero == "ranger";
            string[] names;
            Color tint;
            switch (hero)
            {
                case "warrior": names = new[] {"Golpe Fendido","Ruptura de Aço","Brado de Guerra"}; tint = new Color(1,.53f,.22f); break;
                case "ranger": names = new[] {"Flecha Perfurante","Chuva de Espinhos","Laço de Raízes"}; tint = new Color(.45f,.95f,.45f); break;
                case "rune-knight": names = new[] {"Estocada Rúnica","Onda do Bastião","Selo de Imobilização"}; tint = new Color(.95f,.78f,.35f); break;
                default: names = new[] {"Lança Prismática","Pulso Arcano","Prisão de Cristal"}; tint = new Color(.44f,.67f,1); break;
            }

            var result = new AbilityDefinition[3];
            for (int i = 0; i < result.Length; i++)
            {
                var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
                var hit = ScriptableObject.CreateInstance<AttackDefinition>();
                result[i] = ability;
                ability.displayName = names[i];
                ability.hit = hit;
                ability.cooldown = i == 0 ? 4f : i == 1 ? 7f : 10f;
                ability.radius = i == 1 ? 3.8f : 4.2f;
                ability.effect = i == 0 ? AbilityEffect.WeaponStrike : AbilityEffect.AreaImpact;
                hit.shape = i == 0 && ranged ? AttackShape.Projectile : AttackShape.Arc;
                hit.footprint = i == 0 ? MeleeFootprint.Cone : MeleeFootprint.Circle;
                hit.maximumTargets = i == 0 ? 1 : 0;
                hit.damage = i == 0 ? 24 : i == 1 ? 15 : 5;
                hit.range = ranged ? 10 : 3;
                hit.arc = i == 0 ? 48 : 360;
                hit.windup = .16f + .06f * i;
                hit.recovery = .26f;
                hit.knockback = i == 1 ? 1f : 0;
                hit.color = i == 2 ? Color.Lerp(tint,new Color(.55f,.3f,1),.6f) : tint;
                hit.control = i == 2 ? ControlEffect.Root : ControlEffect.None;
                hit.controlDuration = i == 2 ? 2.2f : 0;
                if (hero == "arcanist")
                    hit.prismaticEffect = i == 0 ? PrismaticEffect.Shatter : i == 2 ? PrismaticEffect.Imprison : PrismaticEffect.Pulse;
            }
            return result;
        }

        MeshRenderer Visual(string name)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(world.Hero.transform,false);
            go.transform.localPosition=new Vector3(name.EndsWith("Main")?.55f:-.55f,1,.3f);
            return go.GetComponent<MeshRenderer>();
        }
        void Changed(){Revision++;}
        public void SetNotice(string value){Notice=value;Revision++;}
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
        void OnDestroy()
        {
            if(Service!=null)Service.Changed-=Changed;
            if(starterSkills == null) return;
            foreach(var skill in starterSkills)
            {
                Destroy(skill.hit);
                Destroy(skill);
            }
        }
    }
}


