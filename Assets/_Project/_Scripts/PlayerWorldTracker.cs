using FishNet.Connection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerWorldTracker : MonoBehaviour
{
    public static PlayerWorldTracker Instance { get; private set; }
    private Dictionary<NetworkConnection, int> _playerWorlds = new();

    private void Awake() => Instance = this;

    public void SetPlayerWorld(NetworkConnection conn, int worldId)
    {
        _playerWorlds[conn] = worldId;
    }

    public bool TryGetWorld(NetworkConnection conn, out int worldId)
    {
        return _playerWorlds.TryGetValue(conn, out worldId);
    }

    public int GetPlayerCountInWorld(int worldId)
    {
        return _playerWorlds.Values.Count(id => id == worldId);
    }
}