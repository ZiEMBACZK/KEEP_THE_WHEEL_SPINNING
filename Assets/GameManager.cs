using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Unity.Cinemachine;

public class GameManager : NetworkBehaviour 
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private bool restartStarted;
    [SerializeField] public UnityEvent onSceneRestart;
    [SerializeField] public UnityEvent onSceneFirstStart;
    [SerializeField] public CinemachineCamera camera;

    void Start()
    {
        // Check if an instance of GameManager already exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Enforce the singleton pattern
        }
        onSceneFirstStart.Invoke();
    }
    public void StartGame()
    {
        Debug.Log("Game started!");
        // Add game-starting logic here
    }

    // Example method to demonstrate functionality
    public void EndGame()
    {
        Debug.Log("Game ended!");
        // Add game-ending logic here
    }
    public void RestartGame()
    {
        if(!restartStarted)
        {
            if(IsHost)
            {
                StartCoroutine(RestartGameCorutine(5));
            }
            else
            {
                if(IsOwner)
                {
                    RequestRestartSceneServerRpc();

                }
            }

        }
        restartStarted = true;
    }
    [ServerRpc]
    private void RequestRestartSceneServerRpc()
    {
        StartCoroutine(RestartGameCorutine(5));
    }
    private IEnumerator RestartGameCorutine(float waitBeforeSceneChange)
    {
        yield return new WaitForSeconds(waitBeforeSceneChange);
        restartStarted = false;
        onSceneRestart.Invoke();
        NetworkManager.Singleton.SceneManager.LoadScene("NGO_Setup", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
