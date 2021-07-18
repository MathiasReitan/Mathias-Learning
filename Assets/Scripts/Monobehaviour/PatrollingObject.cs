using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrollingObject : MonoBehaviour
{
    [Tooltip("Starting to go left if unchecked, otherwise right")]
    public bool startingRight = true;

    public float speed = 5.0f;

    public bool fallOffLedges = false;

    private Vector2 currentDirection;
    private Vector2 halfTransformScale;
    private LayerMask platformLayerMask;
    
    private void Start()
    {
        currentDirection = startingRight ? Vector2.right : Vector2.left;
        halfTransformScale = transform.localScale / 2;
        platformLayerMask = LayerMask.GetMask("Platform");
    }

    private void Update()
    {
        //Change direction if touching wall or (fall off ledges disabled and touching edge)
        if (TouchingWall(currentDirection) || !fallOffLedges && TouchingPlatformEdge(currentDirection))
            currentDirection *= -1;
    }

    private void FixedUpdate()
    {
        transform.Translate(speed * Time.fixedDeltaTime * currentDirection);
    }

    private bool TouchingWall(Vector2 direction)
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(transform.position, new Vector2(0.2f, halfTransformScale.y), 0.0f, direction,
            halfTransformScale.x, platformLayerMask);
        return raycastHit2D.collider != null;
    }

    private bool TouchingPlatformEdge(Vector2 direction)
    {
        RaycastHit2D raycastHit2D = Physics2D.Raycast((Vector2) transform.position + direction * halfTransformScale.x,
            Vector2.down, halfTransformScale.y + 0.1f, platformLayerMask);
        return raycastHit2D.collider == null;
    }
}
