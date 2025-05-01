using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipyardMenu : MonoBehaviour
{
    private VisualElement root;

    private bool mouseOnMenu = false;
    private Vector2 localMousePosition;

    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset resourceNeedTemplate;
    public VisualTreeAsset shipsOptionTemplate;

    private Planet planet;

    private Button buildButton;

    public List<SpaceShip> shipValues;
    private SpaceShip selectedSpaceShip;

    private void Update()
    {
        if (Input.GetMouseButton(0) & mouseOnMenu) MoveWindow(root, Input.mousePosition);
    }

    private void OnDestroy()
    {
        planet.SetShipyardMenu(null);
        planet.UpdateResourceDisplays();
        UIController.UpdateMoney();
    }

    public void MakeShipyardMenu(Planet planet)
    {
        this.planet = planet;

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

        selectedSpaceShip = shipValues.ElementAt(0);

        buildButton = root.Q<Button>("buildbutton");
        buildButton.clicked += () => { BuildSelected(); };

        UpdateSelectedInfo(root);

        Button deconstructButton = root.Q<Button>("deconstructbutton");
        deconstructButton.clicked += () =>
        {
            planet.SetSpecialBuilding(null);
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
            UIController.RemoveLastFromUIStack();
        };

        VisualElement shipButtonContainer = root.Q<VisualElement>("shipsbuttoncontainer");
        foreach (SpaceShip spaceShipValues in shipValues)
        {
            VisualElement shipsOption = shipsOptionTemplate.Instantiate();
            Button optionButton = shipsOption.Q<Button>("optionbutton");
            //if (previousButton == null) { previousButton = optionButton; }
            optionButton.clicked += () =>
            {
                SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_SELECT);
                selectedSpaceShip = spaceShipValues;
                UpdateSelectedInfo(root);
            };

            shipsOption.style.flexGrow = 1;
            shipButtonContainer.Add(shipsOption);
        }

        planet.UpdateResourceDisplays();
    }

    private void BuildSelected()
    {
        if (CanBuild())
        {
            foreach (ResourceAmount cost in selectedSpaceShip.cost)
            {
                if (cost.resource.type == Resource.Type.MONEY) PlayerInventory.ChangeMoneyAmount(-cost.amount);
                else planet.GetPlanetResourceHandler().ChangeResourceAmount(new ResourceAmount(cost.resource, -cost.amount));
            }
            PlayerInventory.AddShip(planet, selectedSpaceShip);
            planet.UpdateResourceDisplays();
            UIController.UpdateShipsList();
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
        }
    }

    private void UpdateSelectedInfo(VisualElement root)
    {
        buildButton.SetEnabled(CanBuild());

        root.Q<Label>("name").text = selectedSpaceShip.name;
        root.Q<Label>("cargocapacity").text = "Cargo capacity: " + selectedSpaceShip.cargoCapacity + " units";

        VisualElement costList = root.Q<VisualElement>("costlist");
        costList.Clear();
        foreach (ResourceAmount shipCost in selectedSpaceShip.cost)
        {
            VisualElement shipCostTemplate = resourceNeedTemplate.Instantiate();
            shipCostTemplate.Q<Label>("need").text = shipCost.amount.ToString();
            VisualElement shipCostTemplateImage = shipCostTemplate.Q<VisualElement>("needimage");
            shipCostTemplateImage.style.backgroundImage = new StyleBackground(shipCost.resource.resourceSprite);
            shipCostTemplateImage.style.unityBackgroundImageTintColor = new StyleColor(shipCost.resource.spriteColor);
            costList.Add(shipCostTemplate);
        }
    }

    public void UpdateResourcePanel(List<VisualElement> resourceContainers)
    {
        buildButton.SetEnabled(CanBuild());

        VisualElement resourcesPanel = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("resourcespanel");
        resourcesPanel.Clear();
        foreach (VisualElement resourceContainer in resourceContainers) resourcesPanel.Add(resourceContainer);
    }

    private VisualElement GetResourceContainer(Resource resource, VisualElement resourcesPanel)
    {
        foreach (VisualElement resourceContainer in resourcesPanel.Children())
        {
            if (resourceContainer.name == resource.name) return resourceContainer;
        }
        return null;
    }

    private bool CanBuild()
    {
        foreach (ResourceAmount resourceNeeded in selectedSpaceShip.cost)
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
