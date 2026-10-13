using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares
{
    public sealed class HeroHud : MonoBehaviour
    {
        [SerializeField] HeroRoster roster;
        [SerializeField] HeroCombat combat;
        [SerializeField] Text status;
        int lastCooldown = -1, lastHp = -1, lastAlly = -1, lastSlot = -1, lastShield = -1;
        public void Configure(HeroRoster heroes, HeroCombat attacks, Text text) { roster = heroes; combat = attacks; status = text; }
        public void Special() => combat.TrySpecial();
        void Update()
        {
            int cd = Mathf.CeilToInt(combat.SpecialCooldownRemaining), hp = roster.Health.Current, ally = roster.Ally.Current, shield = Mathf.CeilToInt(roster.Health.ProtectionRemaining);
            if (cd == lastCooldown && hp == lastHp && ally == lastAlly && roster.Selected == lastSlot && shield == lastShield) return;
            lastCooldown = cd; lastHp = hp; lastAlly = ally; lastSlot = roster.Selected; lastShield = shield;
            status.text = $"{roster.Current.displayName}  VIDA {hp}/{roster.Health.Maximum}\n{roster.Current.special.displayName}: {(cd > 0 ? cd + "s" : "PRONTA")}  |  ALIADO {ally}/{roster.Ally.Maximum}  |  PROTECAO {shield}s";
        }
    }
}
