using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCubesrutine : MonoBehaviour
{
    public GameObject cube;
    public List<GameObject> listOfBullets;
    float speed = 5;
    int currentCubes;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Current Cuebes" + currentCubes);
            for (int i = currentCubes; i < 30; i++)
            {
                listOfBullets.Add(Instantiate(cube, transform.position + new Vector3(Random.Range(0f,5f),Random.Range(0,3f) ,0), Quaternion.identity));
                currentCubes = i +1;
                Debug.Log("i" + i);
            }

        }
        if (listOfBullets != null)
        {
            for (int i = 0; listOfBullets.Count > i; i++)
            {
                listOfBullets[i].transform.position += listOfBullets[i].transform.forward * speed * Time.deltaTime;
            }
        }
    }
}
