using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

public class HealthTracker : MonoBehaviour
{
    public int health = 3;
    public Color damagedColor = Color.red;
    public float damagedColorSeconds = 2.0f;
    
    public float invincibilityBoostSeconds = 2.0f;
    public float invincibilityFlashRate = 0.2f;
    [Range(0.0f, 1.0f)]
    public float invincibilityOpacity = 100.0f;
    
    public bool playerInvincible = false;

    private SpriteRenderer spriteRenderer;
    private Color normalColor;
    private float normalOpacity;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalColor = spriteRenderer.color;
        normalOpacity = spriteRenderer.color.a;
    }

    public void takeDamage(int damageAmount = 1)
    {
        if (playerInvincible)
            return;
        
        
        StartCoroutine(DamageColorTimer());
        StartCoroutine(InvincibilityBoostTimer());

        if (GameInfo.gameWon)
            return;
        
        health -= damageAmount;
        
        if (health == 0)
        {
            Debug.Log("Game Over :')");
            GameInfo.gameOver = true;
        }
    }

    private IEnumerator DamageColorTimer()
    {
        spriteRenderer.color = damagedColor;
        yield return new WaitForSeconds(damagedColorSeconds);
        spriteRenderer.color = normalColor;
    }

    private IEnumerator InvincibilityBoostTimer()
    {
        playerInvincible = true;
        int repeat = (int) (invincibilityBoostSeconds / (invincibilityFlashRate * 2));
        for (int i = 0; i < repeat; i++)
        {
            SetSpriteAlpha(invincibilityOpacity);
            yield return new WaitForSeconds(invincibilityFlashRate);
            SetSpriteAlpha(normalOpacity);
            yield return new WaitForSeconds(invincibilityFlashRate);
        }

        playerInvincible = false;
    }

    private void SetSpriteAlpha(float alpha)
    {
        Color newColor = spriteRenderer.color;
        newColor.a = alpha;
        spriteRenderer.color = newColor;
    }
}
