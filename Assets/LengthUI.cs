using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LengthUI : MonoBehaviour
{
    private Text LengthText;

    void Start()
    {
        LengthText = GetComponent<Text>();
    }

    void FixedUpdate()
    {
        LengthText.text = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameInfo>().getSegments().ToString();
    }
}
