using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    public sealed class CoopCloud : MonoBehaviour
    {
        CoopDirector director; ISession session; CancellationTokenSource operation;
        bool busy, disposing;
        public bool Busy => busy;
        public bool Configured => !string.IsNullOrWhiteSpace(Application.cloudProjectId);
        public string Message { get; private set; } = "UGS não vinculado: configure o projeto para salas pela internet.";
        void Awake() { director = GetComponent<CoopDirector>(); director.Changed += Changed; }
        public async void Create() { await Open(true, null); }
        public async void Join(string code) { await Open(false, code); }
        async Task Open(bool host, string code)
        {
            if (busy || director.Manager.IsListening) return;
            if (!Configured) { Message = "Vincule o projeto em Unity Services e habilite Authentication/Lobby/Relay. Nenhuma credencial foi inventada."; return; }
            if (!host && (string.IsNullOrWhiteSpace(code) || code.Length > 12)) { Message = "Informe o código privado da sala."; return; }
            busy = true; operation = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            try
            {
                Message = "Inicializando serviços e autenticação anônima…";
                if (UnityServices.State != ServicesInitializationState.Initialized) await UnityServices.InitializeAsync();
                operation.Token.ThrowIfCancellationRequested();
                if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
                operation.Token.ThrowIfCancellationRequested();
                director.PrepareCloudConnection();
                session = host
                    ? await MultiplayerService.Instance.CreateSessionAsync(new SessionOptions { MaxPlayers = 4, IsPrivate = true, Name = "Forja das Marés" }.WithRelayNetwork(), operation.Token)
                    : await MultiplayerService.Instance.JoinSessionByCodeAsync(code.Trim().ToUpperInvariant(), new JoinSessionOptions(), operation.Token);
                operation.Token.ThrowIfCancellationRequested();
                director.CompleteCloudConnection(session.Code); Message = "Sala privada UGS: " + session.Code;
            }
            catch (Exception error)
            { Message = error is OperationCanceledException ? "Conexão cancelada ou tempo esgotado." : "Falha nos serviços: " + error.Message; director.Close(); await DisposeSession(); }
            finally { busy = false; operation?.Dispose(); operation = null; }
        }
        void Changed() { if (director.Phase == CoopPhase.Closed) { operation?.Cancel(); _ = DisposeSession(); } }
        public void Cancel() { operation?.Cancel(); director.Close(); }
        async Task DisposeSession()
        {
            if (disposing || session == null) return; disposing = true; var previous = session; session = null;
            try { if (previous.IsHost) await previous.AsHost().DeleteAsync(); else await previous.LeaveAsync(); }
            catch (Exception error) { Message = "Sala local encerrada; serviço não confirmou remoção: " + error.Message; }
            finally { disposing = false; }
        }
        void OnDestroy() { if (director != null) director.Changed -= Changed; operation?.Cancel(); _ = DisposeSession(); }
    }
}
