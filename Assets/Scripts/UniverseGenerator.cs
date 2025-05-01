using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UniverseGenerator : MonoBehaviour
{
    public int starCount = 20;
    public float spaceRadius = 4000.0f;
    public float blackHoleScale;

    public GameObject starPrefab;
    public GameObject blackHolePrefab;
    public GameObject planetPrefab;
    public GameObject orbitLinePrefab;

    public List<StarValues> starValues;
    public List<PlanetValues> planetValues;

    public List<Sprite> planetBackgrounds;
    public List<Sprite> gasPlanetBackgrounds;

    [SerializeField] private StartSettings startSettings;
    [SerializeField] private Resource fuelResource;

    public bool toggleDiscovered;
    public bool toggleReached;

    public List<Star> GenerateStars()
    {
        List<Star> stars = new();
        List<Vector3> starPositions = GenerateStarPositions();

        for (int i = 0; i < starPositions.Count; i++)
        {
            GameObject newStar = Instantiate(starPrefab, starPositions.ElementAt(i), Quaternion.identity);
            StarValues newStarValues = starValues.ElementAt(UnityEngine.Random.Range(0, starValues.Count));
            Star star = newStar.GetComponent<Star>();

            // set star body scale
            star.nativeScale = newStarValues.scale;
            star.ScaleToSize(star.nativeScale, false);

            // set star values
            star.SetName(newStarValues.starNames.ElementAt(UnityEngine.Random.Range(0, newStarValues.starNames.Count)));
            star.nameTagCanvas.enabled = true;
            star.SetDiscovered(toggleDiscovered | i == 0, false);

            // generate planets and their orbit lines
            GeneratePlanets(star, i == 0 ? startSettings.startSystemPlanets : null, false);
            foreach (Planet planet in star.planets) planet.SetVisible(false);
            foreach (LineRenderer lineRenderer in star.GetComponentsInChildren<LineRenderer>()) lineRenderer.enabled = false;

            // add material and set material values
            Material starMaterial = star.body.GetComponent<MeshRenderer>().material;
            starMaterial.SetColor("_Base_color", newStarValues.color);
            starMaterial.SetColor("_CellColor", newStarValues.cellColor);
            starMaterial.SetFloat("_CellDensity", newStarValues.cellDensity);
            starMaterial.SetFloat("_SolarFlare", newStarValues.solarFlare);
            starMaterial.SetFloat("_CellSpeed", newStarValues.cellSpeed);
            star.material = starMaterial;

            stars.Add(star);
        }

        // adds black hole
        stars.Add(GenerateBlackHole());

        return stars;
    }

    private BlackHole GenerateBlackHole()
    {
        Vector3 blackHolePosition = UnityEngine.Random.onUnitSphere.normalized * (spaceRadius * 2.0f);
        GameObject newBlackHole = Instantiate(blackHolePrefab, blackHolePosition, Quaternion.identity);
        BlackHole blackHole = newBlackHole.GetComponent<BlackHole>();

        // set blackhole body scale
        blackHole.nativeScale = blackHoleScale;
        blackHole.ScaleToSize(blackHole.nativeScale, false);

        // set black hole values
        blackHole.SetName("OB-1");
        blackHole.SetDiscovered(toggleDiscovered | false, false);

        // generate planets and their orbit lines
        List<StartSettings.PlanetSettings> blackHolePlanets = new() { startSettings.blackHolePlanet };
        GeneratePlanets(blackHole, blackHolePlanets, true);
        foreach (Planet planet in blackHole.planets) planet.SetVisible(false);
        foreach (LineRenderer lineRenderer in blackHole.GetComponentsInChildren<LineRenderer>()) lineRenderer.enabled = false;

        return blackHole;
    }

    private void GeneratePlanets(Star star, List<StartSettings.PlanetSettings> startPlanetValues, bool forBlackHole)
    {
        List<Planet> planets = new();
        List<Vector3> planetPositions = GeneratePlanetPositions(startPlanetValues);

        for (int i = 0; i < planetPositions.Count; i++)
        {
            GameObject newPlanet = Instantiate(planetPrefab, star.transform.position, Quaternion.identity);
            PlanetValues newPlanetValues = startPlanetValues == null ? planetValues.ElementAt(UnityEngine.Random.Range(0, planetValues.Count)) : startPlanetValues.ElementAt(i).planetValues;
            Planet planet = newPlanet.GetComponent<Planet>();

            // set planet start position adn parent star
            planet.parentStar = star;
            planet.transform.parent = star.transform;
            planet.transform.position += planetPositions.ElementAt(i);

            // set planet scale
            planet.nativeScale = UnityEngine.Random.Range(newPlanetValues.scale.x, newPlanetValues.scale.y);
            planet.ScaleToSize(planet.nativeScale, false);

            // set planet values
            planet.SetName(star.name + " 0" + (i + 1));
            planet.planetValues = newPlanetValues;
            planet.reached = toggleReached | (startPlanetValues != null & !forBlackHole);
            planet.managed = i == 0 & startPlanetValues != null & !forBlackHole;
            planet.specialBuildingSlotBackground = newPlanetValues.planetType.Equals(PlanetValues.PlanetType.Gas) ?
                gasPlanetBackgrounds.ElementAt(UnityEngine.Random.Range(0, gasPlanetBackgrounds.Count)) :
                planetBackgrounds.ElementAt(UnityEngine.Random.Range(0, planetBackgrounds.Count));

            // generate deposits
            if (startPlanetValues == null) planet.GenerateDeposits(newPlanetValues.possibleDeposits, newPlanetValues.depositCap);
            else
            {
                planet.GenerateStartDeposits(startPlanetValues.ElementAt(i).depositList);
                if (i == 0) 
                { 
                    PlanetResourceHandler resourceHandler = planet.GetPlanetResourceHandler();
                    foreach (ResourceAmount resourceAmount in startSettings.startPlanetResourceAmounts) resourceHandler.ChangeResourceAmount(resourceAmount);
                }                                                                                                                                                                                                   
            }

            // set material and textures
            MeshRenderer meshRenderer = planet.body.GetComponent<MeshRenderer>();
            Material material = meshRenderer.material;
            Texture2D randomPlanetTexture = newPlanetValues.planetTextures.ElementAt(UnityEngine.Random.Range(0, newPlanetValues.planetTextures.Count));
            material.SetTexture("_PlanetTexture", randomPlanetTexture);
            Texture2D randomCloudsTexture = newPlanetValues.cloudTextures.Count > 0 ? newPlanetValues.cloudTextures.ElementAt(UnityEngine.Random.Range(0, newPlanetValues.cloudTextures.Count)) : null;
            if (randomCloudsTexture != null) material.SetTexture("_CloudsTexture", randomCloudsTexture);
            planet.material = material;

            // add planet orbit line
            GameObject newOrbitLine = Instantiate(orbitLinePrefab, star.transform.position, Quaternion.identity);
            newOrbitLine.GetComponent<LineRenderer>().useWorldSpace = false;
            newOrbitLine.transform.parent = star.transform;
            newOrbitLine.transform.position = star.transform.position;
            planet.DrawOrbit(200, Vector3.Distance(planet.transform.position, star.transform.position), newOrbitLine);

            // configure planet orbiter
            Orbiter orbiter = newPlanet.GetComponent<Orbiter>();
            orbiter.SetCentre(star.transform);
            orbiter.SetOrbitSpeed(UnityEngine.Random.Range(0.1f, 0.5f));

            // set invisible
            planet.SetVisible(false);

            planets.Add(planet);
        }

        star.planets = planets;
    }






    private List<Vector3> GenerateStarPositions()
    {
        List<Vector3> starPositions = new() { new Vector3(0.0f, 0.0f, 0.0f) };

        while (starPositions.Count < starCount)
        {
            float x = UnityEngine.Random.Range(-spaceRadius, spaceRadius);
            float y = UnityEngine.Random.Range(-spaceRadius, spaceRadius);
            float z = UnityEngine.Random.Range(-spaceRadius, spaceRadius);
            Vector3 newStarPosition = new(x, y, z);

            bool generatePos = true;
            foreach (Vector3 starPosition in starPositions)
            {
                if (Vector3.Distance(newStarPosition, starPosition) < 800.0f) generatePos = false;
            }
            if (generatePos) starPositions.Add(newStarPosition);
        }

        return starPositions;
    }

    private List<Vector3> GeneratePlanetPositions(List<StartSettings.PlanetSettings> planetValues)
    {
        List<Vector3> planetPositions = new();
        int nrOfPlanets = planetValues == null ? UnityEngine.Random.Range(3, 6) : planetValues.Count;

        while (planetPositions.Count < nrOfPlanets)
        {
            Vector2 randomPos = UnityEngine.Random.insideUnitCircle;
            Vector3 newPlanetPosition = new(randomPos.x * 75.0f, 0.0f, randomPos.y * 75.0f);

            bool generatePos = true;
            foreach (Vector3 planetPosition in planetPositions)
            {
                bool closeToPlanet = Mathf.Abs(Vector3.Distance(Vector3.zero, newPlanetPosition) - Vector3.Distance(Vector3.zero, planetPosition)) < 7.5f;
                bool closeToStar = Vector3.Distance(Vector3.zero, newPlanetPosition) < 15.0f;
                if (closeToPlanet | closeToStar)
                {
                    generatePos = false;
                    break;
                }
            }
            if (generatePos) planetPositions.Add(newPlanetPosition);
        }

        return planetPositions;
    }
}
