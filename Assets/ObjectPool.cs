using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    // Singleton instance
    public static ObjectPool Instance;

    // Prefab to pool
    [SerializeField] private GameObject objectPrefab;

    // Initial pool size
    [SerializeField] private int initialPoolSize = 20;

    // Queue to hold available objects
    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Initialize the pool with inactive objects
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewObject();
        }
    }

    // Create a new object and add it to the pool
    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(objectPrefab);
        obj.SetActive(false);
        poolQueue.Enqueue(obj);
        return obj;
    }

    /// <summary>
    /// Retrieves an object from the pool. If the pool is empty, a new object is instantiated.
    /// </summary>
    /// <returns>GameObject from the pool</returns>
    public GameObject GetObject()
    {
        if (poolQueue.Count > 0)
        {
            GameObject obj = poolQueue.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Optionally, increase pool size dynamically
            return CreateNewObject();
        }
    }

    /// <summary>
    /// Returns an object back to the pool.
    /// </summary>
    /// <param name="obj">GameObject to return</param>
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        poolQueue.Enqueue(obj);
    }
}