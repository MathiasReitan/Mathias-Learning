using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*  Gives the ability to collect coins
        Compatible with the Coin prefab
 */

public class CoinCollector : MonoBehaviour
{
    [HideInInspector]
    public int neededCoins = 0;
    [HideInInspector]
    public int coinCount = 0;

    private void Awake()
    {
        neededCoins = GameObject.FindGameObjectsWithTag("Coin").Length;     //Count how many coins are needed
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Coin"))   //only do following code with coins
            return;
        
        Destroy(other.gameObject);      //slaughter the coin on contact

        if (++coinCount >= neededCoins)     //add to coinCount, and check if enough is collected
        {
            Debug.Log("Winner :))");
        }
    }
}
