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

    private void Awake()
    {
        
    }

    private void OnDestroy()
    {
        
    }

    private void Update()
    {
        StartMoving();
    }

    void StartMoving()
    {
        if (HasRoute() & !travelling & !routePaused)
        {
            if (route.ContainsPlanet(currentPlanet))
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
            else
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
        //transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
    }

    public string GetName() { return name; }
    public int GetCargoCapacity() {  return cargoCapacity; }
    public float GetMaxAcceleration() {  return maxAcceleration; }

    public void SetTravelling(bool travelling) { this.travelling = travelling; }
    public void setRoute(Route route) { this.route = route; }
    public void setRoutePaused(bool paused) { routePaused = paused; }

    public bool IsRoutePaused() { return routePaused; }
    public bool HasRoute() { return route != null; }
    public void RemoveRoute() { route = null; }

    public void Sell()
    {

    }

    private void PauseRoute()
    {

    }

    private void Travel(Planet startPlanet, Planet endPlanet)
    {
        
    }
}
