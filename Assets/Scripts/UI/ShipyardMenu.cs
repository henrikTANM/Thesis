using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipyardMenu : MonoBehaviour
{
    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset resourceNeedTemplate;
    public VisualTreeAsset shipsOptionTemplate;

    private UIController uiController;
    private PlayerInventory inventory;

    private Button buildButton;

    public List<SpaceShipValues> shipValues;
    private SpaceShipValues selectedSpaceShip;

    // COLOR CHANGE PROBLEM INLINE
    /*
    [SerializeField] private Color failColor;
    [SerializeField] private Color originalColor;

    public Color whiteish;
    public Color black1;
    public Color black2;

    private Button previousButton;
    */

    public void MakeShipyardMenu(PlanetMenu planetMenu, Planet planet)
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        inventory = GameObject.Find("PlayerInventory").GetComponent<PlayerInventory>();
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += () =>
        {
            planet.SetShipyardMenu(null);
            uiController.RemoveLastFromUIStack();
        };

        selectedSpaceShip = shipValues.ElementAt(0);

        buildButton = root.Q<Button>("buildbutton");
        buildButton.clicked += () => { BuildSelected(planet); };

        UpdateResourcePanel(planet);
        UpdateSelectedInfo(planet, root);

        Button deconstructButton = root.Q<Button>("deconstructbutton");
        deconstructButton.clicked += () =>
        {
            planet.SetSpecialBuilding(null);
            planetMenu.ChangeSpecialBuildingButtonImage();
            uiController.RemoveLastFromUIStack();
        };

        VisualElement shipButtonContainer = root.Q<VisualElement>("shipsbuttoncontainer");
        foreach (SpaceShipValues spaceShipValues in shipValues)
        {
            VisualElement shipsOption = shipsOptionTemplate.Instantiate();
            Button optionButton = shipsOption.Q<Button>("optionbutton");
            //if (previousButton == null) { previousButton = optionButton; }
            optionButton.clicked += () =>
            {
                /*
                ButtonStyleRepository repository = new ButtonStyleRepository();
                repository.ChangeStyle(previousButton, black1, black1, whiteish);
                repository.ChangeStyle(optionButton, whiteish, whiteish, black2);


                previousButton = optionButton;
                */

                selectedSpaceShip = spaceShipValues;
                UpdateSelectedInfo(planet, root);

            };

            shipsOption.style.flexGrow = 1;
            shipButtonContainer.Add(shipsOption);
        }
    }

    private void BuildSelected(Planet planet)
    {
        if (planet.CanBuild(selectedSpaceShip.cost))
        {
            inventory.AddShip(planet, selectedSpaceShip);
            uiController.RemoveLastFromUIStack();
            uiController.RemoveLastFromUIStack();
            uiController.MakeShipsMenu();
        }
    }

    private void UpdateSelectedInfo(Planet planet, VisualElement root)
    {
        UpdateBuildButton(planet);

        root.Q<Label>("name").text = selectedSpaceShip.name;
        root.Q<Label>("acceleration").text = "Acceleraion rate: " + selectedSpaceShip.accelerationRate;
        root.Q<Label>("fuelcapacity").text = "Fuel capacity: " + selectedSpaceShip.fuelCapacity + " tonnes";
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

    private void UpdateBuildButton(Planet planet)
    {
        if (!planet.CanBuild(selectedSpaceShip.cost))
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


}
