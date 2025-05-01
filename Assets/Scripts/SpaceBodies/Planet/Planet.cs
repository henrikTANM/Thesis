using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Planet : SpaceBody
{
    [NonSerialized] public Material material;
    [NonSerialized] public Star parentStar;

    private GameObject orbitLine;

    [SerializeField] private GameObject depositPrefab;
    [SerializeField] private Deposit factoryDeposit;
    private List<DepositHandler> deposits = new();

    [SerializeField] private GameObject planetMenuPrefab;
    private GameObject planetMenu;
    public PlanetMenu planetMenuUI;

    [SerializeField] private GameObject errorMessagePrefab;

    private GameObject buildingChooserMenu;
    private GameObject buildingViewerMenu;
    private GameObject specialBuildingChooserMenu;
    private GameObject shipyardMenu;
    private GameObject tradeHubMenu;
    private GameObject discoveryHubMenu;
    private GameObject bhcfMenu;

    [NonSerialized] public PlanetValues.PlanetType type;

    public List<ProductionBuilding> possibleProductionBuildings;

    private BuildingSlot specialBuildingSlot;

    private PlanetResourceHandler planetResourceHandler;
    private List<ProductionBuildingHandler> productionBuildingHandlers = new();

    [NonSerialized] public Sprite specialBuildingSlotBackground;

    [NonSerialized] public SpecialBuilding specialBuilding;
    private DiscoveryHubHandler discoveryHubHandler;
    private BHCFHandler bhcfHandler;

    [NonSerialized] public bool reached;
    [NonSerialized] public bool managed;

    private List<SpaceShipHandler> shipsInOrbit = new();

    public List<Resource> sellableResources;
    public List<Resource> buyableResources;

    [SerializeField] private VisualTreeAsset resourceTemplate;
    private List<VisualElement> resourceContainers = new();

    [SerializeField] private GameObject resourceViewerPrefab;
    private GameObject resourceViewer;

    [NonSerialized] public PlanetValues planetValues;

    public float scaleDownMultiplier = 0.1f;

    protected override void Awake()
    {
        base.Awake();
        GameEvents.OnCycleChange += UpdateResources;
        GameEvents.OnCycleChange += HandleDiscovery;
        GameEvents.OnCycleChange += HandleBHContainment;

        planetResourceHandler = new(UniverseHandler.instance.allResources, this);
    }

    private void OnDestroy()
    {
        GameEvents.OnCycleChange -= UpdateResources;
        GameEvents.OnCycleChange -= HandleDiscovery;
        GameEvents.OnCycleChange -= HandleBHContainment;
    }

    private void Update()
    {
        collider.radius = body.transform.localScale.x * 0.75f;

        if (UniverseHandler.developmentMode)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                foreach (Resource resource in UniverseHandler.instance.allResources)
                {
                    planetResourceHandler.ChangeResourceAmount(new ResourceAmount(resource, 1000));
                }
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                foreach (Resource resource in UniverseHandler.instance.allResources)
                {
                    planetResourceHandler.ChangeResourceAmount(new ResourceAmount(resource, -1000));
                }
            }
            if (Input.GetKeyDown(KeyCode.V)) parentStar.discoveryProgress = 100;
        }
    }

    private void LateUpdate()
    {
        Vector3 lightDirection = Vector3.Normalize(parentStar.transform.position - transform.position);
        material.SetVector("_SunlightDirection", lightDirection);
        material.SetFloat("_TimeValue", UniverseHandler.timeValue);

        nameTagCanvas.transform.LookAt(
            nameTagCanvas.transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up
            );
    }

    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        if (UniverseHandler.destinationPickerDisplayed)
        {
            DestinationPicker activeRouteMaker = UniverseHandler.activeDestinationPicker;
            if (activeRouteMaker.CanSetDestination(this).Item1)
            {
                hoverOver.color = Color.green;
            }
            else { hoverOver.color = Color.red; }
        }
    }

    protected override void OnMouseExit()
    {   
        Color lastColor = hoverOver.color;
        base.OnMouseExit();
        hoverOver.color = lastColor;
    }

    private void OnMouseDown()
    {
        if (UniverseHandler.destinationPickerDisplayed | !UIController.UIDisplayed())
        {
            if (UniverseHandler.destinationPickerDisplayed) 
            {
                DestinationPicker activeDestinationPicker = UniverseHandler.activeDestinationPicker;
                Tuple<bool, string> canAddStop = activeDestinationPicker.CanSetDestination(this);
                if (canAddStop.Item1)
                {
                    SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
                    activeDestinationPicker.SetDestination(this);
                    hoverOver.color = Color.red;
                }
                else
                {
                    SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
                    MakeErrorMessage(canAddStop.Item2);
                }
            }
            else if (selected) ShowPlanetMenu(true);
            else if (!selected)
            {
                UniverseHandler.AddMoveToPlanet(this);
                if (UniverseHandler.instance.selectedPlanet != null) StartCoroutine(ScaleOverTime(hoverOver.transform, Vector3.zero, 0.1f));
            }
        }
    }

    public override bool IsStar() { return false; }
    public override bool IsPlanet() { return true; }

    public Vector3 GetSpaceShipPosition()
    {
        return transform.position + (body.transform.localScale.x * Vector3.up);
    }

    private void MakeErrorMessage(string error)
    {
        GameObject errorMessage = Instantiate(errorMessagePrefab);
        UIDocument errorMessageUI = errorMessage.GetComponent<UIDocument>();
        errorMessageUI.rootVisualElement.Q<Label>("error").text = error;
        StartCoroutine(WaitForSeconds(errorMessage));
    }

    public void SetSelected(bool selected, UniverseHandler.StateChange stateChange)
    {
        this.selected = selected;

        if (selected)
        {
            if (stateChange.Equals(UniverseHandler.StateChange.STAR_TO_PLANET) | 
                stateChange.Equals(UniverseHandler.StateChange.UNIVERSE_TO_PLANET))
            {
                UniverseHandler.instance.selectedPlanet = this;
                ScaleToSize(nativeScale * scaleDownMultiplier, false);
                orbitLine.SetActive(false);
                nameTagCanvas.enabled = false;
                foreach (Planet planet in parentStar.planets) if (planet != this) planet.SetSelected(false, stateChange);
                parentStar.SetSelected(true, stateChange);
            }

            if (stateChange.Equals(UniverseHandler.StateChange.STAR_TO_OTHER_PLANET) |
                stateChange.Equals(UniverseHandler.StateChange.PLANET_TO_OTHER_PLANET)) {
                UniverseHandler.instance.selectedPlanet = this;
                ScaleToSize(nativeScale * scaleDownMultiplier, false);
                orbitLine.SetActive(false);
                nameTagCanvas.enabled = false;
                foreach (Planet planet in parentStar.planets) if (planet != this) planet.SetSelected(false, stateChange);
                UniverseHandler.instance.selectedStar.SetSelected(false, stateChange);
                parentStar.SetSelected(true, stateChange);
            }

            if (stateChange.Equals(UniverseHandler.StateChange.PLANET_TO_PLANET))
            {
                UniverseHandler.instance.selectedPlanet = this;
                foreach (Planet planet in parentStar.planets) if (planet != this) planet.SetSelected(false, stateChange);
            }
        }
        else
        {
            if (stateChange.Equals(UniverseHandler.StateChange.STAR_TO_PLANET) |
                stateChange.Equals(UniverseHandler.StateChange.UNIVERSE_TO_PLANET) |
                stateChange.Equals(UniverseHandler.StateChange.STAR_TO_OTHER_PLANET) |
                stateChange.Equals(UniverseHandler.StateChange.PLANET_TO_OTHER_PLANET))
            {
                ScaleToSize(nativeScale * scaleDownMultiplier, false);
                orbitLine.SetActive(false);
                nameTagCanvas.enabled = false;
            }


            if (stateChange.Equals(UniverseHandler.StateChange.PLANET_TO_STAR) |
                stateChange.Equals(UniverseHandler.StateChange.PLANET_TO_OTHER_STAR) |
                stateChange.Equals(UniverseHandler.StateChange.PLANET_TO_UNIVERSE))
            {
                ScaleToSize(nativeScale, false);
                orbitLine.SetActive(true);
                nameTagCanvas.enabled = true;
                UniverseHandler.instance.selectedPlanet = null;
            }
        }

        GenerateDisplacedShipPositions();
    }

    public List<SpecialBuilding> GetPossibleSpecialBuildings()
    {
        List<SpecialBuilding> possibleSpecialBuildings = new(planetValues.specialBuildings);
        foreach (Planet planet in parentStar.planets)
        {
            if (planet.specialBuilding != null)
            {
                foreach (SpecialBuilding specialBuilding in possibleSpecialBuildings)
                {
                    if (specialBuilding.Equals(planet.specialBuilding))
                    {
                        possibleSpecialBuildings.Remove(specialBuilding);
                        break;
                    }
                }
            }
        }
        return possibleSpecialBuildings;
    }

    public void SetVisible(bool visible)
    {
        GetComponent<SphereCollider>().enabled = visible;
        body.GetComponent<MeshRenderer>().enabled = visible;
        nameTagCanvas.enabled = visible;
    }

    public void DrawOrbit(int steps, float radius, GameObject orbitLine)
    {
        this.orbitLine = orbitLine;
        LineRenderer orbitRenderer = orbitLine.GetComponent<LineRenderer>();
        orbitRenderer.transform.position = parentStar.transform.position;
        orbitRenderer.positionCount = steps;

        for (int currentStep = 0; currentStep < steps; currentStep++)
        {
            float circumferenceProgress = (float)currentStep / (steps - 1);

            float currentRadian = circumferenceProgress * 2 * Mathf.PI;

            float xScaled = Mathf.Cos(currentRadian);
            float yScaled = Mathf.Sin(currentRadian);

            float x = radius * xScaled;
            float y = 0.0f;
            float z = radius * yScaled;

            Vector3 currentPosition = new(x, y, z);

            orbitRenderer.SetPosition(currentStep, currentPosition);
        }
    }

    public void SetBuildingChooserMenu(GameObject buildingChooserMenu) { this.buildingChooserMenu = buildingChooserMenu; }

    public void SetBuildingViewerMenu(GameObject buildingViewerMenu) { this.buildingViewerMenu = buildingViewerMenu; }

    public void SetSpecialBuildingChooserMenu(GameObject specialBuildingChooserMenu)
    {
        this.specialBuildingChooserMenu = specialBuildingChooserMenu;
    }

    public void SetShipyardMenu(GameObject shipyardMenu) { this.shipyardMenu = shipyardMenu; }
    public void SetTradeHubMenu(GameObject tradeHubMenu) { this.tradeHubMenu = tradeHubMenu; }
    public void SetDiscoveryHubMenu(GameObject discoveryHubMenu) { this.discoveryHubMenu = discoveryHubMenu; }
    public void SetBhcfMenu(GameObject bhcfMenu) { this.bhcfMenu = bhcfMenu; }

    public void SetSpecialBuilding(SpecialBuilding specialBuilding)
    {
        this.specialBuilding = specialBuilding;
        planetMenu.GetComponent<PlanetMenu>()
            .ChangeSpecialBuildingButtonImage(specialBuilding == null ? null : specialBuilding.buildingSprite);
        UIController.UpdatePlanetsList();
        managed = productionBuildingHandlers.Count > 0 | specialBuilding != null;
    }

    public void SetDiscoveryHubHandler(DiscoveryHubHandler discoveryHubHandler) 
    { 
        this.discoveryHubHandler = discoveryHubHandler; 
    }

    public void SetBHCFHandler(BHCFHandler bhcfHandler)
    {
        this.bhcfHandler = bhcfHandler;
    }

    public Sprite GetSettlementSprite()
    {
        return specialBuildingSlotBackground;
    }

    public List<DepositHandler> GetDeposits()
    {
        return deposits;
    }

    public DiscoveryHubHandler GetDiscoveryHubHandler() { return discoveryHubHandler; }
    public BHCFHandler GetBHCFHandler() { return bhcfHandler; }

    public Orbiter GetOrbiter()
    {
        return GetComponent<Orbiter>();
    }

    public void AddShipToOrbit(SpaceShipHandler ship)
    {
        if (!reached)
        {
            reached = true;
            UIController.AddMessage(new Message(
                "Outpost established on " + name + ".",
                Message.MessageType.NOTIFICATION,
                new MessageSender<Planet>(this),
                Message.SenderType.PLANET
                ));
        }
        shipsInOrbit.Add(ship);
        GenerateDisplacedShipPositions();
    }

    public void RemoveShipFromOrbit(SpaceShipHandler ship)
    {
        shipsInOrbit.Remove(ship);
        GenerateDisplacedShipPositions();
    }

    public void GenerateDisplacedShipPositions()
    {
        for (int i = 1; i <= shipsInOrbit.Count; i++)
        {
            shipsInOrbit.ElementAt(i - 1).posInOrbit = (i % 2 == 0 ? 
                Vector3.left * (body.transform.localScale.x / 6.0f) * (float)(i - 1) : 
                Vector3.right * (body.transform.localScale.x / 6.0f) * (float)i);
        }
    }

    public void GenerateDeposits(List<Deposit> possibleDepositList, int depositCap)
    {
        for (int i = 0; i < 3; i++)
        {
            if (type == PlanetValues.PlanetType.Gas && i >= depositCap)
            {
                deposits.Add(null);
            }
            else
            {
                GameObject newDeposit = Instantiate(depositPrefab);
                DepositHandler depositHandler = newDeposit.GetComponent<DepositHandler>();
                Deposit randomDeposit = possibleDepositList.ElementAt(UnityEngine.Random.Range(0, possibleDepositList.Count));
                depositHandler.Make(i < depositCap ? randomDeposit : factoryDeposit, this);
                deposits.Add(depositHandler);
            }
        }
    }

    public void GenerateStartDeposits(List<Deposit> startDepositValues)
    {
        foreach (Deposit deposit in startDepositValues)
        {
            if (deposit != null)
            {
                GameObject newDeposit = Instantiate(depositPrefab);
                DepositHandler depositHandler = newDeposit.GetComponent<DepositHandler>();
                depositHandler.Make(deposit, this);
                deposits.Add(depositHandler);
            } 
            else
            {
                deposits.Add(null);
            }
        }
    }

    public void MakePlanetMenu()
    {
        planetMenu = Instantiate(planetMenuPrefab);
        UIDocument planetMenuUI = planetMenu.GetComponent<UIDocument>();
        planetMenu.GetComponent<PlanetMenu>().MakePlanetMenu(this);
        UIController.AddToUIStack(new UIElement(planetMenu, planetMenuUI), false);
    }

    public PlanetResourceHandler GetPlanetResourceHandler()
    {
        return planetResourceHandler;
    }

    public void AddProductionBuildingHandler(ProductionBuildingHandler productionBuildingHandler) 
    { 
        productionBuildingHandlers.Add(productionBuildingHandler);
        managed = productionBuildingHandlers.Count > 0 | specialBuilding != null;
        UIController.UpdatePlanetsList();
    }
    public void RemoveProductionBuildingHandler(ProductionBuildingHandler productionBuildingHandler) 
    { 
        productionBuildingHandlers.Remove(productionBuildingHandler);
        managed = productionBuildingHandlers.Count > 0 | specialBuilding != null;
        UIController.UpdatePlanetsList();
    }

    private void UpdateResources()
    {
        planetResourceHandler.UpdateResourceAmounts();
        planetResourceHandler.UpdateResourcePerCycles();
        UpdateResourceDisplays();
    }

    public void UpdateResourceDisplays()
    {
        if (planetMenu != null | buildingChooserMenu != null | buildingViewerMenu != null | specialBuildingChooserMenu != null | 
            shipyardMenu != null | tradeHubMenu != null | discoveryHubMenu != null | bhcfMenu != null) UpdateResourcesPanel();

        if (planetMenu != null) planetMenu.GetComponent<PlanetMenu>().UpdateResourcePanel(resourceContainers);
        if (buildingChooserMenu != null) buildingChooserMenu.GetComponent<BuildingChooserMenu>().UpdateResourcePanel(resourceContainers);
        if (buildingViewerMenu != null) buildingViewerMenu.GetComponent<BuildingViewerMenu>().UpdateResourcePanel(resourceContainers);
        if (specialBuildingChooserMenu != null) specialBuildingChooserMenu.GetComponent<SpecialBuildingChooserMenu>().UpdateResourcePanel(resourceContainers);
        if (shipyardMenu != null) shipyardMenu.GetComponent<ShipyardMenu>().UpdateResourcePanel(resourceContainers);
        if (tradeHubMenu != null) tradeHubMenu.GetComponent<TradeHubMenu>().UpdateResourcePanel(resourceContainers);
        if (discoveryHubMenu != null) discoveryHubMenu.GetComponent<DiscoveryHubMenu>().UpdateResourcePanel(resourceContainers);
        if (bhcfMenu != null) bhcfMenu.GetComponent<BHCFMenu>().UpdateResourcePanel(resourceContainers);
    }

    public void UpdateResourcesPanel()
    {
        foreach (ResourceCounter resourceCounter in planetResourceHandler.GetResourceCounters())
        {
            VisualElement resourceContainer = resourceContainers.Find((VisualElement rc) => { return rc.name == resourceCounter.resourceAmount.resource.name; });
            if (resourceContainer == null)
            {
                resourceContainer = resourceTemplate.Instantiate();
                resourceContainer.RegisterCallback<MouseEnterEvent>(evt => { MakeResourceViewer(evt, resourceCounter.resourceAmount.resource); });
                resourceContainer.RegisterCallback<MouseLeaveEvent>(evt => UIController.RemoveLastFromUIStack());
                resourceContainer.name = resourceCounter.resourceAmount.resource.name;
                VisualElement resourceImage = resourceContainer.Q<VisualElement>("resourceimage");
                resourceImage.style.backgroundImage =
                    new StyleBackground(resourceCounter.resourceAmount.resource.resourceSprite);
                resourceImage.style.unityBackgroundImageTintColor =
                    new StyleColor(resourceCounter.resourceAmount.resource.spriteColor);
                resourceContainer.style.alignSelf = Align.Center;
                resourceContainers.Add(resourceContainer);
            }
            resourceContainer.Q<Label>("resourcecount").text
                = resourceCounter.resourceAmount.amount.ToString() + (resourceCounter.change < 0 ? "" : "+") + resourceCounter.change.ToString();
        }
    }

    private void MakeResourceViewer(MouseEnterEvent evt, Resource resource)
    {
        Vector2 mousePos = evt.mousePosition;
        resourceViewer = Instantiate(resourceViewerPrefab);
        UIDocument resourceViewerUI = resourceViewer.GetComponent<UIDocument>();
        resourceViewer.GetComponent<ResourceViewer>().MakeResourceViewer(resource, this, mousePos);
        UIController.AddToUIStack(new UIElement(resourceViewer, resourceViewerUI), true);
        resourceViewer.GetComponent<UIDocument>().sortingOrder = 2;
    }

    private void HandleDiscovery()
    {
        if (discoveryHubHandler != null)
        {
            if (discoveryHubHandler.active) parentStar.HandleDiscovery(discoveryHubHandler);
            if (discoveryHubMenu != null) discoveryHubMenu.GetComponent<DiscoveryHubMenu>().UpdateDiscoveryInfo(discoveryHubHandler.active);
        }
    }

    private void HandleBHContainment()
    {
        if (bhcfHandler != null)
        {
            if (bhcfHandler.active & bhcfHandler.progress < 100) bhcfHandler.progress += bhcfHandler.progressRate;
            if (bhcfMenu != null) bhcfMenu.GetComponent<BHCFMenu>().UpdateBHCFInfo(bhcfHandler.active);
        }
    }

    public void ShowPlanetMenu(bool planetSelected) { StartCoroutine(ShowPlanetMenuDelay(planetSelected)); }
    public IEnumerator ShowPlanetMenuDelay(bool planetSelected)
    {
        yield return new WaitForSeconds(planetSelected ? 0.1f : 0.5f);
        UIController.ClearUIStack();
        MakePlanetMenu();
    }

    public void ShowProductionBuildingViewer(bool planetSelected, BuildingSlot buildingSlot) 
    { 
        StartCoroutine(ShowProductionBuildingViewerDelay(planetSelected, buildingSlot)); 
    }
    public IEnumerator ShowProductionBuildingViewerDelay(bool planetSelected, BuildingSlot buildingSlot)
    {
        if (!planetSelected) yield return new WaitForSeconds(0.5f);
        UIController.ClearUIStack();
        buildingSlot.HandleBuildingSlotClicked();
    }

    public void ShowSpecialBuildingViewer(bool planetSelected)
    {
        StartCoroutine(ShowSpecialBuildingViewerDelay(planetSelected));
    }
    public IEnumerator ShowSpecialBuildingViewerDelay(bool planetSelected)
    {
        if (!planetSelected) yield return new WaitForSeconds(0.5f);
        UIController.ClearUIStack();
        planetMenu.GetComponent<PlanetMenu>().HandleSpecialBuildingButton();
    }

    IEnumerator WaitForSeconds(GameObject error)
    {
        yield return new WaitForSeconds(2.0f);
        Destroy(error);
    }
}
