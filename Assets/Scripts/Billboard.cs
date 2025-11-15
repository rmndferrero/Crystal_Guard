using UnityEngine;

public class WorldSpaceUIFollow : MonoBehaviour
{
    public Transform targetToFollow;
    public Transform cameraToLookAt;
    public Vector3 offset = new Vector3(0, 2.5f, 0);

    void LateUpdate()
    {
        if (targetToFollow == null || cameraToLookAt == null)
        {
            return;
        }

        transform.position = targetToFollow.position + offset;

        // --- THIS IS THE FIX ---
        // We calculate the direction to the camera...
        Vector3 directionToCamera = cameraToLookAt.position - transform.position;
        directionToCamera.y = 0;

        if (directionToCamera == Vector3.zero)
        {
            directionToCamera = cameraToLookAt.forward;
            directionToCamera.y = 0;
        }

        // ...and then we tell the canvas to look in the OPPOSITE direction.
        // This points the "front" of the canvas (where your UI is) at the camera.
        Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
        // --- END OF FIX ---

        transform.rotation = targetRotation;
    }
}