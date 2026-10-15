using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Multiplayer;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    [Serializable]
    public sealed class CoopSessionResume
    {
        public string sessionId, code;
        public bool Matches(string requestedCode) => !string.IsNullOrWhiteSpace(sessionId) && sessionId.Length <= 128
            && !string.IsNullOrWhiteSpace(code) && code.Length <= 12 && !string.IsNullOrWhiteSpace(requestedCode)
            && string.Equals(code, requestedCode.Trim(), StringComparison.OrdinalIgnoreCase);
    }
    public sealed class CoopCloud : MonoBehaviour
    {
        CoopDirector director; ISession session; CancellationTokenSource operation;
        bool busy, disposing;
        CoopInternetSettings settings;
        public bool Busy => busy || disposing;
        string ResumeKey => "CoopCloudResume." + Application.cloudProjectId + "." + settings.Environment + "." + AuthenticationService.Instance.Profile;
        public bool Configured => CoopInternetSettings.HasLinkedProject(Application.cloudProjectId) && settings != null && settings.IsValid;
        public string Message { get; private set; } = "UGS não vinculado: configure o projeto para salas pela internet.";
        void Awake()
        {
            settings = Resources.Load<CoopInternetSettings>(CoopInternetSettings.ResourceName);
            director = GetComponent<CoopDirector>(); director.Changed += Changed;
            if (Configured) Message = "Internet preparada — ambiente: " + settings.Environment + ". Crie uma sala ou entre por código.";
        }
        public async void Create() { await Open(true, null); }
        public async void Join(string code) { await Open(false, code); }
        async Task Open(bool host, string code)
        {
            if (Busy || director.Manager.IsListening) return;
            if (!Configured) { Message = "Internet indisponível: vincule o projeto em Unity Services e confira Forja das Mares/Internet/Configuração."; return; }
            if (!host && (string.IsNullOrWhiteSpace(code) || code.Length > 12)) { Message = "Informe o código privado da sala."; return; }
            busy = true; operation = new CancellationTokenSource(TimeSpan.FromSeconds(settings.ConnectionTimeoutSeconds));
            try
            {
                Message = "Inicializando serviços e autenticação anônima…";
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    var options = new InitializationOptions().SetEnvironmentName(settings.Environment);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    // Isolate SDK identities when testing multiple processes on one PC.
                    var args = System.Environment.GetCommandLineArgs();
                    for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-ugsProfile") options.SetProfile(args[i + 1]);
#endif
                    await UnityServices.InitializeAsync(options);
                }
                operation.Token.ThrowIfCancellationRequested();
                if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
                operation.Token.ThrowIfCancellationRequested();
                director.PrepareCloudConnection();
                session = host
                    ? await MultiplayerService.Instance.CreateSessionAsync(new SessionOptions { MaxPlayers = 4, IsPrivate = true, Name = "Forja das Marés" }.WithRelayNetwork(settings.RelayRegion), operation.Token)
                    : await JoinOrResume(code.Trim().ToUpperInvariant(), operation.Token);
                if (!host)
                {
                    PlayerPrefs.SetString(ResumeKey, JsonUtility.ToJson(new CoopSessionResume { sessionId = session.Id, code = session.Code }));
                    PlayerPrefs.Save();
                }
                operation.Token.ThrowIfCancellationRequested();
                director.CompleteCloudConnection(session.Code); Message = "Sala privada UGS: " + session.Code;
            }
            catch (Exception error)
            { Message = error is OperationCanceledException ? "Conexão cancelada ou tempo esgotado." : "Falha nos serviços: " + error.Message; director.Close(); await DisposeSession(); }
            finally { busy = false; operation?.Dispose(); operation = null; }
        }
        async Task<ISession> JoinOrResume(string code, CancellationToken cancellation)
        {
            CoopSessionResume resume = null;
            try { resume = JsonUtility.FromJson<CoopSessionResume>(PlayerPrefs.GetString(ResumeKey, "")); }
            catch (ArgumentException) { PlayerPrefs.DeleteKey(ResumeKey); }
            if (resume != null && resume.Matches(code))
            {
                Message = "Retornando à sala privada…";
                try { return await MultiplayerService.Instance.ReconnectToSessionAsync(resume.sessionId, new ReconnectSessionOptions(), cancellation); }
                catch (SessionException error) when (error.Error == SessionError.NotInLobby || error.Error == SessionError.SessionNotFound
                    || error.Error == SessionError.SessionDeleted || error.Error == SessionError.Forbidden || error.Error == SessionError.NotAuthorized)
                { PlayerPrefs.DeleteKey(ResumeKey); PlayerPrefs.Save(); }
            }
            return await MultiplayerService.Instance.JoinSessionByCodeAsync(code, new JoinSessionOptions(), cancellation);
        }
        void Changed() { if (director.Phase == CoopPhase.Closed) { operation?.Cancel(); _ = DisposeSession(); } }
        public void Cancel() { operation?.Cancel(); director.Close(); }
        async Task DisposeSession()
        {
            if (disposing || session == null) return; disposing = true; var previous = session; session = null;
            try
            {
                if (previous.IsHost) await previous.AsHost().DeleteAsync();
                else { await previous.LeaveAsync(); PlayerPrefs.DeleteKey(ResumeKey); PlayerPrefs.Save(); }
            }
            catch (Exception error) { Message = "Sala local encerrada; serviço não confirmou remoção: " + error.Message; }
            finally { disposing = false; }
        }
        void OnDestroy() { if (director != null) director.Changed -= Changed; operation?.Cancel(); _ = DisposeSession(); }
    }
}
