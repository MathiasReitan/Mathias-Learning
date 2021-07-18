using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;

public enum Scene
{
     Level_1 = 0, Level_2 = 1, Level_3 = 2, Level_4 = 3, Main_Menu = 4
}

public static class SceneManagement
{ 
    public static void LoadScene(Scene scene)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(scene.ToString());
    }
    
    public static void LoadScene(string scene)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(scene);
    }

    public static void ReloadCurrentScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
