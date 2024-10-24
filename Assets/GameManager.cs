using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //LobbyManager.Instance.OnJoinedLobby += StartGameScene;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void StartGameScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("NGO_Setup", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
