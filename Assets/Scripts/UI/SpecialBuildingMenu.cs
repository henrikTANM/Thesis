using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SpecialBuildingMenu : MonoBehaviour
{
    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset resourceNeedTemplate;

    public SpecialBuilding advancedMachinery;
    public SpecialBuilding shipYard;
    public SpecialBuilding advancedLogistics;

    private SpecialBuilding selectedSpecialBuilding;

    private UIController uiController;

    private Button buildButton;

    /*
    [SerializeField] private Color failColor;
    [SerializeField] private Color originalColor;
    */

    public void MakeSpecialBuildingChooserMenu(PlanetMenu planetMenu, Planet planet)
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += () =>
        {
            planet.SetSpecialBuildingChooserMenu(null);
            uiController.RemoveLastFromUIStack();
        };

        buildButton = root.Q<Button>("buildbutton");
        buildButton.clicked += () => { if (selectedSpecialBuilding != null) BuildSelected(planetMenu, planet); };

        selectedSpecialBuilding = shipYard;
        UpdateResourcePanel(planet);
        UpdateBuildButton(planet);

        Button rawBonusButton = root.Q<Button>("rawbonus");
        rawBonusButton.clicked += () => 
        { 
            selectedSpecialBuilding = advancedMachinery;
            buildButton.text = "Build Advanced Machinery";
            UpdateBuildButton(planet);
        };
        VisualElement rawBonusCost = rawBonusButton.Q<VisualElement>("cost");
        foreach (ResourceAmount costAmount in advancedMachinery.cost)
        {
            VisualElement cost = resourceNeedTemplate.Instantiate();
            cost.Q<Label>("need").text = costAmount.amount.ToString();
            VisualElement costImage = cost.Q<VisualElement>("needimage");
            costImage.style.backgroundImage = new StyleBackground(costAmount.resource.resourceSprite);
            costImage.style.unityBackgroundImageTintColor = new StyleColor(costAmount.resource.spriteColor);
            rawBonusCost.Add(cost);
        }
        rawBonusButton.SetEnabled(true);

        Button shipYardButton = root.Q<Button>("shipyard");
        shipYardButton.clicked += () => 
        { 
            selectedSpecialBuilding = shipYard;
            buildButton.text = "Build ShipYard";
            UpdateBuildButton(planet);
        };
        VisualElement shipYardCost = shipYardButton.Q<VisualElement>("cost");
        foreach (ResourceAmount costAmount in shipYard.cost)
        {
            VisualElement cost = resourceNeedTemplate.Instantiate();
            cost.Q<Label>("need").text = costAmount.amount.ToString();
            VisualElement costImage = cost.Q<VisualElement>("needimage");
            costImage.style.backgroundImage = new StyleBackground(costAmount.resource.resourceSprite);
            costImage.style.unityBackgroundImageTintColor = new StyleColor(costAmount.resource.spriteColor);
            shipYardCost.Add(cost);
        }

        Button advancedLogisticsButton = root.Q<Button>("endbonus");
        advancedLogisticsButton.clicked += () => 
        { 
            selectedSpecialBuilding = advancedLogistics;
            buildButton.text = "Build Advanced Logistics";
            UpdateBuildButton(planet);
        };
        VisualElement advancedLogisticsCost = advancedLogisticsButton.Q<VisualElement>("cost");
        foreach (ResourceAmount costAmount in advancedLogistics.cost)
        {
            VisualElement cost = resourceNeedTemplate.Instantiate();
            cost.Q<Label>("need").text = costAmount.amount.ToString();
            VisualElement costImage = cost.Q<VisualElement>("needimage");
            costImage.style.backgroundImage = new StyleBackground(costAmount.resource.resourceSprite);
            costImage.style.unityBackgroundImageTintColor = new StyleColor(costAmount.resource.spriteColor);
            advancedLogisticsCost.Add(cost);
        }
    }

    private void BuildSelected(PlanetMenu planetMenu, Planet planet)
    {
        if (planet.CanBuild(selectedSpecialBuilding.cost)) 
        {
            planet.SetSpecialBuilding(selectedSpecialBuilding);
            planetMenu.ChangeSpecialBuildingButtonImage(selectedSpecialBuilding.image);
            PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();
            if (selectedSpecialBuilding.name == "Advanced machinery") 
            {
                planetResourceHandler.AddRawMultiplier(1.5f);
                planetResourceHandler.AddEndMultiplier(0.5f);
            }
            if (selectedSpecialBuilding.name == "Advanced logistics")
            {
                planetResourceHandler.AddRawMultiplier(0.5f);
                planetResourceHandler.AddEndMultiplier(1.5f);
            }
            uiController.RemoveLastFromUIStack();
        }
    }

    private void UpdateBuildButton(Planet planet)
    {
        if (!planet.CanBuild(selectedSpecialBuilding.cost))
        {
            //buildButton.style.backgroundColor = new StyleColor(failColor);
            buildButton.SetEnabled(false);
        }
        else
        {
            buildButton.SetEnabled(true);
            //buildButton.style.backgroundColor = new StyleColor(originalColor);
        }
    }

    public void UpdateResourcePanel(Planet planet)
    {
        UpdateBuildButton(planet);

        VisualElement resourcesPanel = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("resourcespanel");
        List<ResourceCount> resourceCounts = planet.GetPlanetResourceHandler().GetResourceCounts();

        foreach (ResourceCount resourceCount in resourceCounts)
        {
            VisualElement resourceContainer = GetResourceContainer(resourceCount.resource, resourcesPanel);
            if (resourceContainer == null)
            {
                resourceContainer = resourceTemplate.Instantiate();
                resourceContainer.name = resourceCount.resource.name;
                VisualElement resourceImage = resourceContainer.Q<VisualElement>("resourceimage");
                resourceImage.style.backgroundImage =
                    new StyleBackground(resourceCount.resource.resourceSprite);
                resourceImage.style.unityBackgroundImageTintColor =
                    new StyleColor(resourceCount.resource.spriteColor);
                resourceContainer.style.alignSelf = Align.Center;
                resourcesPanel.Add(resourceContainer);
            }
            resourceContainer.Q<Label>("resourcecount").text = resourceCount.amount.ToString() + "+" + resourceCount.secondAmount.ToString();
        }
    }

    private VisualElement GetResourceContainer(Resource resource, VisualElement resourcesPanel)
    {
        foreach (VisualElement resourceContainer in resourcesPanel.Children())
        {
            if (resourceContainer.name == resource.name) return resourceContainer;
        }
        return null;
    }
}
