using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollInitializer : MonoBehaviour
{
    public GameObject paperRollPrefab;
    // Start is called before the first frame update
    void Start()
    {
        InitializePaperRolls();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void InitializePaperRolls()
    {
        int rolls_per_layer = 5;
        float x_offset = (6f - 3.7f) / 5;

        // for (int i = 0; i < rolls_per_layer; i++)
        // {
        //     int n1 = Random.Range(0, 100);
        //     int n2 = Random.Range(0, 100);
        //
        //     if (n1 > 30)
        //     {
        //         GameObject roll1 = Instantiate(paperRollPrefab, new Vector3(transform.position.x - 0.85f + i * x_offset, transform.position.y + 0.25f, 0), Quaternion.Euler(Vector3.zero));
        //         roll1.transform.parent = transform;
        //     }
        //
        //     if (n2 > 30)
        //     {
        //         GameObject roll2 = Instantiate(paperRollPrefab, new Vector3(transform.position.x - 0.85f + i * x_offset, transform.position.y + -0.5f, 0), Quaternion.Euler(Vector3.zero));
        //         roll2.transform.parent = transform;
        //     }
        // }
        
        int n1 = Random.Range(0, 100);
        if (n1 % 2 == 0)
        {
            GameObject roll1 = Instantiate(paperRollPrefab, new Vector3(transform.position.x - 0.85f + 0 * x_offset, transform.position.y + 0.25f, 0), Quaternion.Euler(Vector3.zero));
            roll1.transform.parent = transform;
        }
    }
}
