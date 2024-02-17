using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionBuilding : ScriptableObject
{
    public string name;
    public Sprite buildingSprite;
    public Resource outPutResource;
    public List<Resource> inputResources;
}
