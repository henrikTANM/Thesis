using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipViewer : MonoBehaviour
{
    private SpaceShipHandler spaceShipHandler;

    [SerializeField] private VisualTreeAsset noDestinationTemplate;
    [SerializeField] private VisualTreeAsset routeStopInfoTemplate;
    [SerializeField] private VisualTreeAsset pickupPerCycleTemplate;

    [SerializeField] private GameObject destinationPickerPrefab;
    [SerializeField] private GameObject routeStopManagerPrefab;

    [SerializeField] private Sprite editNameSprite;
    [SerializeField] private Sprite confirmEditSprite;
    [SerializeField] private Color editCursorColor;
    private float cursorTimer = 0.0f;

    private VisualElement root;

    private bool mouseOnMenu;
    private Vector2 localMousePosition;

    private Label nameLabel;
    private Label fuelConsumptionLabel;

    private TextField editTextField;
    private Button editNameButton;
    private Button routeStateButton;
    private Button clearRouteButton;
    private Button homeButton;
    private Button destinationButton;

    private void Update()
    {
        if (UniverseHandler.instance.editActive & Input.GetKeyDown(KeyCode.Return)) HandleEditName();
        if (UniverseHandler.instance.editActive & Input.GetKeyDown(KeyCode.Escape)) CloseEdit();
        if (Input.GetMouseButton(0) & mouseOnMenu & !UniverseHandler.instance.editActive) MoveWindow(root, Input.mousePosition);

        if (editTextField != null)
        {
            cursorTimer += Time.deltaTime;
            if (cursorTimer >= 1.0f)
            {
                editTextField.textSelection.cursorColor = Color.black;
                cursorTimer = 0.0f;
            }
            else if (cursorTimer >= 0.5f)
            {
                editTextField.textSelection.cursorColor = editCursorColor;
            }
        }
    }

    private void OnDestroy() { CloseEdit(); }

    public void MakeShipViewer(SpaceShipHandler spaceShipHandler)
    {
        this.spaceShipHandler = spaceShipHandler;
        spaceShipHandler.shipViewer = this;

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

        nameLabel = root.Q<Label>("name");
        UpdateNameLabel();


        root.Q<Label>("interstellar").text = "Interstellar drive: " + (spaceShipHandler.spaceShip.isInterstellar ? "YES" : "NO");
        root.Q<Label>("cargo").text = "Cargo capacity: " + spaceShipHandler.spaceShip.cargoCapacity;

        fuelConsumptionLabel = root.Q<Label>("fuel");
        UpdateFuelConsumptionLabel();

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
            UIController.RemoveLastFromUIStack();
        };

        Button sellButton = root.Q<Button>("sellbutton");
        sellButton.text = "Sell for " + PlayerInventory.GetShipCost(spaceShipHandler).ToString();
        sellButton.clicked += HandleSellButtonClicked;

        editTextField = root.Q<TextField>("edittextfield");
        editTextField.value = spaceShipHandler.name;
        editTextField.maxLength = 20;

        editNameButton = root.Q<Button>("editname");
        editNameButton.clicked += HandleEditName;

        routeStateButton = root.Q<Button>("routestatebutton");
        routeStateButton.clicked += HandleRouteStateButtonClicked;

        clearRouteButton = root.Q<Button>("clearroutebutton");
        clearRouteButton.clicked += HandleClearRouteButtonClicked;

        homeButton = root.Q<Button>("homebutton");
        homeButton.clicked += HandleHomeButtonClicked;

        destinationButton = root.Q<Button>("destinationbutton");
        destinationButton.clicked += HandleDestinationButtonClicked;

        UpdateEditName();
        UpdateButtons();

        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_OPEN);
    }

    private void HandleSellButtonClicked()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
        PlayerInventory.RemoveShip(spaceShipHandler);
        if (spaceShipHandler.HasRoute()) spaceShipHandler.route.RemoveAllResourceFactors();
        Destroy(spaceShipHandler.gameObject);
        UIController.UpdateShipsList();
        UIController.RemoveLastFromUIStack();
    }
    private void HandleEditName()
    {
        if (UniverseHandler.instance.editActive) 
        { 
            if (editTextField.value.Length > 0) spaceShipHandler.name = editTextField.value;
            UIController.UpdateShipsList();
            UpdateNameLabel();
        }
        UniverseHandler.instance.editActive = !UniverseHandler.instance.editActive;
        UpdateEditName();
    }

    private void HandleRouteStateButtonClicked()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
        spaceShipHandler.route.active = !spaceShipHandler.route.active;
        spaceShipHandler.route.UpdateResourceDisplays();
        UpdateButtons();
        UpdateFuelConsumptionLabel();
        UIController.UpdateShipsList();
    }

    private void HandleClearRouteButtonClicked()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
        spaceShipHandler.removeRoute = true;
        spaceShipHandler.route.active = false;
        spaceShipHandler.route.UpdateResourceDisplays();
        if (spaceShipHandler.state.Equals(SpaceShipHandler.SpaceShipState.AT_HOME)) spaceShipHandler.RemoveRoute();
        UpdateButtons();
        UpdateFuelConsumptionLabel();
        UIController.UpdateShipsList();
    }

    private void HandleHomeButtonClicked()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ENTER);
        if (spaceShipHandler.HasRoute()) MakeRouteStopManager(true);
        if (UniverseHandler.instance.editActive) CloseEdit();
    }

    private void HandleDestinationButtonClicked()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ENTER);
        if (spaceShipHandler.HasRoute()) MakeRouteStopManager(false);
        else MakeDestinationPicker();
        if (UniverseHandler.instance.editActive) CloseEdit();
    }

    private void UpdateNameLabel() { nameLabel.text = spaceShipHandler.name; }

    private void UpdateFuelConsumptionLabel()
    {
        fuelConsumptionLabel.text = "Fuel consumption: " + (spaceShipHandler.HasRoute() ?
            (spaceShipHandler.route.active ? spaceShipHandler.spaceShip.fuelConsumption.amount : 0) : 0).ToString() + "/cycle";
    }

    private void UpdateEditName()
    {
        editTextField.SetEnabled(UniverseHandler.instance.editActive);
        editTextField.style.visibility = UniverseHandler.instance.editActive ? Visibility.Visible : Visibility.Hidden;
        editNameButton.style.backgroundImage = new StyleBackground(UniverseHandler.instance.editActive ? confirmEditSprite : editNameSprite);
    }

    public void UpdateButtons()
    {
        UpdateHomeButton(spaceShipHandler.HasRoute() & !spaceShipHandler.removeRoute ? spaceShipHandler.route.homeResourceFactors : new());
        UpdateDestinationButton(spaceShipHandler.HasRoute() & !spaceShipHandler.removeRoute ? spaceShipHandler.route.destinationResourceFactors : new());
        UpdateRouteStateButton();
        UpdateClearRouteButton();
    }

    private void UpdateRouteStateButton()
    {
        if (spaceShipHandler.HasRoute())
        {
            if (spaceShipHandler.route.active)
            {
                routeStateButton.SetEnabled(true);
                routeStateButton.text = "Pause route";
            }
            else if (spaceShipHandler.state.Equals(SpaceShipHandler.SpaceShipState.AT_HOME))
            {
                routeStateButton.SetEnabled(true);
                routeStateButton.text = "Start route";
            }
            else
            {
                routeStateButton.SetEnabled(false);
                routeStateButton.text = "Moving to home planet";
            }
        }
        else
        {
            routeStateButton.SetEnabled(false);
            routeStateButton.text = "No route";
        }
    }

    private void UpdateClearRouteButton()
    {
        clearRouteButton.SetEnabled(spaceShipHandler.HasRoute() & !spaceShipHandler.removeRoute);
    }

    private void UpdateHomeButton(List<Tuple<bool, ResourceFactor>> pickupList)
    {
        homeButton.Clear();
        VisualElement routeInfo = InstantiateRouteStopInfo(pickupList, spaceShipHandler.home);
        homeButton.Add(routeInfo);

        if (spaceShipHandler.HasRoute()) homeButton.SetEnabled(spaceShipHandler.state.Equals(SpaceShipHandler.SpaceShipState.AT_HOME) & !spaceShipHandler.route.active);
        else homeButton.SetEnabled(false);
    }

    private void UpdateDestinationButton(List<Tuple<bool, ResourceFactor>> pickupList)
    {
        destinationButton.Clear();
        if (spaceShipHandler.HasRoute() & !spaceShipHandler.removeRoute)
        {
            VisualElement routeInfo = InstantiateRouteStopInfo(pickupList, spaceShipHandler.route.destination);
            destinationButton.Add(routeInfo);
            destinationButton.SetEnabled(spaceShipHandler.state.Equals(SpaceShipHandler.SpaceShipState.AT_HOME) & !spaceShipHandler.route.active);
        }
        else
        {
            VisualElement noDestination = noDestinationTemplate.Instantiate();
            destinationButton.Add(noDestination);
            destinationButton.SetEnabled(true);
        }
    }

    private void CloseEdit()
    {
        UniverseHandler.instance.editActive = false;
        UpdateEditName();
    }

    private VisualElement InstantiateRouteStopInfo(List<Tuple<bool, ResourceFactor>> pickupList, Planet planet)
    {
        VisualElement routeInfo = routeStopInfoTemplate.Instantiate();
        routeInfo.Q<Label>("planet").text = planet.name;
        VisualElement pickUpListElement = routeInfo.Q<VisualElement>("pickuplist");
        pickUpListElement.Clear();
        foreach (Tuple<bool, ResourceFactor> pickup in pickupList)
        {
            if (pickup.Item1)
            {
                VisualElement pickUpPerCycle = pickupPerCycleTemplate.Instantiate();
                pickUpPerCycle.Q<Label>("percycle").text = Math.Abs(pickup.Item2.resourceAmount.amount).ToString() + "/cycle";
                VisualElement resourceImage = pickUpPerCycle.Q<VisualElement>("resourceimage");
                resourceImage.style.backgroundImage = new StyleBackground(pickup.Item2.resourceAmount.resource.resourceSprite);
                resourceImage.style.unityBackgroundImageTintColor = new StyleColor(pickup.Item2.resourceAmount.resource.spriteColor);
                pickUpListElement.Add(pickUpPerCycle);
            }
        }
        return routeInfo;
    }

    private void MakeDestinationPicker()
    {
        GameObject destinationPicker = Instantiate(destinationPickerPrefab);
        UIDocument destinationPickerUI = destinationPicker.GetComponent<UIDocument>();
        destinationPicker.GetComponent<DestinationPicker>().MakeDestinationPicker(this, spaceShipHandler);
        UIController.AddToUIStack(new UIElement(destinationPicker, destinationPickerUI), false);
    }

    private void MakeRouteStopManager(bool isHome)
    {
        GameObject routeStopManager = Instantiate(routeStopManagerPrefab);
        UIDocument routeStopManagerUI = routeStopManager.GetComponent<UIDocument>();
        routeStopManager.GetComponent<RouteStopManager>().MakeRouteStopManager(this, spaceShipHandler, isHome);
        UIController.AddToUIStack(new UIElement(routeStopManager, routeStopManagerUI), false);
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
