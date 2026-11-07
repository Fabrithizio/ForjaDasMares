using UnityEngine;
using UnityEngine.AI;
namespace ForjaDasMares.Networking
{
    // Provisional jointed geometry. No claim of final humanoid models or animation clips.
    public sealed class TownResident:MonoBehaviour
    {
        [SerializeField]Vector3[] waypoints;
        [SerializeField]Transform leftLeg,rightLeg,leftArm,rightArm;
        [SerializeField]Transform nameplate;
        WorldClickTarget target;
        Transform conversationPartner;
        float conversationUntil;
        NavMeshAgent agent;float wait,phase;int destination;
        public void Configure(Vector3[] route,Transform a,Transform b,Transform c,Transform d,Transform label){waypoints=route;leftLeg=a;rightLeg=b;leftArm=c;rightArm=d;nameplate=label;}
        void LateUpdate(){if(nameplate!=null)nameplate.rotation=Quaternion.Euler(50,0,0);}
        void Start()
        {
            target=GetComponent<WorldClickTarget>();
            if(target!=null)target.Interacting+=FacePlayer;
            phase=transform.GetSiblingIndex()*.7f;
            if(waypoints==null||waypoints.Length<2){var obstacle=gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Capsule;obstacle.radius=.35f;obstacle.height=1.8f;obstacle.center=Vector3.up*.9f;obstacle.carving=true;return;}
            agent=gameObject.AddComponent<NavMeshAgent>();agent.radius=.3f;agent.height=1.7f;agent.speed=1.15f;agent.acceleration=4;agent.angularSpeed=240;agent.stoppingDistance=.2f;agent.avoidancePriority=30+transform.GetSiblingIndex()%30;
            if(NavMesh.SamplePosition(transform.position,out var hit,2,NavMesh.AllAreas)){agent.Warp(hit.position);agent.SetDestination(waypoints[0]);}else agent.enabled=false;
        }
        void FacePlayer(Transform actor)
        {
            conversationPartner=actor;
            conversationUntil=Time.time+1.5f;
            if(agent!=null&&agent.isOnNavMesh)agent.isStopped=true;
            TurnToward(actor);
        }
        void TurnToward(Transform actor)
        {
            Vector3 direction=actor.position-transform.position;
            direction.y=0;
            if(direction.sqrMagnitude>.001f)transform.rotation=Quaternion.LookRotation(direction);
        }
        void OnDestroy(){if(target!=null)target.Interacting-=FacePlayer;}
        void Update()
        {
            if(conversationPartner!=null&&(Time.time<conversationUntil||target.InRange(conversationPartner)))
            {
                TurnToward(conversationPartner);
                return;
            }
            if(agent!=null&&agent.isOnNavMesh&&agent.isStopped)agent.isStopped=false;
            conversationPartner=null;
            float speed=agent!=null&&agent.enabled&&agent.isOnNavMesh?agent.velocity.magnitude:0;
            phase+=Time.deltaTime*(speed>.1f?7:1.4f);float stride=Mathf.Sin(phase)*(speed>.1f?23:2);
            if(leftLeg!=null)leftLeg.localRotation=Quaternion.Euler(stride,0,0);if(rightLeg!=null)rightLeg.localRotation=Quaternion.Euler(-stride,0,0);
            if(leftArm!=null)leftArm.localRotation=Quaternion.Euler(-stride*.65f,0,-5);if(rightArm!=null)rightArm.localRotation=Quaternion.Euler(stride*.65f,0,5);
            if(agent==null||!agent.enabled||!agent.isOnNavMesh||agent.pathPending)return;
            if(agent.remainingDistance<=.4f){wait+=Time.deltaTime;if(wait>3){wait=0;destination=(destination+1)%waypoints.Length;agent.SetDestination(waypoints[destination]);}}
        }
    }
}
