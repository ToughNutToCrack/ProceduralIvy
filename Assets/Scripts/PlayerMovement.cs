using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Unity.Profiling;
using UnityEngine;

// https://www.youtube.com/watch?v=f473C43s8nE 
public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMiltiplier;
    bool readyToJump;

    bool disableMovement;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb; // Ref to rigidbody
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        readyToJump = true;

        disableMovement = false;

    }

    // Update is called once per frame
    void Update()
    {   
        // Ground Check
        grounded = Physics.Raycast(transform.position, Vector3.down ,playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {   if (!disableMovement)
        {
            MovePlayer();
        }
       
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetMouseButtonDown(1))
        {
            disableMovement = !disableMovement;
           
        }

        // When to jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            Console.WriteLine("JUMP");
            readyToJump = false;

            Jump();

            // Continue to jump if you keep pressing jumpkey
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward* verticalInput + orientation.right* horizontalInput;
       
        if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f*airMiltiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > moveSpeed) 
        {
         Vector3 limitedVel = flatVel.normalized * moveSpeed;
         rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity 
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
