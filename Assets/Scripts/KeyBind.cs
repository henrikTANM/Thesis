using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KeyBind
{
    public string description;
    public KeyPressAction action;
    public KeyCode keyCode;

    public KeyBind(string description, KeyPressAction action, KeyCode keyCode)
    {
        this.description = description;
        this.action = action;
        this.keyCode = keyCode;
    }

    public enum KeyPressAction
    {
        UNDO_MOVE,
        REDO_MOVE,
        TOGGLE_TIME,
        TOGGLE_TELESCOPE,
        CLEAR_UI,
        CLEAR_NOTIFICATIONS
    }
}
