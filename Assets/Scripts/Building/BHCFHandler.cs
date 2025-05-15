using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BHCFHandler : ResourceSource
{
    public Planet planet;

    [NonSerialized] public List<ResourceAmount> costResources = new();
    [NonSerialized] public List<ResourceFactor> inputFactors = new();
    [NonSerialized] public int progress;
    [NonSerialized] public int progressRate;

    public BHCFHandler(
        SpecialBuilding blackHoleContainmentFacility,
        List<ResourceAmount> inputResources,
        Planet planet)
    {
        name = blackHoleContainmentFacility.name;
        active = true;
        type = Type.BHCF;
        this.planet = planet;
        progress = 0;
        progressRate = 5;

        foreach (ResourceAmount cost in blackHoleContainmentFacility.cost) costResources.Add(cost);

        PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();
        foreach (ResourceAmount inputResource in inputResources)
            inputFactors.Add(new ResourceFactor(new ResourceAmount(inputResource.resource, -inputResource.amount), this));
        foreach (ResourceFactor inputFactor in inputFactors) planetResourceHandler.AddResourceFactor(inputFactor);
        planetResourceHandler.UpdateResourcePerCycles();
    }

    public void RemoveResourceFactors()
    {
        PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();
        foreach (ResourceFactor inputFactor in inputFactors) planetResourceHandler.RemoveResourceFactor(inputFactor);
        planetResourceHandler.UpdateResourcePerCycles();
    }

    public override void SetActive(bool active, Planet planet, string message)
    {
        base.SetActive(active, planet, message);
        UIController.AddMessage(new Message(
            message != null ? message : name + " has been deactivated: not enough input resources.",
            Message.MessageType.WARNING,
            new MessageSender<BHCFHandler>(this),
            Message.SenderType.BHCF
            ));
    }

    public override void SetActive(bool active, string message)
    {
        base.SetActive(active, message);
        UIController.AddMessage(new Message(
            message,
            Message.MessageType.NOTIFICATION,
            new MessageSender<BHCFHandler>(this),
            Message.SenderType.BHCF
            ));


    }
}
