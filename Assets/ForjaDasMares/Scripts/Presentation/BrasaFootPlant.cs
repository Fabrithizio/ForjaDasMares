using UnityEngine;
namespace ForjaDasMares
{
    // Presentation only. The CharacterController remains the sole motion authority.
    [DefaultExecutionOrder(200)]
    public sealed class BrasaFootPlant : MonoBehaviour
    {
        [SerializeField] HeroModelView view;
        [SerializeField] Transform leftThigh,leftShin,leftFoot,rightThigh,rightShin,rightFoot;
        [SerializeField] LayerMask groundMask=1<<8;
        [SerializeField,Min(.01f)] float ankleHeight=.135f, halfWidth=.19f;
        [SerializeField,Range(.05f,.5f)] float stepReach=.34f;
        Transform actor;
        Vector3 previous,direction;
        Quaternion leftRest,rightRest;
        readonly Foot[] feet={new Foot(),new Foot()};
        bool initialized;
        float pelvisDrop,pelvisVelocity;
        public bool LeftPlanted=>initialized&&feet[0].stance;
        public Vector3 LeftAnchor=>feet[0].anchor;
        sealed class Foot
        {
            public Transform thigh,shin,foot;
            public Vector3 anchor,liftOff,target;
            public Quaternion rotation;
            public float previousPhase,side;
            public bool stance;
        }
        public void Configure(HeroModelView owner,Transform lt,Transform ls,Transform lf,Transform rt,Transform rs,Transform rf)
        {view=owner;leftThigh=lt;leftShin=ls;leftFoot=lf;rightThigh=rt;rightShin=rs;rightFoot=rf;}
        void Start()
        {
            if(view==null||leftFoot==null||rightFoot==null||leftThigh==null||rightThigh==null||leftShin==null||rightShin==null)
            {Debug.LogError("Brasa foot planting requires the six leg bones and model view.",this);enabled=false;return;}
            var presentation=GetComponentInParent<HeroPresentation>();
            if(presentation==null){enabled=false;return;}
            actor=presentation.transform;
            feet[0].thigh=leftThigh;feet[0].shin=leftShin;feet[0].foot=leftFoot;
            feet[1].thigh=rightThigh;feet[1].shin=rightShin;feet[1].foot=rightFoot;
            feet[0].side=Mathf.Sign(Vector3.Dot(leftFoot.position-actor.position,actor.right));
            feet[1].side=Mathf.Sign(Vector3.Dot(rightFoot.position-actor.position,actor.right));
            leftRest=Quaternion.Inverse(actor.rotation)*leftFoot.rotation;
            rightRest=Quaternion.Inverse(actor.rotation)*rightFoot.rotation;
            previous=actor.position;direction=actor.forward;
        }
        void OnEnable(){initialized=false;pelvisDrop=pelvisVelocity=0;}
        Vector3 Ground(Vector3 point)
        {
            if(Physics.Raycast(point+Vector3.up,Vector3.down,out var hit,2.5f,groundMask,QueryTriggerInteraction.Ignore))point.y=hit.point.y+ankleHeight;
            else point.y=actor.position.y+.075f;
            return point;
        }
        void LateUpdate()
        {
            if(actor==null)return;
            Vector3 travel=actor.position-previous;travel.y=0;previous=actor.position;
            if(travel.sqrMagnitude>4)initialized=false;
            if(view.CurrentState==HeroState.Dead||view.CurrentState==HeroState.Dodge){initialized=false;return;}
            if(travel.sqrMagnitude>.000001f)direction=travel.normalized;
            bool moving=view.LocomotionActive;
            float stance=view.StanceFraction;
            for(int i=0;i<2;i++)
            {
                Foot f=feet[i];float phase=Mathf.Repeat(view.GaitPhase+i*.5f,1);
                bool planted=!moving||phase<stance;
                Vector3 neutral=actor.position+actor.right*(f.side*halfWidth);
                if(!initialized)
                {f.anchor=Ground(f.foot.position);f.liftOff=f.anchor;f.rotation=f.foot.rotation;f.stance=planted;}
                if(moving&&planted&&(!f.stance||phase<f.previousPhase))
                {f.anchor=Ground(neutral+direction*stepReach);f.rotation=actor.rotation*(i==0?leftRest:rightRest);}
                if(!planted&&f.stance)f.liftOff=f.anchor;
                Vector3 target;
                if(planted)
                {
                    // At rest, settle into a supported stance rather than freeze a lifted foot.
                    if(!moving)
                    {
                        f.anchor=Vector3.MoveTowards(f.anchor,Ground(neutral),Time.deltaTime*1.8f);
                        f.rotation=Quaternion.Slerp(f.rotation,actor.rotation*(i==0?leftRest:rightRest),1-Mathf.Exp(-12*Time.deltaTime));
                    }
                    target=f.anchor;
                }
                else
                {
                    float swing=Mathf.Clamp01((phase-stance)/(1-stance));
                    target=Vector3.Lerp(f.liftOff,Ground(neutral+direction*stepReach),Mathf.SmoothStep(0,1,swing));
                    target.y+=Mathf.Sin(swing*Mathf.PI)*(view.MovementSpeed>3?.19f:.10f);
                    f.rotation=Quaternion.Slerp(f.rotation,actor.rotation*(i==0?leftRest:rightRest),Time.deltaTime*14);
                }
                f.target=target;f.stance=planted;f.previousPhase=phase;
            }
            // Lower the pelvis slightly when a planted foot would otherwise exceed
            // leg reach (especially when turning from forward movement to a strafe).
            float lowerPelvis=0;
            for(int i=0;i<2;i++)
            {
                Foot f=feet[i];Vector3 offset=f.target-f.thigh.position;
                float length=Vector3.Distance(f.thigh.position,f.shin.position)+Vector3.Distance(f.shin.position,f.foot.position)-.008f;
                float vertical=Mathf.Sqrt(Mathf.Max(.01f,length*length-offset.x*offset.x-offset.z*offset.z));
                lowerPelvis=Mathf.Max(lowerPelvis,-offset.y-vertical);
            }
            pelvisDrop=Mathf.SmoothDamp(pelvisDrop,Mathf.Clamp(lowerPelvis,0,.10f),ref pelvisVelocity,.035f,3,Time.deltaTime);
            leftThigh.parent.position+=Vector3.down*pelvisDrop;
            for(int i=0;i<2;i++){Solve(feet[i],feet[i].target,actor.forward);feet[i].foot.rotation=feet[i].rotation;}
            initialized=true;
        }
        static void Solve(Foot f,Vector3 target,Vector3 forward)
        {
            Vector3 origin=f.thigh.position;
            float upper=Vector3.Distance(origin,f.shin.position),lower=Vector3.Distance(f.shin.position,f.foot.position);
            Vector3 delta=target-origin;float distance=Mathf.Clamp(delta.magnitude,.05f,upper+lower-.002f);
            Vector3 axis=delta.normalized;
            Vector3 bend=Vector3.ProjectOnPlane(forward,axis);
            if(bend.sqrMagnitude<.001f)bend=Vector3.ProjectOnPlane(Vector3.forward,axis);
            bend.Normalize();
            float along=(upper*upper-lower*lower+distance*distance)/(2*distance);
            float height=Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
            Vector3 knee=origin+axis*along+bend*height;
            f.thigh.rotation=Quaternion.FromToRotation(f.shin.position-origin,knee-origin)*f.thigh.rotation;
            f.shin.rotation=Quaternion.FromToRotation(f.foot.position-f.shin.position,origin+axis*distance-f.shin.position)*f.shin.rotation;
        }
    }
}
