using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : Star
{
    [SerializeField] private ParticleSystem accretionDisk;

    void Start()
    {
        accretionDisk.scalingMode = ParticleSystemScalingMode.Hierarchy;
        discoveryBubble.GetComponent<MeshRenderer>().enabled = false;
    }

    void Update()
    {
        nameTagCanvas.transform.LookAt(CameraMovementHandler.instance.transform);
        nameTagCanvas.transform.Rotate(new(0.0f, 180.0f, 0.0f));

        collider.radius = body.transform.localScale.x;
    }

    public override void SetSelected(bool selected, UniverseHandler.StateChange stateChange)
    {
        base.SetSelected(selected, stateChange);
    }
}
