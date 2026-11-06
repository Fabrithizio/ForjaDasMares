using UnityEngine;
namespace ForjaDasMares.Networking
{
    public enum TownSpaceKind { Event, Resident, FutureExpansion }
    // Authoring reservation only: no invisible walls, fake NPC services or runtime polling.
    public sealed class TownActivitySpace : MonoBehaviour
    {
        [SerializeField] string spaceId;
        [SerializeField] TownSpaceKind kind;
        [SerializeField] Vector2 size=new Vector2(4,4);
        public string SpaceId=>spaceId;
        public TownSpaceKind Kind=>kind;
        public Vector2 Size=>size;
        public void Configure(string id,TownSpaceKind category,Vector2 footprint){spaceId=id;kind=category;size=footprint;}
#if UNITY_EDITOR
        void OnDrawGizmosSelected(){Gizmos.color=kind==TownSpaceKind.FutureExpansion?Color.cyan:kind==TownSpaceKind.Event?Color.magenta:Color.yellow;Gizmos.DrawWireCube(transform.position+Vector3.up*.15f,new Vector3(size.x,.3f,size.y));}
#endif
    }
}
