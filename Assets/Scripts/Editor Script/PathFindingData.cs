using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[Serializable]
public class ConnectedNode
{
    public enum ConnectionType
    {
        Jump, Fall, JumpWallJump, FallWallJump, UndefinedForNow
    };

    public bool showConnectionHandles;
    public GameObject platformGameObject;
    public ConnectionType connectionType;
    [HideInInspector] public float objectOnFromPlatformY;
    [HideInInspector] public float objectOnToPlatformY;
    [HideInInspector] public float onPlatformValue;
    [HideInInspector] public float onPlatformValue2;

    [Header("JumpData (Only required if connection type jump)")]
    public float jumpStartX;
    public float jumpDistance;

    [Header("FallData (Only required if connection type fall)")]
    public float fallDirectionSpeed;
    [HideInInspector] public float fallStartX;

    [Header("JumpWallJumpData (Only required if connection type jump-wall-jump")]
    public float preJumpWallJumpStartX;
    public float preJumpWallJumpDirectionSpeed;
    public GameObject jumpWallJumpPlatform;
    public float jumpWallJumpY;
    public float postJumpWallJumpDirectionSpeed;
    [HideInInspector] public Vector2 firstJumpWallJumpHit;

    [Header("FallWallJumpData (Only required if connection type fall-wall-jump")]
    public float preFallWallJumpDirectionSpeed;
    public GameObject fallWallJumpWall;
    public float fallWallJumpY;
    public float postFallWallJumpDirectionSpeed;
    [HideInInspector] public Vector2 firstFallWallJumpHit;
    [HideInInspector] public float fallWallJumpStartX;
}

[Serializable]
public class PathFindingNode
{
    public GameObject platformGameObject;
    public bool showConnectionHandles;
    public List<ConnectedNode> connectedNodes;

    [HideInInspector]
    public float objectOnFromPlatformY;
    
    //ForThePathFindingSpecificValues
    public float FromStartCost { get; set; }
    public float ToGoalDistance { get; set; }
    public float Score { get; set; }
    public PathFindingNode CameFromNode { get; set; }
}

public class PathFindingData : MonoBehaviour
{
    public GameObject pathfindingGameObject;
    public List<PathFindingNode> pathFindingNodes;

    public float CalculateGameObjectOnPlatformY(GameObject platform, GameObject onPlatform)
    {
        Transform platformTransform = platform.transform;
        return platformTransform.position.y + platformTransform.lossyScale.y / 2 + onPlatform.transform.lossyScale.y / 2;
    }
}

[CustomEditor(typeof(PathFindingData))]
public class PathFindingDataCustomEditor : Editor
{
    private void OnSceneGUI()
    {
        PathFindingData pfData = (PathFindingData) target;
        FollowingPlayer followingPlayerScript = pfData.pathfindingGameObject.GetComponent<FollowingPlayer>();
        Vector3 pathfindingGameObjectSize = pfData.pathfindingGameObject.transform.lossyScale;
        foreach (PathFindingNode pfNode in pfData.pathFindingNodes)
        {
            pfNode.objectOnFromPlatformY = pfData.CalculateGameObjectOnPlatformY(pfNode.platformGameObject, pfData.pathfindingGameObject);
            
            if (!pfNode.showConnectionHandles)
                continue;
            
            Handles.color = Color.green;
            Handles.DrawWireCube(pfNode.platformGameObject.transform.position, pfNode.platformGameObject.transform.lossyScale);
            
            foreach (ConnectedNode cNode in pfNode.connectedNodes)
            {
                cNode.objectOnFromPlatformY = pfNode.objectOnFromPlatformY;
                cNode.objectOnToPlatformY = pfData.CalculateGameObjectOnPlatformY(cNode.platformGameObject, pfData.pathfindingGameObject);
                if (!cNode.showConnectionHandles)
                    continue;
                Handles.color = Color.blue;
                Handles.DrawWireCube(cNode.platformGameObject.transform.position, cNode.platformGameObject.transform.lossyScale);
                
                Handles.color = Color.red;
                switch (cNode.connectionType)
                {
                    case ConnectedNode.ConnectionType.Jump:
                        JumpConnection(pfNode, cNode, followingPlayerScript, pathfindingGameObjectSize);
                        break;
                    case ConnectedNode.ConnectionType.Fall:
                        FallConnection(pfData, pfNode, cNode, followingPlayerScript, pathfindingGameObjectSize);
                        break;
                    case ConnectedNode.ConnectionType.JumpWallJump:
                        JumpWallJumpConnection(pfNode, cNode, followingPlayerScript, pathfindingGameObjectSize);
                        break;
                    case ConnectedNode.ConnectionType.FallWallJump:
                        FallWallJumpConnection(pfData, pfNode, cNode, followingPlayerScript, pathfindingGameObjectSize);
                        break;
                }
            }
        }
    }

    private void JumpConnection(PathFindingNode pfNode, ConnectedNode cNode, FollowingPlayer followingPlayerScript, Vector3 pathfindingGameObjectSize)
    {
        if (cNode.jumpDistance > followingPlayerScript.maxSpeed)
            cNode.jumpDistance = followingPlayerScript.maxSpeed;
        else if (cNode.jumpDistance < -followingPlayerScript.maxSpeed)
            cNode.jumpDistance = -followingPlayerScript.maxSpeed;
        cNode.onPlatformValue = 1;
        if (cNode.objectOnToPlatformY <= pfNode.objectOnFromPlatformY + followingPlayerScript.jumpHeight)
            cNode.onPlatformValue = followingPlayerScript.CalculateXFromYInJump(cNode.objectOnToPlatformY - pfNode.objectOnFromPlatformY);
        JumpVisualization(cNode.onPlatformValue, cNode.jumpStartX, pfNode.objectOnFromPlatformY, cNode.jumpDistance, pfNode, followingPlayerScript, pathfindingGameObjectSize);
    }

    private void FallConnection(PathFindingData pfData, PathFindingNode pfNode, ConnectedNode cNode, FollowingPlayer followingPlayerScript, Vector3 pathfindingGameObjectSize)
    {
        if (cNode.fallDirectionSpeed > followingPlayerScript.maxSpeed)
            cNode.fallDirectionSpeed = followingPlayerScript.maxSpeed;
        else if (cNode.fallDirectionSpeed < -followingPlayerScript.maxSpeed)
            cNode.fallDirectionSpeed = -followingPlayerScript.maxSpeed;
        cNode.fallStartX = pfNode.platformGameObject.transform.position.x + (pfNode.platformGameObject.transform.lossyScale.x / 2 + 
                           pfData.pathfindingGameObject.transform.lossyScale.x / 2) * (cNode.fallDirectionSpeed > 0 ? 1 : -1);
        cNode.onPlatformValue = 1;
        if (cNode.objectOnToPlatformY < pfNode.objectOnFromPlatformY)
            cNode.onPlatformValue = followingPlayerScript.CalculateXFromYInFall(cNode.objectOnToPlatformY - pfNode.objectOnFromPlatformY);
        FallVisualization(cNode.onPlatformValue, cNode.fallStartX, cNode.fallDirectionSpeed, pfNode, cNode, followingPlayerScript, pathfindingGameObjectSize);
    }

    private void JumpWallJumpConnection(PathFindingNode pfNode, ConnectedNode cNode, FollowingPlayer followingPlayerScript, Vector3 pathfindingGameObjectSize)
    {
        if (cNode.preJumpWallJumpDirectionSpeed > followingPlayerScript.maxSpeed)
            cNode.preJumpWallJumpDirectionSpeed = followingPlayerScript.maxSpeed;
        else if (cNode.preJumpWallJumpDirectionSpeed < -followingPlayerScript.maxSpeed)
            cNode.preJumpWallJumpDirectionSpeed = -followingPlayerScript.maxSpeed;
        Vector2 jumpWallJumpPlatformSizeDiv2 = cNode.jumpWallJumpPlatform.transform.lossyScale / 2f;
        Vector2 jumpWallJumpPlatformPos = cNode.jumpWallJumpPlatform.transform.position;
        cNode.firstJumpWallJumpHit.x = jumpWallJumpPlatformPos.x + (jumpWallJumpPlatformSizeDiv2.x + pathfindingGameObjectSize.x / 2f) *
            (cNode.preJumpWallJumpDirectionSpeed > 0f ? -1f : 1f);
        cNode.onPlatformValue = Math.Abs((cNode.firstJumpWallJumpHit.x - cNode.preJumpWallJumpStartX) / cNode.preJumpWallJumpDirectionSpeed);
        cNode.firstJumpWallJumpHit.y = pfNode.objectOnFromPlatformY + followingPlayerScript.CalculateJumpY(cNode.onPlatformValue);
        if (cNode.firstJumpWallJumpHit.y < jumpWallJumpPlatformPos.y - jumpWallJumpPlatformSizeDiv2.y ||
                    cNode.firstJumpWallJumpHit.y > jumpWallJumpPlatformPos.y + jumpWallJumpPlatformSizeDiv2.y ||
                    cNode.preJumpWallJumpDirectionSpeed == 0f) 
            cNode.onPlatformValue = 1;
        JumpVisualization(cNode.onPlatformValue, cNode.preJumpWallJumpStartX, pfNode.objectOnFromPlatformY,
            cNode.preJumpWallJumpDirectionSpeed, pfNode, followingPlayerScript, pathfindingGameObjectSize);

        if (cNode.jumpWallJumpY > cNode.firstJumpWallJumpHit.y)
            cNode.jumpWallJumpY = cNode.firstJumpWallJumpHit.y;
        else if (cNode.jumpWallJumpY < jumpWallJumpPlatformPos.y - jumpWallJumpPlatformSizeDiv2.y)
            cNode.jumpWallJumpY = jumpWallJumpPlatformPos.y - jumpWallJumpPlatformSizeDiv2.y;
        StraightFallVisualization(cNode.firstJumpWallJumpHit.y, cNode.jumpWallJumpY, cNode.firstJumpWallJumpHit.x, pathfindingGameObjectSize);
        
        if (cNode.postJumpWallJumpDirectionSpeed > followingPlayerScript.maxSpeed)
            cNode.postJumpWallJumpDirectionSpeed = followingPlayerScript.maxSpeed;
        else if (cNode.postJumpWallJumpDirectionSpeed < -followingPlayerScript.maxSpeed)
            cNode.postJumpWallJumpDirectionSpeed = -followingPlayerScript.maxSpeed;
        cNode.onPlatformValue2 = 1;
        if (cNode.objectOnToPlatformY <= cNode.jumpWallJumpY + followingPlayerScript.jumpHeight)
            cNode.onPlatformValue2 = followingPlayerScript.CalculateXFromYInJump(cNode.objectOnToPlatformY - cNode.jumpWallJumpY);
        JumpVisualization(cNode.onPlatformValue2, cNode.firstJumpWallJumpHit.x, cNode.jumpWallJumpY,
            cNode.postJumpWallJumpDirectionSpeed, pfNode, followingPlayerScript, pathfindingGameObjectSize);
    }

    private void FallWallJumpConnection(PathFindingData pfData, PathFindingNode pfNode, ConnectedNode cNode, FollowingPlayer followingPlayerScript, Vector3 pathfindingGameObjectSize)
    {
        if (cNode.preFallWallJumpDirectionSpeed > followingPlayerScript.maxSpeed)
            cNode.preFallWallJumpDirectionSpeed = followingPlayerScript.maxSpeed;
        else if (cNode.preFallWallJumpDirectionSpeed < -followingPlayerScript.maxSpeed)
            cNode.preFallWallJumpDirectionSpeed = -followingPlayerScript.maxSpeed;
        Vector2 fallWallJumpPlatformPos = cNode.fallWallJumpWall.transform.position;
        Vector2 fallWallJumpPlatformSizeDiv2 = cNode.fallWallJumpWall.transform.lossyScale / 2f;
        cNode.fallWallJumpStartX = pfNode.platformGameObject.transform.position.x + (pfNode.platformGameObject.transform.lossyScale.x / 2 + 
            pfData.pathfindingGameObject.transform.lossyScale.x / 2) * (cNode.preFallWallJumpDirectionSpeed > 0 ? 1 : -1);
        cNode.firstFallWallJumpHit.x = fallWallJumpPlatformPos.x + (fallWallJumpPlatformSizeDiv2.x + pathfindingGameObjectSize.x / 2f) *
                            (cNode.preFallWallJumpDirectionSpeed > 0f ? -1f : 1f);
        cNode.onPlatformValue = Math.Abs((cNode.firstFallWallJumpHit.x - cNode.fallWallJumpStartX) / cNode.preFallWallJumpDirectionSpeed);
        cNode.firstFallWallJumpHit.y = pfNode.objectOnFromPlatformY + followingPlayerScript.CalculateFallY(cNode.onPlatformValue);
        if (cNode.firstFallWallJumpHit.y < fallWallJumpPlatformPos.y - fallWallJumpPlatformSizeDiv2.y ||
                    cNode.firstFallWallJumpHit.y > fallWallJumpPlatformPos.y + fallWallJumpPlatformSizeDiv2.y || cNode.preFallWallJumpDirectionSpeed == 0f)
            cNode.onPlatformValue = 1f;
        FallVisualization(cNode.onPlatformValue, cNode.fallWallJumpStartX, cNode.preFallWallJumpDirectionSpeed, pfNode, cNode, followingPlayerScript, pathfindingGameObjectSize);

        if (cNode.fallWallJumpY > cNode.firstFallWallJumpHit.y) 
            cNode.fallWallJumpY = cNode.firstFallWallJumpHit.y;
        else if (cNode.fallWallJumpY < fallWallJumpPlatformPos.y - fallWallJumpPlatformSizeDiv2.y)
            cNode.fallWallJumpY = fallWallJumpPlatformPos.y - fallWallJumpPlatformSizeDiv2.y;
        StraightFallVisualization(cNode.firstFallWallJumpHit.y, cNode.fallWallJumpY, cNode.firstFallWallJumpHit.x, pathfindingGameObjectSize);
                        
        if (cNode.postFallWallJumpDirectionSpeed > followingPlayerScript.maxSpeed)
            cNode.postFallWallJumpDirectionSpeed = followingPlayerScript.maxSpeed;
        else if (cNode.postFallWallJumpDirectionSpeed < -followingPlayerScript.maxSpeed)
            cNode.postFallWallJumpDirectionSpeed = -followingPlayerScript.maxSpeed;
        cNode.onPlatformValue2 = 1;
        if (cNode.objectOnToPlatformY <= cNode.fallWallJumpY + followingPlayerScript.jumpHeight)
            cNode.onPlatformValue2 = followingPlayerScript.CalculateXFromYInJump(cNode.objectOnToPlatformY - cNode.fallWallJumpY);
        JumpVisualization(cNode.onPlatformValue2, cNode.firstFallWallJumpHit.x, cNode.fallWallJumpY, cNode.postFallWallJumpDirectionSpeed, 
            pfNode, followingPlayerScript, pathfindingGameObjectSize);
    }

    private void JumpVisualization(float onPlatformValue, float jumpStartX, float jumpStartY, float jumpDistance, PathFindingNode pfNode, FollowingPlayer followingPlayerScript, Vector3 pathfindingGameObjectSize)
    {
        float visualStep = 0.1f;
        for (float currentVisualStep = 0f; currentVisualStep < onPlatformValue; currentVisualStep += visualStep)
        {
            Handles.DrawWireCube(new Vector3(jumpStartX + currentVisualStep * jumpDistance, 
                jumpStartY + followingPlayerScript.CalculateJumpY(currentVisualStep)), pathfindingGameObjectSize);
        }
    }

    private void FallVisualization(float onPlatformValue, float fallStartX, float fallDirectionSpeed, PathFindingNode pfNode, ConnectedNode cNode, FollowingPlayer followingPlayerScript, Vector3 pathfindingGameObjectSize)
    {
        float visualStep = 0.1f;
        for (float currentVisualStep = 0f; currentVisualStep < onPlatformValue; currentVisualStep += visualStep)
        {
            Handles.DrawWireCube(
                new Vector3(fallStartX + currentVisualStep * fallDirectionSpeed, pfNode.objectOnFromPlatformY +
                    followingPlayerScript.CalculateFallY(currentVisualStep)), pathfindingGameObjectSize);
        }
    }

    private void StraightFallVisualization(float startY, float endY, float x, Vector3 pathfindingGameObjectSize)
    {
        float visualStep = 2f;
        for (float currentY = startY; currentY > endY; currentY -= visualStep)
        {
            Handles.DrawWireCube(new Vector3(x, currentY), pathfindingGameObjectSize);
        }
    }
}
