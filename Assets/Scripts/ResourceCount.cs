using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCount
{
    public Resource resource;
    public int amount;
    public int perCycle;

    public ResourceCount(Resource resource, int amount, int PerCycle)
    {
        this.resource = resource;
        this.amount = amount;
        this.perCycle = PerCycle;
    }
}
