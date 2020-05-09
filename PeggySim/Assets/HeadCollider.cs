using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadCollider : MonoBehaviour
{
    public bool dead=false;
    private Peggy parent;

    // Start is called before ur butt
        private void OnTriggerEnter(Collider other)
    {
        parent.handleCollision(other.gameObject);
        //if(other!=null){
        if(other.tag == "Segment" && other.name !="FirstBody"){

            dead=true;
            //Destroy(other.GetComponent<HingeJoint>().connectedBody.gameObject);
            other.GetComponent<HingeJoint>().connectedBody = null;
            
        }
        //}    
    }
    void Start()
    {
        parent = this.transform.parent.GetComponent<Peggy>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
