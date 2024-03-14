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

    public List<Sprite> settlementSprites;
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

        VisualElement resourcesPanel = root.Q<VisualElement>("ResourcesList");
        for (int i = 0; i < 6; i++)
        {
            TemplateContainer resourceCounter = resourceTemplate.Instantiate();
            resourceCounter.style.alignSelf = Align.Center;
            resourcesPanel.Add(resourceCounter);
        }

        VisualElement settlementContainer = root.Q<VisualElement>("settlement");
        settlementContainer.style.backgroundImage =
            new StyleBackground(settlementSprites.ElementAt(Random.Range(0, settlementSprites.Count())));
    }


}
