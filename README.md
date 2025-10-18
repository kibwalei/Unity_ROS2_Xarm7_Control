# Unity_ROS2_Xarm7_Control
Real-time Unity–ROS 2 bridge for xArm 7, featuring pose publishing from Unity, IK computation and trajectory planning in ROS 2 (MoveIt 2), and joint state feedback for realistic simulation and control.
# SCRIPTS BREAKDOWN:
# 1. Target_Sender
This script runs inside Unity and publishes target poses to ROS 2. It takes a GameObject’s position and rotation in the Unity scene (for example, a target marker or robot end-effector goal), converts those coordinates to the ROS frame convention, and sends them as messages to the /unity_target_pose topic through the ROS TCP Connector. This allows Unity to act as a user interface for defining where the robot should move.

# 2. ik_service_bridge (Python – ROS 2 side)

This ROS 2 node acts as a bridge between Unity and MoveIt’s inverse kinematics service. It subscribes to the /unity_target_pose topic, then calls MoveIt’s /compute_ik service to compute the joint angles needed for the robot (e.g., xArm 7) to reach that target pose. The resulting joint states are then published to /joint_states, which can be visualized in RViz or sent to the robot controller for execution. Essentially, this script performs the "thinking" — converting Unity’s target into actionable robot motion.

# 3. GripperRotationControl (C# – Unity side)

This Unity script handles the gripper’s open and close motion. It listens for keyboard input (like pressing a key to open or close the fingers) and rotates the finger objects accordingly. It also manages collision detection and object parenting, so that when the gripper closes around an object, the object becomes attached until released. This gives realistic interaction in Unity’s simulation environment.

# 4. GripperFingerCollision (C# – Unity side)

This script detects when each finger of the gripper comes into contact with a “grabbable” (unity tag)object. It uses trigger colliders to set flags like isColliding. When both fingers are colliding and the close command is given, the object can be attached to the gripper by parenting to the finger(via the GripperRotationControl script). It ensures accurate and realistic detection of grip contact in the simulation.
