using UnityEngine;
using UnityEngine.UI;
namespace ForjaDasMares
{
    public sealed class DungeonHud : MonoBehaviour
    {
        [SerializeField] DungeonSession session;
        [SerializeField] Text title, hero, status, interaction, bossText;
        [SerializeField] Image bossFill;
        float timer;
        float fullWidth;
        void Awake() => fullWidth = HealthBarGeometry.Initialize(bossFill);
        void Start()
        {
            gameObject.AddComponent<RusticDungeonHud>().Configure(session, GetComponent<Canvas>(), GetComponent<ProgressionHud>());
            enabled = false;
        }
        static readonly string[] RoomNames = { "Entrada", "Combate", "Combate dificil", "Tesouro", "Descanso", "Evento", "Armadilha", "Chefe" };
        public void Configure(DungeonSession game, Text heading, Text health, Text progress, Text prompt, Text bossLabel, Image fill)
        { session = game; title = heading; hero = health; status = progress; interaction = prompt; bossText = bossLabel; bossFill = fill; }
        void Update()
        {
            timer -= Time.unscaledDeltaTime; if (timer > 0 || session.CurrentRoom == null) return; timer = 0.1f;
            title.text = session.Biome.displayName + " | seed " + session.Seed + (session.FixedMap ? " | FIXO" : " | PROCEDURAL");
            hero.text = session.Heroes.Current.displayName + "  VIDA " + session.Heroes.Health.Current + "/" + session.Heroes.Health.Maximum;
            status.text = session.Finished ? "MASMORRA CONCLUIDA! Moedas " + session.VisibleCoins : !session.Heroes.Health.IsAlive ? "DERROTA - REINICIAR / R" : "Sala " + (session.CurrentRoom.node.id + 1) + " " + RoomNames[(int)session.CurrentRoom.node.category] + " | " + (session.CurrentRoom.completed ? "Concluida" : "Em andamento") + " | Moedas " + session.VisibleCoins + " | Chave " + (session.HasKey ? "SIM" : "NAO");
            interaction.text = session.Nearby != null ? session.Nearby.Label + " | F / INTERAGIR" : "";
            var boss = session.Encounter.Boss; bool alive = boss != null && boss.IsAlive; bossFill.transform.parent.gameObject.SetActive(alive);
            if (alive) { HealthBarGeometry.Set(bossFill, boss.Health.Current, boss.Health.Maximum, fullWidth); bossText.text = boss.Profile.displayName + " | " + boss.Health.Current + "/" + boss.Health.Maximum + " | Fase " + boss.Brain.BossPhase; }
        }
    }
}
