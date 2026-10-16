using UnityEngine;
using UnityEngine.EventSystems;
namespace ForjaDasMares
{
    // Captures one finger; leaving a menu or losing focus cannot leave an attack held.
    public sealed class HoldAttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ISubmitHandler
    {
        ArenaInput input;
        int pointer = int.MinValue;
        public void Configure(ArenaInput source) => input = source;
        public void OnPointerDown(PointerEventData data)
        { if (pointer != int.MinValue || input == null) return; pointer = data.pointerId; input.SetUiAttackHeld(true); }
        public void OnPointerUp(PointerEventData data) { if(data.pointerId == pointer) Release(); }
        public void OnSubmit(BaseEventData data) { if(input!=null)input.RequestAttack(); }
        void Release() { pointer = int.MinValue; if(input != null) input.SetUiAttackHeld(false); }
        void OnDisable() => Release();
        void OnApplicationFocus(bool focus) { if(!focus) Release(); }
    }
}
