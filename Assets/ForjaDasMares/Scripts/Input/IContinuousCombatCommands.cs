namespace ForjaDasMares
{
    public interface IContinuousCombatCommands
    {
        bool AttackHeld { get; }
        bool CombatBlocked { get; }
        bool TakeSuper();
    }
}
