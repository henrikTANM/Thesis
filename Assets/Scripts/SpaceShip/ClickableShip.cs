using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ClickableShip : MonoBehaviour
{
    [SerializeField] private GameObject shipViewerPrefab;
    private GameObject shipViewer;
    ShipViewer shipViewerMenu;

    private UIController uiController;

    private SpaceShip ship;

    private void Awake()
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        ship = GetComponentInParent<SpaceShip>();
    }

    private void OnMouseDown()
    {
        if (shipViewer == null) MakeShipViewer(ship);
    }

    public void MakeShipViewer(SpaceShip ship)
    {
        shipViewer = Instantiate(shipViewerPrefab);
        UIDocument shipViewerUI = shipViewer.GetComponent<UIDocument>();
        shipViewerMenu = shipViewer.GetComponent<ShipViewer>();
        shipViewerMenu.MakeShipViewer(ship);
        uiController.AddToUIStack(new UIElement(shipViewer, shipViewerUI), false);
    }
}
