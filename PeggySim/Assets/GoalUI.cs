using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalUI : MonoBehaviour
{
    private Text GoalText;

    void Start()
    {
        GoalText = GetComponent<Text>();
        
    }

    
    void Update()
    {
        GoalText.text = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameInfo>().getGoal().ToString();
    }
}
