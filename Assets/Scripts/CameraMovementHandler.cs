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

    private bool isStar;
    private bool universeView = true;

    private Vector3 newRotation;
    private Vector3 smoothRotation = Vector3.zero;

    private UniverseHandler universe;

    [SerializeField]
    private Transform target;
    private float distanceFromTarget = 250.0f;

    Vector3 targetPosition;

    [SerializeField]
    private float moveSpeed = 1.5f;

    private bool movingToTarget = false;

    private void Awake()
    {
        InputEvents.OnClusterView += MoveToClusterView;
    }

    private void OnDestroy()
    {
        InputEvents.OnClusterView -= MoveToClusterView;
    }

    void Start()
    {
        ChangePosition();
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
    }

    void Update()
    {
        if (Input.GetMouseButton(1) & !movingToTarget & !universe.AreMenusDisplayed())
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
        if (scrollWheelAxis != 0 & !universeView & !universe.AreMenusDisplayed()) // & universe.timeRunning)
        {
            float newDistanceFromTarget = distanceFromTarget + scrollWheelAxis * -distanceFromTarget;
            distanceFromTarget = Mathf.Clamp(newDistanceFromTarget, target.localScale.x * 7.5f, target.localScale.x * 50.0f);
            ChangePosition();
        }

        if (movingToTarget) // & universe.timeRunning)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed);
            if (transform.position == targetPosition)
            {
                movingToTarget = false;
            }
        } 
        else
        {
            ChangePosition();
        }

    }

    public void MoveToTarget(Transform target, float distanceFromTarget, bool isStar, bool universeView)
    {
        this.target = target;
        this.distanceFromTarget = distanceFromTarget;
        this.isStar = isStar;
        this.universeView = universeView;
        targetPosition = target.position - transform.forward * distanceFromTarget;
        movingToTarget = true;
        moveSpeed = 5.0f * (Vector3.Distance(transform.position, targetPosition) / 100.0f);
    }

    void ChangePosition()
    {
        targetPosition = target.position - transform.forward * distanceFromTarget;
        transform.position = targetPosition;
    }

    void MoveToClusterView()
    {
        MoveToTarget(universe.transform, 250.0f, false, true);
    }
}
