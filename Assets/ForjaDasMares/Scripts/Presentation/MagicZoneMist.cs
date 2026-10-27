using UnityEngine;
using UnityEngine.Rendering;
namespace ForjaDasMares
{
    // Soft ground motes replace the old solid cylinder. Gameplay stays in CombatZone.
    public sealed class MagicZoneMist:MonoBehaviour
    {
        ParticleSystem particles;float radius,clock;Color tint;int serial;
        public void Configure(float area,Color color)
        {
            radius=Mathf.Max(.1f,area);tint=color;
            var body=GetComponent<MeshRenderer>();if(body!=null)body.enabled=false;
            if(particles!=null)return;
            var go=new GameObject("GroundMagicMotes_24");go.transform.SetParent(transform,false);particles=go.AddComponent<ParticleSystem>();
            var main=particles.main;main.playOnAwake=false;main.loop=false;main.maxParticles=24;main.simulationSpace=ParticleSystemSimulationSpace.World;main.scalingMode=ParticleSystemScalingMode.Local;
            var emission=particles.emission;emission.enabled=false;var shape=particles.shape;shape.enabled=false;
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Resources.Load<Material>("HeroEnergySpark");renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            var fade=particles.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.7f,.2f),new GradientAlphaKey(0,1)});fade.color=gradient;
            particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        void OnEnable(){clock=0;if(particles!=null)particles.Clear();}
        void OnDisable(){if(particles!=null)particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
        void Update()
        {
            if(particles==null)return;clock-=Time.deltaTime;if(clock>0)return;clock=.06f;particles.Play();
            for(int i=0;i<2;i++)
            {
                float angle=(serial++%10000)*2.399963f;float distance=radius*Mathf.Sqrt((serial%17+.5f)/17f);
                Vector3 at=transform.position+new Vector3(Mathf.Cos(angle)*distance,.06f,Mathf.Sin(angle)*distance);
                particles.Emit(new ParticleSystem.EmitParams{position=at,velocity=Vector3.up*.15f,startColor=tint,startSize=.12f+radius*.06f,startLifetime=.55f},1);
            }
        }
    }
}
