using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EscapeMenu : MonoBehaviour
{
    private UniverseHandler universe;
    private UIController uiController;

    public void MakeEscapeMenu()
    {
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
        uiController = GameObject.Find("UIController").GetComponent<UIController>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button resumeButton = root.Q<Button>("ResumeButton");
        resumeButton.clicked += Resume;

        Button saveButton = root.Q<Button>("SaveButton");
        //saveButton.clicked += ;

        Button loadButton = root.Q<Button>("LoadButton");
        //loadButton.clicked += ;

        Button optionsButton = root.Q<Button>("OptionsButton");
        // optionsButton.clicked += ;

        Button exitButton = root.Q<Button>("ExitToMenuButton");
        // exitButton.clicked += ;

        Button quitButton = root.Q<Button>("QuitGameButton");
        quitButton.clicked += Application.Quit;
    }

    private void Resume()
    {
        uiController.RemoveLastFromUIStack();
        universe.HandleEscapeMenu();
    }
}
