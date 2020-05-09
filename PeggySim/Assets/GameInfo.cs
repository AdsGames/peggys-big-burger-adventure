using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public int getSegments()
    {
        return GameObject.FindGameObjectsWithTag("Segment").Length;
    }
}
