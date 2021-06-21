using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Status : MonoBehaviour
{
    private AI_Agent AI_Agent;
    private int coinsCollected;
    //private UnityEngine.UI.Text scoreDisplay;

    // Use this for initialization
    void Start()
    {
        coinsCollected = 0;
        AI_Agent = GetComponent<AI_Agent>();
        //scoreDisplay = GameObject.Find("Canvas").transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
    }

    // Update is called once per frame
    void Update()
    {
        //scoreDisplay.text = "Coins: " + coinsCollected;
    }

    public void CollectTP(GameObject coin)
    {
        coinsCollected++;
        //Destroy(coin);
        coin.SetActive(false);
        AI_Agent.coinCollected();
    }
}
