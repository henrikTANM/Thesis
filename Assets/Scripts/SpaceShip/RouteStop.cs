using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class RouteStop
{
    private Planet stop;
    [NonSerialized] public int stopIndex;

    private Route route;

    private int travelTime;

    private List<ResourceCount> shipState;
    private List<ResourceCount> planetState;

    public RouteStop(Planet stop, int stopIndex)
    {
        this.stop = stop;
        this.stopIndex = stopIndex;
        shipState = new();
        planetState = new();
        //CopyPlanetResourceCounts(stop);
    }

    //public void SetShipState(List<ResourceCount> shipState) { this.shipState = shipState; }
    //public void SetPlanetState(List<ResourceCount> planetState) { this.planetState = planetState; }
    public void SetRoute(Route route) { this.route = route; }
    public void SetTravelTime(int travelTime) { this.travelTime = travelTime; }

    public List<ResourceCount> GetShipState() { return shipState; }
    public List<ResourceCount> GetPlanetState() { return planetState; }

    public Route Route() { return route; }
    public int GetTravelTime() { return travelTime; }
    public Planet GetPlanet() { return stop; }
    public int GetIndex() { return stopIndex; }

    /*
    public void CopyPlanetResourceCounts(Planet planet)
    {
        foreach (ResourceCount resourceCount in planet.GetPlanetResourceHandler().GetResourceCounts())
        {
            planetState.Add(new ResourceCount(resourceCount.resource, resourceCount.secondAmount));
        }
    }
    */

    public void ModifyShipState(Resource resource, float amount)
    {
        foreach (ResourceCount resourceCount in shipState)
        {
            if (resourceCount.resource == resource)
            {
                resourceCount.amount = amount;
                return;
            }
        }
        shipState.Add(new ResourceCount(resource, amount));
    }

    public void ModifyPlanetState(Resource resource, float amount)
    {
        foreach (ResourceCount resourceCount in planetState)
        {
            if (resourceCount.resource == resource)
            {
                resourceCount.amount = amount;
                return;
            }
        }
        planetState.Add(new ResourceCount(resource, amount));
    }

    public int GetMinTravelTimeForRoutes()
    {
        return GetMinTravelTime(GetPlanet().GetOrbiter(), route.GetPreviousRouteStop(this).GetPlanet().GetOrbiter());
    }

    public int GetMinTravelTime(Orbiter start, Orbiter end)
    {
        Vector3 startCentrePos = start.GetCentrePos();
        Vector3 endCentrePos = end.GetCentrePos();
        Vector3 startPos = start.transform.position;

        float xzDistance = start.DistanceFromCentre() + end.DistanceFromCentre() + Vector2.Distance(new(startCentrePos.x, startCentrePos.z), new(endCentrePos.x, endCentrePos.z));
        float yDistance = Mathf.Abs(startCentrePos.y - endCentrePos.y);
        float maxDistance = Mathf.Sqrt(Mathf.Pow(xzDistance, 2) + Mathf.Pow(yDistance, 2));
        float maxAcceleration = route.GetShip().GetMaxAcceleration();

        for (int i = 1; i < 100; i++)
        {
            Vector3 endPos = end.GetPosIn(i);

            float halfDistance = maxDistance / 2.0f;
            float halfTravelTime = i / 2.0f;
            float acceleration = ((2 * halfDistance) / Mathf.Pow(halfTravelTime, 2));

            if (acceleration <= maxAcceleration) { return i; }
        }
        return 0;
    }
}
