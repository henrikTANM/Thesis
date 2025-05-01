using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Star : SpaceBody
{
    public GameObject discoveryBubble;
    public int discoveryProgressSpeed = 1;
    public float discoveryProgressFactor = 15.0f;
    public float scaleDownMultiplier = 0.2f;
    public float scaleUpMultiplier = 4.0f;
    [NonSerialized] public int discoveryProgress = 0;
    [NonSerialized] public bool hasDiscovery = false;
    [NonSerialized] public bool discovered = false;

    [NonSerialized] public List<Planet> planets;
    [NonSerialized] public Material material;

    protected override void Awake()
    {
        base.Awake();
        discoveryBubble.GetComponent<MeshRenderer>().enabled = false;
    }

    private void Update()
    {
        nameTagCanvas.transform.LookAt(CameraMovementHandler.instance.transform);
        nameTagCanvas.transform.Rotate(new(0.0f, 180.0f, 0.0f));

        material.SetFloat("_TimeValue", UniverseHandler.timeValue);
        collider.radius = body.transform.localScale.x;

        if (Input.GetKeyDown(UniverseHandler.GetKeyCode(KeyBind.KeyPressAction.TOGGLE_TELESCOPE)) & (hasDiscovery | discoveryProgress > 0)) 
        {
            discoveryBubble.GetComponent<MeshRenderer>().enabled = !discoveryBubble.GetComponent<MeshRenderer>().enabled;
        }
    }

    private void OnMouseDown()
    {
        if (UniverseHandler.destinationPickerDisplayed | !UIController.UIDisplayed())
        {
            UniverseHandler.AddMoveToStar(this);
            StartCoroutine(ScaleOverTime(hoverOver.transform, Vector3.zero, 0.3f));
        }
    }

    public override bool IsStar() { return true; }
    public override bool IsPlanet() { return false; }

    public virtual void SetSelected(bool selected, UniverseHandler.StateChange stateChange)
    { 
        if (this.selected != selected)
        {
            this.selected = selected;

            foreach (Planet planet in planets) planet.SetVisible(selected);
            foreach (LineRenderer lineRenderer in GetComponentsInChildren<LineRenderer>()) lineRenderer.enabled = selected;
        }

        if (selected)
        {
            if (stateChange.Equals(UniverseHandler.StateChange.UNIVERSE_TO_STAR) |
                stateChange.Equals(UniverseHandler.StateChange.STAR_TO_STAR) |
                stateChange.Equals(UniverseHandler.StateChange.PLANET_TO_OTHER_STAR))
            {
                ScaleToSize(nativeScale, false);
                UniverseHandler.instance.selectedStar = this;
                foreach (Star star in UniverseHandler.stars) if (star != this) star.SetSelected(false, stateChange);
            }

            if (stateChange.Equals(UniverseHandler.StateChange.STAR_TO_PLANET) |
                stateChange.Equals(UniverseHandler.StateChange.STAR_TO_OTHER_PLANET) |
                stateChange.Equals(UniverseHandler.StateChange.PLANET_TO_OTHER_PLANET) |
                stateChange.Equals(UniverseHandler.StateChange.UNIVERSE_TO_PLANET))
            {
                ScaleToSize(nativeScale * scaleDownMultiplier, false);
                UniverseHandler.instance.selectedStar = this;
            }

            if (stateChange.Equals(UniverseHandler.StateChange.PLANET_TO_STAR))
            {
                ScaleToSize(nativeScale, false);
                foreach (Planet planet in planets) planet.SetSelected(false, stateChange);
            }
        }
        else
        {

            if (stateChange.Equals(UniverseHandler.StateChange.UNIVERSE_TO_STAR) |
                stateChange.Equals(UniverseHandler.StateChange.STAR_TO_STAR) |
                stateChange.Equals(UniverseHandler.StateChange.STAR_TO_OTHER_PLANET)) 
                ScaleToSize(nativeScale * scaleDownMultiplier, false);

            if (stateChange.Equals(UniverseHandler.StateChange.STAR_TO_UNIVERSE))
            {
                UniverseHandler.instance.selectedStar = null;
                ScaleToSize(nativeScale * scaleUpMultiplier, false);
            }

            if (stateChange.Equals(UniverseHandler.StateChange.PLANET_TO_OTHER_STAR) |
                stateChange.Equals(UniverseHandler.StateChange.PLANET_TO_OTHER_PLANET))
            {
                ScaleToSize(nativeScale * scaleDownMultiplier, false);
                foreach (Planet planet in planets) planet.SetSelected(false, stateChange);
            }

            if (stateChange.Equals(UniverseHandler.StateChange.PLANET_TO_UNIVERSE))
            {
                UniverseHandler.instance.selectedStar = null;
                ScaleToSize(nativeScale * scaleUpMultiplier, false);
                foreach (Planet planet in planets) planet.SetSelected(false, stateChange);
            }
        }
    }

    public void HandleDiscovery(DiscoveryHubHandler discoveryHubHandler)
    {
        discoveryProgress += discoveryProgressSpeed;
        float discoveryBubbleScaleFactor = discoveryProgressSpeed * discoveryProgressFactor;
        discoveryBubble.transform.localScale += new Vector3(discoveryBubbleScaleFactor, discoveryBubbleScaleFactor, discoveryBubbleScaleFactor);

        if (discoveryProgress >= 100)
        {
            discoveryProgress = 100;
            discoveryHubHandler.active = false;
        }

        Mesh mesh = discoveryBubble.GetComponent<MeshFilter>().mesh;
        Vector3 vert = Vector3.Scale(mesh.vertices[0], discoveryBubble.transform.localScale);
        float bubbleRadius = Vector3.Distance(transform.position, vert);
        foreach (Star star in UniverseHandler.stars)
        {
            if (star != this & !star.discovered & Vector3.Distance(transform.position, star.transform.position) <= bubbleRadius) star.SetDiscovered(true, true);
        }
    }

    public void SetDiscovered(bool discovered, bool sendMessage)
    {
        if (this.discovered != discovered & sendMessage)
        {
            UIController.AddMessage(new Message(
                "New star discovered: " + name,
                Message.MessageType.NOTIFICATION,
                new MessageSender<Star>(this),
                Message.SenderType.STAR
                )); 
        }
        this.discovered = discovered;
        body.SetActive(discovered);
        discoveryBubble.SetActive(discovered);
    }
}
