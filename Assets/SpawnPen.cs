using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPen : MonoBehaviour
{
	public GameObject pen;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void spawnPen(){

    	Instantiate(pen);
    }
}
