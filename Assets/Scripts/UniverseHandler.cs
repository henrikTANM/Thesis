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

    [NonSerialized] public List<Star> stars = new();

    [NonSerialized] public int cycleCount = 0;
    [NonSerialized] public float cycleLength = 10.0f;
    [NonSerialized] public float timeCycleValue = 0.0f;
    [NonSerialized] public float timeValue = 0.0f;
    [NonSerialized] public bool timeRunning = true;

    private bool escapeMenuDisplayed = false;
    private bool shipsMenuDisplayed = false;
    [NonSerialized] public bool planetsMenuDisplayed = false;

    private Star activeStar;
    private Planet activePlanet;

    private void Awake()
    {
        universeGenerator = GetComponent<UniverseGenerator>();
        stars = universeGenerator.GenerateStars();

        InputEvents.OnTimeStateChange += HandleTimeChange;
        InputEvents.OnEscapeMenu += HandleEscapeMenu;
        InputEvents.OnShipsMenu += HandleShipsMenu;
        InputEvents.OnClusterView += SetLastActivePlanetInactive;
        InputEvents.OnClusterView += SetAllStarsInactive;
    }

    private void OnDestroy()
    {
        InputEvents.OnTimeStateChange -= HandleTimeChange;
        InputEvents.OnEscapeMenu -= HandleEscapeMenu;
        InputEvents.OnShipsMenu -= HandleShipsMenu;
        InputEvents.OnClusterView -= SetLastActivePlanetInactive;
        InputEvents.OnClusterView -= SetAllStarsInactive;
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
            ResourceEvents.CycleChange();
        }

        if (Input.GetKeyDown(KeyCode.Space) & !escapeMenuDisplayed) InputEvents.TimeStateChange();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (shipsMenuDisplayed)
            {
                InputEvents.ShipsMenu();
            }
            else
            {
                InputEvents.EscapeMenu();
            }
        }
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

    public void SetActiveStar(Star star)
    {
        activeStar = star;
    }

    public Star GetActiveStar()
    {
        return activeStar;
    }

    public void SetActivePlanet(Planet planet)
    {
        activePlanet = planet;
    }

    public Planet GetActivePlanet()
    {
        return activePlanet;
    }

    public bool AreMenusDisplayed()
    {
        return escapeMenuDisplayed | shipsMenuDisplayed;
    }

    public bool IsPlanetMenuDisplayed()
    {
        return planetsMenuDisplayed;
    }

    public void HandleEscapeMenu()
    {
        escapeMenuDisplayed = !escapeMenuDisplayed;
        timeRunning = !escapeMenuDisplayed;
    }

    public void HandleShipsMenu()
    {
        shipsMenuDisplayed = !shipsMenuDisplayed;
        timeRunning = !shipsMenuDisplayed;
    }

    public void HandlePlanetsMenu()
    {
        planetsMenuDisplayed = !planetsMenuDisplayed;
        timeRunning = !planetsMenuDisplayed;
    }

    public void HandleTimeChange()
    {
        timeRunning = !timeRunning;
    }
}
