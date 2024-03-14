using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int money = 100;

    private List<StarShip> ownedShips = new();

    private void Awake()
    {
        AddShip(new("Toyota", 64, 1000, 1000));
        AddShip(new("Audi", 128, 4000, 2000));
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

    public void AddShip(StarShip starShip) { ownedShips.Add(starShip); }

    public void RemoveShip(StarShip starShip) { ownedShips.Remove(starShip); }

    public List<StarShip> GetOwnedShips() {  return ownedShips; }
}
