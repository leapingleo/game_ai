using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerFlock : MonoBehaviour
{
    private Shopper[] customers;
    public Shopper customer1;
    public Shopper customer2;
    public Shopper customer3;
    public Shopper customer4;
    public Shopper customer5;

    public float neighbourRadius = 2f;
    public int size;

    // Start is called before the first frame update
    void Start()
    {
        customers = new Shopper[size];
        customers[0] = customer1;
        customers[1] = customer2;
        customers[2] = customer3;
        customers[3] = customer4;
        customers[4] = customer5;
    }


    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < size; i++)
        {

            List<Transform> nearbyMembers = GetNearbyObjects();

            customers[i].GetComponent<Shopper>().SetNearbyMembers(nearbyMembers);
        }
    }

    List<Transform> GetNearbyObjects()
    {
        List<Transform> neighbours = new List<Transform>();
        for (int i = 0; i < size; i++)
        {
            neighbours.Add(customers[i].transform);
        }
        return neighbours;
    }

    //List<Transform> GetNearbyObjects(Shopper agent)
    //{
    //    List<Transform> neighbours = new List<Transform>();
    //    Collider2D[] neighboursColliders = Physics2D.OverlapCircleAll(agent.transform.position, neighbourRadius);
    //    foreach (Collider2D c in neighboursColliders)
    //    {
    //        if (c != agent.GetComponent<Shopper>().AgentCollider)
    //        {
    //            neighbours.Add(c.transform);
    //        }
    //    }
    //    return neighbours;
    //}
}
