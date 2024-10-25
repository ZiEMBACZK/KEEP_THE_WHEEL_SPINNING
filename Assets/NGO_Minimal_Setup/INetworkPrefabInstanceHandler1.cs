using Unity.Netcode;
using UnityEngine;

public interface INetworkPrefabInstanceHandler1
{
    NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation);
    void Destroy(NetworkObject networkObject);
}
