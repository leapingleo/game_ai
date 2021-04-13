using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{

    //public GameObject[] objects;
    public int numberToSpawn;
    public List<GameObject> spawnPool;
    public GameObject quad;

    // Start is called before the first frame update
    void Start()
    {
        //int random = Random.Range(0, objects.Length);
        //Instantiate(objects[random], transform.position, Quaternion.identity);
        spawnObjects();
    }

    public void spawnObjects()
    {
        int randomItem = 0;
        GameObject toSpawn;
        MeshCollider c = quad.GetComponent<MeshCollider>();

        float screenX, screenY;
        Vector2 pos;

        for (int i = 0; i < numberToSpawn; i++)
        {
            randomItem = Random.Range(0, spawnPool.Count);
            toSpawn = spawnPool[randomItem];

            screenX = Random.Range(c.bounds.min.x, c.bounds.max.x);
            screenY = Random.Range(c.bounds.min.y, c.bounds.max.y);
            pos = new Vector2(screenX, screenY);

            Instantiate(toSpawn, pos, toSpawn.transform.rotation);
        }
    }
}
