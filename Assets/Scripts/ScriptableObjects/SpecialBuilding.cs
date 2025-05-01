using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SpecialBuilding : Building
{
    public string description;
    public Type type;

    public enum Type {
        MACHINERY,
        LOGISTICS,
        SHIPYARD,
        TRADE,
        DISCOVERY,
        BHCF
    };
}
