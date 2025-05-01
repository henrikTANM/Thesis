using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ClickableShip : MonoBehaviour
{
    [SerializeField] private GameObject shipViewerPrefab;
    private GameObject shipViewer;
    ShipViewer shipViewerMenu;

    private SpaceShipHandler spaceShipHandler;

    private void Awake()
    {
        spaceShipHandler = GetComponentInParent<SpaceShipHandler>();
    }

    private void OnMouseDown()
    {

        if (!UIController.UIDisplayed()) 
        {
            UIController.ClearUIStack();
            UIController.MakeShipViewer(spaceShipHandler);
            //universe.MoveToShip(spaceShipHandler);
        }
    }

    /*
    IEnumerator ShowShipViewer()
    {
        yield return new WaitForSeconds(0.5f);
        uiController.MakeShipViewer(spaceShipHandler);
    }
    */
}
