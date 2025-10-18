using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class UnityToRosTargetSender : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/unity_target_pose";
    public Transform rosBase;            // assign ROS base_link GameObject (root of robot in Unity)
    public Transform[] targetPoints;     // assign your empty targets here
    int currentTargetIndex = 0;

    void Start()
    {
        ros = ROSConnection.instance;
        ros.RegisterPublisher<PoseStampedMsg>(topicName);
        if (rosBase == null)
            Debug.LogWarning("Assign ROS Base transform (rosBase) to make coordinates consistent with ROS.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentTargetIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentTargetIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentTargetIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentTargetIndex = 4;

        if (Input.GetKeyDown(KeyCode.T))
        {
            SendTargetPose();
        }
    }

    void SendTargetPose()
    {
        if (targetPoints == null || targetPoints.Length == 0)
        {
            Debug.LogError("No targets assigned.");
            return;
        }

        Transform t = targetPoints[currentTargetIndex];

        // Convert pose from Unity to ROS frame:
        // 1) Compute pose relative to rosBase (if provided)
        Vector3 posUnity;
        Quaternion rotUnity;
        if (rosBase != null)
        {
            posUnity = rosBase.InverseTransformPoint(t.position);
            rotUnity = Quaternion.Inverse(rosBase.rotation) * t.rotation;
        }
        else
        {
            posUnity = t.position;
            rotUnity = t.rotation;
        }

        // 2) Convert axes: Unity (x,y,z), left-handed,Y-up -> ROS (x,y,z), right-handed,Z-up
        // A proven conversion is: ROS.x = posUnity.z; ROS.y = -posUnity.x; ROS.z = posUnity.y
        // For quaternion: (x,y,z,w) -> (z, -x, y, -w)
        Vector3 rosPos = new Vector3(posUnity.z, -posUnity.x, posUnity.y);
        Quaternion rosQuat = new Quaternion(rotUnity.z, -rotUnity.x, rotUnity.y, -rotUnity.w);

        // Build PoseStamped message
        HeaderMsg header = new HeaderMsg
        {
            frame_id = "world", // must match MoveIt planning frame
            stamp = new TimeMsg()    // default zero; MoveIt doesn't require precise time
        };

        PoseMsg pose = new PoseMsg
        {
            position = new RosMessageTypes.Geometry.PointMsg(rosPos.x, rosPos.y, rosPos.z),
            orientation = new RosMessageTypes.Geometry.QuaternionMsg(rosQuat.x, rosQuat.y, rosQuat.z, rosQuat.w)
        };

        PoseStampedMsg ps = new PoseStampedMsg
        {
            header = header,
            pose = pose
        };

        ros.Publish(topicName, ps);
        Debug.Log($"[Unity] Published PoseStamped for target '{t.name}' (index {currentTargetIndex})");
    }
}
