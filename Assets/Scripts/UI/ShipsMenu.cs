using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipsMenu : MonoBehaviour
{

    private UIDocument shipMenuUI;
    public VisualTreeAsset rowTemplate;
    public PlayerInventory inventory;

    public void MakeShipsMenu()
    {
        shipMenuUI = GetComponent<UIDocument>();
        print(inventory.GetOwnedShips().Count);
        foreach (StarShip ship in inventory.GetOwnedShips())
        {
            print("tegin");
            VisualElement ship_Row = rowTemplate.Instantiate();
            ScrollView shipList = shipMenuUI.rootVisualElement.Q<ScrollView>("ShipList");
            shipList.mouseWheelScrollSize = 500.0f;
            

            ship_Row.Q<Label>("name").text = ship.GetName();
            ship_Row.Q<Label>("class").text = ship.GetCargoCapacity().ToString();
            ship_Row.Q<Label>("route").text = "none";

            shipList.Add(ship_Row);
        }
    }
}
