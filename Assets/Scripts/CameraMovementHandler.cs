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

    private static bool universeView = true;

    private Vector3 newRotation;
    private Vector3 smoothRotation = Vector3.zero;

    [SerializeField] private Transform movemenetTarget;
    private static float distanceFromTarget = 3000.0f;

    private static Vector3 targetPosition;

    [SerializeField] private float moveSpeed = 1.5f;

    [NonSerialized] public bool movingToTarget = false;

    public static CameraMovementHandler instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        targetPosition = movemenetTarget.position - transform.forward * distanceFromTarget + transform.up * distanceFromTarget;
        transform.position = targetPosition;
        transform.LookAt(UniverseHandler.instance.transform);
        newRotation = transform.localEulerAngles;
    }

    void Update()
    {
        if (Input.GetMouseButton(1) & !movingToTarget & !UniverseHandler.escapeMenuDisplayed & !UniverseHandler.startWait)
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
        if (scrollWheelAxis != 0 & !universeView & !UniverseHandler.escapeMenuDisplayed & !UniverseHandler.startWait & !UIController.mouseOnSideMenu) // & universe.timeRunning)
        {
            //print(distanceFromTarget + " : " + target.localScale.x * 7.5f);
            float newDistanceFromTarget = distanceFromTarget + scrollWheelAxis * -distanceFromTarget;
            //distanceFromTarget = Mathf.Clamp(newDistanceFromTarget, target.localScale.x * 7.5f, target.localScale.x * 10.0f);
            distanceFromTarget = Mathf.Clamp(newDistanceFromTarget, movemenetTarget.localScale.x * 1.5f, movemenetTarget.localScale.x * 20.0f);
            ChangePosition();
        }

        if (movingToTarget) // & universe.timeRunning)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed);
            if (transform.position == targetPosition)
            {
                movingToTarget = false;
                UniverseHandler.startWait = false;
            }
        } 
        else
        {
            ChangePosition();
        }
    }

    private void ChangePosition()
    {
        targetPosition = movemenetTarget.position - transform.forward * distanceFromTarget;
        transform.position = targetPosition;
    }

    public static void MoveToTarget(Transform target, float distance, bool moveToUniverse)
    {
        instance.movemenetTarget = target;
        distanceFromTarget = distance;
        targetPosition = target.position - instance.transform.forward * distanceFromTarget;
        instance.movingToTarget = true;
        instance.moveSpeed = 3.0f * (Vector3.Distance(instance.transform.position, targetPosition)) * Time.deltaTime;

        if (universeView != moveToUniverse)
        {
            universeView = moveToUniverse;
            GameEvents.UniverseViewChange();
        }
    }

    public static void MoveToUniverseView() { MoveToTarget(UniverseHandler.instance.transform, 3000.0f, true); }
}
