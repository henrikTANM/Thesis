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

    [SerializeField] private Sprite timeRunningImage;
    [SerializeField] private Sprite timeStoppedImage;

    private Button timeButton;

    private UniverseHandler universe;

    private void Awake()
    {
        InputEvents.OnTimeStateChange += ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu += ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu += EscapeMenuState;
        InputEvents.OnShipsMenu += ShipsMenuState;
    }

    private void OnDestroy()
    {
        InputEvents.OnTimeStateChange -= ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu -= ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu -= EscapeMenuState;
        InputEvents.OnShipsMenu -= ShipsMenuState;
    }

    private void Start()
    {
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();

        InitiateUIButtons();
        InitiateEscapeMenuButtons();
        InitiateShipMenuFunctions();

        gameUI.sortingOrder = 1;
        escapeMenuUI.sortingOrder = 0;
        shipsMenuUI.sortingOrder = 0;
        escapeMenuUI.rootVisualElement.style.visibility = Visibility.Hidden;
        shipsMenuUI.rootVisualElement.style.visibility = Visibility.Hidden;
    }

    //main UI buttons

    private void InitiateUIButtons()
    {
        List<Button> buttons = gameUI.rootVisualElement.Query<Button>().ToList();

        Button escapeButton = buttons.ElementAt(0);
        escapeButton.clicked += InputEvents.EscapeMenu;

        Button shipsButton = buttons.ElementAt(1);
        shipsButton.clicked += InputEvents.ShipsMenu;

        Button basesButton = buttons.ElementAt(2);
        // basesButton.clicked += ;

        Button clusterViewButton = buttons.ElementAt(3);
        clusterViewButton.clicked += InputEvents.ClusterView;

        timeButton = buttons.ElementAt(4);
        timeButton.clicked += InputEvents.TimeStateChange;

        Button systemViewButton = buttons.ElementAt(5);
        // systemViewButton.clicked += ;
    }

    // escape menu buttons

    private void InitiateEscapeMenuButtons()
    {
        List<Button> buttons = escapeMenuUI.rootVisualElement.Query<Button>().ToList();

        Button resumeButton = buttons.ElementAt(0);
        resumeButton.clicked += InputEvents.EscapeMenu;

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
        exitButton.clicked += InputEvents.ShipsMenu;
    }

    private void ChangeTimeButtonIcon()
    {
        timeButton.style.backgroundImage =
            new StyleBackground(universe.timeRunning ? timeRunningImage : timeStoppedImage);
        timeButton.style.unityBackgroundImageTintColor =
            universe.timeRunning ? new Color(0f, 180f / 256f, 50f / 256f) : new Color(180f / 256f, 0f, 50f / 256f);
    }

    private void EscapeMenuState()
    {
        bool enabled = gameUI.rootVisualElement.enabledSelf;

        escapeMenuUI.sortingOrder = enabled ? 1 : 0;
        escapeMenuUI.rootVisualElement.style.visibility = 
            enabled ? Visibility.Visible : Visibility.Hidden;

        gameUI.sortingOrder = enabled ? 0 : 1;
        gameUI.rootVisualElement.SetEnabled(!enabled);
    }

    private void ShipsMenuState()
    {
        bool enabled = gameUI.rootVisualElement.enabledSelf;

        shipsMenuUI.sortingOrder = enabled ? 1 : 0;
        shipsMenuUI.rootVisualElement.style.visibility =
            enabled ? Visibility.Visible : Visibility.Hidden;

        gameUI.sortingOrder = enabled ? 0 : 1;
        gameUI.rootVisualElement.SetEnabled(!enabled);
    }
}
