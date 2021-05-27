using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadSceneOnGameOver : MonoBehaviour
{
    public float waitSecondsBeforeLoad = 2.0f;

    private void Update()
    {
        if (GameInfo.gameOver)
        {
            StartCoroutine(WaitReloadScene(waitSecondsBeforeLoad));
        }
    }

    private IEnumerator WaitReloadScene(float waitSeconds)
    {
        yield return new WaitForSeconds(waitSeconds);
        GameInfo.gameOver = false;
        SceneManagement.ReloadCurrentScene();
    }
}
