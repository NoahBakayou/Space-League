using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShipMovement : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 30f;
    public float maxSpeed = 50f;
    public float rotationSpeed = 60f;

    [Header("Strafing")]
    public float strafeSpeed = 25f;
    public float strafePowerCost = 10f;

    [Header("Afterburner")]
    public float afterburnerMultiplier = 2.5f;
    public float afterburnerCostPerSecond = 15f;

    [Header("Reserve Power")]
    public float maxPower = 100f;
    public float regenDelay = 2f;
    public float regenRate = 20f;

    private float currentPower;
    private float regenTimer;
    private Rigidbody rb;
    private Camera cam;
    private ShipInput input;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        input = GetComponent<ShipInput>();
        currentPower = maxPower; 
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleRotationToCursor();
        HandleStrafe();
        HandleAfterburner();
        RegeneratePower(); 
    }

    void HandleMovement()
    {
        float speedLimit = maxSpeed;
        if (input.AfterburnerHeld && currentPower > 0f)
        {
            speedLimit *= afterburnerMultiplier; 
        }

        Vector3 force = transform.forward * (input.Throttle * acceleration);
        rb.AddForce(force, ForceMode.Acceleration);
        if (rb.velocity.magnitude > speedLimit)
        {
            rb.velocity = rb.velocity.normalized * speedLimit;
        }
    }

    void HandleRotationToCursor()
    {
        Ray ray = cam.ScreenPointToRay(input.MousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
        {
            Vector3 lookDir = hit.point - transform.position;
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }

        // Add Q/E axial rotation
        rb.AddRelativeTorque(Vector3.forward * -input.Rotate * rotationSpeed); 
    }

    void HandleStrafe() // Each strafe will consume a set amount of reserve power
    {
        if (Mathf.Abs(input.Strafe) < 0.01f) return;
        if (currentPower <= 0f) return;

        rb.AddRelativeForce(Vector3.right * input.Strafe * strafeSpeed, ForceMode.Acceleration);

        currentPower -= strafePowerCost * Time.fixedDeltaTime;
        regenTimer = regenDelay;
    }

    void HandleAfterburner() // While activated, afterburner will continuously consume reserve power
    {
        if (!input.AfterburnerHeld) return;
        if (currentPower <= 0f) return;

        currentPower -= afterburnerCostPerSecond * Time.fixedDeltaTime;
        regenTimer = regenDelay; 
    }

    void RegeneratePower() // Calculating reserve power regeneration 
    {
        if (regenTimer > 0f)
        {
            regenTimer -= Time.deltaTime;
            return; 
        }

        currentPower = Mathf.MoveTowards(currentPower, maxPower, regenRate * Time.deltaTime); 
    }
}
