using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForjaDasMares.Networking
{
    // Makes the water of a river flow (moves its texture slowly).
    // It is not saved in the scenes: when a scene opens, the water pieces are found by their names and get it.
    public sealed class RioAnimado : MonoBehaviour
    {
        public Vector2 velocidade = new Vector2(.12f, .015f);
        Renderer view; Material own;
        void Start() { view = GetComponent<Renderer>(); if (view != null) own = view.material; }
        void Update() { if (own != null) own.SetTextureOffset("_BaseMap", velocidade * Time.time); }
        void OnDestroy() { if (own != null) Destroy(own); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded; Loaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); }
        static void Loaded(Scene scene, LoadSceneMode mode)
        {
            bool nascente = scene.name.Contains("Nascente");
            foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                Vector2? flow = r.name switch
                {
                    "Rio" => nascente ? new Vector2(-.12f, .01f) : new Vector2(.10f, .01f),
                    "RioFundo" => new Vector2(.16f, -.01f),
                    "PocoDaNascente" => new Vector2(.02f, .03f),
                    "Cachoeira" => new Vector2(0, .9f),
                    _ => (Vector2?)null,
                };
                if (flow == null || r.GetComponent<RioAnimado>() != null) continue;
                r.gameObject.AddComponent<RioAnimado>().velocidade = flow.Value;
            }
        }
    }
}
