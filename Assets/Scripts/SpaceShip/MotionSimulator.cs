using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSimulator : MonoBehaviour
{
    private ParticleSystem.MainModule mainPartycleSystem;
    public SpaceShip ship;

    private UniverseHandler universe;
    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 flightDirection;

    private float halfDistance;
    private float acceleration;
    private float initialV = 0.0f;
    private float currentV;

    private bool breaking;
    private bool moving;

    private void Awake()
    {
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
        mainPartycleSystem = ship.booster.main;
        SetMoving(false);
    }

    void FixedUpdate()
    {
        if (moving)
        {
            if (Vector3.Distance(transform.position, endPos) <= halfDistance)
            {
                mainPartycleSystem.startLifetime = Vector3.Distance(transform.position, endPos) / halfDistance;
                //body.transform.localRotation = Quaternion.LookRotation(flightDirection);
                breaking = true;
            } 
            else
            {
                mainPartycleSystem.startLifetime = Vector3.Distance(transform.position, startPos) / halfDistance;
            }

            float deltaV = (breaking ? -acceleration : acceleration) * Time.fixedDeltaTime;

            if (breaking & (currentV + deltaV <= 0.0f))
            {
                ship.SetTravelling(false);
                GameEvents.ShipStateChange();
                SetMoving(false);
            }
            else
            {
                currentV += deltaV;
                transform.Translate(currentV * Time.fixedDeltaTime * flightDirection);
            }
        }
    }

    public void StartMoving(Orbiter start, Orbiter end, int traveltime)
    {
        startPos = start.transform.position + Vector3.up / 5;
        endPos = end.GetPosIn(traveltime * universe.cycleLength) + Vector3.up / 5;

        transform.position = startPos;
        flightDirection = Vector3.Normalize(endPos - startPos);
        breaking = false;

        ship.body.transform.localRotation = Quaternion.LookRotation(flightDirection);

        currentV = initialV;
        halfDistance = Vector3.Distance(startPos, endPos) / 2.0f;
        acceleration = (2 * halfDistance) / Mathf.Pow((traveltime * universe.cycleLength) / 2.0f, 2);

        SetMoving(true);
    }

    private void SetMoving(bool isMoving)
    {
        ship.EnableEffects(isMoving);
        moving = isMoving;
    }

    public bool IsMoving()
    {
        return moving;
    }
}
