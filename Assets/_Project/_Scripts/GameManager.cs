using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Transporting;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject playerPrefab;
    public NetworkRole networkRole;

    private NetworkManager _networkManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _networkManager = FindFirstObjectByType<NetworkManager>();

        // [EDITOR] Use Multiplayer PlayMode tag to determine if this instance should be a server or client
#if UNITY_EDITOR
        var currentPlayer = Unity.Multiplayer.Playmode.CurrentPlayer.ReadOnlyTags();
        if (currentPlayer.Contains("Server"))
        {
            networkRole = NetworkRole.Server;
            Debug.Log("Starting as Server");
        } else if (currentPlayer.Contains("Client"))
        {
            Debug.Log("Starting as Client");
            networkRole = NetworkRole.Client;
        }
#endif

        // If is server, start the server
        if (networkRole == NetworkRole.Server)
        {
            _networkManager.ServerManager.OnRemoteConnectionState += OnClientConnected;
            _networkManager.ServerManager.StartConnection();
        }

        // If is client, connect client to the server and spawn player
        if (networkRole == NetworkRole.Client)
        {
            _networkManager.ClientManager.StartConnection();
        }
    }

    private void OnClientConnected(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            conn.OnLoadedStartScenes += (conn, asServer) =>
            {
                GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                _networkManager.ServerManager.Spawn(player, conn);
                Debug.Log($"Client connected: {conn.ClientId}, spawned player: {player.name}");
            };
        }
    }

    private void OnDestroy()
    {
        if (_networkManager != null && networkRole == NetworkRole.Server)
        {
            _networkManager.ServerManager.OnRemoteConnectionState -= OnClientConnected;
        }
    }
}

public enum NetworkRole
{
    Server,
    Client,
}
