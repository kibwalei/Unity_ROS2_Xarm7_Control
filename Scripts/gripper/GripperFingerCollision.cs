using UnityEngine;

public class GripperFingerCollision : MonoBehaviour
{
    [Header("Collision State")]
    public GameObject currentObject;
    public bool isColliding = false;

    [Header("Contact Timing (used by GripperRotationController)")]
    public float lastContactTime = 0f;  // Time when this finger last made contact

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable"))
        {
            currentObject = other.gameObject;
            isColliding = true;
            lastContactTime = Time.time;  // Record time of contact
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Keep updating contact time to reflect continuous touch
        if (other.CompareTag("Grabbable") && currentObject == other.gameObject)
        {
            isColliding = true;
            lastContactTime = Time.time;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentObject)
        {
            currentObject = null;
            isColliding = false;
        }
    }
}
