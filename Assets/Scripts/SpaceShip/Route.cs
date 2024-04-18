using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Route
{
    private int currentRouteIndex = 0;
    private SpaceShip ship;
    private List<RouteStop> routeStops = new();
    public Route(SpaceShip ship)
    {
        this.ship = ship;
    }

    public void Create()
    {
        ship.SetRoute(this);
        AddRoutePersCycles();
    }

    public int GetCurrentRouteIndex() { return currentRouteIndex; }

    public SpaceShip GetShip()
    {
        return ship;
    }

    public RouteStop GetPreviousRouteStop(RouteStop routeStop)
    {
        int index = routeStop.GetIndex();
        return routeStops.ElementAt(index == 0 ? routeStops.Count - 1 : index - 1);
    }

    public void ProgressRoute()
    {
        currentRouteIndex = currentRouteIndex == routeStops.Count - 1 ? 0 : currentRouteIndex + 1;
    }

    public RouteStop GetRouteStop(int index)
    {
        return routeStops.ElementAt(index);
    }



    public int GetTotalTravelTime()
    {
        int sum = 0;
        foreach (RouteStop routeStop in routeStops) { sum += routeStop.GetTravelTime(); }
        return sum;
    }

    public void Add(RouteStop routeStop)
    {
        routeStop.SetRoute(this);
        routeStops.Add(routeStop);

        if (routeStops.Count > 1) { routeStops.ForEach(rS => rS.SetTravelTime(rS.GetMinTravelTimeForRoutes())); }
    }

    public void Remove(RouteStop routeStop)
    {
        routeStops.Remove(routeStop);

        if (routeStops.Count > 1) { routeStops.ForEach(rS => rS.SetTravelTime(rS.GetMinTravelTimeForRoutes())); }
    }

    public bool ContainsPlanet(Planet planet)
    {
        foreach (RouteStop routeStop in routeStops)
        {
            if (routeStop.GetPlanet() == planet) return true;
        }
        return false;
    }

    public void AddRoutePersCycles()
    {
        foreach (RouteStop routeStop in routeStops)
        {
            PlanetResourceHandler planetResourceHandler = routeStop.GetPlanet().GetPlanetResourceHandler();

            foreach (ResourceCount resourceCount in routeStop.GetPlanetState())
            {
                planetResourceHandler.AddPerCycle(resourceCount.resource, resourceCount.amount);
            }
        }
    }

    public void RemoveRoutePersCycles()
    {
        foreach (RouteStop routeStop in routeStops)
        {
            PlanetResourceHandler planetResourceHandler = routeStop.GetPlanet().GetPlanetResourceHandler();

            foreach (ResourceCount resourceCount in routeStop.GetPlanetState())
            {
                planetResourceHandler.RemoveperCycle(resourceCount.resource, resourceCount.amount);
            }
        }
    }
}
