using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DepositHandler : MonoBehaviour
{
    [SerializeField] private GameObject buildingSlotPrefab;

    private string name;
    private Planet planet;
    private Sprite depositSprite;
    private List<ProductionBuilding> possibleproductionBuildings;

    private List<BuildingSlot> buildingSlots = new();

    public void Make(Deposit deposit, Planet planet)
    {
        name = deposit.name;
        depositSprite = deposit.depositSprite;
        possibleproductionBuildings = deposit.possibleProductionBuildings;
        this.planet = planet;

        for (int i = 0; i < deposit.buildingCap; i++)
        {
            GameObject buildingSlotObject = Instantiate(buildingSlotPrefab);
            BuildingSlot buildingSlot = buildingSlotObject.GetComponent<BuildingSlot>();
            buildingSlot.SetDeposit(this);
            buildingSlots.Add(buildingSlot);
        }
    }

    public Planet GetPlanet()
    {
        return planet;
    }

    public Sprite GetDepositSprite()
    {
        return depositSprite;
    }

    public List<ProductionBuilding> GetPossibleProductionBuildings()
    {
        return possibleproductionBuildings;
    }

    public List<BuildingSlot> GetBuildingSlots()
    {
        return buildingSlots;
    }
}
