using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCount
{
    public Resource resource;
    public float amount;
    public float secondAmount;

    public ResourceCount(Resource resource, float amount, float secondAmount)
    {
        this.resource = resource;
        this.amount = amount;
        this.secondAmount = secondAmount;
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
