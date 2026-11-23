using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;

namespace ForjaDasMares.Networking.Editor
{
    // Writes every compile result (errors and warnings of our code) to Logs/compilacao.txt in the project folder,
    // so the result can be read without looking at the Unity window.
    [InitializeOnLoad]
    static class RelatorioDeCompilacao
    {
        static readonly StringBuilder texto = new StringBuilder();
        static int erros;
        static RelatorioDeCompilacao()
        {
            CompilationPipeline.compilationStarted -= Comecou; CompilationPipeline.compilationStarted += Comecou;
            CompilationPipeline.assemblyCompilationFinished -= Terminou; CompilationPipeline.assemblyCompilationFinished += Terminou;
            CompilationPipeline.compilationFinished -= Fim; CompilationPipeline.compilationFinished += Fim;
        }
        static void Comecou(object _) { texto.Clear(); erros = 0; texto.AppendLine("COMPILACAO " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); }
        static void Terminou(string assembly, CompilerMessage[] messages)
        {
            foreach (var m in messages)
            {
                if (m.type == CompilerMessageType.Error) erros++;
                else if (!m.file.Contains("ForjaDasMares")) continue;
                texto.AppendLine((m.type == CompilerMessageType.Error ? "ERRO " : "aviso ") + m.message);
            }
        }
        static void Fim(object _)
        {
            texto.AppendLine(erros == 0 ? "RESULTADO OK" : "RESULTADO " + erros + " ERROS");
            try { Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/compilacao.txt", texto.ToString()); } catch { }
        }
    }
}
