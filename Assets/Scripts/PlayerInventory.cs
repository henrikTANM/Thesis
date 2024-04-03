using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int money = 10000;

    private List<SpaceShip> ownedShips = new();

    [SerializeField] private Resource moneyResource;
    [SerializeField] private GameObject spaceShipPrefab;

    private void Awake()
    {
        GameObject spaceShip1O = Instantiate(spaceShipPrefab);
        SpaceShip spaceShip1 = spaceShip1O.GetComponent<SpaceShip>();
        spaceShip1.CreateShip("Toyota", 64, 20.0f);
        AddShip(spaceShip1);

        GameObject spaceShip2O = Instantiate(spaceShipPrefab);
        SpaceShip spaceShip2 = spaceShip2O.GetComponent<SpaceShip>();
        spaceShip2.CreateShip("Audi", 128, 10.0f);
        AddShip(spaceShip2);
    }

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

    public void AddShip(SpaceShip starShip) { ownedShips.Add(starShip); }

    public void RemoveShip(SpaceShip starShip) { ownedShips.Remove(starShip); }

    public List<SpaceShip> GetOwnedShips() {  return ownedShips; }
}
