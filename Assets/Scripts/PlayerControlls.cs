using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

//  Player Controlls Features:
//      Move left and right
//      Jump when on ground
//      Wall jump once, before landing on ground again
//          Wall jump will make player lose control for a short while

public class PlayerControlls : MonoBehaviour
{
    [Tooltip("Characters speed left and right")]
    public float horizontalSpeed = 10.0f;
    [Tooltip("Force applied when jumping")]
    public float jumpForce = 1600.0f;
    [Tooltip("Force pushing player away from the wall when wall jumping")]
    public float wallJumpHorizontalForce = 500.0f;
    [Tooltip("Timespan seconds player loses control after wall jumping")]
    public float wallJumpControlLossTime = 3.0f;

    private float horizontalInput = 0;
    private float jumpInput = 0;
    private bool wallJumpUsed = false;
    private bool playerInAir = true;
    private bool playerControlLess = false;

    private Rigidbody2D rigidbody;
    private float playerHalfWidth;
    private float playerHalfHight;

    private void Awake()    //set up predetermined variables
    {
        rigidbody = GetComponent<Rigidbody2D>();
        Vector3 localScale = transform.localScale;
        playerHalfWidth = localScale.x / 2;
        playerHalfHight = localScale.y / 2;
    }

    private void FixedUpdate()
    {
        if (playerControlLess)  //No need to check player controls if player is control less
            return;
        
        //get player input
        horizontalInput = Input.GetAxis("Horizontal");
        jumpInput = Input.GetAxis("Jump");

        //do horizontal output
        if (horizontalInput != 0)
        {
            rigidbody.velocity *= Vector2.up;   //set x velocity from wall jump to zero, to not add to horizontal input
            transform.Translate(horizontalInput * Time.deltaTime * horizontalSpeed * Vector3.right);
        }

        
        //do jump output
        if (jumpInput == 0) //No need to test jump conditions if jump key not pressed
            return;

        if (playerInAir)
        {
            if (CheckForFloor())        //if touching ground reset playerInAir, and wallJumpsUsed
            {
                playerInAir = false;
                wallJumpUsed = false;
            }

            if (wallJumpUsed)       //no need to test wall jump conditions if wall jump is used
                return;
            if (CheckForWall(Vector2.right))        //Wall jump if wall jumps not used, player in air, and space pressed, and is next to wall
            {
                Jump(Vector2.left * wallJumpHorizontalForce);
                wallJumpUsed = true;
                StartCoroutine(ControllLossTimer());
            }
            else if (CheckForWall(Vector2.left))
            {
                Jump(Vector2.right * wallJumpHorizontalForce);
                wallJumpUsed = true;
                StartCoroutine(ControllLossTimer());
            }
        }
        else
        {
            Jump(Vector2.zero);     //Jump normal if player is not in the air, and space pressed
        }
    }

    private void Jump(Vector2 additionalForce) //jumping
    {
        rigidbody.velocity = Vector2.zero;
        rigidbody.AddForce(Vector2.up * jumpForce + additionalForce, ForceMode2D.Impulse);
        playerInAir = true;
    }

    private bool CheckForWall(Vector2 direction)    //returns true if there is platform on the side of the player
    {
        LayerMask layerMask = LayerMask.GetMask("Platform");
        RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, direction, playerHalfWidth + 0.1f, layerMask);
        return raycastHit.collider != null;
    }

    private bool CheckForFloor()    //returns true if player feet in contact with platform
    {
        LayerMask layerMask = LayerMask.GetMask("Platform");
        RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, Vector2.down, playerHalfHight + 0.1f, layerMask);
        return raycastHit.collider != null;
    }

    private IEnumerator ControllLossTimer() //sets player controlless to true for a specified time
    {
        playerControlLess = true;
        yield return new WaitForSeconds(wallJumpControlLossTime);
        playerControlLess = false;
    }
}