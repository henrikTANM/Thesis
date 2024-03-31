using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UniverseGenerator : MonoBehaviour
{
    public int starCount = 20;
    public float spaceRadius = 100.0f;

    public GameObject starPrefab;
    public GameObject planetPrefab;
    public GameObject orbitLinePrefab;

    public List<StarValues> starValues;
    public List<PlanetValues> planetValues;

    public List<Sprite> settlementSprites;

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
                if (Vector3.Distance(newStarPosition, starPosition) < 20.0f) generatePos = false;
            }
            if (generatePos) starPositions.Add(newStarPosition);
        }

        return starPositions;
    }

    public List<Star> GenerateStars()
    {
        List<Star> stars = new();
        List<Vector3> starPositions = GenerateStarPositions();

        foreach (Vector3 starPosition in starPositions)
        {
            GameObject newStar = Instantiate(starPrefab, starPosition, Quaternion.identity);
            StarValues newStarValues = starValues.ElementAt(UnityEngine.Random.Range(0, starValues.Count));

            float scale = newStarValues.scale;
            newStar.transform.localScale = new Vector3(scale, scale, scale);

            Star star = newStar.GetComponent<Star>();
            star.nativeScale = scale;

            star.SetName(newStarValues.starNames.ElementAt(UnityEngine.Random.Range(0, newStarValues.starNames.Count)));

            GeneratePlanets(star);
            foreach (Planet planet in star.planets) planet.SetVisible(false);
            foreach (LineRenderer lineRenderer in star.GetComponentsInChildren<LineRenderer>()) lineRenderer.enabled = false;

            Material starMaterial = star.starBody.GetComponent<MeshRenderer>().material;
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

    private List<Vector3> GeneratePlanetPositions()
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

    private void GeneratePlanets(Star star)
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
            planet.nativeScale = scale;
            planet.material = material;
            planet.type = newPlanetValues.planetType;

            planet.SetVisible(false);
            planet.parentStar = star;

            planet.SetName(star.GetName() + " 0" + (i + 1));

            planet.type = newPlanetValues.planetType;

            planet.SetSettlementSprite(settlementSprites.ElementAt(UnityEngine.Random.Range(0, settlementSprites.Count())));
            planet.GenerateDeposits(newPlanetValues.possibleDeposits, UnityEngine.Random.Range(1, newPlanetValues.depositCap));

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
