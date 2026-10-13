using UnityEngine;
using UnityEngine.InputSystem;
namespace ForjaDasMares
{
    public sealed class DungeonInteractionInput : MonoBehaviour
    {
        [SerializeField] InputActionAsset actions;
        InputActionAsset runtime;
        bool pending;
        public bool InteractionEnabled => runtime != null && runtime.FindAction("Interact", true).enabled;
        public InputAction InteractionAction => runtime != null ? runtime.FindAction("Interact", true) : null;
        public void Configure(InputActionAsset asset) => actions = asset;
        void Awake() { runtime = Instantiate(actions); runtime.FindAction("Interact", true).performed += OnInteract; }
        void OnInteract(InputAction.CallbackContext context) => pending = true;
        void OnEnable() { if (runtime != null) runtime.Enable(); }
        void OnDisable() { if (runtime != null) runtime.Disable(); pending = false; }
        void OnDestroy() { if (runtime != null) Destroy(runtime); }
        public void RequestInteraction() => pending = true;
        public bool TakeInteraction() { bool value = pending; pending = false; return value; }
    }
}
