using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlanetMenu : MonoBehaviour
{
    UIController uiController;

    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset depositBuildingButtonTemplate;

    public Sprite blockedConstructionSprite;

    [SerializeField] private GameObject tradeMenuPrefab;
    private GameObject tradeMenu;

    [SerializeField] private GameObject specialBuildingChooserMenuPrefab;
    private GameObject specialBuildingChooserMenu;

    [SerializeField] private GameObject bonusBuildingViewerPrefab;
    private GameObject bonusBuildingViewer;

    [SerializeField] private GameObject shipyardMenuPrefab;
    private GameObject shipyardMenu;

    private Button specialBuildingButton;
    public Sprite defaultSpecialButtonImage;

    public void MakePlanetMenu(Planet planet)
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<Label>("planetname").text = planet.GetName();

        Button exitButton = root.Q<Button>("exitbutton"); ;
        exitButton.clicked += uiController.RemoveLastFromUIStack;

        Button tradeButton = root.Q<Button>("tradebutton"); ;
        tradeButton.clicked += () => { MakeTradeMenu(planet); };

        specialBuildingButton = root.Q<Button>("specialbutton"); ;
        specialBuildingButton.clicked += () => { HandleSpecialBuildingButton(planet); };
        specialBuildingButton.style.backgroundImage = new StyleBackground(defaultSpecialButtonImage);

        List<VisualElement> depositContainers = root.Query("de").ToList();
        List<DepositHandler> deposits = planet.GetDeposits();

        for (int i = 0; i < depositContainers.Count; i++)
        {
            VisualElement depositContainer = depositContainers[i];

            if (deposits.ElementAt(i) == null)
            {
                depositContainer.style.backgroundImage =
                    new StyleBackground(blockedConstructionSprite);
                depositContainer.SetEnabled(false);
            }
            else
            {
                depositContainer.style.backgroundImage =
                    new StyleBackground(deposits.ElementAt(i).GetDepositSprite());

                foreach (BuildingSlot buildingSlot in deposits.ElementAt(i).GetBuildingSlots())
                {
                    TemplateContainer buildingButton = depositBuildingButtonTemplate.Instantiate();
                    buildingButton.style.flexGrow = 1;
                    depositContainer.Add(buildingButton);
                    Button button = buildingButton.Q<Button>("building_button");
                    Button buildingSlotButton = buildingSlot.GetButton();
                    if (buildingSlotButton != null)
                    {
                        button.style.backgroundImage = buildingSlotButton.style.backgroundImage;
                        button.style.unityBackgroundImageTintColor = buildingSlotButton.style.unityBackgroundImageTintColor;
                    }
                    buildingSlot.SetButton(button);
                }
            }
        }

        VisualElement settlementContainer = root.Q<VisualElement>("settlement");
        settlementContainer.style.backgroundImage =
            new StyleBackground(planet.GetSettlementSprite());

        UpdateResourcePanel(planet);
    }

    public void UpdateResourcePanel(Planet planet)
    {
        VisualElement resourcesPanel = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("ResourcesList");
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

    private void MakeTradeMenu(Planet planet)
    {
        tradeMenu = Instantiate(tradeMenuPrefab);
        UIDocument tradeMenuUI = tradeMenu.GetComponent<UIDocument>();
        tradeMenu.GetComponent<TradeMenu>().MakeTradeMenu(planet);
        planet.SetTradeMenu(tradeMenu);
        uiController.AddToUIStack(new UIElement(tradeMenu, tradeMenuUI), false);
    }

    private void HandleSpecialBuildingButton(Planet planet)
    {
        SpecialBuilding specialBuilding = planet.GetSpecialBuilding();

        if (specialBuilding == null) { MakeSpecialBuildingChooserMenu(planet); }
        else 
        {
            if (specialBuilding.name == "Shipyard") { MakeShipyardMenu(planet); }
            else { MakeBonusBuildingViewerMenu(planet, specialBuilding); }
        }
    }

    private void MakeSpecialBuildingChooserMenu(Planet planet)
    {
        specialBuildingChooserMenu = Instantiate(specialBuildingChooserMenuPrefab);
        UIDocument specialBuildingChooserMenuUI = specialBuildingChooserMenu.GetComponent<UIDocument>();
        specialBuildingChooserMenu.GetComponent<SpecialBuildingMenu>().MakeSpecialBuildingChooserMenu(this, planet);
        planet.SetSpecialBuildingChooserMenu(specialBuildingChooserMenu);
        uiController.AddToUIStack(new UIElement(specialBuildingChooserMenu, specialBuildingChooserMenuUI), false);
    }

    private void MakeBonusBuildingViewerMenu(Planet planet, SpecialBuilding bonusBuilding)
    {
        bonusBuildingViewer = Instantiate(bonusBuildingViewerPrefab);
        UIDocument bonusBuildingViewerUI = bonusBuildingViewer.GetComponent<UIDocument>();
        bonusBuildingViewer.GetComponent<BonusBuildingViewer>().MakeBonusBuildingViewer(this, planet, bonusBuilding);
        uiController.AddToUIStack(new UIElement(bonusBuildingViewer, bonusBuildingViewerUI), false);
    }

    private void MakeShipyardMenu(Planet planet)
    {
        shipyardMenu = Instantiate(shipyardMenuPrefab);
        UIDocument shipyardMenuUI = shipyardMenu.GetComponent<UIDocument>();
        shipyardMenu.GetComponent<ShipyardMenu>().MakeShipyardMenu(this, planet);
        planet.SetShipyardMenu(shipyardMenu);
        uiController.AddToUIStack(new UIElement(shipyardMenu, shipyardMenuUI), false);
    }

    public void ChangeSpecialBuildingButtonImage(Sprite image) { specialBuildingButton.style.backgroundImage = new StyleBackground(image); }
    public void ChangeSpecialBuildingButtonImage() { specialBuildingButton.style.backgroundImage = new StyleBackground(defaultSpecialButtonImage); }
}
