using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostUI : MonoBehaviour
{
    private Text boostText;
    void Start()
    {
        boostText = GetComponent<Text>();
    }

    void Update()
    {
        if(GameObject.FindGameObjectWithTag("Head").GetComponent<Movement>().currentBoostMeter.ToString().Length > 1)
            boostText.text = GameObject.FindGameObjectWithTag("Head").GetComponent<Movement>().currentBoostMeter.ToString().Substring(0, 2).Replace(".", "").Replace("-", "");
    }
}
