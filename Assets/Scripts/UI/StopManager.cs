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
        List<ResourceCount> cargo = routeStop.Route().GetPreviousRouteStop(routeStop).GetCargoAfter();
        List<ResourceCount> planetResources = routeStop.GetPlanet().GetPlanetResourceHandler().GetResourceCounts();

        foreach (Resource resource in universe.allResources)
        {

            VisualElement dropOffPickUpRow = dropOffPickUpRowPrefab.Instantiate();

            VisualElement resourceImage = dropOffPickUpRow.Q<VisualElement>("resourceimage");
            resourceImage.style.backgroundImage = new StyleBackground(resource.resourceSprite);
            resourceImage.style.unityBackgroundImageTintColor = new StyleColor(resource.spriteColor);

            Label onShip = dropOffPickUpRow.Q<Label>("onship");
            ResourceCount shipResource = FindIn(cargo, resource);
            int resCount = shipResource == null ? 0 : shipResource.amount;
            onShip.text = "On Ship: " + resCount.ToString();

            Label resourcecount = dropOffPickUpRow.Q<Label>("resourcecount");
            resourcecount.text = "0";

            Label onPlanet = dropOffPickUpRow.Q<Label>("onplanet");
            ResourceCount planetResource = FindIn(planetResources, resource);
            print(planetResource);
            int perCycle = planetResource == null ? 0 : planetResource.perCycle;
            onPlanet.text = "Planet: +" + perCycle + "/cycle";

            int totalTravelTime = routeStop.Route().GetTotalTravelTime();

            Button toShip = dropOffPickUpRow.Q<Button>("toship");
            toShip.clicked += () => { ModifyDropOffAmount(onShip, resourcecount, onPlanet, totalTravelTime, -1); };
            Button toPlanet = dropOffPickUpRow.Q<Button>("toplanet");
            toPlanet.clicked += () => { ModifyDropOffAmount(onShip, resourcecount, onPlanet, totalTravelTime, 1); };
            Button minusButton = dropOffPickUpRow.Q<Button>("minusbutton");
            minusButton.clicked += () => { ChangeModifier(resourcecount, -1); };
            Button plusButton = dropOffPickUpRow.Q<Button>("plusbutton");
            plusButton.clicked += () => { ChangeModifier(resourcecount, 1); };

            dropOffPickUpList.Add(dropOffPickUpRow);
        }
    }

    private void ChangeModifier(Label resourcecount, int multiplier)
    {
        int newModifier = int.Parse(resourcecount.text) + multiplier;
        if (newModifier >= 0) { resourcecount.text = newModifier.ToString(); }
    }

    private void ModifyDropOffAmount(Label value, Label change, Label target, int traveltime, int multiplier)
    {

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
