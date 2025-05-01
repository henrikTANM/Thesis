using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [NonSerialized] public int moneyAmount = 10000;
    [NonSerialized] public int moneyChange = 0;
    [NonSerialized] public List<ResourceFactor> moneyFactors = new();

    [NonSerialized] public List<SpaceShipHandler> spaceShips = new();

    public Resource moneyResource;
    [SerializeField] private GameObject spaceShipPrefab;
    [SerializeField] private SpaceShip startShipValues;

    public static PlayerInventory instance;

    private void Awake()
    {
        if (instance == null) instance = this;

        GameEvents.OnAfterCycleChange += UpdateMoney;
    }

    private void OnDestroy()
    {
        GameEvents.OnAfterCycleChange -= UpdateMoney;
    }

    public void Start()
    {
        AddShip(UniverseHandler.stars.ElementAt(0).planets.ElementAt(0), startShipValues);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) & UniverseHandler.developmentMode)
        {
            ChangeMoneyAmount(10000);
        }
    }

    public static void AddShip(Planet planet, SpaceShip spaceShip) 
    {
        GameObject spaceShipObject = Instantiate(instance.spaceShipPrefab);
        SpaceShipHandler spaceShipHandler = spaceShipObject.GetComponent<SpaceShipHandler>();
        spaceShipHandler.Create(spaceShip, planet);
        instance.spaceShips.Add(spaceShipHandler);
        planet.AddShipToOrbit(spaceShipHandler);
    }

    public static void RemoveShip(SpaceShipHandler spaceShipHandler) 
    {
        ChangeMoneyAmount(-GetShipCost(spaceShipHandler));
        instance.spaceShips.Remove(spaceShipHandler);
    }

    public static int GetShipCost(SpaceShipHandler spaceShipHandler)
    {
        foreach (ResourceAmount resourceAmount in spaceShipHandler.spaceShip.cost) { if (resourceAmount.resource == instance.moneyResource) { return resourceAmount.amount; } }
        return 0;
    }

    public static void UpdateMoney()
    {
        UpdateMoneyAmount();
        UpdateMoneyPerCycle();
        UIController.UpdateMoney();
    }

    public static void UpdateMoneyAmount()
    {
        if (instance.moneyAmount + instance.moneyChange < 0)
        {
            foreach (ResourceFactor moneyFactor in FindActiveMoneyFactors())
            {
                if (moneyFactor.resourceAmount.amount < 0)
                {
                    Debug.Log("Set moneyFactor inactive" + moneyFactor.resourceSource.name);
                    moneyFactor.resourceSource.SetActive(false, null);
                }
            }
        }
        else instance.moneyAmount += instance.moneyChange;
    }

    public static void UpdateMoneyPerCycle()
    {
        instance.moneyChange = 0;
        foreach (ResourceFactor moneyFactor in FindActiveMoneyFactors())
        {
            instance.moneyChange += moneyFactor.resourceAmount.amount;
        }
    }

    public static void AddMoneyFactor(ResourceFactor moneyFactor) { instance.moneyFactors.Add(moneyFactor); }
    public static void RemoveMoneyFactor(ResourceFactor moneyFactor) { instance.moneyFactors.Remove(moneyFactor); }

    public static void ChangeMoneyAmount(int amount) { instance.moneyAmount += amount; }
    public static bool CanChangeMoneyAmount(int amount) { return instance.moneyAmount >= amount; }

    public static List<ResourceFactor> FindActiveMoneyFactors()
    {
        return instance.moneyFactors.FindAll((ResourceFactor mf) => mf.resourceSource.active);
    }
}
