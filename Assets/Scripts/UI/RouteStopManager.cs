using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RouteStopManager : MonoBehaviour
{
    private ShipViewer shipViewer;
    private SpaceShipHandler spaceShipHandler;

    private VisualElement root;

    private bool mouseOnMenu;
    private Vector2 localMousePosition;

    private Label usedCargoLabel;

    [SerializeField] private VisualTreeAsset pickupOptionTemplate;

    private List<ResourceAmount> pickupResourceAmounts;

    private void Update()
    {
        if (Input.GetMouseButton(0) & mouseOnMenu) MoveWindow(root, Input.mousePosition);
    }

    public void MakeRouteStopManager(ShipViewer shipViewer, SpaceShipHandler spaceShipHandler, bool isHome)
    {
        this.shipViewer = shipViewer;
        this.spaceShipHandler = spaceShipHandler;

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

        GetPickupOptions(isHome);

        root.Q<Label>("planet").text = "Pick up resources";
        usedCargoLabel = root.Q<Label>("usedcargo");
        UpdateUsedCargoLabel();

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
            UIController.RemoveLastFromUIStack();
        };

        Button setPickupsButton = root.Q<Button>("setpickupsbutton");
        setPickupsButton.clicked += () => HandleSetPickupsButtonClicked(isHome);

        UpdatePickupOptionList(root);
    }

    private void HandleSetPickupsButtonClicked(bool isHome)
    {
        pickupResourceAmounts.RemoveAll(ra => ra.amount == 0);

        if (isHome)
        {
            spaceShipHandler.route.RemoveHomePickupFactors();
            spaceShipHandler.route.AddHomePickupFactors(pickupResourceAmounts);
        }
        else
        {
            spaceShipHandler.route.RemoveDestinationPickupFactors();
            spaceShipHandler.route.AddDestinationPickupFactors(pickupResourceAmounts);
        }
        shipViewer.UpdateButtons();
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
        UIController.RemoveLastFromUIStack();
    }

    private void UpdateUsedCargoLabel()
    {
        usedCargoLabel.text = "Cargo " + GetUsedCargo().ToString() + "/" + spaceShipHandler.spaceShip.cargoCapacity;
    }

    private void UpdatePickupOptionList(VisualElement root)
    {
        VisualElement pickupList = root.Q<VisualElement>("pickuplist");
        foreach (ResourceAmount resourceAmount in pickupResourceAmounts)
        {
            VisualElement pickupOption = pickupOptionTemplate.Instantiate();

            Label resourceCountLabel = pickupOption.Q<Label>("resourcecount");
            resourceCountLabel.text = resourceAmount.amount.ToString();

            VisualElement resourceImage = pickupOption.Q<VisualElement>("resourceimage");
            resourceImage.style.backgroundImage = new StyleBackground(resourceAmount.resource.resourceSprite);
            resourceImage.style.unityBackgroundImageTintColor = new StyleColor(resourceAmount.resource.spriteColor);

            Button minusButton = pickupOption.Q<Button>("minusbutton");
            minusButton.clickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Shift });
            minusButton.clicked += () =>
            {
                if (resourceAmount.amount > 0)
                {
                    SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_MINUS);
                    resourceAmount.amount -= (Input.GetKey(KeyCode.LeftShift) ? (spaceShipHandler.spaceShip.isInterstellar ? 10 : 5) : 1);
                    if (resourceAmount.amount < 0) resourceAmount.amount = 0;
                    resourceCountLabel.text = resourceAmount.amount.ToString();
                }
                UpdateUsedCargoLabel();
            };

            Button plusButton = pickupOption.Q<Button>("plusbutton");
            plusButton.clickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Shift });
            plusButton.clicked += () =>
            {
                int cargoCapacity = spaceShipHandler.spaceShip.cargoCapacity;
                if (GetUsedCargo() < cargoCapacity)
                {
                    SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_PLUS);
                    resourceAmount.amount += (Input.GetKey(KeyCode.LeftShift) ? (spaceShipHandler.spaceShip.isInterstellar ? 10 : 5) : 1);
                    if (GetUsedCargo() > cargoCapacity) resourceAmount.amount -= (GetUsedCargo() - cargoCapacity);
                    resourceCountLabel.text = resourceAmount.amount.ToString();
                }
                UpdateUsedCargoLabel();
            }; 
            pickupList.Add(pickupOption);
        }
    }

    private void GetPickupOptions(bool isHome)
    {
        pickupResourceAmounts = new();
        Planet planet = isHome ? spaceShipHandler.route.home : spaceShipHandler.route.destination;
        foreach (Resource resource in UniverseHandler.instance.allResources)
        {
            ResourceAmount resourceAmount = new ResourceAmount(resource, 0);
            foreach (Tuple<bool, ResourceFactor> resourceFactor in (isHome ? spaceShipHandler.route.homeResourceFactors : spaceShipHandler.route.destinationResourceFactors))
            {
                if (resourceFactor.Item1 & resourceFactor.Item2.resourceAmount.resource.Equals(resource)) resourceAmount.amount = Math.Abs(resourceFactor.Item2.resourceAmount.amount);
            }
            if (resourceAmount.amount != 0 | planet.GetPlanetResourceHandler().GetResourceAmount(resource).amount > 0) pickupResourceAmounts.Add(resourceAmount);
        }
    }

    private int GetUsedCargo()
    {
        int usedCargo = 0;
        foreach (ResourceAmount resourceAmount in pickupResourceAmounts) usedCargo += resourceAmount.amount;
        return usedCargo;
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
