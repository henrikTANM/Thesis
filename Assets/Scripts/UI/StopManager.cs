using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StopManager : MonoBehaviour
{
    private UIController uiController;
    private UniverseHandler universe;

    public VisualTreeAsset dropOffPickUpRowPrefab;

    private List<ResourceCount> cargoState = new();

    private float previousOnShipValue;
    private float onShipValue;
    private int transferCountValue;
    private float onPlanetValue;

    private Label previousOnShip;
    private Label onShip;
    private Label transferCount;
    private Label onPlanet;

    public void MakeStopManager(RouteMaker routemaker, RouteStop routeStop, ScrollView stopList, SpaceShip ship)
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<Label>("stopname").text = routeStop.GetPlanet().GetName();

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += () => { Close(routemaker); };

        SliderInt slider = root.Q<SliderInt>();
        ScrollView dropOffPickUpList = root.Q<ScrollView>("dropoffpickup");

        if (stopList.childCount > 1) 
        {
            MakeSlider(slider, routeStop, ship);
            MakeDropOffPickUpList(routeStop, dropOffPickUpList);
        }
        else
        {
            slider.SetEnabled(false);
            slider.style.visibility = Visibility.Hidden;
        }
    }

    private void Close(RouteMaker routeMaker)
    {
        routeMaker.previousStopManager = null;
        uiController.RemoveLastFromUIStack();
    }

    private void MakeDropOffPickUpList(RouteStop routeStop, ScrollView dropOffPickUpList)
    {
        VisualElement dropOffPickUpRow = dropOffPickUpRowPrefab.Instantiate();

        foreach (Resource resource in universe.allResources)
        {

            ResourceCount previousOnShipResourceCount = FindIn(routeStop.Route().GetPreviousRouteStop(routeStop).GetShipState(), resource);
            ResourceCount onShipResourceCount = FindIn(routeStop.GetShipState(), resource);
            ResourceCount onPlanetResourceCount = FindIn(routeStop.GetPlanet().GetPlanetResourceHandler().GetResourceCounts(), resource);

            previousOnShipValue = previousOnShipResourceCount == null ? 0.0f : previousOnShipResourceCount.amount;

            if (onShipResourceCount == null)
            {
                if (previousOnShipResourceCount.amount == 0.0f)
                {
                    routeStop.AddToShipState(new ResourceCount(resource, 0.0f));
                    onShipValue = 0.0f;
                }
                else
                {
                    ResourceCount newResoureCount = new ResourceCount(resource, previousOnShipResourceCount.amount);
                    routeStop.AddToShipState(newResoureCount);
                    onShipValue = newResoureCount.amount;
                }
            }
            else
            {
                onShipValue = onShipResourceCount.amount;
            }

            transferCountValue = (int)(previousOnShipValue - onShipValue);

            onPlanetValue = onPlanetResourceCount == null ? 0.0f : onPlanetResourceCount.perCycle;

            

            VisualElement resourceImage = dropOffPickUpRow.Q<VisualElement>("resourceimage");
            resourceImage.style.backgroundImage = new StyleBackground(resource.resourceSprite);
            resourceImage.style.unityBackgroundImageTintColor = new StyleColor(resource.spriteColor);

            previousOnShip = dropOffPickUpRow.Q<Label>("ponship");
            previousOnShip.text = previousOnShipValue.ToString();

            onShip = dropOffPickUpRow.Q<Label>("onship");
            onShip.text = "On Ship: " + onShipValue.ToString();

            transferCount = dropOffPickUpRow.Q<Label>("transfercount");
            transferCount.text = transferCountValue.ToString();

            onPlanet = dropOffPickUpRow.Q<Label>("onplanet");
            onPlanet.text = "Planet: " + onPlanetValue + "/cycle";

            int totalTravelTime = routeStop.Route().GetTotalTravelTime();

            Button toShip = dropOffPickUpRow.Q<Button>("toship");
            toShip.clicked += () => { Transfer(totalTravelTime, 1); };
            Button toPlanet = dropOffPickUpRow.Q<Button>("toplanet");
            toPlanet.clicked += () => { Transfer(totalTravelTime, -1); };

            dropOffPickUpList.Add(dropOffPickUpRow);
        }
    }

    private void Transfer(int totalTravelTime, float factor)
    {
        int 
    }

    private void MakeSlider(SliderInt slider, RouteStop routeStop, SpaceShip ship)
    {
        Orbiter previous = routeStop.Route().GetPreviousRouteStop(routeStop).GetPlanet().GetOrbiter();
        Orbiter orbiter = routeStop.GetPlanet().GetOrbiter();
        int minTravelTIme = routeStop.GetMinTravelTimeInCycles(previous, orbiter, ship.GetMaxAcceleration());

        slider.SetEnabled(true);
        slider.style.visibility = Visibility.Visible;

        slider.highValue = minTravelTIme + 10;
        slider.lowValue = minTravelTIme;
        slider.value = routeStop.GetTravelTime();
        slider.label = "Cycles to travel to:" + slider.value.ToString();
        routeStop.SetTravelTime(slider.value);

        slider.RegisterValueChangedCallback(value =>
        {
            routeStop.SetTravelTime(value.newValue);
            slider.label = value.newValue.ToString();
        });
    }

    private ResourceCount FindIn(List<ResourceCount> resourceCounts, Resource by)
    {
        foreach (ResourceCount rC in resourceCounts)
        {
            //print(rC.resource == by);
            if (rC.resource == by) return rC;
        }
        return null;
    }
}
