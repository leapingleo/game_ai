using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockDemo : MonoBehaviour
{
    public Agent agentPrefab;
    List<Agent> agents = new List<Agent>();
    public Transform target;

    public int agentCount;
    public float neighbourRadius = 2f;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < agentCount; i++)
        {
            Agent newAgent = Instantiate(
                agentPrefab,
                Random.insideUnitCircle * 0.5f * 15f,
              //  new Vector3(Random.Range(-8f, 8f), Random.Range(-8f, 8f), 0f),
                Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)),
                transform);
            agents.Add(newAgent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Agent item in agents)
        {
        
            List<Transform> neighbours = GetNearbyObjects(item);

            item.run(neighbours);
            item.UpdateMovement();
        }
    }

    List<Transform> GetNearbyObjects(Agent agent)
    {
        List<Transform> neighbours = new List<Transform>();
        Collider2D[] neighboursColliders = Physics2D.OverlapCircleAll(agent.transform.position, neighbourRadius);
        foreach (Collider2D c in neighboursColliders)
        {
            if (c != agent.AgentCollider)
            {
                neighbours.Add(c.transform);
            }
        }
        return neighbours;
    }
}
