using UnityEngine;
namespace ForjaDasMares
{
    [DefaultExecutionOrder(120)] public sealed class EnemyArrivalVisual:MonoBehaviour
    {
        EnemyActor actor;LineRenderer ring;Transform body;bool wasArriving;
        public void Configure(EnemyActor owner)
        {
            actor=owner;if(ring!=null)return;
            var presentation=GetComponent<EnemyPresentation>();body=presentation.Model;
            var go=new GameObject("ArrivalWarning");go.transform.SetParent(transform,false);ring=go.AddComponent<LineRenderer>();ring.loop=true;ring.useWorldSpace=false;ring.positionCount=24;ring.startWidth=ring.endWidth=.055f;
            var renderer=body.GetComponentInChildren<Renderer>();ring.sharedMaterial=renderer.sharedMaterial;ring.startColor=ring.endColor=new Color(1,.45f,.08f);
            var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",new Color(1,.45f,.08f));ring.SetPropertyBlock(block);
            for(int i=0;i<24;i++){float angle=i*Mathf.PI*2/24;ring.SetPosition(i,new Vector3(Mathf.Cos(angle)*.8f,.06f,Mathf.Sin(angle)*.8f));}ring.enabled=false;
        }
        void LateUpdate()
        {
            if(actor==null||ring==null)return;bool arriving=actor.SpawnRemaining>0&&actor.Health.IsAlive;ring.enabled=arriving;
            if(arriving){float fraction=actor.SpawnRemaining/Mathf.Max(.01f,actor.SpawnDuration);body.localPosition=Vector3.down*Mathf.Clamp01(fraction)*actor.Profile.height;ring.transform.localScale=Vector3.one*(1+.12f*Mathf.Sin(Time.time*16));}
            else if(wasArriving)body.localPosition=Vector3.zero;wasArriving=arriving;
        }
        void OnDisable(){if(ring!=null)ring.enabled=false;}
    }
}
