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

    private GameObject tradeMenu;
    private GameObject specialBuildingChooserMenu;
    private GameObject shipyardMenu;
    private GameObject activeBuildingChooserMenu;
    private GameObject activeBuildingViewerMenu;

    private UIController uiController;

    [NonSerialized] public PlanetValues.PlanetType type;

    public List<ProductionBuilding> possibleProductionBuildings;

    private BuildingSlot specialBuildingSlot;

    private PlanetResourceHandler planetResourceHandler;
    private List<ProductionBuildingHandler> productionBuildingHandlers = new();
    private List<Resource> tradeableResources = new();

    private Sprite settlementSprite;

    private PlayerInventory inventory;

    [SerializeField] private UniverseHandler universe;

    private SpecialBuilding specialBuilding;

    private bool managed;
    private bool reached = false;

    protected override void Awake()
    {
        base.Awake();
        GameEvents.OnCycleChange += () => UpdateResources(true);

        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        inventory = GameObject.Find("PlayerInventory").GetComponent<PlayerInventory>();
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
        planetResourceHandler = new(universe.allResources);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameEvents.OnCycleChange -= () => UpdateResources(true);
    }

    private void Update()
    {
        collider.radius = body.transform.localScale.x;

        //development
        if (Input.GetKeyDown(KeyCode.G))
        {
            foreach (Resource resource in universe.allResources) 
            {
                planetResourceHandler.AddResouce(resource, 1000);
            }
        }
        //
    }

    private void LateUpdate()
    {
        Vector3 lightDirection = Vector3.Normalize(parentStar.transform.position - transform.position);
        material.SetVector("_SunlightDirection", lightDirection);
        material.SetFloat("_TimeValue", universe.timeValue);

        nameTagCanvas.transform.LookAt(
            nameTagCanvas.transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up
            );
    }

    private void OnMouseDown()
    {
        if (universe.routeMakerDisplayed | !universe.UIDisplayed())
        {
            if (universe.routeMakerDisplayed) 
            {
                RouteMaker activeRouteMaker = universe.GetActiveRouteMaker();
                if (activeRouteMaker.CanAddStop(this))
                {
                    hoverOver.color = Color.green;
                    activeRouteMaker.AddStop(this);
                }
                else { hoverOver.color = Color.red; }
            }
            else if (selected)
            {
                if (reached) { StartCoroutine(ShowPlanetMenu(false)); }
            }
            else
            {
                SetSelected(true);
                StartCoroutine(ScaleOverTime(hoverOver.transform, Vector3.zero, 0.3f));
                cameraMovementHandler.MoveToTarget(body.transform, nativeScale, false);
                if (reached) { StartCoroutine(ShowPlanetMenu(true)); }
            }
        }
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
        //collider.radius = selected ? 0.5f : 2.0f;
        orbitLine.SetActive(!selected);

        if (selected)
        {
            universe.SetLastActivePlanetInactive();
            universe.SetActivePlanet(this);
            parentStar.ScaleToSize(scaleDownMultiplier, false);
            foreach (Planet planet in parentStar.planets) planet.ScaleToSize(planet.nativeScale * scaleDownMultiplier, false);
        }
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

    public bool GetReached() 
    {
        return reached;
    }

    public void SetReached(bool reached)
    {
        this.reached = reached;
    }

    public void SetActiveBuildingChooserMenu(GameObject buildingChooserMenu)
    {
        activeBuildingChooserMenu = buildingChooserMenu;
    }

    public void SetActiveBuildingViewerMenu(GameObject buildingViewerMenu)
    {
        activeBuildingViewerMenu = buildingViewerMenu;
    }

    public void SetSettlementSprite(Sprite settlementSprite)
    {
        this.settlementSprite = settlementSprite;
    }

    public void SetTradeMenu(GameObject tradeMenu)
    {
        this.tradeMenu = tradeMenu;
    }

    public void SetSpecialBuildingChooserMenu(GameObject specialBuildingChooserMenu)
    {
        this.specialBuildingChooserMenu = specialBuildingChooserMenu;
    }

    public void SetShipyardMenu(GameObject shipyardMenu)
    {
        this.shipyardMenu = shipyardMenu;
    }

    public void SetSpecialBuilding(SpecialBuilding specialBuilding)
    {
        this.specialBuilding = specialBuilding;
    }

    public bool GetManaged()
    {
        return productionBuildingHandlers.Count > 0;
    }

    public Sprite GetSettlementSprite()
    {
        return settlementSprite;
    }

    public List<DepositHandler> GetDeposits()
    {
        return deposits;
    }

    public BuildingSlot GetSpecialBuildingSlot()
    {
        return specialBuildingSlot;
    }

    public SpecialBuilding GetSpecialBuilding()
    {
        return specialBuilding;
    }

    public List<Resource> GetTradeableResources()
    {
        return tradeableResources;
    }

    public Orbiter GetOrbiter()
    {
        return GetComponent<Orbiter>();
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
                foreach (ProductionBuilding productionBuilding in depositHandler.GetPossibleProductionBuildings())
                {
                    Resource resource = productionBuilding.outputResource.resource;
                    if (!tradeableResources.Contains(resource)) tradeableResources.Add(resource);
                }
                deposits.Add(depositHandler);
            }
        }
    }

    public void MakePlanetMenu()
    {
        planetMenu = Instantiate(planetMenuPrefab);
        UIDocument planetMenuUI = planetMenu.GetComponent<UIDocument>();
        planetMenu.GetComponent<PlanetMenu>().MakePlanetMenu(this);
        uiController.AddToUIStack(new UIElement(planetMenu, planetMenuUI), false);
    }

    public PlanetResourceHandler GetPlanetResourceHandler()
    {
        return planetResourceHandler;
    }

    public void AddProductionBuildingHandler(ProductionBuildingHandler productionBuildingHandler)
    {
        productionBuildingHandlers.Add(productionBuildingHandler);
        UpdateResources(false);
    }

    public void RemoveProductionBuildingHandler(ProductionBuildingHandler productionBuildingHandler)
    {
        for (int i = 0; i < productionBuildingHandlers.Count(); i++) 
        {
            ProductionBuildingHandler pBH = productionBuildingHandlers.ElementAt(i);
            if (productionBuildingHandler.GetName() == pBH.GetName())
            {
                productionBuildingHandlers.RemoveAt(i);
                break;
            }
        }
    }

    private void UpdateResources(bool withUpkeep)
    {
        ActivateProductionBuildings();
        if (withUpkeep)
        {
            Upkeep();
            planetResourceHandler.UpdateResourceCounts();
        }
        UpdateResourceDisplays();
    }

    public void UpdateResourceDisplays()
    {
        if (planetMenu != null) planetMenu.GetComponent<PlanetMenu>().UpdateResourcePanel(this);
        if (tradeMenu != null) tradeMenu.GetComponent<TradeMenu>().UpdateResourcePanel(this);
        if (activeBuildingChooserMenu != null) activeBuildingChooserMenu.GetComponent<BuildingChooserMenu>().UpdateResourcePanel(this, activeBuildingChooserMenu.GetComponent<UIDocument>());
        if (activeBuildingViewerMenu != null) activeBuildingViewerMenu.GetComponent<BuildingViewerMenu>().UpdateResourcePanel(this, activeBuildingViewerMenu.GetComponent<UIDocument>());
        if (specialBuildingChooserMenu != null) specialBuildingChooserMenu.GetComponent<SpecialBuildingMenu>().UpdateResourcePanel(this);
        if (shipyardMenu != null) shipyardMenu.GetComponent<ShipyardMenu>().UpdateResourcePanel(this);
    }

    private void Upkeep()
    {
        foreach (ProductionBuildingHandler productionBuildingHandler in productionBuildingHandlers)
        {
            int sum = 0;
            if (productionBuildingHandler.IsActive())
            {
                sum += productionBuildingHandler.GetUpkeep();
            }
            inventory.RemoveMoney(sum);
        }
    }

    private void ActivateProductionBuildings()
    {
        foreach (ProductionBuildingHandler productionBuildingHandler in productionBuildingHandlers)
        {
            if (!productionBuildingHandler.IsActive() & 
                !productionBuildingHandler.IsManuallyDeActivated() & 
                productionBuildingHandler.CanBeActived())
            {
                productionBuildingHandler.SetActive(true);
            }
        }
    }

    IEnumerator ShowPlanetMenu(bool fromSystemView)
    {
        if (fromSystemView) yield return new WaitForSeconds(0.3f);
        MakePlanetMenu();
    }

    public bool CanBuild(ResourceAmount[] resourceAmounts)
    {
        foreach (ResourceAmount resourceNeeded in resourceAmounts)
        {
            if (resourceNeeded.resource == inventory.GetMoneyResource())
            {
                if (inventory.GetMoney() < resourceNeeded.amount) return false;
            }
            else
            {
                ResourceCount resourceCount = planetResourceHandler.GetResourceCount(resourceNeeded.resource);
                if (resourceCount == null) return false;
                if (resourceCount.amount < resourceNeeded.amount) return false;
            }
        }
        return true;
    }
}
