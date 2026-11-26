using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ForjaDasMares.Networking.Editor
{
    // Runs once by itself when the Great Sword Pack files are in the project (or after a change here):
    // installs the two-handed moves (Animações por arma) and takes the photos of the new blows, writing a report
    // to Logs/animacoes.txt. The same can be done by hand in Forja/Personagem.
    [InitializeOnLoad]
    static class AutoPesada
    {
        const int Version = 4; // 2: combo = the same side slash three times · 3: combo of three sideways blows, new Super · 4: 2nd blow = Golpe_2 (measured)
        static AutoPesada() { EditorApplication.update += Tentar; }
        // Waits until the Editor is idle (not compiling, importing or playing), then runs once.
        static void Tentar()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            EditorApplication.update -= Tentar;
            if (EditorPrefs.GetInt("Forja.Pesada.Trajetoria", 0) < 3 && File.Exists("Assets/ForjaDasMares/Art/Personagens/Animacoes/Pesada/Pesada_Parado.fbx"))
            {
                EditorPrefs.SetInt("Forja.Pesada.Trajetoria", 3);
                try { RevisaoDoCorpo.TrajetoriaDe("Pesada/Golpes/", "Pesada_"); } catch (Exception e) { Debug.LogException(e); }
            }
            {
                if (!File.Exists("Assets/ForjaDasMares/Art/Personagens/Animacoes/Pesada/Pesada_Parado.fbx")) return;
                if (EditorPrefs.GetInt("Forja.Pesada.Versao", 0) >= Version) return;
                EditorPrefs.SetInt("Forja.Pesada.Versao", Version);
                var log = new System.Text.StringBuilder(DateTime.Now + "\n");
                Application.LogCallback grab = (text, trace, type) => { if (text.StartsWith("ANIMACOES_POR_ARMA") || type == LogType.Error || type == LogType.Exception) log.AppendLine(text); };
                Application.logMessageReceived += grab;
                try
                {
                    ExpedicionarioInstaller.InstallWeaponLocomotion();
                    RevisaoDoCorpo.VerGolpesPesada();
                    log.AppendLine("PESADA_PRONTA");
                }
                catch (Exception e) { log.AppendLine("ERRO " + e); Debug.LogException(e); }
                finally { Application.logMessageReceived -= grab; File.WriteAllText("Logs/animacoes.txt", log.ToString()); }
            }
        }
    }
}
