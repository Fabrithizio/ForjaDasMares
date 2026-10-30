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
        public string Label=>label;
        public Vector3 Approach=>transform.position+transform.rotation*approachOffset;
        public bool InRange(Transform actor){Vector3 d=actor.position-transform.position;d.y=0;return d.sqrMagnitude<=useDistance*useDistance;}
        public bool TryUse(Transform actor){if(!InRange(actor))return false;used.Invoke();return true;}
        public void Configure(string text,float distance=2.4f){label=text;useDistance=distance;}
    }
}
