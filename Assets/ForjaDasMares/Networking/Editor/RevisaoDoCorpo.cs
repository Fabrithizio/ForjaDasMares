using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ForjaDasMares.Networking.Editor
{
    // Body check of the two heroes: poses each one in several animations (frames chosen in the hardest moments),
    // takes pictures from the front and from the side and saves one sheet per body in Logs/RevisaoDoCorpo/.
    // Bad bones or bad skin weights show as twisted shoulders, sunken elbows, stretched hips.
    // Works in a temporary scene added on top; nothing in the project changes.
    public static class RevisaoDoCorpo
    {
        const string Animacoes = "Assets/ForjaDasMares/Art/Personagens/Animacoes/";
        static readonly string[] Clipes =
        {
            "", // no animation: the body as it was made (T-pose)
            "Parado", "Andar", "Correr_Rapido", "Ataque_Forte", "Ataque_Area", "Arremessar", "Conjurar",
            "Rolar", "Pular", "Dano", "Coletar", "Espada/Espada_Parado", "Espada/Espada_Correr", "Cajado/Cajado_Parado",
        };
        static readonly float[] Momentos = { .3f, .65f };
        const int Lado = 300;

        // Legs: the feet of the two bodies crossed the middle line while walking (and the knees looked bent in).
        // "Feet spacing" of the humanoid avatar keeps the feet apart in every animation. Then the pictures again.
        [MenuItem("Forja/Personagem/Arrumar as pernas (afastar os pés)")]
        public static void ArrumarPernas()
        {
            foreach (var genero in new[] { "Masculino", "Feminino" })
            {
                var caminho = "Assets/ForjaDasMares/Art/Personagens/" + genero + "/Modelo.fbx";
                if (!(AssetImporter.GetAtPath(caminho) is ModelImporter modelo)) continue;
                var humano = modelo.humanDescription; humano.feetSpacing = .12f; modelo.humanDescription = humano;
                modelo.SaveAndReimport();
            }
            Revisar();
        }

        [MenuItem("Forja/Personagem/Revisar o corpo (fotos)")]
        public static void Revisar()
        {
            var pasta = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs", "RevisaoDoCorpo");
            Directory.CreateDirectory(pasta);
            var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var relatorio = new System.Text.StringBuilder("REVISAO_CORPO\n");
            try
            {
                const int Camada = 31;
                var camGo = new GameObject("CameraRevisao"); SceneManager.MoveGameObjectToScene(camGo, cena);
                var cam = camGo.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 1.05f; cam.cullingMask = 1 << Camada;
                cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.32f, .34f, .37f); cam.nearClipPlane = .05f; cam.farClipPlane = 20;
                var luzGo = new GameObject("LuzRevisao"); SceneManager.MoveGameObjectToScene(luzGo, cena);
                var luz = luzGo.AddComponent<Light>(); luz.type = LightType.Directional; luz.intensity = 1.3f; luz.transform.rotation = Quaternion.Euler(35, 150, 0);
                var rt = new RenderTexture(Lado, Lado, 24) { antiAliasing = 4 }; cam.targetTexture = rt;
                var origem = new Vector3(0, -800, 0);

                foreach (var genero in new[] { "Masculino", "Feminino" })
                {
                    var prefab = Resources.Load<GameObject>("Personagens/" + genero);
                    if (prefab == null) { relatorio.AppendLine(genero + ": prefab não encontrado"); continue; }
                    var corpo = Object.Instantiate(prefab); corpo.name = "Revisao_" + genero; SceneManager.MoveGameObjectToScene(corpo, cena);
                    corpo.transform.position = origem; corpo.transform.rotation = Quaternion.identity;
                    foreach (var t in corpo.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = Camada;
                    foreach (var mb in corpo.GetComponentsInChildren<MonoBehaviour>(true)) mb.enabled = false;
                    foreach (var smr in corpo.GetComponentsInChildren<SkinnedMeshRenderer>(true)) smr.updateWhenOffscreen = true;
                    var animator = corpo.GetComponentInChildren<Animator>(true);
                    if (animator != null) { animator.enabled = true; animator.gameObject.SetActive(true); animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; }
                    var alvo = animator != null ? animator.gameObject : corpo;
                    relatorio.AppendLine(genero + ": avatar " + (animator != null && animator.avatar != null ? (animator.avatar.isHuman ? "humano ok" : "NAO humano") + (animator.avatar.isValid ? "" : " INVALIDO") : "sem avatar"));

                    int colunas = Momentos.Length * 2, linhas = Clipes.Length;
                    var folha = new Texture2D(colunas * Lado, linhas * Lado, TextureFormat.RGB24, false);
                    for (int l = 0; l < linhas; l++)
                    {
                        AnimationClip clip = null;
                        if (Clipes[l] != "")
                        {
                            var caminho = Animacoes + Clipes[l] + ".fbx";
                            clip = AssetDatabase.LoadAllAssetsAtPath(caminho).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));
                            if (clip == null) relatorio.AppendLine("  sem clipe: " + Clipes[l]);
                        }
                        for (int m = 0; m < Momentos.Length; m++)
                        {
                            PlayableGraph grafo = default;
                            if (clip != null && animator != null)
                            {
                                // The same way the game plays it (humanoid muscles, not only the root).
                                grafo = PlayableGraph.Create("Revisao");
                                var saida = AnimationPlayableOutput.Create(grafo, "Saida", animator);
                                var tocar = AnimationClipPlayable.Create(grafo, clip); tocar.SetApplyFootIK(true);
                                saida.SetSourcePlayable(tocar);
                                tocar.SetTime(clip.length * Momentos[m]); tocar.SetSpeed(0);
                                animator.applyRootMotion = false;
                                grafo.Evaluate();
                            }
                            // The body may have walked away from its spot inside the clip: aim at its hips.
                            var quadril = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Hips) : null;
                            var centro = quadril != null ? new Vector3(quadril.position.x, origem.y + .9f, quadril.position.z) : origem + Vector3.up * .9f;
                            // Skinned bodies are not re-skinned by a render in the editor: bake the pose into plain meshes.
                            var assados = new List<GameObject>(); var escondidos = new List<SkinnedMeshRenderer>();
                            foreach (var smr in corpo.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                            {
                                if (!smr.gameObject.activeInHierarchy || !smr.enabled) continue;
                                var malha = new Mesh(); smr.BakeMesh(malha, true);
                                var go = new GameObject("Assado"); SceneManager.MoveGameObjectToScene(go, cena); go.layer = Camada;
                                go.transform.SetPositionAndRotation(smr.transform.position, smr.transform.rotation);
                                go.AddComponent<MeshFilter>().sharedMesh = malha; go.AddComponent<MeshRenderer>().sharedMaterials = smr.sharedMaterials;
                                smr.enabled = false; escondidos.Add(smr); assados.Add(go);
                            }
                            for (int v = 0; v < 2; v++)
                            {
                                var dir = v == 0 ? Vector3.forward : Vector3.right; // front, then the left side
                                cam.transform.position = centro + dir * 4; cam.transform.rotation = Quaternion.LookRotation(-dir);
                                cam.Render();
                                RenderTexture.active = rt;
                                var foto = new Texture2D(Lado, Lado, TextureFormat.RGB24, false); foto.ReadPixels(new Rect(0, 0, Lado, Lado), 0, 0); foto.Apply();
                                RenderTexture.active = null;
                                folha.SetPixels((m * 2 + v) * Lado, (linhas - 1 - l) * Lado, Lado, Lado, foto.GetPixels());
                                Object.DestroyImmediate(foto);
                            }
                            if (grafo.IsValid()) grafo.Destroy();
                            foreach (var go in assados) { Object.DestroyImmediate(go.GetComponent<MeshFilter>().sharedMesh); Object.DestroyImmediate(go); }
                            foreach (var smr in escondidos) smr.enabled = true;
                        }
                    }
                    folha.Apply();
                    File.WriteAllBytes(Path.Combine(pasta, genero + ".png"), folha.EncodeToPNG());
                    Object.DestroyImmediate(folha);
                    Object.DestroyImmediate(corpo); // one body at a time in the pictures
                }
                cam.targetTexture = null; rt.Release();
                relatorio.AppendLine("Linhas: " + string.Join(" | ", Clipes.Select(c => c == "" ? "pose original" : c)));
                relatorio.AppendLine("Colunas: momento 30% frente, 30% lado, 65% frente, 65% lado");
            }
            finally
            {
                EditorSceneManager.CloseScene(cena, true);
                File.WriteAllText(Path.Combine(pasta, "relatorio.txt"), relatorio.ToString());
                Debug.Log(relatorio.ToString());
            }
        }
    }
}
