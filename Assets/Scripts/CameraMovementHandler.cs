using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CameraMovementHandler : MonoBehaviour
{
    [SerializeField]
    private float mouseSensitivity = 3.0f;

    private float rotationX;
    private float rotationY;

    private bool universeView = true;

    private Vector3 newRotation;
    private Vector3 smoothRotation = Vector3.zero;

    private UniverseHandler universe;

    [SerializeField] private Transform target;
    private float distanceFromTarget = 5000.0f;

    Vector3 targetPosition;

    [SerializeField] private float moveSpeed = 1.5f;

    private bool movingToTarget = false;

    private void Awake()
    {
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();

        InputEvents.OnClusterView += MoveToClusterView;
        InputEvents.OnSystemView += MoveToParentStar;
    }

    private void OnDestroy()
    {
        InputEvents.OnClusterView -= MoveToClusterView;
        InputEvents.OnSystemView -= MoveToParentStar;
    }

    void Start()
    {
        targetPosition = target.position - transform.forward * distanceFromTarget + transform.up * distanceFromTarget;
        transform.position = targetPosition;
        transform.LookAt(universe.transform);
        newRotation = transform.localEulerAngles;
    }

    void Update()
    {
        if (Input.GetMouseButton(1) & !movingToTarget & !universe.escapeMenuDisplayed & !universe.startWait)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            rotationY += mouseX;
            rotationX += mouseY;

            Vector3 nextRotation = new(rotationX, -rotationY, 0);
            newRotation = Vector3.SmoothDamp(newRotation, nextRotation, ref smoothRotation, 0.3f);
            transform.localEulerAngles = newRotation;
            ChangePosition();
        }

        float scrollWheelAxis = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheelAxis != 0 & !universeView & !universe.escapeMenuDisplayed & !universe.startWait) // & universe.timeRunning)
        {
            //print(distanceFromTarget + " : " + target.localScale.x * 7.5f);
            float newDistanceFromTarget = distanceFromTarget + scrollWheelAxis * -distanceFromTarget;
            //distanceFromTarget = Mathf.Clamp(newDistanceFromTarget, target.localScale.x * 7.5f, target.localScale.x * 10.0f);
            distanceFromTarget = Mathf.Clamp(newDistanceFromTarget, target.localScale.x * 2.5f, target.localScale.x * 20.0f);
            ChangePosition();
        }

        if (movingToTarget) // & universe.timeRunning)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed);
            if (transform.position == targetPosition)
            {
                movingToTarget = false;
                universe.startWait = false;
            }
        } 
        else
        {
            ChangePosition();
        }

    }

    public void MoveToTarget(Transform target, float distanceFromTarget, bool universeView)
    {
        this.target = target;
        this.distanceFromTarget = distanceFromTarget;
        if (this.universeView != universeView) GameEvents.UniverseViewChange();
        this.universeView = universeView;
        targetPosition = target.position - transform.forward * distanceFromTarget;
        movingToTarget = true;
        moveSpeed = 3.0f * (Vector3.Distance(transform.position, targetPosition)) * Time.deltaTime;
    }

    void ChangePosition()
    {
        targetPosition = target.position - transform.forward * distanceFromTarget;
        transform.position = targetPosition;
    }

    void MoveToParentStar()
    {
        Planet currentPlanet = universe.GetActivePlanet();
        if (currentPlanet != null)
        {
            Star parentStar = universe.GetActivePlanet().parentStar;
            universe.SetLastActivePlanetInactive();
            MoveToTarget(parentStar.transform, parentStar.nativeScale, false);
            parentStar.ScalePlanetsToNative();
            parentStar.ScaleToSize(1.0f, false);
        }
    }

    void MoveToClusterView()
    {
        MoveToTarget(universe.transform, 2500.0f, true);
    }
}
