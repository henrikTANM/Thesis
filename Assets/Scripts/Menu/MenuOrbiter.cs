using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MenuOrbiter : MonoBehaviour
{
    public Transform centre;
    public float orbitSpeed;
    public Texture2D texture;
    public Texture2D cloudsTexture;
    private Material material;
    private float timeValue = 0.0f;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
        material.SetTexture("_PlanetTexture", texture);
        if (cloudsTexture != null) { material.SetTexture("_CloudsTexture", cloudsTexture); }
    }

    void Update()
    {
        transform.position = Orbit(transform.position, centre.position, Vector3.up, orbitSpeed * Time.fixedDeltaTime);
        Vector3 lightDirection = Vector3.Normalize(centre.position - transform.position);
        material.SetVector("_SunlightDirection", lightDirection);
        timeValue += Time.deltaTime;
        material.SetFloat("_TimeValue", timeValue);
    }

    private Vector3 Orbit(Vector3 orbiterPos, Vector3 centre, Vector3 axis, float angle)
    {
        Quaternion quaternion = Quaternion.AngleAxis(angle, axis);
        Vector3 vector2 = orbiterPos - centre;
        vector2 = quaternion * vector2;
        return centre + vector2;
    }
}
