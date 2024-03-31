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
    [NonSerialized] public float orbitSpeed;
    [NonSerialized] public Vector3 orbitAxis;
    [NonSerialized] public Material material;
    [NonSerialized] public Star parentStar;

    private GameObject orbitLine;

    [SerializeField] private GameObject depositPrefab;
    [SerializeField] private Deposit factoryDeposit;
    private List<DepositHandler> deposits = new();

    [SerializeField] private GameObject planetMenuPrefab;
    private GameObject planetMenu;
    private UIDocument planetMenuUI;
    [SerializeField] private GameObject tradeMenuPrefab;
    private GameObject tradeMenu;
    private UIDocument tradeMenuUI;

    private GameObject activeBuildingChooserMenu;
    private GameObject activeBuildingViewerMenu;

    private UIController uiController;

    [NonSerialized] public PlanetValues.PlanetType type;

    public List<ProductionBuilding> possibleProductionBuildings;

    private BuildingSlot specialBuildingSlot;

    private PlanetResourceHandler planetResourceHandler = new();
    private List<ProductionBuildingHandler> productionBuildingHandlers = new();
    private List<Resource> tradeableResources = new();

    private Sprite settlementSprite;

    private PlayerInventory inventory;

    protected override void Awake()
    {
        base.Awake();
        ResourceEvents.OnCycleChange += () => UpdateResources(true);

        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        inventory = GameObject.Find("PlayerInventory").GetComponent<PlayerInventory>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ResourceEvents.OnCycleChange -= () => UpdateResources(true);
    }

    void Update()
    {

        nameTagCanvas.transform.LookAt(cameraMovementHandler.transform);
        nameTagCanvas.transform.Rotate(new(0.0f, 180.0f, 0.0f));

        if (universe.timeRunning) transform.RotateAround(parentStar.transform.position, orbitAxis, orbitSpeed * Time.deltaTime);

        Vector3 lightDirection = Vector3.Normalize(parentStar.transform.position - transform.position);
        material.SetVector("_SunlightDirection", lightDirection);

        material.SetFloat("_TimeValue", universe.timeValue);
    }

    private void OnMouseDown()
    {
        if (!universe.UIMenuDisplayed())
        {
            if (!selected)
            {
                SetSelected(true);
                StartCoroutine(ScaleOverTime(hoverOver.transform, Vector3.zero, 0.3f));
                cameraMovementHandler.MoveToTarget(transform, transform.localScale.x * 10.0f * scaleDownMultiplier, false);
            }
            if (selected)
            {
                uiController.SetCurrentUI(planetMenuUI);
            }
        }
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
        collider.radius = selected ? 0.5f : 2.0f;
        orbitLine.SetActive(!selected);

        if (selected)
        {
            universe.SetLastActivePlanetInactive();
            universe.SetActivePlanet(this);
            parentStar.SetStarBodyScale(scaleDownMultiplier);
            foreach (Planet planet in parentStar.planets) planet.ScaleToSize(planet.nativeScale * scaleDownMultiplier);
        }
    }

    public void SetVisible(bool visible)
    {
        GetComponent<SphereCollider>().enabled = visible;
        GetComponent<MeshRenderer>().enabled = visible;
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

    public Sprite GetSettlementSprite()
    {
        return settlementSprite;
    }

    public List<DepositHandler> GetDeposits()
    {
        return deposits;
    }

    public UIDocument GetPlanetMenuUI()
    {
        return planetMenuUI;
    }

    public UIDocument GetTradeMenuUI()
    {
        return tradeMenuUI;
    }

    public BuildingSlot GetSpecialBuildingSlot()
    {
        return specialBuildingSlot;
    }

    public List<Resource> GetTradeableResources()
    {
        return tradeableResources;
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
        GeneratePlanetUI();
        GenerateTradeUI();
    }

    public void GeneratePlanetUI()
    {
        planetMenu = Instantiate(planetMenuPrefab);
        planetMenuUI = planetMenu.GetComponent<UIDocument>();

        planetMenu.GetComponent<PlanetMenu>().MakePlanetMenu(this);

        InitiatePlanetMenuFunctions();
        uiController.SetUIActive(planetMenuUI, false);
    }

    public void GenerateTradeUI()
    {
        tradeMenu = Instantiate(tradeMenuPrefab);
        tradeMenuUI = tradeMenu.GetComponent<UIDocument>();

        tradeMenuUI.GetComponent<TradeMenu>().MakeTradeMenu(this);

        InitiateTradeMenuFunctions();
        uiController.SetUIActive(tradeMenuUI, false);
    }

    private void InitiatePlanetMenuFunctions()
    {
        Button exitButton = planetMenuUI.rootVisualElement.Q<Button>("exitbutton"); ;
        exitButton.clicked += uiController.UnSetCurrentUI;

        Button tradeButton = planetMenuUI.rootVisualElement.Q<Button>("tradebutton"); ;
        tradeButton.clicked += () =>
        {
            uiController.UnSetCurrentUI();
            uiController.SetCurrentUI(tradeMenuUI);
        };

        Button specialButton = planetMenuUI.rootVisualElement.Q<Button>("specialbutton"); ;
        specialButton.clicked += uiController.UnSetCurrentUI;
    }

    private void InitiateTradeMenuFunctions()
    {
        Button exitButton = tradeMenuUI.rootVisualElement.Q<Button>("exitbutton"); ;
        exitButton.clicked += () =>
        {
            uiController.UnSetCurrentUI();
            uiController.SetCurrentUI(planetMenuUI);
        };
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
        if (withUpkeep) Upkeep();
        planetResourceHandler.UpdateResourceCounts();
        UpdateResourceDisplays();
    }

    public void UpdateResourceDisplays()
    {
        planetMenu.GetComponent<PlanetMenu>().UpdateResourcePanel(this);
        tradeMenu.GetComponent<TradeMenu>().UpdateResourcePanel(this);
        if (activeBuildingChooserMenu != null) activeBuildingChooserMenu.GetComponent<BuildingChooserMenu>().UpdateResourcePanel(this, activeBuildingChooserMenu.GetComponent<UIDocument>());
        if (activeBuildingViewerMenu != null) activeBuildingViewerMenu.GetComponent<BuildingViewerMenu>().UpdateResourcePanel(this, activeBuildingViewerMenu.GetComponent<UIDocument>());
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
                print("tegin");
                productionBuildingHandler.SetActive(true);
            }
        }
    }
}
