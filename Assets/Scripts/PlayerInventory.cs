using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int money = 10000;

    private List<SpaceShip> ownedShips = new();

    [SerializeField] private Resource moneyResource;
    [SerializeField] private GameObject spaceShipPrefab;

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
            planet);
        ownedShips.Add(spaceShip);
    }

    public void RemoveShip(SpaceShip starShip) { ownedShips.Remove(starShip); }

    public List<SpaceShip> GetOwnedShips() {  return ownedShips; }
}
