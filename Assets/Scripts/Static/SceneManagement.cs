using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum Scene
{
     Level_1, Level_2, Level_3
}

public static class SceneManagement
{
    public static void LoadScene(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }

    public static void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
