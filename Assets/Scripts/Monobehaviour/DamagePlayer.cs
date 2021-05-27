using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    public int pointsOfDamage = 1;
    public bool pushPlayerAway = true;
    public float pushAwayForce = 750.0f;
    public float playerControllLessSeconds = 0.5f;

    private GameObject playerGameObject;
    private HealthTracker playerHealthTracker;
    private Rigidbody2D playerRigidbody2D;
    private PlayerController playerPlayerController;

    private void Awake()
    {
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        playerHealthTracker = playerGameObject.GetComponent<HealthTracker>();
        playerRigidbody2D = playerGameObject.GetComponent<Rigidbody2D>();
        playerPlayerController = playerGameObject.GetComponent<PlayerController>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;
        
        playerHealthTracker.takeDamage(pointsOfDamage);
        if (pushPlayerAway)
            PushAway();
    }

    private void PushAway() //lagde ny funksjon siden walljump har litt andre features enn ønsket her
    {
        Vector2 direction = playerGameObject.transform.position - transform.position; //calculate direction for player to get shot
        direction.Normalize(); //Normalize direction to -1 <= value <= 1
        direction.y = 1.0f; //always shoot as much up as possible, x can stay dynamic
        playerRigidbody2D.velocity = Vector2.zero; //reset velocity
        playerRigidbody2D.AddForce(pushAwayForce * direction, ForceMode2D.Impulse); //apply force
        StartCoroutine(playerPlayerController.ControllLossTimer(playerControllLessSeconds)); //make player controllLess
    }
}
