using UnityEngine;
namespace ForjaDasMares.Networking
{
    public sealed class TownExitCue : MonoBehaviour
    {
        [SerializeField] Transform traveller;
        [SerializeField] Renderer arrow;
        [SerializeField,Min(1)] float revealDistance=11;
        public bool Visible=>arrow!=null&&arrow.enabled;
        public void Configure(Transform hero,Renderer marker){traveller=hero;arrow=marker;}
        void Update(){if(arrow!=null)arrow.enabled=traveller!=null&&(traveller.position-transform.position).sqrMagnitude<revealDistance*revealDistance;}
        void OnDisable(){if(arrow!=null)arrow.enabled=false;}
    }
}
