using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBody : MonoBehaviour
{
    public SpriteRenderer hoverOver;
    [NonSerialized] public CameraMovementHandler cameraMovementHandler;
    [NonSerialized] public UniverseHandler universe;
    [NonSerialized] public SphereCollider collider;
    [NonSerialized] public bool selected = false;

    private void Awake() => hoverOver.transform.localScale = Vector3.zero;

    private void Start()
    {
        cameraMovementHandler = GameObject.Find("Main Camera").GetComponent<CameraMovementHandler>();
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
        collider = GetComponent<SphereCollider>();
        collider.radius = 2.0f;
    }

    private void OnMouseEnter()
    {
        if (!Input.GetMouseButton(1) & !selected & universe.timeRunning) StartCoroutine(ScaleOverTime(hoverOver.transform, new Vector3(0.1f, 0.1f, 0.1f), 0.3f));
    }

    private void OnMouseExit()
    {
        if (!Input.GetMouseButton(1) & !selected & universe.timeRunning) StartCoroutine(ScaleOverTime(hoverOver.transform, Vector3.zero, 0.3f));
    }

    private void OnMouseOver()
    {
        if (!Input.GetMouseButton(1) & universe.timeRunning) hoverOver.transform.forward = -(cameraMovementHandler.transform.position - hoverOver.transform.position);
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
}
