using System;
using System.Collections.Generic;
using System.Diagnostics;

public class ProductionBuildingHandler : ResourceSource
{
    public Planet planet;
    public BuildingSlot buildingSlot;
    public List<ResourceAmount> costResources = new();
    public ResourceFactor upkeep;
    public ResourceFactor outputFactor;
    public List<ResourceFactor> inputFactors = new();

    public ProductionBuildingHandler(ProductionBuilding productionBuilding, Planet planet, BuildingSlot buildingSlot)
    {
        name = productionBuilding.name;
        active = true;
        this.planet = planet;
        this.buildingSlot = buildingSlot;

        foreach (ResourceAmount cost in productionBuilding.cost) costResources.Add(cost);

        upkeep = new ResourceFactor(new ResourceAmount(PlayerInventory.instance.moneyResource, -productionBuilding.upkeep), this);
        base.DebugLog(PlayerInventory.instance.ToString());
        PlayerInventory.AddMoneyFactor(upkeep);
        PlayerInventory.UpdateMoneyPerCycle();

        PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();
        outputFactor = new ResourceFactor(productionBuilding.outputResource, this);
        planetResourceHandler.AddResourceFactor(outputFactor);
        foreach (ResourceAmount inputResource in productionBuilding.inputResources) 
            inputFactors.Add(new ResourceFactor(new ResourceAmount(inputResource.resource, -inputResource.amount), this));
        foreach (ResourceFactor inputFactor in inputFactors) planetResourceHandler.AddResourceFactor(inputFactor);
        planetResourceHandler.UpdateResourcePerCycles();

        planet.AddProductionBuildingHandler(this);
    }

    public void RemoveResourceFactors()
    {
        PlayerInventory.RemoveMoneyFactor(upkeep);
        PlayerInventory.UpdateMoneyPerCycle();

        PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();
        planetResourceHandler.RemoveResourceFactor(outputFactor);
        foreach (ResourceFactor inputFactor in inputFactors) planetResourceHandler.RemoveResourceFactor(inputFactor);
        planetResourceHandler.UpdateResourcePerCycles();

        planet.RemoveProductionBuildingHandler(this);
    }

    public override void SetActive(bool active, Planet planet, string message)
    {
        base.SetActive(active, planet, message);
        UIController.AddMessage(new Message(
            message != null ? message : name + " on " + planet.name + " has been deactivated: not enough input resources.",
            Message.MessageType.WARNING,
            new MessageSender<ProductionBuildingHandler>(this),
            Message.SenderType.PRODUCTIONBUILDING
            ));
    }

    public override void SetActive(bool active, string message)
    {
        base.SetActive(active, message);
        UIController.AddMessage(new Message(
            message != null ? message : name + " on " + planet.name + " has been deactivated: not enough money for upkeep.",
            Message.MessageType.WARNING,
            new MessageSender<ProductionBuildingHandler>(this),
            Message.SenderType.PRODUCTIONBUILDING
            ));
    }
}
