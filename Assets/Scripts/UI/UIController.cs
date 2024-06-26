using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField] private StyleSheet styleSheet;

    [SerializeField] private UIDocument gameUI;

    [SerializeField] private GameObject escapeMenuPrefab;
    GameObject escapeMenu;
    [SerializeField] private GameObject shipsMenuPrefab;
    GameObject shipsMenu;
    [SerializeField] private GameObject planetsMenuPrefab;
    GameObject planetsMenu;

    private UIStack uiStack = new();

    [SerializeField] private Sprite timeRunningImage;
    [SerializeField] private Sprite timeStoppedImage;

    private Button timeButton;

    private UniverseHandler universe;
    private PlayerInventory inventory;

    private ProgressBar timer;

    private void Awake()
    {
        InputEvents.OnTimeStateChange += ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu += ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu += MakeEscapeMenu;
        GameEvents.OnMoneyUpdate += UpdateMoney;
    }

    private void OnDestroy()
    {
        InputEvents.OnTimeStateChange -= ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu -= ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu -= MakeEscapeMenu;
        GameEvents.OnMoneyUpdate -= UpdateMoney;
    }

    private void Start()
    {
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
        inventory = GameObject.Find("PlayerInventory").GetComponent<PlayerInventory>();
        UpdateMoney();
        InitiateGameUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (uiStack.IsEmpty())
            {
                InputEvents.EscapeMenu();
            }
            else
            {
                //print(universe.escapeMenuDisplayed + " : " + universe.routeMakerDisplayed);
                if (universe.escapeMenuDisplayed) universe.HandleEscapeMenu();
                if (universe.routeMakerDisplayed) universe.HandleRouteMaker();
                RemoveLastFromUIStack();
            }
        }
        timer.value = universe.timeCycleValue;
    }

    //main UI buttons

    private void InitiateGameUI()
    {
        VisualElement root = gameUI.rootVisualElement;

        Button escapeButton = root.Q<Button>("escapemenubutton");
        escapeButton.clicked += InputEvents.EscapeMenu;

        Button shipsButton = root.Q<Button>("shipsbutton");
        shipsButton.clicked += MakeShipsMenu;

        Button planetsButton = root.Q<Button>("planetsbutton");
        planetsButton.clicked += MakePlanetsMenu;

        Button clusterViewButton = root.Q<Button>("clusterviewbutton");
        clusterViewButton.clicked += InputEvents.ClusterView;

        timeButton = root.Q<Button>("timebutton");
        timeButton.clicked += InputEvents.TimeStateChange;

        Button systemViewButton = root.Q<Button>("systemviewbutton");
        systemViewButton.clicked += InputEvents.SystemView;

        timer = root.Q<ProgressBar>("time");
        timer.lowValue = 0.0f;
        timer.highValue = universe.cycleLength;
    }

    public void SetGameUIActive(bool active)
    {
        VisualElement root = gameUI.rootVisualElement;
        root.Q<VisualElement>("topcontainer").SetEnabled(active);
        root.Q<VisualElement>("topcontainer").style.visibility = active ? Visibility.Visible : Visibility.Hidden;
        root.Q<VisualElement>("rightcontainer").SetEnabled(active);
        root.Q<VisualElement>("rightcontainer").style.visibility = active ? Visibility.Visible : Visibility.Hidden;
    }

    // ships menu functions

    public void ChangeTimeButtonIcon()
    {
        timeButton.style.backgroundImage =
            new StyleBackground(universe.timeRunning ? timeRunningImage : timeStoppedImage);

        timeButton.style.unityBackgroundImageTintColor =
            universe.timeRunning ? new Color(255f / 255f, 243f / 255f, 176f / 255f) : new Color(158f / 255f, 42f / 255f, 43f / 255f);
    }

    public void AddToUIStack(UIElement uiElement, bool disPlayPrevious)
    {   
        uiStack.Add(uiElement, disPlayPrevious);
    }

    public void RemoveLastFromUIStack()
    {
        uiStack.RemoveLast();
    }

    public void ClearUIStack()
    {
        uiStack.Clear();
    }

    public bool UIDisplayed() { return !uiStack.IsEmpty(); }

    public void MakeEscapeMenu() 
    {
        if (escapeMenu != null) return;
        escapeMenu = Instantiate(escapeMenuPrefab);
        UIDocument escapeMenuUI = escapeMenu.GetComponent<UIDocument>();
        escapeMenu.GetComponent<EscapeMenu>().MakeEscapeMenu();
        AddToUIStack(new UIElement(escapeMenu, escapeMenuUI), false); 
    }
    public void MakeShipsMenu() 
    {
        if (shipsMenu != null) return;
        shipsMenu = Instantiate(shipsMenuPrefab);
        UIDocument shipsMenuUI = shipsMenu.GetComponent<UIDocument>();
        shipsMenu.GetComponent<ShipsMenu>().MakeShipsMenu();
        AddToUIStack(new UIElement(shipsMenu, shipsMenuUI), false);
    }

    public void MakePlanetsMenu()
    {
        if (planetsMenu != null) return;
        planetsMenu = Instantiate(planetsMenuPrefab);
        UIDocument planetsMenuUI = planetsMenu.GetComponent<UIDocument>();
        planetsMenu.GetComponent<PlanetsMenu>().MakePlanetsMenu();
        AddToUIStack(new UIElement(planetsMenu, planetsMenuUI), false);
    }

    private void UpdateMoney()
    {
        gameUI.rootVisualElement.Q<Label>("moneyvalue").text = inventory.GetMoney().ToString();
    }
}
