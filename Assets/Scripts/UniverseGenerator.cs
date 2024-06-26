using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UniverseGenerator : MonoBehaviour
{
    public int starCount = 20;
    public float spaceRadius = 1000.0f;

    public GameObject starPrefab;
    public GameObject planetPrefab;
    public GameObject orbitLinePrefab;

    public List<StarValues> starValues;
    public List<PlanetValues> planetValues;

    public List<Sprite> settlementSprites;

    [SerializeField] private StartSettings startSettings;
    [SerializeField] private Resource fuelResource;

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
                if (Vector3.Distance(newStarPosition, starPosition) < 200.0f) generatePos = false;
            }
            if (generatePos) starPositions.Add(newStarPosition);
        }

        return starPositions;
    }

    public List<Star> GenerateStars()
    {
        List<Star> stars = new();
        List<Vector3> starPositions = GenerateStarPositions();

        for (int i = 0; i < starPositions.Count; i++)
        {
            GameObject newStar = Instantiate(starPrefab, starPositions.ElementAt(i), Quaternion.identity);
            StarValues newStarValues = starValues.ElementAt(UnityEngine.Random.Range(0, starValues.Count));

            float scale = newStarValues.scale;

            Star star = newStar.GetComponent<Star>();
            star.nativeScale = scale * 10.0f;
            star.ScaleToSize(star.nativeScale, false);

            star.SetName(newStarValues.starNames.ElementAt(UnityEngine.Random.Range(0, newStarValues.starNames.Count)));

            GeneratePlanets(star, i == 0 ? startSettings.startSystemPlanets : null);
            foreach (Planet planet in star.planets) planet.SetVisible(false);
            foreach (LineRenderer lineRenderer in star.GetComponentsInChildren<LineRenderer>()) lineRenderer.enabled = false;

            Material starMaterial = star.body.GetComponent<MeshRenderer>().material;
            starMaterial.SetColor("_Base_color", newStarValues.color);
            starMaterial.SetColor("_CellColor", newStarValues.cellColor);
            starMaterial.SetFloat("_CellDensity", newStarValues.cellDensity);
            starMaterial.SetFloat("_SolarFlare", newStarValues.solarFlare);
            starMaterial.SetFloat("_CellSpeed", newStarValues.cellSpeed);

            star.material = starMaterial;

            stars.Add(star);
        }
        return stars;
    }

    private List<Vector3> GeneratePlanetPositions(List<StartSettings.PlanetSettings> planetValues)
    {
        List<Vector3> planetPositions = new();
        int nrOfPlanets = planetValues == null ? UnityEngine.Random.Range(3, 6) : planetValues.Count;

        while (planetPositions.Count < nrOfPlanets)
        {
            Vector2 randomPos = UnityEngine.Random.insideUnitCircle;
            Vector3 newPlanetPosition = new(randomPos.x * 20.0f, 0.0f, randomPos.y * 20.0f);

            bool generatePos = true;
            foreach (Vector3 planetPosition in planetPositions)
            {
                bool closeToPlanet = Mathf.Abs(Vector3.Distance(Vector3.zero, newPlanetPosition) - Vector3.Distance(Vector3.zero, planetPosition)) < 2.0f;
                bool closeToStar = Vector3.Distance(Vector3.zero, newPlanetPosition) < 5.0f;
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

    private void GeneratePlanets(Star star, List<StartSettings.PlanetSettings> startPlanetValues)
    {
        List<Vector3> planetPositions = GeneratePlanetPositions(startPlanetValues);

        List<Planet> planets = new();

        for (int i = 0; i < planetPositions.Count; i++)
        {
            GameObject newPlanet = Instantiate(planetPrefab, star.transform.position, Quaternion.identity);
            PlanetValues newPlanetValues = startPlanetValues == null ? planetValues.ElementAt(UnityEngine.Random.Range(0, planetValues.Count)) : startPlanetValues.ElementAt(i).planetValues;

            newPlanet.transform.parent = star.transform;

            newPlanet.transform.position += planetPositions.ElementAt(i);

            float scale = UnityEngine.Random.Range(newPlanetValues.scale.x, newPlanetValues.scale.y);

            

            Orbiter orbiter = newPlanet.GetComponent<Orbiter>();
            orbiter.SetCentre(star.transform);
            orbiter.SetOrbitSpeed(UnityEngine.Random.Range(0.1f, 0.5f));

            Planet planet = newPlanet.GetComponent<Planet>();

            MeshRenderer meshRenderer = planet.body.GetComponent<MeshRenderer>();
            Material material = meshRenderer.material;

            Texture2D randomPlanetTexture = newPlanetValues.planetTextures.ElementAt(UnityEngine.Random.Range(0, newPlanetValues.planetTextures.Count));
            Texture2D randomCloudsTexture = newPlanetValues.cloudTextures.Count > 0 ? newPlanetValues.cloudTextures.ElementAt(UnityEngine.Random.Range(0, newPlanetValues.cloudTextures.Count)) : null;
            material.SetTexture("_PlanetTexture", randomPlanetTexture);

            planet.nativeScale = scale * 2.0f;
            planet.ScaleToSize(planet.nativeScale, false);
            planet.material = material;
            planet.type = newPlanetValues.planetType;

            planet.SetVisible(false);
            planet.parentStar = star;

            planet.SetName(star.GetName() + " 0" + (i + 1));

            planet.type = newPlanetValues.planetType;

            planet.SetSettlementSprite(settlementSprites.ElementAt(UnityEngine.Random.Range(0, settlementSprites.Count())));
            if (startPlanetValues == null)
            {
                planet.GenerateDeposits(newPlanetValues.possibleDeposits, newPlanetValues.depositCap);
            } else
            {
                planet.GenerateStartDeposits(startPlanetValues.ElementAt(i).depositList);
                if (i == 0) { planet.GetPlanetResourceHandler().AddResouce(fuelResource, 1000.0f); }
            }

            if (randomCloudsTexture != null)
            {
                material.SetTexture("_CloudsTexture", randomCloudsTexture);
            }

            planets.Add(planet);

            GameObject newOrbitLine = Instantiate(orbitLinePrefab, star.transform.position, Quaternion.identity);
            newOrbitLine.GetComponent<LineRenderer>().useWorldSpace = false;
            newOrbitLine.transform.parent = star.transform;
            newOrbitLine.transform.position = star.transform.position;
            planet.DrawOrbit(200, Vector3.Distance(planet.transform.position, star.transform.position), newOrbitLine);
        }
        star.planets = planets;
    }
}
