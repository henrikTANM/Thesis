using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbiterTest : MonoBehaviour
{

    public Transform centre;
    public float orbitSpeed;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Orbit(transform.position, centre.position, Vector3.up, orbitSpeed * Time.fixedDeltaTime);
    }

    public Vector3 GetPosIn(float t)
    {
        Vector3 template = new(transform.position.x, transform.position.y, transform.position.z);
        return Orbit(template, centre.position, Vector3.up, orbitSpeed * t);
    }

    private Vector3 Orbit(Vector3 orbiterPos, Vector3 centre, Vector3 axis, float angle)
    {
        Quaternion quaternion = Quaternion.AngleAxis(angle, axis);
        Vector3 vector2 = orbiterPos - centre;
        vector2 = quaternion * vector2;
        return centre + vector2;
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
