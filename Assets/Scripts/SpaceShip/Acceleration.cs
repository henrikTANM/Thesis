using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acceleration : MonoBehaviour
{
    [SerializeField] private OrbiterTest start;
    [SerializeField] private OrbiterTest end;
    private Vector3 startPos;
    private Vector3 endPos;
    // Start is called before the first frame update
    private float halfDistance;
    private float acceleration;
    private float initialV;
    private float currentV;
    private bool breaking;
    private Vector3 flightDirection;

    public float traveltime;

    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Vector3.Distance(transform.position, endPos) <= halfDistance) breaking = true;

        float deltaV = (breaking ? -acceleration : acceleration) * Time.fixedDeltaTime;

        if (breaking & (currentV + deltaV <= 0.0f)) {
            SwitchStartEnd();
            Initialize();
        } 
        else
        {
            currentV += deltaV;
            //print(transform.position);
            //print(flightDirection * currentV * Time.fixedDeltaTime);
            transform.Translate(flightDirection * currentV * Time.fixedDeltaTime);
        }

    }

    private void SwitchStartEnd()
    {
        OrbiterTest placeHolder = start;
        start = end;
        end = placeHolder;
    }

    private void Initialize()
    {
        startPos = start.transform.position;
        endPos = end.GetPosIn(traveltime);

        Vector3 startCentrePos = start.GetCentrePos();
        Vector3 endCentrePos = end.GetCentrePos();

        float xzDistance = start.DistanceFromCentre() + end.DistanceFromCentre() + Vector2.Distance(new(startCentrePos.x, startCentrePos.z), new(endCentrePos.x, endCentrePos.z));
        float yDistance = Mathf.Abs(startCentrePos.y - endCentrePos.y);
        float maxDistance = Mathf.Sqrt(Mathf.Pow(xzDistance, 2) + Mathf.Pow(yDistance, 2));

        transform.position = startPos;
        //print(startPos + " : " + endPos);
        flightDirection = Vector3.Normalize(endPos - startPos);
        //print(flightDirection);
        breaking = false;

        currentV = initialV;
        halfDistance = Vector3.Distance(startPos, endPos) / 2.0f;
        float halfTravelTime = traveltime / 2.0f;
        acceleration = ((2 * halfDistance) / Mathf.Pow(halfTravelTime, 2));
    }
}
