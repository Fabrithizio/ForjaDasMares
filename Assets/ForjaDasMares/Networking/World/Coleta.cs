using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForjaDasMares.Networking
{
    // Erva-luz: glowing herbs that grow on the fields around the Refúgio. Click (or F near it) to pick; it grows back.
    public static class Coleta
    {
        const int Quantas = 12;
        const float Muralha = 36;

        // A spot on the fields outside the walls: 6 to 26 m past the wall, away from the roads (east, west, south)
        // and away from the river in the south.
        public static bool ForaDaCidade(System.Random random, out Vector3 p)
        {
            float a = (float)random.NextDouble() * Mathf.PI * 2, d = Muralha + 6 + (float)random.NextDouble() * 20;
            p = new Vector3(Mathf.Sin(a) * d, 0, Mathf.Cos(a) * d);
            if (p.z < -20) return false;                          // the river side
            if (Mathf.Abs(p.z) < 6 || Mathf.Abs(p.x) < 6) return false; // the roads out of the gates
            return true;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded; Loaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); }
        static void Loaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != Historia.Refugio) return;
            new GameObject("ColetaDoRefugio").AddComponent<Plantador>();
        }

        sealed class Plantador : MonoBehaviour
        {
            IEnumerator Start()
            {
                yield return null; yield return null;
                // On the grass: next to the grass tufts and flowers that the city builder placed off the streets.
                // Outside the walls, on the fields around the town.
                var random = new System.Random(7);
                var used = new List<Vector3>();
                for (int i = 0; i < 400 && used.Count < Quantas; i++)
                {
                    if (!ForaDaCidade(random, out var p)) continue;
                    bool far = true; foreach (var u in used) if ((u - p).sqrMagnitude < 25) far = false;
                    if (!far) continue;
                    if (!UnityEngine.AI.NavMesh.SamplePosition(p, out var hit, 1.2f, UnityEngine.AI.NavMesh.AllAreas)) continue;
                    used.Add(hit.position);
                    var herb = new GameObject("ErvaLuz").AddComponent<PontoDeColeta>();
                    herb.transform.position = hit.position;
                }
            }
        }
    }

    public sealed class PontoDeColeta : MonoBehaviour
    {
        const float Regrow = 75;
        GameObject plant; WorldClickTarget target; float readyAt;
        static Material leaf;

        void Start()
        {
            plant = new GameObject("Planta"); plant.transform.SetParent(transform, false);
            if (leaf == null) { leaf = new Material(Shader.Find("Universal Render Pipeline/Lit")); leaf.SetColor("_BaseColor", new Color(.2f, .55f, .3f)); leaf.SetFloat("_Smoothness", .1f); }
            for (int i = 0; i < 5; i++)
            {
                var l = GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(l.GetComponent<Collider>());
                l.transform.SetParent(plant.transform, false);
                float a = i * 72 * Mathf.Deg2Rad;
                l.transform.localPosition = new Vector3(Mathf.Cos(a) * .16f, .12f, Mathf.Sin(a) * .16f);
                l.transform.localRotation = Quaternion.Euler(30, -i * 72, 0);
                l.transform.localScale = new Vector3(.12f, .04f, .32f);
                l.GetComponent<Renderer>().sharedMaterial = leaf;
            }
            // The glowing bud (natureza desperta).
            var bud = GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(bud.GetComponent<Collider>());
            bud.transform.SetParent(plant.transform, false); bud.transform.localPosition = Vector3.up * .32f; bud.transform.localScale = Vector3.one * .16f;
            var r = bud.GetComponent<Renderer>(); r.sharedMaterial = EfeitosDeHabilidade.Brilho;
            var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", new Color(.5f, 1f, .85f)); r.SetPropertyBlock(block);
            var glow = new GameObject("Luz").AddComponent<Light>(); glow.transform.SetParent(plant.transform, false); glow.transform.localPosition = Vector3.up * .4f;
            glow.type = LightType.Point; glow.color = new Color(.5f, 1f, .85f); glow.range = 2.2f; glow.intensity = .8f;
            EfeitosDeHabilidade.Aura(plant.transform, new Color(.5f, 1f, .85f), true, .25f, 4);
            // Click target.
            var click = gameObject.AddComponent<BoxCollider>(); click.size = new Vector3(.9f, .7f, .9f); click.center = Vector3.up * .3f; WorldClickTarget.Atravessavel(click);
            target = gameObject.AddComponent<WorldClickTarget>(); target.Configure("Colher Erva-luz", 2.2f);
            target.Used.AddListener(Colher);
            WorldClickTarget.Extras.Add(target);
        }
        void OnDestroy() { if (target != null) WorldClickTarget.Extras.Remove(target); }

        void Colher()
        {
            if (Time.time < readyAt) return;
            int amount = Random.value < .3f ? 2 : 1;
            if (!Recursos.Ganhar(Recursos.ErvaLuz, amount, transform.position)) return;
            EfeitosDeHabilidade.Coluna(transform.position, .5f, new Color(.5f, 1f, .85f));
            readyAt = Time.time + Regrow;
            plant.SetActive(false); target.enabled = false;
        }
        void Update()
        {
            if (plant != null && !plant.activeSelf && Time.time >= readyAt) { plant.SetActive(true); target.enabled = true; }
        }
    }
}
