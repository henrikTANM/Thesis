using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents
{
    public static event Action OnCycleChange;
    public static void CycleChange() => OnCycleChange?.Invoke();

    public static event Action OnMoneyUpdate;
    public static void MoneyUpdate() => OnMoneyUpdate?.Invoke();

    public static event Action OnShipStateChange;
    public static void ShipStateChange() => OnShipStateChange?.Invoke();

    public static event Action OnUniverseViewChange;
    public static void UniverseViewChange() => OnUniverseViewChange?.Invoke();
}
