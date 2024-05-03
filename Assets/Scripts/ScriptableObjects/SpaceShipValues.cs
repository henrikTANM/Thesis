using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SpaceShipValues : ScriptableObject
{
    public string name;
    public float fuelCapacity;
    public int cargoCapacity;
    public float accelerationRate;
    public ResourceAmount[] cost;
}
