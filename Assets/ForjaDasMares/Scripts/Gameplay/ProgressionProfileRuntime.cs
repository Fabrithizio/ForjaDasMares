using System;
using System.IO;
using UnityEngine;
namespace ForjaDasMares
{
    public sealed class ProgressionProfileRuntime : MonoBehaviour
    {
        static ProgressionProfileRuntime current;
        public ProfileService Service { get; private set; }
        public ProgressionCatalog Catalog { get; private set; }
        public string SavePath { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetStatics() => current = null;
        public static ProgressionProfileRuntime Open(ProgressionCatalog catalog)
        {
            if (current != null) { if (current.Catalog != catalog) throw new InvalidOperationException("Scenes disagree on the progression catalog."); return current; }
            string directory = Application.persistentDataPath; bool tests = false; var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            { if (args[i] == "-runTests") tests = true; if (args[i] == "-progressionSavePath" && i + 1 < args.Length) directory = args[++i]; }
            if (tests && directory == Application.persistentDataPath) directory = Path.Combine(Application.temporaryCachePath, "ProgressionTests_" + Guid.NewGuid().ToString("N"));
            var service = new ProfileService(catalog, new ProfileStore(Path.Combine(directory, "progress-v2.json")));
            if (service.Profile.expedition != null && !service.FinishExpedition(service.Profile.expedition.runId, false, "Aplicação interrompida")) throw new InvalidOperationException(service.Message);
            current = new GameObject("LocalProgressionProfile").AddComponent<ProgressionProfileRuntime>(); current.Catalog = catalog; current.Service = service; current.SavePath = Path.Combine(directory, "progress-v2.json"); DontDestroyOnLoad(current.gameObject);
            AudioListener.volume = service.Profile.options.volume; return current;
        }
        void OnDestroy() { if (current == this) current = null; }
        void OnApplicationQuit() { if (Service?.Profile.expedition != null) Service.FinishExpedition(Service.Profile.expedition.runId, false, "Abandono"); }
    }
}
