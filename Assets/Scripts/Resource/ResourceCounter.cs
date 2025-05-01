using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCounter
{
    public ResourceAmount resourceAmount;
    public int change;

    public ResourceCounter(ResourceAmount resourceAmount, int change)
    {
        this.resourceAmount = resourceAmount;
        this.change = change;
    }
}
