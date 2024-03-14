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
    public TextMeshProUGUI CycleText;
    [SerializeField] private UIController uiController;

    [NonSerialized] public List<Star> stars = new();

    [NonSerialized] public int cycleCount = 0;
    [NonSerialized] public float cycleLength = 5.0f;
    [NonSerialized] public float timeCycleValue = 0.0f;
    [NonSerialized] public float timeValue = 0.0f;
    [NonSerialized] public bool timeRunning = true;

    [NonSerialized] public bool escapeMenuDisplayed = false;

    private Planet activePlanet;

    private void Awake()
    {
        universeGenerator = GetComponent<UniverseGenerator>();
        stars = universeGenerator.GenerateStars();

        InputEvents.OnTimeStateChange += HandleTimeChange;
        InputEvents.OnEscapeMenu += HandleEscapeMenu;
        InputEvents.OnClusterView += SetLastActivePlanetInactive;
        InputEvents.OnClusterView += SetAllStarsInactive;
    }

    private void OnDestroy()
    {
        InputEvents.OnTimeStateChange -= HandleTimeChange;
        InputEvents.OnEscapeMenu -= HandleEscapeMenu;
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
        CycleText.text = timeCycleValue.ToString();
        if (Input.GetKeyDown(KeyCode.Space) & !escapeMenuDisplayed) InputEvents.TimeStateChange();
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

    public Planet GetActivePlanet()
    {
        return activePlanet;
    }

    public bool UIMenuDisplayed()
    {
        return uiController.GetCurrentUI() != null;
    }

    public void HandleEscapeMenu()
    {
        escapeMenuDisplayed = !escapeMenuDisplayed;
        timeRunning = !escapeMenuDisplayed;
        uiController.ChangeTimeButtonIcon();
    }

    public void HandleTimeChange()
    {
        timeRunning = !timeRunning;
    }
}
