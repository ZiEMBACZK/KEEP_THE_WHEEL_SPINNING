using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Multiplayer;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
public class TestLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private float heartbeatTimer;
    private string playerName;  // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerName = "TestName" + UnityEngine.Random.Range(10, 99);
        Debug.Log(playerName);
    }
    private void Update()
    {
        HandleLobbyHeartBeat();
    }
    private async void HandleLobbyHeartBeat()
    {
        if(hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer < 0 )
            {
                float hearbeatTimerMax = 15;
                heartbeatTimer = hearbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = true,
                Player = GetPlayer()
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            hostLobby = lobby;


            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + lobby.Id + " " + lobby.LobbyCode);

            PrintPlayers(hostLobby);

        }
        catch(LobbyServiceException ex) 
        {
            Debug.Log(ex);
        }
    }
    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter> {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)

                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();


            Debug.Log("Lobbies found: " + queryResponse.Results);
            foreach(Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }

        }
        catch(LobbyServiceException e ) 
        {
            Debug.Log(e);
        }
    }
    private async void JoinLobbyByCode(string lobbyCode)
    {
        try {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);


            Debug.Log("Joined lobby with code " + lobbyCode);

            PrintPlayers(joinedLobby);

        }
        catch(LobbyServiceException e ) 
        {
            Debug.Log(e);
        }
    }
    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject> { { "playerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) } }
        };
    }
    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();

        }
        catch(LobbyServiceException e )
        {
            Debug.Log(e);
        }
        
    }
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in lobby " + lobby.Name);
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }


}
