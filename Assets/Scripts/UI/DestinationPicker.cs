using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DestinationPicker : MonoBehaviour
{
    private ShipViewer shipViewer;
    private SpaceShipHandler spaceShipHandler;

    private void OnDestroy() 
    {
        UniverseHandler.activeDestinationPicker = null;
        UniverseHandler.HandleDestinationPicker();
    }

    public void MakeDestinationPicker(ShipViewer shipViewer, SpaceShipHandler spaceShipHandler)
    {
        this.shipViewer = shipViewer;
        this.spaceShipHandler = spaceShipHandler;

        UniverseHandler.activeDestinationPicker = this;
        UniverseHandler.HandleDestinationPicker();

        Button exitButton = GetComponent<UIDocument>().rootVisualElement.Q<Button>("cancelbutton");
        exitButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
            StartCoroutine(WaitForUIRemove(0.3f));
        };
    }

    public Tuple<bool, string> CanSetDestination(Planet destination)
    {
        if (!spaceShipHandler.home.parentStar.Equals(destination.parentStar) & !spaceShipHandler.spaceShip.isInterstellar) return new(false, "This ship cannot reach plants in other systems");
        if (spaceShipHandler.home.Equals(destination)) return new(false, "Cannot select spaceship home planet for destination");
        return new(true, "Viable destination");
    }

    public void SetDestination(Planet destination)
    {
        float distance = Vector3.Distance(spaceShipHandler.home.parentStar.transform.position, destination.parentStar.transform.position);
        float travelTime = (spaceShipHandler.home.parentStar.Equals(destination.parentStar) ? 1 : (int)(distance / 100)) * 0.99f;
        spaceShipHandler.route = new Route(spaceShipHandler, destination, travelTime);
        shipViewer.UpdateButtons();
        UIController.UpdateShipsList();
        StartCoroutine(WaitForUIRemove(0.3f));
    }

    IEnumerator WaitForUIRemove(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        UIController.RemoveLastFromUIStack();
    }
}
