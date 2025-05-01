using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

public class UniverseHandler : MonoBehaviour
{
    [SerializeField] private UniverseGenerator universeGenerator;

    [NonSerialized] public static List<Star> stars = new();

    [NonSerialized] public int cycleCount = 0;
    public float cycleLength;
    public static float timeCycleValue = 0.0f;
    public static float timeValue = 0.0f;
    public static bool timeRunning = false;

    [NonSerialized] public static bool escapeMenuDisplayed = false;
    [NonSerialized] public static bool destinationPickerDisplayed = false;
    [NonSerialized] public bool editActive = false;
    [NonSerialized] public static DestinationPicker activeDestinationPicker;

    [NonSerialized] public Planet selectedPlanet;
    [NonSerialized] public Star selectedStar;
    [NonSerialized] public SpaceShipHandler selectedSpaceShip;

    public static bool developmentMode = false;

    public List<Resource> allResources;

    public static bool startWait = true;

    public static UniverseHandler instance;

    [NonSerialized] public List<KeyBind> keyBinds = new();
    [SerializeField] private List<KeyBind> defaultKeyBinds;

    private MoveHistory moveHistory = new();


    public enum StateChange
    {
        UNIVERSE_TO_STAR,
        UNIVERSE_TO_PLANET,
        UNIVERSE_TO_SPACESHIP,

        STAR_TO_UNIVERSE,
        STAR_TO_STAR,
        STAR_TO_PLANET,
        STAR_TO_OTHER_PLANET,
        STAR_TO_SPACESHIP,

        PLANET_TO_UNIVERSE,
        PLANET_TO_STAR,
        PLANET_TO_OTHER_STAR,
        PLANET_TO_PLANET,
        PLANET_TO_OTHER_PLANET,
        PLANET_TO_SPACESHIP,

        SPACESHIP_TO_UNIVERSE,
        SPACESHIP_TO_STAR,
        SPACESHIP_TO_PLANET,
        SPACESHIP_TO_SPACESHIP,
    }

    private void Awake()
    {
        if (instance == null) instance = this;

        ResetToDefaultKeyBinds();
        stars = universeGenerator.GenerateStars();

        InputEvents.OnTimeStateChange += HandleTimeChange;
        InputEvents.OnEscapeMenu += HandleEscapeMenu;
        InputEvents.OnClusterView += HandleUniverseView;
        InputEvents.OnSystemView += HandleSystemView;
    }

    private void OnDestroy()
    {
        InputEvents.OnTimeStateChange -= HandleTimeChange;
        InputEvents.OnEscapeMenu -= HandleEscapeMenu;
        InputEvents.OnClusterView -= HandleUniverseView;
        InputEvents.OnClusterView -= HandleSystemView;
    }
    private void Start()
    {
        StartCoroutine(GameStartCameraMove());
    }

    private void Update()
    {
        if (timeRunning)
        {
            timeValue += Time.deltaTime;
            timeCycleValue += Time.deltaTime;
        }

        if (timeCycleValue >= cycleLength)
        {
            timeCycleValue = 0.0f;
            cycleCount++;
            GameEvents.CycleChange();
            GameEvents.AfterCycleChange();
        }

        if (Input.GetKeyDown(GetKeyCode(KeyBind.KeyPressAction.TOGGLE_TIME))
            & !escapeMenuDisplayed & !destinationPickerDisplayed & !editActive) InputEvents.TimeStateChange();

        if (Input.GetKeyDown(GetKeyCode(KeyBind.KeyPressAction.UNDO_MOVE))
            & !escapeMenuDisplayed & !editActive & !CameraMovementHandler.instance.movingToTarget) moveHistory.UndoMove();
        if (Input.GetKeyDown(GetKeyCode(KeyBind.KeyPressAction.REDO_MOVE))
            & !escapeMenuDisplayed & !editActive & !CameraMovementHandler.instance.movingToTarget) moveHistory.RedoMove(); 

        if (Input.GetKey(KeyCode.LeftControl) & Input.GetKeyDown(KeyCode.D)
            & !escapeMenuDisplayed & !destinationPickerDisplayed & !editActive)
        {
            developmentMode = !developmentMode;
            UIController.AddMessage(new Message(
                "Development mode: " + (developmentMode ? "on" : "off"),
                Message.MessageType.NOTIFICATION,
                new MessageSender(),
                Message.SenderType.SYSTEM
                ));
        }
    }

    public static void HandleUniverseView()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.GAME_BUTTON);
        if (instance.selectedStar != null) AddMoveToUniverseView();
    }

    public static void AddMoveToUniverseView()
    {
        instance.moveHistory.AddMove(null);
        MoveToUniverseView();
    }

    public static void MoveToUniverseView()
    {
        if (!destinationPickerDisplayed) UIController.ClearUIStack();

        if (instance.selectedPlanet != null)
            foreach (Star star in stars) star.SetSelected(false, StateChange.PLANET_TO_UNIVERSE);
        else if (instance.selectedStar != null)
            foreach (Star star in stars) star.SetSelected(false, StateChange.STAR_TO_UNIVERSE);

        CameraMovementHandler.MoveToUniverseView();
    }

    public static void HandleSystemView()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.GAME_BUTTON);
        if (instance.selectedPlanet != null) AddMoveToStar(instance.selectedStar);
    }

    public static void AddMoveToStar(Star star)
    {
        instance.moveHistory.AddMove(star);
        MoveToStar(star);
    }

    public static void MoveToStar(Star star)
    {
        if (!destinationPickerDisplayed) UIController.ClearUIStack();

        if (instance.selectedPlanet != null)
        {
            if (star == instance.selectedStar) star.SetSelected(true, StateChange.PLANET_TO_STAR);
            else star.SetSelected(true, StateChange.PLANET_TO_OTHER_STAR);
        }
        else if (instance.selectedStar != null) star.SetSelected(true, StateChange.STAR_TO_STAR);
        else star.SetSelected(true, StateChange.UNIVERSE_TO_STAR);

        CameraMovementHandler.MoveToTarget(star.body.transform, star.body.transform.localScale.x * 10, false);
    }

    public static void AddMoveToPlanet(Planet planet)
    {
        instance.moveHistory.AddMove(planet);
        MoveToPlanet(planet);
    }

    public static void MoveToPlanet(Planet planet)
    {
        if (instance.selectedPlanet != null)
        {
            if (instance.selectedStar == planet.parentStar) planet.SetSelected(true, StateChange.PLANET_TO_PLANET);
            else planet.SetSelected(true, StateChange.PLANET_TO_OTHER_PLANET);
        }
        else if (instance.selectedStar != null)
        {
            if (instance.selectedStar == planet.parentStar) planet.SetSelected(true, StateChange.STAR_TO_PLANET);
            else planet.SetSelected(true, StateChange.STAR_TO_OTHER_PLANET);
        }
        else planet.SetSelected(true, StateChange.UNIVERSE_TO_PLANET);

        CameraMovementHandler.MoveToTarget(planet.body.transform, planet.body.transform.localScale.x * 3, false);
        planet.ShowPlanetMenu(false);
    }

    /*
    
    POSSIBLE FUTURE FEATURE

    public static void MoveToShip(SpaceShipHandler spaceShipHandler)
    {
        if (selectedSpaceShip != null) spaceShipHandler.SetSelected(true, StateChange.SPACESHIP_TO_SPACESHIP);
        else if (selectedPlanet != null) spaceShipHandler.SetSelected(true, StateChange.PLANET_TO_SPACESHIP);
        else if (selectedStar != null) spaceShipHandler.SetSelected(true, StateChange.STAR_TO_SPACESHIP);
        else spaceShipHandler.SetSelected(true, StateChange.UNIVERSE_TO_SPACESHIP);

        cameraMovementHandler.MoveToTarget(spaceShipHandler.body.transform, spaceShipHandler.body.transform.localScale.x * 3, false);
    }
    */

    IEnumerator GameStartCameraMove()
    {
        yield return new WaitForSeconds(1.0f);
        AddMoveToStar(stars.ElementAt(0));
        timeRunning = true;
    }

    public static List<Planet> GetManagedPlanets()
    {
        List<Planet> managedPlanets = new();
        foreach (Star star in stars) { foreach (Planet planet in star.planets) { if (planet.managed) managedPlanets.Add(planet); } }
        return managedPlanets;
    }

    public static bool SelectedPlanetEquals(Planet planet)
    {
        if (instance.selectedPlanet == null) return false;
        else return instance.selectedPlanet.Equals(planet);
    }

    public static void HandleEscapeMenu()
    {
        escapeMenuDisplayed = !escapeMenuDisplayed;
        timeRunning = !escapeMenuDisplayed;
        UIController.ChangeTimeButtonIcon();
        UIController.SetGameUIActive(!escapeMenuDisplayed);
    }

    public static void HandleDestinationPicker()
    {
        destinationPickerDisplayed = !destinationPickerDisplayed;
        timeRunning = false;
        UIController.ChangeTimeButtonIcon();
        UIController.SetGameUIActive(!destinationPickerDisplayed);
    }

    public void HandleTimeChange() 
    { 
        timeRunning = !timeRunning;
        SoundFX.PlayAudioClip(SoundFX.AudioType.GAME_BUTTON);
    }

    public static void ResetToDefaultKeyBinds()
    {
        instance.keyBinds.Clear();
        foreach (KeyBind keyBind in instance.defaultKeyBinds) 
            instance.keyBinds.Add(new(keyBind.description, keyBind.action, keyBind.keyCode));
    }

    public static KeyCode GetKeyCode(KeyBind.KeyPressAction keyPressAction)
    {
        return instance.keyBinds.Find(kb => kb.action.Equals(keyPressAction)).keyCode;
    }
}
