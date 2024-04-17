using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingChooserMenu : MonoBehaviour
{
    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset buildingOptionButton;
    public VisualTreeAsset resourceNeedTemplate;

    public ProductionBuilding selectedProductiomBuilding;
    private PlayerInventory playerInventory;

    private BuildingSlot buildingSlot;
    private Button buildButton;

    [SerializeField] private Color failColor;
    [SerializeField] private Color originalColor;

    private void Awake()
    {
        playerInventory = GameObject.Find("PlayerInventory").GetComponent<PlayerInventory>();
    }

    public void MakeBuildingChooserMenu(BuildingSlot buildingSlot, List<ProductionBuilding> possibleProductionBuildings)
    {
        this.buildingSlot = buildingSlot;

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += buildingSlot.CloseMenu;

        buildButton = root.Q<Button>("buildbutton");
        buildButton.clicked += BuildSelected;

        VisualElement buildingButtonContainer = root.Q<VisualElement>("buildingbutton_container");

        root.Q<VisualElement>("moneyicon").style.unityBackgroundImageTintColor = 
            new StyleColor(playerInventory.GetMoneyResource().spriteColor);

        selectedProductiomBuilding = possibleProductionBuildings.ElementAt(0);
        UpdateSelectedInfo(root);

        foreach (ProductionBuilding productionBuilding in possibleProductionBuildings)
        {
            VisualElement buildingOption = buildingOptionButton.Instantiate();
            Button optionButton = buildingOption.Q<Button>("optionbutton");
            optionButton.clicked += () => 
            {
                selectedProductiomBuilding = productionBuilding;
                UpdateSelectedInfo(root); 
            };
            optionButton.style.backgroundImage = new StyleBackground(productionBuilding.buildingSprite);
            optionButton.style.unityBackgroundImageTintColor = new StyleColor(productionBuilding.outputResource.resource.spriteColor);

            buildingOption.style.flexGrow = 1;
            buildingButtonContainer.Add(buildingOption);
        }
    }

    private void UpdateSelectedInfo(VisualElement root)
    {
        if (!buildingSlot.CanBuildBuilding(selectedProductiomBuilding))
        {
            buildButton.style.backgroundColor = new StyleColor(failColor);
            buildButton.SetEnabled(false);
        }
        else
        {
            buildButton.SetEnabled(true);
            buildButton.style.backgroundColor = new StyleColor(originalColor);
        }

        root.Q<Label>("name").text = selectedProductiomBuilding.name;
        root.Q<Label>("upkeep").text = selectedProductiomBuilding.upkeep + "/Cycle";

        VisualElement producesImage = root.Q<VisualElement>("outputimage");
        producesImage.style.backgroundImage = new StyleBackground(selectedProductiomBuilding.outputResource.resource.resourceSprite);
        producesImage.style.unityBackgroundImageTintColor = new StyleColor(selectedProductiomBuilding.outputResource.resource.spriteColor);
        //producesImage.style.flexGrow = 1;

        root.Q<Label>("outputvalue").text = selectedProductiomBuilding.outputResource.amount + "/Cycle";

        //buildingOption.style.flexGrow = 1;

        VisualElement costList = root.Q<VisualElement>("costlist");
        costList.Clear();
        foreach (ProductionBuilding.ResourceAmount buildingCost in selectedProductiomBuilding.cost)
        {
            VisualElement buildingCostTemplate = resourceNeedTemplate.Instantiate();
            buildingCostTemplate.Q<Label>("need").text = buildingCost.amount.ToString();
            VisualElement buildingCostTemplateImage = buildingCostTemplate.Q<VisualElement>("needimage");
            buildingCostTemplateImage.style.backgroundImage = new StyleBackground(buildingCost.resource.resourceSprite);
            buildingCostTemplateImage.style.unityBackgroundImageTintColor = new StyleColor(buildingCost.resource.spriteColor);
            costList.Add(buildingCostTemplate);
        }

        VisualElement inputList = root.Q<VisualElement>("inputlist");
        inputList.Clear();
        foreach (ProductionBuilding.ResourceAmount resourceNeed in selectedProductiomBuilding.inputResources)
        {
            VisualElement buildingNeedTemplate = resourceNeedTemplate.Instantiate();
            buildingNeedTemplate.Q<Label>("need").text = resourceNeed.amount + "/Cycle";
            VisualElement buildingNeedTemplateImage = buildingNeedTemplate.Q<VisualElement>("needimage");
            buildingNeedTemplateImage.style.backgroundImage = new StyleBackground(resourceNeed.resource.resourceSprite);
            buildingNeedTemplateImage.style.unityBackgroundImageTintColor = new StyleColor(resourceNeed.resource.spriteColor);
            inputList.Add(buildingNeedTemplate);
        }
    }

    private void BuildSelected()
    {
        if (buildingSlot.CanBuildBuilding(selectedProductiomBuilding))
        {
            buildingSlot.BuildBuilding(selectedProductiomBuilding);
        }
    }

    public void UpdateResourcePanel(Planet planet, UIDocument buildingChooserMenuUI)
    {
        if (!buildingSlot.CanBuildBuilding(selectedProductiomBuilding))
        {
            buildButton.style.backgroundColor = new StyleColor(failColor);
            buildButton.SetEnabled(false);
        } 
        else
        {
            buildButton.SetEnabled(true);
            buildButton.style.backgroundColor = new StyleColor(originalColor);
        }


        VisualElement resourcesPanel = buildingChooserMenuUI.rootVisualElement.Q<VisualElement>("resourcespanel");
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
