using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ProductionBuilding : Building
{
    public int upkeep;

    public ResourceAmount[] inputResources;
    public ResourceAmount outputResource;
}
