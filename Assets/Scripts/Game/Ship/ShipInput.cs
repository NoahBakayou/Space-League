using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipInput : MonoBehaviour
{
    public float Throttle { get; private set; }
    public float Strafe { get; private set; }
    public float Rotate { get; private set; }
    public bool AfterburnerHeld { get; private set; }

    public Vector2 MousePosition => Mouse.current.position.ReadValue();

    void Update()
    {
        // Throttle control
        if (Input.GetKey(KeyCode.W))
        {
            Throttle = Mathf.MoveTowards(Throttle, 1f, Time.deltaTime * 1.5f); 
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Throttle = Mathf.MoveTowards(Throttle, 0f, Time.deltaTime * 1.5f); 
        }

        // Strafe A/D
        float strafeInput = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            strafeInput = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            strafeInput = 1f; 
        }
        Strafe = strafeInput;

        // Rotate Q/E
        float rotateInput = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            rotateInput = -1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotateInput = 1f;
        }
        Rotate = rotateInput;

        // Afterburner
        AfterburnerHeld = Input.GetKey(KeyCode.Space);
    }
}

