using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BonusBuildingViewer : MonoBehaviour
{
    private UIController uiController;

    public void MakeBonusBuildingViewer(PlanetMenu planetMenu, Planet planet, SpecialBuilding specialBuilding)
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += uiController.RemoveLastFromUIStack;

        root.Q<Label>("name").text = specialBuilding.name;
        root.Q<Label>("info").text = specialBuilding.description;

        Button deconstructButton = root.Q<Button>("deconstructbutton");
        deconstructButton.clicked += () =>
        { 
            planet.SetSpecialBuilding(null);
            planetMenu.ChangeSpecialBuildingButtonImage();
            PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();
            if (specialBuilding.name == "Advanced machinery")
            {
                planetResourceHandler.RemoveRawMultipiler(1.5f);
                planetResourceHandler.RemoveEndMultipiler(0.5f);
            }
            if (specialBuilding.name == "Advanced logistics")
            {
                planetResourceHandler.RemoveRawMultipiler(0.5f);
                planetResourceHandler.RemoveEndMultipiler(1.5f);
            }
            uiController.RemoveLastFromUIStack(); 
        };
    }
}
