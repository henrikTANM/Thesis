using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingChooserMenu : MonoBehaviour
{
    public VisualTreeAsset buildingOptionTemplate;
    public VisualTreeAsset resourceCostTemplate;
    public VisualTreeAsset resourceNeedTemplate;

    public void MakeBuildingChooserMenu(BuildingSlot buildingSlot, UIDocument root, List<ProductionBuilding> possibleProductionBuildings)
    {
        VisualElement buildingButtonContainer =
           root.rootVisualElement.Q<VisualElement>("buildingbutton_container");

        Button exitButton = root.rootVisualElement.Q<Button>("exitbutton");
        exitButton.clicked += buildingSlot.CloseBuildingChooserMenu;

        buildingButtonContainer.Clear();

        foreach (ProductionBuilding productionBuilding in possibleProductionBuildings)
        {
            TemplateContainer buildingOption = buildingOptionTemplate.Instantiate();

            buildingOption.Q<Label>("building_name").text = productionBuilding.name;
            buildingOption.Q<VisualElement>("building_image").style.backgroundImage =
                new StyleBackground(productionBuilding.buildingSprite);

            foreach (ProductionBuilding.ResourceCount buildingCost in productionBuilding.cost)
            {
                TemplateContainer buildingCostTemplate = resourceCostTemplate.Instantiate();
                buildingCostTemplate.Q<Label>("cost").text = buildingCost.amount.ToString();
                VisualElement costResourceImage = buildingCostTemplate.Q<VisualElement>("cost_image");
                costResourceImage.style.backgroundImage =
                    new StyleBackground(buildingCost.resource.resourceSprite);
                buildingOption.Q<VisualElement>("cost_list").Add(buildingCostTemplate);
            }

            buildingOption.Q<Label>("upkeep_text").text = productionBuilding.upkeep + "/Cycle";

            foreach (ProductionBuilding.ResourceCount resourceNeed in productionBuilding.inputResources)
            {
                TemplateContainer buildingNeedTemplate = resourceNeedTemplate.Instantiate();
                buildingNeedTemplate.Q<Label>("need").text = resourceNeed.amount + "/Cycle";
                VisualElement costResourceImage = buildingNeedTemplate.Q<VisualElement>("need_image");
                costResourceImage.style.backgroundImage =
                    new StyleBackground(resourceNeed.resource.resourceSprite);
                buildingOption.Q<VisualElement>("needs_list").Add(buildingNeedTemplate);
            }

            VisualElement producesImage = buildingOption.Q<VisualElement>("produces_image");
            producesImage.style.backgroundImage =
                new StyleBackground(productionBuilding.outputResource.resource.resourceSprite);
            producesImage.style.flexGrow = 1;

            buildingOption.Q<Label>("produces_text").text = productionBuilding.outputResource.amount + "/Cycle";

            buildingOption.style.flexGrow = 1;

            buildingButtonContainer.Add(buildingOption);

            Button button = buildingOption.Q<Button>("choose_button");
            button.clicked += () => buildingSlot.SetProductionBuilding(productionBuilding);
        }
    }


}
