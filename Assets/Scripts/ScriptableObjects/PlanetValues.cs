using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlanetValues : ScriptableObject
{
    public Vector2 scale;
    public List<Texture2D> planetTextures;
    public List<Texture2D> cloudTextures;

    public List<Deposit> possibleDeposits;
    public int depositCap;
}
