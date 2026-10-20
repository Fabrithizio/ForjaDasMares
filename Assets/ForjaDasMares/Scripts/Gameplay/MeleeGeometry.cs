using UnityEngine;
namespace ForjaDasMares
{
    public static class MeleeGeometry
    {
        public static bool Contains(Vector3 delta,Vector3 forward,float range,float arc,float width,MeleeFootprint footprint)
        {
            delta.y=0;forward.y=0;forward=forward.sqrMagnitude>.001f?forward.normalized:Vector3.forward;
            if(arc>=359.9f||footprint==MeleeFootprint.Circle)return delta.sqrMagnitude<=range*range;
            if(footprint==MeleeFootprint.Line){float along=Vector3.Dot(delta,forward),across=Vector3.Dot(delta,Vector3.Cross(Vector3.up,forward));return along>=0&&along<=range&&Mathf.Abs(across)<=width*.5f;}
            return delta.sqrMagnitude<=range*range&&(delta.sqrMagnitude<.001f||Vector3.Dot(forward,delta.normalized)>=Mathf.Cos(arc*.5f*Mathf.Deg2Rad));
        }
    }
}
