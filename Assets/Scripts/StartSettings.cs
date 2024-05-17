using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StartSettings : ScriptableObject
{
    public List<PlanetSettings> startSystemPlanets;
    public SpaceShipValues startSpaceShipValue;

    [Serializable]
    public class PlanetSettings
    {
        public PlanetValues planetValues;
        public List<Deposit> depositList;
    }
}
