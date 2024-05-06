using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipViewer : MonoBehaviour
{
    private UIController uiController;
    private PlayerInventory inventory;

    public VisualTreeAsset createRouteButtonPrefab;
    public VisualTreeAsset routeButtonsPrefab;
    public VisualTreeAsset routeStopNamePrefab;
    public VisualTreeAsset routeStopInfoPrefab;

    [SerializeField] private GameObject routeMakerPrefab;
    private GameObject routeMaker;

    private ScrollView routeStopList;
    private VisualElement routeStopInfo;
    private VisualElement routeButtons;

    private Label routeStatus;

    public void MakeShipViewer(ShipsMenu shipsMenu, SpaceShip ship)
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        inventory = GameObject.Find("PlayerInventory").GetComponent<PlayerInventory>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<Label>("name").text = ship.GetName();

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += uiController.RemoveLastFromUIStack;

        root.Q<Label>("acceleration").text = "Acceleration rate: " + ship.GetMaxAcceleration().ToString();
        root.Q<Label>("fuel").text = "Fuel capacity: " + ship.GetFuelCapacity().ToString();
        root.Q<Label>("cargo").text = "Cargo capacity: " + ship.GetCargoCapacity().ToString();

        Button sellButton = root.Q<Button>("sellbutton");
        sellButton.clicked += () =>  Sell(ship);

        routeStatus = root.Q<Label>("status");
        UpdateRouteStatus(ship);

        routeStopList = root.Q<ScrollView>("routestoplist");
        routeStopInfo = root.Q<ScrollView>("stopinfo");
        UpdateRouteInfo(ship);

        routeButtons = root.Q<VisualElement>("buttons");
        UpdateButtons(ship);
    }

    public void UpdateRouteInfo(SpaceShip ship)
    {
        routeStopList.Clear();

        if (ship.HasRoute())
        {
            foreach (RouteStop routeStop in ship.GetRoute().GetRouteStops())
            {
                VisualElement routeStopName = routeStopNamePrefab.Instantiate();
                Button routeStopNameButton = routeStopName.Q<Button>("button");
                routeStopNameButton.text = routeStop.GetPlanet().GetName();
                routeStopNameButton.clicked += () => UpdateRouteStopInfo(routeStop);
                routeStopList.Add(routeStopName);
                if (routeStop.GetIndex() == 0) { UpdateRouteStopInfo(routeStop); }
            }
        }
    }

    public void UpdateRouteStopInfo(RouteStop routeStop)
    {
        routeStopInfo.Clear();

        RouteStop previous = routeStop.Route().GetPreviousRouteStop(routeStop);
        foreach (ResourceCount resourceCount in routeStop.GetShipState())
        {
            ResourceCount previousCount = previous.GetInShipState(resourceCount.resource);
            VisualElement infoRow = routeStopInfoPrefab.Instantiate();
            infoRow.Q<VisualElement>("res").style.backgroundImage = new StyleBackground(resourceCount.resource.resourceSprite);
            infoRow.Q<VisualElement>("res").style.unityBackgroundImageTintColor = new StyleColor(resourceCount.resource.spriteColor);
            infoRow.Q<Label>("previous").text = (previousCount != null ? previousCount.amount : 0).ToString();
            infoRow.Q<Label>("new").text = resourceCount.amount.ToString();
            routeStopInfo.Add(infoRow);
        }
    }

    public void UpdateButtons(SpaceShip ship)
    {
        routeButtons.Clear();

        if (ship.HasRoute())
        {
            VisualElement routeButtonsContainer = routeButtonsPrefab.Instantiate();

            Button cancelButton = routeButtonsContainer.Q<Button>("cancelbutton");
            cancelButton.clicked += () => CancelRoute(ship);

            Button pauseButton = routeButtonsContainer.Q<Button>("pausebutton");
            pauseButton.clicked += () =>
            {
                ship.ChangeRoutePaused();
                pauseButton.text = ship.IsRoutePaused() ? "Unpause route" : "Pause route";
                UpdateRouteStatus(ship);
            };

            routeButtons.Add(routeButtonsContainer);
        }
        else
        {
            VisualElement createButtonContainer = createRouteButtonPrefab.Instantiate();

            Button createButton = createButtonContainer.Q<Button>("createbutton");
            createButton.clicked += () => { MakeRouteMaker(this, ship); };

            routeButtons.Add(createButtonContainer);
        }
    }

    public void CancelRoute(SpaceShip ship)
    {
        ship.RemoveRoute();
        UpdateRouteInfo(ship);
        UpdateRouteStatus(ship);
        routeStopInfo.Clear();
        UpdateButtons(ship);
    }

    private void Sell(SpaceShip ship)
    {
        CancelRoute(ship);
        inventory.RemoveShip(ship);
        ship.Sell();
        uiController.RemoveLastFromUIStack();
    }

    public void UpdateRouteStatus(SpaceShip ship)
    {
        routeStatus.text = (ship.HasRoute() ? (ship.IsRoutePaused() ? "Route paused" : "Route") : "No route").ToString();
    }

    private void MakeRouteMaker(ShipViewer shipViewer, SpaceShip ship)
    {
        routeMaker = Instantiate(routeMakerPrefab);
        UIDocument routeMakerUI = routeMaker.GetComponent<UIDocument>();
        routeMaker.GetComponent<RouteMaker>().MakeRouteMaker(shipViewer, ship);
        uiController.AddToUIStack(new UIElement(routeMaker, routeMakerUI), false);
    }
}
