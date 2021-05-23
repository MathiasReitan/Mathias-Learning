using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*  Makes the object rotate
 */

public class SpinObject : MonoBehaviour
{
    [Tooltip("Which axes the object spins on, indicated by 1 or 0")]
    public Vector3 direction;
    [Tooltip("Speed of spinning (Can also be negative)")]
    public float speed;

    private void Update()
    {
        transform.Rotate(direction, speed * Time.deltaTime);
    }
}
