using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawner : MonoBehaviour
{
    public float spawnRate;
    public GameObject aICustomerPrefab;
    public GameObject entranceManager;
    private float nextSpawnTimer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextSpawnTimer)
        {
            nextSpawnTimer += spawnRate;
            Vector3 spawnLocation = entranceManager.transform.GetChild(Random.Range(0, entranceManager.transform.childCount)).position;
            GameObject aICustomer = Instantiate(aICustomerPrefab, spawnLocation, Quaternion.identity);
            aICustomer.transform.parent = transform;
        }
    }
}
