using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Planet : SpaceBody
{
    [NonSerialized] public float orbitSpeed;
    [NonSerialized] public Vector3 orbitAxis;
    [NonSerialized] public Material material;
    [NonSerialized] public Star parentStar;

    private void Start()
    {
        GenerateDeposits();
    }

    void Update()
    {
        if (universe.timeRunning) transform.RotateAround(parentStar.transform.position, orbitAxis, orbitSpeed * Time.deltaTime);

        Vector3 lightDirection = Vector3.Normalize(parentStar.transform.position - transform.position);
        material.SetVector("_SunlightDirection", lightDirection);

        material.SetFloat("_TimeValue", universe.timeValue);
    }

    private void OnMouseDown()
    {
        if (!selected)
        {
            SetSelected(true);
            StartCoroutine(ScaleOverTime(hoverOver.transform, Vector3.zero, 0.3f));
            cameraMovementHandler.MoveToTarget(transform, transform.localScale.x * 7.5f, false, false);
        }
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
        collider.radius = selected ? 0.5f : 2.0f;

        if (selected)
        {
            universe.SetLastActivePlanetInactive();
            universe.SetActivePlanet(this);
        }
    }

    public void SetVisible(bool visible)
    {
        GetComponent<SphereCollider>().enabled = visible;
        GetComponent<MeshRenderer>().enabled = visible;
    }

    public void DrawOrbit(int steps, float radius, LineRenderer orbitRenderer)
    {
        orbitRenderer.transform.position = parentStar.transform.position;
        orbitRenderer.positionCount = steps;

        for (int currentStep = 0; currentStep < steps; currentStep++)
        {
            float circumferenceProgress = (float)currentStep / (steps - 1);

            float currentRadian = circumferenceProgress * 2 * Mathf.PI;

            float xScaled = Mathf.Cos(currentRadian);
            float yScaled = Mathf.Sin(currentRadian);

            float x = radius * xScaled;
            float y = 0.0f;
            float z = radius * yScaled;

            Vector3 currentPosition = new(x, y, z);

            orbitRenderer.SetPosition(currentStep, currentPosition);
        }
    }

    public void GenerateDeposits()
    {

    }
}
