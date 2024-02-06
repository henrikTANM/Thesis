using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputEvents
{
    public static event Action OnTimeStateChange;
    public static void TimeStateChange() => OnTimeStateChange?.Invoke();

    public static event Action OnEscapeMenu;
    public static void EscapeMenu() => OnEscapeMenu?.Invoke();

    public static event Action OnShipsMenu;
    public static void ShipsMenu() => OnShipsMenu?.Invoke();

    public static event Action OnClusterView;
    public static void ClusterView() => OnClusterView?.Invoke();
}
