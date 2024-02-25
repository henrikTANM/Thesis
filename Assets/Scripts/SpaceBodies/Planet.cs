using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private List<DepositHandler> deposits = new();
    public int depositCap;

    [SerializeField] private GameObject planetMenuPrefab;
    private GameObject planetMenu;
    private UIDocument planetMenuUI;
    private UIController uiController;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) & universe.planetsMenuDisplayed)
        {
            HandlePlanetMenu();
        }

        nameTagCanvas.transform.LookAt(cameraMovementHandler.transform);
        nameTagCanvas.transform.Rotate(new(0.0f, 180.0f, 0.0f));

        if (universe.timeRunning) transform.RotateAround(parentStar.transform.position, orbitAxis, orbitSpeed * Time.deltaTime);

        Vector3 lightDirection = Vector3.Normalize(parentStar.transform.position - transform.position);
        material.SetVector("_SunlightDirection", lightDirection);

        material.SetFloat("_TimeValue", universe.timeValue);
    }

    private void OnMouseDown()
    {
        if (!selected & !universe.AreMenusDisplayed() & !universe.IsPlanetMenuDisplayed())
        {
            SetSelected(true);
            StartCoroutine(ScaleOverTime(hoverOver.transform, Vector3.zero, 0.3f));
            cameraMovementHandler.MoveToTarget(transform, transform.localScale.x * 7.5f, false);
        }
        if (selected & (universe.GetActivePlanet() != null))
        {
            HandlePlanetMenu();
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
        }
    }

    public void SetVisible(bool visible)
    {
        GetComponent<SphereCollider>().enabled = visible;
        GetComponent<MeshRenderer>().enabled = visible;
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

    public List<DepositHandler> GetDeposits()
    {
        return deposits;
    }

    public UIDocument GetPlanetMenuUI()
    {
        return planetMenuUI;
    }

    public void GenerateDeposits(List<Deposit> possibleDepositList, int depositCap)
    {
        this.depositCap = depositCap;
        for (int i = 0; i < depositCap; i++)
        {
            Deposit randomDeposit = possibleDepositList.ElementAt(UnityEngine.Random.Range(0, possibleDepositList.Count));

            GameObject newDeposit = Instantiate(depositPrefab, transform.position, Quaternion.identity);
            DepositHandler depositHandler = newDeposit.GetComponent<DepositHandler>();

            depositHandler.SetName(randomDeposit.name);
            depositHandler.SetDepositSprite(randomDeposit.depositSprite);
            depositHandler.SetPossibleProductionBuildings(randomDeposit.possibleProductionBuildings);
            depositHandler.SetBuildingCap(randomDeposit.buildingCap);

            print(depositHandler);
            deposits.Add(depositHandler);
            GeneratePlanetMenuUI();
        }
    }

    public void GeneratePlanetMenuUI()
    {
        planetMenu = Instantiate(planetMenuPrefab, transform.position, Quaternion.identity);
        planetMenuUI = planetMenu.GetComponent<UIDocument>();
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        planetMenu.GetComponent<PlanetMenu>().MakePlanetMenu(this);
        InitiatePlanetMenuFunctions();
        uiController.SetUIActive(planetMenuUI, false);
    }

    private void InitiatePlanetMenuFunctions()
    {
        List<Button> buttons = planetMenuUI.rootVisualElement.Query<Button>().ToList();

        Button exitButton = buttons.ElementAt(0);
        exitButton.clicked += HandlePlanetMenu;
    }

    private void HandlePlanetMenu()
    {
        uiController.UIMenuState(planetMenuUI);
        universe.HandlePlanetsMenu();
    }
}
