using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BHCFMenu : MonoBehaviour
{
    private Planet planet;
    private BHCFHandler bhcfHandler;

    private VisualElement root;

    private bool mouseOnMenu = false;
    private Vector2 localMousePosition;

    [SerializeField] private VisualTreeAsset perCycleTemplate;

    private Label progress;
    private Label progressPerCycle;
    private VisualElement inputList;

    private void Update()
    {
        if (Input.GetMouseButton(0) & mouseOnMenu) MoveWindow(root, Input.mousePosition);
    }

    private void OnDestroy() 
    {
        planet.SetBhcfMenu(null);
        planet.UpdateResourceDisplays();
        UIController.UpdateMoney();
    }

    public void MakeBHCFMenu(Planet planet)
    {
        this.planet = planet;
        bhcfHandler = planet.GetBHCFHandler();

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

        Button activateButton = root.Q<Button>("activatebutton");
        activateButton.text = bhcfHandler.active ? "Deactivate" : "Activate";
        activateButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
            bhcfHandler.active = !bhcfHandler.active;
            activateButton.text = bhcfHandler.active ? "Deactivate" : "Activate";
            UpdateBHCFInfo(bhcfHandler.active);
            planet.GetPlanetResourceHandler().UpdateResourcePerCycles();
            planet.UpdateResourceDisplays();
        };

        progress = root.Q<Label>("progress");
        progressPerCycle = root.Q<Label>("progpercycle");
        UpdateBHCFInfo(bhcfHandler.active);

        VisualElement inputList = root.Q<VisualElement>("inputlist");
        foreach (ResourceFactor resourceFactor in bhcfHandler.inputFactors)
        {
            VisualElement perCycle = perCycleTemplate.Instantiate();
            perCycle.Q<Label>("percycle").text = resourceFactor.resourceAmount.amount.ToString() + "/cycle";
            VisualElement resourceImage = perCycle.Q<VisualElement>("resourceimage");
            resourceImage.style.backgroundImage = new StyleBackground(resourceFactor.resourceAmount.resource.resourceSprite);
            resourceImage.style.unityBackgroundImageTintColor = new StyleColor(resourceFactor.resourceAmount.resource.spriteColor);
            inputList.Add(perCycle);
        }

        planet.UpdateResourceDisplays();
    }

    public void UpdateBHCFInfo(bool active)
    {
        progress.text = "Containment progress " + bhcfHandler.progress.ToString() + " %";
        progressPerCycle.text = "Containment rate " + (active ? bhcfHandler.progressRate : 0).ToString() + "%/cycle";
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
