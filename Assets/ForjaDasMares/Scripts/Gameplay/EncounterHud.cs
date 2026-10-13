using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares
{
    public sealed class EncounterHud : MonoBehaviour
    {
        [SerializeField] EncounterController encounter;
        [SerializeField] Text status, bossText;
        [SerializeField] Image bossFill;
        string previousStatus, previousBoss;
        float refresh;
        public void Configure(EncounterController controller, Text label, Text bossLabel, Image fill) { encounter = controller; status = label; bossText = bossLabel; bossFill = fill; }
        void Update()
        {
            refresh -= Time.unscaledDeltaTime; if (refresh > 0) return; refresh = 0.1f;
            string value = encounter.State == EncounterState.Ready ? "Encontro: " + encounter.Selected.displayName : encounter.State == EncounterState.Won ? "VITORIA! Selos de teste: " + encounter.Seals : encounter.State == EncounterState.Failed ? "DERROTA - R / Reiniciar" : "Onda " + encounter.WaveNumber + " | Inimigos: " + encounter.Enemies.ActiveCount;
            if (value != previousStatus) { status.text = value; previousStatus = value; }
            var boss = encounter.Boss; bool show = boss != null && boss.IsAlive;
            bossFill.transform.parent.gameObject.SetActive(show);
            value = show ? boss.Profile.displayName + " | " + boss.Health.Current + "/" + boss.Health.Maximum + " | Fase " + boss.Brain.BossPhase : "";
            if (previousBoss != value) { bossText.text = value; previousBoss = value; }
            if (show) bossFill.fillAmount = (float)boss.Health.Current / boss.Health.Maximum;
        }
    }
}
