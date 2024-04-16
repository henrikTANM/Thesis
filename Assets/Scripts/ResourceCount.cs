using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCount
{
    public Resource resource;
    public float amount;
    public float perCycle;

    public ResourceCount(Resource resource, float amount, float perCycle)
    {
        this.resource = resource;
        this.amount = amount;
        this.perCycle = perCycle;
    }

    public ResourceCount(Resource resource, float amount)
    {
        this.resource = resource;
        this.amount = amount;
    }

    public bool Equals(ResourceCount count)
    {
        return resource == count.resource;
    }
}
