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

    public void MakePlanetMenu(Planet planet)
    {
        VisualElement root = planet.GetPlanetMenuUI().rootVisualElement;
        root.Q<Label>("planetname").text = planet.GetName();

        List<VisualElement> depositContainers = root.Query("de").ToList();
        List<DepositHandler> deposits = planet.GetDeposits();
        for (int i = 0; i < depositContainers.Count; i++)
        {
            VisualElement depositContainer = depositContainers[i];
            if (i >= deposits.Count)
            {
                depositContainer.SetEnabled(false);
                depositContainer.style.visibility = Visibility.Hidden;
            } 
            else
            {
                depositContainer.style.backgroundImage = 
                    new StyleBackground(deposits.ElementAt(i).GetDepositSprite());
                for (int j = 0; j < deposits.ElementAt(i).GetBuildingCap(); j++)
                {
                    TemplateContainer buildingButton = depositBuildingButtonTemplate.Instantiate();
                    buildingButton.style.flexGrow = 1;
                    depositContainer.Add(buildingButton);
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
    }
}
