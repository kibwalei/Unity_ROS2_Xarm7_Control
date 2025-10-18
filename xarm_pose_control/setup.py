from setuptools import setup

package_name = 'xarm_pose_control'

setup(
    name=package_name,
    version='0.0.0',
    packages=[package_name],
    data_files=[
        ('share/ament_index/resource_index/packages',
            ['resource/' + package_name]),
        ('share/' + package_name, ['package.xml']),
    ],
    install_requires=['setuptools'],
    zip_safe=True,
    maintainer='b9',
    maintainer_email='b9@example.com',
    description='Python node for sending target poses to MoveIt',
    license='MIT',
    tests_require=['pytest'],
    entry_points={
        'console_scripts': [
            'pose_to_moveit = xarm_pose_control.pose_to_moveit:main',
            'unity_ik_bridge = xarm_pose_control.unity_ik_bridge:main',
            'ik_service_bridge = xarm_pose_control.ik_service_bridge:main',
        ],
    },
)
