using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipsMenu : MonoBehaviour
{
    UIController uiController;

    public VisualTreeAsset ownedShipRowTemplate;

    [SerializeField] private GameObject shipViewerPrefab;
    private GameObject shipViewer;

    public void MakeShipsMenu(PlayerInventory inventory)
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += uiController.RemoveLastFromUIStack;

        ScrollView shipsList = root.Q<ScrollView>("ShipList");
        shipsList.mouseWheelScrollSize = 500.0f;

        //shipsList.Clear();

        foreach (SpaceShip ship in inventory.GetOwnedShips())
        {
            VisualElement ship_Row = ownedShipRowTemplate.Instantiate();

            Button shipViewButton = ship_Row.Q<Button>("shipviewbutton");
            shipViewButton.clicked += () => { MakeShipViewer(ship); };

            ship_Row.Q<Label>("name").text = ship.GetName();
            ship_Row.Q<Label>("class").text = ship.GetCargoCapacity().ToString();
            ship_Row.Q<Label>("route").text = "none";

            shipsList.Add(ship_Row);
        }
    }

    public void MakeShipViewer(SpaceShip ship)
    {
        shipViewer = Instantiate(shipViewerPrefab);
        UIDocument shipViewerUI = shipViewer.GetComponent<UIDocument>();
        shipViewer.GetComponent<ShipViewer>().MakeShipViewer(this, ship);
        uiController.AddToUIStack(new UIElement(shipViewer, shipViewerUI), false);
    }
}
