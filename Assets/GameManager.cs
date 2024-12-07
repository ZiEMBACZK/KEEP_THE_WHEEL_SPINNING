using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
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
        Debug.Log("ShouldRestartScene");
        restartStarted = false;
        onSceneRestart.Invoke();
        NetworkManager.Singleton.SceneManager.LoadScene("NGO_Setup", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    private void StartRestartGameCourutine()
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
    private void FindCamera()
    {
        if(camera == null)
        {
            FindAnyObjectByType(typeof(CinemachineCamera));
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeSceneReferences();
    }
    private void InitializeSceneReferences()
    {
        camera = FindAnyObjectByType<CinemachineCamera>();                              //this is basicly a crime but with this amout of object in scene it should be ok
                                                                                        //we propably want to create some kind referances holder that is singelTone but without Dont destroy on load
                                                                                        //its propably less of a crime but idk 

        if (camera != null)
        {
            Debug.Log("Main Camera found and reference updated.");
        }
        else
        {
            Debug.LogWarning("Main Camera not found in the scene!");
        }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
