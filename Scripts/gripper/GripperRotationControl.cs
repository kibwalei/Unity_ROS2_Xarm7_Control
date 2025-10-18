using UnityEngine;

public class GripperRotationController : MonoBehaviour
{
    [Header("Finger Collision Scripts")]
    public GripperFingerCollision leftFingerCollider;
    public GripperFingerCollision rightFingerCollider;

    [Header("Assign Finger Transforms (local pivots)")]
    public Transform leftFinger;
    public Transform rightFinger;

    [Header("Rotation Axis (local)")]
    public Vector3 leftAxis = Vector3.forward;
    public Vector3 rightAxis = Vector3.forward;

    [Header("Angles (degrees)")]
    public float leftClosedAngle = -20.5f;
    public float leftOpenAngle = 0f;
    public float rightClosedAngle = 20.5f;
    public float rightOpenAngle = 0f;

    [Header("Runtime / Smoothing")]
    [Range(0f, 1f)] public float gripAmount = 1f; // 0 = closed, 1 = open
    public float speed = 0.5f;

    private GameObject grabbedObject;
    private bool isGrabbing = false;
    private Transform grabParentFinger = null; // Dynamic: left or right finger

    // Internal
    Quaternion leftInitialLocalRot;
    Quaternion rightInitialLocalRot;
    float targetGripAmount = 1f;

    void Start()
    {
        if (leftFinger == null || rightFinger == null)
        {
            Debug.LogError("[GripperRotationController] Assign leftFinger and rightFinger in the Inspector.");
            enabled = false;
            return;
        }

        leftInitialLocalRot = leftFinger.localRotation;
        rightInitialLocalRot = rightFinger.localRotation;
        targetGripAmount = gripAmount;
    }

    void Update()
    {
        // --- Controls ---
        if (Input.GetKeyDown(KeyCode.O)) targetGripAmount = 1f;  // open
        if (Input.GetKeyDown(KeyCode.C)) targetGripAmount = 0f;  // close

        // Smooth open/close animation
        gripAmount = Mathf.MoveTowards(gripAmount, targetGripAmount, Time.deltaTime * speed);

        // Apply rotations
        float leftAngle = Mathf.Lerp(leftClosedAngle, leftOpenAngle, gripAmount);
        float rightAngle = Mathf.Lerp(rightClosedAngle, rightOpenAngle, gripAmount);
        ApplyLeftRotation(leftAngle);
        ApplyRightRotation(rightAngle);

        // Handle grabbing/releasing
        HandleGrabLogic();
    }

    void ApplyLeftRotation(float angleDeg)
    {
        Quaternion delta = Quaternion.AngleAxis(angleDeg, leftAxis.normalized);
        leftFinger.localRotation = leftInitialLocalRot * delta;
    }

    void ApplyRightRotation(float angleDeg)
    {
        Quaternion delta = Quaternion.AngleAxis(angleDeg, rightAxis.normalized);
        rightFinger.localRotation = rightInitialLocalRot * delta;
    }

    void HandleGrabLogic()
    {
        bool bothTouching =
            leftFingerCollider != null && rightFingerCollider != null &&
            leftFingerCollider.currentObject != null &&
            rightFingerCollider.currentObject != null &&
            leftFingerCollider.currentObject == rightFingerCollider.currentObject;

        // --- GRAB ---
        if (!isGrabbing && bothTouching && gripAmount < 0.2f)
        {
            GameObject target = leftFingerCollider.currentObject;

            if (target != null && target.CompareTag("Grabbable"))
            {
                Rigidbody rb = target.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }

                // Decide which finger last touched the object
                if (rightFingerCollider.lastContactTime > leftFingerCollider.lastContactTime)
                    grabParentFinger = rightFinger;
                else
                    grabParentFinger = leftFinger;

                // Parent to that finger
                target.transform.SetParent(grabParentFinger);
                target.transform.localPosition = grabParentFinger.InverseTransformPoint(target.transform.position);
                target.transform.localRotation = Quaternion.Inverse(grabParentFinger.rotation) * target.transform.rotation;

                grabbedObject = target;
                isGrabbing = true;

                Debug.Log($"[Gripper] Grabbed {grabbedObject.name} — parented to {(grabParentFinger == leftFinger ? "LEFT" : "RIGHT")} finger");
            }
        }

        // --- RELEASE ---
        if (isGrabbing && gripAmount > 0.8f)
        {
            if (grabbedObject != null)
            {
                grabbedObject.transform.SetParent(null);

                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.velocity = Vector3.zero;
                }

                Debug.Log($"[Gripper] Released {grabbedObject.name}");
                grabbedObject = null;
            }

            isGrabbing = false;
            grabParentFinger = null;
        }
    }

    // Optional external control
    public void SetGripAmount(float amount)
    {
        targetGripAmount = Mathf.Clamp01(amount);
    }
}
