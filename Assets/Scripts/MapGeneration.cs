using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{

    //public GameObject[] objects;
    public int numberToSpawn;
    public List<GameObject> spawnPool;
    public GameObject quad;
    public LayerMask m_LayerMask;


    // Start is called before the first frame update
    void Start()
    {
        //int random = Random.Range(0, objects.Length);
        //Instantiate(objects[random], transform.position, Quaternion.identity);
        spawnObjects();
    }

    void Update()
    {
        
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
            int j = 0;
            for (; j < numberToSpawn; j++)
            {
                // Take a random position POS
                // Check colision of POS with all objects,
                //    if space available: spawn
                randomItem = Random.Range(0, spawnPool.Count);
                toSpawn = spawnPool[randomItem];

                screenX = Random.Range(c.bounds.min.x, c.bounds.max.x);
                screenY = Random.Range(c.bounds.min.y, c.bounds.max.y);
                pos = new Vector2(screenX, screenY);

                GameObject spawnedObject = Instantiate(toSpawn, pos, toSpawn.transform.rotation) ;
                Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale * 5, Quaternion.identity, m_LayerMask);
                if (hitColliders.Length != 0)
                {
                    Destroy(spawnedObject);
                }
            }
                if (j == numberToSpawn)
                {
                    break;
                }
        }
    }
    //private void DestroyObjects()
    //{
    //    foreach (GameObject o in GameObject.FindGameObjectsWithTag("Spawnable"))
    //    {
    //        Destroy(o);
    //    }
    //}
}
