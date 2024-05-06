using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProductionBuilding;

public class PlanetResourceHandler
{
    List<ResourceCount> resourceCounts = new();
    float rawMultiplier = 1.0f;
    float endMultiplier = 1.0f;
    List<string> rawResourceNames = new() { "Ore", "Water", "Food", "Crystal", "Gas", "Fibre"};

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

    public void AddRawMultiplier(float rawMultiplier)
    {
        this.rawMultiplier *= rawMultiplier;
        foreach (ResourceCount resourceCount in resourceCounts) 
        { 
            if (rawResourceNames.Contains(resourceCount.resource.name)) 
            { 
                resourceCount.secondAmount *= rawMultiplier; 
            } 
        }
    }

    public void RemoveRawMultipiler(float rawMultiplier)
    {
        this.rawMultiplier /= rawMultiplier;
        foreach (ResourceCount resourceCount in resourceCounts)
        {
            if (rawResourceNames.Contains(resourceCount.resource.name))
            {
                resourceCount.secondAmount /= rawMultiplier;
            }
        }
    }

    public void AddEndMultiplier(float endMultiplier)
    {
        this.endMultiplier *= endMultiplier;
        foreach (ResourceCount resourceCount in resourceCounts) 
        { 
            if (!rawResourceNames.Contains(resourceCount.resource.name)) 
            { 
                resourceCount.secondAmount *= endMultiplier; 
            } 
        }
    }

    public void RemoveEndMultipiler(float endMultiplier)
    {
        this.endMultiplier /= endMultiplier;
        foreach (ResourceCount resourceCount in resourceCounts)
        {
            if (!rawResourceNames.Contains(resourceCount.resource.name))
            {
                resourceCount.secondAmount /= endMultiplier;
            }
        }
    }

    public float GetRawMultiplier()
    {
        return rawMultiplier;
    }

    public float GetEndMultiplier()
    {
        return endMultiplier;
    }

    public void AddPerCycle(Resource resource, float perCycle)
    {
        ResourceCount resourceCount = GetResourceCount(resource);
        resourceCount.secondAmount += (perCycle * (rawResourceNames.Contains(resource.name) ? rawMultiplier : endMultiplier));
    }

    public void AddResouce(Resource resource, float amount)
    {
        ResourceCount resourceCount = GetResourceCount(resource);
        resourceCount.amount += amount;
    }

    public void RemoveperCycle(Resource resource, float perCycle)
    {
        Debug.Log(rawMultiplier + " : " + endMultiplier);
        ResourceCount resourceCount = GetResourceCount(resource);
        resourceCount.secondAmount -= (perCycle * (rawResourceNames.Contains(resource.name) ? rawMultiplier : endMultiplier));
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
