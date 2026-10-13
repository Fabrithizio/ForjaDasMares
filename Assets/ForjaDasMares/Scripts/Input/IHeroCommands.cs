namespace ForjaDasMares
{
    public interface IHeroCommands : IPlayerCommands
    {
        bool TakeSpecial();
        bool TakeNextHero();
        int TakeHeroSlot();
        bool TakeTestDamage();
        bool TakeVictory();
    }
}
