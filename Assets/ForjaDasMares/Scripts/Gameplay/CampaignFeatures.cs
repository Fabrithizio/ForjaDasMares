using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace ForjaDasMares
{
    public enum ExpeditionFeatureKind { Altar, Challenge, Cache, Barrel, BreakableWall, FragileBridge }
    [Serializable]public sealed class FeatureState {public int id,stage,health;public uint claimed;public Vector3 position;public float progress,remaining;}
    [Serializable]public sealed class PartyPing {public int kind;public Vector3 position;public float remaining;}
    [Serializable]public sealed class FeatureSnapshot {public FeatureState[] nodes;public PartyPing[] pings;}
    public sealed class ExpeditionFeature
    {
        public float hazardClock;public Renderer danger;public ExpeditionFeatureKind kind;public DungeonRoom room;public Health health;public Transform root;public Renderer visual;public FeatureState state;
    }
    // Side activities never replace the main encounter controller or claim main-room completion.
    public sealed class CampaignFeatures:MonoBehaviour
    {
        CampaignRun run;DungeonSession dungeon;ExpeditionFeatureDefinition data;ExpeditionFeature active;
        readonly List<ExpeditionFeature> nodes=new List<ExpeditionFeature>(10);readonly List<EnemyActor> spawned=new List<EnemyActor>(16);
        readonly Collider[] overlaps=new Collider[64];readonly Health[] unique=new Health[64];MaterialPropertyBlock tint;
        NavMeshPath escortPath;void Awake(){tint=new MaterialPropertyBlock();escortPath=new NavMeshPath();}Vector3[] escortCorners;int escortCorner,count,failures;float escortLength,escortTravelled;float nextSpawn,eventClock,scan;
        FeatureSnapshot snapshot;public IReadOnlyList<ExpeditionFeature> Nodes=>nodes;public PartyPing[] Pings=>snapshot?.pings;
        public bool EventActive=>active!=null&&active.state.stage==1;
        public ExpeditionFeature Active=>active;
        public void Build(CampaignRun owner,DungeonSession map)
        {
            if(dungeon!=null)dungeon.TrapsDisabled-=DisarmBridges;run=owner;dungeon=map;dungeon.TrapsDisabled+=DisarmBridges;data=run.Level.features;nodes.Clear();spawned.Clear();active=null;dungeon.SideActivityBusy=false;dungeon.Encounter.Enemies.AuxiliaryObjective=null;
            if(dungeon.Party!=null)for(int i=0;i<4;i++)dungeon.Party.Get(i)?.GetComponent<CombatBuildEffects>()?.ResetRun();
            var rooms=dungeon.Generator.Rooms;if(data!=null)
            {
                Add(rooms[0],ExpeditionFeatureKind.Altar,0);Add(rooms[rooms.Length/2],ExpeditionFeatureKind.Altar,1);
                Add(rooms[Mathf.Min(3,rooms.Length-2)],ExpeditionFeatureKind.Challenge,2);
                Add(rooms[2],ExpeditionFeatureKind.Cache,3);Add(rooms[rooms.Length-3],ExpeditionFeatureKind.Cache,4);
                for(int i=1;i<rooms.Length-1&&nodes.Count<12;i+=2)Add(rooms[i],ExpeditionFeatureKind.Barrel,5+i);
                var cache=nodes.Find(x=>x.kind==ExpeditionFeatureKind.Cache);if(cache!=null){Vector3 direction=(cache.root.position-cache.room.Center).normalized;Add(cache.room,ExpeditionFeatureKind.BreakableWall,19,cache.root.position-direction*1.7f);}
                var trap=System.Array.Find(rooms,x=>x.node.category==RoomCategory.Trap);Add(trap??rooms[rooms.Length/2],ExpeditionFeatureKind.FragileBridge,20);
            }
            snapshot=new FeatureSnapshot{nodes=new FeatureState[nodes.Count],pings=new PartyPing[4]};for(int i=0;i<4;i++)snapshot.pings[i]=new PartyPing();for(int i=0;i<nodes.Count;i++)snapshot.nodes[i]=nodes[i].state;
        }
        void Add(DungeonRoom room,ExpeditionFeatureKind kind,int socket,Vector3? preferred=null)
        {
            Vector3 point=room.Center;bool valid=false;
            for(int i=0;i<30;i++)
            {
                float angle=(socket*97+i*137)*Mathf.Deg2Rad;Vector3 desired=preferred.HasValue?preferred.Value+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(i*.08f):room.Center+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*room.walkableHalf*(.35f+i%3*.15f);
                if(!NavMesh.SamplePosition(desired,out var hit,.5f,NavMesh.AllAreas)||!room.Contains(hit.position,room.walkableHalf-1)||Physics.CheckSphere(hit.position+Vector3.up*.6f,.8f,1<<9,QueryTriggerInteraction.Ignore))continue;
                bool overlaps=false;foreach(var placed in nodes)if((placed.root.position-hit.position).sqrMagnitude<(kind==ExpeditionFeatureKind.BreakableWall?2.25f:9))overlaps=true;
                if(run.Adventure!=null)foreach(var prop in run.Adventure.Props)if((prop.transform.position-hit.position).sqrMagnitude<6.25f)overlaps=true;
                foreach(var item in room.interactions)if((item.transform.position-hit.position).sqrMagnitude<6.25f)overlaps=true;
                if(overlaps)continue;point=hit.position;valid=true;break;
            }
            if(!valid){Debug.LogWarning("Optional feature could not be placed safely: "+kind);return;}
            var go=new GameObject("Expedition_"+kind+"_"+nodes.Count);go.transform.SetParent(room.root.transform);go.transform.position=point;
            var visual=GameObject.CreatePrimitive(kind==ExpeditionFeatureKind.Barrel?PrimitiveType.Cylinder:kind==ExpeditionFeatureKind.Cache||kind==ExpeditionFeatureKind.BreakableWall||kind==ExpeditionFeatureKind.FragileBridge?PrimitiveType.Cube:PrimitiveType.Sphere);visual.transform.SetParent(go.transform,false);visual.transform.localPosition=Vector3.up*.65f;visual.transform.localScale=kind==ExpeditionFeatureKind.Barrel?new Vector3(.8f,.6f,.8f):Vector3.one*.8f;
            visual.layer=CampaignAdventure.PropLayer;var renderer=visual.GetComponent<Renderer>();renderer.sharedMaterial=run.Level.challengeMaterial;
            var health=go.AddComponent<Health>();health.Configure(kind==ExpeditionFeatureKind.Challenge?data.objectiveHealth:kind==ExpeditionFeatureKind.BreakableWall?70:24,CombatFaction.Training);
            var model=kind==ExpeditionFeatureKind.Barrel?data.barrelModel:kind==ExpeditionFeatureKind.Altar?data.altarModel:kind==ExpeditionFeatureKind.Cache?data.cacheModel:kind==ExpeditionFeatureKind.BreakableWall?data.wallModel:kind==ExpeditionFeatureKind.FragileBridge?data.bridgeModel:data.challengeModel;if(model!=null){visual.GetComponent<MeshFilter>().sharedMesh=null;var authored=Instantiate(model,visual.transform);authored.transform.localPosition=Vector3.down*.65f;authored.transform.localRotation=Quaternion.identity;authored.transform.localScale=Vector3.one;}
            var node=new ExpeditionFeature{kind=kind,room=room,health=health,root=go.transform,visual=renderer,state=new FeatureState{id=nodes.Count,position=point,health=health.Current}};
            var defense=go.AddComponent<ExpeditionFeatureDefense>();defense.Configure(this,node);health.ConfigureDefense(defense);
            if(kind==ExpeditionFeatureKind.BreakableWall){visual.transform.localPosition=Vector3.up*1.15f;visual.transform.localScale=new Vector3(3,2.3f,.5f);visual.layer=9;var damage=new GameObject("DamageableWall",typeof(BoxCollider));damage.layer=CampaignAdventure.PropLayer;damage.transform.SetParent(visual.transform,false);var obstacle=visual.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.size=Vector3.one;obstacle.carving=true;if(model!=null){var authored=visual.transform.GetChild(0);authored.localPosition=new Vector3(0,-.5f,0);authored.localScale=new Vector3(1/3f,1/2.3f,2);}}
            if(kind==ExpeditionFeatureKind.FragileBridge){visual.transform.localScale=Vector3.one;if(model!=null)visual.transform.GetChild(0).localPosition=Vector3.down*.65f;var hazard=GameObject.CreatePrimitive(PrimitiveType.Cylinder);hazard.name="HazardBelowBridge";hazard.transform.SetParent(go.transform,false);hazard.transform.localPosition=Vector3.up*.025f;hazard.transform.localScale=new Vector3(4.8f,.015f,4.8f);var collider=hazard.GetComponent<Collider>();collider.enabled=false;Destroy(collider);node.danger=hazard.GetComponent<Renderer>();node.danger.sharedMaterial=run.Level.challengeMaterial;}
            if(kind!=ExpeditionFeatureKind.Barrel&&kind!=ExpeditionFeatureKind.BreakableWall&&kind!=ExpeditionFeatureKind.FragileBridge){var collider=visual.GetComponent<Collider>();collider.enabled=false;Destroy(collider);}if(model!=null){var authored=visual.transform.GetChild(0);var surfaces=authored.GetComponentsInChildren<Renderer>();if(surfaces.Length>0){Bounds bounds=surfaces[0].bounds;foreach(var surface in surfaces)bounds.Encapsulate(surface.bounds);float desired=kind==ExpeditionFeatureKind.BreakableWall?3.1f:kind==ExpeditionFeatureKind.FragileBridge?4:1;authored.localScale*=desired/Mathf.Max(.01f,Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z)));bounds=surfaces[0].bounds;foreach(var surface in surfaces)bounds.Encapsulate(surface.bounds);authored.position+=Vector3.up*(go.transform.position.y+.02f-bounds.min.y);}}
            nodes.Add(node);Paint(node);
        }
        public bool CanDamage(ExpeditionFeature node,Health source)=>run!=null&&run.Authority&&run.Running&&source!=null&&source.IsAlive&&((node.kind==ExpeditionFeatureKind.Barrel||node.kind==ExpeditionFeatureKind.BreakableWall||node.kind==ExpeditionFeatureKind.FragileBridge)&&node.state.stage==0&&source.Faction==CombatFaction.Player||node==active&&source.Faction==CombatFaction.Enemy);
        bool CanUse(Health player,ExpeditionFeature node)=>run.Authority&&run.Running&&player!=null&&player.IsAlive&&player.Faction==CombatFaction.Player&&(player.transform.position-node.root.position).sqrMagnitude<12.25f&&!Physics.Linecast(player.transform.position+Vector3.up,node.root.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore);
        public ExpeditionFeature Nearby(Health player)
        {if(player==null||!player.IsAlive)return null;ExpeditionFeature best=null;float distance=12.25f;foreach(var n in nodes){if(n.kind==ExpeditionFeatureKind.Barrel||n.kind==ExpeditionFeatureKind.BreakableWall||n.kind==ExpeditionFeatureKind.FragileBridge||n.state.stage!=0)continue;float d=(n.root.position-player.transform.position).sqrMagnitude;if(d<distance&&!Physics.Linecast(player.transform.position+Vector3.up,n.root.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore)){distance=d;best=n;}}return best;}
        public bool ChooseBoon(Health player,int slot,int id,int choice)
        {
            if(slot<0||slot>=4||dungeon.Party?.Get(slot)!=player||id<0||id>=nodes.Count||choice<0||choice>2)return false;var n=nodes[id];if(n.kind!=ExpeditionFeatureKind.Altar||n.state.stage!=0||(n.state.claimed&(1u<<slot))!=0||!CanUse(player,n)||dungeon.Encounter.Enemies.ThreatNear(player.transform.position,12))return false;
            var build=player.GetComponent<CombatBuildEffects>();if(build==null||!build.ChooseBoon(choice))return false;n.state.claimed|=1u<<slot;return true;
        }
        public bool Use(Health player,int id)
        {
            if(id<0||id>=nodes.Count)return false;var n=nodes[id];if(n.state.stage!=0||!CanUse(player,n))return false;
            if(n.kind==ExpeditionFeatureKind.Cache){if(run.SideRewardRequested==null||!run.SideRewardRequested(2000+id,false))return false;n.state.stage=2;Paint(n);return true;}
            if(n.kind!=ExpeditionFeatureKind.Challenge||EventActive||dungeon.Encounter.Enemies.ActiveCount!=0||dungeon.FightRoom!=null||data.enemies==null||data.enemies.Length==0)return false;
            active=n;n.state.stage=1;n.state.remaining=data.timeLimit;count=failures=0;eventClock=nextSpawn=0;spawned.Clear();dungeon.SideActivityBusy=true;
            n.health.Restore();if(data.optionalEvent!=OptionalEventKind.Waves)
            {
                n.visual.gameObject.layer=11;var collider=n.visual.gameObject.AddComponent<SphereCollider>();collider.radius=.65f;dungeon.Encounter.Enemies.AuxiliaryObjective=n.health;
                if(data.optionalEvent==OptionalEventKind.Escort){Vector3 desired=n.room.Center;if(!NavMesh.SamplePosition(desired,out var hit,1,NavMesh.AllAreas)||!NavMesh.CalculatePath(n.root.position,hit.position,NavMesh.AllAreas,escortPath)||escortPath.status!=NavMeshPathStatus.PathComplete){Finish(false);return false;}escortCorners=escortPath.corners;escortCorner=1;escortLength=escortTravelled=0;for(int c=1;c<escortCorners.Length;c++)escortLength+=Vector3.Distance(escortCorners[c-1],escortCorners[c]);}
            }
            Paint(n);return true;
        }
        void Update()
        {
            if(run==null||!run.Running||!run.Authority)return;scan+=Time.deltaTime;if(scan<.1f)return;float step=scan;scan=0;
            foreach(var ping in snapshot.pings)ping.remaining=Mathf.Max(0,ping.remaining-step);
            foreach(var n in nodes)
            {
                if(n.kind==ExpeditionFeatureKind.Barrel&&n.state.stage==0&&!n.health.IsAlive){n.state.stage=1;n.state.remaining=.7f;dungeon.Telegraphs.Acquire(TelegraphShape.Circle,n.root.position,Vector3.forward,data.barrelRadius,0,360,.7f,Color.red,this);}
                else if(n.kind==ExpeditionFeatureKind.Barrel&&n.state.stage==1){n.state.remaining-=step;if(n.state.remaining<=0){n.state.stage=2;Explode(n);}}
                if(n.kind==ExpeditionFeatureKind.Challenge&&n.state.stage==3&&run.SideRewardRequested!=null&&run.SideRewardRequested(3000+n.state.id,false)){n.state.stage=2;CombatAudioPool.Play(run.Level.hitSound,.15f);}if(n.kind==ExpeditionFeatureKind.BreakableWall&&!n.health.IsAlive)n.state.stage=2;
                if(n.kind==ExpeditionFeatureKind.FragileBridge)TickBridge(n,step);Paint(n);
            }
            if(!EventActive)return;eventClock+=step;active.state.remaining=Mathf.Max(0,data.timeLimit-eventClock);
            bool nearby=false;for(int i=0;i<4;i++){var p=dungeon.Party?.Get(i);if(p!=null&&p.IsAlive&&p.gameObject.activeInHierarchy&&active.room.Contains(p.transform.position))nearby=true;}
            if(!nearby||eventClock>=data.timeLimit||!active.health.IsAlive){Finish(false);return;}
            if(count<data.enemyBudget&&eventClock>=nextSpawn&&dungeon.Encounter.Enemies.ActiveCount<Mathf.Min(12,dungeon.Encounter.Enemies.Limit))
            {
                float angle=count*137*Mathf.Deg2Rad;Vector3 point=active.room.Center+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*active.room.walkableHalf*.65f;
                var enemy=count==data.enemyBudget-1&&data.miniboss!=null?data.miniboss:data.enemies[count%data.enemies.Length];var actor=dungeon.Encounter.Enemies.TrySpawn(enemy,point);
                nextSpawn=eventClock+(actor!=null?1.5f:.4f);if(actor!=null){spawned.Add(actor);count++;failures=0;}else if(++failures>=20){Finish(false);return;}
            }
            int alive=0;foreach(var enemy in spawned)if(enemy!=null&&enemy.IsAlive)alive++;
            bool objective=true;
            if(data.optionalEvent==OptionalEventKind.Defend){active.state.progress=Mathf.Clamp01(eventClock/data.defenseDuration);objective=eventClock>=data.defenseDuration;}
            else if(data.optionalEvent==OptionalEventKind.Escort)
            {
                bool escorting=false;for(int i=0;i<4;i++){var p=dungeon.Party?.Get(i);if(p!=null&&p.IsAlive&&(p.transform.position-active.root.position).sqrMagnitude<25)escorting=true;}
                if(escorting&&!dungeon.Encounter.Enemies.ThreatNear(active.root.position,3)&&escortCorners!=null&&escortCorner<escortCorners.Length){Vector3 target=escortCorners[escortCorner];Vector3 before=active.root.position;active.root.position=Vector3.MoveTowards(before,target,step*1.2f);escortTravelled+=Vector3.Distance(before,active.root.position);if(Vector3.Distance(active.root.position,target)<.15f)escortCorner++;}
                objective=escortCorners!=null&&escortCorner>=escortCorners.Length;active.state.progress=escortCorners!=null?Mathf.Clamp01(escortTravelled/Mathf.Max(.1f,escortLength)):0;
            }
            else active.state.progress=Mathf.Clamp01((float)(count-alive)/data.enemyBudget);
            if(count>=data.enemyBudget&&alive==0&&objective)Finish(true);
        }
        void Finish(bool won)
        {
            if(active==null)return;active.state.stage=won?3:4;dungeon.SideActivityBusy=false;dungeon.Encounter.Enemies.AuxiliaryObjective=null;
            var collider=active.visual.GetComponent<Collider>();if(collider!=null){collider.enabled=false;Destroy(collider);}if(!won)foreach(var actor in spawned)if(actor!=null&&actor.IsAlive)actor.Recycle();spawned.Clear();Paint(active);
        }
        void DisarmBridges(DungeonRoom room){foreach(var n in nodes)if(n.room==room&&n.kind==ExpeditionFeatureKind.FragileBridge){n.state.stage=3;n.health.Restore();n.hazardClock=0;Paint(n);}}
        void TickBridge(ExpeditionFeature node,float step)
        {
            if(node.state.stage==0){bool occupied=false;for(int i=0;i<4;i++){var hp=dungeon.Party?.Get(i);if(hp!=null&&hp.IsAlive&&(hp.transform.position-node.root.position).sqrMagnitude<2.25f)occupied=true;}if(occupied)node.state.progress+=step;if(!node.health.IsAlive||node.state.progress>=2.5f){node.state.stage=1;node.state.remaining=1;dungeon.Telegraphs.Acquire(TelegraphShape.Circle,node.root.position,Vector3.forward,2.4f,0,360,1,Color.red,this);}}
            else if(node.state.stage==1){node.state.remaining-=step;if(node.state.remaining<=0){node.state.stage=2;node.state.remaining=0;}}
            else if(node.state.stage==2){if(node.hazardClock>0){node.hazardClock-=step;if(node.hazardClock<=0)Explode(node,data.bridgeDamage,2.4f);}node.state.remaining-=step;if(node.state.remaining<=0){node.state.remaining=2.4f;node.hazardClock=.7f;dungeon.Telegraphs.Acquire(TelegraphShape.Circle,node.root.position,Vector3.forward,2.4f,0,360,.7f,Color.red,this);}}
        }
        void Explode(ExpeditionFeature node,int damage=-1,float radius=-1)
        {
            if(damage<0)damage=data.barrelDamage;if(radius<0)radius=data.barrelRadius;
            int size=Physics.OverlapSphereNonAlloc(node.root.position,radius,overlaps,(1<<10)|(1<<11)|(1<<CampaignAdventure.PropLayer),QueryTriggerInteraction.Ignore),used=0;
            for(int i=0;i<size;i++){var hp=overlaps[i].GetComponentInParent<Health>();if(hp==null||hp==node.health||!hp.IsAlive)continue;bool duplicate=false;for(int j=0;j<used;j++)if(unique[j]==hp)duplicate=true;if(duplicate)continue;unique[used++]=hp;if(!Physics.Linecast(node.root.position+Vector3.up,hp.transform.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore))hp.ApplyDamage(damage);}Array.Clear(overlaps,0,size);Array.Clear(unique,0,used);CombatAudioPool.Play(run.Level.crateSound,.3f);
        }
        void Paint(ExpeditionFeature node)
        {
            Color color=node.state.stage==4?new Color(.4f,.1f,.1f):node.state.stage>=2?new Color(.2f,.5f,.3f):node.kind==ExpeditionFeatureKind.Barrel?new Color(.8f,.22f,.06f):node.kind==ExpeditionFeatureKind.Altar?new Color(.18f,.75f,1):node.kind==ExpeditionFeatureKind.Cache?new Color(.85f,.64f,.15f):new Color(.65f,.25f,.8f);
            tint.SetColor("_BaseColor",color);node.visual.SetPropertyBlock(tint);bool breakable=node.kind==ExpeditionFeatureKind.Barrel||node.kind==ExpeditionFeatureKind.BreakableWall||node.kind==ExpeditionFeatureKind.FragileBridge;node.visual.gameObject.SetActive(!breakable||node.state.stage<2||node.kind==ExpeditionFeatureKind.FragileBridge&&node.state.stage==3);if(node.danger!=null){tint.SetColor("_BaseColor",node.state.stage==3?new Color(.15f,.55f,.3f):node.state.stage<2?new Color(.3f,.16f,.1f):new Color(.8f,.12f,.03f));node.danger.SetPropertyBlock(tint);}
        }
        public bool Ping(int slot,int kind,Health source)
        {
            if(snapshot==null||!run.Authority||!run.Running||slot<0||slot>=4||kind<0||kind>3||source==null||!source.IsAlive)return false;
            Vector3 point=source.transform.position+source.transform.forward*5;if(kind==1){Health target=null;float best=144;foreach(var enemy in dungeon.Encounter.Enemies.Actors){if(!enemy.IsAlive)continue;float distance=(enemy.transform.position-source.transform.position).sqrMagnitude;if(distance<best&&!Physics.Linecast(source.transform.position+Vector3.up,enemy.transform.position+Vector3.up,1<<9,QueryTriggerInteraction.Ignore)){best=distance;target=enemy.Health;}}if(target!=null)point=target.transform.position;}
            if(Physics.Linecast(source.transform.position+Vector3.up,point+Vector3.up,out var obstacle,1<<9,QueryTriggerInteraction.Ignore))point=obstacle.point-source.transform.forward*.5f;
            if(!NavMesh.SamplePosition(point,out var nav,1,NavMesh.AllAreas))point=source.transform.position;else point=nav.position;
            var ping=snapshot.pings[slot];ping.kind=kind;ping.position=point;ping.remaining=6;return true;
        }
        public FeatureSnapshot Capture(){foreach(var n in nodes){n.state.position=n.root.position;n.state.health=n.health.Current;}return snapshot;}
        public void Apply(FeatureSnapshot value)
        {
            if(run.Authority||value?.nodes==null||value.nodes.Length!=nodes.Count||value.pings==null||value.pings.Length!=4)return;
            for(int i=0;i<nodes.Count;i++){var n=nodes[i];var s=value.nodes[i];if(s==null||s.id!=i)continue;n.state.stage=s.stage;n.state.claimed=s.claimed;n.state.progress=s.progress;n.state.remaining=s.remaining;n.root.position=s.position;n.health.SetReplicaState(s.health,n.health.Maximum);Paint(n);}for(int i=0;i<4;i++)snapshot.pings[i]=value.pings[i];
        }
        void OnDestroy(){if(dungeon!=null)dungeon.TrapsDisabled-=DisarmBridges;}
        void OnDisable(){if(dungeon!=null){dungeon.SideActivityBusy=false;dungeon.Encounter.Enemies.AuxiliaryObjective=null;}}
    }
    public sealed class ExpeditionFeatureDefense:MonoBehaviour,IHitDefense
    {
        CampaignFeatures owner;ExpeditionFeature node;public void Configure(CampaignFeatures value,ExpeditionFeature feature){owner=value;node=feature;}
        public float DamageMultiplier(Vector3 direction,Health source,AttackDefinition attack)=>owner!=null&&owner.CanDamage(node,source)?1:0;
    }
}
