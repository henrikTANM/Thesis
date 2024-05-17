using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpaceShip: MonoBehaviour
{
    public GameObject body;
    public ParticleSystem booster;
    private TrailRenderer trailRenderer;

    private UniverseHandler universe;
    private string name;
    private float fuelCapacity;
    private float fuelOnShip;
    private int cargoCapacity;
    private float maxAcceleration;

    private ResourceAmount[] cost;

    private Route route;

    private bool routePaused = false;

    private Planet currentPlanet;

    public MotionSimulator motionSimulator;

    public Resource fuel;

    private bool travelling = false;

    private Vector3 nativeScale;

    private Vector3 posInOrbit;

    private ShipViewer shipViewer;

    private void Awake()
    {
        body.GetComponent<BoxCollider>().enabled = false;
        body.GetComponent<MeshRenderer>().enabled = false;
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
        trailRenderer = body.GetComponent<TrailRenderer>();
        trailRenderer.enabled = false;
        trailRenderer.textureScale = new(0.0f, 0.0f);

        GameEvents.OnUniverseViewChange += ToggleTrailRenderer;
    }

    private void OnDestroy()
    {
        GameEvents.OnUniverseViewChange -= ToggleTrailRenderer;
    }

    private void Update()
    {
        Moving();
        body.transform.localScale = (universe.GetActivePlanet() == null ? nativeScale : nativeScale / 10.0f);
        //booster.transform.localScale = (universe.GetActivePlanet() == null ? new(0.4f, 0.4f, 0.4f) : new(0.04f, 0.04f, 0.04f));
        //booster.transform.localPosition = new(0.0f, -0.83f, -9.21f);
        if (!travelling | (routePaused & !travelling)) { transform.position = currentPlanet.shipPos + posInOrbit; }
    }

    void Moving()
    {

        if (HasRoute() & !travelling & !routePaused)
        {
            TryToLoadFuel();
            if (!CanTravel())
            {
                ChangeRoutePaused();
                return;
            } 
            else
            {
                fuelOnShip -= route.GetCurrentRouteStop().GetTravelDisctance() / 2;
            }
            if (!route.ContainsPlanet(currentPlanet) | currentPlanet != route.GetRouteStop(route.GetCurrentRouteIndex()).GetPlanet())
            {
                RouteStop firstRouteStop = route.GetRouteStop(0);
                currentPlanet.RemoveShipFromOrbit(this);
                motionSimulator.StartMoving(
                    currentPlanet.GetOrbiter(),
                    firstRouteStop.GetPlanet().GetOrbiter(),
                    firstRouteStop.GetMinTravelTime(currentPlanet.GetOrbiter(), firstRouteStop.GetPlanet().GetOrbiter())
                    );
                currentPlanet = firstRouteStop.GetPlanet();
                currentPlanet.AddShipToOrbit(this);
                travelling = true;
                GameEvents.ShipStateChange();
            }
            else
            {
                Planet startPlanet = route.GetRouteStop(route.GetCurrentRouteIndex()).GetPlanet();
                route.ProgressRoute();
                Planet endPlanet = route.GetRouteStop(route.GetCurrentRouteIndex()).GetPlanet();
                currentPlanet.RemoveShipFromOrbit(this);
                motionSimulator.StartMoving(
                    startPlanet.GetOrbiter(),
                    endPlanet.GetOrbiter(),
                    route.GetRouteStop(route.GetCurrentRouteIndex()).GetTravelTime()
                    );
                currentPlanet = endPlanet;
                currentPlanet.AddShipToOrbit(this);
                travelling = true;
                GameEvents.ShipStateChange();
            }
        }
    }

    public void SetPosInOrbit(Vector3 pos)
    {
        posInOrbit = pos;
    }

    public void TryToLoadFuel()
    {
        PlanetResourceHandler currentPlanetResourceHandler = currentPlanet.GetPlanetResourceHandler();
        if (fuelOnShip < fuelCapacity)
        {
            float loadAmount = fuelCapacity - fuelOnShip;
            float fuelOnPlanet = currentPlanetResourceHandler.GetResourceCount(fuel).amount;
            if (loadAmount >= fuelOnPlanet & fuelOnPlanet > 0.0f)
            {
                fuelOnShip += fuelOnPlanet;
                currentPlanetResourceHandler.RemoveResouce(fuel, fuelOnPlanet);

            }
            else if (fuelOnPlanet > 0.0f)
            {
                fuelOnShip += loadAmount;
                currentPlanetResourceHandler.RemoveResouce(fuel, loadAmount);
            }
        }
    }

    public bool CanTravel() 
    {
        float fuelNeeded = route.GetCurrentRouteStop().GetTravelDisctance() / 2;
        return fuelOnShip >= fuelNeeded;
    }


    public void CreateShip(string name, float fuelCapacity, int cargoCapacity, float maxAcceleration, ResourceAmount[] cost,  Planet currentPlanet, float scale)
    {
        this.name = name;
        this.fuelCapacity = fuelCapacity;
        this.cargoCapacity = cargoCapacity;
        this.maxAcceleration = maxAcceleration;
        this.cost = cost;
        this.currentPlanet = currentPlanet;
        nativeScale = new(scale, scale, scale);
        body.GetComponent<BoxCollider>().enabled = true;
        body.GetComponent<MeshRenderer>().enabled = true;
    }

    public string GetName() { return name; }
    public int GetCargoCapacity() {  return cargoCapacity; }
    public float GetMaxAcceleration() {  return maxAcceleration; }

    public float GetFuelCapacity() { return fuelCapacity; }

    public float GetFuelOnShip() {  return fuelOnShip; }

    public Planet GetCurrentPlanet() { return currentPlanet; }

    public Route GetRoute() { return route; }

    public ResourceAmount[] GetCost()
    {
        return cost;
    }

    public void SetShipViewer(ShipViewer shipViewer)
    {
        this.shipViewer = shipViewer;
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
        if (HasRoute()) { if (!routePaused) route.RemoveRoutePersCycles(); }
        route = null;
    }

    public void ChangeRoutePaused()
    {
        routePaused = !routePaused;
        if (routePaused) route.RemoveRoutePersCycles();
        else route.AddRoutePersCycles();
    }

    public void EnableEffects(bool enable)
    {
        trailRenderer.enabled = enable;
        if (motionSimulator.IsMoving()) { booster.Play(); }
        else { booster.Pause(); }
    }

    public void ToggleTrailRenderer()
    {
        if (travelling)
        {
            trailRenderer.textureScale = trailRenderer.textureScale.x == 0.0f ? new(1.0f, 1.0f) : new(0.0f, 0.0f);
        }
    }
}
