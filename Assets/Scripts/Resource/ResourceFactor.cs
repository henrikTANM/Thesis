using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceFactor
{
    public ResourceAmount resourceAmount;
    public ResourceSource resourceSource;

    public ResourceFactor(ResourceAmount resourceAmount, ResourceSource resourceSource)
    {
        this.resourceAmount = resourceAmount;
        this.resourceSource = resourceSource;
    }
}
