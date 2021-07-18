using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardingObject : MonoBehaviour
{
    private Vector2 guardingPos;

    private void Start()
    {
        guardingPos = transform.position;
    }
}
