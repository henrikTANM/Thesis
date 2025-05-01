using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpaceBody : MonoBehaviour
{
    public GameObject body;

    [NonSerialized] public string name;

    public SpriteRenderer hoverOver;
    [NonSerialized] public SphereCollider collider;
    [NonSerialized] public float nativeScale;
    [NonSerialized] public Canvas nameTagCanvas;
    [NonSerialized] public bool selected = false;

    protected virtual void Awake()
    {
        hoverOver.transform.localScale = Vector3.zero;

        collider = GetComponentInChildren<SphereCollider>();
        nameTagCanvas = GetComponentInChildren<Canvas>();

        collider.radius = 2.0f;
    }

    protected virtual void OnMouseEnter()
    {
        hoverOver.color = Color.white;
        if (!Input.GetMouseButton(1) & !selected & (UniverseHandler.destinationPickerDisplayed | !UIController.UIDisplayed())) 
            StartCoroutine(ScaleOverTime(hoverOver.transform, new Vector3(0.1f, 0.1f, 0.1f), 0.1f));
    }

    protected virtual void OnMouseExit()
    {
        StartCoroutine(ScaleOverTime(hoverOver.transform, Vector3.zero, 0.1f));
        hoverOver.color = Color.white;
    }

    private void OnMouseOver()
    {
        if (!Input.GetMouseButton(1)) 
            hoverOver.transform.forward = -(CameraMovementHandler.instance.transform.position - hoverOver.transform.position);
    }

    public virtual bool IsStar() { return false; }
    public virtual bool IsPlanet() { return false; }

    public void SetName(string name)
    {
        this.name = name;
        TextMeshProUGUI nameText = nameTagCanvas.GetComponentInChildren<TextMeshProUGUI>();
        nameText.text = name;
        nameTagCanvas.enabled = false;
    }

    public void ScaleToSize(float size, bool overTime)
    {
        if (overTime) StartCoroutine(ScaleOverTime(body.transform, new(size, size, size), 0.5f));
        else body.transform.localScale = new(size, size, size);
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
