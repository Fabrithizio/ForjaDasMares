using System;
using UnityEngine;

namespace ForjaDasMares
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(1)] int maximum = 100;
        int current;
        public int Current => current;
        public int Maximum => maximum;
        public event Action Changed;
        public event Action Damaged;
        void Awake() => Restore();
        public bool ApplyDamage(int damage)
        {
            if (damage <= 0 || current <= 0) return false;
            current = Mathf.Max(0, current - damage);
            Damaged?.Invoke();
            Changed?.Invoke();
            return true;
        }
        public void Restore() { current = maximum; Changed?.Invoke(); }
    }
}
