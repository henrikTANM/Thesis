using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipsMenu : MonoBehaviour
{
    private UIController uiController;
    private PlayerInventory inventory;
    [SerializeField] private CameraMovementHandler cameraMovementHandler;

    public VisualTreeAsset ownedShipRowTemplate;

    [SerializeField] private GameObject shipViewerPrefab;
    private GameObject shipViewer;

    private ScrollView shipsList;

    private void Awake()
    {
        GameEvents.OnShipStateChange += UpdateShipsList;
    }

    private void OnDestroy()
    {
        GameEvents.OnShipStateChange -= UpdateShipsList;
    }

    public void MakeShipsMenu()
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        inventory = GameObject.Find("PlayerInventory").GetComponent<PlayerInventory>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += uiController.RemoveLastFromUIStack;

        shipsList = root.Q<ScrollView>("ShipList");
        shipsList.mouseWheelScrollSize = 500.0f;

        UpdateShipsList();
    }

    public void UpdateShipsList()
    {
        shipsList.Clear();
        foreach (SpaceShip ship in inventory.GetOwnedShips())
        {
            VisualElement ship_Row = ownedShipRowTemplate.Instantiate();

            Button shipViewButton = ship_Row.Q<Button>("shipviewbutton");
            shipViewButton.clicked += () => { MakeShipViewer(ship); };

            ship_Row.Q<Label>("name").text = ship.GetName();
            ship_Row.Q<Label>("location").text = (ship.IsTravelling() ? "On route to " : "Currently at ") + ship.GetCurrentPlanet().GetName();

            Button camera = ship_Row.Q<Button>("camera");
            camera.clicked += () =>
            {
                ship.GetCurrentPlanet().MoveToPlanet();
                ship.GetCurrentPlanet().SetSelected(true);
                uiController.ClearUIStack();
            };

            shipsList.Add(ship_Row);
        }
    }

    public void MakeShipViewer(SpaceShip ship)
    {
        shipViewer = Instantiate(shipViewerPrefab);
        UIDocument shipViewerUI = shipViewer.GetComponent<UIDocument>();
        shipViewer.GetComponent<ShipViewer>().MakeShipViewer(ship);
        uiController.AddToUIStack(new UIElement(shipViewer, shipViewerUI), false);
    }
}
