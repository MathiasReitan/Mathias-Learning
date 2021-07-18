using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    public Color buttonTextColor;
    public Color selectedButtonTextColor;

    public Button button;
    private TextMeshProUGUI textMeshProUGUI;
    private void Awake()
    {
        textMeshProUGUI = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        button = GetComponent<Button>();
    }

    public void SetToSelected()
    {
        if (MenuController.currentlySelectedMenuButton != null)
            MenuController.currentlySelectedMenuButton.textMeshProUGUI.color = buttonTextColor;
        textMeshProUGUI.color = selectedButtonTextColor;
        MenuController.currentlySelectedMenuButton = this;
    }
    
    //Predefined OnClick methods

    public void LogClicked()
    {
        Debug.Log("Clicked", this);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void CloseMenu()
    {
        MenuController.currentlyActiveMenu.DeactivateMenu();
    }

    public void LoadScene(string sceneName)
    {
        SceneManagement.LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        SceneManagement.ReloadCurrentScene();
    }

    public void SwitchActiveMenu(MenuController menuToOpen)
    {
        StartCoroutine(SwitchActiveMenuDelayCoroutine(menuToOpen));
    }

    private IEnumerator SwitchActiveMenuDelayCoroutine(MenuController menuToOpen)
    {
        yield return new WaitForSecondsRealtime(0.1f);
        menuToOpen.ForceActivateMenu();
    }
}
