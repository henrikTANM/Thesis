using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SpecialBuilding : ScriptableObject
{
    public string name;
    public string description;
    public Sprite image;

    public ResourceAmount[] cost;
}
