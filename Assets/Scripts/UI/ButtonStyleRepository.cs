using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonStyleRepository
{
    public void ChangeStyle(Button button, Color borderColor, Color bgColor, Color bgImageColor)
    {

        button.style.borderBottomColor = new StyleColor(borderColor);
        button.style.borderLeftColor = new StyleColor(borderColor);
        button.style.borderRightColor = new StyleColor(borderColor);
        button.style.borderTopColor = new StyleColor(borderColor);

        button.style.backgroundColor = new StyleColor(bgColor);
        button.style.unityBackgroundImageTintColor = new StyleColor(bgImageColor);
    }
}
