using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.MessageBox;

public class TradeHubMenu : MonoBehaviour
{
    private Planet planet;

    private VisualElement root;

    private bool mouseOnMenu = false;
    private Vector2 localMousePosition;

    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset tradeableResourceTemplate;

    private void Update()
    {
        if (Input.GetMouseButton(0) & mouseOnMenu) MoveWindow(root, Input.mousePosition);
    }

    private void OnDestroy()
    {
        planet.SetTradeHubMenu(null);
        planet.UpdateResourceDisplays();
        UIController.UpdateMoney();
    }

    public void MakeTradeHubMenu(Planet planet)
    {
        this.planet = planet;

        root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<NavigationSubmitEvent>((evt) =>
        {
            evt.StopPropagation();
        }, TrickleDown.TrickleDown);
        root.RegisterCallback<MouseDownEvent>(evt =>
        {
            mouseOnMenu = true;
            localMousePosition = evt.localMousePosition;
        }, TrickleDown.TrickleDown);
        root.RegisterCallback<MouseUpEvent>(evt => mouseOnMenu = false, TrickleDown.TrickleDown);

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
            UIController.RemoveLastFromUIStack();
        };

        Button deconstructButton = root.Q<Button>("deconstructbutton");
        deconstructButton.clicked += () =>
        {
            planet.SetSpecialBuilding(null);
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
            UIController.RemoveLastFromUIStack();
        };

        VisualElement buyList = root.Q<VisualElement>("buylist");
        VisualElement sellList = root.Q<VisualElement>("selllist");

        //root.Q<VisualElement>("mainwindow").style.backgroundImage = new StyleBackground(planet.GetSettlementSprite());

        foreach (Resource resource in planet.buyableResources)
        {
            VisualElement buyableResource = tradeableResourceTemplate.Instantiate();

            VisualElement buyableResourceImage = buyableResource.Q<VisualElement>("resourceimage");
            buyableResourceImage.style.backgroundImage = new StyleBackground(resource.resourceSprite);
            buyableResourceImage.style.unityBackgroundImageTintColor = new StyleColor(resource.spriteColor);

            Label buyCount = buyableResource.Q<Label>("resourcecount");
            Label buyPrice = buyableResource.Q<Label>("price");

            Button buyButton = buyableResource.Q<Button>("buybutton");
            buyButton.clicked += () => BuyResources(resource, buyCount, buyPrice, buyButton);
            buyButton.SetEnabled(false);

            Button buyMinusButton = buyableResource.Q<Button>("minusbutton");
            buyMinusButton.clickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Shift });
            buyMinusButton.clickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Control });
            buyMinusButton.clicked += () =>
            {
                SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_MINUS);
                ModifyBuyAmount(resource, buyCount, buyPrice, buyButton, true);
            };
            Button buyPlusButton = buyableResource.Q<Button>("plusbutton");
            buyPlusButton.clickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Shift });
            buyPlusButton.clickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Control });
            buyPlusButton.clicked += () =>
            {
                SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_PLUS);
                ModifyBuyAmount(resource, buyCount, buyPrice, buyButton, false);
            };

            buyList.Add(buyableResource);
        }

        foreach (Resource resource in planet.sellableResources)
        {
            VisualElement sellableResource = tradeableResourceTemplate.Instantiate();

            VisualElement sellableResourceImage = sellableResource.Q<VisualElement>("resourceimage");
            sellableResourceImage.style.backgroundImage = new StyleBackground(resource.resourceSprite);
            sellableResourceImage.style.unityBackgroundImageTintColor = new StyleColor(resource.spriteColor);

            Label sellCount = sellableResource.Q<Label>("resourcecount");
            Label sellPrice = sellableResource.Q<Label>("price");

            Button sellButton = sellableResource.Q<Button>("buybutton");
            sellButton.clicked += () => SellResources(resource, sellCount, sellPrice, sellButton);
            sellButton.SetEnabled(false);

            Button sellMinusButton = sellableResource.Q<Button>("minusbutton");
            sellMinusButton.clickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Shift });
            sellMinusButton.clickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Control });
            sellMinusButton.clicked += () =>
            {
                SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_MINUS);
                ModifySellAmount(resource, sellCount, sellPrice, sellButton, true);
            };
            Button sellPlusButton = sellableResource.Q<Button>("plusbutton");
            sellPlusButton.clickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Shift });
            sellPlusButton.clickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Control });
            sellPlusButton.clicked += () => 
            {
                SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_PLUS);
                ModifySellAmount(resource, sellCount, sellPrice, sellButton, false); 
            };

            sellList.Add(sellableResource);
        }

        planet.UpdateResourceDisplays();
    }

    public void UpdateResourcePanel(List<VisualElement> resourceContainers)
    {
        VisualElement resourcesPanel = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("resourcespanel");
        resourcesPanel.Clear();
        foreach (VisualElement resourceContainer in resourceContainers) resourcesPanel.Add(resourceContainer);
    }

    private void BuyResources(Resource resource, Label amount, Label price, Button buyButton)
    {
        PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();
        if (PlayerInventory.instance.moneyAmount < int.Parse(price.text))
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
            buyButton.SetEnabled(false); 
        }
        else
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
            planetResourceHandler.ChangeResourceAmount(new ResourceAmount(resource, int.Parse(amount.text)));
            PlayerInventory.ChangeMoneyAmount(-int.Parse(price.text));
            planet.UpdateResourceDisplays();
            UIController.UpdateMoney();
            buyButton.SetEnabled(false);
        }
        amount.text = "0";
        price.text = "0";
    }

    private void ModifyBuyAmount(Resource resource, Label countlabel, Label priceLabel, Button buyButton, bool minusButton)
    {
        int maxResourceAmount = PlayerInventory.instance.moneyAmount / (resource.defaultValue * 4);

        int toBuy = minusButton ? -10 : 10;
        if (Input.GetKey(KeyCode.LeftShift)) toBuy *= 5;

        int countlabelValue = int.Parse(countlabel.text) + toBuy;
        if (Input.GetKey(KeyCode.LeftControl)) countlabelValue = minusButton ? 0 : maxResourceAmount;
        else countlabelValue += minusButton ? (countlabelValue % 10) : -(countlabelValue % 10);
        int priceLabelValue = countlabelValue * resource.defaultValue * 4;

        if (countlabelValue > 0 & priceLabelValue <= PlayerInventory.instance.moneyAmount)
        {
            countlabel.text = countlabelValue.ToString();
            priceLabel.text = priceLabelValue.ToString();
            buyButton.SetEnabled(true);
        }
        else if (countlabelValue <= 0)
        {
            countlabel.text = 0.0f.ToString();
            priceLabel.text = 0.0f.ToString();
            buyButton.SetEnabled(false);
        }
        else if (priceLabelValue > PlayerInventory.instance.moneyAmount)
        {
            countlabel.text = maxResourceAmount.ToString();
            priceLabel.text = (maxResourceAmount * resource.defaultValue * 4).ToString();
            buyButton.SetEnabled(true);
        }
    }

    private void SellResources(Resource resource, Label amount, Label price, Button sellButton)
    {
        PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();
        if (!planetResourceHandler.CanChangeResourceAmount(new ResourceAmount(resource, -int.Parse(amount.text))))
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
            sellButton.SetEnabled(false);
        }
        else
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
            planetResourceHandler.ChangeResourceAmount(new ResourceAmount(resource, -int.Parse(amount.text)));
            PlayerInventory.ChangeMoneyAmount(int.Parse(price.text));
            planet.UpdateResourceDisplays();
            UIController.UpdateMoney();
            sellButton.SetEnabled(false);
        }
        amount.text = "0";
        price.text = "0";
    }

    private void ModifySellAmount(Resource resource, Label countlabel, Label priceLabel, Button sellButton, bool minusButton)
    {
        int maxResourceAmount = planet.GetPlanetResourceHandler().GetResourceAmount(resource).amount;

        int toBuy = minusButton ? -10 : 10;
        if (Input.GetKey(KeyCode.LeftShift)) toBuy *= 5;

        int countlabelValue = int.Parse(countlabel.text) + toBuy;
        if (Input.GetKey(KeyCode.LeftControl)) countlabelValue = minusButton ? 0 : maxResourceAmount;
        else countlabelValue += minusButton ? (countlabelValue % 10) : -(countlabelValue % 10);
        int priceLabelValue = countlabelValue * resource.defaultValue;

        if (countlabelValue > 0 & countlabelValue <= maxResourceAmount)
        {
            countlabel.text = countlabelValue.ToString();
            priceLabel.text = priceLabelValue.ToString();
            sellButton.SetEnabled(true);
        }
        else if (countlabelValue <= 0)
        {
            countlabel.text = 0.0f.ToString();
            priceLabel.text = 0.0f.ToString();
            sellButton.SetEnabled(false);
        }
        else if (countlabelValue > maxResourceAmount)
        {
            countlabel.text = maxResourceAmount.ToString();
            priceLabel.text = (maxResourceAmount * resource.defaultValue).ToString();
            sellButton.SetEnabled(maxResourceAmount != 0);
        }
    }

    private void MoveWindow(VisualElement root, Vector3 mousePos)
    {
        Vector2 pos = new(mousePos.x, Screen.height - mousePos.y);
        pos = RuntimePanelUtils.ScreenToPanel(root.panel, pos);
        pos = new(pos.x - localMousePosition.x, pos.y - localMousePosition.y);

        root.style.top = pos.y;
        root.style.left = pos.x;
    }
}
