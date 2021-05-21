using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

/*  Player Controlls Features:
        Move left and right
        Jump when on ground
            If jump is pressed just before hitting the ground it will still be detected
        Wall jump once, before landing on ground again
            Wall jump will make player lose control for a short while
*/

public class PlayerControlls : MonoBehaviour
{
    [Header("Left Right Controlls")]
    [Tooltip("Characters speed left and right")]
    public float horizontalSpeed = 10.0f;
    [Tooltip("Force applied when jumping")]
    [Header("Jump Controlls")]
    public float jumpForce = 1600.0f;
    [Tooltip("Seconds deciding how long after jump being pressed it will still detect a jump (does not affect wall jumping)")]
    public float jumpExtraDetectionTime = 0.2f;
    [Header("Wall Jump Controlls")]
    [Tooltip("Force pushing player away from the wall when wall jumping")]
    public float wallJumpHorizontalForce = 500.0f;
    [Tooltip("Timespan seconds player loses control after wall jumping")]
    public float wallJumpControlLossTime = 3.0f;

    private float horizontalInput = 0;
    private bool jumpInput = false;
    private bool wallJumpUsed = false;
    private bool playerInAir = false;
    private bool playerControlLess = false;
    private float secondsSinceLastJumpPress = 10.0f;

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

    private void Update()
    {
        //get player input
        horizontalInput = Input.GetAxis("Horizontal");
        if (Input.GetButtonDown("Jump"))
        {
            jumpInput = true;
            secondsSinceLastJumpPress = 0;
        }

        secondsSinceLastJumpPress += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (playerControlLess)  //No need to check player controls if player is control-less
            return;
        
        
        /* Markus Kommentar:
         * Lurt å ha Input i Update. (Med det nye Input Systemet så trenger ikke input være i Update men kan heller vøære i egen funksjon
         * Det å ha det i fixedupdate kan forårsake at det blir noen missed frames som gjør at kontrollene føles unøyaktige-
         *
         * Generelt vil man ha Fysikk i Fixed Update, det meste annet kan holdes i Update.
         */
        //FIXED
        

        //do horizontal output
        if (horizontalInput != 0)
        {
            rigidbody.velocity *= Vector2.up;   //set x velocity from wall jump to zero, to not add to horizontal input
            transform.Translate(horizontalInput * Time.deltaTime * horizontalSpeed * Vector3.right);
        }
        
        //do jump output
        if (!jumpInput && secondsSinceLastJumpPress > jumpExtraDetectionTime) //No need to test jump conditions if jump key not pressed
            return;

        /* Markus Kommentar:
         * Du kan walljumpe mens du står på bakken.
         * Helst burde man ikke kunne walljumpe så lenge man rører ved bakken.
         */
        //FIXED
        
        if (playerInAir)
        {
            if (CheckForFloor())        //if touching ground reset playerInAir, and wallJumpsUsed
            {
                playerInAir = false;
                wallJumpUsed = false;
            }

            if (wallJumpUsed || !playerInAir || !jumpInput)       //no need to test wall jump conditions if wall jump is used, or player is on ground
                return;
            
            if (CheckForWall(Vector2.right))        //Wall jump if wall jump not used, player in air, and space pressed, and is next to wall
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

        jumpInput = false;
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

/* Markus Kommentar
* Veldig bra arbeid! Utrolig digg walljump, føles skikkelig smooth ut å bruke.

 
  TODO: Bugs
  
  TODO: Av og til når man hopper opp og treffer siden av en platform så walljumper man litt til siden
  FIXED
  TODO: ny bug der det ikke blir registrert hopp hvis en trykker like før en treffer platformen
  FIXED
  
*/