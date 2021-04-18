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
            int retries = 0;
            


            while (retries < numberToSpawn)
            {
                // Take a random position POS
                // Check colision of POS with all objects,
                //    if space available: spawn
                randomItem = Random.Range(0, spawnPool.Count);



                toSpawn = spawnPool[randomItem];



                screenX = Random.Range(c.bounds.min.x, c.bounds.max.x);
                screenY = Random.Range(c.bounds.min.y, c.bounds.max.y);
                pos = new Vector2(screenX, screenY);



                
                Collider2D[] hitColliders = Physics2D.OverlapBoxAll(pos, new Vector2(3.7f, 2.8f), 0.0f, LayerMask.GetMask("Default"));
                GameObject spawnedObject = Instantiate(toSpawn, pos, toSpawn.transform.rotation);
                spawnedObject.transform.parent = transform;
                if (hitColliders.Length != 0)
                {
                    Destroy(spawnedObject);
                    retries++;
                }
                else
                {

                    break;
                }
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
