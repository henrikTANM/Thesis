using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShip: MonoBehaviour
{
    private string name;

    private int cargoCapacity;
    private float maxAcceleration;

    private Route route;

    private bool routePaused = false;

    private Planet currentPlanet;
    private Planet destination;

    public MotionSimulator motionSimulator;

    private bool travelling = false;

    private Type shipType;
    private enum Type
    {
        WARP,
        NON_WARP
    }

    private void Update()
    {
        Moving();
    }

    void Moving()
    {
        if (HasRoute() & !travelling & !routePaused)
        {
            if (currentPlanet != route.GetRouteStop(0).GetPlanet())
            {
                RouteStop firstRouteStop = route.GetRouteStop(0);

                motionSimulator.StartMoving(
                    currentPlanet.GetOrbiter(),
                    firstRouteStop.GetPlanet().GetOrbiter(),
                    firstRouteStop.GetMinTravelTime(currentPlanet.GetOrbiter(), firstRouteStop.GetPlanet().GetOrbiter())
                    );
                currentPlanet = firstRouteStop.GetPlanet();
                travelling = true;
            }
            else
            {
                Planet startPlanet = route.GetRouteStop(route.GetCurrentRouteIndex()).GetPlanet();
                route.ProgressRoute();
                Planet endPlanet = route.GetRouteStop(route.GetCurrentRouteIndex()).GetPlanet();
                currentPlanet = endPlanet;

                motionSimulator.StartMoving(
                    startPlanet.GetOrbiter(),
                    endPlanet.GetOrbiter(),
                    route.GetRouteStop(route.GetCurrentRouteIndex()).GetTravelTime()
                    );
                travelling = true;
            }
        } 
        else if (!travelling | routePaused)
        {
            transform.position = currentPlanet.transform.position;
        }
    }


    public void CreateShip(string name, int cargoCapacity, float maxAcceleration, Planet currentPlanet)
    {
        this.name = name;
        this.cargoCapacity = cargoCapacity;
        this.maxAcceleration = maxAcceleration;
        this.currentPlanet = currentPlanet;
        transform.position = currentPlanet.transform.position;
    }

    public string GetName() { return name; }
    public int GetCargoCapacity() {  return cargoCapacity; }
    public float GetMaxAcceleration() {  return maxAcceleration; }

    public void SetTravelling(bool travelling) { this.travelling = travelling; }
    public void SetRoute(Route route) { this.route = route; }

    public bool IsRoutePaused() { return routePaused; }
    public bool HasRoute() { return route != null; }

    public void Sell()
    {
        if (HasRoute()) RemoveRoute();
        //TODO add money back
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
