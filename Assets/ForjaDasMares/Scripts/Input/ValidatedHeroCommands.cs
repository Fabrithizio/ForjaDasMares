using UnityEngine;
namespace ForjaDasMares
{
    [System.Flags] public enum HeroButtons : byte { None = 0, Attack = 1, Dodge = 2, Special = 4, Interact = 8 }
    public sealed class ValidatedHeroCommands : MonoBehaviour, IHeroCommands
    {
        Vector2 move;
        HeroButtons pending;
        float receivedAt = -100;
        uint lastSequence;
        bool received;
        public Vector2 Move => Time.unscaledTime - receivedAt < .35f ? move : Vector2.zero;
        public Vector2 PointerPosition => Vector2.zero;
        public bool HasPointer => false;
        public static bool IsValid(Vector2 movement, Vector2 aim, HeroButtons buttons)
        {
            return Finite(movement.x) && Finite(movement.y) && Finite(aim.x) && Finite(aim.y)
                && movement.sqrMagnitude <= 1.001f && aim.sqrMagnitude <= 1.001f
                && (buttons & ~(HeroButtons.Attack | HeroButtons.Dodge | HeroButtons.Special | HeroButtons.Interact)) == 0;
        }
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public bool Accept(uint sequence, Vector2 movement, Vector2 aim, HeroButtons buttons)
        {
            if (!IsValid(movement, aim, buttons) || received && unchecked((int)(sequence - lastSequence)) <= 0) return false;
            received = true; lastSequence = sequence; receivedAt = Time.unscaledTime; move = Vector2.ClampMagnitude(movement, 1); pending |= buttons;
            return true;
        }
        public void Clear() { move = Vector2.zero; pending = HeroButtons.None; received = false; receivedAt = -100; }
        bool Take(HeroButtons button) { bool value = (pending & button) != 0; pending &= ~button; return value; }
        public bool TakeAttack() => Take(HeroButtons.Attack);
        public bool TakeDodge() => Take(HeroButtons.Dodge);
        public bool TakeSpecial() => Take(HeroButtons.Special);
        public bool TakeInteraction() => Take(HeroButtons.Interact);
        public bool TakeReset() => false;
        public bool TakeNextHero() => false;
        public int TakeHeroSlot() => -1;
        public bool TakeTestDamage() => false;
        public bool TakeVictory() => false;
    }
}
