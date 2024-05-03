using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RouteMaker : MonoBehaviour
{
    private UIController uiController;
    private UniverseHandler universe;

    private SpaceShip ship;

    public VisualTreeAsset stopPrefab;
    private ScrollView stopList;

    [SerializeField] private GameObject stopManagerPrefab;
    private GameObject stopManager;
    [NonSerialized] public GameObject previousStopManager;

    private Route route;

    public void MakeRouteMaker(ShipViewer shipViewer, SpaceShip ship)
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
        this.ship = ship;

        universe.SetActiveRouteMaker(this);
        universe.HandleRouteMaker();

        route = new(ship);

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button exitButton = root.Q<Button>("cancelbutton");
        exitButton.clicked += Close;

        Button createButton = root.Q<Button>("createbutton");
        createButton.clicked += () => 
        { 
            route.Create();
            shipViewer.UpdateRouteInfo(ship);
            shipViewer.UpdateButtons(ship);
            Close();
        };

        stopList = root.Q<ScrollView>("stoplist");
        stopList.mouseWheelScrollSize = 100.0f;
    }

    public void AddStop(Planet planet)
    {
        RouteStop routeStop = new(planet, stopList.childCount);
        VisualElement stopLine = stopPrefab.Instantiate();

        Button deletebutton = stopLine.Q<Button>("deletebutton");
        deletebutton.clicked += () => { stopList.Remove(stopLine); };

        Button stopButton = stopLine.Q<Button>("stopbutton");
        stopButton.clicked += () => { MakeStopManager(routeStop); };
        stopButton.text = planet.GetName();

        route.Add(routeStop);
        stopList.Add(stopLine);
    }

    private void Close()
    {
        uiController.RemoveLastFromUIStack();
        universe.HandleRouteMaker();
    }

    private void MakeStopManager(RouteStop routeStop)
    {
        if (previousStopManager != null) { previousStopManager.GetComponent<StopManager>().Close(this); }
        stopManager = Instantiate(stopManagerPrefab);
        UIDocument stopManagerUI = stopManager.GetComponent<UIDocument>();
        stopManager.GetComponent<StopManager>().MakeStopManager(this, routeStop, stopList, ship);
        uiController.AddToUIStack(new UIElement(stopManager, stopManagerUI), true);
        previousStopManager = stopManager;
    }
}
