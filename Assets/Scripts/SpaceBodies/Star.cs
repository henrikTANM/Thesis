using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Star : SpaceBody
{
    public GameObject starBody;
    [NonSerialized] public List<Planet> planets;
    [NonSerialized] public Material material;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void Update()
    {
        nameTagCanvas.transform.LookAt(cameraMovementHandler.transform);
        nameTagCanvas.transform.Rotate(new(0.0f, 180.0f, 0.0f));

        material.SetFloat("_TimeValue", universe.timeValue);
    }

    private void OnMouseDown()
    {
        if (!selected & (universe.routeMakerDisplayed | !universe.UIDisplayed()))
        {
            SetSelected(true, false);
            StartCoroutine(ScaleOverTime(hoverOver.transform, Vector3.zero, 0.3f));
            cameraMovementHandler.MoveToTarget(transform, nativeScale * 7.5f, false);
        }
    }

    public void ScalePlanetsToNative()
    {
        foreach (Planet planet in planets) planet.ScaleToNative();
    }

    public void SetSelected(bool selected, bool universeView)
    {   
        this.selected = selected;
        collider.radius = selected ? 0.5f : 2.0f;
        nameTagCanvas.enabled = selected;

        foreach (Planet planet in planets) planet.SetVisible(selected);
        foreach (LineRenderer lineRenderer in GetComponentsInChildren<LineRenderer>()) lineRenderer.enabled = selected;

        if (selected)
        {
            ScaleToNative();
            SetStarBodyScale(1.0f);
            foreach (Star star in universe.stars) if (star != this) star.SetSelected(false, false);
        }
        else if (!universeView) ScaleToSize(0.25f);
        else SetStarBodyScale(1.0f);
    }

    public void SetStarBodyScale(float scale)
    {
        starBody.transform.localScale = new(scale, scale, scale);
    }
}
