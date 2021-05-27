using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*  Gives the object the ability to move between checkpoints dynamiccaly
 */

//Class containing info for each teoretical checkpoint
public enum MoveMode
{
    Speed, Seconds
}

[Serializable]
public class CheckpointInfo
{
    [Header("Checkpoint Options")]
    [Tooltip("Coordinates for this checkpoint")]
    public Vector2 position;
    [Tooltip("How long the object waits at checkpoint before going again")]
    public float pauseTime;
    [Header("Movement Options")] 
    [Tooltip("What kind of movement is applied when leaving this checkpoint")]
    public MoveMode moveMode;
    [Tooltip("Travels at specified speed (number between 0 and 1), No value needed if move mode is not speed")]
    public float speed;
    [Tooltip("Travel will take specified seconds, No value needed if move mode is not seconds")]
    public float seconds;
}

//class for moving between checkpoints
public class MovingBetweenCheckpoints : MonoBehaviour
{
    [Tooltip("Seconds it takes before the object starts moving")]
    public float secondsBeforeStartup = 0.0f;
    [Tooltip("List of checkpoints and info on each checkpoint")]
    public List<CheckpointInfo> checkpoints = new List<CheckpointInfo>();

    private int checkpointIndex = 0;
    private float currentStandpoint = 0;

    private CheckpointInfo currentCeckpoint;
    private Vector2 nextCheckpointPos;
    private void OnEnable()
    {
        currentCeckpoint = checkpoints[0];
        nextCheckpointPos = checkpoints[1].position;
        StartCoroutine(MoveBetweenCheckpoints());
    }

    private IEnumerator MoveBetweenCheckpoints()
    {
        yield return new WaitForSeconds(secondsBeforeStartup);
        while (!GameInfo.gameOver)    //while game is not over
        {
            while (currentStandpoint < 1.0f)
            {
                currentStandpoint = Math.Min(1.0f, currentStandpoint + CalculateMoveStep(currentCeckpoint.moveMode));
                transform.position = Vector2.Lerp(currentCeckpoint.position, nextCheckpointPos, currentStandpoint);
                yield return null;
            }

            yield return new WaitForSeconds(currentCeckpoint.pauseTime);
            
            currentStandpoint = 0.0f;
            checkpointIndex = (checkpointIndex + 1) % checkpoints.Count;
            currentCeckpoint = checkpoints[checkpointIndex];
            nextCheckpointPos = checkpoints[(checkpointIndex + 1) % checkpoints.Count].position;
        }
    }

    private float CalculateMoveStep(MoveMode moveMode)
    {
        switch (moveMode)
        {
            case MoveMode.Seconds:
                return 1 / currentCeckpoint.seconds * Time.deltaTime;
            case MoveMode.Speed:
                return currentCeckpoint.speed * Time.deltaTime / Vector2.Distance(currentCeckpoint.position, nextCheckpointPos);
            default:
                Debug.LogError("CalculateMoveStep in MovingBetweenCheckpoints: MoveValue not defined");
                return -1.0f;
        }
    }
}
