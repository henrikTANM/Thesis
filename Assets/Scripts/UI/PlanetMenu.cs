using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlanetMenu : MonoBehaviour
{
    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset depositBuildingButtonTemplate;

    public Sprite blockedConstructionSprite;

    public void MakePlanetMenu(Planet planet)
    {
        VisualElement root = planet.GetPlanetMenuUI().rootVisualElement;
        root.Q<Label>("planetname").text = planet.GetName();

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
                    buildingSlot.SetButton(button);
                }
            }
        }

        VisualElement settlementContainer = root.Q<VisualElement>("settlement");
        settlementContainer.style.backgroundImage =
            new StyleBackground(planet.GetSettlementSprite());
    }

    public void UpdateResourcePanel(Planet planet)
    {
        VisualElement resourcesPanel = planet.GetPlanetMenuUI().rootVisualElement.Q<VisualElement>("ResourcesList");
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
            resourceContainer.Q<Label>("resourcecount").text = resourceCount.amount.ToString() + "+" + resourceCount.perCycle.ToString();
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
}
