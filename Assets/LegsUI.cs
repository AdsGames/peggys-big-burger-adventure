using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LegsUI : MonoBehaviour
{
     private Text boostText;
    void Start()
    {
        boostText = GetComponent<Text>();
    }

    void Update()
    {
        if(GameObject.FindGameObjectWithTag("Butt").GetComponent<ButtScript>().connected){
            boostText.text = "Attached";
        }else
        {
            boostText.text = "Missing";
        }
    }

}
