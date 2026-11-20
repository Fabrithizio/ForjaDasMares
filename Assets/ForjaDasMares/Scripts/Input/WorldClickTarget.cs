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
        public bool TryUse(Transform actor){if(!isActiveAndEnabled||actor==null||!InRange(actor))return false;if(Intercept!=null&&Intercept(this,actor))return true;Interacting?.Invoke(actor);used.Invoke();return true;}
        public void Configure(string text,float distance=2.4f){label=text;useDistance=distance;}
    }
}
