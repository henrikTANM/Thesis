using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;

public class StopManager : MonoBehaviour
{
    private UIController uiController;
    private UniverseHandler universe;

    public VisualTreeAsset dropOffPickUpRowPrefab;

    private SliderInt slider;

    private int totalTravelTime;

    public void MakeStopManager(RouteMaker routemaker, RouteStop routeStop, ScrollView stopList, SpaceShip ship)
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<Label>("stopname").text = routeStop.GetPlanet().GetName();

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += () => { Close(routemaker); };

        slider = root.Q<SliderInt>();
        ScrollView dropOffPickUpList = root.Q<ScrollView>("dropoffpickup");

        if (stopList.childCount > 1) 
        {
            MakeSlider(slider, routeStop, ship);
            MakeDropOffPickUpList(slider, routeStop, dropOffPickUpList);
        }
        else
        {
            slider.SetEnabled(false);
            slider.style.visibility = Visibility.Hidden;
        }
    }

    public void Close(RouteMaker routeMaker)
    {
        routeMaker.previousStopManager = null;
        uiController.RemoveLastFromUIStack();
    }

    private void MakeDropOffPickUpList(SliderInt slider, RouteStop routeStop, ScrollView dropOffPickUpList)
    {
        totalTravelTime = routeStop.Route().GetTotalTravelTime();

        foreach (Resource resource in universe.allResources)
        {
            VisualElement dropOffPickUpRow = dropOffPickUpRowPrefab.Instantiate();

            totalTravelTime = routeStop.Route().GetTotalTravelTime();

            ResourceCount previousOnShipResourceCount = FindIn(routeStop.Route().GetPreviousRouteStop(routeStop).GetShipState(), resource);
            ResourceCount onShipResourceCount = FindIn(routeStop.GetShipState(), resource);
            ResourceCount onPlanetResourceCount = routeStop.GetPlanet().GetPlanetResourceHandler().GetResourceCount(resource);

            float previousOnShipValue = 0.0f;
            float onShipValue = 0.0f;
            float transferCountValue = 0;
            float onPlanetValue = 0.0f;

            previousOnShipValue = previousOnShipResourceCount == null ? 0.0f : previousOnShipResourceCount.amount;

            if (onShipResourceCount == null)
            {
                if (previousOnShipValue == 0.0f) { onShipValue = 0.0f; }
                else { onShipValue = previousOnShipValue; }
            }
            else
            {
                onShipValue = onShipResourceCount.amount;
            }

            transferCountValue = onShipValue - previousOnShipValue;

            onPlanetValue = (onPlanetResourceCount == null ? 0.0f : onPlanetResourceCount.secondAmount) + (-transferCountValue / (float)totalTravelTime);


            VisualElement resourceImage = dropOffPickUpRow.Q<VisualElement>("resourceimage");
            resourceImage.style.backgroundImage = new StyleBackground(resource.resourceSprite);
            resourceImage.style.unityBackgroundImageTintColor = new StyleColor(resource.spriteColor);

            Label previousOnShip = dropOffPickUpRow.Q<Label>("ponship");
            previousOnShip.text = "On Ship: " + previousOnShipValue.ToString();

            Label onShip = dropOffPickUpRow.Q<Label>("onship");
            onShip.text = onShipValue.ToString();

            Label transferCount = dropOffPickUpRow.Q<Label>("transfercount");
            transferCount.text = transferCountValue.ToString();

            Label onPlanet = dropOffPickUpRow.Q<Label>("onplanet");
            onPlanet.text = "Planet: " + onPlanetValue.ToString() + "/cycle";

            void Transfer(float factor)
            {
                float newTransferCountValue = float.Parse(transferCount.text) + factor;
                float newOnShipValue = float.Parse(onShip.text) + factor;
                float newOnPlanetValue = float.Parse(onPlanet.text.Substring(
                    onPlanet.text.IndexOf(":") + 2,
                    onPlanet.text.IndexOf("/") - (onPlanet.text.IndexOf(":") + 2)
                    )) - (factor / (float)totalTravelTime);

                if ((newOnShipValue >= 0.0f) & (newOnPlanetValue >= 0.0f))
                {
                    transferCount.text = newTransferCountValue.ToString();
                    onShip.text = newOnShipValue.ToString();
                    onPlanet.text = "Planet: " + newOnPlanetValue.ToString() + "/cycle";

                    routeStop.ModifyShipState(resource, newOnShipValue);
                    routeStop.ModifyPlanetState(resource, -newTransferCountValue / (float)totalTravelTime);
                }
            }

            Button toShip = dropOffPickUpRow.Q<Button>("toship");
            toShip.clicked += () => { Transfer(1.0f); };
            Button toPlanet = dropOffPickUpRow.Q<Button>("toplanet");
            toPlanet.clicked += () => { Transfer(-1.0f); };

            dropOffPickUpList.Add(dropOffPickUpRow);

            slider.RegisterValueChangedCallback(value =>
            {
                routeStop.SetTravelTime(value.newValue);
                slider.label = value.newValue.ToString();
                totalTravelTime = routeStop.Route().GetTotalTravelTime();

                float change = -(float.Parse(transferCount.text)) / (float)totalTravelTime;
                onPlanetValue = (onPlanetResourceCount == null ? 0.0f : onPlanetResourceCount.secondAmount) + change;
                onPlanet.text = "Planet: " + onPlanetValue.ToString() + "/cycle";
                routeStop.ModifyPlanetState(resource, change);

            });
        }
    }

    private void MakeSlider(SliderInt slider, RouteStop routeStop, SpaceShip ship)
    {

        slider.SetEnabled(true);
        slider.style.visibility = Visibility.Visible;

        int minTravelTime = routeStop.GetMinTravelTimeForRoutes();

        slider.highValue = minTravelTime + 10;
        slider.lowValue = minTravelTime;
        slider.value = routeStop.GetTravelTime();
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
