using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private GameInfo info;
    public float segmentScale = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        info = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameInfo>();

    }

    // Update is called once per frame
    void Update()
    {
    
        transform.localPosition = new Vector3(0,3.5f + (info.getSegments()*segmentScale),5.5f +(info.getSegments()*segmentScale));
    }
}
