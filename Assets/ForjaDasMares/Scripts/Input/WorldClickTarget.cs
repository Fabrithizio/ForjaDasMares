using UnityEngine;
using UnityEngine.Events;
namespace ForjaDasMares
{
    // Explicit click-to-approach interaction; NPCs are never treated as damage targets.
    public sealed class WorldClickTarget:MonoBehaviour
    {
        [SerializeField] string label="Interagir";
        [SerializeField,Min(.5f)]float useDistance=2.4f;
        [SerializeField]Vector3 approachOffset=Vector3.back*1.6f;
        [SerializeField]UnityEvent used=new UnityEvent();
        public UnityEvent Used=>used;
        public event System.Action<Transform> Interacting;
        public string Label=>label;
        public string HoverLabel { get; set; }
        public Vector3 Approach=>transform.position+transform.rotation*approachOffset;
        public bool InRange(Transform actor){Vector3 d=actor.position-transform.position;d.y=0;return d.sqrMagnitude<=useDistance*useDistance;}
        // The story can take over a talk (for example Nara during the start of the game). True = handled by the story.
        public static System.Func<WorldClickTarget,Transform,bool> Intercept;
        // Targets made while playing (herbs...), so the F key and the hint also find them.
        public static readonly System.Collections.Generic.List<WorldClickTarget> Extras=new System.Collections.Generic.List<WorldClickTarget>();
        // Colliders of a building that stand for a target (click the Raiz-Mãe itself to use it).
        public static readonly System.Collections.Generic.Dictionary<Collider,WorldClickTarget> Alias=new System.Collections.Generic.Dictionary<Collider,WorldClickTarget>();
        // The closest of the extra targets (gates, loot, herbs...) that the hero can use right now.
        // Click shapes (loot, herbs, arrows, secrets) must not be walls: whoever walks passes through them.
        public static void Atravessavel(Collider shape)
        {
            if(shape==null)return;
            foreach(var walker in Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None))Physics.IgnoreCollision(walker,shape,true);
        }
        // Big things (a Raiz-Mãe, an exit arrow): clicking anywhere within ClickRadius (metres, on the ground plane)
        // of ClickCenter uses them, even where the model has no collider of its own.
        // What lights up (gold outline) when the mouse is over it; null = the target's own model.
        public Renderer[] Realce{get;set;}
        // Clicks only count on the object itself, never on scenery standing between the camera and the hero.
        public float ClickRadius{get;set;}
        public Vector3? ClickCenter{get;set;}
        public static readonly System.Collections.Generic.List<WorldClickTarget> Areas=new System.Collections.Generic.List<WorldClickTarget>();
        public void ClicavelEmVolta(float radius,Vector3? center=null){ClickRadius=radius;ClickCenter=center;if(!Areas.Contains(this))Areas.Add(this);}
        public static WorldClickTarget PertoDoClique(Vector3 point)
        {
            WorldClickTarget best=null;float distance=float.MaxValue;
            for(int i=Areas.Count-1;i>=0;i--)
            {
                var t=Areas[i];if(t==null){Areas.RemoveAt(i);continue;}
                if(!t.isActiveAndEnabled)continue;
                Vector3 d=point-(t.ClickCenter??t.transform.position);d.y=0;
                if(d.sqrMagnitude<=t.ClickRadius*t.ClickRadius&&d.sqrMagnitude<distance){distance=d.sqrMagnitude;best=t;}
            }
            return best;
        }
        public static WorldClickTarget MaisPerto(Transform hero)
        {
            WorldClickTarget best=null;float distance=float.MaxValue;
            if(hero==null)return null;
            foreach(var t in Extras)
            {
                if(t==null||!t.isActiveAndEnabled||!t.InRange(hero))continue;
                float d=(t.transform.position-hero.position).sqrMagnitude;
                if(d<distance){distance=d;best=t;}
            }
            return best;
        }
        public bool TryUse(Transform actor){if(!isActiveAndEnabled||actor==null||!InRange(actor))return false;if(Intercept!=null&&Intercept(this,actor))return true;Interacting?.Invoke(actor);used.Invoke();return true;}
        public void Configure(string text,float distance=2.4f){label=text;useDistance=distance;}
    }
}
