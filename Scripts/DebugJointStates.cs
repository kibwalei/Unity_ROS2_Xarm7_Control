using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

[DisallowMultipleComponent]
public class DebugJointStates : MonoBehaviour
{
    public string topicName = "/joint_states";

    // If empty in Inspector, script will auto-fill with all ArticulationBody children
    public ArticulationBody[] jointArticulations;

    ROSConnection ros;
    private Dictionary<string, ArticulationBody> jointMap = new Dictionary<string, ArticulationBody>();
    private Queue<JointStateMsg> incoming = new Queue<JointStateMsg>();
    private readonly object queueLock = new object();

    void Start()
    {
        ros = ROSConnection.instance;
        if (ros == null)
        {
            Debug.LogError("[DebugJointStates] No ROSConnection instance found in scene!");
            return;
        }

        // Subscribe
        ros.Subscribe<JointStateMsg>(topicName, JointStateCallback);
        Debug.Log($"[DebugJointStates] Subscribed to {topicName}");

        // Auto-fill articulation list if none assigned
        if (jointArticulations == null || jointArticulations.Length == 0)
        {
            jointArticulations = GetComponentsInChildren<ArticulationBody>();
            Debug.Log($"[DebugJointStates] Auto-filled {jointArticulations.Length} articulation bodies from children.");
        }
        else
        {
            Debug.Log($"[DebugJointStates] Inspector provided {jointArticulations.Length} articulation bodies.");
        }

        // Build name -> articulation dictionary (use GameObject names)
        jointMap.Clear();
        foreach (var ab in jointArticulations)
        {
            if (ab == null) continue;
            if (!jointMap.ContainsKey(ab.name))
                jointMap.Add(ab.name, ab);
            else
                Debug.LogWarning($"[DebugJointStates] Duplicate articulation name in Unity: {ab.name}");
        }

        Debug.Log("[DebugJointStates] Unity joint names: " + string.Join(", ", jointMap.Keys));
    }

    // This callback runs when a message arrives (thread may be different). We just enqueue.
    void JointStateCallback(JointStateMsg msg)
    {
        // quick lightweight log so you can see messages arriving
        Debug.Log($"[ROS CB] Received /joint_states ({msg.name.Length} names). First: {(msg.name.Length>0?msg.name[0]:"-")}");
        lock (queueLock)
        {
            incoming.Enqueue(msg);
        }
    }

    void Update()
    {
        JointStateMsg msg = null;
        lock (queueLock)
        {
            if (incoming.Count > 0) msg = incoming.Dequeue();
        }
        if (msg == null) return;

        // Full log of names (helpful)
        Debug.Log($"[Update] Processing joint_states with {msg.name.Length} entries: {string.Join(", ", msg.name)}");

        // detect unmapped names and try fuzzy mapping
        List<string> unmapped = new List<string>();
        for (int i = 0; i < msg.name.Length; i++)
        {
            string rosName = msg.name[i];
            var ab = FindJointByName(rosName);
            if (ab == null) unmapped.Add(rosName);
        }

        if (unmapped.Count > 0)
            Debug.LogWarning("[Update] Unmapped ROS joint names: " + string.Join(", ", unmapped));
        else
            Debug.Log("[Update] All ROS joint names have a Unity match (exact or fuzzy).");

        // Try to move the first mappable joint (adds +20 degrees to incoming position)
        for (int i = 0; i < msg.name.Length; i++)
        {
            string rosName = msg.name[i];
            var ab = FindJointByName(rosName);
            if (ab == null) continue;

            // compute desired target (incoming pos is in radians)
            float desiredDeg = (float)(msg.position.Length > i ? msg.position[i] * Mathf.Rad2Deg : 0.0f);

            // Add wiggle so movement is obvious for testing
            float wiggle = 20f;
            float newTargetDeg = desiredDeg + wiggle;

            var drive = ab.xDrive;
            // make sure drive has enough authority
            drive.stiffness = Mathf.Max(drive.stiffness, 1000f);
            drive.damping = Mathf.Max(drive.damping, 100f);
            // set target
            drive.target = newTargetDeg;
            ab.xDrive = drive;

            Debug.Log($"[Update] Setting joint '{rosName}' (Unity name '{ab.name}') target -> {newTargetDeg} deg (incoming {desiredDeg} deg).");
            // only move one joint for this debug iteration
            break;
        }
    }

    // Try exact match first, then fuzzy matches (contains / reverse contains)
    private ArticulationBody FindJointByName(string rosName)
    {
        if (string.IsNullOrEmpty(rosName)) return null;
        if (jointMap.ContainsKey(rosName)) return jointMap[rosName];

        // fuzzy: try to find Unity joint that contains ROS name (case-insensitive)
        string lowerRos = rosName.ToLowerInvariant();
        foreach (var kv in jointMap)
        {
            string unityName = kv.Key.ToLowerInvariant();
            if (unityName.Contains(lowerRos) || lowerRos.Contains(unityName))
                return kv.Value;
        }

        // no match
        return null;
    }
}

