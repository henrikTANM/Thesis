using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SpaceShip : ScriptableObject
{
    public string name;
    public int cargoCapacity;
    public bool isInterstellar;
    public ResourceAmount[] cost;
    public ResourceAmount fuelConsumption;
    public float shipScale;
}
