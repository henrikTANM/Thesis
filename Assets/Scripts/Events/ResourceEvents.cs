using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceEvents
{
    public static event Action OnCycleChange;
    public static void CycleChange() => OnCycleChange?.Invoke();
}
