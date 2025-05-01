using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class EscapeMenu : MonoBehaviour
{
    [SerializeField] private GameObject keyBindsMenuPrefab;

    public void OnDestroy()
    {
        UniverseHandler.HandleEscapeMenu();
    }

    public void MakeEscapeMenu()
    {

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<NavigationSubmitEvent>((evt) =>
        {
            evt.StopPropagation();
        }, TrickleDown.TrickleDown);

        Button resumeButton = root.Q<Button>("resume");
        resumeButton.clicked += Resume;

        Button optionsButton = root.Q<Button>("keybinds");
        optionsButton.clicked += MakeKeyBindsMenu;

        Button exitButton = root.Q<Button>("exit");
        exitButton.clicked += ExitToMenu;

        Button quitButton = root.Q<Button>("quit");
        quitButton.clicked += Application.Quit;

        SoundFX.PlayAudioClip(SoundFX.AudioType.SIDE_BUTTON);
    }

    private void Resume()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
        UIController.RemoveLastFromUIStack();
    }

    private void MakeKeyBindsMenu()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
        GameObject keyBindsMenu = Instantiate(keyBindsMenuPrefab);
        UIDocument keyBindsMenuUI = keyBindsMenu.GetComponent<UIDocument>();
        keyBindsMenu.GetComponent<KeyBindsMenu>().MakeKeyBindsMenu();
        UIController.AddToUIStack(new UIElement(keyBindsMenu, keyBindsMenuUI), false);
    }

    private void ExitToMenu()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
        SceneManager.LoadScene("MenuScene");
    }
}
