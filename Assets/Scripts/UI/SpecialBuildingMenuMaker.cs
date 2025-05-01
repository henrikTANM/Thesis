using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpecialBuildingMenuMaker : MonoBehaviour
{
    [SerializeField] private GameObject bonusBuildingViewerPrefab;
    [SerializeField] private GameObject shipyardMenuPrefab;
    [SerializeField] private GameObject tradeHubMenuPrefab;
    [SerializeField] private GameObject discoveryHubMenuPrefab;
    [SerializeField] private GameObject bhcfMenuPrefab;

    public void MakeMenu(Planet planet, SpecialBuilding specialBuilding)
    {
        SpecialBuilding.Type type = specialBuilding.type;
        if (type == SpecialBuilding.Type.MACHINERY | type == SpecialBuilding.Type.LOGISTICS) 
        { MakeBonusBuildingViewerMenu(planet, specialBuilding); }
        else if (type == SpecialBuilding.Type.SHIPYARD) { MakeShipyardMenu(planet); }
        else if (type == SpecialBuilding.Type.TRADE) { MakeTradeHubMenu(planet); }
        else if (type == SpecialBuilding.Type.DISCOVERY) { MakeDiscoveryHubMenu(planet); }
        else if (type == SpecialBuilding.Type.BHCF) {  MakeBHCFMenu(planet); }
    }

    private void MakeBonusBuildingViewerMenu(Planet planet, SpecialBuilding specialBuilding)
    {
        GameObject bonusBuildingViewer = Instantiate(bonusBuildingViewerPrefab);
        UIDocument bonusBuildingViewerUI = bonusBuildingViewer.GetComponent<UIDocument>();
        bonusBuildingViewer.GetComponent<BonusBuildingViewer>().MakeBonusBuildingViewer(planet, specialBuilding);
        UIController.AddToUIStack(new UIElement(bonusBuildingViewer, bonusBuildingViewerUI), false);
    }

    private void MakeShipyardMenu(Planet planet)
    {
        GameObject shipyardMenu = Instantiate(shipyardMenuPrefab);
        UIDocument shipyardMenuUI = shipyardMenu.GetComponent<UIDocument>();
        planet.SetShipyardMenu(shipyardMenu);
        shipyardMenu.GetComponent<ShipyardMenu>().MakeShipyardMenu(planet);
        UIController.AddToUIStack(new UIElement(shipyardMenu, shipyardMenuUI), false);
    }

    private void MakeTradeHubMenu(Planet planet)
    {
        GameObject tradeHubMenu = Instantiate(tradeHubMenuPrefab);
        UIDocument tradeHubMenuUI = tradeHubMenu.GetComponent<UIDocument>();
        planet.SetTradeHubMenu(tradeHubMenu);
        tradeHubMenu.GetComponent<TradeHubMenu>().MakeTradeHubMenu(planet);
        UIController.AddToUIStack(new UIElement(tradeHubMenu, tradeHubMenuUI), false);
    }

    private void MakeDiscoveryHubMenu(Planet planet)
    {
        GameObject discoveryHubMenu = Instantiate(discoveryHubMenuPrefab);
        UIDocument discoveryHubMenuUI = discoveryHubMenu.GetComponent<UIDocument>();
        planet.SetDiscoveryHubMenu(discoveryHubMenu);
        discoveryHubMenu.GetComponent<DiscoveryHubMenu>().MakeDiscoveryHubMenu(planet);
        UIController.AddToUIStack(new UIElement(discoveryHubMenu, discoveryHubMenuUI), false);
    }

    private void MakeBHCFMenu(Planet planet)
    {
        GameObject bhcfMenu = Instantiate(bhcfMenuPrefab);
        UIDocument bhcfMenuUI = bhcfMenu.GetComponent<UIDocument>();
        planet.SetBhcfMenu(bhcfMenu);
        bhcfMenu.GetComponent<BHCFMenu>().MakeBHCFMenu(planet);
        UIController.AddToUIStack(new UIElement(bhcfMenu, bhcfMenuUI), false);
    }
}
