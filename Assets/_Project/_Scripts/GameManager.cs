using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
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
                if (!asServer) return;

                // TODO: World selection UI instead of just putting everyone in a new world
                int worldId = WorldManager.Instance.GetActiveWorldCount() + 1;

                PlayerWorldTracker.Instance.SetPlayerWorld(conn, worldId);

                GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                player.GetComponent<WorldObject>().WorldId = worldId;
                _networkManager.ServerManager.Spawn(player, conn);

                WorldManager.Instance.GetOrCreateWorld(worldId);

                // Rebuild observers for all clients
                foreach (NetworkConnection existingConn in _networkManager.ServerManager.Clients.Values)
                    InstanceFinder.ServerManager.Objects.RebuildObservers(existingConn);
            };
        }
    }

    public void MovePlayerToWorld(NetworkConnection conn, int worldId)
    {
        // 1. Track the player's world first
        PlayerWorldTracker.Instance.SetPlayerWorld(conn, worldId);

        // 2. Set the player's WorldObject id
        NetworkObject playerNob = conn.FirstObject;
        if (playerNob == null) { Debug.LogError("No player object!"); return; }
        playerNob.GetComponent<WorldObject>().WorldId = worldId;

        // 3. Now get or create the world (spawns it if needed)
        World world = WorldManager.Instance.GetOrCreateWorld(worldId);

        // 4. Rebuild observers now that everything is set
        InstanceFinder.ServerManager.Objects.RebuildObservers(conn);
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
