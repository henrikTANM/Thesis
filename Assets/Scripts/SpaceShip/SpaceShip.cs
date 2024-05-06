using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShip: MonoBehaviour
{
    private string name;
    private float fuelCapacity;
    private int cargoCapacity;
    private float maxAcceleration;

    private ResourceAmount[] cost;

    private Route route;

    private bool routePaused = false;

    private Planet currentPlanet;

    public MotionSimulator motionSimulator;

    public Resource fuel;

    private bool travelling = false;

    private void Update()
    {
        Moving();
    }

    void Moving()
    {
        if (HasRoute() & !travelling & !routePaused)
        {
            if (!CanTravel())
            {
                ChangeRoutePaused();
                return;
            }
            if (!route.ContainsPlanet(currentPlanet))
            {
                RouteStop firstRouteStop = route.GetRouteStop(0);

                motionSimulator.StartMoving(
                    currentPlanet.GetOrbiter(),
                    firstRouteStop.GetPlanet().GetOrbiter(),
                    firstRouteStop.GetMinTravelTime(currentPlanet.GetOrbiter(), firstRouteStop.GetPlanet().GetOrbiter())
                    );
                currentPlanet = firstRouteStop.GetPlanet();
                travelling = true;
                GameEvents.ShipStateChange();
            }
            else
            {
                Planet startPlanet = route.GetRouteStop(route.GetCurrentRouteIndex()).GetPlanet();
                route.ProgressRoute();
                Planet endPlanet = route.GetRouteStop(route.GetCurrentRouteIndex()).GetPlanet();

                motionSimulator.StartMoving(
                    startPlanet.GetOrbiter(),
                    endPlanet.GetOrbiter(),
                    route.GetRouteStop(route.GetCurrentRouteIndex()).GetTravelTime()
                    );
                currentPlanet = endPlanet;
                travelling = true;
                GameEvents.ShipStateChange();
            }
        }
        else if (!travelling | routePaused)
        {
            transform.position = currentPlanet.transform.position;
        }
    }

    public bool CanTravel() 
    {
        float fuelOnPlanet = currentPlanet.GetPlanetResourceHandler().GetResourceCount(fuel).amount;
        float fuelNeeded = route.GetCurrentRouteStop().GetTravelDisctance();
        return fuelOnPlanet >= fuelNeeded;
    }


    public void CreateShip(string name, float fuelCapacity, int cargoCapacity, float maxAcceleration, ResourceAmount[] cost,  Planet currentPlanet)
    {
        this.name = name;
        this.fuelCapacity = fuelCapacity;
        this.cargoCapacity = cargoCapacity;
        this.maxAcceleration = maxAcceleration;
        this.cost = cost;
        this.currentPlanet = currentPlanet;
        transform.position = currentPlanet.transform.position;
    }

    public string GetName() { return name; }
    public int GetCargoCapacity() {  return cargoCapacity; }
    public float GetMaxAcceleration() {  return maxAcceleration; }

    public float GetFuelCapacity() { return fuelCapacity; }

    public Planet GetCurrentPlanet() { return currentPlanet; }

    public Route GetRoute() { return route; }

    public ResourceAmount[] GetCost()
    {
        return cost;
    }

    public void SetTravelling(bool travelling) 
    { 
        if (!travelling & !currentPlanet.GetReached()) { currentPlanet.SetReached(true); }
        this.travelling = travelling; 
    }
    public void SetRoute(Route route) { this.route = route; }

    public bool IsRoutePaused() { return routePaused; }
    public bool HasRoute() { return route != null; }
    public bool IsTravelling() { return travelling; }

    public void Sell()
    {
        Destroy(this);
    }

    public void RemoveRoute()
    {
        if (!routePaused) route.RemoveRoutePersCycles();
        route = null;
    }

    public void ChangeRoutePaused()
    {
        routePaused = !routePaused;
        if (routePaused) route.RemoveRoutePersCycles();
        else route.AddRoutePersCycles();
    }
}
