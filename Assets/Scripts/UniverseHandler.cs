using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

public class UniverseHandler : MonoBehaviour
{
    private UniverseGenerator universeGenerator;
    [SerializeField] private CameraMovementHandler cameraMovementHandler;
    [SerializeField] private UIController uiController;

    [NonSerialized] public List<Star> stars = new();

    [NonSerialized] public int cycleCount = 0;
    [NonSerialized] public float cycleLength = 3.0f;
    [NonSerialized] public float timeCycleValue = 0.0f;
    [NonSerialized] public float timeValue = 0.0f;
    [NonSerialized] public bool timeRunning = false;

    [NonSerialized] public bool escapeMenuDisplayed = false;
    [NonSerialized] public bool routeMakerDisplayed = false;

    private Planet activePlanet;

    private RouteMaker activeRouteMaker;

    public List<Resource> allResources;

    public bool startWait = true;
    public Star startStar;

    private void Awake()
    {
        universeGenerator = GetComponent<UniverseGenerator>();
        stars = universeGenerator.GenerateStars();

        InputEvents.OnTimeStateChange += HandleTimeChange;
        InputEvents.OnEscapeMenu += HandleEscapeMenu;
        InputEvents.OnRouteMaker += HandleRouteMaker;
        InputEvents.OnClusterView += SetLastActivePlanetInactive;
        InputEvents.OnClusterView += SetAllStarsInactive;
    }

    private void OnDestroy()
    {
        InputEvents.OnTimeStateChange -= HandleTimeChange;
        InputEvents.OnEscapeMenu -= HandleEscapeMenu;
        InputEvents.OnRouteMaker += HandleRouteMaker;
        InputEvents.OnClusterView -= SetLastActivePlanetInactive;
        InputEvents.OnClusterView -= SetAllStarsInactive;
    }
    private void Start()
    {
        StartCoroutine(GameStartCameraMove());
    }

    private void Update()
    {
        if (timeRunning)
        {
            timeValue += Time.deltaTime;
            timeCycleValue += Time.deltaTime;
        }

        if (timeCycleValue >= cycleLength)
        {
            timeCycleValue = 0.0f;
            cycleCount++;
            GameEvents.CycleChange();
        }
        if (Input.GetKeyDown(KeyCode.Space) & !escapeMenuDisplayed & !routeMakerDisplayed) InputEvents.TimeStateChange();
    }

    public List<Planet> GetAllManagedPlanets()
    {
        List<Planet> managedPlanets = new();
        foreach (Star star in stars){ foreach (Planet planet in star.planets) { if (planet.GetManaged()) managedPlanets.Add(planet); } }
        return managedPlanets;
    }

    public void SetAllStarsInactive()
    {
        foreach (Star star in stars) star.SetSelected(false, true);
    }

    public void SetLastActivePlanetInactive()
    {
        if (activePlanet != null) activePlanet.SetSelected(false);
        activePlanet = null;
    }

    public void SetActivePlanet(Planet planet)
    {
        activePlanet = planet;
    }

    public void SetActiveRouteMaker(RouteMaker routeMaker)
    {
        activeRouteMaker = routeMaker;
    }

    public Planet GetActivePlanet()
    {
        return activePlanet;
    }

    public RouteMaker GetActiveRouteMaker()
    {
        return activeRouteMaker;
    }

    public bool UIDisplayed()
    {
        return uiController.UIDisplayed();
    }

    public void HandleEscapeMenu()
    {
        escapeMenuDisplayed = !escapeMenuDisplayed;
        timeRunning = !escapeMenuDisplayed;
        uiController.ChangeTimeButtonIcon();
        uiController.SetGameUIActive(!escapeMenuDisplayed);
    }

    public void HandleRouteMaker()
    {
        routeMakerDisplayed = !routeMakerDisplayed;
        timeRunning = !routeMakerDisplayed;
        uiController.ChangeTimeButtonIcon();
        uiController.SetGameUIActive(!routeMakerDisplayed);
    }

    public void HandleTimeChange()
    {
        timeRunning = !timeRunning;
    }

    IEnumerator GameStartCameraMove()
    {
        yield return new WaitForSeconds(1.0f);
        Star firstStar = stars.ElementAt(0);
        Planet firstPlanet = firstStar.planets.ElementAt(0);
        firstStar.SetSelected(true, false);
        foreach (Planet planet in firstStar.planets) { planet.SetReached(true); }
        firstPlanet.SetSelected(true);
        cameraMovementHandler.MoveToTarget(firstPlanet.body.transform, firstPlanet.body.transform.localScale.x * 3.0f, false);
        timeRunning = true;
    }
}
