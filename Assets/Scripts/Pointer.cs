using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    // Start is called before the first frame update

   // private GameObject playerCam;
    //public float pointerDist = 0.5f;
    //public float lerpFactor = 0.02f;

    public Texture2D targetTexture;
    float size;
    float centerX;
    float centerY;
    Rect rect;


    void Start()
    {
       // playerCam = GameObject.Find("PlayerCam");
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            size = Mathf.Min(Screen.width - 200, Screen.height - 200) * 0.1f;
            Console.WriteLine("Size:", size);
        }
        else
        {
            size = Mathf.Min(Screen.width, Screen.height) * 0.1f;
        }
    }

    void OnGUI()
    {
        Event m_Event = Event.current;

        centerX = Screen.width / 2f;
        centerY = Screen.height / 2f;
        //float size = Mathf.Min(Screen.width-200, Screen.height-200) * 0.1f;

        rect = new Rect(centerX - size / 2f, centerY - size / 2f, size, size);

        GUI.DrawTexture(rect, targetTexture);
    }
}
