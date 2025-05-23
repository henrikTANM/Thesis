using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents
{
    public static event Action OnCycleChange;
    public static void CycleChange() => OnCycleChange?.Invoke();

    public static event Action OnAfterCycleChange;
    public static void AfterCycleChange() => OnAfterCycleChange?.Invoke();

    public static event Action OnMoneyUpdate;
    public static void MoneyUpdate() => OnMoneyUpdate?.Invoke();

    public static event Action OnUniverseViewChange;
    public static void UniverseViewChange() => OnUniverseViewChange?.Invoke();
}
