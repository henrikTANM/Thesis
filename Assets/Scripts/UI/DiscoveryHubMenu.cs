using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DiscoveryHubMenu : MonoBehaviour
{
    private Planet planet;
    private DiscoveryHubHandler discoveryHubHandler;

    private VisualElement root;

    private bool mouseOnMenu = false;
    private Vector2 localMousePosition;

    private Label discoveryProgress;
    private Label discoveryProgressPerCycle;
    private Label foodPerCycle;

    private void Update()
    {
        if (Input.GetMouseButton(0) & mouseOnMenu) MoveWindow(root, Input.mousePosition);
    }

    private void OnDestroy() 
    {
        planet.SetDiscoveryHubMenu(null);
        planet.UpdateResourceDisplays();
        UIController.UpdateMoney();
    }

    public void MakeDiscoveryHubMenu(Planet planet)
    {
        this.planet = planet;
        discoveryHubHandler = planet.GetDiscoveryHubHandler();

        root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<NavigationSubmitEvent>((evt) =>
        {
            evt.StopPropagation();
        }, TrickleDown.TrickleDown);
        root.RegisterCallback<MouseDownEvent>(evt =>
        {
            mouseOnMenu = true;
            localMousePosition = evt.localMousePosition;
        }, TrickleDown.TrickleDown);
        root.RegisterCallback<MouseUpEvent>(evt => mouseOnMenu = false, TrickleDown.TrickleDown);

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
            UIController.RemoveLastFromUIStack();
        };

        Button deconstructButton = root.Q<Button>("deconstructbutton");
        deconstructButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
            discoveryHubHandler.RemoveResourceFactors();
            planet.parentStar.hasDiscovery = false;
            planet.SetDiscoveryHubHandler(null);
            planet.SetSpecialBuilding(null);
            UIController.RemoveLastFromUIStack();
        };

        Button activateButton = root.Q<Button>("activatebutton");
        activateButton.text = discoveryHubHandler.active ? "Deactivate" : "Activate";
        activateButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
            discoveryHubHandler.active = !discoveryHubHandler.active;
            activateButton.text = discoveryHubHandler.active ? "Deactivate" : "Activate";
            UpdateDiscoveryInfo(discoveryHubHandler.active);
            planet.GetPlanetResourceHandler().UpdateResourcePerCycles();
            planet.UpdateResourceDisplays();
        };

        discoveryProgress = root.Q<Label>("progress");
        discoveryProgressPerCycle = root.Q<Label>("progpercycle");
        foodPerCycle = root.Q<Label>("foodpercycle");
        UpdateDiscoveryInfo(discoveryHubHandler.active);

        planet.UpdateResourceDisplays();
    }

    public void UpdateDiscoveryInfo(bool active)
    {
        discoveryProgress.text = "Discovery progress: " + planet.parentStar.discoveryProgress.ToString() + " %";
        discoveryProgressPerCycle.text = (active ? planet.parentStar.discoveryProgressSpeed : 0).ToString() + "%/cycle";
        foodPerCycle.text = (active ? discoveryHubHandler.inputFactor.resourceAmount.amount : 0).ToString() + "/cycle";
    }

    public void UpdateResourcePanel(List<VisualElement> resourceContainers)
    {
        VisualElement resourcesPanel = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("resourcespanel");
        resourcesPanel.Clear();
        foreach (VisualElement resourceContainer in resourceContainers) resourcesPanel.Add(resourceContainer);
    }

    private void MoveWindow(VisualElement root, Vector3 mousePos)
    {
        Vector2 pos = new(mousePos.x, Screen.height - mousePos.y);
        pos = RuntimePanelUtils.ScreenToPanel(root.panel, pos);
        pos = new(pos.x - localMousePosition.x, pos.y - localMousePosition.y);

        root.style.top = pos.y;
        root.style.left = pos.x;
    }
}
