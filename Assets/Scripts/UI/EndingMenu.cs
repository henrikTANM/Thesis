using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class EndingMenu : MonoBehaviour
{
    public void MakeEndingMenu()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<NavigationSubmitEvent>((evt) =>
        {
            evt.StopPropagation();
        }, TrickleDown.TrickleDown);

        Button endGameButton = root.Q<Button>("endgame");
        endGameButton.clicked += EndGame;

        Button keepPlayingButton = root.Q<Button>("keepplaying");
        keepPlayingButton.clicked += KeepPlaying;
    }

    private void EndGame()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_EXIT);
        SceneManager.LoadScene("MenuScene");
    }

    private void KeepPlaying()
    {
        SoundFX.PlayAudioClip(SoundFX.AudioType.MENU_ACTION);
        UIController.RemoveLastFromUIStack();
        UIController.SetGameUIActive(true);
    }
}
