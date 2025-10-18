using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class XarmJointStateSubscriber : MonoBehaviour
{
    ROSConnection ros;

    [Header("ROS Settings")]
    public string topicName = "/joint_states";

    [Header("Joint Transforms (Match ROS joint order)")]
    public Transform[] jointTransforms; // link1...link7 in correct order
    public string[] rosJointNames;      // "joint1", "joint2", ..., "joint7"

    [Header("Rotation Axis per Joint (local axes)")]
    public Vector3[] rotationAxes; // One per joint — normalized

    [Header("Joint6 Offset")]
    public float joint6OffsetDegrees = 90f; // 90 degree offset for joint6

    [Header("Debug")]
    public bool printAngles = false;

    private Quaternion[] initialLocalRotations; // store rest rotations

    void Start()
    {
        ros = ROSConnection.instance;

        if (jointTransforms.Length != rosJointNames.Length ||
            rotationAxes.Length != rosJointNames.Length)
        {
            Debug.LogError("[XarmJointStateSubscriber] Array lengths mismatch!");
            enabled = false;
            return;
        }

        // Cache initial orientations
        initialLocalRotations = new Quaternion[jointTransforms.Length];
        for (int i = 0; i < jointTransforms.Length; i++)
        {
            initialLocalRotations[i] = jointTransforms[i].localRotation;
        }

        ros.Subscribe<JointStateMsg>(topicName, JointStateCallback);
        Debug.Log($"[XarmJointStateSubscriber] Subscribed to {topicName}");
    }

    void JointStateCallback(JointStateMsg msg)
    {
        for (int i = 0; i < rosJointNames.Length; i++)
        {
            int idx = System.Array.IndexOf(msg.name, rosJointNames[i]);
            if (idx >= 0 && idx < msg.position.Length)
            {
                double jointPos = msg.position[idx]; // radians
                float angleDeg = (float)(jointPos * Mathf.Rad2Deg);
                
                // Apply 90-degree offset to joint6
                if (rosJointNames[i] == "joint6")
                {
                    angleDeg += joint6OffsetDegrees;
                }
                
                ApplyJointRotation(i, angleDeg);

                if (printAngles)
                    Debug.Log($"[Joint {rosJointNames[i]}] {angleDeg:F2}°");
            }
        }
    }

    void ApplyJointRotation(int jointIndex, float angleDeg)
    {
        Transform joint = jointTransforms[jointIndex];
        if (joint == null) return;

        Vector3 axis = rotationAxes[jointIndex].normalized;
        if (axis == Vector3.zero)
        {
            Debug.LogWarning($"[Joint {rosJointNames[jointIndex]}] Rotation axis not set!");
            return;
        }

        // Apply rotation relative to the rest pose
        Quaternion deltaRotation = Quaternion.AngleAxis(angleDeg, axis);
        joint.localRotation = initialLocalRotations[jointIndex] * deltaRotation;
    }
}