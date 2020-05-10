using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandBrakeUI : MonoBehaviour
{
    private Text handBrakeText;

    void Start()
    {
        handBrakeText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(GameObject.FindGameObjectWithTag("Head").GetComponent<Movement>().currentHandBrakeMeter.ToString().Length > 2)
            handBrakeText.text = GameObject.FindGameObjectWithTag("Head").GetComponent<Movement>().currentHandBrakeMeter.ToString().Substring(0, 2).Replace(".","").Replace("-","");
    }
}
