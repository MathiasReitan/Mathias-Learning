using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Countdown : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Starting time")]
    public float startTime = 60.0f;
    [Tooltip("When the countown is smaller that this number it will change color")]
    public float shortTimeCap = 10.0f;
    [Header("Aesthetics")]
    [Tooltip("Default color of countdown")]
    public Color defaultColor;
    [Tooltip("Color when there is little time left")]
    public Color shortTimeColor;

    private float currentTime = 60;

    private TextMeshProUGUI textMeshProUGUI;

    private void Awake()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        currentTime = startTime;
        textMeshProUGUI.color = defaultColor;
    }

    private void Update()
    {
        if (GameInfo.gameOver)
            return;

        currentTime = Math.Max(0, currentTime - Time.deltaTime);
        textMeshProUGUI.text = currentTime.ToString("0.00");
        if (currentTime <= 0 && !GameInfo.gameWon)
        {
            GameInfo.gameOver = true;
            Debug.Log("Game Over :')");
        }
        else if (currentTime <= shortTimeCap)
        {
            textMeshProUGUI.color = shortTimeColor;
        }
    }
}
