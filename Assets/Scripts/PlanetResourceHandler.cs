using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProductionBuilding;

public class PlanetResourceHandler
{
    List<ResourceCount> resourceCounts = new();

    public PlanetResourceHandler(List<Resource> resources)
    {
        foreach (Resource resource in resources)
        {
            resourceCounts.Add(new ResourceCount(resource, 0, 0));
        }
    }

    public void UpdateResourceCounts()
    {
        foreach (ResourceCount resourceCount in resourceCounts)
        {
            resourceCount.amount += resourceCount.secondAmount;
        }
    }

    public void AddPerCycle(Resource resource, float perCycle)
    {
        ResourceCount resourceCount = GetResourceCount(resource);
        resourceCount.secondAmount += perCycle;
    }

    public void AddResouce(Resource resource, float amount)
    {
        ResourceCount resourceCount = GetResourceCount(resource);
        resourceCount.amount += amount;
    }

    public void RemoveperCycle(Resource resource, float perCycle)
    {
        ResourceCount resourceCount = GetResourceCount(resource);
        resourceCount.secondAmount -= perCycle;
    }

    public void RemoveResouce(Resource resource, float amount)
    {
        ResourceCount resourceCount = GetResourceCount(resource);
        resourceCount.amount -= amount;
    }

    public ResourceCount GetResourceCount(Resource resource)
    {
        foreach (ResourceCount resourceCount in resourceCounts)
        {
            if (resourceCount.resource == resource) return resourceCount;
        }
        return null;
    }

    public List<ResourceCount> GetResourceCounts()
    {
        return resourceCounts;
    }
}
