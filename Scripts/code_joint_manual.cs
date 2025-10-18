using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class code_joint_manual : MonoBehaviour
{
    [Header("Motion")]
    [Tooltip("Degrees per second")]
    public float rotationSpeed = 45f;

    [Tooltip("Local axis to rotate around (Y = vertical in Unity).")]
    public Vector3 localAxis = Vector3.up;

    [Header("Trigger")]
    public bool requireKeyHold = false;
    public KeyCode triggerKey = KeyCode.Space;

    [Header("Rotation Limits")]
    public bool useLimits = true;

    [Tooltip("Minimum angle (in degrees) from the starting rotation.")]
    public float minAngle = -90f;

    [Tooltip("Maximum angle (in degrees) from the starting rotation.")]
    public float maxAngle = 90f;

    private float currentAngle = 0f;

    void Update()
    {
        // If rotation is gated by key input (e.g. Space bar)
        if (requireKeyHold && !Input.GetKey(triggerKey)) return;

        // --- NEW: Direction input using arrow keys ---
        float direction = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) direction = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) direction = -1f;

        if (direction == 0f) return; // No input

        float delta = rotationSpeed * Time.deltaTime;
        float nextAngle = currentAngle + delta * direction;

        // Clamp within limits
        if (useLimits)
        {
            if (nextAngle < minAngle || nextAngle > maxAngle) return;
        }

        transform.Rotate(localAxis, delta * direction, Space.Self);
        currentAngle = nextAngle;
    }
}
