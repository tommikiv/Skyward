using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World : NetworkBehaviour
{
    public int WorldId { get; private set; }

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase dirtTile;
    [SerializeField] private TileBase stoneTile;

    public void Initialize(int worldId)
    {
        WorldId = worldId;
        GetComponent<WorldObject>().WorldId = worldId;
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        // Generate using WorldId as seed so it's consistent
        Random.InitState(WorldId);

        int width = 100;
        int height = 100;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (y < 50 && y > 43)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), dirtTile);
                } else if (y <= 43)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), stoneTile);
                }
                
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Sync initial tilemap state to late-joining clients
        if (IsServerInitialized) return; // server already has it
        GenerateTerrain();
    }

    // Client requests to place a tile
    /*
    [ServerRpc(RequireOwnership = false)]
    public void PlaceTileServer(Vector3Int position, string tileId, NetworkConnection sender = null)
    {
        // Validate: is the sender actually in this world?
        if (!PlayerWorldTracker.Instance.TryGetWorld(sender, out int worldId) || worldId != WorldId)
            return;

        // Validate: is the position within range of the player?
        // (add your own range check here)

        TileBase tile = TileRegistry.GetTile(tileId);
        if (tile == null) return;

        tilemap.SetTile(position, tile);
        SyncTileObservers(position, tileId);
    }

    // Client requests to break a tile
    [ServerRpc(RequireOwnership = false)]
    public void BreakTileServer(Vector3Int position, NetworkConnection sender = null)
    {
        if (!PlayerWorldTracker.Instance.TryGetWorld(sender, out int worldId) || worldId != WorldId)
            return;

        // Validate: is there actually a tile here?
        if (tilemap.GetTile(position) == null) return;

        tilemap.SetTile(position, null);
        SyncTileObservers(position, null);
    }
    
    // Only sync to observers of this NetworkObject (i.e. players in this world)
    [ObserversRpc]
    private void SyncTileObservers(Vector3Int position, string tileId)
    {
        if (IsServer) return; // server already applied it

        TileBase tile = tileId != null ? TileRegistry.GetTile(tileId) : null;
        tilemap.SetTile(position, tile);
    }
    */
}