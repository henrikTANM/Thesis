using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Route : ResourceSource
{
    [NonSerialized] public SpaceShipHandler spaceShipHandler;
    [NonSerialized] public Planet home;
    [NonSerialized] public Planet destination;
    [NonSerialized] public float travelTime;
    [NonSerialized] public ResourceFactor fuelFactor;
    [NonSerialized] public List<Tuple<bool, ResourceFactor>> homeResourceFactors;
    [NonSerialized] public List<Tuple<bool, ResourceFactor>> destinationResourceFactors;

    public Route(SpaceShipHandler spaceShipHandler, Planet destination, float travelTime)
    {
        active = false;
        type = Type.ROUTE;
        this.spaceShipHandler = spaceShipHandler;
        home = spaceShipHandler.home;
        this.destination = destination;
        this.travelTime = travelTime;
        homeResourceFactors = new();
        destinationResourceFactors = new();

        fuelFactor = new ResourceFactor(new ResourceAmount(spaceShipHandler.spaceShip.fuelConsumption.resource, -spaceShipHandler.spaceShip.fuelConsumption.amount), this);
        home.GetPlanetResourceHandler().AddResourceFactor(fuelFactor);
        home.UpdateResourceDisplays();
    }

    public void AddHomePickupFactors(List<ResourceAmount> pickUpList)
    {
        foreach (ResourceAmount resourceAmount in pickUpList)
        {
            homeResourceFactors.Add(new(true, new ResourceFactor(new ResourceAmount(resourceAmount.resource, -resourceAmount.amount), this)));
            destinationResourceFactors.Add(new(false, new ResourceFactor(new ResourceAmount(resourceAmount.resource, resourceAmount.amount), this)));
        }
        foreach (Tuple<bool, ResourceFactor> resourceFactor in homeResourceFactors) if (resourceFactor.Item1) home.GetPlanetResourceHandler().AddResourceFactor(resourceFactor.Item2);
        foreach (Tuple<bool, ResourceFactor> resourceFactor in destinationResourceFactors) if (!resourceFactor.Item1) destination.GetPlanetResourceHandler().AddResourceFactor(resourceFactor.Item2);

        UpdateResourceDisplays();
    }

    public void AddDestinationPickupFactors(List<ResourceAmount> pickUpList)
    {
        foreach (ResourceAmount resourceAmount in pickUpList)
        {
            homeResourceFactors.Add(new(false, new ResourceFactor(new ResourceAmount(resourceAmount.resource, resourceAmount.amount), this)));
            destinationResourceFactors.Add(new(true, new ResourceFactor(new ResourceAmount(resourceAmount.resource, -resourceAmount.amount), this)));
        }
        foreach (Tuple<bool, ResourceFactor> resourceFactor in destinationResourceFactors) if (resourceFactor.Item1) destination.GetPlanetResourceHandler().AddResourceFactor(resourceFactor.Item2);
        foreach (Tuple<bool, ResourceFactor> resourceFactor in homeResourceFactors) if (!resourceFactor.Item1) home.GetPlanetResourceHandler().AddResourceFactor(resourceFactor.Item2);

        UpdateResourceDisplays();
    }

    public void RemoveHomePickupFactors()
    {
        List<Tuple<bool, ResourceFactor>> homeFactorsToRemove = new();
        List<Tuple<bool, ResourceFactor>> destinationFactorsToRemove = new();

        foreach (Tuple<bool, ResourceFactor> resourceFactor in homeResourceFactors)
        {
            if (resourceFactor.Item1)
            {
                home.GetPlanetResourceHandler().RemoveResourceFactor(resourceFactor.Item2);
                homeFactorsToRemove.Add(resourceFactor);
            }
        }
        foreach (Tuple<bool, ResourceFactor> resourceFactor in destinationResourceFactors)
        {
            if (!resourceFactor.Item1)
            {
                destination.GetPlanetResourceHandler().RemoveResourceFactor(resourceFactor.Item2);
                destinationFactorsToRemove.Add(resourceFactor);
            }
        }

        foreach (Tuple<bool, ResourceFactor> resourceFactor in homeFactorsToRemove) homeResourceFactors.Remove(resourceFactor);
        foreach (Tuple<bool, ResourceFactor> resourceFactor in destinationFactorsToRemove) destinationResourceFactors.Remove(resourceFactor);

        UpdateResourceDisplays();
    }

    public void RemoveDestinationPickupFactors()
    {
        List<Tuple<bool, ResourceFactor>> destinationFactorsToRemove = new();
        List<Tuple<bool, ResourceFactor>> homeFactorsToRemove = new();

        foreach (Tuple<bool, ResourceFactor> resourceFactor in destinationResourceFactors)
        {
            if (resourceFactor.Item1)
            {
                destination.GetPlanetResourceHandler().RemoveResourceFactor(resourceFactor.Item2);
                destinationFactorsToRemove.Add(resourceFactor);
            }
        }
        foreach (Tuple<bool, ResourceFactor> resourceFactor in homeResourceFactors)
        {
            if (!resourceFactor.Item1)
            {
                home.GetPlanetResourceHandler().RemoveResourceFactor(resourceFactor.Item2);
                homeFactorsToRemove.Add(resourceFactor);
            }
        }

        foreach (Tuple<bool, ResourceFactor> resourceFactor in destinationFactorsToRemove) destinationResourceFactors.Remove(resourceFactor);
        foreach (Tuple<bool, ResourceFactor> resourceFactor in homeFactorsToRemove) homeResourceFactors.Remove(resourceFactor);

        UpdateResourceDisplays();
    }

    public void RemoveAllResourceFactors()
    {
        home.GetPlanetResourceHandler().RemoveResourceFactor(fuelFactor);
        RemoveHomePickupFactors();
        RemoveDestinationPickupFactors();
    }

    public void UpdateResourceDisplays()
    {
        home.UpdateResourceDisplays();
        destination.UpdateResourceDisplays();
    }

    public override void SetActive(bool active, Planet planet, string message)
    {
        base.SetActive(active, planet, message);
        UIController.AddMessage(new Message(
            message != null ? message : "Route has been stopped: not enough resources at " + planet.name + ".",
            Message.MessageType.WARNING,
            new MessageSender<Route>(this),
            Message.SenderType.ROUTE
            ));
    }
}
