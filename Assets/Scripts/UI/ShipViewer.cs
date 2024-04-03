using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipViewer : MonoBehaviour
{
    private UIController uiController;

    public VisualTreeAsset createRouteButtonPrefab;
    public VisualTreeAsset routeButtonsPrefab;

    [SerializeField] private GameObject routeMakerPrefab;
    private GameObject routeMaker;

    private VisualElement routeButtons;

    public void MakeShipViewer(ShipsMenu shipsMenu, SpaceShip ship)
    {
        uiController = GameObject.Find("UIController").GetComponent<UIController>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<Label>("name").text = ship.GetName();

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += uiController.RemoveLastFromUIStack;

        Button sellButton = root.Q<Button>("sellbutton");
        sellButton.clicked += ship.Sell;




        routeButtons = root.Q<VisualElement>("buttons");
        UpdateButtons(ship);
    }

    public void UpdateButtons(SpaceShip ship)
    {
        routeButtons.Clear();

        if (ship.HasRoute())
        {
            VisualElement routeButtonsContainer = routeButtonsPrefab.Instantiate();

            Button cancelButton = routeButtonsContainer.Q<Button>("cancelbutton");
            cancelButton.clicked += ship.RemoveRoute;

            Button pauseButton = routeButtonsContainer.Q<Button>("pausebutton");
            pauseButton.clicked += () => { ship.setRoutePaused(!ship.IsRoutePaused()); };

            routeButtons.Add(routeButtonsContainer);
        }
        else
        {
            VisualElement createButtonContainer = createRouteButtonPrefab.Instantiate();

            Button createButton = createButtonContainer.Q<Button>("createbutton");
            createButton.clicked += () => { MakeRouteMaker(this, ship); };

            routeButtons.Add(createButtonContainer);
        }
    }

    private void MakeRouteMaker(ShipViewer shipViewer, SpaceShip ship)
    {
        routeMaker = Instantiate(routeMakerPrefab);
        UIDocument routeMakerUI = routeMaker.GetComponent<UIDocument>();
        routeMaker.GetComponent<RouteMaker>().MakeRouteMaker(shipViewer, ship);
        uiController.AddToUIStack(new UIElement(routeMaker, routeMakerUI), false);
    }
}
