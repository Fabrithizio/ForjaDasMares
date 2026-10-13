using System;
using UnityEngine;

namespace ForjaDasMares
{
    [DefaultExecutionOrder(-75)]
    public sealed class HeroRoster : MonoBehaviour
    {
        [SerializeField] HeroDefinition[] heroes;
        [SerializeField] PlayerMotor motor;
        [SerializeField] HeroCombat combat;
        [SerializeField] Health health, ally;
        [SerializeField] HeroPresentation presentation;
        int selected;
        public HeroDefinition Current => heroes[selected];
        public int Count => heroes.Length;
        public int Selected => selected;
        public Health Health => health;
        public Health Ally => ally;
        public event Action Changed;
        public void Configure(HeroDefinition[] data, PlayerMotor movement, HeroCombat attacks, Health hitPoints, Health trainingAlly, HeroPresentation visual)
        { heroes = data; motor = movement; combat = attacks; health = hitPoints; ally = trainingAlly; presentation = visual; }
        void Awake() => Select(0);
        void Update()
        {
            if (!(motor.Commands is IHeroCommands input)) return;
            int slot = input.TakeHeroSlot(); if (slot >= 0) Select(slot);
            if (input.TakeNextHero()) Next();
            if (input.TakeTestDamage()) TestDamage();
            if (input.TakeVictory()) Victory();
        }
        public void Select(int index)
        {
            if (heroes == null || index < 0 || index >= heroes.Length || heroes[index] == null) return;
            selected = index; HeroDefinition hero = heroes[index];
            health.Configure(hero.maximumHealth, CombatFaction.Player, hero.armor, motor);
            motor.ApplyHero(hero, health); combat.ApplyHero(hero); presentation.Show(hero.visualSlot);
            Changed?.Invoke();
        }
        public void Next() => Select((selected + 1) % heroes.Length);
        public void TestDamage() { health.ApplyDamage(40); if (ally != null) ally.ApplyDamage(40); }
        public void Victory() => presentation.ShowVictory();
        public void ResetHero() { health.Restore(); combat.ResetCombat(); presentation.ResetPose(); }
    }
}
