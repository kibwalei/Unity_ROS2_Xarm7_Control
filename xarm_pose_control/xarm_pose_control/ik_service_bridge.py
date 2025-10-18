#!/usr/bin/env python3
import rclpy
from rclpy.node import Node
from geometry_msgs.msg import PoseStamped
from moveit_msgs.srv import GetPositionIK
from trajectory_msgs.msg import JointTrajectory, JointTrajectoryPoint

class IKBridge(Node):
    def __init__(self):
        super().__init__('ik_bridge')
        self.pose_sub = self.create_subscription(
            PoseStamped, '/unity_target_pose', self.pose_callback, 10)

        self.ik_client = self.create_client(GetPositionIK, '/compute_ik')
        self.traj_pub = self.create_publisher(
            JointTrajectory, '/xarm7_traj_controller/joint_trajectory', 10)

        self.get_logger().info("IK + Trajectory Bridge initialized — waiting for poses...")

    def pose_callback(self, pose_msg):
        if not self.ik_client.service_is_ready():
            self.get_logger().warn("Waiting for /compute_ik service...")
            self.ik_client.wait_for_service()

        req = GetPositionIK.Request()
        req.ik_request.group_name = 'xarm7'
        req.ik_request.pose_stamped = pose_msg

        self.get_logger().info(
            f"Requesting IK for pose: x={pose_msg.pose.position.x:.3f}, "
            f"y={pose_msg.pose.position.y:.3f}, z={pose_msg.pose.position.z:.3f}"
        )

        future = self.ik_client.call_async(req)
        future.add_done_callback(self.ik_response_callback)

    def ik_response_callback(self, future):
        try:
            response = future.result()
            if response.error_code.val == 1:
                joint_state = response.solution.joint_state
                self.get_logger().info(f"IK Solved ✅ | joints: {joint_state.position}")

                # --- Build and publish trajectory ---
                traj = JointTrajectory()
                traj.joint_names = joint_state.name

                point = JointTrajectoryPoint()
                point.positions = joint_state.position
                point.time_from_start.sec = 5  # move in ? seconds

                traj.points.append(point)

                self.traj_pub.publish(traj)
                self.get_logger().info("🚀 Published JointTrajectory to /xarm7_traj_controller/joint_trajectory")
            else:
                self.get_logger().error(f"IK Failed ❌ with code {response.error_code.val}")
        except Exception as e:
            self.get_logger().error(f"Service call failed: {e}")

def main(args=None):
    rclpy.init(args=args)
    node = IKBridge()
    rclpy.spin(node)
    node.destroy_node()
    rclpy.shutdown()

if __name__ == '__main__':
    main()
