using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnScript : MonoBehaviour
{
    // Start is called before the first frame update
    public float respawnTime=5;
    public float spawnHeight=1;
    private float time=99999;
    public GameObject item;
    private GameObject spawnedItem;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(spawnedItem==null){
            time+=Time.deltaTime;
        }
        
        if(time>=respawnTime){
            time=0;
            spawnedItem = Instantiate(item);
 

            spawnedItem.transform.position = new Vector3(transform.position.x,spawnHeight,transform.position.z);

            print("borger");
        }
    }
}
