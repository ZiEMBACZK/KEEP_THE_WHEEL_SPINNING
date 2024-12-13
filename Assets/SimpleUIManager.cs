using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimpleUIManager : NetworkBehaviour
{
    [SerializeField] private Button CreateLobbyButton;
    [SerializeField] private Button QuickJoinButton;
    [SerializeField] private Toggle ReadyCheck;
    [SerializeField] bool allPlayersReady;
    [SerializeField] public UnityEvent tankDisApearEffectEvent;
    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e)
    {
        CreateLobbyButton.gameObject.SetActive(true);
        QuickJoinButton.gameObject.SetActive(true);
        ReadyCheck.gameObject.SetActive(false);
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        FadeOut(QuickJoinButton, 0.5f);
        FadeOut(CreateLobbyButton, 0.5f);

        
        ReadyCheck.gameObject.SetActive(true);


    }
    private void LobbyManager_onAuthenticated(object sender, EventArgs e)
    {
        FadeInButton(CreateLobbyButton, 1f);
        FadeInButton(QuickJoinButton, 1f);
        CreateLobbyButton.gameObject.SetActive(true);
        QuickJoinButton.gameObject.SetActive(true);

        ReadyCheck.gameObject.SetActive(false);


    }
    private void FadeInButton(Button button, float duration)
    {
        button.GetComponentInChildren<Image>().DOFade(1f, duration);
        button.GetComponentInChildren<TextMeshProUGUI>().DOFade(1f, duration);
    }
    private void FadeOut(Button button, float duration)
    {
        button.GetComponentInChildren<Image>().DOFade(0f, duration);
        
        button.GetComponentInChildren<TextMeshProUGUI>().DOFade(0f, duration).OnComplete(() =>
        {

            button.gameObject.SetActive(false);

        });
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
    }
    UnityEvent startGameEvent;
    private void Awake()
    {

        CreateLobbyButton.onClick.AddListener(() =>
        {
            OnCreateLobbyClicked();
        });
        QuickJoinButton.onClick.AddListener(() =>
        {
            OnQuickJoinClicked();
        });
        ReadyCheck.onValueChanged.AddListener(ToggleReadyCheck);
    }
    private void Start()
    {
        CreateLobbyButton.gameObject.SetActive(false);
        QuickJoinButton.gameObject.SetActive(false);
        ReadyCheck.gameObject.SetActive(false);
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
        LobbyManager.Instance.OnAutheticated += LobbyManager_onAuthenticated;

    }
    private void OnCreateLobbyClicked()
    {
        LobbyManager.Instance.CreateLobby("LobbyName", 2, false);
        
    }
    private void OnQuickJoinClicked()
    {
        LobbyManager.Instance.QuickJoinLobby();
    }
    private void ToggleReadyCheck(bool isReady)
    {
        // GameManager.Instance.SetPlayerReadyServerRpc(isReady);
       GameManager.Instance.AddReadyStateToPlayersDataServerRpc(isReady);
    }


}

