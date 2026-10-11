using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAbilitesUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!GameObject.FindGameObjectWithTag("Butt").GetComponent<ButtScript>().connected){
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(true);


        }else{
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(2).gameObject.SetActive(true);
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
