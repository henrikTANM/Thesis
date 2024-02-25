using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ProductionBuilding : ScriptableObject
{
    public string name;
    public Sprite buildingSprite;
    public Resource outputResource;
    public List<Resource> inputResources;
}
