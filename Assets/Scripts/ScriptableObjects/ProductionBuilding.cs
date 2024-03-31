using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ProductionBuilding : ScriptableObject
{
    public string name;
    public Sprite buildingSprite;

    public ResourceAmount[] cost;
    public int upkeep;

    public ResourceAmount[] inputResources;
    public ResourceAmount outputResource;

    [Serializable]
    public class ResourceAmount
    {
        public Resource resource;
        public int amount;
    }
}
