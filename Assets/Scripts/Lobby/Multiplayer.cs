using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

[System.Serializable]
public enum EncryptionType
{
    DTLS, // Datagram Transport Layer Security
    WSS  // Web Socket Secure
}
// Note: Also Udp and Ws are possible choices

public class Multiplayer : MonoBehaviour
{
    [SerializeField] string lobbyName = "Lobby";
    [SerializeField] int maxPlayers = 4;
    [SerializeField] EncryptionType encryption = EncryptionType.DTLS;

    public static Multiplayer Instance { get; private set; }

    public string PlayerId { get; private set; }
    public string PlayerName { get; private set; }

    Lobby currentLobby;
    string connectionType => encryption == EncryptionType.DTLS ? k_dtlsEncryption : k_wssEncryption;

    const float k_lobbyHeartbeatInterval = 15f;
    const float k_lobbyPollInterval = 3f;
    float timer_lobbyHeartbeatInterval = 0;
    float timer_lobbyPollInterval = 0;
    const string k_keyJoinCode = "RelayJoinCode";
    const string k_dtlsEncryption = "dtls"; // Datagram Transport Layer Security
    const string k_wssEncryption = "wss"; // Web Socket Secure, use for WebGL builds


    async void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        await Authenticate();
    }

    private void Update()
    {
        timer_lobbyHeartbeatInterval -= Time.deltaTime;
        timer_lobbyPollInterval -= Time.deltaTime;

        if (currentLobby != null)
        {
            if (currentLobby.HostId == AuthenticationService.Instance.PlayerId
                && timer_lobbyHeartbeatInterval <= 0)
            {
                timer_lobbyHeartbeatInterval = k_lobbyHeartbeatInterval;
                HandleHeartbeatAsync();
            }

            if (UnityServices.State == ServicesInitializationState.Initialized
                && AuthenticationService.Instance.IsSignedIn
                // && SceneManager.GetActiveScene().name == Loader.Scene.LobbyScene.ToString()
                && timer_lobbyPollInterval <= 0)
            {
                timer_lobbyPollInterval = k_lobbyPollInterval;
                HandlePollForUpdatesAsync();
            }
        }
    }

    async Task Authenticate()
    {
        await Authenticate("Player" + Random.Range(0, 1000));
    }

    async Task Authenticate(string playerName)
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(playerName);

            await UnityServices.InitializeAsync(options);
        }

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as " + AuthenticationService.Instance.PlayerId);
        };

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            PlayerId = AuthenticationService.Instance.PlayerId;
            PlayerName = playerName;
        }
    }

    public async Task CreateLobby()
    {
        try
        {
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation);

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log("Created lobby: " + currentLobby.Name + " with code " + currentLobby.LobbyCode);

            await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                        {k_keyJoinCode, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
                    }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                allocation, connectionType));

            StartHost();

        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Failed to create lobby: " + e.Message);
        }
    }

    public async Task QuickJoinLobby()
    {
        try
        {
            currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string relayJoinCode = currentLobby.Data[k_keyJoinCode].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                joinAllocation, connectionType));

            StartClient();

        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Failed to quick join lobby: " + e.Message);
        }
    }

    public async Task JoinLobby(Lobby lobby)
    {
        try
        {
            // currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            currentLobby = lobby;
            // pollForUpdatesTimer.Start();

            string relayJoinCode = currentLobby.Data[k_keyJoinCode].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                joinAllocation, connectionType));

            StartClient();

        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Failed to quick join lobby: " + e.Message);
        }
    }

    async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log("Failed to allocate relay: " + e.Message);
            return default;
        }
    }

    async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log("Failed to get relay join code: " + e.Message);
            return default;
        }
    }

    async Task<JoinAllocation> JoinRelay(string relayJoinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log("Failed to join relay: " + e.Message);
            return default;
        }
    }

    async Task HandleHeartbeatAsync()
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            Debug.Log("Sent heartbeat ping to lobby: " + currentLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Failed to heartbeat lobby: " + e.Message);
        }
    }

    async Task HandlePollForUpdatesAsync()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            Debug.Log("Polled for updates on lobby: " + lobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Failed to poll for updates on lobby: " + e.Message);
        }
    }

    public async void ShowLobbies()
    {
        QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
        var lobbies = queryResponse.Results;
        foreach (Lobby lobby in lobbies)
        {
            Debug.Log("--> Lobby " + lobby.Id);
            ShowPlayers(lobby);
        }
    }

    void ShowPlayers(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            string s = $"PlayerId {player.Id} name {player}";
            Debug.Log("Playerid: " + player.Id);
        }
    }

    void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Loader.NetworkLoadScene(Loader.SceneName.MainScene);
    }

    void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Loader.NetworkLoadScene(Loader.SceneName.MainScene);
    }

    public async Task<List<Lobby>> RefreshLobbies()
    {
        QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
        return queryResponse.Results;
    }
}

