using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float FollowSpeed = 5f; // Increased speed for better control
    public float yOffset = 1f;
    public Transform target;

    // Use LateUpdate for camera logic.
    // It runs after all Update functions have been called, 
    // ensuring the target has already moved for this frame.
    void LateUpdate() 
    {
        // 1. Calculate the desired position
        // The camera should not move on the Z-axis (set to -10f for a standard 2D setup)
        Vector3 desiredPosition = new Vector3(
            target.position.x, 
            target.position.y + yOffset, 
            transform.position.z // Keep the camera's original Z depth (usually -10)
        );

        // 2. Smoothly move the camera to the desired position using Lerp
        // Lerp makes the camera lag behind the target, creating the smooth follow effect.
        transform.position = Vector3.Lerp(
            transform.position, 
            desiredPosition, 
            FollowSpeed * Time.deltaTime // Multiplying by Time.deltaTime makes the speed framerate-independent
        );
    }
}