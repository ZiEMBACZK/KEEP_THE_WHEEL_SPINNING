using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    public static PlayerSpawner Instance { get; private set; }

    private readonly Dictionary<ulong, GameObject> spawnedPlayers = new Dictionary<ulong, GameObject>();

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (IsHost)
        {
            SpawnPlayersAtGameStart();

        }
    }

    private void SpawnPlayersAtGameStart()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!spawnedPlayers.ContainsKey(clientId))
            {
                SpawnPlayer(clientId);
            }
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        // Instantiate and spawn the player prefab
        GameObject playerInstance = Instantiate(playerPrefab, transform.position, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        // Track the spawned player
        spawnedPlayers[clientId] = playerInstance;
    }

    public GameObject GetPlayer(ulong clientId)
    {
        // Return the player's GameObject if it exists
        if (spawnedPlayers.TryGetValue(clientId, out var playerObject))
        {
            return playerObject;
        }
        return null;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
