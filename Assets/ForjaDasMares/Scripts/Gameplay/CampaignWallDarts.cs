using UnityEngine;
namespace ForjaDasMares
{
    // Clock and hit validation come from the host. Replicas render the same schedule.
    public sealed class CampaignWallDarts : MonoBehaviour
    {
        CampaignRun run;DungeonSession session;DungeonRoom room;CampaignLevel level;
        LineRenderer warning,arrow;Renderer muzzle;MaterialPropertyBlock tint;
        readonly DamageResolver damage=new DamageResolver();
        Vector3 start,end,direction,last;
        int cycle=-1;float offset;bool blocked;
        public void Configure(CampaignRun owner,DungeonSession map,DungeonRoom context,int lane,CampaignLevel data)
        {
            tint??=new MaterialPropertyBlock();run=owner;session=map;room=context;level=data;offset=lane*.65f;
            float half=map.OpenLandscape?room.walkableHalf:5.15f;
            start=room.Center+new Vector3(-half,.8f,-.5f+lane*1.25f);end=start+Vector3.right*(half*2);direction=Vector3.right;last=start;
            warning=Line("Warning_DodgeTheLane",new Color(1,.22f,.08f),.045f,2);warning.SetPosition(0,start);warning.SetPosition(1,end);
            var mount=GameObject.CreatePrimitive(PrimitiveType.Cube);mount.name="WallDartMount";mount.transform.SetParent(transform,false);mount.transform.position=start-direction*.25f;mount.transform.localScale=new Vector3(.45f,.8f,.8f);mount.GetComponent<Collider>().enabled=false;Destroy(mount.GetComponent<Collider>());mount.GetComponent<Renderer>().sharedMaterial=level.challengeMaterial;
            var barrel=GameObject.CreatePrimitive(PrimitiveType.Cylinder);barrel.name="VisibleDartEmitter";barrel.transform.SetParent(transform,false);barrel.transform.position=start-direction*.08f;barrel.transform.rotation=Quaternion.FromToRotation(Vector3.up,direction);barrel.transform.localScale=new Vector3(.32f,.4f,.32f);barrel.GetComponent<Collider>().enabled=false;Destroy(barrel.GetComponent<Collider>());muzzle=barrel.GetComponent<Renderer>();muzzle.sharedMaterial=level.challengeMaterial;
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
            bool active=run.Running&&(session.CurrentRoom==room||session.OpenLandscape&&room.entered)&&!(room.node.category==RoomCategory.Trap&&room.objectiveComplete);
            if(!active){warning.enabled=arrow.enabled=false;return;}
            float clock=run.Clock+offset;int next=Mathf.FloorToInt(clock/level.dartInterval);float time=Mathf.Repeat(clock,level.dartInterval);
            if(cycle!=next){cycle=next;damage.Clear();last=start;blocked=false;}
            warning.enabled=time<1.1f;
            tint.SetColor("_BaseColor",warning.enabled?Color.Lerp(new Color(.3f,.17f,.08f),new Color(1,.25f,.03f),time/1.1f):new Color(.25f,.3f,.32f));muzzle.SetPropertyBlock(tint);
            float distance=(time-1.1f)*(session.OpenLandscape?20:9);
            bool flying=distance>=0&&distance<Vector3.Distance(start,end)&&!blocked;arrow.enabled=flying;
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
