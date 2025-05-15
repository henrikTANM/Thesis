using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static ProductionBuilding;

public class BuildingSlot : MonoBehaviour
{
    private Button button;

    private DepositHandler depositHandler;
    private Planet planet;

    private UIController uiController;

    [SerializeField] private GameObject buildingChooserMenuPrefab;
    [SerializeField] private GameObject buildingViewerMenuPrefab;

    private ProductionBuildingHandler productionBuildingHandler;

    [SerializeField] private Sprite constrictionSprite;
    [SerializeField] private Color constrictionSpriteColor;


    private void Awake()
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
    }


    public Planet GetPlanet() { return planet; }
    public Button GetButton() { return button; }

    public void SetDeposit(DepositHandler depositHandler) { this.depositHandler = depositHandler; }
    public void SetPlanet(Planet planet) { this.planet = planet; }

    public void SetButton(Button button)
    {
        this.button = button;
        button.clicked += HandleBuildingSlotClicked;
    }

    public bool CanBuildBuilding(ProductionBuilding productionBuilding)
    {
        foreach (ResourceAmount resourceNeeded in productionBuilding.cost)
        {
            if (resourceNeeded.resource.type == Resource.Type.MONEY) { if (!PlayerInventory.CanChangeMoneyAmount(resourceNeeded.amount)) return false; }
            else if (!planet.GetPlanetResourceHandler().CanChangeResourceAmount(resourceNeeded)) return false;
        }
        return true;
    }

    public void BuildBuilding(ProductionBuilding productionBuilding)
    {
        foreach (ResourceAmount resourceNeeded in productionBuilding.cost)
        {
            if (resourceNeeded.resource.type == Resource.Type.MONEY) PlayerInventory.ChangeMoneyAmount(-resourceNeeded.amount);
            else planet.GetPlanetResourceHandler().ChangeResourceAmount(new ResourceAmount(resourceNeeded.resource, -resourceNeeded.amount));
        }

        button.style.backgroundImage = new StyleBackground(productionBuilding.buildingSprite);
        button.style.unityBackgroundImageTintColor = new StyleColor(productionBuilding.outputResource.resource.spriteColor);

        productionBuildingHandler = new ProductionBuildingHandler(productionBuilding, planet, this);
    } 

    public void DeleteBuilding(List<ResourceAmount> costAmounts)
    {
        foreach (ResourceAmount resourceRefund in costAmounts)
        {
            if (resourceRefund.resource.type == Resource.Type.MONEY) PlayerInventory.ChangeMoneyAmount(resourceRefund.amount / 2);
            else planet.GetPlanetResourceHandler().ChangeResourceAmount(new ResourceAmount(resourceRefund.resource, resourceRefund.amount / 2));
        }

        button.style.backgroundImage = new StyleBackground(constrictionSprite);
        button.style.unityBackgroundImageTintColor = new StyleColor(constrictionSpriteColor);

        productionBuildingHandler.RemoveResourceFactors();
        productionBuildingHandler = null;
    }

    public void HandleBuildingSlotClicked()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ENTER);
        if (productionBuildingHandler != null) MakeBuildingViewerMenu();
        else MakeBuildingChooserMenu();
    }

    private void MakeBuildingViewerMenu()
    {
        GameObject buildingViewerMenu = Instantiate(buildingViewerMenuPrefab);
        UIDocument buildingViewerMenuUI = buildingViewerMenu.GetComponent<UIDocument>();
        planet.SetBuildingViewerMenu(buildingViewerMenu);
        buildingViewerMenu.GetComponent<BuildingViewerMenu>().MakeBuildingViewerMenu(this, productionBuildingHandler);
        UIController.AddToUIStack(new UIElement(buildingViewerMenu, buildingViewerMenuUI), false);
    }

    private void MakeBuildingChooserMenu()
    {
        GameObject buildingChooserMenu = Instantiate(buildingChooserMenuPrefab);
        UIDocument buildingChooserMenuUI = buildingChooserMenu.GetComponent<UIDocument>();
        planet.SetBuildingChooserMenu(buildingChooserMenu);
        buildingChooserMenu.GetComponent<BuildingChooserMenu>().MakeBuildingChooserMenu(this, depositHandler.GetPossibleProductionBuildings());
        UIController.AddToUIStack(new UIElement(buildingChooserMenu, buildingChooserMenuUI), false);
    }
}
