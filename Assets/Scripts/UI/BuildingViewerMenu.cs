using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingViewerMenu : MonoBehaviour
{
    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset resourceNeedTemplate;

    private PlayerInventory playerInventory;

    private void Awake()
    {
        playerInventory = GameObject.Find("PlayerInventory").GetComponent<PlayerInventory>();
    }

    public void MakeBuildingViewerMenu(BuildingSlot buildingSlot, ProductionBuildingHandler productionBuildingHandler)
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += buildingSlot.CloseMenu;

        Button activateButton = root.Q<Button>("activatebutton");
        bool active = productionBuildingHandler.IsActive();
        activateButton.text = active ? "ACTIVE" : "DEACTIVATED";
        activateButton.clicked += () =>
        {
            if (productionBuildingHandler.IsManuallyDeActivated() & productionBuildingHandler.CanBeActived())
            {
                productionBuildingHandler.SetManuallyDeActivated(false);
                activateButton.text = "ACTIVE";
            }
            else if (active) {
                productionBuildingHandler.SetManuallyDeActivated(true);
                activateButton.text = "DEACTIVATED";

            }
        };

        Button deleteButton = root.Q<Button>("deletebutton");
        deleteButton.clicked += () => buildingSlot.DeleteBuilding(productionBuildingHandler.GetCostAmounts());

        root.Q<VisualElement>("moneyicon").style.unityBackgroundImageTintColor =
            new StyleColor(playerInventory.GetMoneyResource().spriteColor);

        UpdateSelectedInfo(root.Q<VisualElement>("info"), productionBuildingHandler);
    }

    private void UpdateSelectedInfo(VisualElement root, ProductionBuildingHandler productionBuildingHandler)
    {
        root.Q<Label>("name").text = productionBuildingHandler.GetName();
        root.Q<Label>("upkeep").text = productionBuildingHandler.GetUpkeep() + "/Cycle";

        VisualElement producesImage = root.Q<VisualElement>("outputimage");
        producesImage.style.backgroundImage = new StyleBackground(productionBuildingHandler.GetOutputResource().resource.resourceSprite);
        producesImage.style.unityBackgroundImageTintColor = new StyleColor(productionBuildingHandler.GetOutputResource().resource.spriteColor);
        //producesImage.style.flexGrow = 1;

        root.Q<Label>("outputvalue").text = productionBuildingHandler.GetOutputResource().amount + "/Cycle";

        //buildingOption.style.flexGrow = 1;

        VisualElement refundList = root.Q<VisualElement>("refund");
        refundList.Clear();
        foreach (ProductionBuilding.ResourceAmount buildingCost in productionBuildingHandler.GetCostAmounts())
        {
            VisualElement buildingCostTemplate = resourceNeedTemplate.Instantiate();
            buildingCostTemplate.Q<Label>("need").text = (buildingCost.amount / 2).ToString();
            VisualElement buildingCostTemplateImage = buildingCostTemplate.Q<VisualElement>("needimage");
            buildingCostTemplateImage.style.backgroundImage = new StyleBackground(buildingCost.resource.resourceSprite);
            buildingCostTemplateImage.style.unityBackgroundImageTintColor = new StyleColor(buildingCost.resource.spriteColor);
            refundList.Add(buildingCostTemplate);
        }

        VisualElement inputList = root.Q<VisualElement>("inputlist");
        inputList.Clear();
        foreach (ProductionBuilding.ResourceAmount resourceNeed in productionBuildingHandler.GetInputResources())
        {
            VisualElement buildingNeedTemplate = resourceNeedTemplate.Instantiate();
            buildingNeedTemplate.Q<Label>("need").text = resourceNeed.amount + "/Cycle";
            VisualElement buildingNeedTemplateImage = buildingNeedTemplate.Q<VisualElement>("needimage");
            buildingNeedTemplateImage.style.backgroundImage = new StyleBackground(resourceNeed.resource.resourceSprite);
            buildingNeedTemplateImage.style.unityBackgroundImageTintColor = new StyleColor(resourceNeed.resource.spriteColor);
            inputList.Add(buildingNeedTemplate);
        }
    }

    public void UpdateResourcePanel(Planet planet, UIDocument buildingViewerMenuUI)
    {
        VisualElement resourcesPanel = buildingViewerMenuUI.rootVisualElement.Q<VisualElement>("resourcespanel");
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
            resourceContainer.Q<Label>("resourcecount").text = resourceCount.amount.ToString() + "+" + resourceCount.perCycle.ToString();
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
