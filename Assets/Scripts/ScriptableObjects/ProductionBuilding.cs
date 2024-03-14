using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ProductionBuilding : ScriptableObject
{
    public string name;
    public Sprite buildingSprite;

    public ResourceCount[] cost;
    public int upkeep;

    public ResourceCount[] inputResources;
    public ResourceCount outputResource;

    [Serializable]
    public class ResourceCount
    {
        public Resource resource;
        public int amount;
    }
}
