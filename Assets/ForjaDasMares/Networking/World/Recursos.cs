using UnityEngine;

namespace ForjaDasMares.Networking
{
    // Resources of the world (stack in the profile: ProfileService.Recurso / GanharRecurso / Trocar).
    public static class Recursos
    {
        public const string ErvaLuz = "erva_luz", Couro = "couro", Carne = "carne", Chifre = "chifre", PocaoCura = "pocao_cura";
        public static string Nome(string id) =>
            id == ErvaLuz ? "Erva-luz" : id == Couro ? "Couro" : id == Carne ? "Carne" : id == Chifre ? "Chifre de cervo" : id == PocaoCura ? "Poção de cura" : id;
        public static readonly string[] Todos = { ErvaLuz, Couro, Carne, Chifre, PocaoCura };
        public static Color Cor(string id) =>
            id == ErvaLuz ? new Color(.5f, 1f, .8f) : id == PocaoCura ? new Color(1f, .4f, .45f) : id == Chifre ? new Color(.95f, .9f, .75f) : new Color(.85f, .65f, .45f);

        // The profile of the local player in this scene.
        public static ProfileService Servico
        {
            get
            {
                if (cached == null) cached = Object.FindFirstObjectByType<RegionalInventory>();
                return cached != null ? cached.Service : null;
            }
        }
        static RegionalInventory cached;
        public static int Quantos(string id) => Servico?.Recurso(id) ?? 0;
        public static bool Ganhar(string id, int amount, Vector3 at)
        {
            var s = Servico; if (s == null) return false;
            bool ok = s.GanharRecurso(id, amount, "+" + amount + " " + Nome(id));
            if (ok) TextoFlutuante.Mostrar(at + Vector3.up * 1.6f, "+" + amount + " " + Nome(id), Cor(id));
            return ok;
        }
    }

    // A short text that rises and fades in the world ("+1 Erva-luz").
    public sealed class TextoFlutuante : MonoBehaviour
    {
        TextMesh text; float t; Color color;
        public static void Mostrar(Vector3 at, string message, Color c)
        {
            var go = new GameObject("Texto");
            go.transform.position = at;
            var t = go.AddComponent<TextoFlutuante>();
            t.text = go.AddComponent<TextMesh>(); t.text.text = message; t.text.characterSize = .09f; t.text.fontSize = 48;
            t.text.anchor = TextAnchor.MiddleCenter; t.text.alignment = TextAlignment.Center; t.text.color = c; t.color = c;
        }
        void Update()
        {
            t += Time.deltaTime;
            transform.position += Vector3.up * Time.deltaTime * .8f;
            var cam = Camera.main; if (cam != null) transform.rotation = cam.transform.rotation;
            text.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(1.6f - t));
            if (t > 1.6f) Destroy(gameObject);
        }
    }
}

namespace ForjaDasMares.Networking
{
    // A short line at the top of the screen (a thought of the hero, a discovery...), in any scene.
    public sealed class AvisoNaTela : MonoBehaviour
    {
        public static void Mostrar(string text, float seconds) => new GameObject("Aviso").AddComponent<AvisoNaTela>().Begin(text, seconds);
        void Begin(string text, float seconds) => StartCoroutine(Run(text, seconds));
        System.Collections.IEnumerator Run(string text, float seconds)
        {
            yield return new WaitForSeconds(1.2f);
            var go = new GameObject("AvisoUI", typeof(RectTransform)); go.transform.SetParent(transform, false);
            var c = go.AddComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 30000;
            var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var box = RusticHudTheme.Rect((RectTransform)go.transform, "Caixa", new Vector2(.5f, 1), new Vector2(0, -190), new Vector2(900, 64));
            RusticHudTheme.Panel(box, new Color(.03f, .06f, .07f, .88f), 14);
            var label = RusticHudTheme.Label(box, "Texto", new Vector2(.5f, .5f), Vector2.zero, new Vector2(860, 56), text, 24, TextAnchor.MiddleCenter);
            label.color = RusticHudTheme.Gold; label.fontStyle = FontStyle.Italic;
            var group = box.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false;
            for (float t = 0; t < .6f; t += Time.unscaledDeltaTime) { group.alpha = t / .6f; yield return null; }
            group.alpha = 1; yield return new WaitForSecondsRealtime(seconds);
            for (float t = 0; t < .6f; t += Time.unscaledDeltaTime) { group.alpha = 1 - t / .6f; yield return null; }
            Destroy(gameObject);
        }
    }
}
