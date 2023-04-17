using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attach this script to the players camera (The camera should be in the camera holder object)
public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    // Attach Orientation Object  (just create Empty Object, and put in the players object tree)
    public Transform orientation; // Get ref to players orientation

    float xRotation;
    float yRotation;

    bool disableMovement;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        disableMovement = false;
    }

    // Update is called once per frame
    void Update()
    {
        MyInput();

        if (!disableMovement)
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // rotate cam and orientation 
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
        
    }

    private void MyInput()
    {
      

        if (Input.GetMouseButtonDown(1))
        {
            disableMovement = !disableMovement;

            if (disableMovement)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

                 
            
        }


    }
}
