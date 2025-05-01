using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpaceShipHandler: MonoBehaviour
{
    public GameObject body;
    public ParticleSystem boosterParticleSystem;
    public SpaceShipMotionHandler spaceShipMotionHandler;
    public Resource fuelResource;

    [NonSerialized] public string name;

    [NonSerialized] public float scaleDownMultiplier = 0.05f;
    [NonSerialized] public float nativeScale;

    private TrailRenderer trailRenderer;
    private UniverseHandler universe;

    [NonSerialized] public Planet home;
    [NonSerialized] public Planet currentPlanet;

    [NonSerialized] public SpaceShip spaceShip;
    [NonSerialized] public Route route;
    [NonSerialized] public bool removeRoute = false;
    [NonSerialized] public SpaceShipState state = SpaceShipState.AT_HOME;

    [NonSerialized] public Vector3 posInOrbit;

    [NonSerialized] public ShipViewer shipViewer;

    public enum SpaceShipState
    {
        AT_HOME,
        AT_DESTINATION,
        MOVING_TO_HOME,
        MOVING_TO_DESTINATION
    }

    private void Awake()
    {
        //body.GetComponent<BoxCollider>().enabled = false;
        //body.GetComponent<MeshRenderer>().enabled = false;

        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();

        trailRenderer = body.GetComponent<TrailRenderer>();
        trailRenderer.textureScale = new(0.0f, 0.0f);

        EnableEffects(false);

        GameEvents.OnUniverseViewChange += ToggleTrailRenderer;
        GameEvents.OnAfterCycleChange += StartMoving;
    }

    private void OnDestroy()
    {
        GameEvents.OnUniverseViewChange -= ToggleTrailRenderer;
        GameEvents.OnAfterCycleChange -= StartMoving;
    }

    private void Update()
    {
        body.transform.localScale = UniverseHandler.instance.selectedPlanet == null ?
            new Vector3(nativeScale, nativeScale, nativeScale) :
            new Vector3(nativeScale * scaleDownMultiplier, nativeScale * scaleDownMultiplier, nativeScale * scaleDownMultiplier);
        if (!IsMoving()) { transform.position = currentPlanet.GetSpaceShipPosition() + posInOrbit; }
    }

    public void Create(SpaceShip spaceShip, Planet planet)
    {
        this.spaceShip = spaceShip;
        name = spaceShip.name;
        nativeScale = spaceShip.shipScale;
        home = planet;
        currentPlanet = planet;
    }

    public void EndMoving()
    {
        currentPlanet = currentPlanet.Equals(home) ? route.destination : home;
        currentPlanet.AddShipToOrbit(this);
        ChangeShipState();
    }

    private void StartMoving() 
    {
        if (removeRoute & state.Equals(SpaceShipState.AT_HOME)) RemoveRoute();
        if (HasRoute())
        {
            if (!IsMoving())
            {
                bool atHome = state.Equals(SpaceShipState.AT_HOME);
                bool enoughFuel = home.GetPlanetResourceHandler().GetResourceCounter(fuelResource).resourceAmount.amount > spaceShip.fuelConsumption.amount * route.travelTime * 2;

                if (route.active & atHome & !enoughFuel) route.SetActive(false, home, "Route has been stopped: not enough fuel for 1 round trip.");
                else if (route.active | (!route.active & !atHome))
                {
                    if (atHome) home.RemoveShipFromOrbit(this);
                    else route.destination.RemoveShipFromOrbit(this);

                    ChangeShipState();

                    spaceShipMotionHandler.StartMoving(
                        atHome ? home.GetOrbiter() : route.destination.GetOrbiter(),
                        atHome ? route.destination.GetOrbiter() : home.GetOrbiter(),
                        route.travelTime
                        );
                }
            }
        }
    }

    public bool HasRoute() { return route != null; }

    public void RemoveRoute()
    {
        removeRoute = !removeRoute;
        route.RemoveAllResourceFactors();
        route = null;
    }

    private void ChangeShipState()
    {
        if (state.Equals(SpaceShipState.AT_HOME)) state = SpaceShipState.MOVING_TO_DESTINATION;
        else if (state.Equals(SpaceShipState.MOVING_TO_DESTINATION)) state = SpaceShipState.AT_DESTINATION;
        else if (state.Equals(SpaceShipState.AT_DESTINATION)) state = SpaceShipState.MOVING_TO_HOME;
        else if (state.Equals(SpaceShipState.MOVING_TO_HOME)) state = SpaceShipState.AT_HOME;

        if (shipViewer != null) shipViewer.UpdateButtons();
        UIController.UpdateShipsList();

        EnableEffects(IsMoving());
    }

    public bool IsMoving()
    {
        return state.Equals(SpaceShipState.MOVING_TO_HOME) | state.Equals(SpaceShipState.MOVING_TO_DESTINATION);
    }

    public void EnableEffects(bool enable)
    {
        trailRenderer.enabled = enable;
        if (enable) boosterParticleSystem.Play();
        else boosterParticleSystem.Pause();
    }

    public void ToggleTrailRenderer()
    {
        if (IsMoving())
        {
            trailRenderer.textureScale = (trailRenderer.textureScale.x == 0.0f ? new(1.0f, 1.0f) : new(0.0f, 0.0f));
        }
    }
}
