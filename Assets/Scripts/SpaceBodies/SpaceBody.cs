using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpaceBody : MonoBehaviour
{
    private string name;

    public SpriteRenderer hoverOver;
    [NonSerialized] public CameraMovementHandler cameraMovementHandler;
    [NonSerialized] public UniverseHandler universe;
    [NonSerialized] public SphereCollider collider;
    [NonSerialized] public float nativeScale;
    [NonSerialized] public Canvas nameTagCanvas;
    [NonSerialized] public bool selected = false;
    [NonSerialized] public float scaleDownMultiplier = 0.1f;

    private void Awake()
    {
        hoverOver.transform.localScale = Vector3.zero;

        cameraMovementHandler = GameObject.Find("Main Camera").GetComponent<CameraMovementHandler>();
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
        collider = GetComponent<SphereCollider>();
        nameTagCanvas = GetComponentInChildren<Canvas>();

        collider.radius = 2.0f;

        InputEvents.OnClusterView += ScaleToNative;
    }

    private void OnDestroy()
    {
        InputEvents.OnClusterView -= ScaleToNative;
    }

    private void OnMouseEnter()
    {
        if (!Input.GetMouseButton(1) & !selected & !universe.UIMenuDisplayed()) 
            StartCoroutine(ScaleOverTime(hoverOver.transform, new Vector3(0.1f, 0.1f, 0.1f), 0.3f));
    }

    private void OnMouseExit()
    {
           StartCoroutine(ScaleOverTime(hoverOver.transform, Vector3.zero, 0.3f));
    }

    private void OnMouseOver()
    {
        if (!Input.GetMouseButton(1)) 
            hoverOver.transform.forward = -(cameraMovementHandler.transform.position - hoverOver.transform.position);
    }

    public IEnumerator ScaleOverTime(Transform objectToScale, Vector3 toScale, float duration)
    {
        float counter = 0.0f;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            objectToScale.localScale = Vector3.Lerp(objectToScale.localScale, toScale, counter / duration);
            yield return null;
        }
    }

    public string GetName()
    {
        return name;
    }

    public void SetName(string name)
    {
        this.name = name;
        TextMeshProUGUI nameText = nameTagCanvas.GetComponentInChildren<TextMeshProUGUI>();
        nameText.text = name;
    }

    public void ScaleToSize(float size)
    {
        StartCoroutine(ScaleOverTime(transform, new(size, size, size), 0.5f));
    }

    public void ScaleToNative()
    {
        ScaleToSize(nativeScale);
    }
}
