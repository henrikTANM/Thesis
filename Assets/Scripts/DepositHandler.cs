using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepositHandler : MonoBehaviour
{
    private string name;
    private Sprite depositSprite;
    private List<ProductionBuilding> possibleproductionBuildings;
    private int buildingCap;

    private List<ProductionBuildingHandler> productionBuildings;

    public string GetName()
    {
        return name;
    }

    public Sprite GetDepositSprite()
    {
        return depositSprite;
    }

    public List<ProductionBuilding> GetPossibleProductionBuildings()
    {
        return possibleproductionBuildings;
    }

    public int GetBuildingCap()
    {
        return buildingCap;
    }

    public void SetName(string name)
    {
        this.name = name;
    } 

    public void SetDepositSprite(Sprite depositSprite)
    {
        this.depositSprite = depositSprite;
    } 

    public void SetPossibleProductionBuildings(List<ProductionBuilding> possibleProductionBuildings)
    {
        this.possibleproductionBuildings = possibleProductionBuildings;
    } 

    public void SetBuildingCap(int buildingCap)
    {
        this.buildingCap = buildingCap;
    }
}
