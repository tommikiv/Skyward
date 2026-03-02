using FishNet.Object;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public void RequestJoinWorld(int worldId)
    {
        if (!IsOwner) return;
        Debug.Log($"Requesting to join world {worldId} from client");
        ServerRpcJoinWorld(worldId);
    }

    [ServerRpc]
    private void ServerRpcJoinWorld(int worldId)
    {
        Debug.Log($"Received request to join world {worldId} on server from player {Owner.ClientId}");
        GameManager.Instance.MovePlayerToWorld(Owner, worldId);
    }
}
