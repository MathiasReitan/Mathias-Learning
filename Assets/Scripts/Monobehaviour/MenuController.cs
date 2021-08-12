using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Menu")]
    public bool menuActive;
    public bool toggleActiveWithPlayerInput;
    public bool pauseGameWhenActive;
    [Header("Menu buttons")]
    public Color buttonTextColor;
    public Color selectedButtonTextColor;
    public List<MenuButton> menuButtons = new List<MenuButton>();

    public static MenuButton currentlySelectedMenuButton; 
    public static MenuController currentlyActiveMenu;

    [Header("Required prefabs:")]
    public GameObject menuButtonPrefab;
    
    private Canvas canvas;
    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        if (menuButtons.Count >= 1)
        {
            menuButtons[0].SetToSelected();
        }
        SetMenuActive(menuActive);
    }

    private void OnUpDown(InputValue value)
    {
        if (!menuActive)
            return;
        int inputValue = (int) value.Get<float>();
        if (inputValue == 0)
            return;
        int currentlySelectedButtonIndex = menuButtons.FindIndex(f => f == currentlySelectedMenuButton);
        menuButtons[(currentlySelectedButtonIndex + inputValue + menuButtons.Count) % menuButtons.Count].SetToSelected();
    }

    private void OnSelect()
    {
        if (!menuActive)
            return;
        currentlySelectedMenuButton.button.onClick.Invoke();
    }

    private void OnToggleVisible()
    {
        if (!toggleActiveWithPlayerInput)
            return;
        SetMenuActive(!canvas.enabled);
    }

    public void DeactivateMenu()
    {
        if (currentlyActiveMenu != this)
            return;
        canvas.enabled = false;
        menuActive = false;
        currentlyActiveMenu = null;
        if (pauseGameWhenActive)
            Time.timeScale = 1;
    }

    public void ActivateMenu()     //will not open if a menu is already active
    {
        if (currentlyActiveMenu != null)
            return;
        menuButtons[0].SetToSelected();
        canvas.enabled = true;
        menuActive = true;
        currentlyActiveMenu = this;
        if (pauseGameWhenActive)
            Time.timeScale = 0;
    }
    public void SetMenuActive(bool state)
    {
        if (state)
            ActivateMenu();
        else
            DeactivateMenu();
    }

    public void ForceActivateMenu()     //Will close other active menu as well, to force possibility of this menu opening possible
    {
        if (currentlyActiveMenu != null)
            currentlyActiveMenu.DeactivateMenu();
        ActivateMenu();
    }







    //Auto Inspector Functionality
    [Header("Inspector Auto Functions")]
    public bool enableAutoInspectorFunctions;
    public Vector2 firstButtonCoordinates;
    public float verticalDistance;
    public bool autoAddMenuButton;

    private void OnValidate()
    {
        if (!enableAutoInspectorFunctions)
            return;
        
        if (autoAddMenuButton)  //add a button to menu buttons
        {
            menuButtons.Add(Instantiate(menuButtonPrefab, transform).GetComponent<MenuButton>());
            MenuButton lastButton = menuButtons[menuButtons.Count - 1];
            lastButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = buttonTextColor;
            lastButton.buttonTextColor = buttonTextColor;
            lastButton.selectedButtonTextColor = selectedButtonTextColor;
            
            autoAddMenuButton = false;
        }

        Canvas temporaryCanvas = GetComponent<Canvas>();    //set canvas renderer to right value if required
        if (menuActive != temporaryCanvas.enabled)
            temporaryCanvas.enabled ^= true;

        for (int i = 0; i < menuButtons.Count; ++i) //auto align all menu buttons, set to unselected color
        {
            if (menuButtons[i] == null)
                break;
            Vector2 newButtonPosition = new Vector2(firstButtonCoordinates.x, firstButtonCoordinates.y - verticalDistance * i);
            menuButtons[i].GetComponent<RectTransform>().anchoredPosition = newButtonPosition;
            menuButtons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = buttonTextColor;
        }
        
        foreach (Transform child in transform)  //delete buttons not in menu buttons list
        {
            if (child.CompareTag("MenuButton") && !menuButtons.Contains(child.gameObject.GetComponent<MenuButton>()))
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    UnityEditor.Undo.DestroyObjectImmediate(child.gameObject);
                };
        }
    }
}
