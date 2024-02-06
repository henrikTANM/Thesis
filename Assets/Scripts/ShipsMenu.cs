using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipsMenu : MonoBehaviour
{

    private UIDocument UIDocument;
    public VisualTreeAsset rowTemplate;
    public PlayerInventory inventory;

    private void OnEnable()
    {
        UIDocument = GetComponent<UIDocument>();

        foreach (StarShip ship in inventory.GetOwnedShips())
        {
            TemplateContainer ship_Row = rowTemplate.Instantiate();

            ship_Row.Q<Label>("type").text = ship.GetName();
            ship_Row.Q<Label>("cc").text = ship.GetCargoCapacity().ToString();
            ship_Row.Q<Label>("fc").text = ship.GetFuelCapacity().ToString();
            ship_Row.Q<Label>("tpower").text = ship.GetThrustPower().ToString();

            UIDocument.rootVisualElement.Q("ShipList").Add(ship_Row);
        }
    }
}
