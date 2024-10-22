using UnityEngine;
using UnityEngine.InputSystem;

public class FauxGravitySingleton : MonoBehaviour
{
    // Singleton instance
    public static FauxGravitySingleton Instance { get; private set; }
    [SerializeField] private Transform _planet;


    public Transform PlanetTransfom 
    {
        get
        {
            return _planet;
        }
        set
        {
            _planet = value;
        }
    }
    private void Awake()
    {
        // Check if an instance already exists and enforce the singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.Log("SOMETHING WENT TERIBLE WRONG PLS CHECK THIS ");
            Destroy(gameObject); // Destroy duplicate instance
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: keep the instance across scenes
        }
    }
}
