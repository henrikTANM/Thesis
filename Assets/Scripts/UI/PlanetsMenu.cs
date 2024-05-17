using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlanetsMenu : MonoBehaviour
{
    private UIController uiController;
    private UniverseHandler universe;

    public VisualTreeAsset planetRowTemplate;

    private ScrollView planetsList;

    public void MakePlanetsMenu()
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += uiController.RemoveLastFromUIStack;

        planetsList = root.Q<ScrollView>("planetslist");
        planetsList.mouseWheelScrollSize = 500.0f;

        UpdatePlanetsList();
    }

    public void UpdatePlanetsList()
    {
        planetsList.Clear();
        foreach (Planet planet in universe.GetAllManagedPlanets())
        {
            VisualElement planetRow = planetRowTemplate.Instantiate();

            planetRow.Q<Label>("name").text = planet.GetName();

            Button camera = planetRow.Q<Button>("camera");
            camera.clicked += () =>
            {
                planet.MoveToPlanet();
                planet.SetSelected(true);
                uiController.ClearUIStack();
            };

            planetsList.Add(planetRow);
        }
    }
}
