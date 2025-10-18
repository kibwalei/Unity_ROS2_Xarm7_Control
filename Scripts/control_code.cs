using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JointControl
{
    [Header("Joint Settings")]
    public string jointName = "Joint";
    public Transform jointTransform;

    [Tooltip("Degrees per second")]
    public float rotationSpeed = 45f;

    [Tooltip("Local axis to rotate around (Y = vertical in Unity).")]
    public Vector3 localAxis = Vector3.up;

    [Header("Trigger Keys")]
    public KeyCode positiveKey = KeyCode.RightArrow;
    public KeyCode negativeKey = KeyCode.LeftArrow;

    [Header("Rotation Limits")]
    public bool useLimits = true;

    [Tooltip("Minimum angle (in degrees) from the starting rotation.")]
    public float minAngle = -90f;

    [Tooltip("Maximum angle (in degrees) from the starting rotation.")]
    public float maxAngle = 90f;

    [HideInInspector] public float currentAngle = 0f;
}

public class MultiJointController : MonoBehaviour
{
    [Header("Robot Joints")]
    public List<JointControl> joints = new List<JointControl>();

    void Update()
    {
        foreach (var joint in joints)
        {
            if (joint.jointTransform == null) continue;

            float direction = 0f;
            if (Input.GetKey(joint.positiveKey)) direction = 1f;
            else if (Input.GetKey(joint.negativeKey)) direction = -1f;

            if (direction == 0f) continue;

            float delta = joint.rotationSpeed * Time.deltaTime;
            float nextAngle = joint.currentAngle + delta * direction;

            if (joint.useLimits)
            {
                if (nextAngle < joint.minAngle || nextAngle > joint.maxAngle)
                    continue;
            }

            joint.jointTransform.Rotate(joint.localAxis, delta * direction, Space.Self);
            joint.currentAngle = nextAngle;
        }
    }
}
