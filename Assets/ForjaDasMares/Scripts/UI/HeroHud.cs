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
        RectTransform powerFill;
        Text powerText;
        Button superButton;
        void Start()
        {
            // The dungeon has its own compact HUD. The preserved test arena also
            // needs a functional super control on touch devices.
            if(!combat.EnhancedControls||GetComponent<DungeonHud>()!=null)return;
            var card=RusticHudTheme.Rect(transform,"ArenaPowerCard",new Vector2(.5f,0),new Vector2(0,20),new Vector2(360,70));RusticHudTheme.Panel(card,RusticHudTheme.Ink);
            powerText=RusticHudTheme.Label(card,"PowerText",new Vector2(0,1),new Vector2(12,-7),new Vector2(240,35),"",13);
            var track=RusticHudTheme.Rect(card,"SuperTrack",new Vector2(0,0),new Vector2(12,12),new Vector2(240,8));RusticHudTheme.Panel(track,RusticHudTheme.Surface,4);
            powerFill=RusticHudTheme.Rect(track,"SuperFill",Vector2.zero,Vector2.zero,new Vector2(240,8));RusticHudTheme.Panel(powerFill,RusticHudTheme.Gold,4);
            var input=roster.GetComponent<ArenaInput>();superButton=RusticHudTheme.Button(card,"SuperAttack",new Vector2(1,0),new Vector2(-10,10),new Vector2(86,50),Application.isMobilePlatform?"SUPER":"SUPER / Q",()=>input.RequestSuper());
            var progression=GetComponent<ProgressionHud>();if(progression!=null)progression.RegisterHitArea(card);
        }
        public void Configure(HeroRoster heroes, HeroCombat attacks, Text text) { roster = heroes; combat = attacks; status = text; }
        public void Special() => combat.TrySpecial();
        void Update()
        {
            if(powerFill!=null)
            {
                powerFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,240*combat.SuperProgress);
                powerText.text=combat.SuperReady?"SUPER PRONTO!":combat.IsHeavy?"GOLPE PESADO!":"SUPER "+Mathf.RoundToInt(combat.SuperProgress*100)+"%  ·  Segure ataque";
                superButton.interactable=combat.SuperReady&&roster.Health.IsAlive;
            }
            int cd = Mathf.CeilToInt(combat.SpecialCooldownRemaining), hp = roster.Health.Current, ally = roster.Ally.Current, shield = Mathf.CeilToInt(roster.Health.ProtectionRemaining);
            if (cd == lastCooldown && hp == lastHp && ally == lastAlly && roster.Selected == lastSlot && shield == lastShield) return;
            lastCooldown = cd; lastHp = hp; lastAlly = ally; lastSlot = roster.Selected; lastShield = shield;
            status.text = $"{roster.Current.displayName}  VIDA {hp}/{roster.Health.Maximum}\n{roster.Current.special.displayName}: {(cd > 0 ? cd + "s" : "PRONTA")}  |  ALIADO {ally}/{roster.Ally.Maximum}  |  PROTECAO {shield}s";
        }
    }
}
