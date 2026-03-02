using FishNet.Object;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerWorldInteraction : NetworkBehaviour
{
    [SerializeField] private float interactRange = 5f;
    [SerializeField] private TileBase placeTile;

    private PlayerInput _playerInput;

    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector3 mouseScreen = _playerInput.GetMousePosition();
        Vector3 mouseWorld = _camera.ScreenToWorldPoint(mouseScreen);
        Vector3Int tilePos = GetTilePos(mouseWorld);

        if (!InRange(mouseWorld)) return;

        if (_playerInput.IsPrimaryPressed())
        {
            Debug.Log($"Requesting break tile at {tilePos}");
            RequestBreakTile(tilePos);
        }

        // TODO: Replace "dirt" with currently selected tile from inventory
        if (_playerInput.IsSecondaryPressed())
            RequestPlaceTile(tilePos, "dirt");
    }

    private Vector3Int GetTilePos(Vector3 worldPos)
    {
        return new Vector3Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y), 0);
    }

    private bool InRange(Vector3 worldPos)
    {
        return Vector2.Distance(transform.position, worldPos) <= interactRange;
    }

    [ServerRpc]
    private void RequestBreakTile(Vector3Int tilePos)
    {
        // Validate range server-side
        Vector3 tileWorld = new Vector3(tilePos.x + 0.5f, tilePos.y + 0.5f, 0f);
        if (Vector2.Distance(transform.position, tileWorld) > interactRange) {
            Debug.LogWarning("Player tried to break a tile out of range");
            return;
        }

        // Find the world this player is in
        World world = GetPlayerWorld();
        if (world == null)
        {
            Debug.LogWarning("Player is not in a world but tried to break a tile");
            return;
        }

        Debug.Log($"Breaking tile at {tilePos} in world {world.WorldId}");
        world.BreakTile(tilePos);
    }

    [ServerRpc]
    private void RequestPlaceTile(Vector3Int tilePos, string tileId)
    {
        // Validate range server-side
        Vector3 tileWorld = new Vector3(tilePos.x + 0.5f, tilePos.y + 0.5f, 0f);
        if (Vector2.Distance(transform.position, tileWorld) > interactRange) return;

        World world = GetPlayerWorld();
        if (world == null) return;

        world.PlaceTile(tilePos, tileId);
    }

    private World GetPlayerWorld()
    {
        if (!PlayerWorldTracker.Instance.TryGetWorld(Owner, out int worldId))
            return null;

        return WorldManager.Instance.GetWorld(worldId);
    }
}