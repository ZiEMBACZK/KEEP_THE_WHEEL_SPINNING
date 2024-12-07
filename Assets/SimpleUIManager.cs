using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimpleUIManager : NetworkBehaviour
{
    bool shouldJoinLobby;
    private void OnEnable()
    {
        LobbyManager.OnLobbyException += HandleLobbyException;
        LobbyManager.Instance.OnJoinedLobby += ChangeScene;
    }

    private void OnDisable()
    {
        LobbyManager.OnLobbyException -= HandleLobbyException;
        LobbyManager.Instance.OnJoinedLobby -= ChangeScene;
    }
    UnityEvent startGameEvent;
    private void Awake()
    {

        GetComponent<Button>().onClick.AddListener(() => {
            TryQuickJoin();
        });
        shouldJoinLobby = true;
    }
    private void HandleLobbyException(Exception e)
    {
        if (e is LobbyServiceException lobbyException)
        {
            if (lobbyException.Reason == LobbyExceptionReason.NoOpenLobbies)
            {
                if (shouldJoinLobby)
                {
                    LobbyManager.Instance.CreateLobby("lobbyName", 2, false);
                }
            }
            else
            {
                Debug.LogError($"Unexpected lobby error: {lobbyException.Message}");
            }
        }
        else
        {
            Debug.LogError($"Unhandled exception: {e.Message}");
        }
    }
    private void ChangeScene(object sender, LobbyManager.LobbyEventArgs e)
    {
        Debug.Log(e.lobby.LobbyCode);
        StartCoroutine(WaitBeforeStartingLobby(e));
    }
    private IEnumerator WaitBeforeStartingLobby(LobbyManager.LobbyEventArgs e)
    {
        yield return new WaitUntil(() => e.lobby.AvailableSlots == 0);
        yield return new WaitForSeconds(5f);
        if(IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("NGO_Setup", LoadSceneMode.Single);

        }
    }
    private void TryQuickJoin()
    {
        LobbyManager.Instance.QuickJoinLobby();
    }
    private void Update()
    {
    }
    private void StartGameBehaviour()
    {
        //LobbyManager.Instance.OnLobbyListChanged();
    }
    private void ChageSceneName()
    {

    }
}
