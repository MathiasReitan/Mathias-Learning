using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class FollowingPlayer : MonoBehaviour
{
    [Header("GameObject references")]
    public GameObject gameObjectToFollow;
    public PathFindingData pathFindingData;
    [Header("Movement")]
    public float jumpHeight;
    public float maxSpeed;
    public float fallCurveSharpness;

    private int currentPlatformIndex;

    private Rigidbody2D rigidbody2D;
    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        currentPlatformIndex = FindPlatformIndexInPFData(GetCurrentPlatform());
        
        //StartCoroutine(FollowPathCoroutine(GenerateRandomPath(100)));
        StartCoroutine(FollowTheGameObjectCoroutine());
    }


    public float CalculateJumpY(float x)    //calculates y value in a jump based on the relative x progression of the jump. x ranges from 0 start if jump to infinity end of jump
    {
        float jumpHeightTimesFourTimesX = jumpHeight * 4 * x;
        return -jumpHeightTimesFourTimesX * x + jumpHeightTimesFourTimesX;
    }
    
    public float CalculateXFromYInJump(float y) //Calculates on what x value will be on a spesific y value. y ranges from 0 to this.jumpHight
    {
        float sqrtJumpHight = (float) Math.Sqrt(jumpHeight);
        return ((float) Math.Sqrt(-y + jumpHeight) + sqrtJumpHight) / (2 * sqrtJumpHight);
    }

    public float CalculateFallY(float x)
    {
        return -fallCurveSharpness * x * x;
    }

    public float CalculateXFromYInFall(float y)
    {
        return (float) Math.Sqrt(-y) / (float) Math.Sqrt(fallCurveSharpness);
    }

    private int FindPlatformIndexInPFData(GameObject platform)
    {
        return pathFindingData.pathFindingNodes.FindIndex(f => f.platformGameObject == platform);
    }

    private GameObject GetCurrentPlatform()
    {
        Vector2 pos = transform.position;
        Vector2 scale = transform.localScale;
        return Physics2D.BoxCast(new Vector2(pos.x, pos.y - scale.y / 2f - 0.1f), new Vector2(scale.x, 0.1f), 0f,
            Vector2.down, Mathf.Infinity, LayerMask.GetMask("Platform")).collider.gameObject;
    }

    private List<ConnectedNode> GenerateRandomPath(int length)
    {
        List<ConnectedNode> randomPath = new List<ConnectedNode>(length);
        int platformIndex = currentPlatformIndex;
        for (int i = 0; i < length; ++i)
        {
            List<ConnectedNode> connectedNodes = pathFindingData.pathFindingNodes[platformIndex].connectedNodes;
            randomPath.Add(connectedNodes[Random.Range(0, connectedNodes.Count)]);
            platformIndex = FindPlatformIndexInPFData(randomPath[randomPath.Count - 1].platformGameObject);
        }
        return randomPath;
    }

    private List<ConnectedNode> GenerateShortestPath(PathFindingNode startPathFindingNode, PathFindingNode goalPathFindingNode)
    {
        startPathFindingNode.FromStartCost = 0f;
        startPathFindingNode.ToGoalDistance = Vector2.Distance(startPathFindingNode.platformGameObject.transform.position,
                                                  goalPathFindingNode.platformGameObject.transform.position) / maxSpeed;
        startPathFindingNode.Score = startPathFindingNode.ToGoalDistance;
        List<PathFindingNode> openNodes = new List<PathFindingNode>() {startPathFindingNode};
        List<PathFindingNode> closedNodes = new List<PathFindingNode>();
        PathFindingNode currentNode = new PathFindingNode();
        foreach (PathFindingNode pfn in pathFindingData.pathFindingNodes)
            pfn.CameFromNode = null;
        {
            
        }
        while (openNodes.Count > 0)
        {
            currentNode = openNodes.Aggregate((i1, i2) => i1.Score < i2.Score ? i1 : i2);
            if (currentNode == goalPathFindingNode)
                break;
            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);
            foreach (ConnectedNode connection in currentNode.connectedNodes)
            {
                PathFindingNode connectedNode = pathFindingData.pathFindingNodes[FindPlatformIndexInPFData(connection.platformGameObject)];
                if (closedNodes.Contains(connectedNode))
                    continue;
                float newFromStartCost = currentNode.FromStartCost + Vector2.Distance(currentNode.platformGameObject.transform.position,
                                                                             connectedNode.platformGameObject.transform.position);
                if (!openNodes.Contains(connectedNode) || newFromStartCost < connectedNode.FromStartCost)
                {
                    connectedNode.FromStartCost = newFromStartCost;
                    connectedNode.ToGoalDistance = Vector2.Distance(connectedNode.platformGameObject.transform.position,
                                                                    goalPathFindingNode.platformGameObject.transform.position);
                    connectedNode.Score = connectedNode.FromStartCost + connectedNode.ToGoalDistance;
                    connectedNode.CameFromNode = currentNode;
                    if (!openNodes.Contains(connectedNode))
                        openNodes.Add(connectedNode);
                }
                
            }
        }
        //convert the found path into executable format
        List<ConnectedNode> reversePath = new List<ConnectedNode>();
        while (currentNode.CameFromNode != null)
        {
            ConnectedNode connectionUsed = currentNode.CameFromNode.connectedNodes.Find(f => f.platformGameObject == currentNode.platformGameObject);
            reversePath.Add(connectionUsed);
            currentNode = currentNode.CameFromNode;
        }
        reversePath.Reverse();
        return reversePath;
    }

    private IEnumerator FollowTheGameObjectCoroutine()
    {
        while (!GameInfo.gameOver && !GameInfo.gameWon)
        {
            int goingToIndex = Random.Range(0, pathFindingData.pathFindingNodes.Count);
            yield return FollowPathCoroutine(GenerateShortestPath(
                pathFindingData.pathFindingNodes[currentPlatformIndex],
                pathFindingData.pathFindingNodes[goingToIndex]));
            currentPlatformIndex = goingToIndex;
        }
    }

    private IEnumerator FollowPathCoroutine(List<ConnectedNode> path) //follows the path until the goal is reached or the goal is not containing the object to follow
    {
        foreach (ConnectedNode connectedNode in path)
        {
            switch (connectedNode.connectionType)
            {
                case ConnectedNode.ConnectionType.Fall:
                    yield return DoFallCoroutine(connectedNode, connectedNode.fallStartX, connectedNode.fallDirectionSpeed);
                    break;
                case ConnectedNode.ConnectionType.Jump:
                    yield return DoJumpCoroutine(connectedNode, connectedNode.jumpStartX, connectedNode.objectOnFromPlatformY,
                        connectedNode.jumpDistance, connectedNode.onPlatformValue);
                    break;
                case ConnectedNode.ConnectionType.FallWallJump:
                    yield return DoFallWallJumpCoroutine(connectedNode);
                    break;
                case ConnectedNode.ConnectionType.JumpWallJump:
                    yield return DoJumpWallJumpCoroutine(connectedNode);
                    break;
            }
        }
    }

    private IEnumerator DoFallCoroutine(ConnectedNode connectedNode, float fallStartX, float fallDirectionSpeed)
    {
        Vector3 startPos = new Vector3(fallStartX, connectedNode.objectOnFromPlatformY);
        if (transform.position != startPos)
            yield return GoToPositionCoroutine(startPos);
        float horizontalDistance = Math.Abs(connectedNode.onPlatformValue * fallDirectionSpeed);
        float currentBetweenPoint = 0f;
        while (currentBetweenPoint < connectedNode.onPlatformValue)
        {
            currentBetweenPoint = Math.Min(connectedNode.onPlatformValue, currentBetweenPoint + Math.Abs(fallDirectionSpeed * Time.deltaTime / horizontalDistance));
            transform.position = new Vector3(startPos.x + currentBetweenPoint * fallDirectionSpeed, startPos.y + CalculateFallY(currentBetweenPoint));
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator DoJumpCoroutine(ConnectedNode connectedNode, float jumpStartX, float jumpStartY, float jumpDirectionSpeed, float onPlatformValue)
    {
        Vector3 startPos = new Vector3(jumpStartX, jumpStartY);
        if (transform.position != startPos)
            yield return GoToPositionCoroutine(startPos);
        float horizontalDistance = Math.Abs(onPlatformValue * jumpDirectionSpeed);
        float currentBetweenPoint = 0f;
        while (currentBetweenPoint < onPlatformValue)
        {
            currentBetweenPoint = Math.Min(onPlatformValue, currentBetweenPoint + Math.Abs(jumpDirectionSpeed * Time.deltaTime / horizontalDistance));
            transform.position = new Vector3(startPos.x + currentBetweenPoint * jumpDirectionSpeed, startPos.y + CalculateJumpY(currentBetweenPoint));
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator DoFallWallJumpCoroutine(ConnectedNode connectedNode)
    {
        yield return DoFallCoroutine(connectedNode, connectedNode.fallWallJumpStartX, connectedNode.preFallWallJumpDirectionSpeed);
        Vector2 wallJumpStarPos = new Vector2(connectedNode.firstFallWallJumpHit.x, connectedNode.fallWallJumpY);
        yield return GoToPositionCoroutine(wallJumpStarPos);
        yield return DoJumpCoroutine(connectedNode, wallJumpStarPos.x, connectedNode.fallWallJumpY,
            connectedNode.postFallWallJumpDirectionSpeed, connectedNode.onPlatformValue2);
    }

    private IEnumerator DoJumpWallJumpCoroutine(ConnectedNode connectedNode)
    {
        yield return DoJumpCoroutine(connectedNode, connectedNode.preJumpWallJumpStartX, connectedNode.objectOnFromPlatformY,
            connectedNode.preJumpWallJumpDirectionSpeed, connectedNode.onPlatformValue);
        Vector2 wallJumpStartPos = new Vector2(connectedNode.firstJumpWallJumpHit.x, connectedNode.jumpWallJumpY);
        yield return GoToPositionCoroutine(wallJumpStartPos);
        yield return DoJumpCoroutine(connectedNode, connectedNode.firstJumpWallJumpHit.x, connectedNode.jumpWallJumpY,
            connectedNode.postJumpWallJumpDirectionSpeed, connectedNode.onPlatformValue2);
    }

    private IEnumerator GoToPositionCoroutine(Vector2 endPos)
    {
        Vector2 startPos = transform.position;
        float currentBetweenPoint = 0f;
        while (currentBetweenPoint < 1f)
        {
            currentBetweenPoint = Math.Min(1f, currentBetweenPoint + maxSpeed * Time.deltaTime / Vector2.Distance(startPos, endPos));
            transform.position = Vector2.Lerp(startPos, endPos, currentBetweenPoint);
            yield return new WaitForEndOfFrame();
        }
    }
}
