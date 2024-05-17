using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIStack : MonoBehaviour
{
    private List<UIElement> uiStack;

    public UIStack() 
    {
        uiStack = new();
    }

    public bool IsEmpty()
    {
        return uiStack.Count == 0;
    }

    public UIElement GetLast()
    {
        return IsEmpty() ? null : uiStack.ElementAt(uiStack.Count - 1);
    }

    public void Add(UIElement uiElement, bool disPlayPrevious)
    {
        if (!IsEmpty() & !disPlayPrevious) SetUIVisible(GetLast().GetUIDocument(), false);
        //SetUIVisible(uiElement, true);
        uiStack.Add(uiElement);
    }

    public void RemoveLast()
    {
        Destroy(GetLast().GetGameObject());
        uiStack.RemoveAt(uiStack.Count - 1);
        if (!IsEmpty()) SetUIVisible(GetLast().GetUIDocument(), true);
    }

    public void Clear() {
        foreach (UIElement uiElement in uiStack)
        {
            Destroy(uiElement.GetGameObject());
        }
        uiStack.Clear(); 
    }

    public void SetUIVisible(UIDocument uiDoc, bool visible)
    {
        uiDoc.sortingOrder = visible ? 1 : 0;
        uiDoc.rootVisualElement.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
    }
}
