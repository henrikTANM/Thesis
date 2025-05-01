using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoveryHubHandler : ResourceSource
{
    public Planet planet;

    public List<ResourceAmount> costResources = new();
    public ResourceFactor inputFactor;

    public DiscoveryHubHandler(
        SpecialBuilding discoveryHub, 
        ResourceAmount inputResource,
        Planet planet)
    {
        name = discoveryHub.name;
        active = true;
        this.planet = planet;

        foreach (ResourceAmount cost in discoveryHub.cost) costResources.Add(cost);

        PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();
        inputFactor = new ResourceFactor(new ResourceAmount(inputResource.resource, -inputResource.amount), this);
        planetResourceHandler.AddResourceFactor(inputFactor);
        planetResourceHandler.UpdateResourcePerCycles();
    }

    public void RemoveResourceFactors()
    {
        PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();
        planetResourceHandler.RemoveResourceFactor(inputFactor);
    }

    public override void SetActive(bool active, Planet planet, string message)
    {
        base.SetActive(active, planet, message);
        UIController.AddMessage(new Message(
            message != null ? message : name + " on " + planet.name + " has been deactivated: not enough input resources.",
            Message.MessageType.WARNING,
            new MessageSender<DiscoveryHubHandler>(this),
            Message.SenderType.DISCOVERYHUB
            ));
    }
}
