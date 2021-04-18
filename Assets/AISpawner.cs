using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawner : MonoBehaviour
{
    public float spawnRate;
    public GameObject aICustomerPrefab;
    public GameObject entranceManager;
    private float nextSpawnTimer;
    private float timer;
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > nextSpawnTimer)
        {
            nextSpawnTimer = timer + spawnRate;
            Vector3 spawnLocation = entranceManager.transform.GetChild(Random.Range(0, entranceManager.transform.childCount)).position;
            GameObject aICustomer = Instantiate(aICustomerPrefab, spawnLocation, Quaternion.identity);
            aICustomer.transform.parent = transform;
            spawnRate = Random.Range(3f, 10f);
        }
    }
}
