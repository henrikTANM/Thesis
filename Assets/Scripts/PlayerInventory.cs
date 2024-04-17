using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int money = 10000;

    private List<SpaceShip> ownedShips = new();

    [SerializeField] private Resource moneyResource;
    [SerializeField] private GameObject spaceShipPrefab;

    public void AddMoney(int amount) { 
        money += amount;
        ResourceEvents.MoneyUpdate();
    }

    public void RemoveMoney(int amount) { 
        money -= amount; 
        ResourceEvents.MoneyUpdate();
    }

    public int GetMoney() { return money; }

    public Resource GetMoneyResource() { return moneyResource; }

    public void AddShip(Planet planet) 
    {
        GameObject spaceShipObject = Instantiate(spaceShipPrefab);
        SpaceShip spaceShip = spaceShipObject.GetComponent<SpaceShip>();
        spaceShip.CreateShip("Toyota", 64, 20.0f, planet);
        ownedShips.Add(spaceShip);
    }

    public void RemoveShip(SpaceShip starShip) { ownedShips.Remove(starShip); }

    public List<SpaceShip> GetOwnedShips() {  return ownedShips; }
}
