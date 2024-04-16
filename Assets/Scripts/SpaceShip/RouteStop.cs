using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RouteStop
{
    private Planet stop;
    private int stopIndex;

    private Route route;

    private int travelTime;

    private List<ResourceCount> shipState;
    private List<ResourceCount> planetState;

    public RouteStop(Planet stop, int stopIndex)
    {
        this.stop = stop;
        this.stopIndex = stopIndex;
        shipState = new();
        planetState = new(stop.GetPlanetResourceHandler().GetResourceCounts());
    }

    public void SetShipState(List<ResourceCount> shipState) { this.shipState = shipState; }
    public void SetPlanetState(List<ResourceCount> planetState) { this.planetState = planetState; }
    public void SetRoute(Route route) { this.route = route; }

    public void SetTravelTime(int travelTime) { this.travelTime = travelTime; }

    public List<ResourceCount> GetShipState() { return shipState; }
    public List<ResourceCount> GetPlanetState() { return planetState; }

    public void AddToShipState(ResourceCount resourceCount) { shipState.Add(resourceCount); }
    public Route Route() { return route; }
    public int GetTravelTime() { return travelTime; }
    public Planet GetPlanet() { return stop; }
    public int GetIndex() { return stopIndex; }

    public int GetMinTravelTimeInCycles(Orbiter start, Orbiter end, float maxAcceleration)
    {
        Vector3 startCentrePos = start.GetCentrePos();
        Vector3 endCentrePos = end.GetCentrePos();
        Vector3 startPos = start.transform.position;

        float xzDistance = start.DistanceFromCentre() + end.DistanceFromCentre() + Vector2.Distance(new(startCentrePos.x, startCentrePos.z), new(endCentrePos.x, endCentrePos.z));
        float yDistance = Mathf.Abs(startCentrePos.y - endCentrePos.y);
        float maxDistance = Mathf.Sqrt(Mathf.Pow(xzDistance, 2) + Mathf.Pow(yDistance, 2));

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
