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
        public static void Revisar() => Fotografar(Clipes, Momentos, new[] { "Masculino", "Feminino" }, "");

        // New moves (Animacoes/Espada/Golpes): imported as Humanoid and pictured on the male body, 5 moments each,
        // so we can choose which ones become the combo, the charged blow and the skills.
        [MenuItem("Forja/Personagem/Ver golpes novos da espada (fotos)")]
        public static void VerGolpes() => VerGolpesDe("Espada/Golpes/", "Golpes_");
        [MenuItem("Forja/Personagem/Ver golpes da arma pesada (fotos)")]
        public static void VerGolpesPesada() => VerGolpesDe("Pesada/Golpes/", "Pesada_");
        public static void VerGolpesDe(string sub, string prefixo)
        {
            string pasta = Animacoes + sub;
            var nomes = new List<string>();
            foreach (var arquivo in Directory.GetFiles(pasta, "*.fbx").OrderBy(f => f))
            {
                var caminho = arquivo.Replace('\\', '/');
                if (AssetImporter.GetAtPath(caminho) is ModelImporter imp && (imp.animationType != ModelImporterAnimationType.Human || imp.materialImportMode != ModelImporterMaterialImportMode.None))
                {
                    imp.animationType = ModelImporterAnimationType.Human; imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    imp.importAnimation = true; imp.materialImportMode = ModelImporterMaterialImportMode.None; imp.importCameras = imp.importLights = false;
                    imp.SaveAndReimport();
                }
                nomes.Add(sub + Path.GetFileNameWithoutExtension(arquivo));
            }
            Fotografar(nomes.ToArray(), new[] { .15f, .35f, .5f, .65f, .85f }, new[] { "Masculino" }, prefixo);
        }

        // Where the right hand goes in each blow, to tell side slashes from blows that come from above:
        // horizontal = how far the hand sweeps around the body (seen from above), vertical = how much it rises
        // and falls. Writes Logs/RevisaoDoCorpo/<prefixo>trajetoria.txt and a picture of the paths
        // (left half seen from above, right half seen from the front).
        public static void TrajetoriaDe(string sub, string prefixo)
        {
            string pasta = Animacoes + sub;
            var saida = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs", "RevisaoDoCorpo");
            Directory.CreateDirectory(saida);
            var texto = new System.Text.StringBuilder("TRAJETORIA da mao direita (m) · horizontal = varrida em volta do corpo · vertical = sobe e desce\n");
            var prefab = Resources.Load<GameObject>("Personagens/Masculino");
            var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var arquivos = Directory.GetFiles(pasta, "*.fbx").OrderBy(f => f).ToArray();
            const int Q = 220; var folha = new Texture2D(Q * 2, Q * Mathf.Max(1, arquivos.Length), TextureFormat.RGB24, false);
            var fundo = Enumerable.Repeat(new Color(.12f, .13f, .15f), folha.width * folha.height).ToArray(); folha.SetPixels(fundo);
            try
            {
                var corpo = Object.Instantiate(prefab); SceneManager.MoveGameObjectToScene(corpo, cena);
                corpo.transform.SetPositionAndRotation(new Vector3(0, -800, 0), Quaternion.identity);
                foreach (var mb in corpo.GetComponentsInChildren<MonoBehaviour>(true)) mb.enabled = false;
                var animator = corpo.GetComponentInChildren<Animator>(true);
                animator.enabled = true; animator.gameObject.SetActive(true); animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; animator.applyRootMotion = false;
                for (int l = 0; l < arquivos.Length; l++)
                {
                    var clip = AssetDatabase.LoadAllAssetsAtPath(arquivos[l].Replace('\\', '/')).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));
                    if (clip == null) continue;
                    var pontos = new List<Vector3>();
                    for (int i = 0; i <= 40; i++)
                    {
                        var grafo = PlayableGraph.Create("Trajetoria");
                        var o = AnimationPlayableOutput.Create(grafo, "Saida", animator);
                        var p = AnimationClipPlayable.Create(grafo, clip); p.SetApplyFootIK(true); o.SetSourcePlayable(p);
                        p.SetTime(clip.length * i / 40f); p.SetSpeed(0); grafo.Evaluate();
                        var quadril = animator.GetBoneTransform(HumanBodyBones.Hips); var mao = animator.GetBoneTransform(HumanBodyBones.RightHand);
                        pontos.Add(mao.position - new Vector3(quadril.position.x, corpo.transform.position.y, quadril.position.z));
                        grafo.Destroy();
                    }
                    float minY = pontos.Min(v => v.y), maxY = pontos.Max(v => v.y);
                    float angulo = 0; for (int i = 1; i < pontos.Count; i++) angulo += Mathf.Abs(Vector2.SignedAngle(new Vector2(pontos[i - 1].x, pontos[i - 1].z), new Vector2(pontos[i].x, pontos[i].z)));
                    float largura = pontos.Max(v => v.x) - pontos.Min(v => v.x);
                    // The blow itself: the fastest stretch of the hand (where the game cuts the clip). Which way it
                    // turns seen from above (+ = from the hero's right to his left) and how much it rises or falls.
                    int pico = 1; float maior = 0;
                    for (int i = 1; i < pontos.Count; i++) { float v = (pontos[i] - pontos[i - 1]).magnitude; if (v > maior) { maior = v; pico = i; } }
                    int de = Mathf.Max(1, pico - 6), ate = Mathf.Min(pontos.Count - 1, pico + 6);
                    float virada = 0; for (int i = de; i <= ate; i++) virada += Vector2.SignedAngle(new Vector2(pontos[i - 1].x, pontos[i - 1].z), new Vector2(pontos[i].x, pontos[i].z));
                    float queda = pontos[ate].y - pontos[de - 1].y;
                    string lado = Mathf.Abs(virada) < 40 ? "quase não vira" : virada > 0 ? "da DIREITA para a ESQUERDA" : "da ESQUERDA para a DIREITA";
                    texto.AppendLine(Path.GetFileNameWithoutExtension(arquivos[l]) + " · varre " + Mathf.RoundToInt(angulo) + "° em volta · largura " + largura.ToString("0.00") + " · sobe/desce " + (maxY - minY).ToString("0.00") + " · altura " + minY.ToString("0.0") + "-" + maxY.ToString("0.0")
                        + " || GOLPE (" + Mathf.RoundToInt(100f * pico / 40) + "%): vira " + Mathf.RoundToInt(virada) + "° " + lado + " · " + (queda < -.25f ? "DESCE " : queda > .25f ? "SOBE " : "reto ") + queda.ToString("0.00"));
                    for (int i = 0; i < pontos.Count; i++)
                    {
                        var c = Color.Lerp(new Color(.3f, .7f, 1f), new Color(1f, .5f, .2f), i / (float)pontos.Count);
                        Ponto(folha, l, 0, pontos[i].x, pontos[i].z, Q, arquivos.Length, c);   // from above
                        Ponto(folha, l, 1, pontos[i].x, pontos[i].y - 1, Q, arquivos.Length, c); // from the front
                    }
                }
                Object.DestroyImmediate(corpo);
            }
            finally
            {
                EditorSceneManager.CloseScene(cena, true);
                folha.Apply(); File.WriteAllBytes(Path.Combine(saida, prefixo + "trajetoria.png"), folha.EncodeToPNG()); Object.DestroyImmediate(folha);
                File.WriteAllText(Path.Combine(saida, prefixo + "trajetoria.txt"), texto.ToString());
            }
        }
        static void Ponto(Texture2D folha, int linha, int coluna, float a, float b, int q, int linhas, Color c)
        {
            int x = coluna * q + Mathf.RoundToInt(q / 2 + a * q / 2.4f), y = (linhas - 1 - linha) * q + Mathf.RoundToInt(q / 2 + b * q / 2.4f);
            for (int dx = -2; dx <= 2; dx++) for (int dy = -2; dy <= 2; dy++)
                { int px = x + dx, py = y + dy; if (px >= coluna * q && px < (coluna + 1) * q && py >= 0 && py < folha.height) folha.SetPixel(px, py, c); }
        }

        static void Fotografar(string[] Clipes, float[] Momentos, string[] generos, string prefixo)
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

                foreach (var genero in generos)
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
                    File.WriteAllBytes(Path.Combine(pasta, prefixo + genero + ".png"), folha.EncodeToPNG());
                    Object.DestroyImmediate(folha);
                    Object.DestroyImmediate(corpo); // one body at a time in the pictures
                }
                cam.targetTexture = null; rt.Release();
                relatorio.AppendLine("Linhas: " + string.Join(" | ", Clipes.Select(c => c == "" ? "pose original" : c)));
                relatorio.AppendLine("Colunas: cada momento (" + string.Join(", ", Momentos.Select(m => Mathf.RoundToInt(m * 100) + "%")) + ") de frente e de lado");
            }
            finally
            {
                EditorSceneManager.CloseScene(cena, true);
                File.WriteAllText(Path.Combine(pasta, prefixo + "relatorio.txt"), relatorio.ToString());
                Debug.Log(relatorio.ToString());
            }
        }
    }
}
