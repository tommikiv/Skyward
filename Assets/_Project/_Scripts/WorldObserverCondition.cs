using FishNet.Connection;
using FishNet.Object;
using FishNet.Observing;
using UnityEngine;

[CreateAssetMenu(menuName = "Observers/World Condition")]
public class WorldObserverCondition : ObserverCondition
{
    public override bool ConditionMet(NetworkConnection connection, bool alreadyAdded, out bool notProcessed)
    {
        notProcessed = false;
        Debug.Log($"ConditionMet called for: {NetworkObject.name}");

        WorldObject worldObject = NetworkObject.GetComponent<WorldObject>();
        if (worldObject == null) return true;

        if (!PlayerWorldTracker.Instance.TryGetWorld(connection, out int playerWorldId))
        {
            Debug.Log($"Connection {connection.ClientId} not in any world yet");
            return false;
        }

        bool met = worldObject.WorldId == playerWorldId;
        Debug.Log($"Observer check: object worldId={worldObject.WorldId}, player worldId={playerWorldId}, met={met}");
        return met;
    }

    public override ObserverConditionType GetConditionType()
        => ObserverConditionType.Normal;
}