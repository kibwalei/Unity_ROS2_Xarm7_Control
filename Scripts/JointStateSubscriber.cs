using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class JointStateSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public ArticulationBody[] jointArticulations;
    public string topicName = "/unity_joint_states";

    void Start()
    {
        ros = ROSConnection.instance;
        ros.Subscribe<JointStateMsg>(topicName, JointStateCallback);
    }

    void JointStateCallback(JointStateMsg msg)
    {
        if (msg.position.Length != jointArticulations.Length)
        {
            Debug.LogWarning("Received joint count doesn't match robot joint count.");
            return;
        }

        for (int i = 0; i < jointArticulations.Length; i++)
        {
            var drive = jointArticulations[i].xDrive;
            drive.target = Mathf.Rad2Deg * (float)msg.position[i]; // convert radians to degrees
            jointArticulations[i].xDrive = drive;
        }
    }
}
