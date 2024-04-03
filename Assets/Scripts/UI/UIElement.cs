using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIElement
{
    private GameObject gameObject;
    private UIDocument uiDocument;

    public UIElement(GameObject gameObject, UIDocument uiDocument)
    {
        this.gameObject = gameObject;
        this.uiDocument = uiDocument;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public UIDocument GetUIDocument()
    {
        return uiDocument;
    }
}
