using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // Hold a button of the bar (finger or mouse) to read what it does. The card opens above the button, away from
    // the finger, and closes when you let go; letting go after reading does NOT use the button.
    // On the PC, resting the mouse over the button also shows it (and then a click still uses it).
    public sealed class SegurarParaVer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        const float Segurar = .45f, Pairar = .7f;
        Func<string> texto; PointerEventData apertando; float desde = -1, sobre = -1; bool mostrando;
        static RectTransform cartao; static Text cartaoTexto; static SegurarParaVer dono;

        public static void Em(Component botao, Func<string> texto)
        {
            if (botao == null) return;
            var s = botao.gameObject.AddComponent<SegurarParaVer>(); s.texto = texto;
        }

        public void OnPointerDown(PointerEventData data) { apertando = data; desde = Time.unscaledTime; }
        public void OnPointerUp(PointerEventData data)
        {
            // Read it: the button is not used.
            if (mostrando && apertando != null) data.eligibleForClick = false;
            apertando = null; desde = -1; Esconder();
        }
        public void OnPointerEnter(PointerEventData data) { if (!UnityEngine.Device.Application.isMobilePlatform) sobre = Time.unscaledTime; }
        public void OnPointerExit(PointerEventData data) { sobre = -1; if (apertando == null) Esconder(); }
        void OnDisable() { apertando = null; desde = sobre = -1; Esconder(); }

        void Update()
        {
            if (mostrando) return;
            bool segurou = apertando != null && desde >= 0 && Time.unscaledTime - desde >= Segurar;
            bool pairou = apertando == null && sobre >= 0 && Time.unscaledTime - sobre >= Pairar;
            if (!segurou && !pairou) return;
            if (segurou) apertando.eligibleForClick = false;
            Mostrar(segurou);
        }

        void Mostrar(bool comDedo)
        {
            var canvas = GetComponentInParent<Canvas>()?.rootCanvas; if (canvas == null) return;
            var raiz = (RectTransform)canvas.transform;
            if (cartao == null || cartao.parent != raiz)
            {
                if (cartao != null) Destroy(cartao.gameObject);
                cartao = RusticHudTheme.Rect(raiz, "DicaDoBotao", Vector2.one * .5f, Vector2.zero, new Vector2(380, 150));
                var painel = RusticHudTheme.Panel(cartao, RusticHudTheme.Ink, 14); painel.borderColor = RusticHudTheme.Gold; painel.raycastTarget = false;
                cartaoTexto = RusticHudTheme.Label(cartao, "Texto", Vector2.one * .5f, Vector2.zero, new Vector2(352, 130), "", 17, TextAnchor.UpperLeft);
                cartaoTexto.horizontalOverflow = HorizontalWrapMode.Wrap; cartaoTexto.verticalOverflow = VerticalWrapMode.Overflow; cartaoTexto.raycastTarget = false;
            }
            cartaoTexto.text = texto != null ? texto() : "";
            float altura = Mathf.Max(80, cartaoTexto.preferredHeight + 26);
            cartao.sizeDelta = new Vector2(380, altura); ((RectTransform)cartaoTexto.transform).sizeDelta = new Vector2(352, altura - 20);
            cartao.SetAsLastSibling(); cartao.gameObject.SetActive(true);

            // Above the button: high enough that a thumb on the button does not cover it; then kept on the screen.
            var botao = (RectTransform)transform;
            var cantos = new Vector3[4]; botao.GetWorldCorners(cantos);
            Vector2 topo = raiz.InverseTransformPoint((cantos[1] + cantos[2]) * .5f);
            float acima = comDedo && UnityEngine.Device.Application.isMobilePlatform ? 110 : 24;
            var pos = topo + new Vector2(0, acima + altura * .5f);
            var meio = raiz.rect.size * .5f;
            pos.x = Mathf.Clamp(pos.x, -meio.x + 200, meio.x - 200);
            if (pos.y + altura * .5f > meio.y - 10)
            {
                // No room above: beside it, toward the middle of the screen.
                Vector2 centro = raiz.InverseTransformPoint((cantos[0] + cantos[2]) * .5f);
                pos = new Vector2(centro.x + (centro.x > 0 ? -1 : 1) * (botao.rect.width * .5f + (comDedo ? 140 : 40) + 190), centro.y);
            }
            pos.y = Mathf.Clamp(pos.y, -meio.y + altura * .5f + 10, meio.y - altura * .5f - 10);
            cartao.anchoredPosition = pos;
            mostrando = true; dono = this;
        }

        void Esconder()
        {
            if (!mostrando) return;
            mostrando = false;
            if (dono == this && cartao != null) cartao.gameObject.SetActive(false);
        }
    }
}
