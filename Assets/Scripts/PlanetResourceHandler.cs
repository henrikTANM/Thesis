using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProductionBuilding;

public class PlanetResourceHandler
{
    List<ResourceCount> resourceCounts = new();

    public void UpdateResourceCounts()
    {
        foreach (ResourceCount resourceCount in resourceCounts)
        {
            resourceCount.amount += resourceCount.secondAmount;
        }
    }

    public void AddPerCycle(Resource resource, int perCycle)
    {
        ResourceCount resourceCount = GetResourceCount(resource);
        if (resourceCount != null)
        {
            resourceCount.secondAmount += perCycle;
            //Debug.Log(resourceCount.perCycle + " : " + perCycle + " add");
        }
        else resourceCounts.Add(new ResourceCount(resource, 0, perCycle));
    }

    public void RemoveperCycle(Resource resource, int perCycle)
    {
        ResourceCount resourceCount = GetResourceCount(resource);
        resourceCount.secondAmount += perCycle;
    }

    public void RemoveResouce(Resource resource, int amount)
    {
        ResourceCount resourceCount = GetResourceCount(resource);
        resourceCount.amount -= amount;
    }

    public void AddResouce(Resource resource, int amount)
    {
        ResourceCount resourceCount = GetResourceCount(resource);
        if (resourceCount != null) resourceCount.amount += amount;
        else resourceCounts.Add(new ResourceCount(resource, amount, 0));
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
