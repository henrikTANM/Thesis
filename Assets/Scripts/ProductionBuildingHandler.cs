using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProductionBuilding;

public class ProductionBuildingHandler
{
    private Planet planet;

    private bool active = false;
    private bool manuallyDeactivated = false;

    private string name;
    private int upkeep;
    private ResourceAmount outputResource = new();
    private List<ResourceAmount> inputResources = new();
    private List<ResourceAmount> costAmounts = new();

    public ProductionBuildingHandler(ProductionBuilding productionBuilding, Planet planet)
    {
        name = productionBuilding.name;
        upkeep = productionBuilding.upkeep;
        outputResource = productionBuilding.outputResource;
        foreach (ResourceAmount inputResource in productionBuilding.inputResources)
        {
            inputResources.Add(inputResource);
        }
        foreach (ResourceAmount cost in productionBuilding.cost)
        {
            costAmounts.Add(cost);
        }
        this.planet = planet;
        this.planet.AddProductionBuildingHandler(this);
    }

    public void SetActive(bool active)
    {
        this.active = active;

        PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();

        if (active)
        {
            planetResourceHandler.AddPerCycle(outputResource.resource, outputResource.amount);
            foreach (ResourceAmount inputResourcePerCycle in inputResources)
            {
                planetResourceHandler.RemoveperCycle(inputResourcePerCycle.resource, inputResourcePerCycle.amount);
            }
        }
        else
        {
            planetResourceHandler.RemoveperCycle(outputResource.resource, -outputResource.amount);
            foreach (ResourceAmount inputResourcePerCycle in inputResources)
            {
                planetResourceHandler.AddPerCycle(inputResourcePerCycle.resource, inputResourcePerCycle.amount);
            }
        }
    }

    public string GetName() { return name; }

    public int GetUpkeep() { return upkeep; }

    public ResourceAmount GetOutputResource()
    {
        return outputResource;
    }

    public List<ResourceAmount> GetInputResources()
    {
        return inputResources;
    }

    public List<ResourceAmount> GetCostAmounts()
    {
        return costAmounts;
    }

    public void SetManuallyDeActivated(bool deactivated)
    {
        SetActive(!deactivated);
        manuallyDeactivated = deactivated;
        planet.UpdateResourceDisplays();
    }

    public bool IsActive() { return active; }
    public bool IsManuallyDeActivated() { return manuallyDeactivated; }

    public bool CanBeActived()
    {
        PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();
        foreach (ResourceAmount inputResource in inputResources) {
            ResourceCount resourcecount = planetResourceHandler.GetResourceCount(inputResource.resource);
            if (resourcecount != null)
            {
                if (resourcecount.secondAmount < inputResource.amount) return false;
            }
            else return false;
        }
        return true;
    }
}
