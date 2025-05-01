using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceViewer : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset resourceFactorTemplate;

    public void MakeResourceViewer(Resource resource, Planet planet, Vector2 mousePos)
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        VisualElement resourceFactorList = root.Q<VisualElement>("resourcelist");
        resourceFactorList.style.top = new StyleLength(mousePos.y);
        resourceFactorList.style.left = new StyleLength(mousePos.x);


        List<ResourceFactor> resourceFactors = planet.GetPlanetResourceHandler().FindActiveResourceFactors(resource);

        if (resourceFactors.Count == 0)
        {
            VisualElement factor = resourceFactorTemplate.Instantiate();

            Label noFactorsLabel = factor.Q<Label>("name");
            noFactorsLabel.text = "No factors ";

            resourceFactorList.Add(factor);
        }

        foreach (ResourceFactor resourceFactor in resourceFactors)
        {
            VisualElement factor = resourceFactorTemplate.Instantiate();

            Label sourceName = factor.Q<Label>("name");
            sourceName.text = resourceFactor.resourceSource.name + ": " + (resourceFactor.resourceAmount.amount > 0 ? "+" : "") + resourceFactor.resourceAmount.amount;

            VisualElement resourceImage = factor.Q<VisualElement>("image");
            resourceImage.style.backgroundImage =
                    new StyleBackground(resourceFactor.resourceAmount.resource.resourceSprite);
            resourceImage.style.unityBackgroundImageTintColor =
                new StyleColor(resourceFactor.resourceAmount.resource.spriteColor);

            resourceFactorList.Add(factor);
        }
    }
}
