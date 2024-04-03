using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static ProductionBuilding;

public class BuildingSlot : MonoBehaviour
{
    private Button button;

    private DepositHandler depositHandler;

    private UIController uiController;

    [SerializeField] private GameObject buildingChooserMenuPrefab;
    private GameObject buildingChooserMenu;
    [SerializeField] private GameObject buildingViewerMenuPrefab;
    private GameObject buildingViewerMenu;

    private ProductionBuildingHandler productionBuildingHandler;

    private PlayerInventory playerInventory;

    [SerializeField] private Sprite constrictionSprite;
    [SerializeField] private Color constrictionSpriteColor;


    private void Awake()
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        playerInventory = GameObject.Find("PlayerInventory").GetComponent<PlayerInventory>();
    }

    public void SetDeposit(DepositHandler depositHandler)
    {
        this.depositHandler = depositHandler;
    }

    public void SetButton(Button button)
    {
        this.button = button;
        button.clicked += ButtonClicked;
    }

    public void SetProductionBuilding(ProductionBuilding productionBuilding)
    {
        productionBuildingHandler = new ProductionBuildingHandler(productionBuilding, depositHandler.GetPlanet());
        button.style.backgroundImage = new StyleBackground(productionBuilding.buildingSprite);
        button.style.unityBackgroundImageTintColor = new StyleColor(productionBuilding.outputResource.resource.spriteColor);
        CloseMenu();
    }

    public void RemoveProductionBuilding()
    {
        button.style.backgroundImage = new StyleBackground(constrictionSprite);
        button.style.unityBackgroundImageTintColor = new StyleColor(constrictionSpriteColor);
        if(productionBuildingHandler.IsActive()) productionBuildingHandler.SetActive(false);
        productionBuildingHandler = null;
        CloseMenu();
    }



    public bool CanBuildBuilding(ProductionBuilding productionBuilding)
    {
        foreach (ResourceAmount resourceNeeded in productionBuilding.cost)
        {
            if (resourceNeeded.resource == playerInventory.GetMoneyResource())
            {
                if (playerInventory.GetMoney() < resourceNeeded.amount) return false;
            }
            else
            {
                ResourceCount resourceCount = depositHandler.GetPlanet().GetPlanetResourceHandler().GetResourceCount(resourceNeeded.resource);
                if (resourceCount == null) return false;
                if (resourceCount.amount < resourceNeeded.amount) return false;
            }
        }
        return true;
    }

    public void BuildBuilding(ProductionBuilding productionBuilding)
    {
        Planet planet = depositHandler.GetPlanet();
        foreach (ResourceAmount resourceNeeded in productionBuilding.cost)
        {
            if (resourceNeeded.resource == playerInventory.GetMoneyResource())
            {
                playerInventory.RemoveMoney(resourceNeeded.amount);
            }
            else
            {
                planet.GetPlanetResourceHandler().RemoveResouce(resourceNeeded.resource, resourceNeeded.amount);
            }
        }
        SetProductionBuilding(productionBuilding);
        planet.SetActiveBuildingChooserMenu(null);
        planet.UpdateResourceDisplays();
    } 

    public void DeleteBuilding(List<ResourceAmount> costAmounts)
    {
        Planet planet = depositHandler.GetPlanet();
        foreach (ResourceAmount resourceRefund in costAmounts)
        {
            if (resourceRefund.resource == playerInventory.GetMoneyResource())
            {
                playerInventory.AddMoney(resourceRefund.amount / 2);
            }
            else
            {
                depositHandler.GetPlanet().GetPlanetResourceHandler().AddResouce(resourceRefund.resource, resourceRefund.amount / 2);
            }
        }
        planet.RemoveProductionBuildingHandler(productionBuildingHandler);
        RemoveProductionBuilding();
        planet.SetActiveBuildingViewerMenu(null);
        planet.UpdateResourceDisplays();
    }

    public void CloseMenu()
    {
        uiController.RemoveLastFromUIStack();
    }

    private void ButtonClicked()
    {
        if (productionBuildingHandler != null)
        {
            buildingViewerMenu = Instantiate(buildingViewerMenuPrefab);
            UIDocument buildingViewerMenuUI = buildingViewerMenu.GetComponent<UIDocument>();
            buildingViewerMenu.GetComponent<BuildingViewerMenu>().MakeBuildingViewerMenu(this, productionBuildingHandler);
            depositHandler.GetPlanet().SetActiveBuildingViewerMenu(buildingViewerMenu);
            depositHandler.GetPlanet().UpdateResourceDisplays();
            uiController.AddToUIStack(new UIElement(buildingViewerMenu, buildingViewerMenuUI), false);
        }
        else
        {
            buildingChooserMenu = Instantiate(buildingChooserMenuPrefab);
            UIDocument buildingChooserMenuUI = buildingChooserMenu.GetComponent<UIDocument>();
            buildingChooserMenu.GetComponent<BuildingChooserMenu>().MakeBuildingChooserMenu(this, depositHandler.GetPossibleProductionBuildings());
            depositHandler.GetPlanet().SetActiveBuildingChooserMenu(buildingChooserMenu);
            depositHandler.GetPlanet().UpdateResourceDisplays();
            uiController.AddToUIStack(new UIElement(buildingChooserMenu, buildingChooserMenuUI), false);
        }
    }

    public ProductionBuildingHandler GetProductionBuildingHandler() { return productionBuildingHandler; }

}
