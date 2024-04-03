using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShip: MonoBehaviour
{
    private string name;
    private Route route;
    private bool routePaused = false;
    private bool warpCapable;
    private int cargoCapacity;
    private float maxAcceleration;

    private List<ResourceCount> cargo;

    public void CreateShip(string name, int cargoCapacity, float maxAcceleration)
    {
        this.name = name;
        this.cargoCapacity = cargoCapacity;
        this.maxAcceleration = maxAcceleration;
    }

    public string GetName() { return name; }
    public bool IsRoutePaused() { return routePaused; }
    public int GetCargoCapacity() {  return cargoCapacity; }
    public float GetMaxAcceleration() {  return maxAcceleration; }

    public List<ResourceCount> GetCargo() { return cargo; }

    public void setRoute(Route route) { this.route = route; }
    public void setRoutePaused(bool paused) { routePaused = paused; }

    public bool HasRoute() { return route != null; }
    public void RemoveRoute() { route = null; }



    public void Sell()
    {

    }

}
