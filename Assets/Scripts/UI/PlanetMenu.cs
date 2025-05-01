using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlanetMenu : MonoBehaviour
{
    private SpecialBuildingMenuMaker specialBuildingMenuMaker;
    private Planet planet;

    private VisualElement root;

    private bool mouseOnMenu = false;
    private Vector2 localMousePosition;

    public VisualTreeAsset resourceTemplate;
    public VisualTreeAsset depositBuildingButtonTemplate;

    public Sprite blockedConstructionSprite;
    public Sprite defaultSpecialButtonImage;

    [SerializeField] private GameObject specialBuildingChooserMenuPrefab;
    private GameObject specialBuildingChooserMenu;

    private Button specialBuildingButton;

    private void Update()
    {
        if (Input.GetMouseButton(0) & mouseOnMenu) MoveWindow(root, Input.mousePosition);
    }

    public void MakePlanetMenu(Planet planet)
    {
        specialBuildingMenuMaker = GetComponent<SpecialBuildingMenuMaker>();
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

        root.Q<Label>("planetname").text = planet.name;

        Button exitButton = root.Q<Button>("exitbutton");
        exitButton.clicked += () =>
        {
            SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
            UIController.RemoveLastFromUIStack();
        };

        root.Q<VisualElement>("background").style.backgroundImage = 
            new StyleBackground(planet.GetSettlementSprite());

        specialBuildingButton = root.Q<Button>("specialbutton"); ;
        specialBuildingButton.clicked += HandleSpecialBuildingButton;
        SpecialBuilding specialBuilding = planet.specialBuilding;
        specialBuildingButton.style.backgroundImage = 
            new StyleBackground(specialBuilding == null ? defaultSpecialButtonImage : specialBuilding.buildingSprite);

        List<VisualElement> depositContainers = root.Query("de").ToList();
        List<DepositHandler> deposits = planet.GetDeposits();

        for (int i = 0; i < depositContainers.Count; i++)
        {
            VisualElement depositContainer = depositContainers[i];

            if (deposits.ElementAt(i) == null)
            {
                depositContainer.style.backgroundImage =
                    new StyleBackground(blockedConstructionSprite);
                depositContainer.SetEnabled(false);
            }
            else
            {
                depositContainer.style.backgroundImage =
                    new StyleBackground(deposits.ElementAt(i).GetDepositSprite());

                foreach (BuildingSlot buildingSlot in deposits.ElementAt(i).GetBuildingSlots())
                {
                    TemplateContainer buildingButton = depositBuildingButtonTemplate.Instantiate();
                    buildingButton.style.flexGrow = 1;
                    depositContainer.Add(buildingButton);
                    Button button = buildingButton.Q<Button>("building_button");
                    Button buildingSlotButton = buildingSlot.GetButton();
                    if (buildingSlotButton != null)
                    {
                        button.style.backgroundImage = buildingSlotButton.style.backgroundImage;
                        button.style.unityBackgroundImageTintColor = buildingSlotButton.style.unityBackgroundImageTintColor;
                    }
                    buildingSlot.SetButton(button);
                }
            }
        }

        root.Q<VisualElement>("reachedblocker").SetEnabled(!planet.reached);
        root.Q<VisualElement>("reachedblocker").style.visibility = !planet.reached ? Visibility.Visible : Visibility.Hidden;

        planet.UpdateResourceDisplays();
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_OPEN);
    }

    public void UpdateResourcePanel(List<VisualElement> resourceContainers)
    {
        VisualElement resourcesPanel = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("resourcespanel");
        resourcesPanel.Clear();
        foreach (VisualElement resourceContainer in resourceContainers) resourcesPanel.Add(resourceContainer);
    }

    public void HandleSpecialBuildingButton()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ENTER);
        SpecialBuilding specialBuilding = planet.specialBuilding;
        if (specialBuilding == null) { MakeSpecialBuildingChooserMenu(); }
        else { specialBuildingMenuMaker.MakeMenu(planet, specialBuilding); }
    }

    private void MakeSpecialBuildingChooserMenu()
    {
        specialBuildingChooserMenu = Instantiate(specialBuildingChooserMenuPrefab);
        UIDocument specialBuildingChooserMenuUI = specialBuildingChooserMenu.GetComponent<UIDocument>();
        planet.SetSpecialBuildingChooserMenu(specialBuildingChooserMenu);
        specialBuildingChooserMenu.GetComponent<SpecialBuildingChooserMenu>().MakeSpecialBuildingChooserMenu(planet);
        UIController.AddToUIStack(new UIElement(specialBuildingChooserMenu, specialBuildingChooserMenuUI), false);
    }

    public void ChangeSpecialBuildingButtonImage(Sprite image) 
    {
        specialBuildingButton.style.backgroundImage = image == null ?
            new StyleBackground(defaultSpecialButtonImage) :
            new StyleBackground(image);
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
