using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField] private StyleSheet styleSheet;

    [SerializeField] private UIDocument gameUI;
    [SerializeField] private UIDocument escapeMenuUI;
    [SerializeField] private UIDocument shipsMenuUI;
    private UIDocument currentUI;

    [SerializeField] private Sprite timeRunningImage;
    [SerializeField] private Sprite timeStoppedImage;

    private Button timeButton;

    private UniverseHandler universe;
    private PlayerInventory playerInventory;

    private void Awake()
    {
        InputEvents.OnTimeStateChange += ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu += ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu += SetEscapeMenu;
        ResourceEvents.OnMoneyUpdate += UpdateMoney;
    }

    private void OnDestroy()
    {
        InputEvents.OnTimeStateChange -= ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu -= ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu -= SetEscapeMenu;
        ResourceEvents.OnMoneyUpdate -= UpdateMoney;
    }

    private void Start()
    {
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
        playerInventory = GameObject.Find("PlayerInventory").GetComponent<PlayerInventory>();
        UpdateMoney();

        InitiateUIButtons();
        InitiateEscapeMenuButtons();
        InitiateShipMenuFunctions();

        SetUIActive(escapeMenuUI, false);
        SetUIActive(shipsMenuUI, false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentUI == null)
            {
                InputEvents.EscapeMenu();
            }
            else
            {
                UnSetCurrentUI();
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (currentUI == null)
            {
                SetShipsMenu();
            }
            else if (currentUI == shipsMenuUI)
            {
                UnSetCurrentUI();
            }
        }
    }

    //main UI buttons

    private void InitiateUIButtons()
    {
        List<Button> buttons = gameUI.rootVisualElement.Query<Button>().ToList();

        Button escapeButton = buttons.ElementAt(0);
        escapeButton.clicked += InputEvents.EscapeMenu;

        Button shipsButton = buttons.ElementAt(1);
        shipsButton.clicked += SetShipsMenu;

        Button basesButton = buttons.ElementAt(2);
        // basesButton.clicked += ;

        Button clusterViewButton = buttons.ElementAt(3);
        clusterViewButton.clicked += InputEvents.ClusterView;

        timeButton = buttons.ElementAt(4);
        timeButton.clicked += InputEvents.TimeStateChange;

        Button systemViewButton = buttons.ElementAt(5);
        systemViewButton.clicked += InputEvents.SystemView;
    }

    // escape menu buttons

    private void InitiateEscapeMenuButtons()
    {
        List<Button> buttons = escapeMenuUI.rootVisualElement.Query<Button>().ToList();

        Button resumeButton = buttons.ElementAt(0);
        resumeButton.clicked += UnSetCurrentUI;

        Button saveButton = buttons.ElementAt(1);
        //saveButton.clicked += ;

        Button loadButton = buttons.ElementAt(2);
        //loadButton.clicked += ;

        Button optionsButton = buttons.ElementAt(3);
        // optionsButton.clicked += ;

        Button exitButton = buttons.ElementAt(4);
        // exitButton.clicked += ;

        Button quitButton = buttons.ElementAt(5);
        quitButton.clicked += Application.Quit;
    }

    // ships menu functions

    private void InitiateShipMenuFunctions()
    {
        List<Button> buttons = shipsMenuUI.rootVisualElement.Query<Button>().ToList();

        Button exitButton = buttons.ElementAt(0);
        exitButton.clicked += UnSetCurrentUI;
    }

    public void ChangeTimeButtonIcon()
    {
        timeButton.style.backgroundImage =
            new StyleBackground(universe.timeRunning ? timeRunningImage : timeStoppedImage);

        timeButton.style.unityBackgroundImageTintColor =
            universe.timeRunning ? new Color(0f, 180f / 256f, 50f / 256f) : new Color(180f / 256f, 0f, 50f / 256f);
    }

    public void SetUIActive(UIDocument uiDoc, bool active)
    {
        uiDoc.sortingOrder = active ? 1 : 0;
        uiDoc.rootVisualElement.style.visibility = active ? Visibility.Visible : Visibility.Hidden;

        gameUI.sortingOrder = active ? 0 : 1;
        gameUI.rootVisualElement.SetEnabled(!active);
    }

    public void SetCurrentUI(UIDocument uiDoc)
    {
        currentUI = uiDoc;
        SetUIActive(currentUI, true);
    }

    public void UnSetCurrentUI()
    {
        if (currentUI == escapeMenuUI) universe.HandleEscapeMenu();

        SetUIActive(currentUI, false);
        currentUI = null;
    }

    public UIDocument GetCurrentUI() { return currentUI; }

    private void SetEscapeMenu() { SetCurrentUI(escapeMenuUI); }
    private void SetShipsMenu() { SetCurrentUI(shipsMenuUI); }

    private void UpdateMoney()
    {
        gameUI.rootVisualElement.Q<Label>("moneyvalue").text = playerInventory.GetMoney().ToString();
    }
}
