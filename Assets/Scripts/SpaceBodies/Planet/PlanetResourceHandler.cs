using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ProductionBuilding;

public class PlanetResourceHandler
{
    public Planet planet;
    private List<ResourceCounter> resourceCounters = new();
    private List<ResourceFactor> resourceFactors = new();
    float rawMultiplier = 1.0f;
    float endMultiplier = 1.0f;

    public PlanetResourceHandler(List<Resource> resources, Planet planet)
    {
        this.planet = planet;
        foreach (Resource resource in resources) { resourceCounters.Add(new ResourceCounter(new ResourceAmount(resource, 0), 0)); }
    }

    public void UpdateResourceAmounts()
    {
        foreach (ResourceCounter resourceCounter in resourceCounters)
        {
            if (resourceCounter.resourceAmount.amount + resourceCounter.change < 0)
            {
                foreach (ResourceFactor resourceFactor in FindActiveResourceFactors(resourceCounter.resourceAmount.resource))
                {
                    if (resourceFactor.resourceAmount.amount < 0)
                    {
                        Debug.Log("Set resourceFactor inactive" + resourceFactor.resourceSource.name);
                        resourceFactor.resourceSource.SetActive(false, planet, null);
                    }
                }
            }
            else resourceCounter.resourceAmount.amount += resourceCounter.change;
        }
    }

    public void UpdateResourcePerCycles() 
    { 
        foreach (ResourceCounter resourceCounter in resourceCounters)
        {
            resourceCounter.change = 0;
            foreach (ResourceFactor resourceFactor in FindActiveResourceFactors(resourceCounter.resourceAmount.resource)) 
            {
                resourceCounter.change += resourceFactor.resourceAmount.amount;
            }
        }
    }

    public List<ResourceCounter> GetResourceCounters() { return resourceCounters; }
    public ResourceCounter GetResourceCounter(Resource resource) { return FindResourceCounter(resource); }
    public ResourceAmount GetResourceAmount(Resource resource) { return FindResourceCounter(resource).resourceAmount; }

    public void AddResourceFactor(ResourceFactor resourceFactor) { resourceFactors.Add(resourceFactor); }
    public void RemoveResourceFactor(ResourceFactor resourceFactor) { resourceFactors.Remove(resourceFactor); }

    public void ChangeResourceAmount(ResourceAmount resourceAmount) 
    { 
        FindResourceCounter(resourceAmount.resource).resourceAmount.amount += resourceAmount.amount; 
    }
    public bool CanChangeResourceAmount(ResourceAmount resourceAmount) 
    {
        return FindResourceCounter(resourceAmount.resource).resourceAmount.amount >= resourceAmount.amount;
    }

    public void AddRawMultiplier(float rawMultiplier) { this.rawMultiplier *= rawMultiplier; }
    public void RemoveRawMultipiler(float rawMultiplier) { this.rawMultiplier /= rawMultiplier; }
    public void AddEndMultiplier(float endMultiplier) { this.endMultiplier *= endMultiplier; }
    public void RemoveEndMultipiler(float endMultiplier) { this.endMultiplier /= endMultiplier; }

    public List<ResourceFactor> FindActiveResourceFactors(Resource resource)
    {
        List<ResourceFactor> activeFactors = resourceFactors.FindAll((ResourceFactor rf) => { return rf.resourceAmount.resource == resource & rf.resourceSource.active; });
        List<ResourceFactor> modifiedFactors = new();
        foreach (ResourceFactor rf in activeFactors)
        {
            int amount = (int)(rf.resourceAmount.amount * (rf.resourceSource.type.Equals(ResourceSource.Type.PRODUCTION) ? 
                (rf.resourceAmount.resource.type.Equals(Resource.Type.RAW) ? rawMultiplier : endMultiplier) : 1.0f));
            modifiedFactors.Add(new(new(rf.resourceAmount.resource, amount), rf.resourceSource));
        }
        return modifiedFactors;
    }

    private ResourceCounter FindResourceCounter(Resource resource)
    {
        return resourceCounters.Find((ResourceCounter ra) => { return ra.resourceAmount.resource == resource; });
    }
}
