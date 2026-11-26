using System;
using UnityEngine;
using UnityEngine.UI;

namespace ForjaDasMares.Networking
{
    // The Super button when the bar is full (Fabricio, 26/11): it pops when it fills, then keeps breathing,
    // shivers every so often and throws embers up like a little fire, so the player feels it is ready.
    public sealed class SuperPronto : MonoBehaviour
    {
        Func<bool> pronto; bool estava; float pop, tremor, brasa, proximoTremor;
        readonly Brasa[] brasas = new Brasa[24]; int proxima;
        static readonly Color Ouro = new Color(1f, .78f, .3f), Fogo = new Color(1f, .45f, .12f);

        sealed class Brasa { public RectTransform rt; public Image img; public Vector2 vel; public float vida, idade; }

        public static SuperPronto Em(Button botao, Func<bool> pronto)
        {
            var s = botao.gameObject.GetComponent<SuperPronto>() ?? botao.gameObject.AddComponent<SuperPronto>();
            s.pronto = pronto; return s;
        }

        void Update()
        {
            bool agora = pronto != null && pronto();
            float dt = Time.unscaledDeltaTime;
            if (agora && !estava) { pop = 1; for (int i = 0; i < 14; i++) Soltar(true); proximoTremor = .6f; }
            estava = agora;
            pop = Mathf.MoveTowards(pop, 0, dt * 3.5f);
            float escala = 1, giro = 0;
            if (agora)
            {
                escala = 1 + .05f * Mathf.Sin(Time.unscaledTime * 5f) + .25f * pop * pop;
                if ((proximoTremor -= dt) <= 0) { tremor = .35f; proximoTremor = 1.6f; }
                if ((brasa -= dt) <= 0) { brasa = .1f; Soltar(false); }
            }
            tremor = Mathf.Max(0, tremor - dt);
            if (tremor > 0) giro = Mathf.Sin(Time.unscaledTime * 55) * 7 * (tremor / .35f);
            transform.localScale = Vector3.one * escala;
            transform.localRotation = Quaternion.Euler(0, 0, giro);
            foreach (var b in brasas)
            {
                if (b == null || !b.rt.gameObject.activeSelf) continue;
                b.idade += dt; float k = b.idade / b.vida;
                if (k >= 1) { b.rt.gameObject.SetActive(false); continue; }
                b.vel += new Vector2(Mathf.Sin(b.idade * 9 + b.vida * 20) * 40, 30) * dt;
                b.rt.anchoredPosition += b.vel * dt;
                b.rt.localScale = Vector3.one * (1 - k * .7f);
                var c = Color.Lerp(Ouro, Fogo, k); c.a = 1 - k * k; b.img.color = c;
            }
        }

        // An ember: a tiny gold diamond flying up from the button (a burst flies out to every side).
        void Soltar(bool explosao)
        {
            var b = brasas[proxima] ??= Nova(); proxima = (proxima + 1) % brasas.Length;
            var area = (RectTransform)transform; float w = area.rect.width * .5f;
            b.rt.gameObject.SetActive(true); b.idade = 0;
            if (explosao)
            {
                float a = UnityEngine.Random.value * Mathf.PI * 2;
                b.rt.anchoredPosition = Vector2.zero; b.vel = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * UnityEngine.Random.Range(90, 170); b.vida = UnityEngine.Random.Range(.4f, .7f);
            }
            else
            {
                b.rt.anchoredPosition = new Vector2(UnityEngine.Random.Range(-w, w) * .7f, w * .3f);
                b.vel = new Vector2(UnityEngine.Random.Range(-12f, 12f), UnityEngine.Random.Range(35f, 70f)); b.vida = UnityEngine.Random.Range(.6f, 1f);
            }
            float size = UnityEngine.Random.Range(4f, 7f); b.rt.sizeDelta = new Vector2(size, size);
            b.rt.SetAsLastSibling();
        }

        Brasa Nova()
        {
            var go = new GameObject("Brasa", typeof(RectTransform), typeof(Image)); go.transform.SetParent(transform, false);
            var img = go.GetComponent<Image>(); img.raycastTarget = false;
            var rt = (RectTransform)go.transform; rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f); rt.localRotation = Quaternion.Euler(0, 0, 45);
            go.SetActive(false);
            return new Brasa { rt = rt, img = img };
        }

        void OnDisable() { transform.localScale = Vector3.one; transform.localRotation = Quaternion.identity; }
    }
}
