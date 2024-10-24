using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject playerPrefab; // Assign the player prefab in the Inspector

    private void Start()
    {
        OnSceneChanged();
    }

    private void OnSceneChanged()
    {
        // Only the server (host) can spawn player objects
        if (IsHost)
        {
            // Loop through all connected clients, including the host
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                SpawnPlayerForClient(client.ClientId);
            }

        }
        
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        // Instantiate the player prefab
        GameObject playerInstance = Instantiate(playerPrefab);

        // Spawn the player object and associate it with the client
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
