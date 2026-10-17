using UnityEngine;
namespace ForjaDasMares
{
    // Procedural motion for the original segmented models, including interpolated replicas.
    public sealed class EnemyModelMotion : MonoBehaviour
    {
        EnemyActor actor;Transform leftLeg,rightLeg,leftArm,rightArm;
        Vector3 previous;float stride,speed;
        bool replica;EnemyState replicaState;
        public void SetReplicaState(EnemyState state){replica=true;replicaState=state;}
        public void ClearReplicaState(){replica=false;}
        void Awake()
        {
            actor=GetComponentInParent<EnemyActor>();
            foreach(var part in GetComponentsInChildren<Transform>())
            {if(part.name.StartsWith("LegL")&&!part.name.Contains("Mesh"))leftLeg=part;if(part.name.StartsWith("LegR")&&!part.name.Contains("Mesh"))rightLeg=part;if(part.name.StartsWith("ArmL")&&!part.name.Contains("Mesh"))leftArm=part;if(part.name.StartsWith("ArmR")&&!part.name.Contains("Mesh"))rightArm=part;}
        }
        void OnEnable(){previous=transform.position;stride=speed=0;}
        void LateUpdate()
        {
            if(actor==null)return;Vector3 delta=transform.position-previous;delta.y=0;previous=transform.position;
            float travel=Mathf.Min(delta.magnitude,.4f);speed=Mathf.Lerp(speed,travel/Mathf.Max(Time.deltaTime,.001f),1-Mathf.Exp(-15*Time.deltaTime));
            stride+=travel*5;float step=actor.Health.IsAlive?Mathf.Sin(stride)*Mathf.Clamp01(speed)*30:0;
            if(leftLeg!=null)leftLeg.localRotation=Quaternion.Euler(step,0,0);if(rightLeg!=null)rightLeg.localRotation=Quaternion.Euler(-step,0,0);
            float attack=actor.Health.IsAlive&&(replica?replicaState:actor.Brain.State)==EnemyState.Attack?-45:0;
            if(leftArm!=null)leftArm.localRotation=Quaternion.Euler(-step*.6f+attack,0,0);if(rightArm!=null)rightArm.localRotation=Quaternion.Euler(step*.6f+attack,0,0);
        }
    }
}
