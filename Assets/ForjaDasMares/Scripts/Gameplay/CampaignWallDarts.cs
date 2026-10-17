using UnityEngine;
namespace ForjaDasMares
{
    // Clock and hit validation come from the host. Replicas render the same schedule.
    public sealed class CampaignWallDarts : MonoBehaviour
    {
        CampaignRun run;DungeonSession session;DungeonRoom room;CampaignLevel level;
        LineRenderer warning,arrow;
        readonly DamageResolver damage=new DamageResolver();
        Vector3 start,end,direction,last;
        int cycle=-1;float offset;bool blocked;
        public void Configure(CampaignRun owner,DungeonSession map,DungeonRoom context,int lane,CampaignLevel data)
        {
            run=owner;session=map;room=context;level=data;offset=lane*.65f;
            start=room.Center+new Vector3(-5.15f,.8f,-.5f+lane*1.25f);end=start+Vector3.right*10.3f;direction=Vector3.right;last=start;
            warning=Line("Warning_DodgeTheLane",new Color(1,.22f,.08f),.045f,2);warning.SetPosition(0,start);warning.SetPosition(1,end);
            arrow=Line("Arrow_OriginalPlaceholder",new Color(1,.83f,.3f),.075f,4);
        }
        LineRenderer Line(string name,Color color,float width,int count)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=level.challengeMaterial;
            line.useWorldSpace=true;line.positionCount=count;line.startWidth=line.endWidth=width;line.numCapVertices=2;
            var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",color);line.SetPropertyBlock(block);line.enabled=false;return line;
        }
        void Update()
        {
            bool active=run.Running&&session.CurrentRoom==room&&!(room.node.category==RoomCategory.Trap&&room.objectiveComplete);
            if(!active){warning.enabled=arrow.enabled=false;return;}
            float clock=run.Clock+offset;int next=Mathf.FloorToInt(clock/level.dartInterval);float time=Mathf.Repeat(clock,level.dartInterval);
            if(cycle!=next){cycle=next;damage.Clear();last=start;blocked=false;}
            warning.enabled=time<1.1f;
            float distance=(time-1.1f)*9;
            bool flying=distance>=0&&distance<10.3f&&!blocked;arrow.enabled=flying;
            if(!flying)return;
            Vector3 point=start+direction*distance;
            if(Physics.Linecast(last,point,1<<9,QueryTriggerInteraction.Ignore)){blocked=true;arrow.enabled=false;return;}
            if(run.Authority)
            {
                float length=Vector3.Distance(last,point);int steps=Mathf.Max(1,Mathf.CeilToInt(length/.3f));
                for(int i=1;i<=steps;i++)damage.Area(Vector3.Lerp(last,point,(float)i/steps),.46f,direction,360,level.dartAttack,null,1<<11,1<<9,false);
            }
            arrow.SetPosition(0,point-direction*.65f);arrow.SetPosition(1,point);arrow.SetPosition(2,point-direction*.18f+Vector3.forward*.13f);arrow.SetPosition(3,point-direction*.18f-Vector3.forward*.13f);last=point;
        }
    }
}
