using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class MenuStar : MonoBehaviour
{
    private Material material;
    private float timeValue = 0.0f;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        timeValue += Time.deltaTime;
        material.SetFloat("_TimeValue", timeValue);
    }
}
