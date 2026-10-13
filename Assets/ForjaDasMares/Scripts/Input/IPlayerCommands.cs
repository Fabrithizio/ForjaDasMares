using UnityEngine;

namespace ForjaDasMares
{
    // Input adapters supply intentions. Gameplay never reads keyboard or touch APIs.
    public interface IPlayerCommands
    {
        Vector2 Move { get; }
        Vector2 PointerPosition { get; }
        bool HasPointer { get; }
        bool TakeAttack();
        bool TakeDodge();
        bool TakeReset();
    }
}
