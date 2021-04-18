using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Text rollOnHandText;
    public Text timerText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timerText.text = "" + (int)GroupManager.Instance.timer;
        rollOnHandText.text = "" + GroupManager.Instance.groupTotalRolls;
    }
}
