using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadCollider : MonoBehaviour
{
    public bool dead=false;
    // Start is called bef
        private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Segment" && other.name !="FirstBody"){

            dead=true;
            other.GetComponent<CharacterJoint>().connectedBody = null;
        }
            
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
