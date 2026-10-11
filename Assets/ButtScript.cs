using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtScript : MonoBehaviour
{
    private HingeJoint joint;
    private GameObject icon;
    private SphereCollider s;
    public bool connected=true;
    // Start is called before the first frame update
    void Start()
    {
        s = GetComponent<SphereCollider>();
        joint = GetComponent<HingeJoint>();
        icon = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        icon.transform.eulerAngles = new Vector3 (0,0,0);


        if(joint.connectedBody==null || joint.connectedBody.gameObject.tag!="Segment"){
            if(connected){
                icon.SetActive(true);
                transform.position = new Vector3(transform.position.x,transform.position.y+5,transform.position.z);

                transform.localScale = new Vector3(3*3.91f,3*3.91f,3*3.91f);
            }
            connected=false;

        }else{
            connected=true;
            // this should get set at reattachement pls
            //transform.localScale = new Vector3(3.91f,3.91f,3.91f);
            icon.SetActive(false);
            
        }
        
    }
}
