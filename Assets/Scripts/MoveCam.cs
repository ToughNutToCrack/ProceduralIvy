using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attach this script to the camear holder object
public class MoveCam : MonoBehaviour
{   
    // This will be the camera position object that's in the player object tree, which will help relocate the cameara to the player.
    public Transform cameraPosition; // get ref to player orientation
    // Start is called before the first frame update

    public float lerpFactor = 0.5f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, cameraPosition.position, lerpFactor) ;
    }
}
