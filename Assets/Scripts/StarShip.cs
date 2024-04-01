using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarShip: MonoBehaviour
{
    private string name;
    private bool warpCapable;
    private int cargoCapacity;
    private int fuelCapacity;
    private int thrustPower;



    public StarShip(string name, int cargoCapacity, int fuelCapacity, int thrustPower)
    {
        this.name = name;
        this.cargoCapacity = cargoCapacity;
        this.fuelCapacity = fuelCapacity;
        this.thrustPower = thrustPower;
    }

    public string GetName() { return name; }
    public int GetCargoCapacity() {  return cargoCapacity; }
    public int GetFuelCapacity() {  return fuelCapacity; }
    public int GetThrustPower() {  return thrustPower; }

}
