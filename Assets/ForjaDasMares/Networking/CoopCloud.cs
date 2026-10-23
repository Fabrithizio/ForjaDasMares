using System;
using System.Collections.Generic;
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
        bool busy, disposing,advertising,advertDirty;float advertClock;string advertKey="";string testPool="";
        public string LastError {get;private set;}
        public bool PublicRoom=>session!=null&&!session.IsPrivate;
        public bool CanChangeVisibility=>session!=null&&session.IsHost&&director.Campaign!=null;
        public void ToggleVisibility()
        {
            if(!CanChangeVisibility||Busy)return;
            var host=session.AsHost();host.IsPrivate=!host.IsPrivate;advertDirty=true;
        }
        CoopInternetSettings settings;
        public bool Busy => busy || disposing || advertising;
        string ResumeKey => "CoopCloudResume." + Application.cloudProjectId + "." + settings.Environment + "." + AuthenticationService.Instance.Profile;
        public bool Configured => CoopInternetSettings.HasLinkedProject(Application.cloudProjectId) && settings != null && settings.IsValid;
        public string Message { get; private set; } = "UGS não vinculado: configure o projeto para salas pela internet.";
        void Awake()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            var flags=System.Environment.GetCommandLineArgs();for(int i=0;i<flags.Length-1;i++)if(flags[i]=="-campaignTestPool"&&flags[i+1].Length<=32)testPool="-qa-"+flags[i+1];
#endif
            settings = Resources.Load<CoopInternetSettings>(CoopInternetSettings.ResourceName);
            director = GetComponent<CoopDirector>(); director.Changed += Changed;
            if (Configured) Message = "Internet preparada — ambiente: " + settings.Environment + ". Crie uma sala ou entre por código.";
        }
        public async void Create() { await Open(true, null); }
        public async void Join(string code) { await Open(false, code); }
        public async void FindCampaignGroup(){await Open(false,null,true);}
        public string CampaignKey=>"fdm-campaign-p"+CoopLimits.Protocol+"-l"+(director.Campaign!=null?director.Campaign.Selected:0)+testPool;
        public SessionOptions CampaignOptions(bool privateRoom)
        {
            var options=new SessionOptions{MaxPlayers=4,IsPrivate=privateRoom,Name=director.Campaign!=null?director.Campaign.Level.displayName:"Forja das Marés"};
            if(director.Campaign!=null)options.SessionProperties["campaign"]=new SessionProperty(CampaignKey,VisibilityPropertyOptions.Public,PropertyIndex.String1);
            return options.WithRelayNetwork(settings.RelayRegion);
        }
        async Task<ISession> FindCampaign(CancellationToken cancellation)
        {
            Message="Procurando grupo para esta fase…";
            var options=new QuerySessionsOptions{Count=10,FilterOptions=new List<FilterOption>{
                new FilterOption(FilterField.StringIndex1,CampaignKey,FilterOperation.Equal),
                new FilterOption(FilterField.IsLocked,"false",FilterOperation.Equal),
                new FilterOption(FilterField.AvailableSlots,"0",FilterOperation.Greater)}};
            var found=await MultiplayerService.Instance.QuerySessionsAsync(options,cancellation);
            foreach(var candidate in found.Sessions)
            {
                cancellation.ThrowIfCancellationRequested();
                try{return await MultiplayerService.Instance.JoinSessionByIdAsync(candidate.Id,new JoinSessionOptions(),cancellation);}
                catch(SessionException error){Message="A vaga mudou; procurando outro grupo: "+error.Error;}
            }
            Message="Nenhum grupo disponível: criando uma sala aberta desta fase.";
            return await MultiplayerService.Instance.CreateSessionAsync(CampaignOptions(false),cancellation);
        }
        async Task Open(bool host, string code,bool find=false)
        {
            if (Busy || director.Manager.IsListening) return;
            if (!Configured) { Message = "Internet indisponível: vincule o projeto em Unity Services e confira Forja das Mares/Internet/Configuração."; return; }
            if (!host && !find && (string.IsNullOrWhiteSpace(code) || code.Length > 12)) { Message = "Informe o código privado da sala."; return; }
            LastError=null;busy = true; operation = new CancellationTokenSource(TimeSpan.FromSeconds(settings.ConnectionTimeoutSeconds));
            try
            {
                Message = "Conectando à internet…";
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
                director.PrepareCloudConnection(find);
                session = find ? await FindCampaign(operation.Token) : host
                    ? await MultiplayerService.Instance.CreateSessionAsync(CampaignOptions(true), operation.Token)
                    : await JoinOrResume(code.Trim().ToUpperInvariant(), operation.Token);
                if (!session.IsHost)
                {
                    PlayerPrefs.SetString(ResumeKey, JsonUtility.ToJson(new CoopSessionResume { sessionId = session.Id, code = session.Code }));
                    PlayerPrefs.Save();
                }
                operation.Token.ThrowIfCancellationRequested();
                director.CompleteCloudConnection(session.Code,session.IsPrivate);advertDirty=true; Message = (session.IsPrivate?"Sala privada: ":"Grupo aberto da fase: ")+session.Code;
            }
            catch (Exception error)
            { Message = FriendlyConnectionError(error);Debug.LogWarning("CLOUD_CONNECTION_FAILED "+error.GetType().Name+": "+error.Message); LastError=Message; director.Close(); await DisposeSession(); }
            finally { busy = false; operation?.Dispose(); operation = null; }
        }
        public static string FriendlyConnectionError(Exception error)
        {
            if(error is OperationCanceledException)return "Conexão cancelada ou tempo esgotado.";
            if(error is SessionException failure)
            {
                if(failure.Error==SessionError.SessionNotFound||failure.Error==SessionError.SessionDeleted||failure.Error==SessionError.InvalidSessionIdentifier)return "Sala não encontrada ou encerrada. Peça um código novo ao host.";
                if(failure.Error==SessionError.RateLimitExceeded)return "Muitas tentativas seguidas. Aguarde alguns segundos antes de tentar novamente.";
                if(failure.Error==SessionError.Forbidden)return "Esta sala não aceita sua entrada agora. Confira o código com o host.";
            }
            return "Não foi possível conectar à sala. Confira a internet e tente novamente; a sala pode ter encerrado ou ficado sem vagas.";
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
        async Task SaveCampaignAdvert(bool locked)
        {
            var current=session;if(current==null||!current.IsHost||director.Campaign==null||advertising)return;
            advertising=true;
            try
            {
                var owner=current.AsHost();owner.IsLocked=locked;
                owner.SetProperty("campaign",new SessionProperty(CampaignKey,VisibilityPropertyOptions.Public,PropertyIndex.String1));
                await owner.SavePropertiesAsync();
            }
            catch(Exception error){if(ReferenceEquals(session,current)&&!disposing){advertDirty=true;advertClock=3;Message="Não foi possível atualizar o anúncio da sala: "+error.Message;}}
            finally{advertising=false;}
        }
        void Changed()
        {
            advertDirty=true;
            if(director.Phase==CoopPhase.Closed){operation?.Cancel();_=DisposeSession();}
        }
        void Update()
        {
            if(session==null||!session.IsHost||director.Campaign==null||busy||disposing||advertising)return;
            advertClock-=Time.unscaledDeltaTime;if(advertClock>0)return;advertClock=1;
            bool locked=director.Phase!=CoopPhase.Lobby&&!director.CanAdmitNewPlayer||!director.HasAdmissionSeat;
            string key=CampaignKey+":"+locked+":"+session.IsPrivate;
            if(!advertDirty&&key==advertKey)return;advertDirty=false;advertKey=key;_=SaveCampaignAdvert(locked);
        }
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
