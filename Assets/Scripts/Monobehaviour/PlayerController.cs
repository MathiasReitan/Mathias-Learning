using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;

/*  Gives player ability to be controlled
    Compatible with the Platform prefab
    Player-Controls Features:
        values can be set in inspector realtime
        Move left and right
        Jump when on ground
            If jump is pressed just before hitting the ground it will still be detected
        Wall jump
            Wall jump will make player lose control for a short while
            Cannot wall jump same direction more than once (must shift left right)
        On moving platforms, will follow platform dynamically
*/

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [Tooltip("Max speed horizontally")]
    public float maxHorizontalSpeed = 12.0f;
    [Tooltip("Seconds it takes to reach full speed")]
    public float accelerationTime = 0.1f;
    [Tooltip("Seconds it takes to lose all speed")]
    public float decelerationTime = 0.2f;
    [Header("Jumping")] 
    [Tooltip("Force applied upwards when jumping")]
    public float jumpForce = 1600.0f;
    [Tooltip("Seconds after jump pressed it will still register a jump")]
    public float jumpExtraDetectionTime = 0.2f;
    [Header("Wall Jumping")] 
    [Tooltip("Horizontal force applied when wall jumping")]
    public float wallJumpHorizontalForce = 550.0f;
    [Tooltip("Seconds player loses control after wall jumping")]
    public float wallJumpControlLossTime = 0.5f;
    
    private float horizontalInput = 0;
    private float horizontalForce = 0;
    private float secondsLastJumpPress = 0;
    private bool playerControlLess = false;
    private Vector2 lastWallJumpDirection = Vector2.zero;
    private bool isTouchingFloor = false;
    
    private Rigidbody2D rigidbody2D;
    private LayerMask platformLayerMask;
    private float playerHalfHight;
    private float playerHalfWidth;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        platformLayerMask = LayerMask.GetMask("Platform");
        playerHalfHight = transform.localScale.y / 2;
        playerHalfWidth = transform.localScale.x / 2;
    }

    private void Start()
    {
        secondsLastJumpPress = jumpExtraDetectionTime; //set to avoid player jumping when scene loaded
    }

    private void OnWalk(InputValue value)
    {
        //get horizontal input
        horizontalInput = value.Get<float>();
    }

    private void OnJump(InputValue value)
    {
        
        // Handy info: du kan faktisk bruke fuksjonen TouchingFloor()
        // direkte i if statementen uten å binde den til en bool variabel. da den allerede er en bool.
        
        /*
         * Eksempel:
         * if (!TouchingFloor() && lastWallJumpDirection != Vector2.left && TouchingWall(Vector2.right)
         * vil fungere helt likt som grounded boolen din.
         */
        //Kjekkt å vite, endte opp med en delt variabel for update og OnJump, med noen forandringer i scriptet ¯\_(ツ)_/¯

        //do wall jump if possible
        //    (normal wall jump is in update because of the extra time detection)
        if (!isTouchingFloor && lastWallJumpDirection != Vector2.left && TouchingWall(Vector2.right))
        {
            WallJump(-wallJumpHorizontalForce);
            lastWallJumpDirection = Vector2.left;
            return;
        }
        if (!isTouchingFloor && lastWallJumpDirection != Vector2.right && TouchingWall(Vector2.left))
        {
            WallJump(wallJumpHorizontalForce);
            lastWallJumpDirection = Vector2.right;
            return;
        }
        
        //reset seconds since last jump press if it was not a wall jump (for extra detection time)
        secondsLastJumpPress = 0;
    }

    private void Update()
    {
        if (GameInfo.gameOver)
        {
            return;
        }
        /*
         *  Calculate Horizontal Forces kunne vært en egen funksjon bare for å rydde opp Update litt.
         */
        //Fixed
        CalculateHorizontalForce();
        
        //ground check
        isTouchingFloor = TouchingFloor();

        if (isTouchingFloor)  //reset wall jump direction if on ground
        {
            lastWallJumpDirection = Vector2.zero;
        }
        
        //check if jump possible, and update seconds since last jump press
        secondsLastJumpPress += Time.deltaTime;
        if (secondsLastJumpPress <= jumpExtraDetectionTime && isTouchingFloor)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        if (playerControlLess || horizontalForce == 0.0f || GameInfo.gameOver)
            return;
        //apply horizontal movement, if player has control and there is something to apply
        rigidbody2D.velocity *= Vector2.up; //set velocity x to zero to not add to moving position manually 
        transform.Translate(horizontalForce * Time.fixedDeltaTime * Vector3.right);
    }

    private void CalculateHorizontalForce()
    {
        //Calculate horizontalForce
        if (horizontalInput == 0 && horizontalForce != 0)
        {
            int positiveOrNegative = (horizontalForce > 0) ? 1 : -1;
            
            horizontalForce -= 1 / decelerationTime * maxHorizontalSpeed * Time.deltaTime * positiveOrNegative;
            
            int newPositiovbeOrNegative = (horizontalForce > 0) ? 1 : -1;
            if (positiveOrNegative != newPositiovbeOrNegative)
            {
                horizontalForce = 0;
            }
        }
        else if (Math.Abs(horizontalForce) != maxHorizontalSpeed)
        {
            horizontalForce += 1 / accelerationTime * maxHorizontalSpeed * Time.deltaTime * horizontalInput;
            if (Math.Abs(horizontalForce) > maxHorizontalSpeed)
            {
                horizontalForce = maxHorizontalSpeed * horizontalInput;
            }
        }
    }

    //player jumps normally
    private void Jump()
    {
        rigidbody2D.velocity *= Vector2.right;  //reset y value in velocity to zero
        rigidbody2D.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
    }

    //player jumps away from a wall
    private void WallJump(float forceHorizontally)
    {
        rigidbody2D.velocity = Vector2.zero;
        Jump();
        rigidbody2D.AddForce(forceHorizontally * Vector2.right, ForceMode2D.Impulse);
        StartCoroutine(ControllLossTimer(wallJumpControlLossTime));
    }

    //check if player is touching floor
    private bool TouchingFloor()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(transform.position, new Vector2(playerHalfWidth, 0.2f), 0.0f,
            Vector2.down, playerHalfHight, platformLayerMask);
        if (raycastHit2D.collider == null)
        {
            transform.parent = null; //remove parent since is not touching floor
            return false;
        }
        transform.parent = raycastHit2D.transform; //set player parent to hit object to make player move with moving platforms
        return true;
    }

    //check if player is touching wall
    private bool TouchingWall(Vector2 direction)
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(transform.position, new Vector2(0.2f, playerHalfHight), 0.0f,
            direction, playerHalfWidth, platformLayerMask);
        return raycastHit2D.collider != null;
    }

    //set playerControlLess to true for a specified time
    public IEnumerator ControllLossTimer(float timeSeconds)
    {
        playerControlLess = true;
        yield return new WaitForSeconds(timeSeconds);
        playerControlLess = false;
    }
}
