using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int money = 10000;

    private List<SpaceShip> ownedShips = new();

    [SerializeField] private Resource moneyResource;
    [SerializeField] private GameObject spaceShipPrefab;
    [SerializeField] private SpaceShipValues startShipValues;

    private UniverseHandler universe;

    public void Start()
    {
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
        AddShip(universe.stars.ElementAt(0).planets.ElementAt(0), startShipValues);
    }

    public void Update()
    {
        // development
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddMoney(10000);
        }
        //
    }

    public void AddMoney(int amount) { 
        money += amount;
        GameEvents.MoneyUpdate();
    }

    public void RemoveMoney(int amount) { 
        money -= amount; 
        GameEvents.MoneyUpdate();
    }

    public int GetMoney() { return money; }

    public Resource GetMoneyResource() { return moneyResource; }

    public void AddShip(Planet planet, SpaceShipValues spaceShipValues) 
    {
        GameObject spaceShipObject = Instantiate(spaceShipPrefab);
        SpaceShip spaceShip = spaceShipObject.GetComponent<SpaceShip>();
        spaceShip.CreateShip(
            spaceShipValues.name, 
            spaceShipValues.fuelCapacity, 
            spaceShipValues.cargoCapacity, 
            spaceShipValues.accelerationRate, 
            spaceShipValues.cost,
            planet,
            spaceShipValues.shipScale);
        ownedShips.Add(spaceShip);
    }

    public void RemoveShip(SpaceShip ship) 
    {
        AddMoney(GetShipCost(ship));
        ownedShips.Remove(ship); 
    }

    public int GetShipCost(SpaceShip ship)
    {
        foreach (ResourceAmount resourceAmount in ship.GetCost()) { if (resourceAmount.resource == moneyResource) { return resourceAmount.amount; } }
        return 0;
    }

    public List<SpaceShip> GetOwnedShips() {  return ownedShips; }
}
