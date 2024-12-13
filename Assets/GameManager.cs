using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.UI;


public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private bool restartStarted;
    [SerializeField] public UnityEvent OnGameStart;
    [SerializeField] public UnityEvent OnPlayerWon;
    [SerializeField] public CinemachineCamera camera;
    [SerializeField] private bool allPlayersReady;
    public NetworkList<PlayerData> playerDataNetworkList;
    public event EventHandler OnPlayerDataNetworkListChanged;
    public event EventHandler OnAllPlayersReadyChanged;
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, int> playerScoreDictionary;
    public event EventHandler OnReadyChanged;
    private  float timer;
    [SerializeField]
    TextMeshProUGUI textMeshProUGUI;
    [SerializeField]
    TextMeshProUGUI scorePro;
    [SerializeField]
    public Transform[] spawnPoints;
    private readonly Dictionary<ulong, GameObject> spawnedPlayers = new Dictionary<ulong, GameObject>();
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Camera lobbyRotatingCamera;
    [SerializeField] public Camera playerCamera;
    [SerializeField] private RawImage rawImageOfPlayersSpinning;
    [SerializeField] public Transform planetTransform;
    int tick = 1;

    void Awake()
    {
        playerDataNetworkList = new NetworkList<PlayerData>(); //this is Network list that adds new player to the list on connection and holds values within Playerdata like 
        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerScoreDictionary = new Dictionary<ulong, int>();
        playerDataNetworkList.OnListChanged += InvokeOnListChangedEvent;
        // Check if an instance of GameManager already exists
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Enforce the singleton pattern
        }
    }
    private void Start()
    {
        
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallBack;

    }
    private void Update()
    {
        foreach (var kvp in playerScoreDictionary) 
        {
            Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");

            scorePro.text = $"Key: {kvp.Key}, Value: {kvp.Value}";
        }


        
    }
    private void DisplayTimer(string valueToDisplay)
    {
        textMeshProUGUI.gameObject.SetActive(true);
        textMeshProUGUI.text = valueToDisplay;
    }
   
    private void OnClientConnectedCallBack(ulong clientId)
    {
        if(IsHost)
        {
            playerDataNetworkList.Add(new PlayerData
            {
                clientId = clientId


            });

        }
    }
    private void InvokeOnListChangedEvent(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }
    public IEnumerator AnimateCamera()
    {
        FadeOut(1f);
        yield return new WaitForSeconds(1f);
        Sequence sequance = DOTween.Sequence();
        sequance.Append(lobbyRotatingCamera.transform.DOMove(playerCamera.transform.position, 4f));
        sequance.Join(lobbyRotatingCamera.transform.DORotateQuaternion(playerCamera.transform.rotation, 4f));
        sequance.AppendCallback(() =>
        {
            playerCamera.GetComponent<Camera>().enabled = true;

        });
    }
    private IEnumerator DisPlayCanvasTimer()
    {
        textMeshProUGUI.text = tick.ToString();
        yield return new WaitForSeconds(1);
        if(tick > 3)
        {
            //StartGameHere
            if(IsHost)
            {
                textMeshProUGUI.gameObject.SetActive(false);
                SpawnPlayersAtGameStart();
            }

            yield break;
        }
        else
        {
            tick++;
            StartCoroutine(DisPlayCanvasTimer());
        }
    }
    private void AllPlayersReadyToggle(bool value)
    {
        allPlayersReady = value;
        OnAllPlayersReadyChanged?.Invoke(this, EventArgs.Empty);

    }
    public void GameSoloStart()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }
    [ServerRpc(RequireOwnership = false)]
    public void AddReadyStateToPlayersDataServerRpc(bool value = false, ServerRpcParams serverRpcParams = default)
    {
        int playersReady = 0;
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == serverRpcParams.Receive.SenderClientId)
            {
                var player = playerDataNetworkList[i]; // Get a copy
                player.isReady = value; // Modify the copy
                playerDataNetworkList[i] = player; // Write it back to the list
            }
        }
        foreach (var player in playerDataNetworkList) 
        {
            if(player.isReady)
            {
                playersReady++;

            }
        }
        if(playersReady == playerDataNetworkList.Count)
        {
            allPlayersReady = true;
            textMeshProUGUI.gameObject.SetActive(true);
            StartCoroutine(DisPlayCanvasTimer());
            DisplayTimerForClientClientRpc();
        }
        
        
    }
    [ClientRpc]
    private void DisplayTimerForClientClientRpc()
    {
        StartCoroutine(DisPlayCanvasTimer());
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(bool value= false ,ServerRpcParams serverRpcParams = default)
    {
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId, value);
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = value;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                AllPlayersReadyToggle(false);

            }
            else
            {
                AllPlayersReadyToggle(true);
            }
        }

    }
    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId, bool value)
    {
        playerReadyDictionary[clientId] = value;
        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }
    [ServerRpc(RequireOwnership = false)]
    public void ChangeScoreServerRpc(ServerRpcParams serverRpcParams = default)
    {
        int currentScore = 0;
        if (playerScoreDictionary.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            currentScore = playerScoreDictionary[serverRpcParams.Receive.SenderClientId];

        }
        currentScore += 1;
        playerScoreDictionary[serverRpcParams.Receive.SenderClientId] = currentScore;
        ChangeScoreClientRpc(serverRpcParams.Receive.SenderClientId, currentScore);

    }
    [ClientRpc]
    private void ChangeScoreClientRpc(ulong clientid, int currentscore)
    {
        playerScoreDictionary[clientid] = currentscore;
    }
    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }
    public void SpawnPlayersAtGameStart()
    {
        int transformCounter = 0;
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!spawnedPlayers.ContainsKey(clientId))
            {
                Debug.Log(spawnPoints[transformCounter].gameObject.name);
                SpawnPlayer(clientId, spawnPoints[transformCounter]);
                transformCounter++;
            }
        }
    }

    private void SpawnPlayer(ulong clientId, Transform spawnPoint)
    {
        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position,  spawnPoint.rotation);
        playerInstance.transform.rotation = spawnPoint.transform.rotation;
        playerInstance.transform.position = spawnPoint.position;
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

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
    public void FadeOut(float duration)
    {
        rawImageOfPlayersSpinning.DOFade(0f, duration).SetEase(Ease.Linear);
    }


}

