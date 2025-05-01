using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipMotionHandler : MonoBehaviour
{
    private ParticleSystem.MainModule mainPartycleSystem;
    public SpaceShipHandler spaceShipHandler;

    private UniverseHandler universe;
    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 flightDirection;

    private float halfDistance;
    private float acceleration;
    private float initialV = 0.0f;
    private float currentV;

    private bool breaking;

    private void Awake()
    {
        universe = GameObject.Find("Universe").GetComponent<UniverseHandler>();
        mainPartycleSystem = spaceShipHandler.boosterParticleSystem.main;
    }

    void FixedUpdate()
    {
        //print(universe.timeRunning);
        if (spaceShipHandler.IsMoving() & UniverseHandler.timeRunning)
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

            if (breaking & (currentV + deltaV <= 0.0f)) spaceShipHandler.EndMoving();
            else
            {
                currentV += deltaV;
                transform.Translate(currentV * Time.fixedDeltaTime * flightDirection);
            }
        }
    }

    public void StartMoving(Orbiter start, Orbiter end, float traveltime)
    {
        startPos = start.transform.position;
        endPos = end.GetPosIn(traveltime * UniverseHandler.instance.cycleLength);

        transform.position = startPos;
        flightDirection = Vector3.Normalize(endPos - startPos);
        breaking = false;

        spaceShipHandler.body.transform.localRotation = Quaternion.LookRotation(flightDirection);
        spaceShipHandler.body.transform.Rotate(new Vector3(0, 270, 0));

        currentV = initialV;
        halfDistance = Vector3.Distance(startPos, endPos) / 2.0f;
        //Debug.Log(halfDistance + " " + traveltime + " " + universe.cycleLength);
        acceleration = (2 * halfDistance) / Mathf.Pow((traveltime * UniverseHandler.instance.cycleLength) / 2.0f, 2);
    }
}
