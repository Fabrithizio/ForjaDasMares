using UnityEngine;

namespace ForjaDasMares
{
    public sealed class CombatFeedback : MonoBehaviour
    {
        [SerializeField] MeleeAttack attack;
        [SerializeField] Transform weaponPivot;
        [SerializeField] Renderer slash;
        Quaternion restRotation;
        public void Configure(MeleeAttack combat, Transform pivot, Renderer slashRenderer) { attack = combat; weaponPivot = pivot; slash = slashRenderer; }
        void Awake() { restRotation = weaponPivot.localRotation; slash.enabled = false; }
        void LateUpdate()
        {
            bool active = attack.IsAttacking;
            slash.enabled = active;
            weaponPivot.localRotation = active ? restRotation * Quaternion.Euler(0f, Mathf.Lerp(-65f, 75f, attack.NormalizedTime), 0f) : restRotation;
        }
    }
}
