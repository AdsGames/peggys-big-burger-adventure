using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour
{
    public HingeJoint joint;
    float age;
    void Start()
    {
       joint = GetComponent<HingeJoint>();
       age = Time.time;
       
    }
    public float getAge(){
        return age;
    }

    void Update(){
        if(joint!=null){
            if(joint.connectedBody==null || joint.connectedBody.gameObject.tag=="SegmentDead"){
           
                gameObject.tag = "SegmentDead";
                Destroy(joint);
            }
        }
    }
}
