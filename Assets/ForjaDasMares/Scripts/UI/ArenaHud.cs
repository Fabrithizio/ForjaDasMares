using UnityEngine;
using UnityEngine.UI;

namespace ForjaDasMares
{
    public sealed class ArenaHud : MonoBehaviour
    {
        [SerializeField] Health trainingTarget;
        [SerializeField] Text label;
        [SerializeField] Image fill;
        [SerializeField] GameObject touchControls;
        [SerializeField] Text instructions;
        [SerializeField] bool showTouchInEditor;
        public void Configure(Health target, Text text, Image healthFill) { trainingTarget = target; label = text; fill = healthFill; }
        public void ConfigureTouch(GameObject panel, Text help) { touchControls = panel; instructions = help; }
        void Awake()
        {
            bool touch = Application.isMobilePlatform || showTouchInEditor;
            if (touchControls != null) touchControls.SetActive(touch);
            if (instructions != null) instructions.text = touch ? "ARRASTE PARA MOVER  |  ATAQUE / ESQUIVA  |  REINICIAR NO TOPO" : "WASD / SETAS: mover   MOUSE: mirar   CLIQUE: atacar   ESPAÇO: esquiva   R: reiniciar";
        }
        void OnEnable() { trainingTarget.Changed += Refresh; Refresh(); }
        void Start()
        {
            if (instructions != null) instructions.text = Application.isMobilePlatform || showTouchInEditor ? "SEGURE O JOYSTICK | ATAQUE / ESQUIVA / HABILIDADE | HEROI NO TOPO" : "WASD: mover | MOUSE: mirar/atacar | E: habilidade | ESPACO: esquiva | 1-4/TAB: heroi | R: reiniciar";
        }
        void OnDisable() => trainingTarget.Changed -= Refresh;
        void Refresh()
        {
            label.text = trainingTarget.Current > 0 ? $"ALVO DE TREINO  {trainingTarget.Current}/{trainingTarget.Maximum}" : "ALVO DERROTADO — R PARA REINICIAR";
            fill.fillAmount = (float)trainingTarget.Current / trainingTarget.Maximum;
        }
    }
}
