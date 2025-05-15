using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField] private StyleSheet styleSheet;

    [SerializeField] private UIDocument gameUI;

    [SerializeField] private GameObject endingMenuPrefab;
    GameObject endingMenu;
    [SerializeField] private GameObject escapeMenuPrefab;
    GameObject escapeMenu;
    [SerializeField] private GameObject shipViewerPrefab;
    GameObject shipViewer;

    private UIStack uiStack = new();

    [SerializeField] private Sprite timeRunningImage;
    [SerializeField] private Sprite timeStoppedImage;

    private Button timeButton;

    private UniverseHandler universe;

    private ProgressBar timer;

    [SerializeField] private GameObject moneyViewerPrefab;
    private GameObject moneyViewer;

    private VisualElement sideMenu;
    private Button sideMenuButton;
    private bool sideMenuOpen = false;
    public static bool mouseOnSideMenu = false;

    private VisualElement planetsList;
    private Button planetsButton;
    private bool planetsListOpen = false;
    [SerializeField] private VisualTreeAsset planetsListButtonTemplate;

    private VisualElement shipsList;
    private Button shipsButton;
    private bool shipsListOpen = false;
    [SerializeField] private VisualTreeAsset shipsListButtonTemplate;

    [SerializeField] private VisualTreeAsset messageButtonTemplate;
    [SerializeField] private Sprite notificationImage;
    [SerializeField] private Color notificationColor;
    [SerializeField] private Sprite waringImage;
    [SerializeField] private Color warningColor;
    private VisualElement messageButtonsContainer;
    private List<VisualElement> messageButtonList = new();
    [SerializeField] private int maxMessagesOnScreen = 10;

    public static UIController instance;

    private void Awake()
    {
        if (instance == null) instance = this;

        InputEvents.OnTimeStateChange += ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu += ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu += MakeEscapeMenu;
    }

    private void OnDestroy()
    {
        InputEvents.OnTimeStateChange -= ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu -= ChangeTimeButtonIcon;
        InputEvents.OnEscapeMenu -= MakeEscapeMenu;
    }

    private void Start()
    {
        UpdateMoney();
        InitiateGameUI();
    }

    private void Update()
    {
        if (!UniverseHandler.instance.editActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (uiStack.IsEmpty()) InputEvents.EscapeMenu();
                else RemoveLastFromUIStack();
            }
            if (Input.GetKeyDown(UniverseHandler.GetKeyCode(KeyBind.KeyPressAction.CLEAR_NOTIFICATIONS))
                & !UniverseHandler.escapeMenuDisplayed)
            {
                messageButtonList.Clear();
                UpdateMessageButtons();
            }
            if (Input.GetKeyDown(UniverseHandler.GetKeyCode(KeyBind.KeyPressAction.CLEAR_UI))
                & !UniverseHandler.escapeMenuDisplayed) ClearUIStack();
        }
        timer.value = UniverseHandler.timeCycleValue;
    }

    private void InitiateGameUI()
    {
        VisualElement root = gameUI.rootVisualElement;
        root.RegisterCallback<NavigationSubmitEvent>((evt) =>
        {
            evt.StopPropagation();
        }, TrickleDown.TrickleDown);

        sideMenu = root.Q<VisualElement>("sidemenu");
        sideMenu.RegisterCallback<MouseEnterEvent>((evt) => mouseOnSideMenu = true);
        sideMenu.RegisterCallback<MouseLeaveEvent>((evt) => mouseOnSideMenu = false);

        sideMenuButton = root.Q<Button>("sidemenubutton");
        sideMenuButton.clicked += HandleSideMenuTransition;

        ScrollView sideMenuScrollView = root.Q<ScrollView>("list");
        sideMenuScrollView.verticalScroller.SetEnabled(false);

        planetsList = sideMenu.Q<VisualElement>("planetslist");
        shipsList = sideMenu.Q<VisualElement>("shipslist");

        planetsButton = root.Q<Button>("planetsbutton");
        planetsButton.clicked += HandlePlanetListButton;

        shipsButton = root.Q<Button>("shipsbutton");
        shipsButton.clicked += HandleShipsListButton;

        messageButtonsContainer = root.Q<VisualElement>("messagelist");

        Button escapeButton = root.Q<Button>("escapemenubutton");
        escapeButton.clicked += InputEvents.EscapeMenu;

        Button clusterViewButton = root.Q<Button>("clusterviewbutton");
        clusterViewButton.clicked += InputEvents.ClusterView;

        timeButton = root.Q<Button>("timebutton");
        timeButton.clicked += InputEvents.TimeStateChange;

        Button systemViewButton = root.Q<Button>("systemviewbutton");
        systemViewButton.clicked += InputEvents.SystemView;

        VisualElement moneyContainer = root.Q<VisualElement>("statscontainer");
        moneyContainer.RegisterCallback<MouseEnterEvent>(evt => MakeMoneyViewer(evt));
        moneyContainer.RegisterCallback<MouseLeaveEvent>(evt => RemoveLastFromUIStack());

        timer = root.Q<ProgressBar>("time");
        timer.lowValue = 0.0f;
        timer.highValue = UniverseHandler.instance.cycleLength;
    }

    public static void SetGameUIActive(bool active)
    {
        VisualElement root = instance.gameUI.rootVisualElement;
        root.Q<VisualElement>("topcontainer").SetEnabled(active);
        root.Q<VisualElement>("topcontainer").style.visibility = active ? Visibility.Visible : Visibility.Hidden;
        root.Q<VisualElement>("leftcontainer").SetEnabled(active);
        root.Q<VisualElement>("leftcontainer").style.visibility = active ? Visibility.Visible : Visibility.Hidden;
        root.Q<VisualElement>("rightcontainer").SetEnabled(active);
        root.Q<VisualElement>("rightcontainer").style.visibility = active ? Visibility.Visible : Visibility.Hidden;
    }

    // ships menu functions

    public static void ChangeTimeButtonIcon()
    {
        instance.timeButton.style.backgroundImage =
            new StyleBackground(UniverseHandler.timeRunning ? instance.timeRunningImage : instance.timeStoppedImage);

        instance.timeButton.style.unityBackgroundImageTintColor =
            UniverseHandler.timeRunning ? new Color(255f / 255f, 243f / 255f, 176f / 255f) : new Color(158f / 255f, 42f / 255f, 43f / 255f);
    }

    public static void AddToUIStack(UIElement uiElement, bool disPlayPrevious)
    {   
        instance.uiStack.Add(uiElement, disPlayPrevious);
    }

    public static void RemoveLastFromUIStack()
    {
        instance.uiStack.RemoveLast();
    }

    public static void ClearUIStack()
    {
        instance.uiStack.Clear();
    }

    public static bool UIDisplayed() { return !instance.uiStack.IsEmpty(); }

    public void MakeEscapeMenu() 
    {
        if (escapeMenu != null) return;
        escapeMenu = Instantiate(escapeMenuPrefab);
        UIDocument escapeMenuUI = escapeMenu.GetComponent<UIDocument>();
        escapeMenu.GetComponent<EscapeMenu>().MakeEscapeMenu();
        AddToUIStack(new UIElement(escapeMenu, escapeMenuUI), false);
    }

    public void MakeEndingMenu()
    {
        SetGameUIActive(false);
        endingMenu = Instantiate(endingMenuPrefab);
        UIDocument endingMenuUI = endingMenu.GetComponent<UIDocument>();
        endingMenu.GetComponent<EndingMenu>().MakeEndingMenu();
        AddToUIStack(new UIElement(endingMenu, endingMenuUI), false);
    }

    public static void MakeShipViewer(SpaceShipHandler spaceShipHandler)
    {
        GameObject shipViewer = Instantiate(instance.shipViewerPrefab);
        UIDocument shipViewerUI = shipViewer.GetComponent<UIDocument>();
        shipViewer.GetComponent<ShipViewer>().MakeShipViewer(spaceShipHandler);
        AddToUIStack(new UIElement(shipViewer, shipViewerUI), false);
    }

    public void MakeMoneyViewer(MouseEnterEvent evt)
    {
        Vector2 mousePos = evt.mousePosition;
        moneyViewer = Instantiate(moneyViewerPrefab);
        UIDocument moneyViewerUI = moneyViewer.GetComponent<UIDocument>();
        moneyViewer.GetComponent<MoneyViewer>().MakeMoneyViewer(mousePos);
        AddToUIStack(new UIElement(moneyViewer, moneyViewerUI), true);
        moneyViewer.GetComponent<UIDocument>().sortingOrder = 2;
    }

    public static void UpdateMoney()
    {
        instance.gameUI.rootVisualElement.Q<Label>("moneyvalue").text = 
            PlayerInventory.instance.moneyAmount.ToString() + 
            (PlayerInventory.instance.moneyChange < 0 ? "" : "+") + 
            PlayerInventory.instance.moneyChange.ToString();
    }

    private void HandleSideMenuTransition()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.SIDE_BUTTON);

        if (sideMenuOpen)
        {
            sideMenu.AddToClassList("items_closed");
            sideMenuButton.AddToClassList("sidemenubutton_closed");
        }
        else
        {
            sideMenu.RemoveFromClassList("items_closed");
            sideMenuButton.RemoveFromClassList("sidemenubutton_closed");
        }

        sideMenuOpen = !sideMenuOpen;
    }

    private void HandlePlanetListButton()
    {
        VisualElement icon = planetsButton.Q<VisualElement>("icon");
        if (planetsListOpen) icon.RemoveFromClassList("gat_icon_open");
        else icon.AddToClassList("gat_icon_open");

        planetsListOpen = !planetsListOpen;
        planetsList.Clear();
        UpdatePlanetsList();
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_SELECT);
    }

    public static void UpdatePlanetsList()
    {
        if (instance.planetsListOpen)
        {
            instance.planetsList.Clear();
            foreach (Planet planet in UniverseHandler.GetManagedPlanets())
            {
                Button planetButton = instance.planetsListButtonTemplate.Instantiate().Q<Button>("button");
                planetButton.text = planet.name;
                planetButton.clicked += () => 
                {
                    if (UniverseHandler.SelectedPlanetEquals(planet)) planet.ShowPlanetMenu(true);
                    else UniverseHandler.AddMoveToPlanet(planet);
                };
                instance.planetsList.Add(planetButton);
            }
        }
    }

    private void HandleShipsListButton()
    {
        VisualElement icon = shipsButton.Q<VisualElement>("icon");
        if (shipsListOpen) icon.RemoveFromClassList("gat_icon_open");
        else icon.AddToClassList("gat_icon_open");

        shipsListOpen = !shipsListOpen;
        shipsList.Clear();
        UpdateShipsList();
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_SELECT);
    }

    private static void HandleSpaceShipButton(SpaceShipHandler spaceShipHandler)
    {
        ClearUIStack();
        MakeShipViewer(spaceShipHandler);
    }

    public static void UpdateShipsList()
    {
        if (instance.shipsListOpen)
        {
            instance.shipsList.Clear();
            foreach (SpaceShipHandler spaceShipHandler in PlayerInventory.instance.spaceShips)
            {
                Button shipButton = instance.shipsListButtonTemplate.Instantiate().Q<Button>("button");
                shipButton.Q<Label>("name").text = spaceShipHandler.name;
                string routeInfo = "No route";
                if (spaceShipHandler.HasRoute())
                {
                    string home = spaceShipHandler.route.home.name;
                    string destination = spaceShipHandler.route.destination.name;
                    if (!spaceShipHandler.route.active) 
                        routeInfo = "Paused, " + home + " <-> " + destination;
                    else
                    {
                        if (spaceShipHandler.state.Equals(SpaceShipHandler.SpaceShipState.AT_HOME) |
                            spaceShipHandler.state.Equals(SpaceShipHandler.SpaceShipState.MOVING_TO_DESTINATION)) 
                            routeInfo = "Moving, " + home + " -> " + destination;
                        if (spaceShipHandler.state.Equals(SpaceShipHandler.SpaceShipState.AT_DESTINATION) |
                            spaceShipHandler.state.Equals(SpaceShipHandler.SpaceShipState.MOVING_TO_HOME)) 
                            routeInfo = "Moving, " + destination + " -> " + home;
                    }
                }
                shipButton.Q<Label>("route").text = routeInfo;
                shipButton.clicked += () => HandleSpaceShipButton(spaceShipHandler);
                instance.shipsList.Add(shipButton);
            }
        }
    }

    public static void UpdateMessageButtons()
    {
        instance.messageButtonsContainer.Clear();
        for (int i = instance.messageButtonList.Count - 1; i >= instance.messageButtonList.Count - instance.maxMessagesOnScreen; i--)
        {
            if (i < 0) break;
            instance.messageButtonsContainer.Add(instance.messageButtonList.ElementAt(i));
        }
    }

    public static void AddMessage(Message message)
    {
        Button messageButton = instance.messageButtonTemplate.Instantiate().Q<Button>("messagebutton");
        messageButton.RegisterCallback<MouseUpEvent>(evt =>
        {
            if (evt.button == 0)
            {
                bool validMessage = HandleMessageButton(message);
                if (!validMessage)
                {
                    instance.messageButtonList.Remove(messageButton);
                    UpdateMessageButtons();
                    SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
                }
            }
            else if (evt.button == 1)
            {
                instance.messageButtonList.Remove(messageButton);
                UpdateMessageButtons();
                SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
            }
        });
        Label messageLabel = messageButton.Q<Label>("message");
        messageLabel.text = message.message;
        VisualElement messageImagae = messageButton.Q<VisualElement>("image");
        bool notification = message.messageType.Equals(Message.MessageType.NOTIFICATION);
        messageImagae.style.backgroundImage =
                new StyleBackground(notification ? instance.notificationImage : instance.waringImage);
        messageImagae.style.unityBackgroundImageTintColor =
            new StyleColor(notification ? instance.notificationColor : instance.warningColor);
        instance.messageButtonList.Add(messageButton);

        UpdateMessageButtons();
        SoundFX.PlayAudioClip(message.messageType.Equals(Message.MessageType.NOTIFICATION) ?
            SoundFX.AudioType.NOTIFICATION :
            SoundFX.AudioType.WARNING);
    }

    private static bool HandleMessageButton(Message message)
    {
        if (message.senderType.Equals(Message.SenderType.STAR))
        {
            MessageSender<Star> messageSender = (MessageSender<Star>)message.sender;
            Star star = messageSender.sender;
            UniverseHandler.AddMoveToStar(star);
        }
        else if (message.senderType.Equals(Message.SenderType.PLANET))
        {
            MessageSender<Planet> messageSender = (MessageSender<Planet>)message.sender;
            Planet planet = messageSender.sender;
            if (!UniverseHandler.SelectedPlanetEquals(planet)) UniverseHandler.AddMoveToPlanet(planet);
        }
        else if(message.senderType.Equals(Message.SenderType.PRODUCTIONBUILDING))
        {
            MessageSender<ProductionBuildingHandler> messageSender = (MessageSender<ProductionBuildingHandler>)message.sender;
            ProductionBuildingHandler productionBuildingHandler = messageSender.sender;
            Planet planet = productionBuildingHandler.planet;
            if (!planet.productionBuildingHandlers.Contains(productionBuildingHandler)) return false;
            bool isSelectedPlanet = UniverseHandler.SelectedPlanetEquals(planet);
            if (!isSelectedPlanet) UniverseHandler.AddMoveToPlanet(planet);
            else planet.ShowPlanetMenu(true);
            planet.ShowProductionBuildingViewer(isSelectedPlanet, productionBuildingHandler.buildingSlot);
        }
        else if (message.senderType.Equals(Message.SenderType.DISCOVERYHUB))
        {
            MessageSender<DiscoveryHubHandler> messageSender = (MessageSender<DiscoveryHubHandler>)message.sender;
            DiscoveryHubHandler discoveryHubHandler = messageSender.sender;
            Planet planet = discoveryHubHandler.planet;
            if (planet.discoveryHubHandler == null) return false;
            bool isSelectedPlanet = UniverseHandler.SelectedPlanetEquals(planet);
            if (!isSelectedPlanet) UniverseHandler.AddMoveToPlanet(planet);
            else planet.ShowPlanetMenu(true);
            planet.ShowSpecialBuildingViewer(isSelectedPlanet);
        }
        else if (message.senderType.Equals(Message.SenderType.BHCF))
        {
            MessageSender<BHCFHandler> messageSender = (MessageSender<BHCFHandler>)message.sender;
            BHCFHandler bhcfHandler = messageSender.sender;
            Planet planet = bhcfHandler.planet;
            if (planet.bhcfHandler == null) return false;
            bool isSelectedPlanet = UniverseHandler.SelectedPlanetEquals(planet);
            if (!isSelectedPlanet) UniverseHandler.AddMoveToPlanet(planet);
            else planet.ShowPlanetMenu(true);
            planet.ShowSpecialBuildingViewer(isSelectedPlanet);
        }
        else if (message.senderType.Equals(Message.SenderType.ROUTE))
        {
            MessageSender<Route> messageSender = (MessageSender<Route>)message.sender;
            MakeShipViewer(messageSender.sender.spaceShipHandler);
        }
        return true;
    }
}
