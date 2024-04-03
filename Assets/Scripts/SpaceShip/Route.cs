using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Route
{
    private List<RouteStop> routeStops = new();
    public Route()
    {

    }

    public void Create()
    {

    }

    public RouteStop GetPreviousRouteStop(RouteStop routeStop)
    {
        int index = routeStop.GetIndex();
        return routeStops.ElementAt(index == 0 ? routeStops.Count - 1 : index);
    }

    public int GetTotalTravelTime()
    {
        int sum = 0;
        foreach(RouteStop routeStop in routeStops) { sum += routeStop.GetTravelTime(); }
        return sum;
    }

    public void Add(RouteStop routeStop)
    {
        routeStop.SetRoute(this);
        routeStops.Add(routeStop);
    }

    public void Remove(RouteStop routeStop)
    {
        routeStops.Remove(routeStop);
    }
}
