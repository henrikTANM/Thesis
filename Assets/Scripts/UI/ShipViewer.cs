using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipViewer : MonoBehaviour
{
    private UIController uiController;

    public VisualTreeAsset createRouteButtonPrefab;
    public VisualTreeAsset routeButtonsPrefab;
    public VisualTreeAsset routeStopNamePrefab;

    [SerializeField] private GameObject routeMakerPrefab;
    private GameObject routeMaker;

    private ScrollView routeStopList;
    private VisualElement routeStopInfo;
    private VisualElement routeButtons;

    public void MakeShipViewer(ShipsMenu shipsMenu, SpaceShip ship)
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<Label>("name").text = ship.GetName();

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += uiController.RemoveLastFromUIStack;

        Button sellButton = root.Q<Button>("sellbutton");
        sellButton.clicked += ship.Sell;

        routeStopList = root.Q<ScrollView>("routestoplist");
        routeStopInfo = root.Q<ScrollView>("stopinfo");
        UpdateRouteInfo(ship);

        routeButtons = root.Q<VisualElement>("buttons");
        UpdateButtons(ship);
    }

    public void UpdateRouteInfo(SpaceShip ship)
    {
        if (ship.HasRoute())
        {
            foreach (RouteStop routeStop in ship.GetRoute().GetRouteStops())
            {
                VisualElement routeStopName = routeStopNamePrefab.Instantiate();
                Button routeStopNameButton = routeStopName.Q<Button>("button");
                routeStopNameButton.text = routeStop.GetPlanet().GetName();
                //routeStopNameButton.clicked +=
                routeStopList.Add(routeStopName);
            }
        }
    }

    public void UpdateButtons(SpaceShip ship)
    {
        routeButtons.Clear();

        if (ship.HasRoute())
        {
            VisualElement routeButtonsContainer = routeButtonsPrefab.Instantiate();

            Button cancelButton = routeButtonsContainer.Q<Button>("cancelbutton");
            cancelButton.clicked += ship.RemoveRoute;

            Button pauseButton = routeButtonsContainer.Q<Button>("pausebutton");
            pauseButton.clicked += ship.ChangeRoutePaused;

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

    private void MakeRouteMaker(ShipViewer shipViewer, SpaceShip ship)
    {
        routeMaker = Instantiate(routeMakerPrefab);
        UIDocument routeMakerUI = routeMaker.GetComponent<UIDocument>();
        routeMaker.GetComponent<RouteMaker>().MakeRouteMaker(shipViewer, ship);
        uiController.AddToUIStack(new UIElement(routeMaker, routeMakerUI), false);
    }
}
