using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Star : SpaceBody
{
    [NonSerialized] public List<Planet> planets;
    [NonSerialized] public float nativeScale;
    [NonSerialized] public Material material;

    private void Update()
    {
        material.SetFloat("_TimeValue", universe.timeValue);
    }

    private void OnMouseDown()
    {
        if (!selected & !universe.AreMenusDisplayed())
        {
            SetSelected(true, false);
            StartCoroutine(ScaleOverTime(hoverOver.transform, Vector3.zero, 0.3f));
            cameraMovementHandler.MoveToTarget(transform, nativeScale * 7.5f, true, false);
        }
    }

    public void SetSelected(bool selected, bool universeView)
    {   
        this.selected = selected;
        collider.radius = selected ? 0.5f : 2.0f;

        foreach (Planet planet in planets) planet.SetVisible(selected);

        foreach (LineRenderer lineRenderer in GetComponentsInChildren<LineRenderer>()) lineRenderer.enabled = selected;

        if (selected)
        {
            universe.SetActiveStar(this);
            StartCoroutine(ScaleOverTime(transform, new Vector3(nativeScale, nativeScale, nativeScale), 1.5f));
            foreach (Star star in universe.stars) if (star != this) star.SetSelected(false, false);
        }
        else if (!universeView) StartCoroutine(ScaleOverTime(transform, new Vector3(0.25f, 0.25f, 0.25f), 1.5f));
        else StartCoroutine(ScaleOverTime(transform, new Vector3(nativeScale, nativeScale, nativeScale), 0.5f));
    }
}
