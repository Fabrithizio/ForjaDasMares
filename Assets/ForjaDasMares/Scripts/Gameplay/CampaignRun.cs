using System;
using System.Collections.Generic;
using UnityEngine;
namespace ForjaDasMares
{
    [Serializable] public sealed class CampaignSnapshot
    {public float clock;public int wave;public uint occupied;public float[] progress;}
    public sealed class CampaignRun : MonoBehaviour
    {
        [SerializeField] DungeonSession dungeon;
        [SerializeField] CampaignLevel[] levels;
        readonly List<CooperativePlates> puzzles=new List<CooperativePlates>();
        readonly CampaignSnapshot snapshot=new CampaignSnapshot();
        public bool Authority {get;set;}
        public bool Running {get;set;}
        public int Selected {get;private set;}
        public float Clock {get;private set;}
        int replicaWave=1;
        public int Count=>levels.Length;
        public CampaignLevel Level=>levels[Selected];
        public CampaignLevel At(int index)=>levels[Mathf.Clamp(index,0,levels.Length-1)];
        public IReadOnlyList<CooperativePlates> Puzzles=>puzzles;
        public string Objective
        {
            get
            {
                foreach(var puzzle in puzzles)if(puzzle.Room==dungeon.CurrentRoom&&!puzzle.Solved)
                    return "Dois heróis nas placas · "+puzzle.Occupants+"/2 · "+Mathf.RoundToInt(puzzle.Progress*100)+"%";
                return dungeon.CurrentRoom!=null&&dungeon.CurrentRoom.encounter!=null&&!dungeon.CurrentRoom.completed?"Derrotem a onda "+(Authority?dungeon.Encounter.WaveNumber:replicaWave)+" e protejam o grupo":"Explorem juntos · E / Interagir para baús, alavancas e saída";
            }
        }
        public void Configure(DungeonSession owner,CampaignLevel[] data){dungeon=owner;levels=data;}
        void Awake(){dungeon.Generated+=Build;}
        void OnDestroy(){if(dungeon!=null)dungeon.Generated-=Build;}
        public void Select(int index){Selected=Mathf.Clamp(index,0,levels.Length-1);}
        public void Prepare(int index){Select(index);Level.Validate();Clock=0;dungeon.Generator.ConfigureCampaign(Level);}
        void Update()
        {
            if(!Running)return;
            Clock+=Time.deltaTime;
            if(Authority)foreach(var puzzle in puzzles)puzzle.Tick(Time.deltaTime);
        }
        void Build()
        {
            puzzles.Clear();Clock=0;
            foreach(var room in dungeon.Generator.Rooms)
            {
                if(room.node.category==RoomCategory.Event)
                {
                    var pair=room.root.AddComponent<CooperativePlates>();pair.Configure(dungeon,room,Level);
                    puzzles.Add(pair);
                }
                if(room.node.category==RoomCategory.Trap||room.node.category==RoomCategory.HardCombat)
                    for(int lane=0;lane<2+Level.biomeIndex;lane++)
                    {
                        var go=new GameObject("WallDarts_"+lane);go.transform.SetParent(room.root.transform,false);
                        go.AddComponent<CampaignWallDarts>().Configure(this,dungeon,room,lane,Level);
                    }
            }
            snapshot.progress=new float[puzzles.Count];snapshot.occupied=0;
            // A low transparent mist sea fills the exterior without hiding the walkable floor.
            Vector3 min=Vector3.zero,max=Vector3.zero;
            foreach(var room in dungeon.Generator.Rooms){min=Vector3.Min(min,room.Center);max=Vector3.Max(max,room.Center);}
            var sea=GameObject.CreatePrimitive(PrimitiveType.Plane);sea.name="ExteriorMist_OriginalPlaceholder";
            var collider=sea.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            sea.transform.SetParent(dungeon.Generator.Rooms[0].root.transform);sea.transform.position=(min+max)*.5f+Vector3.down*.65f;
            sea.transform.localScale=new Vector3((max.x-min.x+160)/10,1,(max.z-min.z+160)/10);
            sea.GetComponent<Renderer>().sharedMaterial=Level.mist;
            Camera.main.backgroundColor=Level.atmosphere;
        }
        public CampaignSnapshot Capture()
        {
            snapshot.clock=Clock;snapshot.wave=dungeon.Encounter.WaveNumber;snapshot.occupied=0;
            for(int i=0;i<puzzles.Count;i++){snapshot.progress[i]=puzzles[i].Progress;snapshot.occupied|=(uint)puzzles[i].Mask<<(i*2);}
            return snapshot;
        }
        public void Apply(CampaignSnapshot value)
        {
            if(value==null||value.progress==null||value.progress.Length!=puzzles.Count||float.IsNaN(value.clock)||float.IsInfinity(value.clock))return;
            Clock=Mathf.Max(0,value.clock);replicaWave=Mathf.Max(1,value.wave);
            for(int i=0;i<puzzles.Count;i++)puzzles[i].Apply(value.progress[i],(int)(value.occupied>>(i*2))&3);
        }
    }
    public sealed class CooperativePlates : MonoBehaviour
    {
        DungeonSession session;CampaignLevel level;
        readonly Renderer[] plates=new Renderer[2];
        readonly Health[] occupants=new Health[2];
        MaterialPropertyBlock block;
        float held;
        public DungeonRoom Room {get;private set;}
        public float Progress=>Mathf.Clamp01(held/level.plateHold);
        public bool Solved=>Room.objectiveComplete;
        public int Mask {get;private set;}
        public int Occupants=>(Mask&1)+((Mask>>1)&1);
        public Vector3 Point(int index)=>plates[index].transform.position;
        public void Configure(DungeonSession owner,DungeonRoom room,CampaignLevel data)
        {
            session=owner;Room=room;level=data;block=new MaterialPropertyBlock();
            for(int i=0;i<2;i++)
            {
                var plate=GameObject.CreatePrimitive(PrimitiveType.Cylinder);plate.name="CooperativePlate_"+(i+1);
                plate.transform.SetParent(transform,false);plate.transform.localPosition=new Vector3(i==0?-3:3,.04f,i==0?-1:1);plate.transform.localScale=new Vector3(1.8f,.035f,1.8f);
                var collider=plate.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
                plates[i]=plate.GetComponent<Renderer>();plates[i].sharedMaterial=data.challengeMaterial;
            }
            Paint();
        }
        public void Tick(float delta)
        {
            if(session.CurrentRoom!=Room||Solved)return;
            Mask=0;occupants[0]=occupants[1]=null;
            if(session.Party!=null)for(int p=0;p<4;p++)
            {
                var hp=session.Party.Get(p);if(hp==null||!hp.IsAlive||!hp.gameObject.activeInHierarchy)continue;
                for(int i=0;i<2;i++)
                {
                    Vector3 offset=hp.transform.position-Point(i);offset.y=0;
                    if(offset.sqrMagnitude<=.85f*.85f&&occupants[i]==null){occupants[i]=hp;Mask|=1<<i;}
                }
            }
            bool valid=Mask==3&&occupants[0]!=occupants[1];
            held=valid?Mathf.Min(level.plateHold,held+Mathf.Max(0,delta)):Mathf.Max(0,held-delta*2);
            if(held>=level.plateHold)session.CompleteObjective(Room,true);
            Paint();
        }
        public void Apply(float progress,int mask){if(float.IsNaN(progress)||float.IsInfinity(progress))return;held=Mathf.Clamp01(progress)*level.plateHold;Mask=mask&3;Paint();}
        void Paint()
        {
            for(int i=0;i<2;i++)
            {block.SetColor("_BaseColor",Solved?new Color(.2f,.9f,.55f):(Mask&(1<<i))!=0?new Color(1,.8f,.25f):new Color(.35f,.48f,.65f));plates[i].SetPropertyBlock(block);}
        }
    }
}
