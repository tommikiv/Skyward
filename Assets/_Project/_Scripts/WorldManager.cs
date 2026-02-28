using FishNet;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    [SerializeField] private GameObject worldPrefab;

    private Dictionary<int, World> _activeWorlds = new();

    private void Awake() => Instance = this;

    public World GetOrCreateWorld(int worldId)
    {
        if (_activeWorlds.TryGetValue(worldId, out World existing))
            return existing;

        GameObject worldObj = Instantiate(worldPrefab);
        World world = worldObj.GetComponent<World>();

        // Make sure Initialize is called BEFORE Spawn
        world.Initialize(worldId);

        InstanceFinder.ServerManager.Spawn(worldObj);

        _activeWorlds[worldId] = world;
        return world;
    }

    public void TryDespawnWorld(int worldId)
    {
        if (!_activeWorlds.TryGetValue(worldId, out World world)) return;

        // Only despawn if no players are left in it
        if (PlayerWorldTracker.Instance.GetPlayerCountInWorld(worldId) > 0) return;

        world.Despawn();
        Destroy(world.gameObject);
        _activeWorlds.Remove(worldId);
    }

    public int GetActiveWorldCount() => _activeWorlds.Count;
}
