using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingSlot : MonoBehaviour
{
    private Button button;

    private DepositHandler depositHandler;

    private UIController uiController;
    [SerializeField] private GameObject buildingChooserMenuPrefab;
    private GameObject buildingChooserMenu;

    private ProductionBuilding productionBuilding;

    private void Awake()
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
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
        this.productionBuilding = productionBuilding;
        button.style.backgroundImage = new StyleBackground(productionBuilding.buildingSprite);
        CloseBuildingChooserMenu();
    }

    public void CloseBuildingChooserMenu()
    {
        uiController.UnSetCurrentUI();
        print(depositHandler.GetPlanet());
        print(depositHandler.GetPlanet().GetPlanetMenuUI());
        uiController.SetCurrentUI(depositHandler.GetPlanet().GetPlanetMenuUI());
        Destroy(buildingChooserMenu);
    }

    private void ButtonClicked()
    {
        if (productionBuilding != null)
        {

        }
        else
        {
            buildingChooserMenu = Instantiate(buildingChooserMenuPrefab);
            UIDocument buildingChooserMenuUI = buildingChooserMenu.GetComponent<UIDocument>();
            buildingChooserMenu.GetComponent<BuildingChooserMenu>().MakeBuildingChooserMenu(this, buildingChooserMenuUI, depositHandler.GetPossibleProductionBuildings());
            uiController.UnSetCurrentUI();
            uiController.SetCurrentUI(buildingChooserMenuUI);
        }
    }

}
