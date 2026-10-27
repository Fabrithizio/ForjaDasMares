using UnityEngine;
using UnityEngine.Rendering;
namespace ForjaDasMares
{
    // Bounded visual attached once to pooled shots and remote shot proxies.
    public sealed class PooledProjectileTrail : MonoBehaviour
    {
        TrailRenderer trail;
        public void Configure(Material material,Color color,float radius,bool restart=false)
        {
            if(trail==null)
            {
                trail=gameObject.AddComponent<TrailRenderer>();trail.sharedMaterial=material;
                trail.time=.20f;trail.minVertexDistance=.055f;trail.numCapVertices=3;
                trail.shadowCastingMode=ShadowCastingMode.Off;trail.receiveShadows=false;
                trail.autodestruct=false;
            }
            trail.startWidth=Mathf.Clamp(radius*1.9f,.035f,.28f);trail.endWidth=0;
            trail.startColor=new Color(color.r,color.g,color.b,.7f);trail.endColor=new Color(color.r,color.g,color.b,0);
            trail.emitting=true;if(restart)trail.Clear();
        }
        void OnEnable(){if(trail!=null)trail.Clear();}
        void OnDisable(){if(trail!=null){trail.emitting=false;trail.Clear();}}
    }
}
