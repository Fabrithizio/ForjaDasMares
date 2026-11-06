using UnityEngine;

namespace ForjaDasMares.Networking
{
    // A physical road threshold. Only the configured local hero can request travel.
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TownRoadPassage : MonoBehaviour
    {
        [SerializeField] TownHub hub;
        [SerializeField] Transform traveller;
        public void Configure(TownHub town, Transform hero) { hub=town; traveller=hero; }
        void OnTriggerEnter(Collider other) => Enter(other);
        void OnTriggerStay(Collider other) => Enter(other);
        void Enter(Collider other)
        {
            if(hub==null || traveller==null || other==null || hub.IsTravelling) return;
            if(other.transform==traveller || other.transform.IsChildOf(traveller)) hub.Explore();
        }
    }
}
