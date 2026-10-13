using UnityEngine;
using UnityEngine.AI;
namespace ForjaDasMares
{
    public sealed class DungeonDoor : MonoBehaviour
    {
        BoxCollider barrier;
        NavMeshObstacle obstacle;
        Renderer surface;
        public int A { get; private set; }
        public int B { get; private set; }
        public bool IsClosed => barrier != null && barrier.enabled;
        public void Configure(int a, int b)
        {
            A = a; B = b; barrier = GetComponent<BoxCollider>(); surface = GetComponent<Renderer>();
            obstacle = gameObject.AddComponent<NavMeshObstacle>(); obstacle.shape = NavMeshObstacleShape.Box; obstacle.size = Vector3.one; obstacle.carving = true; obstacle.carveOnlyStationary = true; obstacle.enabled = false;
            SetClosed(false);
        }
        public void SetClosed(bool closed) { barrier.enabled = closed; obstacle.enabled = closed; surface.enabled = closed; }
        public void SetVisible(bool visible) => surface.enabled = visible && IsClosed;
    }
}
