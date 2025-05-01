using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static KeyBind;

public class KeyBindsMenu : MonoBehaviour
{
    private VisualElement root;
    private ScrollView keyBindsList;

    [SerializeField] private VisualTreeAsset keyBindTemplate;
    [SerializeField] private GameObject uiBlockerPrefab;

    private KeyBind keyBindToChange;

    private void OnGUI()
    {
        if (Event.current.isKey & Event.current.type == EventType.KeyDown & UniverseHandler.instance.editActive)
        {
            KeyCode pressedKey = Event.current.keyCode;
            if (pressedKey != KeyCode.Escape & pressedKey != KeyCode.Return)
                UniverseHandler.instance.keyBinds.Find(kb => kb.action.Equals(keyBindToChange.action)).keyCode = Event.current.keyCode;
            if (pressedKey != KeyCode.Escape) UIController.RemoveLastFromUIStack();
            CloseKeyBindChange();
        }
    }

    public void MakeKeyBindsMenu()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<NavigationSubmitEvent>((evt) =>
        {
            evt.StopPropagation();
        }, TrickleDown.TrickleDown);

        Button okButton = root.Q<Button>("ok");
        okButton.clicked += UIController.RemoveLastFromUIStack;

        Button resetToDefaultButton = root.Q<Button>("reset");
        resetToDefaultButton.clicked += () =>
        {
            UniverseHandler.ResetToDefaultKeyBinds();
            UpdateKeyBindsList();
        };

        keyBindsList = root.Q<ScrollView>("keybindslist");
        UpdateKeyBindsList();
    }

    private void UpdateKeyBindsList()
    {
        keyBindsList.Clear();
        foreach (KeyBind keyBind in UniverseHandler.instance.keyBinds)
        {
            VisualElement keybindRow = keyBindTemplate.Instantiate();
            keybindRow.Q<Label>("action").text = keyBind.description;

            Button keyBindButton = keybindRow.Q<Button>("key");
            keyBindButton.text = keyBind.keyCode.ToString();
            keyBindButton.clicked += () => 
            {
                keyBindButton.text = "Press key...";
                keyBindToChange = keyBind;
                UniverseHandler.instance.editActive = true;

                GameObject uiBlocker = Instantiate(uiBlockerPrefab);
                UIDocument uiBlockerUI = GetComponent<UIDocument>();
                UIController.AddToUIStack(new UIElement(uiBlocker, uiBlockerUI), true);
            };

            keyBindsList.Add(keybindRow);
        }
    }

    private void CloseKeyBindChange()
    {
        keyBindToChange = null;
        UpdateKeyBindsList();
        UniverseHandler.instance.editActive = false;
    }
}
