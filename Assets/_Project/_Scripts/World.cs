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
    [SerializeField] private Vector2 tilemapOffset = new Vector2(-50, -50);

    private Dictionary<Vector3Int, string> _tileModifications = new();
    private Dictionary<string, TileBase> _tileRegistry;

    private void Awake()
    {
        tilemap.transform.position = tilemapOffset;
        // TODO: Replace with ScriptableObject registry
        _tileRegistry = new Dictionary<string, TileBase>
        {
            { "dirt", dirtTile },
            { "stone", stoneTile },
        };
    }

    private TileBase GetTile(string tileId)
    {
        _tileRegistry.TryGetValue(tileId, out TileBase tile);
        return tile;
    }

    public void Initialize(int worldId)
    {
        WorldId = worldId;
        GetComponent<WorldObject>().WorldId = worldId;
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        Random.InitState(WorldId);
        int width = 100;
        int height = 100;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (y < 50 && y > 43)
                    tilemap.SetTile(new Vector3Int(x, y, 0), dirtTile);
                else if (y <= 43)
                    tilemap.SetTile(new Vector3Int(x, y, 0), stoneTile);
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsServerInitialized) return;
        GenerateTerrain();

        // Replay any modifications that happened before this client joined
        foreach (var kvp in _tileModifications)
        {
            TileBase tile = kvp.Value != null ? GetTile(kvp.Value) : null;
            tilemap.SetTile(kvp.Key, tile);
        }
    }

    public void BreakTile(Vector3Int position)
    {
        // Account for offset of tilemap in world space
        position -= Vector3Int.FloorToInt((Vector3)tilemapOffset);
        if (tilemap.GetTile(position) == null) return;
        tilemap.SetTile(position, null);
        _tileModifications[position] = null;
        SyncBreakTile(position);
    }

    public void PlaceTile(Vector3Int position, string tileId)
    {
        position -= Vector3Int.FloorToInt((Vector3)tilemapOffset);
        TileBase tile = GetTile(tileId);
        if (tile == null) return;
        if (tilemap.GetTile(position) != null) return;
        tilemap.SetTile(position, tile);
        _tileModifications[position] = tileId;
        SyncPlaceTile(position, tileId);
    }

    [ObserversRpc]
    private void SyncBreakTile(Vector3Int position)
    {
        if (IsServerInitialized) return;
        tilemap.SetTile(position, null);
    }

    [ObserversRpc]
    private void SyncPlaceTile(Vector3Int position, string tileId)
    {
        if (IsServerInitialized) return;
        tilemap.SetTile(position, GetTile(tileId));
    }

    public void SyncModificationsToClient(NetworkConnection conn)
    {
        foreach (var kvp in _tileModifications)
            TargetRpcSyncTile(conn, kvp.Key, kvp.Value);
    }

    [TargetRpc]
    private void TargetRpcSyncTile(NetworkConnection conn, Vector3Int position, string tileId)
    {
        TileBase tile = tileId != null ? GetTile(tileId) : null;
        tilemap.SetTile(position, tile);
    }
}