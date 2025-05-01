using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BonusBuildingViewer : MonoBehaviour
{
    private VisualElement root;

    private Planet planet;

    private bool mouseOnMenu = false;
    private Vector2 localMousePosition;

    private void Update()
    {
        if (Input.GetMouseButton(0) & mouseOnMenu) MoveWindow(root, Input.mousePosition);
    }

    private void OnDestroy()
    {
        planet.UpdateResourceDisplays();
        UIController.UpdateMoney();
    }

    public void MakeBonusBuildingViewer(Planet planet, SpecialBuilding specialBuilding)
    {
        this.planet = planet;

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

        root.Q<Label>("name").text = specialBuilding.name;
        root.Q<Label>("info").text = specialBuilding.description;

        Button deconstructButton = root.Q<Button>("deconstructbutton");
        deconstructButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
            planet.SetSpecialBuilding(null);
            PlanetResourceHandler planetResourceHandler = planet.GetPlanetResourceHandler();
            if (specialBuilding.name == "Advanced machinery")
            {
                planetResourceHandler.RemoveRawMultipiler(1.5f);
                planetResourceHandler.RemoveEndMultipiler(0.5f);
            }
            if (specialBuilding.name == "Advanced logistics")
            {
                planetResourceHandler.RemoveRawMultipiler(0.5f);
                planetResourceHandler.RemoveEndMultipiler(1.5f);
            }

            UIController.RemoveLastFromUIStack();
        };
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
