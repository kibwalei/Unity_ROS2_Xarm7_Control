using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Trajectory;
using System.Collections;

public class XArmTrajectoryPublisher : MonoBehaviour
{

    ROSConnection ros;
    public string topicName = "/xarm7_traj_controller/joint_trajectory";

    void Start()
    {
        ros = ROSConnection.instance;
        StartCoroutine(SendTrajectorySequence());
    }

    IEnumerator SendTrajectorySequence()
    {
        // --- First trajectory target ---
        JointTrajectoryMsg trajectory1 = new JointTrajectoryMsg();
        trajectory1.joint_names = new string[]
        {
            "joint1", "joint2", "joint3", "joint4", "joint5", "joint6", "joint7"
        };

        double[] target1 = {0.0, 1.74, 0, 1.95, 0.0, 0.21, -0.05};

        JointTrajectoryPointMsg point1 = new JointTrajectoryPointMsg();
        point1.positions = target1;
        point1.time_from_start.sec = 2; // 2 seconds to reach pose

        trajectory1.points = new JointTrajectoryPointMsg[] { point1 };

        ros.Publish(topicName, trajectory1);
        Debug.Log("Trajectory 1 sent");

        // --- Wait for 2 seconds before next target ---
        yield return new WaitForSeconds(2f);

        // --- Second trajectory target ---
        JointTrajectoryMsg trajectory2 = new JointTrajectoryMsg();
        trajectory2.joint_names = trajectory1.joint_names;

        double[] target2 = {0.5, 1.2, -0.3, 1.5, 0.2, -0.5, 0.1};

        JointTrajectoryPointMsg point2 = new JointTrajectoryPointMsg();
        point2.positions = target2;
        point2.time_from_start.sec = 2;

        trajectory2.points = new JointTrajectoryPointMsg[] { point2 };

        ros.Publish(topicName, trajectory2);
        Debug.Log("Trajectory 2 sent");
    }
}
