using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // Healing potion: key 4 (PC) or a small button that only shows while you carry potions.
    // Heals 40 at once; 10 s between potions.
    public sealed class Pocoes : MonoBehaviour
    {
        public const int Cura = 40; public const float Espera = 10;
        RegionalWorld world; ArenaInput input; float readyAt, lookAt;
        Button button; Text label, count; RectTransform shade;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded; Loaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); }
        static void Loaded(Scene scene, LoadSceneMode mode) { if (FindFirstObjectByType<Pocoes>() == null) new GameObject("Pocoes").AddComponent<Pocoes>(); }

        void Update()
        {
            if (world == null) { if (Time.unscaledTime < lookAt) return; lookAt = Time.unscaledTime + .5f; world = FindFirstObjectByType<RegionalWorld>(); if (world == null || world.Hero == null) { world = null; return; } input = world.Hero.GetComponent<ArenaInput>(); }
            if (button == null) { if (Time.unscaledTime < lookAt) return; lookAt = Time.unscaledTime + .5f; if (!Build()) return; }
            int count = Recursos.Quantos(Recursos.PocaoCura);
            bool blocked = input != null && input.CombatBlocked;
            button.gameObject.SetActive(count > 0 && !blocked);
            float wait = Mathf.Max(0, readyAt - Time.time);
            label.text = wait > 0 ? Mathf.CeilToInt(wait).ToString() : "";
            this.count.text = count.ToString();
            shade.sizeDelta = new Vector2(-6, (((RectTransform)button.transform).sizeDelta.y - 6) * Mathf.Clamp01(wait / Espera));
            if (!blocked && Keyboard.current != null && Keyboard.current.digit4Key.wasPressedThisFrame) Beber();
        }

        public void Beber()
        {
            if (world == null || Time.time < readyAt) return;
            var hp = world.Hero.Health;
            if (!hp.IsAlive || hp.Current >= hp.Maximum) return;
            var s = Recursos.Servico; if (s == null || !s.GastarRecurso(Recursos.PocaoCura, 1, "Poção de cura usada.")) return;
            hp.Heal(Cura); readyAt = Time.time + Espera;
            Descobertas.Marcar(Descobertas.PocaoBebida);
            EfeitosDeHabilidade.Coluna(world.Hero.transform.position, .9f, new Color(1f, .4f, .45f));
            TextoFlutuante.Mostrar(world.Hero.transform.position + Vector3.up * 2, "+" + Cura, new Color(.5f, 1f, .5f));
        }

        // A small pocket of its own, with the potion icon and how many you carry.
        // PC: inside the combat bar, at its right end (the bar keeps 56 px for it). Phone: above the attack button, right of the Super.
        bool Build()
        {
            bool touch = Application.isMobilePlatform;
            var parent = GameObject.Find(touch ? "TouchSafeArea" : "CombatDock");
            if (parent == null) return false;
            float size = touch ? 50 : 42;
            button = touch
                ? RusticHudTheme.Button((RectTransform)parent.transform, "Pocao", new Vector2(1, 0), new Vector2(-4, 176), Vector2.one * size, "", Beber)
                : RusticHudTheme.Button((RectTransform)parent.transform, "Pocao", new Vector2(1, 0), new Vector2(-12, 23), Vector2.one * size, "", Beber);
            var g = button.GetComponent<RoundedHudGraphic>();
            g.radius = touch ? size / 2 : 9; g.border = 2; g.color = new Color(.16f, .03f, .05f, .97f); g.topColor = new Color(.34f, .07f, .1f, .97f);
            g.borderColor = Color.Lerp(RusticHudTheme.Gold, new Color(1f, .45f, .5f), .45f);
            // The icon (Resources/Icones/Itens/pocao_cura.png, game-icons.net); a red drop if it is missing.
            var tex = Resources.Load<Texture2D>("Icones/Itens/pocao_cura");
            var art = RusticHudTheme.Rect(button.transform, "Icone", new Vector2(.5f, .5f), new Vector2(0, 1), Vector2.one * (size * .62f));
            if (tex != null) { var raw = art.gameObject.AddComponent<RawImage>(); raw.texture = tex; raw.color = new Color(1f, .62f, .66f); raw.raycastTarget = false; }
            else { var d = art.gameObject.AddComponent<RoundedHudGraphic>(); d.color = new Color(1f, .3f, .35f); d.radius = size * .2f; d.border = 0; d.raycastTarget = false; }
            shade = RusticHudTheme.Rect(button.transform, "Recarga", new Vector2(.5f, 0), new Vector2(0, 3), Vector2.zero);
            shade.anchorMin = Vector2.zero; shade.anchorMax = new Vector2(1, 0); shade.pivot = new Vector2(.5f, 0);
            var sg = shade.gameObject.AddComponent<RoundedHudGraphic>(); sg.color = new Color(0, 0, 0, .62f); sg.border = 0; sg.radius = touch ? size / 2 - 3 : 7; sg.raycastTarget = false;
            label = button.GetComponentInChildren<Text>(); label.fontSize = 18; label.fontStyle = FontStyle.Bold; label.alignment = TextAnchor.MiddleCenter;
            label.transform.SetAsLastSibling();
            // How many: a little badge on the lower right.
            var badge = RusticHudTheme.Rect(button.transform, "Quantas", new Vector2(1, 0), new Vector2(1, 1), new Vector2(17, 15));
            var bg = badge.gameObject.AddComponent<RoundedHudGraphic>(); bg.color = new Color(0, 0, 0, .7f); bg.border = 0; bg.radius = 6; bg.raycastTarget = false;
            count = RusticHudTheme.Label(badge, "N", new Vector2(.5f, .5f), Vector2.zero, new Vector2(17, 15), "", 11, TextAnchor.MiddleCenter);
            count.color = RusticHudTheme.Parchment; count.fontStyle = FontStyle.Bold; count.raycastTarget = false;
            // The key, on PC only.
            if (!touch)
            {
                var key = RusticHudTheme.Label(button.transform, "Tecla", new Vector2(0, 1), new Vector2(2, -1), new Vector2(12, 13), "4", 10, TextAnchor.MiddleCenter);
                key.color = RusticHudTheme.Gold; key.fontStyle = FontStyle.Bold; key.raycastTarget = false;
            }
            return true;
        }
    }
}
