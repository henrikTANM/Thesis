using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StartSettings : ScriptableObject
{
    public List<PlanetSettings> startSystemPlanets;
    public PlanetSettings blackHolePlanet;
    public SpaceShip startSpaceShipValue;

    public List<ResourceAmount> startPlanetResourceAmounts;

    [Serializable]
    public class PlanetSettings
    {
        public PlanetValues planetValues;
        public List<Deposit> depositList;
    }
}
