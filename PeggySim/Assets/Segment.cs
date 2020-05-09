using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour
{
    public HingeJoint joint;

    void Start()
    {
       joint = GetComponent<HingeJoint>();
    }

    void Update(){
        if(joint.connectedBody==null || joint.connectedBody.gameObject.tag=="SegmentDead"){
           
            gameObject.tag = "SegmentDead";
        }
    }
}
