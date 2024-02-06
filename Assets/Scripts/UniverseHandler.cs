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
    public int starCount = 20;
    public float spaceRadius = 100.0f;

    public GameObject starPrefab;
    public GameObject planetPrefab;
    public GameObject orbitLinePrefab;

    public List<StarValues> starValues;
    public List<PlanetValues> planetValues;

    [NonSerialized] public List<Star> stars = new();

    [NonSerialized] public float timeValue = 0.0f;
    [NonSerialized] public bool timeRunning = true;

    private bool escapeMenuDisplayed = false;
    private bool shipsMenuDisplayed = false;

    private Star activeStar;
    private Planet activePlanet;

    private void Awake()
    {
        GenerateStars();

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
        if (timeRunning) timeValue += (Time.deltaTime);

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

    List<Vector3> GenerateStarPositions()
    {
        List<Vector3> starPositions = new(){ new Vector3(0.0f, 0.0f, 0.0f) };

        while (starPositions.Count < starCount)
        {
            float x = UnityEngine.Random.Range(-spaceRadius, spaceRadius);
            float y = UnityEngine.Random.Range(-spaceRadius, spaceRadius);
            float z = UnityEngine.Random.Range(-spaceRadius, spaceRadius);
            Vector3 newStarPosition = new(x, y, z);

            bool generatePos = true;
            foreach (Vector3 starPosition in starPositions)
            {
                if (Vector3.Distance(newStarPosition, starPosition) < 20.0f) generatePos = false;
            }
            if (generatePos) starPositions.Add(newStarPosition);
        }

        return starPositions;
    }

    void GenerateStars()
    {
        List<Vector3> starPositions = GenerateStarPositions();

        foreach (Vector3 starPosition in starPositions)
        {
            GameObject newStar = Instantiate(starPrefab, starPosition, Quaternion.identity);
            StarValues newStarValues = starValues.ElementAt(UnityEngine.Random.Range(0, starValues.Count));

            float scale = newStarValues.scale;
            newStar.transform.localScale = new Vector3(scale, scale, scale);

            Star star = newStar.GetComponent<Star>();
            star.nativeScale = scale;
            stars.Add(star);
            GeneratePlanets(star);
            foreach (LineRenderer lineRenderer in star.GetComponentsInChildren<LineRenderer>()) lineRenderer.enabled = false;

            Material starMaterial = newStar.GetComponent<MeshRenderer>().material;
            starMaterial.SetColor("_Base_color", newStarValues.color);
            starMaterial.SetColor("_CellColor", newStarValues.cellColor);
            starMaterial.SetFloat("_CellDensity", newStarValues.cellDensity);
            starMaterial.SetFloat("_SolarFlare", newStarValues.solarFlare);
            starMaterial.SetFloat("_CellSpeed", newStarValues.cellSpeed);

            star.material = starMaterial;
        }
    }

    List<Vector3> GeneratePlanetPositions()
    {
        List<Vector3> planetPositions = new();
        int nrOfPlanets = UnityEngine.Random.Range(2, 6);

        while (planetPositions.Count < nrOfPlanets)
        {
            float z = UnityEngine.Random.Range(6.0f, 20.0f);
            Vector3 newPlanetPosition = new(0.0f, 0.0f, z);

            bool generatePos = true;
            foreach (Vector3 planetPosition in planetPositions)
            {
                if (Vector3.Distance(newPlanetPosition, planetPosition) < 2.0f) generatePos = false;
            }
            if (generatePos) planetPositions.Add(newPlanetPosition);
        }

        return planetPositions;
    }

    void GeneratePlanets(Star star)
    {
        List<Vector3> planetPositions = GeneratePlanetPositions();

        List<Planet> planets = new();

        for (int i = 0; i < planetPositions.Count; i++)
        {
            GameObject newPlanet = Instantiate(planetPrefab, star.transform.position, Quaternion.identity);
            PlanetValues newPlanetValues = planetValues.ElementAt(UnityEngine.Random.Range(0, planetValues.Count));

            newPlanet.transform.parent = star.transform;

            newPlanet.transform.position += planetPositions.ElementAt(i);
           
            float scale = UnityEngine.Random.Range(newPlanetValues.scale.x, newPlanetValues.scale.y);
            newPlanet.transform.localScale = new Vector3(scale, scale, scale);

            MeshRenderer meshRenderer = newPlanet.GetComponent<MeshRenderer>();
            Material material = meshRenderer.material;

            Texture2D randomPlanetTexture = newPlanetValues.planetTextures.ElementAt(UnityEngine.Random.Range(0, newPlanetValues.planetTextures.Count));
            Texture2D randomCloudsTexture = newPlanetValues.cloudTextures.Count > 0 ? newPlanetValues.cloudTextures.ElementAt(UnityEngine.Random.Range(0, newPlanetValues.cloudTextures.Count)) : null;
            material.SetTexture("_PlanetTexture", randomPlanetTexture);
            if (randomCloudsTexture != null)
            {
                material.SetTexture("_CloudsTexture", randomCloudsTexture);
            }

            Planet planet = newPlanet.GetComponent<Planet>();
            planet.orbitSpeed = UnityEngine.Random.Range(0.1f, 0.5f);
            planet.orbitAxis = Vector3.up;
            planet.material = material;
            planet.SetVisible(false);
            planet.parentStar = star;
            planets.Add(planet);

            GameObject newOrbitLine = Instantiate(orbitLinePrefab, star.transform.position, Quaternion.identity);
            newOrbitLine.GetComponent<LineRenderer>().useWorldSpace = false;
            newOrbitLine.transform.parent = star.transform;
            newOrbitLine.transform.position = star.transform.position;
            planet.DrawOrbit(200, Vector3.Distance(planet.transform.position, star.transform.position), newOrbitLine.GetComponent<LineRenderer>());
        }
        star.planets = planets;
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

    public void HandleTimeChange()
    {
        timeRunning = !timeRunning;
    }
}
