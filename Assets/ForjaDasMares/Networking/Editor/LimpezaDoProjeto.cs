using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ForjaDasMares.Networking.Editor
{
    // Removes the old code and scenes the game no longer uses, in steps (decided with Fabricio, 20/11/2026).
    // Everything goes to the Windows Recycle Bin (it can be restored from there).
    public static class LimpezaDoProjeto
    {
        const string R = "Assets/ForjaDasMares/";

        // Step 1: old automatic tests, old installers that already ran, scripts nothing uses and old scenes.
        static readonly string[] Etapa1 =
        {
            R + "Tests", R + "Networking/Tests",
            R + "Editor/ArenaBuilder.cs", R + "Editor/BrasaModelInstaller.cs", R + "Editor/DungeonInstaller.cs",
            R + "Editor/GaleriasInstaller.cs", R + "Editor/GaleriasMeshes.cs", R + "Editor/HeroCombatPolishInstaller.cs",
            R + "Editor/PrismaModelInstaller.cs", R + "Editor/ProgressionInstaller.cs", R + "Editor/ProjectTechnicalAudit.cs",
            R + "Editor/SapphireMageInstaller.cs",
            R + "Networking/Editor/ActOneInstaller.cs", R + "Networking/Editor/AdventurePolishInstaller.cs",
            R + "Networking/Editor/CampaignAdventureInstaller.cs", R + "Networking/Editor/CampaignForgeInstaller.cs",
            R + "Networking/Editor/CampaignInstaller.cs", R + "Networking/Editor/CampaignRpgInstaller.cs",
            R + "Networking/Editor/CampaignWildernessInstaller.cs", R + "Networking/Editor/CapitaoAfogadoInstaller.cs",
            R + "Networking/Editor/CatacombPreviewInstaller.cs", R + "Networking/Editor/CityLifeInstaller.cs",
            R + "Networking/Editor/CoopInstaller.cs", R + "Networking/Editor/CoopInternetSetup.cs",
            R + "Networking/Editor/RegionalInventoryInstaller.cs", R + "Networking/Editor/RegionalWorldInstaller.cs",
            R + "Networking/Editor/SoundscapeInstaller.cs", R + "Networking/Editor/TownArrivalPolish.cs",
            R + "Networking/Editor/TownArtInstaller.cs", R + "Networking/Editor/TownGardenExpansion.cs",
            R + "Networking/Editor/TownInstaller.cs", R + "Networking/Editor/TownLayoutInstaller.cs",
            R + "Networking/Editor/TownPlazaPolish.cs", R + "Networking/Editor/TownVisualRepairInstaller.cs",
            R + "Scripts/Gameplay/EncounterHud.cs", R + "Scripts/Gameplay/ProgressionSmokeProbe.cs",
            R + "Scripts/Npc/PersonagemVitrine.cs", R + "Scripts/Presentation/BrasaFootPlant.cs",
            R + "Scripts/Presentation/CombatFeedback.cs", R + "Scripts/Presentation/HeroPowerFeedback.cs",
            R + "Scripts/Presentation/TargetFeedback.cs", R + "Scripts/UI/ArenaHud.cs", R + "Scripts/UI/DungeonHud.cs",
            R + "Scripts/UI/HealthBarGeometry.cs", R + "Scripts/UI/HeroHud.cs", R + "Scripts/UI/RusticDungeonHud.cs",
        };
        // Scenes the game uses today; every other scene inside Assets/ForjaDasMares is old.
        static readonly string[] CenasDoJogo =
        {
            R + "Scenes/Inicio.unity", R + "Scenes/RefugioAntigo.unity", R + "Scenes/RefugioUltimoSino.unity",
            R + "Scenes/CampaignCoop.unity", // old Expedições: goes in step 3
        };

        // Fabricio's folder of downloaded models (outside the project): sorts the loose zips into folders.
        // Only moves files (nothing is deleted); folders left empty are removed.
        [MenuItem("Forja/Limpeza/Organizar a pasta Modelos 3D/Personagens")]
        public static void OrganizarModelos()
        {
            string raiz = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Modelos 3D/Personagens"));
            if (!Directory.Exists(raiz)) raiz = @"C:\Users\Fabricio_S\Desktop\Jogos\Modelos 3D\Personagens";
            if (!Directory.Exists(raiz)) { EditorUtility.DisplayDialog("Forja", "Não achei a pasta Modelos 3D/Personagens.", "OK"); return; }
            var mover = new (string de, string para)[]
            {
                ("Iria · curandeira da fonte.zip", "NPCs/Iria/Iria.zip"),
                ("Mercador da feira.zip", "NPCs/Bras (mercador)/Mercador da feira.zip"),
                ("NPC - Oficial da Liga", "NPCs/Nara (oficial da Liga)"),
                ("NPC - Teo", "NPCs/Teo"),
                ("Meshy_AI_Fantasy_Adventurer_Boy_20261118_animated_mixamo_Running_Reload.zip", "Heroi/Masculino.zip"),
                ("Meshy_AI_Female_Adventurer_20261118_animated_mixamo_Idle_Turn_Right.zip", "Heroi/Feminino.zip"),
                ("Meshy_AI_Ghost_Pirate_Rig_Ready_20261118_animated_mixamo_All_animation.zip", "Inimigos/Capitao Afogado.zip"),
                ("Meshy_AI_Javali_Low_Poly_5k_20261120_animated_All_animation.zip", "Animais/Javali.zip"),
                ("Meshy_AI_lowpoly_forest_deer_20261120_animated_All_animation.zip", "Animais/Cervo.zip"),
                ("Montarias/Meshy_AI_mystic_wolf_mount_3d_20261121_animated_All_animation.zip", "Montarias/Lobo azul.zip"),
                ("Guerreiro Brasa", "Antigos (nao usados)/Guerreiro Brasa"),
                ("Maga Prisma", "Antigos (nao usados)/Maga Prisma"),
                ("Animacoes/Magic Spell Pack.zip", "Animacoes/Pacotes Mixamo/Magic Spell Pack.zip"),
                ("Animacoes/Espada/Sword and Shield Pack.zip", "Animacoes/Pacotes Mixamo/Sword and Shield Pack.zip"),
                ("Animacoes/Espada/Cajado/Pro Magic Pack.zip", "Animacoes/Pacotes Mixamo/Pro Magic Pack.zip"),
            };
            int feitos = 0; var log = new System.Text.StringBuilder();
            foreach (var (de, para) in mover)
            {
                string a = Path.Combine(raiz, de), b = Path.Combine(raiz, para);
                bool arquivo = File.Exists(a), pasta = Directory.Exists(a);
                if ((!arquivo && !pasta) || File.Exists(b) || Directory.Exists(b)) continue;
                Directory.CreateDirectory(Path.GetDirectoryName(b));
                if (arquivo) File.Move(a, b); else Directory.Move(a, b);
                feitos++; log.Append(de).Append(" → ").Append(para).Append('\n');
            }
            // Empty folders that stayed behind.
            foreach (var vazia in new[] { "Animacoes/Espada/Cajado", "Animacoes/Espada", "Iria · curandeira da fonte", "Outros" })
            {
                string p = Path.Combine(raiz, vazia);
                if (Directory.Exists(p) && !Directory.EnumerateFileSystemEntries(p).Any()) Directory.Delete(p);
            }
            Debug.Log("MODELOS_ORGANIZADOS " + feitos + "\n" + log);
            EditorUtility.DisplayDialog("Forja", feitos + " itens organizados em Modelos 3D/Personagens.", "OK");
        }

        // After step 1 some old objects still carry components whose scripts were deleted ("script is missing").
        // This removes those empty components from the game scenes and the prefabs of the project.
        [MenuItem("Forja/Limpeza/Tirar componentes sem script (avisos amarelos)")]
        public static void TirarSemScript()
        {
            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            int total = 0;
            var aberta = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
            var cenas = AssetDatabase.FindAssets("t:Scene", new[] { R.TrimEnd('/') }).Select(AssetDatabase.GUIDToAssetPath).ToArray();
            // Prefabs first (scene copies of a prefab are cleaned through it).
            foreach (var path in AssetDatabase.FindAssets("t:Prefab", new[] { R.TrimEnd('/') }).Select(AssetDatabase.GUIDToAssetPath))
            {
                var root = PrefabUtility.LoadPrefabContents(path); int antes = total;
                foreach (var t in root.GetComponentsInChildren<Transform>(true)) total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                if (total != antes) PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
            foreach (var path in cenas)
            {
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
                int antes = total;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                        if (!PrefabUtility.IsPartOfPrefabInstance(t.gameObject)) total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                if (total != antes) UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            }
            if (!string.IsNullOrEmpty(aberta)) UnityEditor.SceneManagement.EditorSceneManager.OpenScene(aberta);
            Debug.Log("SEM_SCRIPT_REMOVIDOS " + total);
            EditorUtility.DisplayDialog("Forja", total + " componentes sem script removidos.", "OK");
        }

        [MenuItem("Forja/Limpeza/Etapa 1 · apagar testes, instaladores e cenas antigas")]
        public static void Etapa1Apagar()
        {
            var cenas = AssetDatabase.FindAssets("t:Scene", new[] { R.TrimEnd('/') }).Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !CenasDoJogo.Contains(p) && !p.StartsWith(R + "Scenes/Areas/")).ToList();
            var tudo = Etapa1.Where(p => File.Exists(p) || Directory.Exists(p)).Concat(cenas).Distinct().ToArray();
            if (tudo.Length == 0) { EditorUtility.DisplayDialog("Forja · Limpeza", "Nada para apagar: a Etapa 1 já foi feita.", "OK"); return; }
            string lista = string.Join("\n", tudo.Select(p => p.Replace(R, "")).Take(40)) + (tudo.Length > 40 ? "\n... e mais " + (tudo.Length - 40) : "");
            if (!EditorUtility.DisplayDialog("Forja · Limpeza · Etapa 1",
                tudo.Length + " itens vão para a Lixeira do Windows (dá pra recuperar):\n\n" + lista, "Apagar", "Cancelar")) return;
            var falhou = new List<string>();
            AssetDatabase.MoveAssetsToTrash(tudo, falhou);
            // The build list keeps only scenes that still exist.
            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Where(s => File.Exists(s.path)).ToArray();
            AssetDatabase.Refresh();
            Debug.Log("LIMPEZA_ETAPA1 apagados=" + (tudo.Length - falhou.Count) + " falhas=" + falhou.Count + (falhou.Count > 0 ? " · " + string.Join(", ", falhou) : "") + "\n" + string.Join("\n", tudo));
        }
    }
}
