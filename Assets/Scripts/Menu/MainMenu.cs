using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    private void Awake()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button newGameButton = root.Q<Button>("newgame");
        newGameButton.clicked += NewGame;

        Button loadGameButton = root.Q<Button>("loadgame");
        loadGameButton.clicked += LoadGameMenu;

        Button optionsButton = root.Q<Button>("options");
        optionsButton.clicked += OptionsMenu;

        Button exitButton = root.Q<Button>("exit");
        exitButton.clicked += Application.Quit;
    }

    private void NewGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void LoadGameMenu()
    {
        
    }

    private void OptionsMenu()
    {

    }
}
