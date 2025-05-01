using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Orbiter : MonoBehaviour
{
    private UniverseHandler universe;

    private Transform centre;
    private float orbitSpeed;

    private void Awake()
    {
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
    }

    void Update()
    {
        if (UniverseHandler.timeRunning) transform.position = Orbit(transform.position, centre.position, Vector3.up, orbitSpeed * Time.deltaTime);
    }

    public Vector3 GetPosIn(float t)
    {
        Vector3 template = new(transform.position.x, transform.position.y, transform.position.z);
        return Orbit(template, centre.position, Vector3.up, orbitSpeed * t);
    }

    private Vector3 Orbit(Vector3 orbiterPos, Vector3 centre, Vector3 axis, float angle)
    {
        Quaternion quaternion = Quaternion.AngleAxis(angle, axis);
        Vector3 vector3 = orbiterPos - centre;
        vector3 = quaternion * vector3;
        return centre + vector3;
    }

    public float DistanceFromCentre()
    {
        return Vector3.Distance(transform.position, centre.position);
    }

    public Vector3 GetCentrePos()
    {
        return centre.position;
    }

    public void SetCentre(Transform transform)
    {
        centre = transform;
    }

    public void SetOrbitSpeed(float orbitSpeed)
    {
        this.orbitSpeed = orbitSpeed;
    }

}
