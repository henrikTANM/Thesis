using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SpecialBuildingChooserMenu : MonoBehaviour
{
    private VisualElement root;

    private bool mouseOnMenu = false;
    private Vector2 localMousePosition;

    public VisualTreeAsset specialBuildingButtonTemplate;
    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset resourceNeedTemplate;

    private List<SpecialBuilding> specialBuildings;

    public ResourceAmount discoveryHubInputResourceAmount;
    public List<ResourceAmount> bchfInputResourceAmounts;

    private SpecialBuilding selectedSpecialBuilding;

    private Planet planet;

    private Button buildButton;

    private void Update()
    {
        if (Input.GetMouseButton(0) & mouseOnMenu) MoveWindow(root, Input.mousePosition);
    }

    private void OnDestroy()
    {
        planet.SetSpecialBuildingChooserMenu(null);
        planet.UpdateResourceDisplays();
        UIController.UpdateMoney();
    }

    public void MakeSpecialBuildingChooserMenu(Planet planet)
    {
        this.planet = planet;
        specialBuildings = planet.GetPossibleSpecialBuildings();

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
        buildButton.clicked += () => { if (selectedSpecialBuilding != null) BuildSelected(); };

        VisualElement specialBuildingButtons = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("specialoptions");

        foreach (SpecialBuilding building in specialBuildings)
        {
            VisualElement specialBuildingButtonContainer = specialBuildingButtonTemplate.Instantiate();

            Button specialBuildingButton = specialBuildingButtonContainer.Q<Button>("button");
            //specialBuildingButton.RegisterCallback<MouseEnterEvent>(x => OnMouseEnter(x));
            specialBuildingButton.clicked += () => 
            {
                SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_SELECT);
                selectedSpecialBuilding = building;
                buildButton.text = "Build " + building.name;
                UpdateBuildButton();
            };

            specialBuildingButton.Q<Label>("name").text = building.name;
            specialBuildingButton.Q<Label>("desc").text = building.description;

            VisualElement image = specialBuildingButton.Q<VisualElement>("image");
            image.style.backgroundImage = new StyleBackground(building.buildingSprite);

            VisualElement specialCost = specialBuildingButton.Q<VisualElement>("cost");
            foreach (ResourceAmount costAmount in building.cost)
            {
                VisualElement cost = resourceNeedTemplate.Instantiate();
                cost.Q<Label>("need").text = costAmount.amount.ToString();
                VisualElement costImage = cost.Q<VisualElement>("needimage");
                costImage.style.backgroundImage = new StyleBackground(costAmount.resource.resourceSprite);
                costImage.style.unityBackgroundImageTintColor = new StyleColor(costAmount.resource.spriteColor);
                specialCost.Add(cost);
            }
            specialBuildingButton.style.width = Length.Percent(100.0f / specialBuildings.Count);
            specialBuildingButtons.Add(specialBuildingButton);
        }

        planet.UpdateResourceDisplays();
    }

    private void BuildSelected()
    {
        if (CanBuild())
        {
            PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();

            foreach (ResourceAmount cost in selectedSpecialBuilding.cost)
            {
                if (cost.resource.type == Resource.Type.MONEY) PlayerInventory.ChangeMoneyAmount(-cost.amount);
                else planetResourceHandler.ChangeResourceAmount(new ResourceAmount(cost.resource, -cost.amount));
            }

            if (selectedSpecialBuilding.type == SpecialBuilding.Type.MACHINERY) 
            {
                planetResourceHandler.AddRawMultiplier(1.5f);
                planetResourceHandler.AddEndMultiplier(0.5f);
            }
            if (selectedSpecialBuilding.type == SpecialBuilding.Type.LOGISTICS)
            {
                planetResourceHandler.AddRawMultiplier(0.5f);
                planetResourceHandler.AddEndMultiplier(1.5f);
            }
            if (selectedSpecialBuilding.type == SpecialBuilding.Type.DISCOVERY)
            {
                planet.parentStar.hasDiscovery = true;
                planet.SetDiscoveryHubHandler(new DiscoveryHubHandler(
                    selectedSpecialBuilding, 
                    discoveryHubInputResourceAmount, 
                    planet));
            }
            if (selectedSpecialBuilding.type == SpecialBuilding.Type.BHCF)
            {
                planet.SetBHCFHandler(new BHCFHandler(
                    selectedSpecialBuilding,
                    bchfInputResourceAmounts,
                    planet
                    ));
            }

            planet.SetSpecialBuilding(selectedSpecialBuilding);
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
            UIController.RemoveLastFromUIStack();
        }
    }

    private void UpdateBuildButton()
    {
        if (selectedSpecialBuilding == null) { buildButton.SetEnabled(false); }
        else
        {
            if (CanBuild()) buildButton.SetEnabled(true);
            else buildButton.SetEnabled(false);        }
    }

    public void UpdateResourcePanel(List<VisualElement> resourceContainers)
    {
        UpdateBuildButton();

        VisualElement resourcesPanel = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("resourcespanel");
        resourcesPanel.Clear();
        foreach (VisualElement resourceContainer in resourceContainers) resourcesPanel.Add(resourceContainer);
    }

    private bool CanBuild()
    {
        if (selectedSpecialBuilding.type == SpecialBuilding.Type.DISCOVERY & planet.parentStar.hasDiscovery) return false;
        foreach (ResourceAmount resourceNeeded in selectedSpecialBuilding.cost)
        {
            if (resourceNeeded.resource.type == Resource.Type.MONEY) { if (!PlayerInventory.CanChangeMoneyAmount(resourceNeeded.amount)) return false; }
            else if (!planet.GetPlanetResourceHandler().CanChangeResourceAmount(resourceNeeded)) return false;
        }
        return true;
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
