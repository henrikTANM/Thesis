using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MoneyViewer : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset moneyFactorTemplate;

    public void MakeMoneyViewer(Vector2 mousePos)
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        VisualElement moneyFactorList = root.Q<VisualElement>("resourcelist");
        moneyFactorList.style.top = new StyleLength(mousePos.y);
        moneyFactorList.style.left = new StyleLength(mousePos.x);

        List<ResourceFactor> moneyFactors = PlayerInventory.FindActiveMoneyFactors();

        if (moneyFactors.Count == 0)
        {
            VisualElement factor = moneyFactorTemplate.Instantiate();

            Label noFactorsLabel = factor.Q<Label>("name");
            noFactorsLabel.text = "No factors ";

            moneyFactorList.Add(factor);
        }

        Dictionary<string, List<int>> moneyFactorMap = new();

        foreach (ResourceFactor moneyFactor in moneyFactors)
        {
            string name = moneyFactor.resourceSource.name;
            if (!moneyFactorMap.ContainsKey(name)) moneyFactorMap.Add(name, new());
            moneyFactorMap[name].Add(moneyFactor.resourceAmount.amount);
        }

        foreach (string factorName in moneyFactorMap.Keys)
        {
            int amountSum = 0;
            foreach (int amount in moneyFactorMap[factorName]) amountSum += amount;

            VisualElement factor = moneyFactorTemplate.Instantiate();

            Label sourceName = factor.Q<Label>("name");
            sourceName.text =  factorName + " x " + moneyFactorMap[factorName].Count + ": " + (amountSum > 0 ? "+" : "") + amountSum;

            VisualElement moneyImage = factor.Q<VisualElement>("image");
            moneyImage.style.backgroundImage =
                    new StyleBackground(PlayerInventory.instance.moneyResource.resourceSprite);
            moneyImage.style.unityBackgroundImageTintColor =
                new StyleColor(PlayerInventory.instance.moneyResource.spriteColor);

            moneyFactorList.Add(factor);
        }
    }
}
