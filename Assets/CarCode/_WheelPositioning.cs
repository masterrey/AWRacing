using UnityEngine;

public class _WheelPositioning : MonoBehaviour
{
    // This script is used to position the wheels in the correct place
    // This script is attached to each wheel

    [SerializeField]
    private Transform wheelTransform;

    private WheelCollider wheelCollider;

    private void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }

    private void Update()
    {
        UpdateWheelPosition();
    }

    private void UpdateWheelPosition()
    {
        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }
}
