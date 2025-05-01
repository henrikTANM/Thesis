using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingChooserMenu : MonoBehaviour
{
    private VisualElement root;

    private bool mouseOnMenu = false;
    private Vector2 localMousePosition;

    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset buildingOptionButton;
    public VisualTreeAsset resourceNeedTemplate;

    private ProductionBuilding selectedProductionBuilding;

    private BuildingSlot buildingSlot;
    private Button buildButton;

    private void Update()
    {
        if (Input.GetMouseButton(0) & mouseOnMenu) MoveWindow(root, Input.mousePosition);
    }

    private void OnDestroy() 
    {
        buildingSlot.GetPlanet().SetBuildingChooserMenu(null);
        buildingSlot.GetPlanet().UpdateResourceDisplays();
        UIController.UpdateMoney();
    }

    public void MakeBuildingChooserMenu(BuildingSlot buildingSlot, List<ProductionBuilding> possibleProductionBuildings)
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

        buildButton = root.Q<Button>("buildbutton");
        buildButton.clicked += BuildSelected;

        VisualElement buildingButtonContainer = root.Q<VisualElement>("buildingbutton_container");

        root.Q<VisualElement>("moneyicon").style.unityBackgroundImageTintColor = 
            new StyleColor(PlayerInventory.instance.moneyResource.spriteColor);

        selectedProductionBuilding = possibleProductionBuildings.ElementAt(0);
        UpdateSelectedInfo(root);

        foreach (ProductionBuilding productionBuilding in possibleProductionBuildings)
        {
            VisualElement buildingOption = buildingOptionButton.Instantiate();
            Button optionButton = buildingOption.Q<Button>("optionbutton");
            optionButton.clicked += () => 
            {
                SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_SELECT);
                selectedProductionBuilding = productionBuilding;
                UpdateBuildButton();
                UpdateSelectedInfo(root); 
            };
            optionButton.style.backgroundImage = new StyleBackground(productionBuilding.buildingSprite);
            optionButton.style.unityBackgroundImageTintColor = new StyleColor(productionBuilding.outputResource.resource.spriteColor);

            buildingOption.style.flexGrow = 1;
            buildingButtonContainer.Add(buildingOption);
        }

        buildingSlot.GetPlanet().UpdateResourceDisplays();
    }

    private void UpdateSelectedInfo(VisualElement root)
    {
        root.Q<Label>("name").text = selectedProductionBuilding.name;
        root.Q<Label>("upkeep").text = selectedProductionBuilding.upkeep + "/Cycle";

        VisualElement producesImage = root.Q<VisualElement>("outputimage");
        producesImage.style.backgroundImage = new StyleBackground(selectedProductionBuilding.outputResource.resource.resourceSprite);
        producesImage.style.unityBackgroundImageTintColor = new StyleColor(selectedProductionBuilding.outputResource.resource.spriteColor);
        //producesImage.style.flexGrow = 1;

        root.Q<Label>("outputvalue").text = selectedProductionBuilding.outputResource.amount + "/Cycle";

        //buildingOption.style.flexGrow = 1;

        VisualElement costList = root.Q<VisualElement>("costlist");
        costList.Clear();
        foreach (ResourceAmount buildingCost in selectedProductionBuilding.cost)
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
        foreach (ResourceAmount resourceNeed in selectedProductionBuilding.inputResources)
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
        if (buildingSlot.CanBuildBuilding(selectedProductionBuilding))
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
            buildingSlot.BuildBuilding(selectedProductionBuilding);
            UIController.RemoveLastFromUIStack();
        }
    }

    public void UpdateResourcePanel(List<VisualElement> resourceContainers)
    {
        UpdateBuildButton();
        VisualElement resourcesPanel = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("resourcespanel");
        resourcesPanel.Clear();
        foreach (VisualElement resourceContainer in resourceContainers) resourcesPanel.Add(resourceContainer);
    }

    private void UpdateBuildButton()
    {
        if (buildingSlot.CanBuildBuilding(selectedProductionBuilding)) buildButton.SetEnabled(true);
        else buildButton.SetEnabled(false);
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
