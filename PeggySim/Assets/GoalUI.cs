using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalUI : MonoBehaviour
{
    private Text GoalText;
    private int highscore=1;
    void Start()
    {
        GoalText = GetComponent<Text>();
        
    }

    
    void Update()
    {
        int new_highscore  = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameInfo>().getSegments();
        if(new_highscore>highscore)
            highscore=new_highscore;
        GoalText.text =highscore.ToString();
    }
}
