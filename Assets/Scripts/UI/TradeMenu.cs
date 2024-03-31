using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.MessageBox;

public class TradeMenu : MonoBehaviour
{
    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset tradeableResourceTemplate;

    public PlayerInventory inventory;

    private void Awake()
    {
        inventory = GameObject.Find("PlayerInventory").GetComponent<PlayerInventory>();
    }

    public void MakeTradeMenu(Planet planet)
    {
        VisualElement root = planet.GetTradeMenuUI().rootVisualElement;
        VisualElement buyList = root.Q<VisualElement>("buylist");
        VisualElement sellList = root.Q<VisualElement>("selllist");

        root.Q<VisualElement>("mainwindow").style.backgroundImage = new StyleBackground(planet.GetSettlementSprite());

        foreach (Resource resource in planet.GetTradeableResources())
        {
            VisualElement buyableResource = tradeableResourceTemplate.Instantiate();

            VisualElement buyableResourceImage = buyableResource.Q<VisualElement>("resourceimage");
            buyableResourceImage.style.backgroundImage = new StyleBackground(resource.resourceSprite);
            buyableResourceImage.style.unityBackgroundImageTintColor = new StyleColor(resource.spriteColor);

            Label buyCount = buyableResource.Q<Label>("resourcecount");
            Label buyPrice = buyableResource.Q<Label>("price");

            Button buyMinusButton = buyableResource.Q<Button>("minusbutton");
            buyMinusButton.clicked += () => { ModifyBuyAmount(resource, buyCount, buyPrice, - 5); };
            Button buyPlusButton = buyableResource.Q<Button>("plusbutton");
            buyPlusButton.clicked += () => { ModifyBuyAmount(resource, buyCount, buyPrice, 5); };

            Button buyButton = buyableResource.Q<Button>("buybutton");
            buyButton.clicked += () => 
            {
                ModifyBuyAmount(resource, buyCount, buyPrice, 0);
                buyResources(resource, buyCount, buyPrice, planet); 
            };
            Button buyRepeatingButton = buyableResource.Q<Button>("repeatingbutton");
            buyRepeatingButton.clicked += () => { MakeRepeatingBuyTransaction(); };

            buyList.Add(buyableResource);



            VisualElement sellableResource = tradeableResourceTemplate.Instantiate();

            VisualElement sellableResourceImage = sellableResource.Q<VisualElement>("resourceimage");
            sellableResourceImage.style.backgroundImage = new StyleBackground(resource.resourceSprite);
            sellableResourceImage.style.unityBackgroundImageTintColor = new StyleColor(resource.spriteColor);

            Label sellCount = sellableResource.Q<Label>("resourcecount");
            Label sellPrice = sellableResource.Q<Label>("price");

            Button sellMinusButton = sellableResource.Q<Button>("minusbutton");
            sellMinusButton.clicked += () => { ModifySellAmount(planet, resource, buyCount, buyPrice, -1); };
            Button sellPlusButton = sellableResource.Q<Button>("plusbutton");
            sellPlusButton.clicked += () => { ModifySellAmount(planet, resource, buyCount, buyPrice, 1); };

            Button sellButton = sellableResource.Q<Button>("buybutton");
            sellButton.clicked += () => 
            {
                ModifySellAmount(planet, resource, sellCount, sellPrice, 0);
                sellResources(resource, sellCount, sellPrice, planet);
            };
            Button sellRepeatingButton = sellableResource.Q<Button>("repeatingbutton");
            sellRepeatingButton.clicked += () => { MakeRepeatingSellTransaction(); };

            sellList.Add(sellableResource);
        }
    }

    public void UpdateResourcePanel(Planet planet)
    {
        VisualElement resourcesPanel = planet.GetTradeMenuUI().rootVisualElement.Q<VisualElement>("resourcespanel");
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

    private void buyResources(Resource resource, Label amount, Label price, Planet planet)
    {
        planet.GetPlanetResourceHandler().AddResouce(resource, int.Parse(amount.text));
        inventory.RemoveMoney(int.Parse(price.text));
        planet.UpdateResourceDisplays();
    }

    private void MakeRepeatingBuyTransaction()
    {

    }

    private void ModifyBuyAmount(Resource resource, Label countlabel, Label priceLabel, int multiplier)
    {
        int countlabelValue = int.Parse(countlabel.text);
        int newCountlabelValue = countlabelValue + multiplier * (Input.GetKey(KeyCode.LeftShift) ? 10 : 1);
        if (newCountlabelValue >= 0)
        {
            countlabelValue = newCountlabelValue;
            int priceLabelValue = countlabelValue * resource.defaultValue * 2;
            if (priceLabelValue <= inventory.GetMoney())
            {
                countlabel.text = countlabelValue.ToString();
                priceLabel.text = priceLabelValue.ToString();
            }
        }
    }

    private void sellResources(Resource resource, Label amount, Label price, Planet planet)
    {
        planet.UpdateResourceDisplays();
    }

    private void MakeRepeatingSellTransaction()
    {

    }

    private void ModifySellAmount(Planet planet, Resource resource, Label countlabel, Label priceLabel, int multiplier)
    {
        ResourceCount resourceCount = planet.GetPlanetResourceHandler().GetResourceCount(resource);

        if (resourceCount != null)
        {
            int countlabelValue = int.Parse(countlabel.text);
            countlabelValue += multiplier * (Input.GetKey(KeyCode.LeftShift) ? 10 : 1);
            if (countlabelValue >= 0 & countlabelValue <= resourceCount.amount)
            {
                int priceLabelValue = countlabelValue * resource.defaultValue;
                countlabel.text = countlabelValue.ToString();
                priceLabel.text = priceLabelValue.ToString();
            }
        }
    }
}
