using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("Car Settings")]
    public float steeringSensitivity = 0.01f;
    public float lookAhead = 30;
    public float maxTorque = 200;
    public float maxSteerAngle = 60;
    public float maxBrakeTorque = 500;
    public float accelCornerMax = 20;
    public float brakeCornerMax = 10;
    public float accelVelocityThreshold = 20;
    public float brakeVelocityThreshold = 10;
    public float antiroll = 5000;
    public int fitness = 0;


    Drive[] ds;
    public Circuit circuit;
    Vector3 target;
    int currentWP = 0;
    Rigidbody rb;
    public GameObject brakelight;
    GameObject tracker;
    int currentTrackerWP = 0;
    AvoidDetector avoid;



    // Start is called before the first frame update
    void Start()
    {
        ds = this.GetComponentsInChildren<Drive>();
        circuit = GameObject.FindGameObjectWithTag("circuit").GetComponent<Circuit>();
        target = circuit.waypoints[currentWP].transform.position;
        rb = this.GetComponent<Rigidbody>();

        tracker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        DestroyImmediate(tracker.GetComponent<Collider>());
        tracker.GetComponent<MeshRenderer>().enabled = false;
        tracker.transform.position = this.transform.position;
        tracker.transform.rotation = this.transform.rotation;

        avoid = this.GetComponent<AvoidDetector>();

        this.GetComponent<AntiRoll>().antiRoll = antiroll;

        foreach (Drive drive in ds)
        {
            drive.maxTorque = maxTorque;
            drive.maxSteerAngle = maxSteerAngle;
            drive.maxBrakeTorque = maxBrakeTorque;
        }


    }


    float trackerSpeed = 15.0f;
    void ProgressTracker()
    {
        Debug.DrawLine(this.transform.position, tracker.transform.position);
        if (Vector3.Distance(this.transform.position, tracker.transform.position) > lookAhead)
        {
            trackerSpeed -= 1.0f;
            if (trackerSpeed < 2) trackerSpeed = 2;
            return;
        }

        if (Vector3.Distance(this.transform.position, tracker.transform.position) < lookAhead / 2.0f)
        {
            trackerSpeed += 1.0f;
            if (trackerSpeed > 15) trackerSpeed = 15;
        }

        tracker.transform.LookAt(circuit.waypoints[currentTrackerWP].transform.position);
        tracker.transform.Translate(0, 0, trackerSpeed * Time.deltaTime);

        if (Vector3.Distance(tracker.transform.position,
            circuit.waypoints[currentTrackerWP].transform.position) < 1)
        {
            currentTrackerWP++;
            fitness++;
            if (currentTrackerWP >= circuit.waypoints.Length)
                currentTrackerWP = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ProgressTracker();
        target = tracker.transform.position;

        Vector3 localTarget;
        

        if (Time.time < avoid.avoidTime)
        {

            localTarget = tracker.transform.right * avoid.avoidPath;
        }
        else
        {

            localTarget = this.transform.InverseTransformPoint(target);
        }

        //float distanceToTarget = Vector3.Distance(target, this.transform.position);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;


        float s = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(rb.linearVelocity.magnitude);

        float corner = Mathf.Clamp(Mathf.Abs(targetAngle), 0, 90);
        float cornerFactor = corner / 90.0f;

        float a = 1;
        if(corner > accelCornerMax && rb.linearVelocity.magnitude > accelVelocityThreshold)
            a = Mathf.Lerp(0, 1, 1 - cornerFactor);

        float b = 0;
        if (corner > brakeCornerMax && rb.linearVelocity.magnitude > brakeVelocityThreshold)
            b = Mathf.Lerp(0, 1, cornerFactor);

        if (avoid.reverse)
        {
            a = -1 * a;
            s = -1 * s;
        }

        for (int i = 0; i < ds.Length; i++)
        {
            ds[i].Go(a, s, b);
        }

        if (b > 0)
        {
            brakelight.SetActive(true);
        }
        else
        {
            brakelight.SetActive(false);
        }

        /*if (distanceToTarget < 4)
        {
            currentWP++;
            if (currentWP >= circuit.waypoints.Length)
                currentWP = 0;

            target = circuit.waypoints[currentWP].transform.position;
        }*/
    }
}

