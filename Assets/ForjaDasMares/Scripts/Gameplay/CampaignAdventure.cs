using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace ForjaDasMares
{
    public enum AdventurePropKind { Supply, Seal, Fountain }
    [Serializable] public sealed class AdventureSnapshot { public int[] health;public uint fountains;public bool rewarded; }
    // Bounded, deterministic decorations. All rewards and healing run on the authority only.
    public sealed class CampaignAdventure : MonoBehaviour
    {
        public const int PropLayer=14;
        readonly List<CampaignAdventureProp> props=new List<CampaignAdventureProp>(48);
        readonly AdventureSnapshot snapshot=new AdventureSnapshot();
        CampaignRun run;DungeonSession dungeon;CampaignAdventureDefinition data;
        float timer;bool rewarded;int sealTotal;
        public IReadOnlyList<CampaignAdventureProp> Props=>props;
        public int SealCount {get{int n=0;foreach(var p in props)if(p.Kind==AdventurePropKind.Seal&&!p.Health.IsAlive)n++;return n;}}
        public int SealGoal=>sealTotal;
        public bool Rewarded=>rewarded;
        public string Objective=>sealTotal==0?"":"Selos antigos: "+SealCount+"/"+sealTotal+(rewarded?" · recompensa recebida":" · equipamento raro ao completar");
        public void Build(CampaignRun owner,DungeonSession session)
        {
            run=owner;dungeon=session;data=run.Level.adventure;props.Clear();rewarded=false;sealTotal=0;timer=0;
            if(data==null){snapshot.health=Array.Empty<int>();return;}
            Physics.IgnoreLayerCollision(PropLayer,11,true);Physics.IgnoreLayerCollision(PropLayer,13,true);
            dungeon.Heroes.GetComponent<HeroCombat>().AddTargets(1<<PropLayer);
            int seals=Mathf.Clamp(data.sealGoal,1,3),wells=Mathf.Clamp(data.fountains,1,3);
            for(int i=0;i<dungeon.Generator.Rooms.Length;i++)
            {
                var room=dungeon.Generator.Rooms[i];
                if(i>0&&room.node.category!=RoomCategory.Boss)
                    for(int j=0;j<Mathf.Clamp(data.suppliesPerSector,1,3)&&props.Count<40;j++)Add(room,AdventurePropKind.Supply,j);
                for(int n=0;n<seals;n++)if(i==2+(dungeon.Generator.Rooms.Length-4)*n/seals)
                {if(Add(room,AdventurePropKind.Seal,3))sealTotal++;}
                for(int n=0;n<wells;n++)if(i==1+(dungeon.Generator.Rooms.Length-3)*(n+1)/(wells+1))Add(room,AdventurePropKind.Fountain,4);
            }
            if(sealTotal!=seals)throw new InvalidOperationException("Campaign seals could not be placed safely.");
            snapshot.health=new int[props.Count];
        }
        bool Add(DungeonRoom room,AdventurePropKind kind,int socket)
        {
            if(props.Count>=48)return false;
            // Search reserved side pockets; never place a prop in the entrance/corridor or inside geometry.
            for(int attempt=0;attempt<12;attempt++)
            {
                float angle=(socket*101+attempt*137+run.Level.seed%31)*Mathf.Deg2Rad;
                Vector3 desired=room.Center+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(2.7f+attempt%2*.5f);
                if(!NavMesh.SamplePosition(desired,out var hit,.45f,NavMesh.AllAreas)||!room.Contains(hit.position,3.8f))continue;
                Vector3 point=hit.position;
                if(Physics.CheckBox(point+Vector3.up*.6f,new Vector3(.6f,.55f,.6f),Quaternion.identity,1<<9,QueryTriggerInteraction.Ignore))continue;
                bool overlap=false;foreach(var p in props)if((p.transform.position-point).sqrMagnitude<2.25f){overlap=true;break;}
                if(overlap)continue;
                var go=new GameObject("Adventure_"+props.Count+"_"+kind);go.transform.SetParent(room.root.transform);go.transform.position=point;
                var prop=go.AddComponent<CampaignAdventureProp>();prop.Configure(this,room,kind,props.Count,data,dungeon.Biome);props.Add(prop);return true;
            }
            return false;
        }
        public bool CanDamage(Health source)
        {
            if(run==null||!run.Authority||!run.Running||source==null||!source.IsAlive||!source.gameObject.activeInHierarchy||source.Faction!=CombatFaction.Player)return false;
            for(int i=0;i<4;i++)if(dungeon.Party?.Get(i)==source)return true;return false;
        }
        void Update()
        {
            if(run==null||!run.Running||!run.Authority)return;
            timer-=Time.deltaTime;if(timer>0)return;timer=.15f;
            foreach(var p in props)if(p.Kind==AdventurePropKind.Supply&&!p.Health.IsAlive&&!p.Claimed)
                p.Claimed=run.SideRewardRequested!=null&&run.SideRewardRequested(p.Id,false);
            if(!rewarded&&sealTotal>0&&SealCount==sealTotal)
                rewarded=run.SideRewardRequested!=null&&run.SideRewardRequested(1000,true);
        }
        public string NearbyLabel(Health player)
        {
            if(player==null||!player.IsAlive)return "";
            foreach(var p in props)if(p.Kind==AdventurePropKind.Fountain&&!p.Claimed&&(p.transform.position-player.transform.position).sqrMagnitude<=4.41f)return "INTERAGIR · fonte de cura do grupo (uso único)";
            return "";
        }
        public bool TryUse(Health player)
        {
            if(!CanDamage(player)||dungeon.Encounter.State==EncounterState.Spawning||dungeon.Encounter.State==EncounterState.Fighting)return false;
            foreach(var p in props)
            {
                if(p.Kind!=AdventurePropKind.Fountain||p.Claimed||p.Room!=dungeon.CurrentRoom||(p.transform.position-player.transform.position).sqrMagnitude>4.41f)continue;
                if(Physics.Linecast(player.transform.position+Vector3.up,p.transform.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore))continue;
                bool healed=false;
                for(int i=0;i<4;i++)
                {
                    var member=dungeon.Party?.Get(i);if(member==null||!member.gameObject.activeInHierarchy||!member.IsAlive||!p.Room.Contains(member.transform.position))continue;
                    if(Physics.Linecast(p.transform.position+Vector3.up,member.transform.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore))continue;
                    healed|=member.Heal(Mathf.CeilToInt(member.Maximum*Mathf.Clamp(data.fountainHealing,.1f,.6f)));
                }
                if(healed){p.Claimed=true;p.Refresh();return true;}
            }
            return false;
        }
        public AdventureSnapshot Capture()
        {
            snapshot.fountains=0;snapshot.rewarded=rewarded;int well=0;
            for(int i=0;i<props.Count;i++){snapshot.health[i]=props[i].Health.Current;if(props[i].Kind==AdventurePropKind.Fountain){if(props[i].Claimed)snapshot.fountains|=1u<<well;well++;}}
            return snapshot;
        }
        public void Apply(AdventureSnapshot value)
        {
            if(value?.health==null||value.health.Length!=props.Count||run.Authority)return;
            rewarded=value.rewarded;int well=0;
            for(int i=0;i<props.Count;i++)
            {var p=props[i];p.Health.SetReplicaState(value.health[i],p.Health.Maximum);if(p.Kind==AdventurePropKind.Fountain)p.Claimed=(value.fountains&(1u<<well++))!=0;p.Refresh();}
        }
    }
    public sealed class CampaignAdventureProp : MonoBehaviour,IHitDefense
    {
        CampaignAdventure owner;Transform model;Renderer core;MaterialPropertyBlock block;Color color;float flash;
        public Health Health {get;private set;}
        public AdventurePropKind Kind {get;private set;}
        public DungeonRoom Room {get;private set;}
        public int Id {get;private set;}
        public bool Claimed {get;set;}
        public void Configure(CampaignAdventure adventure,DungeonRoom room,AdventurePropKind kind,int id,CampaignAdventureDefinition data,BiomeDefinition biome)
        {
            owner=adventure;Room=room;Kind=kind;Id=id;block=new MaterialPropertyBlock();
            Health=gameObject.AddComponent<Health>();Health.Configure(kind==AdventurePropKind.Seal?data.sealHealth:data.supplyHealth,CombatFaction.Training);Health.ConfigureDefense(this);
            var visual=GameObject.CreatePrimitive(kind==AdventurePropKind.Supply?PrimitiveType.Cube:PrimitiveType.Cylinder);visual.name="OriginalGeometry_"+kind;visual.transform.SetParent(transform,false);
            visual.transform.localPosition=Vector3.up*.65f;visual.transform.localScale=kind==AdventurePropKind.Supply?new Vector3(1,.95f,1):new Vector3(.8f,.65f,.8f);
            visual.layer=CampaignAdventure.PropLayer;model=visual.transform;core=visual.GetComponent<Renderer>();core.sharedMaterial=biome.accent;
            if(kind==AdventurePropKind.Fountain){visual.GetComponent<Collider>().enabled=false;Destroy(visual.GetComponent<Collider>());}
            color=kind==AdventurePropKind.Supply?new Color(.47f,.28f,.12f):kind==AdventurePropKind.Seal?data.sealColor:data.healingColor;
            for(int i=0;i<2;i++)
            {
                var band=GameObject.CreatePrimitive(PrimitiveType.Cube);band.name="Trim";band.transform.SetParent(model,false);band.transform.localPosition=new Vector3(0,i==0?-.33f:.33f,0);band.transform.localScale=new Vector3(1.06f,.12f,1.06f);
                band.GetComponent<Renderer>().sharedMaterial=biome.wall;band.GetComponent<Collider>().enabled=false;Destroy(band.GetComponent<Collider>());
            }
            Health.Damaged+=Hit;Health.Changed+=Refresh;Refresh();
        }
        public float DamageMultiplier(Vector3 direction,Health source,AttackDefinition attack)=>Kind!=AdventurePropKind.Fountain&&owner.CanDamage(source)?1:0;
        void Hit(){flash=.12f;Refresh();}
        void Update(){if(flash<=0)return;flash=Mathf.Max(0,flash-Time.deltaTime);Refresh();}
        public void Refresh()
        {
            if(model==null)return;bool broken=!Health.IsAlive||Claimed&&Kind==AdventurePropKind.Fountain;
            model.localPosition=Vector3.up*(broken?.12f:.65f);
            var size=Kind==AdventurePropKind.Supply?new Vector3(1,.95f,1):new Vector3(.8f,.65f,.8f);if(broken)size.y=.12f;model.localScale=size;
            var collider=core.GetComponent<Collider>();if(collider!=null)collider.enabled=Health.IsAlive;
            block.SetColor("_BaseColor",flash>0?Color.white:broken?color*.3f:color);core.SetPropertyBlock(block);
        }
        void OnDestroy(){if(Health!=null){Health.Damaged-=Hit;Health.Changed-=Refresh;}}
    }
}
