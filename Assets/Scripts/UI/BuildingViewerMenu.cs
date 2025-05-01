using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingViewerMenu : MonoBehaviour
{
    private BuildingSlot buildingSlot;

    private VisualElement root;

    private bool mouseOnMenu = false;
    private Vector2 localMousePosition;

    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset resourceNeedTemplate;

    private void Update()
    {
        if (Input.GetMouseButton(0) & mouseOnMenu) MoveWindow(root, Input.mousePosition);
    }

    private void OnDestroy() 
    {
        buildingSlot.GetPlanet().SetBuildingViewerMenu(null);
        buildingSlot.GetPlanet().UpdateResourceDisplays();
        UIController.UpdateMoney();
    }

    public void MakeBuildingViewerMenu(BuildingSlot buildingSlot, ProductionBuildingHandler productionBuildingHandler)
    {
        this.buildingSlot = buildingSlot;

        root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<NavigationSubmitEvent>((evt) =>
        {
            evt.StopPropagation();
        }, TrickleDown.TrickleDown);
        root.RegisterCallback<MouseDownEvent>(evt =>
        {
            mouseOnMenu = true;
            localMousePosition = evt.localMousePosition;
        }, TrickleDown.TrickleDown);
        root.RegisterCallback<MouseUpEvent>(evt => mouseOnMenu = false, TrickleDown.TrickleDown);

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
            UIController.RemoveLastFromUIStack();
        };

        Button activateButton = root.Q<Button>("activatebutton");
        activateButton.text = productionBuildingHandler.active ? "DEACTIVATE" : "ACTIVATE";
        activateButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
            productionBuildingHandler.active = !productionBuildingHandler.active;
            activateButton.text = productionBuildingHandler.active ? "DEACTIVATE" : "ACTIVATE";
            buildingSlot.GetPlanet().GetPlanetResourceHandler().UpdateResourcePerCycles();
            buildingSlot.GetPlanet().UpdateResourceDisplays();
        };

        Button deconstructButton = root.Q<Button>("deletebutton");
        deconstructButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
            buildingSlot.DeleteBuilding(productionBuildingHandler.costResources);
            UIController.RemoveLastFromUIStack();
        };

        root.Q<VisualElement>("moneyicon").style.unityBackgroundImageTintColor =
            new StyleColor(PlayerInventory.instance.moneyResource.spriteColor);

        UpdateSelectedInfo(root.Q<VisualElement>("info"), productionBuildingHandler);

        buildingSlot.GetPlanet().UpdateResourceDisplays();
    }

    private void UpdateSelectedInfo(VisualElement root, ProductionBuildingHandler productionBuildingHandler)
    {
        root.Q<Label>("name").text = productionBuildingHandler.name;
        root.Q<Label>("upkeep").text = productionBuildingHandler.upkeep.resourceAmount.amount + "/Cycle";

        VisualElement producesImage = root.Q<VisualElement>("outputimage");
        producesImage.style.backgroundImage = new StyleBackground(productionBuildingHandler.outputFactor.resourceAmount.resource.resourceSprite);
        producesImage.style.unityBackgroundImageTintColor = new StyleColor(productionBuildingHandler.outputFactor.resourceAmount.resource.spriteColor);
        //producesImage.style.flexGrow = 1;

        root.Q<Label>("outputvalue").text = productionBuildingHandler.outputFactor.resourceAmount.amount + "/Cycle";

        //buildingOption.style.flexGrow = 1;

        VisualElement refundList = root.Q<VisualElement>("refund");
        refundList.Clear();
        foreach (ResourceAmount buildingCost in productionBuildingHandler.costResources)
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
        foreach (ResourceFactor resourceNeed in productionBuildingHandler.inputFactors)
        {
            VisualElement buildingNeedTemplate = resourceNeedTemplate.Instantiate();
            buildingNeedTemplate.Q<Label>("need").text = resourceNeed.resourceAmount.amount + "/Cycle";
            VisualElement buildingNeedTemplateImage = buildingNeedTemplate.Q<VisualElement>("needimage");
            buildingNeedTemplateImage.style.backgroundImage = new StyleBackground(resourceNeed.resourceAmount.resource.resourceSprite);
            buildingNeedTemplateImage.style.unityBackgroundImageTintColor = new StyleColor(resourceNeed.resourceAmount.resource.spriteColor);
            inputList.Add(buildingNeedTemplate);
        }
    }

    public void UpdateResourcePanel(List<VisualElement> resourceContainers)
    {
        VisualElement resourcesPanel = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("resourcespanel");
        resourcesPanel.Clear();
        foreach (VisualElement resourceContainer in resourceContainers) resourcesPanel.Add(resourceContainer);
    }

    private void MoveWindow(VisualElement root, Vector3 mousePos)
    {
        Vector2 pos = new(mousePos.x, Screen.height - mousePos.y);
        pos = RuntimePanelUtils.ScreenToPanel(root.panel, pos);
        pos = new(pos.x - localMousePosition.x, pos.y - localMousePosition.y);

        root.style.top = pos.y;
        root.style.left = pos.x;
    }
}
