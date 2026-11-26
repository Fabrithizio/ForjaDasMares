using UnityEngine;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // The enemy you are focused on, at the top centre of the screen (like Drakensang): name, level, kind and
    // life. Nothing about enemies floats over their heads any more, so the fight stays clear.
    // Focus, in this order: the enemy under the mouse, the enemy the hero is locked on, the last enemy the hero
    // hit (for 6 s; this is the phone's case).
    public sealed class AlvoNoTopo : MonoBehaviour
    {
        const float Largura = 440, Barra = 412;
        RegionalWorld world; ArenaInput input; Health heroi;
        CanvasGroup grupo; Text nome, tipo, numeros; RectTransform fill, rastro; Image fillImg;
        Health ultimoAcertado; float quandoAcertei = -99;
        Health mostrando; float rastroValor = 1, flash;

        public static AlvoNoTopo Criar(RegionalWorld world, RectTransform root)
        {
            var box = RusticHudTheme.Rect(root, "AlvoNoTopo", new Vector2(.5f, 1), new Vector2(0, -98), new Vector2(Largura, 62));
            var a = box.gameObject.AddComponent<AlvoNoTopo>(); a.world = world; a.Montar(box); return a;
        }

        void Montar(RectTransform box)
        {
            grupo = gameObject.AddComponent<CanvasGroup>(); grupo.alpha = 0; grupo.blocksRaycasts = false; grupo.interactable = false;
            RusticHudTheme.Panel(box, RusticHudTheme.Ink, 12);
            nome = RusticHudTheme.Label(box, "Nome", new Vector2(0, 1), new Vector2(14, -6), new Vector2(300, 24), "", 17, TextAnchor.MiddleLeft); nome.color = RusticHudTheme.Gold;
            tipo = RusticHudTheme.Label(box, "Tipo", new Vector2(1, 1), new Vector2(-14, -6), new Vector2(120, 24), "", 13, TextAnchor.MiddleRight); tipo.color = RusticHudTheme.Muted;
            var track = RusticHudTheme.Rect(box, "Trilho", new Vector2(.5f, 0), new Vector2(0, 10), new Vector2(Barra, 18));
            track.gameObject.AddComponent<Image>().color = new Color(.08f, .02f, .03f, .95f);
            rastro = Faixa(track, "Rastro", new Color(1f, .9f, .75f, .85f));
            fill = Faixa(track, "Vida", new Color(.78f, .12f, .16f)); fillImg = fill.GetComponent<Image>();
            numeros = RusticHudTheme.Label(track, "Numeros", new Vector2(.5f, .5f), Vector2.zero, new Vector2(Barra, 18), "", 13, TextAnchor.MiddleCenter);
            numeros.color = Color.white; var sombra = numeros.gameObject.AddComponent<Shadow>(); sombra.effectColor = new Color(0, 0, 0, .8f);
        }

        static RectTransform Faixa(RectTransform track, string name, Color c)
        {
            var r = RusticHudTheme.Rect(track, name, new Vector2(0, .5f), Vector2.zero, new Vector2(Barra, 18));
            r.pivot = new Vector2(0, .5f); r.anchoredPosition = Vector2.zero; r.gameObject.AddComponent<Image>().color = c; return r;
        }

        void Update()
        {
            if (world == null || world.Hero == null) return;
            if (input == null) input = world.Hero.GetComponent<ArenaInput>();
            if (heroi == null)
            {
                heroi = world.Hero.GetComponent<Health>();
                if (heroi != null) heroi.DealtHit += h => { if (h != null && h.Faction == CombatFaction.Enemy) { ultimoAcertado = h; quandoAcertei = Time.time; } };
            }
            var alvo = Escolher();
            if (alvo != mostrando) { mostrando = alvo; if (alvo != null) { rastroValor = Fracao(alvo); Preencher(alvo); } }
            grupo.alpha = Mathf.MoveTowards(grupo.alpha, alvo != null ? 1 : 0, Time.unscaledDeltaTime * 6);
            if (mostrando == null) return;
            float f = Fracao(mostrando);
            if (f < rastroValor - .001f && flash <= 0) flash = .12f;
            rastroValor = Mathf.MoveTowards(rastroValor, f, Time.unscaledDeltaTime * (rastroValor > f + .25f ? .9f : .45f));
            if (rastroValor < f) rastroValor = f;
            fill.sizeDelta = new Vector2(Barra * f, 18); rastro.sizeDelta = new Vector2(Barra * rastroValor, 18);
            flash = Mathf.Max(0, flash - Time.unscaledDeltaTime);
            fillImg.color = Color.Lerp(new Color(.78f, .12f, .16f), new Color(1f, .75f, .7f), flash / .12f);
            numeros.text = mostrando.Current + " / " + mostrando.Maximum;
        }

        Health Escolher()
        {
            var desk = input != null ? input.Desktop : null;
            if (desk != null)
            {
                if (Vivo(desk.SelectionTarget)) return desk.SelectionTarget;
                if (Vivo(desk.Travado)) return desk.Travado;
            }
            if (Vivo(ultimoAcertado) && Time.time - quandoAcertei < 6) return ultimoAcertado;
            return null;
        }
        static bool Vivo(Health h) => h != null && h.isActiveAndEnabled && h.IsAlive && h.Faction == CombatFaction.Enemy;
        static float Fracao(Health h) => Mathf.Clamp01(h.Current / (float)Mathf.Max(1, h.Maximum));

        void Preencher(Health h)
        {
            var actor = h.GetComponent<EnemyActor>();
            if (actor != null && actor.Profile != null)
            {
                var p = actor.Profile;
                nome.text = p.displayName + (p.level > 0 ? "  <size=13><color=#A3BFCC>Nv " + p.level + "</color></size>" : "");
                tipo.text = p.kind == EnemyKind.Boss ? "CHEFE" : p.miniboss ? "ELITE" : "";
                tipo.color = p.kind == EnemyKind.Boss ? new Color(1f, .55f, .35f) : RusticHudTheme.Gold;
                return;
            }
            var bicho = h.GetComponent<AnimalSelvagem>();
            nome.text = bicho != null ? bicho.Nome : h.gameObject.name;
            tipo.text = bicho != null ? "ANIMAL" : ""; tipo.color = RusticHudTheme.Muted;
        }
    }
}
