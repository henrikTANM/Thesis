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
    private UIDocument buildingChooserMenuUI;
    [SerializeField] private GameObject buildingViewerMenuPrefab;
    private GameObject buildingViewerMenu;
    private UIDocument buildingViewerMenuUI;

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
        CloseBuildingChooserMenu();
    }

    public void RemoveProductionBuilding()
    {
        button.style.backgroundImage = new StyleBackground(constrictionSprite);
        button.style.unityBackgroundImageTintColor = new StyleColor(constrictionSpriteColor);
        if(productionBuildingHandler.IsActive()) productionBuildingHandler.SetActive(false);
        productionBuildingHandler = null;
        CloseBuildingViewerMenu();
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

    public UIDocument GetBuildingChooserMenuUI()
    {
        return buildingChooserMenuUI;
    }

    public UIDocument GetBuildingViewerMenuUI()
    {
        return buildingViewerMenuUI;
    }

    public void CloseBuildingViewerMenu()
    {
        uiController.UnSetCurrentUI();
        uiController.SetCurrentUI(depositHandler.GetPlanet().GetPlanetMenuUI());
        Destroy(buildingViewerMenu);
    }

    public void CloseBuildingChooserMenu()
    {
        uiController.UnSetCurrentUI();
        uiController.SetCurrentUI(depositHandler.GetPlanet().GetPlanetMenuUI());
        Destroy(buildingChooserMenu);
    }

    private void ButtonClicked()
    {
        if (productionBuildingHandler != null)
        {
            buildingViewerMenu = Instantiate(buildingViewerMenuPrefab);
            depositHandler.GetPlanet().SetActiveBuildingViewerMenu(buildingViewerMenu);
            buildingViewerMenuUI = buildingViewerMenu.GetComponent<UIDocument>();
            buildingViewerMenu.GetComponent<BuildingViewerMenu>().MakeBuildingViewerMenu(this, productionBuildingHandler);
            depositHandler.GetPlanet().UpdateResourceDisplays();
            uiController.UnSetCurrentUI();
            uiController.SetCurrentUI(buildingViewerMenuUI);
        }
        else
        {
            buildingChooserMenu = Instantiate(buildingChooserMenuPrefab);
            depositHandler.GetPlanet().SetActiveBuildingChooserMenu(buildingChooserMenu);
            buildingChooserMenuUI = buildingChooserMenu.GetComponent<UIDocument>();
            buildingChooserMenu.GetComponent<BuildingChooserMenu>().MakeBuildingChooserMenu(this, depositHandler.GetPossibleProductionBuildings());
            depositHandler.GetPlanet().UpdateResourceDisplays();
            uiController.UnSetCurrentUI();
            uiController.SetCurrentUI(buildingChooserMenuUI);
        }
    }

    public ProductionBuildingHandler GetProductionBuildingHandler() { return productionBuildingHandler; }

}
