using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class Deposit : ScriptableObject
{
    public string name;
    public List<Sprite> possibleDepositSprites;
    public List<ProductionBuilding> possibleProductionBuildings;
    public int buildingCap;
}
