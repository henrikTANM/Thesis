using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private List<StarShip> ownedShips = new List<StarShip>();


    private void Awake()
    {
        AddShip(new("Toyota", 64, 1000, 1000));
        AddShip(new("Audi", 128, 4000, 2000));
    }

    public void AddShip(StarShip starShip) { ownedShips.Add(starShip); }

    public void RemoveShip(StarShip starShip) { ownedShips.Remove(starShip); }

    public List<StarShip> GetOwnedShips() {  return ownedShips; }
}
